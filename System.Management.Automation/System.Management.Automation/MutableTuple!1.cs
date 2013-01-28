namespace System.Management.Automation
{
    using System;
    using System.CodeDom.Compiler;

    [GeneratedCode("DLR", "2.0")]
    internal class MutableTuple<T0> : MutableTuple
    {
        private T0 _item0;

        public MutableTuple()
        {
        }

        public MutableTuple(T0 item0)
        {
            this._item0 = item0;
        }

        protected override object GetValueImpl(int index)
        {
            if (index != 0)
            {
                throw new ArgumentOutOfRangeException("index");
            }
            return this.Item000;
        }

        protected override void SetValueImpl(int index, object value)
        {
            if (index != 0)
            {
                throw new ArgumentOutOfRangeException("index");
            }
            this.Item000 = LanguagePrimitives.ConvertTo<T0>(value);
        }

        public override int Capacity
        {
            get
            {
                return 1;
            }
        }

        public T0 Item000
        {
            get
            {
                return this._item0;
            }
            set
            {
                this._item0 = value;
                base._valuesSet[0] = true;
            }
        }
    }
}

