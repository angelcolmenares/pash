namespace System.Management.Automation.Language
{
    using System;

    public class NullString
    {
        private static readonly NullString _value = new NullString();

        private NullString()
        {
        }

        public override string ToString()
        {
            return null;
        }

        public static NullString Value
        {
            get
            {
                return _value;
            }
        }
    }
}

