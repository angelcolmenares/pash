namespace Microsoft.PowerShell.Commands.Internal.Format
{
    using System;
    using System.Collections.Specialized;
    using System.Text;

    internal class TableWriter
    {
        private bool disabled;
        private const string ellipsis = "...";
        private bool hideHeader;
        private ScreenInfo si;
        private int startColumn;

        internal static int ComputeWideViewBestItemsPerRowFit(int stringLen, int screenColumns)
        {
            if ((stringLen <= 0) || (screenColumns < 1))
            {
                return 1;
            }
            if (stringLen >= screenColumns)
            {
                return 1;
            }
            int num = 1;
            while (true)
            {
                int num2 = num + 1;
                int num3 = (stringLen * num2) + (num2 - 1);
                if (num3 >= screenColumns)
                {
                    return num;
                }
                num++;
            }
        }

        internal void GenerateHeader(string[] values, LineOutput lo)
        {
            if (!this.disabled && !this.hideHeader)
            {
                this.GenerateRow(values, lo, true, null, lo.DisplayCells);
                string[] strArray = new string[values.Length];
                for (int i = 0; i < this.si.columnInfo.Length; i++)
                {
                    if (this.si.columnInfo[i].width <= 0)
                    {
                        strArray[i] = "";
                    }
                    else
                    {
                        int width = this.si.columnInfo[i].width;
                        if (!string.IsNullOrEmpty(values[i]))
                        {
                            int num3 = lo.DisplayCells.Length(values[i]);
                            if (num3 < width)
                            {
                                width = num3;
                            }
                        }
                        strArray[i] = new string('-', width);
                    }
                }
                this.GenerateRow(strArray, lo, false, null, lo.DisplayCells);
            }
        }

        private StringCollection GenerateMultiLineRowField(string val, int k, int aligment, DisplayCells dc)
        {
            StringCollection strings = StringManipulationHelper.GenerateLines(dc, val, this.si.columnInfo[k].width, this.si.columnInfo[k].width);
            for (int i = 0; i < strings.Count; i++)
            {
                if (dc.Length(strings[i]) < this.si.columnInfo[k].width)
                {
                    strings[i] = GenerateRowField(strings[i], this.si.columnInfo[k].width, aligment, dc);
                }
            }
            return strings;
        }

        private string GenerateRow(string[] values, int[] alignment, DisplayCells dc)
        {
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < this.si.columnInfo.Length; i++)
            {
                if (this.si.columnInfo[i].width > 0)
                {
                    int length = builder.Length;
                    if (i > 0)
                    {
                        builder.Append(new string(' ', 1));
                    }
                    else if (this.startColumn > 0)
                    {
                        builder.Append(new string(' ', this.startColumn));
                    }
                    builder.Append(GenerateRowField(values[i], this.si.columnInfo[i].width, alignment[i], dc));
                }
            }
            return builder.ToString();
        }

        internal void GenerateRow(string[] values, LineOutput lo, bool multiLine, int[] alignment, DisplayCells dc)
        {
            if (!this.disabled)
            {
                int length = this.si.columnInfo.Length;
                int[] numArray = new int[length];
                if (alignment == null)
                {
                    for (int i = 0; i < length; i++)
                    {
                        numArray[i] = this.si.columnInfo[i].alignment;
                    }
                }
                else
                {
                    for (int j = 0; j < length; j++)
                    {
                        if (alignment[j] == 0)
                        {
                            numArray[j] = this.si.columnInfo[j].alignment;
                        }
                        else
                        {
                            numArray[j] = alignment[j];
                        }
                    }
                }
                if (multiLine)
                {
                    string[] strArray = this.GenerateTableRow(values, numArray, lo.DisplayCells);
                    for (int k = 0; k < strArray.Length; k++)
                    {
                        lo.WriteLine(strArray[k]);
                    }
                }
                else
                {
                    lo.WriteLine(this.GenerateRow(values, numArray, dc));
                }
            }
        }

        private static string GenerateRowField(string val, int width, int alignment, DisplayCells dc)
        {
            string str = StringManipulationHelper.TruncateAtNewLine(val);
            if (str == null)
            {
                str = "";
            }
            string str2 = str;
            int num = dc.Length(str2);
            if (num < width)
            {
                int count = width - num;
                switch (alignment)
                {
                    case 2:
                    {
                        int num3 = count / 2;
                        int num4 = count - num3;
                        str = new string(' ', num3) + str + new string(' ', num4);
                        goto Label_0182;
                    }
                    case 3:
                        str = new string(' ', count) + str;
                        goto Label_0182;
                }
                str = str + new string(' ', count);
            }
            else if (num > width)
            {
                int displayCells = width - "...".Length;
                if (displayCells > 0)
                {
                    switch (alignment)
                    {
                        case 2:
                            str = str.Substring(0, dc.GetHeadSplitLength(str, displayCells)) + "...";
                            goto Label_0182;

                        case 3:
                        {
                            int tailSplitLength = dc.GetTailSplitLength(str, displayCells);
                            str = str.Substring(str.Length - tailSplitLength);
                            str = "..." + str;
                            goto Label_0182;
                        }
                    }
                    str = str.Substring(0, dc.GetHeadSplitLength(str, displayCells)) + "...";
                }
                else
                {
                    int num7 = width;
                    switch (alignment)
                    {
                        case 2:
                            str = str.Substring(0, dc.GetHeadSplitLength(str, num7));
                            goto Label_0182;

                        case 3:
                        {
                            int length = dc.GetTailSplitLength(str, num7);
                            str = str.Substring(str.Length - length, length);
                            goto Label_0182;
                        }
                    }
                    str = str.Substring(0, dc.GetHeadSplitLength(str, num7));
                }
            }
        Label_0182:
            if (dc.Length(str) == width)
            {
                return str;
            }
            switch (alignment)
            {
                case 2:
                    return (str + " ");

                case 3:
                    return (" " + str);
            }
            return (str + " ");
        }

        private string[] GenerateTableRow(string[] values, int[] alignment, DisplayCells ds)
        {
            int[] numArray = new int[this.si.columnInfo.Length];
            int num = 0;
            for (int i = 0; i < this.si.columnInfo.Length; i++)
            {
                if (this.si.columnInfo[i].width > 0)
                {
                    numArray[num++] = i;
                }
            }
            if (num == 0)
            {
                return null;
            }
            StringCollection[] stringsArray = new StringCollection[num];
            for (int j = 0; j < stringsArray.Length; j++)
            {
                stringsArray[j] = this.GenerateMultiLineRowField(values[numArray[j]], numArray[j], alignment[numArray[j]], ds);
                if (j > 0)
                {
                    for (int num4 = 0; num4 < stringsArray[j].Count; num4++)
                    {
                        stringsArray[j][num4] = new string(' ', 1) + stringsArray[j][num4];
                    }
                }
                else if (this.startColumn > 0)
                {
                    for (int num5 = 0; num5 < stringsArray[j].Count; num5++)
                    {
                        stringsArray[j][num5] = new string(' ', this.startColumn) + stringsArray[j][num5];
                    }
                }
            }
            int count = 0;
            for (int k = 0; k < stringsArray.Length; k++)
            {
                if (stringsArray[k].Count > count)
                {
                    count = stringsArray[k].Count;
                }
            }
            for (int m = 0; m < stringsArray.Length; m++)
            {
                int width = this.si.columnInfo[numArray[m]].width;
                if (m > 0)
                {
                    width++;
                }
                else
                {
                    width += this.startColumn;
                }
                int num10 = count - stringsArray[m].Count;
                if (num10 > 0)
                {
                    for (int num11 = 0; num11 < num10; num11++)
                    {
                        stringsArray[m].Add(new string(' ', width));
                    }
                }
            }
            string[] strArray = new string[count];
            for (int n = 0; n < strArray.Length; n++)
            {
                StringBuilder builder = new StringBuilder();
                for (int num13 = 0; num13 < stringsArray.Length; num13++)
                {
                    builder.Append(stringsArray[num13][n]);
                }
                strArray[n] = builder.ToString();
            }
            return strArray;
        }

        internal void Initialize(int leftMarginIndent, int screenColumns, int[] columnWidths, int[] alignment, bool suppressHeader)
        {
            if (leftMarginIndent < 0)
            {
                leftMarginIndent = 0;
            }
            if ((screenColumns - leftMarginIndent) < 5)
            {
                this.disabled = true;
            }
            else
            {
                this.startColumn = leftMarginIndent;
                this.hideHeader = suppressHeader;
                new ColumnWidthManager(screenColumns - leftMarginIndent, 1, 1).CalculateColumnWidths(columnWidths);
                bool flag = false;
                for (int i = 0; i < columnWidths.Length; i++)
                {
                    if (columnWidths[i] >= 1)
                    {
                        flag = true;
                        break;
                    }
                }
                if (!flag)
                {
                    this.disabled = true;
                }
                else
                {
                    this.si = new ScreenInfo();
                    this.si.screenColumns = screenColumns;
                    this.si.columnInfo = new ColumnInfo[columnWidths.Length];
                    int startColumn = this.startColumn;
                    for (int j = 0; j < columnWidths.Length; j++)
                    {
                        this.si.columnInfo[j] = new ColumnInfo();
                        this.si.columnInfo[j].startCol = startColumn;
                        this.si.columnInfo[j].width = columnWidths[j];
                        this.si.columnInfo[j].alignment = alignment[j];
                        startColumn += columnWidths[j] + 1;
                    }
                }
            }
        }

        private class ColumnInfo
        {
            internal int alignment = 1;
            internal int startCol;
            internal int width;
        }

        private class ScreenInfo
        {
            internal TableWriter.ColumnInfo[] columnInfo;
            internal const int minimumColumnWidth = 1;
            internal const int minimumScreenColumns = 5;
            internal int screenColumns;
            internal const int separatorCharacterCount = 1;
        }
    }
}

