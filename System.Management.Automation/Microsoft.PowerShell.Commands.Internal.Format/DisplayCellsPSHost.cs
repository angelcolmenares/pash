namespace Microsoft.PowerShell.Commands.Internal.Format
{
    using System;
    using System.Management.Automation.Host;

    internal class DisplayCellsPSHost : DisplayCells
    {
        private PSHostRawUserInterface _rawUserInterface;

        internal DisplayCellsPSHost(PSHostRawUserInterface rawUserInterface)
        {
            this._rawUserInterface = rawUserInterface;
        }

        internal override int GetHeadSplitLength(string str, int offset, int displayCells)
        {
            return base.GetSplitLengthInternalHelper(str, offset, displayCells, true);
        }

        internal override int GetTailSplitLength(string str, int offset, int displayCells)
        {
            return base.GetSplitLengthInternalHelper(str, offset, displayCells, false);
        }

        internal override int Length(char character)
        {
            try
            {
                return this._rawUserInterface.LengthInBufferCells(character);
            }
            catch (HostException)
            {
            }
            return 1;
        }

        internal override int Length(string str)
        {
            try
            {
                return this._rawUserInterface.LengthInBufferCells(str);
            }
            catch (HostException)
            {
            }
            if (!string.IsNullOrEmpty(str))
            {
                return str.Length;
            }
            return 0;
        }

        internal override int Length(string str, int offset)
        {
            try
            {
                return this._rawUserInterface.LengthInBufferCells(str, offset);
            }
            catch (HostException)
            {
            }
            if (!string.IsNullOrEmpty(str))
            {
                return (str.Length - offset);
            }
            return 0;
        }
    }
}

