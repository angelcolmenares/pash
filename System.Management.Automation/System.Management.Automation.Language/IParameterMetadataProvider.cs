namespace System.Management.Automation.Language
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Management.Automation;

    internal interface IParameterMetadataProvider
    {
        RuntimeDefinedParameterDictionary GetParameterMetadata(bool automaticPositions, ref bool usesCmdletBinding);
        PowerShell GetPowerShell(ExecutionContext context, Dictionary<string, object> variables, bool filterNonUsingVariables, bool? createLocalScope, params object[] args);
        IEnumerable<Attribute> GetScriptBlockAttributes();
        string GetWithInputHandlingForInvokeCommand();
        bool UsesCmdletBinding();

        ScriptBlockAst Body { get; }

        ReadOnlyCollection<ParameterAst> Parameters { get; }
    }
}

