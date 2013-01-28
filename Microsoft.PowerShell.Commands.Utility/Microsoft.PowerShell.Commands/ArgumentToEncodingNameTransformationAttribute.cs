namespace Microsoft.PowerShell.Commands
{
    using System;
    using System.Management.Automation;

    internal sealed class ArgumentToEncodingNameTransformationAttribute : ArgumentTransformationAttribute
    {
        public override object Transform(EngineIntrinsics engineIntrinsics, object inputData)
        {
            string str;
            if (!LanguagePrimitives.TryConvertTo<string>(inputData, out str) || (((!string.Equals(str, "unknown", StringComparison.OrdinalIgnoreCase) && !string.Equals(str, "string", StringComparison.OrdinalIgnoreCase)) && (!string.Equals(str, "unicode", StringComparison.OrdinalIgnoreCase) && !string.Equals(str, "bigendianunicode", StringComparison.OrdinalIgnoreCase))) && (((!string.Equals(str, "utf8", StringComparison.OrdinalIgnoreCase) && !string.Equals(str, "utf7", StringComparison.OrdinalIgnoreCase)) && (!string.Equals(str, "utf32", StringComparison.OrdinalIgnoreCase) && !string.Equals(str, "ascii", StringComparison.OrdinalIgnoreCase))) && (!string.Equals(str, "default", StringComparison.OrdinalIgnoreCase) && !string.Equals(str, "oem", StringComparison.OrdinalIgnoreCase)))))
            {
                return inputData;
            }
            return EncodingConversion.Convert(null, str);
        }
    }
}

