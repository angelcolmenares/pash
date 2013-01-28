namespace System.Management.Automation.Language
{
    using System;

    internal sealed class EmptyScriptPosition : IScriptPosition
    {
        public string GetFullScript()
        {
            return null;
        }

        public int ColumnNumber
        {
            get
            {
                return 0;
            }
        }

        public string File
        {
            get
            {
                return null;
            }
        }

        public string Line
        {
            get
            {
                return "";
            }
        }

        public int LineNumber
        {
            get
            {
                return 0;
            }
        }

        public int Offset
        {
            get
            {
                return 0;
            }
        }
    }
}

