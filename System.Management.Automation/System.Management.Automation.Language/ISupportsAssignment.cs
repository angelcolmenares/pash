namespace System.Management.Automation.Language
{
    internal interface ISupportsAssignment
    {
        IAssignableValue GetAssignableValue();
    }
}

