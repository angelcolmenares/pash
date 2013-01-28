namespace System.Management.Automation
{
    using System;
    using System.CodeDom.Compiler;

    [GeneratedCode("DLR", "2.0")]
    internal class MutableTuple<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> : MutableTuple<T0, T1, T2, T3, T4, T5, T6, T7>
    {
        private T10 _item10;
        private T11 _item11;
        private T12 _item12;
        private T13 _item13;
        private T14 _item14;
        private T15 _item15;
        private T8 _item8;
        private T9 _item9;

        public MutableTuple()
        {
        }

        public MutableTuple(T0 item0, T1 item1, T2 item2, T3 item3, T4 item4, T5 item5, T6 item6, T7 item7, T8 item8, T9 item9, T10 item10, T11 item11, T12 item12, T13 item13, T14 item14, T15 item15) : base(item0, item1, item2, item3, item4, item5, item6, item7)
        {
            this._item8 = item8;
            this._item9 = item9;
            this._item10 = item10;
            this._item11 = item11;
            this._item12 = item12;
            this._item13 = item13;
            this._item14 = item14;
            this._item15 = item15;
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
                    return base.Item004;

                case 5:
                    return base.Item005;

                case 6:
                    return base.Item006;

                case 7:
                    return base.Item007;

                case 8:
                    return this.Item008;

                case 9:
                    return this.Item009;

                case 10:
                    return this.Item010;

                case 11:
                    return this.Item011;

                case 12:
                    return this.Item012;

                case 13:
                    return this.Item013;

                case 14:
                    return this.Item014;

                case 15:
                    return this.Item015;
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
                    base.Item004 = LanguagePrimitives.ConvertTo<T4>(value);
                    return;

                case 5:
                    base.Item005 = LanguagePrimitives.ConvertTo<T5>(value);
                    return;

                case 6:
                    base.Item006 = LanguagePrimitives.ConvertTo<T6>(value);
                    return;

                case 7:
                    base.Item007 = LanguagePrimitives.ConvertTo<T7>(value);
                    return;

                case 8:
                    this.Item008 = LanguagePrimitives.ConvertTo<T8>(value);
                    return;

                case 9:
                    this.Item009 = LanguagePrimitives.ConvertTo<T9>(value);
                    return;

                case 10:
                    this.Item010 = LanguagePrimitives.ConvertTo<T10>(value);
                    return;

                case 11:
                    this.Item011 = LanguagePrimitives.ConvertTo<T11>(value);
                    return;

                case 12:
                    this.Item012 = LanguagePrimitives.ConvertTo<T12>(value);
                    return;

                case 13:
                    this.Item013 = LanguagePrimitives.ConvertTo<T13>(value);
                    return;

                case 14:
                    this.Item014 = LanguagePrimitives.ConvertTo<T14>(value);
                    return;

                case 15:
                    this.Item015 = LanguagePrimitives.ConvertTo<T15>(value);
                    return;
            }
            throw new ArgumentOutOfRangeException("index");
        }

        public override int Capacity
        {
            get
            {
                return 0x10;
            }
        }

        public T8 Item008
        {
            get
            {
                return this._item8;
            }
            set
            {
                this._item8 = value;
                base._valuesSet[8] = true;
            }
        }

        public T9 Item009
        {
            get
            {
                return this._item9;
            }
            set
            {
                this._item9 = value;
                base._valuesSet[9] = true;
            }
        }

        public T10 Item010
        {
            get
            {
                return this._item10;
            }
            set
            {
                this._item10 = value;
                base._valuesSet[10] = true;
            }
        }

        public T11 Item011
        {
            get
            {
                return this._item11;
            }
            set
            {
                this._item11 = value;
                base._valuesSet[11] = true;
            }
        }

        public T12 Item012
        {
            get
            {
                return this._item12;
            }
            set
            {
                this._item12 = value;
                base._valuesSet[12] = true;
            }
        }

        public T13 Item013
        {
            get
            {
                return this._item13;
            }
            set
            {
                this._item13 = value;
                base._valuesSet[13] = true;
            }
        }

        public T14 Item014
        {
            get
            {
                return this._item14;
            }
            set
            {
                this._item14 = value;
                base._valuesSet[14] = true;
            }
        }

        public T15 Item015
        {
            get
            {
                return this._item15;
            }
            set
            {
                this._item15 = value;
                base._valuesSet[15] = true;
            }
        }
    }
}

