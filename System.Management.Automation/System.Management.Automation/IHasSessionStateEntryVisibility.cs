namespace System.Management.Automation
{
    using System;

    internal interface IHasSessionStateEntryVisibility
    {
        SessionStateEntryVisibility Visibility { get; set; }
    }
}

