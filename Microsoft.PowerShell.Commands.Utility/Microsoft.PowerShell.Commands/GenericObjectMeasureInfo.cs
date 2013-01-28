namespace Microsoft.PowerShell.Commands
{
    using System;

    public sealed class GenericObjectMeasureInfo : MeasureInfo
    {
        private double? average;
        private int count;
        private object max;
        private object min;
        private double? sum;

        public GenericObjectMeasureInfo()
        {
            double? nullable = null;
            this.sum = nullable;
            this.average = this.sum = nullable;
            this.max = this.min = null;
        }

        public double? Average
        {
            get
            {
                return this.average;
            }
            set
            {
                this.average = value;
            }
        }

        public int Count
        {
            get
            {
                return this.count;
            }
            set
            {
                this.count = value;
            }
        }

        public object Maximum
        {
            get
            {
                return this.max;
            }
            set
            {
                this.max = value;
            }
        }

        public object Minimum
        {
            get
            {
                return this.min;
            }
            set
            {
                this.min = value;
            }
        }

        public double? Sum
        {
            get
            {
                return this.sum;
            }
            set
            {
                this.sum = value;
            }
        }
    }
}

