namespace System.Management.Automation.Language
{
    using System;

    internal sealed class EmptyScriptExtent : IScriptExtent
    {
        public override bool Equals(object obj)
        {
            IScriptExtent extent = obj as IScriptExtent;
            if (extent == null)
            {
                return false;
            }
            return (((string.IsNullOrEmpty(extent.File) && (extent.StartLineNumber == this.StartLineNumber)) && ((extent.StartColumnNumber == this.StartColumnNumber) && (extent.EndLineNumber == this.EndLineNumber))) && ((extent.EndColumnNumber == this.EndColumnNumber) && string.IsNullOrEmpty(extent.Text)));
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public int EndColumnNumber
        {
            get
            {
                return 0;
            }
        }

        public int EndLineNumber
        {
            get
            {
                return 0;
            }
        }

        public int EndOffset
        {
            get
            {
                return 0;
            }
        }

        public IScriptPosition EndScriptPosition
        {
            get
            {
                return PositionUtilities.EmptyPosition;
            }
        }

        public string File
        {
            get
            {
                return null;
            }
        }

        public int StartColumnNumber
        {
            get
            {
                return 0;
            }
        }

        public int StartLineNumber
        {
            get
            {
                return 0;
            }
        }

        public int StartOffset
        {
            get
            {
                return 0;
            }
        }

        public IScriptPosition StartScriptPosition
        {
            get
            {
                return PositionUtilities.EmptyPosition;
            }
        }

        public string Text
        {
            get
            {
                return "";
            }
        }
    }
}

