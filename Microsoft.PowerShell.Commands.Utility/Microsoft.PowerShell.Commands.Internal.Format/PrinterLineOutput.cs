namespace Microsoft.PowerShell.Commands.Internal.Format
{
    using Microsoft.PowerShell.Commands.Utility;
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Drawing.Printing;

    internal sealed class PrinterLineOutput : LineOutput
    {
        private static readonly string DefaultPrintFontName = OutPrinterDisplayStrings.DefaultPrintFontName;
        private const int DefaultPrintFontSize = 8;
        private int deviceColumns = 80;
        private int deviceRows = 40;
        private Queue<string> lines = new Queue<string>();
        private LineOutput.DoPlayBackCall playbackCall;
        private string printerName;
        private Font printFont;
        private string printFontName;
        private int printFontSize;
        private bool printingInitalized;
        private WriteLineHelper writeLineHelper;

        internal PrinterLineOutput(string printerName)
        {
            this.printerName = printerName;
            WriteLineHelper.WriteCallback wlc = new WriteLineHelper.WriteCallback(this.OnWriteLine);
            WriteLineHelper.WriteCallback wc = new WriteLineHelper.WriteCallback(this.OnWrite);
            this.writeLineHelper = new WriteLineHelper(true, wlc, wc, this.DisplayCells);
        }

        private void CreateFont(Graphics g)
        {
            if (this.printFont == null)
            {
                if (string.IsNullOrEmpty(this.printFontName))
                {
                    this.printFontName = DefaultPrintFontName;
                }
                if (this.printFontSize <= 0)
                {
                    this.printFontSize = 8;
                }
                this.printFont = new Font(this.printFontName, (float) this.printFontSize);
                this.VerifyFont(g);
            }
        }

        private void DoPrint()
        {
            try
            {
                PrintDocument document = new PrintDocument();
                if (!string.IsNullOrEmpty(this.printerName))
                {
                    document.PrinterSettings.PrinterName = this.printerName;
                }
                document.PrintPage += new PrintPageEventHandler(this.pd_PrintPage);
                document.Print();
            }
            finally
            {
                if (this.printFont != null)
                {
                    this.printFont.Dispose();
                    this.printFont = null;
                }
            }
        }

        internal override void ExecuteBufferPlayBack(LineOutput.DoPlayBackCall playback)
        {
            this.playbackCall = playback;
            this.DoPrint();
        }

        private void OnWrite(string s)
        {
            this.lines.Enqueue(s);
        }

        private void OnWriteLine(string s)
        {
            this.lines.Enqueue(s);
        }

        private void pd_PrintPage(object sender, PrintPageEventArgs ev)
        {
            float y = 0f;
            int num2 = 0;
            float left = ev.MarginBounds.Left;
            float top = ev.MarginBounds.Top;
            this.CreateFont(ev.Graphics);
            float height = this.printFont.GetHeight(ev.Graphics);
            float num6 = ((float) ev.MarginBounds.Height) / this.printFont.GetHeight(ev.Graphics);
            if (!this.printingInitalized)
            {
                string text = "ABCDEF";
                float num7 = ev.Graphics.MeasureString(text, this.printFont).Width / ((float) text.Length);
                float num8 = ((float) ev.MarginBounds.Width) / num7;
                this.printingInitalized = true;
                this.deviceRows = (int) num6;
                this.deviceColumns = (int) num8;
                this.playbackCall();
            }
            while ((num2 < num6) && (this.lines.Count > 0))
            {
                string s = this.lines.Dequeue();
                y = top + (num2 * height);
                ev.Graphics.DrawString(s, this.printFont, Brushes.Black, left, y, new StringFormat());
                num2++;
            }
            ev.HasMorePages = this.lines.Count > 0;
        }

        private void VerifyFont(Graphics g)
        {
            string text = "ABCDEF";
            float num = g.MeasureString(text, this.printFont).Width / ((float) text.Length);
            string str2 = ".;'}l|";
            float num2 = g.MeasureString(str2, this.printFont).Width / ((float) str2.Length);
            if (Math.Abs((float) (num - num2)) >= 0.001f)
            {
                this.printFont.Dispose();
                this.printFont = new Font(DefaultPrintFontName, 8f);
            }
        }

        internal override void WriteLine(string s)
        {
            base.CheckStopProcessing();
            this.writeLineHelper.WriteLine(s, this.ColumnNumber);
        }

        internal override int ColumnNumber
        {
            get
            {
                base.CheckStopProcessing();
                return this.deviceColumns;
            }
        }

        internal override bool RequiresBuffering
        {
            get
            {
                return true;
            }
        }

        internal override int RowNumber
        {
            get
            {
                base.CheckStopProcessing();
                return this.deviceRows;
            }
        }
    }
}

