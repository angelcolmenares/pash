namespace Microsoft.PowerShell.Commands
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.Management.Automation;
    using System.Management.Automation.Host;
    using System.Security;
    using System.Text;

    [Cmdlet("Read", "Host", HelpUri="http://go.microsoft.com/fwlink/?LinkID=113371"), OutputType(new Type[] { typeof(string), typeof(SecureString) })]
    public sealed class ReadHostCommand : PSCmdlet
    {
        private object prompt;
        private bool safe;

        protected override void BeginProcessing()
        {
            PSHostUserInterface uI = base.Host.UI;
            if (this.prompt != null)
            {
                string str;
                IEnumerator enumerator = LanguagePrimitives.GetEnumerator(this.prompt);
                if (enumerator != null)
                {
                    StringBuilder builder = new StringBuilder();
                    while (enumerator.MoveNext())
                    {
                        string str2 = (string) LanguagePrimitives.ConvertTo(enumerator.Current, typeof(string), CultureInfo.InvariantCulture);
                        if (!string.IsNullOrEmpty(str2))
                        {
                            if (builder.Length > 0)
                            {
                                builder.Append(' ');
                            }
                            builder.Append(str2);
                        }
                    }
                    str = builder.ToString();
                }
                else
                {
                    str = (string) LanguagePrimitives.ConvertTo(this.prompt, typeof(string), CultureInfo.InvariantCulture);
                }
                FieldDescription description = new FieldDescription(str);
                if (this.AsSecureString != 0)
                {
                    description.SetParameterType(typeof(SecureString));
                }
                else
                {
                    description.SetParameterType(typeof(string));
                }
                Collection<FieldDescription> descriptions = new Collection<FieldDescription> {
                    description
                };
                Dictionary<string, PSObject> dictionary = base.Host.UI.Prompt("", "", descriptions);
                if (dictionary != null)
                {
                    foreach (PSObject obj2 in dictionary.Values)
                    {
                        base.WriteObject(obj2);
                    }
                }
            }
            else
            {
                object obj3;
                if (this.AsSecureString != 0)
                {
                    obj3 = base.Host.UI.ReadLineAsSecureString();
                }
                else
                {
                    obj3 = base.Host.UI.ReadLine();
                }
                base.WriteObject(obj3);
            }
        }

        [Parameter]
        public SwitchParameter AsSecureString
        {
            get
            {
                return this.safe;
            }
            set
            {
                this.safe = (bool) value;
            }
        }

        [AllowNull, Parameter(Position=0, ValueFromRemainingArguments=true)]
        public object Prompt
        {
            get
            {
                return this.prompt;
            }
            set
            {
                this.prompt = value;
            }
        }
    }
}

