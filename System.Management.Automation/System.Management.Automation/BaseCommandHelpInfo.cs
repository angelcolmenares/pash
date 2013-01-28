namespace System.Management.Automation
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Management.Automation.Runspaces;
    using System.Text;

    internal abstract class BaseCommandHelpInfo : HelpInfo
    {
        private System.Management.Automation.HelpCategory _helpCategory;

        internal BaseCommandHelpInfo(System.Management.Automation.HelpCategory helpCategory)
        {
            this._helpCategory = helpCategory;
        }

        internal override PSObject[] GetParameter(string pattern)
        {
            if (((this.FullHelp == null) || (this.FullHelp.Properties["parameters"] == null)) || (this.FullHelp.Properties["parameters"].Value == null))
            {
                return base.GetParameter(pattern);
            }
            PSObject obj2 = PSObject.AsPSObject(this.FullHelp.Properties["parameters"].Value);
            if (obj2.Properties["parameter"] == null)
            {
                return base.GetParameter(pattern);
            }
            PSObject[] objArray = (PSObject[]) LanguagePrimitives.ConvertTo(obj2.Properties["parameter"].Value, typeof(PSObject[]), CultureInfo.InvariantCulture);
            if (string.IsNullOrEmpty(pattern))
            {
                return objArray;
            }
            List<PSObject> list = new List<PSObject>();
            WildcardPattern pattern2 = new WildcardPattern(pattern, WildcardOptions.IgnoreCase);
            foreach (PSObject obj3 in objArray)
            {
                if ((obj3.Properties["name"] != null) && (obj3.Properties["name"].Value != null))
                {
                    string input = obj3.Properties["name"].Value.ToString();
                    if (pattern2.IsMatch(input))
                    {
                        list.Add(obj3);
                    }
                }
            }
            return list.ToArray();
        }

        internal override Uri GetUriForOnlineHelp()
        {
            Uri uriFromCommandInfo = null;
            UriFormatException exception = null;
            try
            {
                uriFromCommandInfo = GetUriFromCommandPSObject(this.FullHelp);
                if (uriFromCommandInfo != null)
                {
                    return uriFromCommandInfo;
                }
            }
            catch (UriFormatException exception2)
            {
                exception = exception2;
            }
            uriFromCommandInfo = this.LookupUriFromCommandInfo();
            if (uriFromCommandInfo != null)
            {
                return uriFromCommandInfo;
            }
            if (exception != null)
            {
                throw exception;
            }
            return base.GetUriForOnlineHelp();
        }

        internal static Uri GetUriFromCommandPSObject(PSObject commandFullHelp)
        {
            if (((commandFullHelp != null) && (commandFullHelp.Properties["relatedLinks"] != null)) && (commandFullHelp.Properties["relatedLinks"].Value != null))
            {
                PSObject obj2 = PSObject.AsPSObject(commandFullHelp.Properties["relatedLinks"].Value);
                if (obj2.Properties["navigationLink"] != null)
                {
                    object[] objArray = (object[]) LanguagePrimitives.ConvertTo(obj2.Properties["navigationLink"].Value, typeof(object[]), CultureInfo.InvariantCulture);
                    foreach (object obj3 in objArray)
                    {
                        if (obj3 != null)
                        {
                            PSNoteProperty property = PSObject.AsPSObject(obj3).Properties["uri"] as PSNoteProperty;
                            if (property != null)
                            {
                                string result = string.Empty;
                                LanguagePrimitives.TryConvertTo<string>(property.Value, CultureInfo.InvariantCulture, out result);
                                if (!string.IsNullOrEmpty(result))
                                {
                                    try
                                    {
                                        return new Uri(result);
                                    }
                                    catch (UriFormatException)
                                    {
                                        throw PSTraceSource.NewInvalidOperationException("HelpErrors", "InvalidURI", new object[] { result });
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return null;
        }

        internal Uri LookupUriFromCommandInfo()
        {
            CommandTypes cmdlet = CommandTypes.Cmdlet;
            switch (this.HelpCategory)
            {
                case System.Management.Automation.HelpCategory.Function:
                    cmdlet = CommandTypes.Function;
                    break;

                case System.Management.Automation.HelpCategory.Filter:
                    cmdlet = CommandTypes.Filter;
                    break;

                case System.Management.Automation.HelpCategory.ExternalScript:
                    cmdlet = CommandTypes.ExternalScript;
                    break;

                case System.Management.Automation.HelpCategory.Cmdlet:
                    cmdlet = CommandTypes.Cmdlet;
                    break;

                case System.Management.Automation.HelpCategory.ScriptCommand:
                    cmdlet = CommandTypes.Script;
                    break;

                default:
                    return null;
            }
            string name = this.Name;
            string result = string.Empty;
            if (this.FullHelp.Properties["ModuleName"] != null)
            {
                PSNoteProperty property = this.FullHelp.Properties["ModuleName"] as PSNoteProperty;
                if (property != null)
                {
                    LanguagePrimitives.TryConvertTo<string>(property.Value, CultureInfo.InvariantCulture, out result);
                }
            }
            string commandName = name;
            if (!string.IsNullOrEmpty(result))
            {
                commandName = string.Format(CultureInfo.InvariantCulture, @"{0}\{1}", new object[] { result, name });
            }
            ExecutionContext executionContextFromTLS = LocalPipeline.GetExecutionContextFromTLS();
            if (executionContextFromTLS != null)
            {
                try
                {
                    CommandInfo info = null;
                    if (cmdlet == CommandTypes.Cmdlet)
                    {
                        info = executionContextFromTLS.SessionState.InvokeCommand.GetCmdlet(commandName);
                    }
                    else
                    {
                        info = executionContextFromTLS.SessionState.InvokeCommand.GetCommands(commandName, cmdlet, false).FirstOrDefault<CommandInfo>();
                    }
                    if ((info == null) || (info.CommandMetadata == null))
                    {
                        return null;
                    }
                    if (!string.IsNullOrEmpty(info.CommandMetadata.HelpUri))
                    {
                        try
                        {
                            return new Uri(info.CommandMetadata.HelpUri);
                        }
                        catch (UriFormatException)
                        {
                            throw PSTraceSource.NewInvalidOperationException("HelpErrors", "InvalidURI", new object[] { info.CommandMetadata.HelpUri });
                        }
                    }
                }
                catch (CommandNotFoundException)
                {
                }
            }
            return null;
        }

        internal override bool MatchPatternInContent(WildcardPattern pattern)
        {
            string synopsis = this.Synopsis;
            string detailedDescription = this.DetailedDescription;
            if (synopsis == null)
            {
                synopsis = string.Empty;
            }
            if (detailedDescription == null)
            {
                detailedDescription = string.Empty;
            }
            if (!pattern.IsMatch(synopsis))
            {
                return pattern.IsMatch(detailedDescription);
            }
            return true;
        }

        internal string DetailedDescription
        {
            get
            {
                if (this.FullHelp == null)
                {
                    return "";
                }
                if ((this.FullHelp.Properties["Description"] == null) || (this.FullHelp.Properties["Description"].Value == null))
                {
                    return "";
                }
                object[] objArray = (object[]) LanguagePrimitives.ConvertTo(this.FullHelp.Properties["Description"].Value, typeof(object[]), CultureInfo.InvariantCulture);
                if ((objArray == null) || (objArray.Length == 0))
                {
                    return "";
                }
                StringBuilder builder = new StringBuilder(400);
                foreach (object obj2 in objArray)
                {
                    if (obj2 != null)
                    {
                        PSObject obj3 = PSObject.AsPSObject(obj2);
                        if (((obj3 != null) && (obj3.Properties["Text"] != null)) && (obj3.Properties["Text"].Value != null))
                        {
                            string str = obj3.Properties["Text"].Value.ToString();
                            builder.Append(str);
                            builder.Append(Environment.NewLine);
                        }
                    }
                }
                return builder.ToString().Trim();
            }
        }

        internal PSObject Details
        {
            get
            {
                if ((this.FullHelp != null) && ((this.FullHelp.Properties["Details"] != null) && (this.FullHelp.Properties["Details"].Value != null)))
                {
                    return PSObject.AsPSObject(this.FullHelp.Properties["Details"].Value);
                }
                return null;
            }
        }

        internal override System.Management.Automation.HelpCategory HelpCategory
        {
            get
            {
                return this._helpCategory;
            }
        }

        internal override string Name
        {
            get
            {
                PSObject details = this.Details;
                if (details == null)
                {
                    return "";
                }
                if ((details.Properties["Name"] == null) || (details.Properties["Name"].Value == null))
                {
                    return "";
                }
                string str = details.Properties["Name"].Value.ToString();
                if (str == null)
                {
                    return "";
                }
                return str.Trim();
            }
        }

        internal override string Synopsis
        {
            get
            {
                PSObject details = this.Details;
                if (details == null)
                {
                    return "";
                }
                if ((details.Properties["Description"] == null) || (details.Properties["Description"].Value == null))
                {
                    return "";
                }
                object[] objArray = (object[]) LanguagePrimitives.ConvertTo(details.Properties["Description"].Value, typeof(object[]), CultureInfo.InvariantCulture);
                if ((objArray == null) || (objArray.Length == 0))
                {
                    return "";
                }
                PSObject obj3 = (objArray[0] == null) ? null : PSObject.AsPSObject(objArray[0]);
                if (((obj3 == null) || (obj3.Properties["Text"] == null)) || (obj3.Properties["Text"].Value == null))
                {
                    return "";
                }
                string str = obj3.Properties["Text"].Value.ToString();
                if (str == null)
                {
                    return "";
                }
                return str.Trim();
            }
        }
    }
}

