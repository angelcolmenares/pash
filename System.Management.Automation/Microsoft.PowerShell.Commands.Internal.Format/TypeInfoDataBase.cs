namespace Microsoft.PowerShell.Commands.Internal.Format
{
    using System;

    internal sealed class TypeInfoDataBase
    {
        internal DefaultSettingsSection defaultSettingsSection = new DefaultSettingsSection();
        internal DisplayResourceManagerCache displayResourceManagerCache = new DisplayResourceManagerCache();
        internal FormatControlDefinitionHolder formatControlDefinitionHolder = new FormatControlDefinitionHolder();
        internal TypeGroupsSection typeGroupSection = new TypeGroupsSection();
        internal ViewDefinitionsSection viewDefinitionsSection = new ViewDefinitionsSection();
    }
}

