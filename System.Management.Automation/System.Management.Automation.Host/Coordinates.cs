namespace System.Management.Automation.Host
{
    using System;
    using System.Globalization;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public struct Coordinates
    {
        private int x;
        private int y;
        public int X
        {
            get
            {
                return this.x;
            }
            set
            {
                this.x = value;
            }
        }
        public int Y
        {
            get
            {
                return this.y;
            }
            set
            {
                this.y = value;
            }
        }
        public Coordinates(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "{0},{1}", new object[] { this.X, this.Y });
        }

        public override bool Equals(object obj)
        {
            bool flag = false;
            if (obj is Coordinates)
            {
                flag = this == ((Coordinates) obj);
            }
            return flag;
        }

        public override int GetHashCode()
        {
            long x = 0L;
            if (this.X < 0)
            {
                if (this.X == -2147483648)
                {
                    x = (long) (-1 * (this.X + 1));
                }
                else
                {
                    x = (long) -this.X;
                }
            }
            else
            {
                x = (long) this.X;
            }
            x *= (long) 0x100000000L;
            if (this.Y < 0)
            {
                if (this.Y == -2147483648)
                {
                    x += -1 * (this.Y + 1);
                }
                else
                {
                    x += -this.Y;
                }
            }
            else
            {
                x += this.Y;
            }
            return x.GetHashCode();
        }

        public static bool operator ==(Coordinates first, Coordinates second)
        {
            return ((first.X == second.X) && (first.Y == second.Y));
        }

        public static bool operator !=(Coordinates first, Coordinates second)
        {
            return !(first == second);
        }
    }
}

