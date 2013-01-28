namespace Microsoft.PowerShell.Commands
{
    using System;
    using System.Globalization;
    using System.Management.Automation;
    using System.Threading;

    public class ObjectCmdletBase : PSCmdlet
    {
        private bool _caseSensitive;
        internal CultureInfo _cultureInfo;

        [Parameter]
        public SwitchParameter CaseSensitive
        {
            get
            {
                return this._caseSensitive;
            }
            set
            {
                this._caseSensitive = (bool) value;
            }
        }

        [Parameter]
        public string Culture
        {
            get
            {
                if (this._cultureInfo == null)
                {
                    return null;
                }
                return this._cultureInfo.ToString();
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    this._cultureInfo = null;
                }
                else
                {
                    int num;
                    string s = value.Trim();
                    if (s.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
                    {
                        if ((s.Length > 2) && int.TryParse(s.Substring(2), NumberStyles.AllowHexSpecifier, Thread.CurrentThread.CurrentCulture, out num))
                        {
                            this._cultureInfo = new CultureInfo(num);
                            return;
                        }
                    }
                    else if (int.TryParse(s, NumberStyles.AllowThousands, Thread.CurrentThread.CurrentCulture, out num))
                    {
                        this._cultureInfo = new CultureInfo(num);
                        return;
                    }
                    this._cultureInfo = new CultureInfo(value);
                }
            }
        }
    }
}

