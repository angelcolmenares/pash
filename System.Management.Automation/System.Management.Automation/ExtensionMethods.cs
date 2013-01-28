namespace System.Management.Automation
{
    using System;
    using System.Runtime.CompilerServices;

    internal static class ExtensionMethods
    {
        public static void SafeInvoke(this EventHandler eventHandler, object sender, EventArgs eventArgs)
        {
            if (eventHandler != null)
            {
                eventHandler(sender, eventArgs);
            }
        }

        public static void SafeInvoke<T> (this EventHandler<T> eventHandler, object sender, T eventArgs) where T: EventArgs
		{
			if (eventHandler != null) {
				eventHandler (sender, eventArgs);
			}
        }
    }
}

