namespace System.Management.Automation
{
    using System;
    using System.CodeDom.Compiler;

    [GeneratedCode("DLR", "2.0")]
    internal class MutableTuple<T0, T1, T2, T3> : MutableTuple<T0, T1>
    {
        private T2 _item2;
        private T3 _item3;

        public MutableTuple()
        {
        }

        public MutableTuple(T0 item0, T1 item1, T2 item2, T3 item3) : base(item0, item1)
        {
            this._item2 = item2;
            this._item3 = item3;
        }

        protected override object GetValueImpl(int index)
        {
            switch (index)
            {
                case 0:
                    return base.Item000;

                case 1:
                    return base.Item001;

                case 2:
                    return this.Item002;

                case 3:
                    return this.Item003;
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
                    base.Item001 = LanguagePrimitives.ConvertTo<T1>(value);
                    return;

                case 2:
                    this.Item002 = LanguagePrimitives.ConvertTo<T2>(value);
                    return;

                case 3:
                    this.Item003 = LanguagePrimitives.ConvertTo<T3>(value);
                    return;
            }
            throw new ArgumentOutOfRangeException("index");
        }

        public override int Capacity
        {
            get
            {
                return 4;
            }
        }

        public T2 Item002
        {
            get
            {
                return this._item2;
            }
            set
            {
                this._item2 = value;
                base._valuesSet[2] = true;
            }
        }

        public T3 Item003
        {
            get
            {
                return this._item3;
            }
            set
            {
                this._item3 = value;
                base._valuesSet[3] = true;
            }
        }
    }
}

