namespace Microsoft.PowerShell.Commands.Internal.Format
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Management.Automation;

    internal static class DefaultScalarTypes
    {
        private static readonly HashSet<string> defaultScalarTypesHash = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        static DefaultScalarTypes()
        {
            defaultScalarTypesHash.Add("System.String");
            defaultScalarTypesHash.Add("System.SByte");
            defaultScalarTypesHash.Add("System.Byte");
            defaultScalarTypesHash.Add("System.Int16");
            defaultScalarTypesHash.Add("System.UInt16");
            defaultScalarTypesHash.Add("System.Int32");
            defaultScalarTypesHash.Add("System.UInt32");
            defaultScalarTypesHash.Add("System.Int64");
            defaultScalarTypesHash.Add("System.UInt64");
            defaultScalarTypesHash.Add("System.Char");
            defaultScalarTypesHash.Add("System.Single");
            defaultScalarTypesHash.Add("System.Double");
            defaultScalarTypesHash.Add("System.Boolean");
            defaultScalarTypesHash.Add("System.Decimal");
            defaultScalarTypesHash.Add("System.IntPtr");
            defaultScalarTypesHash.Add("System.Security.SecureString");
            defaultScalarTypesHash.Add("System.Numerics.BigInteger");
        }

        internal static bool IsTypeInList(Collection<string> typeNames)
        {
            string str = PSObjectHelper.PSObjectIsOfExactType(typeNames);
            if (string.IsNullOrEmpty(str))
            {
                return false;
            }
            string str2 = Deserializer.MaskDeserializationPrefix(str);
            if (string.IsNullOrEmpty(str2))
            {
                return false;
            }
            return (PSObjectHelper.PSObjectIsEnum(typeNames) || defaultScalarTypesHash.Contains(str2));
        }
    }
}

