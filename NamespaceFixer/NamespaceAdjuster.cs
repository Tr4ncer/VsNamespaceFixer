﻿using Microsoft.VisualStudio.Shell;
using NamespaceFixer.Core;
using NamespaceFixer.NamespaceBuilder;
using System;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;

namespace NamespaceFixer
{
    internal sealed class NamespaceAdjuster
    {
        private INamespaceBuilder _namespaceBuilder;

        private readonly NamespaceAdjusterPackage _package;
        private readonly VsServiceInfo _serviceInfo;

        private NamespaceAdjuster(NamespaceAdjusterPackage package)
        {
            _package = package;
            _serviceInfo = new VsServiceInfo(this);
        }

        public static NamespaceAdjuster Instance { get; private set; }

        internal IServiceProvider ServiceProvider => (Package)_package;

        public static void Initialize(NamespaceAdjusterPackage package)
        {
            Instance = new NamespaceAdjuster(package);
            Instance.Initialize();
        }

        public void Initialize()
        {
            var commandService = ServiceProvider.GetService(typeof(IMenuCommandService)) as OleMenuCommandService;

            if (commandService != null)
            {
                var menuCommandSolutionId = new CommandID(Guids.CmdSetNamespaceFixerSolution, Ids.CmdIdAdjustNamespace);
                var menuItemSolution = new MenuCommand(MenuItemCallback, menuCommandSolutionId);
                commandService.AddCommand(menuItemSolution);

                var menuCommandProjectId = new CommandID(Guids.CmdSetNamespaceFixerProject, Ids.CmdIdAdjustNamespace);
                var menuItemProject = new MenuCommand(MenuItemCallback, menuCommandProjectId);
                commandService.AddCommand(menuItemProject);

                var menuCommandFileId = new CommandID(Guids.CmdSetNamespaceFixerFile, Ids.CmdIdAdjustNamespace);
                var menuItemFile = new MenuCommand(MenuItemCallback, menuCommandFileId);
                commandService.AddCommand(menuItemFile);
            }
        }

        /// <summary>
        /// Click on the button 'Adjust namespaces'.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuItemCallback(object sender, EventArgs e)
        {
            // TODO SENDER => Solution, Project or File ?
            ThreadHelper.ThrowIfNotOnUIThread();

            try
            {
                var selectedItemPaths = _serviceInfo.SolutionSelectionService.GetSelectedItemsPaths();

                var allPaths = _serviceInfo.InnerPathFinder.GetAllInnerPaths(selectedItemPaths);

                if (!allPaths.Any())
                {
                    return;
                }

                var projectFile = ProjectHelper.GetProjectFilePath(allPaths[0]);
                var solutionFile = ProjectHelper.GetSolutionFilePath(_serviceInfo, projectFile.Directory.FullName);

                _namespaceBuilder = NamespaceBuilderFactory.CreateNamespaceBuilderService(projectFile.Extension, _package.GetOptionPage());

                allPaths.ToList().ForEach(f => FixNamespace(f, solutionFile, projectFile));
            }
            finally
            {
                MsBuildEvaluationHelper.ClearCache();
            }
        }

        private void FixNamespace(string filePath, FileInfo solutionFile, FileInfo projectFile)
        {
            if (!File.Exists(filePath) || IgnoreFile(filePath))
            {
                return;
            }

            var encoding = NamespaceFixer.Extensions.PathExtensions.GetEncoding(filePath);

            var fileContent = File.ReadAllText(filePath, encoding);

            var desiredNamespace = _namespaceBuilder.GetNamespace(filePath, solutionFile, projectFile);

            var updated = _namespaceBuilder.UpdateFile(ref fileContent, desiredNamespace);

            if (updated)
            {
                File.WriteAllText(filePath, fileContent, encoding);
            }
        }

        private bool IgnoreFile(string path)
        {
            var extensionWithoutDot = Path.GetExtension(path).Substring(1);

            return ExtensionsToIgnore.Contains(extensionWithoutDot);
        }

        private string[] ExtensionsToIgnore => _package.GetOptionPage()
                    .FileExtensionsToIgnore
                    .Split(';')
                    .Select(ignoredExtension => ignoredExtension.Replace(".", string.Empty).Trim())
                    .Where(ext => !string.IsNullOrEmpty(ext))
                    .ToArray();
    }
}