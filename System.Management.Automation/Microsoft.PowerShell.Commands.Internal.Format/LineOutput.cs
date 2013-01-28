namespace Microsoft.PowerShell.Commands.Internal.Format
{
    using System;
    using System.Management.Automation;
    using System.Runtime.CompilerServices;

    internal abstract class LineOutput
    {
        protected static Microsoft.PowerShell.Commands.Internal.Format.DisplayCells _displayCellsDefault = new Microsoft.PowerShell.Commands.Internal.Format.DisplayCells();
        private bool _isStopping;

        protected LineOutput()
        {
        }

        internal void CheckStopProcessing()
        {
            if (this._isStopping)
            {
                throw new PipelineStoppedException();
            }
        }

        internal virtual void ExecuteBufferPlayBack(DoPlayBackCall playback)
        {
        }

        internal void StopProcessing()
        {
            this._isStopping = true;
        }

        internal abstract void WriteLine(string s);

        internal abstract int ColumnNumber { get; }

        internal virtual Microsoft.PowerShell.Commands.Internal.Format.DisplayCells DisplayCells
        {
            get
            {
                this.CheckStopProcessing();
                return _displayCellsDefault;
            }
        }

        internal virtual bool RequiresBuffering
        {
            get
            {
                return false;
            }
        }

        internal abstract int RowNumber { get; }

        internal WriteStreamType WriteStream { get; set; }

        internal delegate void DoPlayBackCall();
    }
}

