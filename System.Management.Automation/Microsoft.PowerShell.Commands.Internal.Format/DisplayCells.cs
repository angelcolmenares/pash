namespace Microsoft.PowerShell.Commands.Internal.Format
{
    using System;

    internal class DisplayCells
    {
        internal virtual int GetHeadSplitLength(string str, int displayCells)
        {
            return this.GetHeadSplitLength(str, 0, displayCells);
        }

        internal virtual int GetHeadSplitLength(string str, int offset, int displayCells)
        {
            int num = str.Length - offset;
            if (num >= displayCells)
            {
                return displayCells;
            }
            return num;
        }

        protected int GetSplitLengthInternalHelper(string str, int offset, int displayCells, bool head)
        {
            int num = 0;
            int num2 = 0;
            int num4 = head ? offset : (str.Length - 1);
            int num5 = head ? (str.Length - 1) : offset;
            while ((!head || (num4 <= num5)) && (head || (num4 >= num5)))
            {
                int num3 = this.Length(str[num4]);
                if ((num + num3) > displayCells)
                {
                    return num2;
                }
                num += num3;
                num2++;
                if (num == displayCells)
                {
                    return num2;
                }
                num4 = head ? (num4 + 1) : (num4 - 1);
            }
            return num2;
        }

        internal virtual int GetTailSplitLength(string str, int displayCells)
        {
            return this.GetTailSplitLength(str, 0, displayCells);
        }

        internal virtual int GetTailSplitLength(string str, int offset, int displayCells)
        {
            int num = str.Length - offset;
            if (num >= displayCells)
            {
                return displayCells;
            }
            return num;
        }

        internal virtual int Length(char character)
        {
            return 1;
        }

        internal virtual int Length(string str)
        {
            return this.Length(str, 0);
        }

        internal virtual int Length(string str, int offset)
        {
            return (str.Length - offset);
        }
    }
}

