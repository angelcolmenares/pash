namespace System.Management.Automation.Help
{
    using System;
    using System.Globalization;

    internal class CultureSpecificUpdatableHelp
    {
        private CultureInfo _culture;
        private System.Version _version;

        internal CultureSpecificUpdatableHelp(CultureInfo culture, System.Version version)
        {
            this._culture = culture;
            this._version = version;
        }

        internal CultureInfo Culture
        {
            get
            {
                return this._culture;
            }
            set
            {
                this._culture = value;
            }
        }

        internal System.Version Version
        {
            get
            {
                return this._version;
            }
            set
            {
                this._version = value;
            }
        }
    }
}

