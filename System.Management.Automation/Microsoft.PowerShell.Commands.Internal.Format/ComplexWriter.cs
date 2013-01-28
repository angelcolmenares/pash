namespace Microsoft.PowerShell.Commands.Internal.Format
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Text;

    internal sealed class ComplexWriter
    {
        private IndentationManager indentationManager = new IndentationManager();
        private LineOutput lo;
        private const int maxRecursionDepth = 50;
        private StringBuilder stringBuffer = new StringBuilder();
        private int textColumns;

        private void AddToBuffer(string s)
        {
            this.stringBuffer.Append(s);
        }

        private void GenerateFormatEntryDisplay(FormatEntry fe, int currentDepth)
        {
            foreach (object obj2 in fe.formatValueList)
            {
                FormatEntry entry = obj2 as FormatEntry;
                if (entry != null)
                {
                    if (currentDepth < 50)
                    {
                        if (entry.frameInfo != null)
                        {
                            using (this.indentationManager.StackFrame(entry.frameInfo))
                            {
                                this.GenerateFormatEntryDisplay(entry, currentDepth + 1);
                                continue;
                            }
                        }
                        this.GenerateFormatEntryDisplay(entry, currentDepth + 1);
                    }
                }
                else if (obj2 is FormatNewLine)
                {
                    this.WriteToScreen();
                }
                else
                {
                    FormatTextField field = obj2 as FormatTextField;
                    if (field != null)
                    {
                        this.AddToBuffer(field.text);
                    }
                    else
                    {
                        FormatPropertyField field2 = obj2 as FormatPropertyField;
                        if (field2 != null)
                        {
                            this.AddToBuffer(field2.propertyValue);
                        }
                    }
                }
            }
        }

        internal void Initialize(LineOutput lineOutput, int numberOfTextColumns)
        {
            this.lo = lineOutput;
            this.textColumns = numberOfTextColumns;
        }

        internal void WriteObject(List<FormatValue> formatValueList)
        {
            this.indentationManager.Clear();
            foreach (FormatEntry entry in formatValueList)
            {
                this.GenerateFormatEntryDisplay(entry, 0);
            }
            this.WriteToScreen();
        }

        internal void WriteString(string s)
        {
            this.indentationManager.Clear();
            this.AddToBuffer(s);
            this.WriteToScreen();
        }

        private void WriteToScreen()
        {
            int leftIndentation = this.indentationManager.LeftIndentation;
            int rightIndentation = this.indentationManager.RightIndentation;
            int firstLineIndentation = this.indentationManager.FirstLineIndentation;
            int num4 = (this.textColumns - rightIndentation) - leftIndentation;
            if (num4 <= 0)
            {
                this.stringBuffer = new StringBuilder();
            }
            int num5 = (firstLineIndentation > 0) ? firstLineIndentation : -firstLineIndentation;
            if (num5 >= num4)
            {
                firstLineIndentation = 0;
            }
            int firstLineLen = (this.textColumns - rightIndentation) - leftIndentation;
            int followingLinesLen = firstLineLen;
            if (firstLineIndentation >= 0)
            {
                firstLineLen -= firstLineIndentation;
            }
            else
            {
                followingLinesLen += firstLineIndentation;
            }
            StringCollection strings = StringManipulationHelper.GenerateLines(this.lo.DisplayCells, this.stringBuffer.ToString(), firstLineLen, followingLinesLen);
            int count = leftIndentation;
            int num9 = leftIndentation;
            if (firstLineIndentation >= 0)
            {
                count += firstLineIndentation;
            }
            else
            {
                num9 -= firstLineIndentation;
            }
            bool flag = true;
            foreach (string str in strings)
            {
                if (flag)
                {
                    flag = false;
                    this.lo.WriteLine(StringManipulationHelper.PadLeft(str, count));
                }
                else
                {
                    this.lo.WriteLine(StringManipulationHelper.PadLeft(str, num9));
                }
            }
            this.stringBuffer = new StringBuilder();
        }
    }
}

