namespace System.Management.Automation
{
    using System;
    using System.Management.Automation.Internal;

    internal sealed class NativeCommand : InternalCommand
    {
        private NativeCommandProcessor myCommandProcessor;

        internal override void DoStopProcessing()
        {
            try
            {
                if (this.myCommandProcessor != null)
                {
                    this.myCommandProcessor.StopProcessing();
                }
            }
            catch (Exception exception)
            {
                CommandProcessorBase.CheckForSevereException(exception);
            }
        }

        internal NativeCommandProcessor MyCommandProcessor
        {
            get
            {
                return this.myCommandProcessor;
            }
            set
            {
                this.myCommandProcessor = value;
            }
        }
    }
}

