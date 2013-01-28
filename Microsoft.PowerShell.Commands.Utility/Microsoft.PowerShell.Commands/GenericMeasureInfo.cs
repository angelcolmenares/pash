namespace Microsoft.PowerShell.Commands
{
    using System;

    public sealed class GenericMeasureInfo : MeasureInfo
    {
        private double? average;
        private int count;
        private double? max;
        private double? min;
        private double? sum;

        public GenericMeasureInfo()
        {
            double? nullable = null;
            this.min = nullable;
            this.average = this.sum = this.max = this.min = nullable;
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

        public double? Maximum
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

        public double? Minimum
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

