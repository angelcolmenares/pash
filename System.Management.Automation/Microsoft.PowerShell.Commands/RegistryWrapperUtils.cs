namespace Microsoft.PowerShell.Commands
{
    using Microsoft.Win32;
    using System;
    using System.Globalization;
    using System.IO;

    internal static class RegistryWrapperUtils
    {
        public static object ConvertUIntToValueForRegistryIfNeeded(object value, RegistryValueKind kind)
        {
            if (kind == RegistryValueKind.DWord)
            {
                try
                {
                    value = BitConverter.ToInt32(BitConverter.GetBytes(Convert.ToUInt32(value, CultureInfo.InvariantCulture)), 0);
                }
                catch (OverflowException)
                {
                }
                return value;
            }
            if (kind == RegistryValueKind.QWord)
            {
                try
                {
                    value = BitConverter.ToInt64(BitConverter.GetBytes(Convert.ToUInt64(value, CultureInfo.InvariantCulture)), 0);
                }
                catch (OverflowException)
                {
                }
            }
            return value;
        }

        public static object ConvertValueToUIntFromRegistryIfNeeded(string name, object value, RegistryValueKind kind)
        {
            try
            {
                if (kind == RegistryValueKind.DWord)
                {
                    value = (int) value;
                    if (((int) value) < 0)
                    {
                        value = BitConverter.ToUInt32(BitConverter.GetBytes((int) value), 0);
                    }
                    return value;
                }
                if (kind == RegistryValueKind.QWord)
                {
                    value = (long) value;
                    if (((long) value) < 0L)
                    {
                        value = BitConverter.ToUInt64(BitConverter.GetBytes((long) value), 0);
                    }
                }
            }
            catch (IOException)
            {
            }
            return value;
        }
    }
}

