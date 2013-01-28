namespace Microsoft.PowerShell.Commands.Internal.Format
{
    using System;
    using System.Management.Automation;
    using System.Runtime.CompilerServices;

    internal class WriteLineHelper
    {
        private DisplayCells _displayCells;
        private bool lineWrap;
        private WriteCallback writeCall;
        private WriteCallback writeLineCall;

        internal WriteLineHelper(bool lineWrap, WriteCallback wlc, WriteCallback wc, DisplayCells displayCells)
        {
            if (wlc == null)
            {
                throw PSTraceSource.NewArgumentNullException("wlc");
            }
            if (displayCells == null)
            {
                throw PSTraceSource.NewArgumentNullException("displayCells");
            }
            this._displayCells = displayCells;
            this.writeLineCall = wlc;
            this.writeCall = (wc != null) ? wc : wlc;
            this.lineWrap = lineWrap;
        }

        internal void WriteLine(string s, int cols)
        {
            this.WriteLineInternal(s, cols);
        }

        private void WriteLineInternal(string val, int cols)
        {
            if (string.IsNullOrEmpty(val))
            {
                this.writeLineCall(val);
            }
            else if (!this.lineWrap)
            {
                this.writeCall(val);
            }
            else
            {
                string[] strArray = StringManipulationHelper.SplitLines(val);
                for (int i = 0; i < strArray.Length; i++)
                {
                    int num2 = this._displayCells.Length(strArray[i]);
                    if (num2 < cols)
                    {
                        this.writeLineCall(strArray[i]);
                    }
                    else if (num2 == cols)
                    {
                        this.writeCall(strArray[i]);
                    }
                    else
                    {
                        string str = strArray[i];
                        do
                        {
                            int headSplitLength = this._displayCells.GetHeadSplitLength(str, cols);
                            this.WriteLineInternal(str.Substring(0, headSplitLength), cols);
                            str = str.Substring(headSplitLength);
                        }
                        while (this._displayCells.Length(str) > cols);
                        this.WriteLineInternal(str, cols);
                    }
                }
            }
        }

        internal delegate void WriteCallback(string s);
    }
}

