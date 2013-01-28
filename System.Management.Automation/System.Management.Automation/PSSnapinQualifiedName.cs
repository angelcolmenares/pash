namespace System.Management.Automation
{
    using System;
    using System.Globalization;

    internal class PSSnapinQualifiedName
    {
        private string _fullName;
        private string _psSnapinName;
        private string _shortName;

        private PSSnapinQualifiedName(string[] splitName)
        {
            if (splitName.Length == 1)
            {
                this._shortName = splitName[0];
            }
            else
            {
                if (splitName.Length != 2)
                {
                    throw PSTraceSource.NewArgumentException("name");
                }
                if (!string.IsNullOrEmpty(splitName[0]))
                {
                    this._psSnapinName = splitName[0];
                }
                this._shortName = splitName[1];
            }
            if (!string.IsNullOrEmpty(this._psSnapinName))
            {
                this._fullName = string.Format(CultureInfo.InvariantCulture, @"{0}\{1}", new object[] { this._psSnapinName, this._shortName });
            }
            else
            {
                this._fullName = this._shortName;
            }
        }

        internal static PSSnapinQualifiedName GetInstance(string name)
        {
            if (name == null)
            {
                return null;
            }
            PSSnapinQualifiedName name2 = null;
            string[] splitName = name.Split(new char[] { '\\' });
            if ((splitName.Length < 0) || (splitName.Length > 2))
            {
                return null;
            }
            name2 = new PSSnapinQualifiedName(splitName);
            if (string.IsNullOrEmpty(name2.ShortName))
            {
                return null;
            }
            return name2;
        }

        public override string ToString()
        {
            return this._fullName;
        }

        internal string FullName
        {
            get
            {
                return this._fullName;
            }
        }

        internal string PSSnapInName
        {
            get
            {
                return this._psSnapinName;
            }
        }

        internal string ShortName
        {
            get
            {
                return this._shortName;
            }
        }
    }
}

