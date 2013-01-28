namespace System.Management.Automation
{
    using System;
    using System.CodeDom.Compiler;

    [GeneratedCode("DLR", "2.0")]
    internal class MutableTuple<T0, T1, T2, T3, T4, T5, T6, T7> : MutableTuple<T0, T1, T2, T3>
    {
        private T4 _item4;
        private T5 _item5;
        private T6 _item6;
        private T7 _item7;

        public MutableTuple()
        {
        }

        public MutableTuple(T0 item0, T1 item1, T2 item2, T3 item3, T4 item4, T5 item5, T6 item6, T7 item7) : base(item0, item1, item2, item3)
        {
            this._item4 = item4;
            this._item5 = item5;
            this._item6 = item6;
            this._item7 = item7;
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
                    return base.Item002;

                case 3:
                    return base.Item003;

                case 4:
                    return this.Item004;

                case 5:
                    return this.Item005;

                case 6:
                    return this.Item006;

                case 7:
                    return this.Item007;
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
                    base.Item002 = LanguagePrimitives.ConvertTo<T2>(value);
                    return;

                case 3:
                    base.Item003 = LanguagePrimitives.ConvertTo<T3>(value);
                    return;

                case 4:
                    this.Item004 = LanguagePrimitives.ConvertTo<T4>(value);
                    return;

                case 5:
                    this.Item005 = LanguagePrimitives.ConvertTo<T5>(value);
                    return;

                case 6:
                    this.Item006 = LanguagePrimitives.ConvertTo<T6>(value);
                    return;

                case 7:
                    this.Item007 = LanguagePrimitives.ConvertTo<T7>(value);
                    return;
            }
            throw new ArgumentOutOfRangeException("index");
        }

        public override int Capacity
        {
            get
            {
                return 8;
            }
        }

        public T4 Item004
        {
            get
            {
                return this._item4;
            }
            set
            {
                this._item4 = value;
                base._valuesSet[4] = true;
            }
        }

        public T5 Item005
        {
            get
            {
                return this._item5;
            }
            set
            {
                this._item5 = value;
                base._valuesSet[5] = true;
            }
        }

        public T6 Item006
        {
            get
            {
                return this._item6;
            }
            set
            {
                this._item6 = value;
                base._valuesSet[6] = true;
            }
        }

        public T7 Item007
        {
            get
            {
                return this._item7;
            }
            set
            {
                this._item7 = value;
                base._valuesSet[7] = true;
            }
        }
    }
}

