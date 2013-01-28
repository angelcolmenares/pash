namespace System.Management.Automation
{
    using System;

    public abstract class PSTypeConverter
    {
        protected PSTypeConverter()
        {
        }

        public virtual bool CanConvertFrom(PSObject sourceValue, Type destinationType)
        {
            return this.CanConvertFrom(GetSourceValueAsObject(sourceValue), destinationType);
        }

        public abstract bool CanConvertFrom(object sourceValue, Type destinationType);
        public virtual bool CanConvertTo(PSObject sourceValue, Type destinationType)
        {
            return this.CanConvertTo(GetSourceValueAsObject(sourceValue), destinationType);
        }

        public abstract bool CanConvertTo(object sourceValue, Type destinationType);
        public virtual object ConvertFrom(PSObject sourceValue, Type destinationType, IFormatProvider formatProvider, bool ignoreCase)
        {
            return this.ConvertFrom(GetSourceValueAsObject(sourceValue), destinationType, formatProvider, ignoreCase);
        }

        public abstract object ConvertFrom(object sourceValue, Type destinationType, IFormatProvider formatProvider, bool ignoreCase);
        public virtual object ConvertTo(PSObject sourceValue, Type destinationType, IFormatProvider formatProvider, bool ignoreCase)
        {
            return this.ConvertTo(GetSourceValueAsObject(sourceValue), destinationType, formatProvider, ignoreCase);
        }

        public abstract object ConvertTo(object sourceValue, Type destinationType, IFormatProvider formatProvider, bool ignoreCase);
        private static object GetSourceValueAsObject(PSObject sourceValue)
        {
            if (sourceValue == null)
            {
                return null;
            }
            if (sourceValue.BaseObject is PSCustomObject)
            {
                return sourceValue;
            }
            return PSObject.Base(sourceValue);
        }
    }
}

