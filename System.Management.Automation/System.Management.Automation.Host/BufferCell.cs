namespace System.Management.Automation.Host
{
    using System;
    using System.Globalization;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public struct BufferCell
    {
        private const string StringsBaseName = "MshHostRawUserInterfaceStrings";
        private char character;
        private ConsoleColor foregroundColor;
        private ConsoleColor backgroundColor;
        private System.Management.Automation.Host.BufferCellType bufferCellType;
        public char Character
        {
            get
            {
                return this.character;
            }
            set
            {
                this.character = value;
            }
        }
        public ConsoleColor ForegroundColor
        {
            get
            {
                return this.foregroundColor;
            }
            set
            {
                this.foregroundColor = value;
            }
        }
        public ConsoleColor BackgroundColor
        {
            get
            {
                return this.backgroundColor;
            }
            set
            {
                this.backgroundColor = value;
            }
        }
        public System.Management.Automation.Host.BufferCellType BufferCellType
        {
            get
            {
                return this.bufferCellType;
            }
            set
            {
                this.bufferCellType = value;
            }
        }
        public BufferCell(char character, ConsoleColor foreground, ConsoleColor background, System.Management.Automation.Host.BufferCellType bufferCellType)
        {
            this.character = character;
            this.foregroundColor = foreground;
            this.backgroundColor = background;
            this.bufferCellType = bufferCellType;
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "'{0}' {1} {2} {3}", new object[] { this.Character, this.ForegroundColor, this.BackgroundColor, this.BufferCellType });
        }

        public override bool Equals(object obj)
        {
            bool flag = false;
            if (obj is BufferCell)
            {
                flag = this == ((BufferCell) obj);
            }
            return flag;
        }

        public override int GetHashCode()
        {
            int num = ((int) (this.ForegroundColor ^ this.BackgroundColor)) << 0x10;
            num |= this.Character;
            return num.GetHashCode();
        }

        public static bool operator ==(BufferCell first, BufferCell second)
        {
            return ((((first.Character == second.Character) && (first.BackgroundColor == second.BackgroundColor)) && (first.ForegroundColor == second.ForegroundColor)) && (first.BufferCellType == second.BufferCellType));
        }

        public static bool operator !=(BufferCell first, BufferCell second)
        {
            return !(first == second);
        }
    }
}

