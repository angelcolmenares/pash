namespace System.Management.Automation
{
    using System;

    internal class WinRTHelper
    {
        internal static bool IsWinRTType(Type type)
        {
            return type.Attributes.ToString().Contains("WindowsRuntime");
        }
    }
}

