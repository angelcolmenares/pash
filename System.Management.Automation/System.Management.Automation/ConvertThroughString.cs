namespace System.Management.Automation
{
    using System;

    public class ConvertThroughString : PSTypeConverter
    {
        public override bool CanConvertFrom(object sourceValue, Type destinationType)
        {
            if (sourceValue is string)
            {
                return false;
            }
            return true;
        }

        public override bool CanConvertTo(object sourceValue, Type destinationType)
        {
            return false;
        }

        public override object ConvertFrom(object sourceValue, Type destinationType, IFormatProvider formatProvider, bool ignoreCase)
        {
            string valueToConvert = (string) LanguagePrimitives.ConvertTo(sourceValue, typeof(string), formatProvider);
            return LanguagePrimitives.ConvertTo(valueToConvert, destinationType, formatProvider);
        }

        public override object ConvertTo(object sourceValue, Type destinationType, IFormatProvider formatProvider, bool ignoreCase)
        {
            throw PSTraceSource.NewNotSupportedException();
        }
    }
}

