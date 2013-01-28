namespace System.Management.Automation.Host
{
    using System;
    using System.Globalization;
    using System.Management.Automation;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public struct Rectangle
    {
        private const string StringsBaseName = "MshHostRawUserInterfaceStrings";
        private const string LessThanErrorTemplateResource = "LessThanErrorTemplate";
        private int left;
        private int top;
        private int right;
        private int bottom;
        public int Left
        {
            get
            {
                return this.left;
            }
            set
            {
                this.left = value;
            }
        }
        public int Top
        {
            get
            {
                return this.top;
            }
            set
            {
                this.top = value;
            }
        }
        public int Right
        {
            get
            {
                return this.right;
            }
            set
            {
                this.right = value;
            }
        }
        public int Bottom
        {
            get
            {
                return this.bottom;
            }
            set
            {
                this.bottom = value;
            }
        }
        public Rectangle(int left, int top, int right, int bottom)
        {
            if (right < left)
            {
                throw PSTraceSource.NewArgumentException("right", "MshHostRawUserInterfaceStrings", "LessThanErrorTemplate", new object[] { "right", "left" });
            }
            if (bottom < top)
            {
                throw PSTraceSource.NewArgumentException("bottom", "MshHostRawUserInterfaceStrings", "LessThanErrorTemplate", new object[] { "bottom", "top" });
            }
            this.left = left;
            this.top = top;
            this.right = right;
            this.bottom = bottom;
        }

        public Rectangle(Coordinates upperLeft, Coordinates lowerRight) : this(upperLeft.X, upperLeft.Y, lowerRight.X, lowerRight.Y)
        {
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "{0},{1} ; {2},{3}", new object[] { this.Left, this.Top, this.Right, this.Bottom });
        }

        public override bool Equals(object obj)
        {
            bool flag = false;
            if (obj is Rectangle)
            {
                flag = this == ((Rectangle) obj);
            }
            return flag;
        }

        public override int GetHashCode()
        {
            long num = 0L;
            int num2 = this.Top ^ this.Bottom;
            if (num2 < 0)
            {
                if (num2 == -2147483648)
                {
                    num = (long) (-1 * (num2 + 1));
                }
                else
                {
                    num = (long) -num2;
                }
            }
            else
            {
                num = (long) num2;
            }
            num *= (long) 0x100000000L;
            int num3 = this.Left ^ this.Right;
            if (num3 < 0)
            {
                if (num3 == -2147483648)
                {
                    num += -1 * (num3 + 1);
                }
                else
                {
                    num += -num2;
                }
            }
            else
            {
                num += num3;
            }
            return num.GetHashCode();
        }

        public static bool operator ==(Rectangle first, Rectangle second)
        {
            return ((((first.Top == second.Top) && (first.Left == second.Left)) && (first.Bottom == second.Bottom)) && (first.Right == second.Right));
        }

        public static bool operator !=(Rectangle first, Rectangle second)
        {
            return !(first == second);
        }
    }
}

