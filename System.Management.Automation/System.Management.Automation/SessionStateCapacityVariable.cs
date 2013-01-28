namespace System.Management.Automation
{
    using System;
    using System.Globalization;

    internal class SessionStateCapacityVariable : PSVariable
    {
        private int _fastValue;
        private int maxCapacity;
        private int minCapacity;
        private SessionStateCapacityVariable sharedCapacityVariable;

        public SessionStateCapacityVariable(string name, SessionStateCapacityVariable sharedCapacityVariable, ScopedItemOptions options) : base(name, sharedCapacityVariable.Value, options)
        {
            this.maxCapacity = 0x7fffffff;
            ValidateRangeAttribute item = new ValidateRangeAttribute(0, 0x7fffffff);
            base.Attributes.Add(item);
            this.sharedCapacityVariable = sharedCapacityVariable;
            this.Description = sharedCapacityVariable.Description;
            this._fastValue = (int) sharedCapacityVariable.Value;
        }

        internal SessionStateCapacityVariable(string name, int defaultCapacity, int maxCapacity, int minCapacity, ScopedItemOptions options) : base(name, defaultCapacity, options)
        {
            this.maxCapacity = 0x7fffffff;
            ValidateRangeAttribute item = new ValidateRangeAttribute(minCapacity, maxCapacity);
            this.minCapacity = minCapacity;
            this.maxCapacity = maxCapacity;
            base.Attributes.Add(item);
            this._fastValue = defaultCapacity;
        }

        public override bool IsValidValue(object value)
        {
            int num = (int) value;
            return (((num >= this.minCapacity) && (num <= this.maxCapacity)) || base.IsValidValue(value));
        }

        internal int FastValue
        {
            get
            {
                return this._fastValue;
            }
        }

        public override object Value
        {
            get
            {
                if (this.sharedCapacityVariable != null)
                {
                    return this.sharedCapacityVariable.Value;
                }
                return base.Value;
            }
            set
            {
                this.sharedCapacityVariable = null;
                base.Value = LanguagePrimitives.ConvertTo(value, typeof(int), CultureInfo.InvariantCulture);
                this._fastValue = (int) base.Value;
            }
        }
    }
}

