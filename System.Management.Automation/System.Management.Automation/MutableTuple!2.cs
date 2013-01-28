namespace System.Management.Automation
{
    using System;
    using System.CodeDom.Compiler;

    [GeneratedCode("DLR", "2.0")]
    internal class MutableTuple<T0, T1> : MutableTuple<T0>
    {
        private T1 _item1;

        public MutableTuple()
        {
        }

        public MutableTuple(T0 item0, T1 item1) : base(item0)
        {
            this._item1 = item1;
        }

        protected override object GetValueImpl(int index)
        {
            switch (index)
            {
                case 0:
                    return base.Item000;

                case 1:
                    return this.Item001;
            }
            throw new ArgumentOutOfRangeException("index");
        }

        protected override void SetValueImpl(int index, object value)
        {
            switch (index)
            {
                case 0:
                    base.Item000 = LanguagePrimitives.ConvertTo<T0>(value);
                    return;

                case 1:
                    this.Item001 = LanguagePrimitives.ConvertTo<T1>(value);
                    return;
            }
            throw new ArgumentOutOfRangeException("index");
        }

        public override int Capacity
        {
            get
            {
                return 2;
            }
        }

        public T1 Item001
        {
            get
            {
                return this._item1;
            }
            set
            {
                this._item1 = value;
                base._valuesSet[1] = true;
            }
        }
    }
}

