namespace System.Management.Automation
{
    using System;
    using System.CodeDom.Compiler;

    [GeneratedCode("DLR", "2.0")]
    internal class MutableTuple<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24, T25, T26, T27, T28, T29, T30, T31> : MutableTuple<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>
    {
        private T16 _item16;
        private T17 _item17;
        private T18 _item18;
        private T19 _item19;
        private T20 _item20;
        private T21 _item21;
        private T22 _item22;
        private T23 _item23;
        private T24 _item24;
        private T25 _item25;
        private T26 _item26;
        private T27 _item27;
        private T28 _item28;
        private T29 _item29;
        private T30 _item30;
        private T31 _item31;

        public MutableTuple()
        {
        }

        public MutableTuple(T0 item0, T1 item1, T2 item2, T3 item3, T4 item4, T5 item5, T6 item6, T7 item7, T8 item8, T9 item9, T10 item10, T11 item11, T12 item12, T13 item13, T14 item14, T15 item15, T16 item16, T17 item17, T18 item18, T19 item19, T20 item20, T21 item21, T22 item22, T23 item23, T24 item24, T25 item25, T26 item26, T27 item27, T28 item28, T29 item29, T30 item30, T31 item31) : base(item0, item1, item2, item3, item4, item5, item6, item7, item8, item9, item10, item11, item12, item13, item14, item15)
        {
            this._item16 = item16;
            this._item17 = item17;
            this._item18 = item18;
            this._item19 = item19;
            this._item20 = item20;
            this._item21 = item21;
            this._item22 = item22;
            this._item23 = item23;
            this._item24 = item24;
            this._item25 = item25;
            this._item26 = item26;
            this._item27 = item27;
            this._item28 = item28;
            this._item29 = item29;
            this._item30 = item30;
            this._item31 = item31;
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
                    return base.Item008;

                case 9:
                    return base.Item009;

                case 10:
                    return base.Item010;

                case 11:
                    return base.Item011;

                case 12:
                    return base.Item012;

                case 13:
                    return base.Item013;

                case 14:
                    return base.Item014;

                case 15:
                    return base.Item015;

                case 0x10:
                    return this.Item016;

                case 0x11:
                    return this.Item017;

                case 0x12:
                    return this.Item018;

                case 0x13:
                    return this.Item019;

                case 20:
                    return this.Item020;

                case 0x15:
                    return this.Item021;

                case 0x16:
                    return this.Item022;

                case 0x17:
                    return this.Item023;

                case 0x18:
                    return this.Item024;

                case 0x19:
                    return this.Item025;

                case 0x1a:
                    return this.Item026;

                case 0x1b:
                    return this.Item027;

                case 0x1c:
                    return this.Item028;

                case 0x1d:
                    return this.Item029;

                case 30:
                    return this.Item030;

                case 0x1f:
                    return this.Item031;
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
                    base.Item008 = LanguagePrimitives.ConvertTo<T8>(value);
                    return;

                case 9:
                    base.Item009 = LanguagePrimitives.ConvertTo<T9>(value);
                    return;

                case 10:
                    base.Item010 = LanguagePrimitives.ConvertTo<T10>(value);
                    return;

                case 11:
                    base.Item011 = LanguagePrimitives.ConvertTo<T11>(value);
                    return;

                case 12:
                    base.Item012 = LanguagePrimitives.ConvertTo<T12>(value);
                    return;

                case 13:
                    base.Item013 = LanguagePrimitives.ConvertTo<T13>(value);
                    return;

                case 14:
                    base.Item014 = LanguagePrimitives.ConvertTo<T14>(value);
                    return;

                case 15:
                    base.Item015 = LanguagePrimitives.ConvertTo<T15>(value);
                    return;

                case 0x10:
                    this.Item016 = LanguagePrimitives.ConvertTo<T16>(value);
                    return;

                case 0x11:
                    this.Item017 = LanguagePrimitives.ConvertTo<T17>(value);
                    return;

                case 0x12:
                    this.Item018 = LanguagePrimitives.ConvertTo<T18>(value);
                    return;

                case 0x13:
                    this.Item019 = LanguagePrimitives.ConvertTo<T19>(value);
                    return;

                case 20:
                    this.Item020 = LanguagePrimitives.ConvertTo<T20>(value);
                    return;

                case 0x15:
                    this.Item021 = LanguagePrimitives.ConvertTo<T21>(value);
                    return;

                case 0x16:
                    this.Item022 = LanguagePrimitives.ConvertTo<T22>(value);
                    return;

                case 0x17:
                    this.Item023 = LanguagePrimitives.ConvertTo<T23>(value);
                    return;

                case 0x18:
                    this.Item024 = LanguagePrimitives.ConvertTo<T24>(value);
                    return;

                case 0x19:
                    this.Item025 = LanguagePrimitives.ConvertTo<T25>(value);
                    return;

                case 0x1a:
                    this.Item026 = LanguagePrimitives.ConvertTo<T26>(value);
                    return;

                case 0x1b:
                    this.Item027 = LanguagePrimitives.ConvertTo<T27>(value);
                    return;

                case 0x1c:
                    this.Item028 = LanguagePrimitives.ConvertTo<T28>(value);
                    return;

                case 0x1d:
                    this.Item029 = LanguagePrimitives.ConvertTo<T29>(value);
                    return;

                case 30:
                    this.Item030 = LanguagePrimitives.ConvertTo<T30>(value);
                    return;

                case 0x1f:
                    this.Item031 = LanguagePrimitives.ConvertTo<T31>(value);
                    return;
            }
            throw new ArgumentOutOfRangeException("index");
        }

        public override int Capacity
        {
            get
            {
                return 0x20;
            }
        }

        public T16 Item016
        {
            get
            {
                return this._item16;
            }
            set
            {
                this._item16 = value;
                base._valuesSet[0x10] = true;
            }
        }

        public T17 Item017
        {
            get
            {
                return this._item17;
            }
            set
            {
                this._item17 = value;
                base._valuesSet[0x11] = true;
            }
        }

        public T18 Item018
        {
            get
            {
                return this._item18;
            }
            set
            {
                this._item18 = value;
                base._valuesSet[0x12] = true;
            }
        }

        public T19 Item019
        {
            get
            {
                return this._item19;
            }
            set
            {
                this._item19 = value;
                base._valuesSet[0x13] = true;
            }
        }

        public T20 Item020
        {
            get
            {
                return this._item20;
            }
            set
            {
                this._item20 = value;
                base._valuesSet[20] = true;
            }
        }

        public T21 Item021
        {
            get
            {
                return this._item21;
            }
            set
            {
                this._item21 = value;
                base._valuesSet[0x15] = true;
            }
        }

        public T22 Item022
        {
            get
            {
                return this._item22;
            }
            set
            {
                this._item22 = value;
                base._valuesSet[0x16] = true;
            }
        }

        public T23 Item023
        {
            get
            {
                return this._item23;
            }
            set
            {
                this._item23 = value;
                base._valuesSet[0x17] = true;
            }
        }

        public T24 Item024
        {
            get
            {
                return this._item24;
            }
            set
            {
                this._item24 = value;
                base._valuesSet[0x18] = true;
            }
        }

        public T25 Item025
        {
            get
            {
                return this._item25;
            }
            set
            {
                this._item25 = value;
                base._valuesSet[0x19] = true;
            }
        }

        public T26 Item026
        {
            get
            {
                return this._item26;
            }
            set
            {
                this._item26 = value;
                base._valuesSet[0x1a] = true;
            }
        }

        public T27 Item027
        {
            get
            {
                return this._item27;
            }
            set
            {
                this._item27 = value;
                base._valuesSet[0x1b] = true;
            }
        }

        public T28 Item028
        {
            get
            {
                return this._item28;
            }
            set
            {
                this._item28 = value;
                base._valuesSet[0x1c] = true;
            }
        }

        public T29 Item029
        {
            get
            {
                return this._item29;
            }
            set
            {
                this._item29 = value;
                base._valuesSet[0x1d] = true;
            }
        }

        public T30 Item030
        {
            get
            {
                return this._item30;
            }
            set
            {
                this._item30 = value;
                base._valuesSet[30] = true;
            }
        }

        public T31 Item031
        {
            get
            {
                return this._item31;
            }
            set
            {
                this._item31 = value;
                base._valuesSet[0x1f] = true;
            }
        }
    }
}

