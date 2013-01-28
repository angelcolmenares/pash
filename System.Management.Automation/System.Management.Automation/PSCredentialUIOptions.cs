namespace System.Management.Automation
{
    using System;

    [Flags]
    public enum PSCredentialUIOptions
    {
        AlwaysPrompt = 2,
        Default = 1,
        None = 0,
        ReadOnlyUserName = 3,
        ValidateUserNameSyntax = 1
    }
}

