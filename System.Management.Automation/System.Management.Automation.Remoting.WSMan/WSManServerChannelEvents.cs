namespace System.Management.Automation.Remoting.WSMan
{
    using System;
    using System.Threading;

    public static class WSManServerChannelEvents
    {
        public static  event EventHandler ShuttingDown;

        internal static void RaiseShuttingDownEvent()
        {
            EventHandler shuttingDown = ShuttingDown;
            if (shuttingDown != null)
            {
                shuttingDown(null, EventArgs.Empty);
            }
        }
    }
}

