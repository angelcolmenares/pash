namespace System.Management.Automation.Host
{
    using System;
    using System.Globalization;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public struct Size
    {
        private int width;
        private int height;
        public int Width
        {
            get
            {
                return this.width;
            }
            set
            {
                this.width = value;
            }
        }
        public int Height
        {
            get
            {
                return this.height;
            }
            set
            {
                this.height = value;
            }
        }
        public Size(int width, int height)
        {
            this.width = width;
            this.height = height;
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "{0},{1}", new object[] { this.Width, this.Height });
        }

        public override bool Equals(object obj)
        {
            bool flag = false;
            if (obj is Size)
            {
                flag = this == ((Size) obj);
            }
            return flag;
        }

        public override int GetHashCode()
        {
            long width = 0L;
            if (this.Width < 0)
            {
                if (this.Width == -2147483648)
                {
                    width = (long) (-1 * (this.Width + 1));
                }
                else
                {
                    width = (long) -this.Width;
                }
            }
            else
            {
                width = (long) this.Width;
            }
            width *= (long) 0x100000000L;
            if (this.Height < 0)
            {
                if (this.Height == -2147483648)
                {
                    width += -1 * (this.Height + 1);
                }
                else
                {
                    width += (long)-this.Height;
                }
            }
            else
            {
                width += (long)this.Height;
            }
            return width.GetHashCode();
        }

        public static bool operator ==(Size first, Size second)
        {
            return ((first.Width == second.Width) && (first.Height == second.Height));
        }

        public static bool operator !=(Size first, Size second)
        {
            return !(first == second);
        }
    }
}

