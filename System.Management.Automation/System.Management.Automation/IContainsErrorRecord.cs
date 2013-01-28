namespace System.Management.Automation
{
    public interface IContainsErrorRecord
    {
        System.Management.Automation.ErrorRecord ErrorRecord { get; }
    }
}

