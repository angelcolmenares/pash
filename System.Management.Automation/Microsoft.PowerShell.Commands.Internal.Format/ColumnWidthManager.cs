namespace Microsoft.PowerShell.Commands.Internal.Format
{
    using System;

    internal sealed class ColumnWidthManager
    {
        private int minimumColumnWidth;
        private int separatorWidth;
        private int tableWidth;

        internal ColumnWidthManager(int tableWidth, int minimumColumnWidth, int separatorWidth)
        {
            this.tableWidth = tableWidth;
            this.minimumColumnWidth = minimumColumnWidth;
            this.separatorWidth = separatorWidth;
        }

        private bool AssignColumnWidths(int[] columnWidths)
        {
            bool flag = true;
            int num = 0;
            for (int i = 0; i < columnWidths.Length; i++)
            {
                if (columnWidths[i] <= 0)
                {
                    flag = false;
                    break;
                }
                num += columnWidths[i];
            }
            if (flag)
            {
                num += this.separatorWidth * (columnWidths.Length - 1);
                return (num <= this.tableWidth);
            }
            bool[] flagArray = new bool[columnWidths.Length];
            for (int j = 0; j < columnWidths.Length; j++)
            {
                flagArray[j] = columnWidths[j] > 0;
                if (columnWidths[j] == 0)
                {
                    columnWidths[j] = this.minimumColumnWidth;
                }
            }
            int num4 = this.CurrentTableWidth(columnWidths);
            int num5 = this.tableWidth - num4;
            if (num5 < 0)
            {
                return false;
            }
            if (num5 != 0)
            {
                while (num5 > 0)
                {
                    for (int k = 0; k < columnWidths.Length; k++)
                    {
                        if (!flagArray[k])
                        {
                            columnWidths[k]++;
                            num5--;
                            if (num5 == 0)
                            {
                                break;
                            }
                        }
                    }
                }
                return true;
            }
            return true;
        }

        internal void CalculateColumnWidths(int[] columnWidths)
        {
            if (!this.AssignColumnWidths(columnWidths))
            {
                this.TrimToFit(columnWidths);
            }
        }

        private int CurrentTableWidth(int[] columnWidths)
        {
            int num = 0;
            int num2 = 0;
            for (int i = 0; i < columnWidths.Length; i++)
            {
                if (columnWidths[i] > 0)
                {
                    num += columnWidths[i];
                    num2++;
                }
            }
            return (num + (this.separatorWidth * (num2 - 1)));
        }

        private static int GetLastVisibleColumn(int[] columnWidths)
        {
            for (int i = 0; i < columnWidths.Length; i++)
            {
                if (columnWidths[i] < 0)
                {
                    return (i - 1);
                }
            }
            return (columnWidths.Length - 1);
        }

        private void TrimToFit(int[] columnWidths)
        {
            int num2;
        Label_0000:
            num2 = this.CurrentTableWidth(columnWidths) - this.tableWidth;
            if (num2 > 0)
            {
                int lastVisibleColumn = GetLastVisibleColumn(columnWidths);
                if (lastVisibleColumn >= 0)
                {
                    int num4 = columnWidths[lastVisibleColumn] - num2;
                    if (num4 < this.minimumColumnWidth)
                    {
                        columnWidths[lastVisibleColumn] = -1;
                    }
                    else
                    {
                        columnWidths[lastVisibleColumn] = num4;
                    }
                    goto Label_0000;
                }
            }
        }
    }
}

