using System;

namespace NamespaceFixer.Core
{
    internal static class Statics
    {
        /// <summary>
        /// Version of the extension.
        /// 
        /// To update in:
        /// - below.
        /// - assemblyinfo.
        /// - source.extension.vsixmanifest
        /// </summary>
        public const string PackageVersion = "2.3";

        public const string CsProjectFileExtension = "csproj";
        public const string VbProjectFileExtension = "vbproj";
    }

    /// <summary>
    /// Must match guid in .vsct.
    /// </summary>
    internal static class Guids
    {
        public static readonly Guid CmdSetNamespaceFixerProject = new Guid("{19492BCB-32B3-4EC3-8826-D67CD5526653}");
        public static readonly Guid CmdSetNamespaceFixerFile = new Guid("{43F670C9-731D-414C-86F1-DF489BC29795}");
        public const string NamespaceFixerPackage = "3C7C5ABE-82AC-4A37-B077-0FF60E8B1FD3";

        // https://docs.microsoft.com/visualstudio/extensibility/how-to-use-rule-based-ui-context-for-visual-studio-extensions
        public const string UiContextSupportedProjects = "C694C8AD-1300-4ADC-93D8-EBFA2915354B";
        public const string UiContextSupportedFiles = "24308ADC-7EF3-4832-A79F-DCFCBDDA9B40";
    }

    internal static class Ids
    {
        public const int CmdIdAdjustNamespace = 0x2001;
    }
}