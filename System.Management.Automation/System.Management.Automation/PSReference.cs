namespace System.Management.Automation
{
    using System;
    using System.Globalization;
    using System.Reflection;

    public class PSReference
    {
        private object _value;

        public PSReference(object value)
        {
            this._value = value;
        }

        internal static PSReference CreateInstance(object value, Type typeOfValue)
        {
            return (PSReference) Activator.CreateInstance(typeof(PSReference<>).MakeGenericType(new Type[] { typeOfValue }), BindingFlags.NonPublic | BindingFlags.Instance, null, new object[] { value }, CultureInfo.InvariantCulture);
        }

        public object Value
        {
            get
            {
                PSVariable variable = this._value as PSVariable;
                if (variable != null)
                {
                    return variable.Value;
                }
                return this._value;
            }
            set
            {
                PSVariable variable = this._value as PSVariable;
                if (variable != null)
                {
                    variable.Value = value;
                }
                else
                {
                    this._value = value;
                }
            }
        }
    }
}

