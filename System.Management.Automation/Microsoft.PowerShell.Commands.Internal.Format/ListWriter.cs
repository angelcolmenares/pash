namespace Microsoft.PowerShell.Commands.Internal.Format
{
    using System;
    using System.Collections.Specialized;

    internal class ListWriter
    {
        private int columnWidth;
        private bool disabled;
        private const int MinFieldWidth = 1;
        private const int MinLabelWidth = 1;
        private string[] propertyLabels;
        private int propertyLabelsDisplayLength;
        private const string Separator = " : ";

        internal void Initialize(string[] propertyNames, int screenColumnWidth, DisplayCells dc)
        {
            this.columnWidth = screenColumnWidth;
            if ((propertyNames == null) || (propertyNames.Length == 0))
            {
                this.disabled = true;
            }
            else
            {
                this.disabled = false;
                if ((((screenColumnWidth - " : ".Length) - 1) - 1) < 0)
                {
                    this.disabled = true;
                }
                else
                {
                    int num = (screenColumnWidth - " : ".Length) - 1;
                    this.propertyLabelsDisplayLength = 0;
                    int[] numArray = new int[propertyNames.Length];
                    for (int i = 0; i < propertyNames.Length; i++)
                    {
                        numArray[i] = dc.Length(propertyNames[i]);
                        if (numArray[i] > this.propertyLabelsDisplayLength)
                        {
                            this.propertyLabelsDisplayLength = numArray[i];
                        }
                    }
                    if (this.propertyLabelsDisplayLength > num)
                    {
                        this.propertyLabelsDisplayLength = num;
                    }
                    this.propertyLabels = new string[propertyNames.Length];
                    for (int j = 0; j < propertyNames.Length; j++)
                    {
                        string[] strArray = null;
                        IntPtr ptr;
                        if (numArray[j] < this.propertyLabelsDisplayLength)
                        {
                            this.propertyLabels[j] = propertyNames[j] + new string(' ', this.propertyLabelsDisplayLength - numArray[j]);
                        }
                        else if (numArray[j] > this.propertyLabelsDisplayLength)
                        {
                            this.propertyLabels[j] = propertyNames[j].Substring(0, dc.GetHeadSplitLength(propertyNames[j], this.propertyLabelsDisplayLength));
                        }
                        else
                        {
                            this.propertyLabels[j] = propertyNames[j];
                        }
						strArray = this.propertyLabels;
						ptr = (IntPtr) j;
						strArray[(int) (ptr)] = strArray[(int) ptr] + " : ";
                    }
                    this.propertyLabelsDisplayLength += " : ".Length;
                }
            }
        }

        internal void WriteProperties(string[] values, LineOutput lo)
        {
            if (!this.disabled)
            {
                string[] strArray = null;
                if (values == null)
                {
                    strArray = new string[this.propertyLabels.Length];
                    for (int j = 0; j < this.propertyLabels.Length; j++)
                    {
                        strArray[j] = "";
                    }
                }
                else if (values.Length < this.propertyLabels.Length)
                {
                    strArray = new string[this.propertyLabels.Length];
                    for (int k = 0; k < this.propertyLabels.Length; k++)
                    {
                        if (k < values.Length)
                        {
                            strArray[k] = values[k];
                        }
                        else
                        {
                            strArray[k] = "";
                        }
                    }
                }
                else if (values.Length > this.propertyLabels.Length)
                {
                    strArray = new string[this.propertyLabels.Length];
                    for (int m = 0; m < this.propertyLabels.Length; m++)
                    {
                        strArray[m] = values[m];
                    }
                }
                else
                {
                    strArray = values;
                }
                for (int i = 0; i < this.propertyLabels.Length; i++)
                {
                    this.WriteProperty(i, strArray[i], lo);
                }
            }
        }

        private void WriteProperty(int k, string propertyValue, LineOutput lo)
        {
            if (propertyValue == null)
            {
                propertyValue = "";
            }
            string[] strArray = StringManipulationHelper.SplitLines(propertyValue);
            string str = null;
            for (int i = 0; i < strArray.Length; i++)
            {
                string prependString = null;
                if (i == 0)
                {
                    prependString = this.propertyLabels[k];
                }
                else
                {
                    if (str == null)
                    {
                        str = prependString = new string(' ', this.propertyLabelsDisplayLength);
                    }
                    prependString = str;
                }
                this.WriteSingleLineHelper(prependString, strArray[i], lo);
            }
        }

        private void WriteSingleLineHelper(string prependString, string line, LineOutput lo)
        {
            if (line == null)
            {
                line = "";
            }
            int firstLineLen = this.columnWidth - this.propertyLabelsDisplayLength;
            StringCollection strings = StringManipulationHelper.GenerateLines(lo.DisplayCells, line, firstLineLen, firstLineLen);
            string str = new string(' ', this.propertyLabelsDisplayLength);
            for (int i = 0; i < strings.Count; i++)
            {
                if (i == 0)
                {
                    lo.WriteLine(prependString + strings[i]);
                }
                else
                {
                    lo.WriteLine(str + strings[i]);
                }
            }
        }
    }
}

