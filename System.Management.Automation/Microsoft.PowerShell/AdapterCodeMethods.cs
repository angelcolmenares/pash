namespace Microsoft.PowerShell
{
    using System;
    using System.Globalization;
    using System.Management.Automation;
    using System.Reflection;

    public static class AdapterCodeMethods
    {
        public static string ConvertDNWithBinaryToString(PSObject deInstance, PSObject dnWithBinaryInstance)
        {
            if (dnWithBinaryInstance == null)
            {
                throw PSTraceSource.NewArgumentException("dnWithBinaryInstance");
            }
            object baseObject = dnWithBinaryInstance.BaseObject;
            return (string) baseObject.GetType().InvokeMember("DNString", BindingFlags.GetProperty | BindingFlags.Public, null, baseObject, null, CultureInfo.InvariantCulture);
        }

        public static long ConvertLargeIntegerToInt64(PSObject deInstance, PSObject largeIntegerInstance)
        {
            if (largeIntegerInstance == null)
            {
                throw PSTraceSource.NewArgumentException("largeIntegerInstance");
            }
            object baseObject = largeIntegerInstance.BaseObject;
            Type type = baseObject.GetType();
            int num = (int) type.InvokeMember("HighPart", BindingFlags.GetProperty | BindingFlags.Public, null, baseObject, null, CultureInfo.InvariantCulture);
            int num2 = (int) type.InvokeMember("LowPart", BindingFlags.GetProperty | BindingFlags.Public, null, baseObject, null, CultureInfo.InvariantCulture);
            byte[] array = new byte[8];
            BitConverter.GetBytes(num2).CopyTo(array, 0);
            BitConverter.GetBytes(num).CopyTo(array, 4);
            return BitConverter.ToInt64(array, 0);
        }
    }
}

