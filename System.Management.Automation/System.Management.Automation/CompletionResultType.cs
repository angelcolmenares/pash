namespace System.Management.Automation
{
    using System;

    public enum CompletionResultType
    {
        Text,
        History,
        Command,
        ProviderItem,
        ProviderContainer,
        Property,
        Method,
        ParameterName,
        ParameterValue,
        Variable,
        Namespace,
        Type
    }
}

