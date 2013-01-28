namespace System.Management.Automation
{
    using System;

    internal class ServerSteppablePipelineDriverEventArg : EventArgs
    {
        internal ServerSteppablePipelineDriver SteppableDriver;

        internal ServerSteppablePipelineDriverEventArg(ServerSteppablePipelineDriver driver)
        {
            this.SteppableDriver = driver;
        }
    }
}

