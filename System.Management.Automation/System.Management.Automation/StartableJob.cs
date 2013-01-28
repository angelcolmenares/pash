namespace System.Management.Automation
{
    using System;

    internal abstract class StartableJob : Job
    {
        internal StartableJob(string commandName, string jobName) : base(commandName, jobName)
        {
        }

        internal abstract void StartJob();
    }
}

