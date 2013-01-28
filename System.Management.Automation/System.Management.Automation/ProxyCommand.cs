namespace System.Management.Automation
{
    using System;
    using System.Text;

    public sealed class ProxyCommand
    {
        private ProxyCommand()
        {
        }

        private static void AppendContent(StringBuilder sb, string section, PSObject[] array)
        {
            if (array != null)
            {
                bool flag = true;
                foreach (PSObject obj2 in array)
                {
                    string objText = GetObjText(obj2);
                    if (!string.IsNullOrEmpty(objText))
                    {
                        if (flag)
                        {
                            flag = false;
                            sb.Append("\n\n");
                            sb.Append(section);
                            sb.Append("\n\n");
                        }
                        sb.Append(objText);
                        sb.Append("\n");
                    }
                }
                if (!flag)
                {
                    sb.Append("\n");
                }
            }
        }

        private static void AppendContent(StringBuilder sb, string section, object obj)
        {
            if (obj != null)
            {
                string objText = GetObjText(obj);
                if (!string.IsNullOrEmpty(objText))
                {
                    sb.Append("\n");
                    sb.Append(section);
                    sb.Append("\n\n");
                    sb.Append(objText);
                    sb.Append("\n");
                }
            }
        }

        private static void AppendType(StringBuilder sb, string section, PSObject parent)
        {
            PSObject property = GetProperty<PSObject>(parent, "type");
            PSObject obj3 = GetProperty<PSObject>(property, "name");
            if (obj3 != null)
            {
                sb.AppendFormat("\n\n{0}\n\n", section);
                sb.Append(GetObjText(obj3));
                sb.Append("\n");
            }
            else
            {
                PSObject obj4 = GetProperty<PSObject>(property, "uri");
                if (obj4 != null)
                {
                    sb.AppendFormat("\n\n{0}\n\n", section);
                    sb.Append(GetObjText(obj4));
                    sb.Append("\n");
                }
            }
        }

        public static string Create(CommandMetadata commandMetadata)
        {
            if (commandMetadata == null)
            {
                throw PSTraceSource.NewArgumentNullException("commandMetaData");
            }
            return commandMetadata.GetProxyCommand("");
        }

        public static string Create(CommandMetadata commandMetadata, string helpComment)
        {
            if (commandMetadata == null)
            {
                throw PSTraceSource.NewArgumentNullException("commandMetaData");
            }
            return commandMetadata.GetProxyCommand(helpComment);
        }

        public static string GetBegin(CommandMetadata commandMetadata)
        {
            if (commandMetadata == null)
            {
                throw PSTraceSource.NewArgumentNullException("commandMetaData");
            }
            return commandMetadata.GetBeginBlock();
        }

        public static string GetCmdletBindingAttribute(CommandMetadata commandMetadata)
        {
            if (commandMetadata == null)
            {
                throw PSTraceSource.NewArgumentNullException("commandMetaData");
            }
            return commandMetadata.GetDecl();
        }

        public static string GetEnd(CommandMetadata commandMetadata)
        {
            if (commandMetadata == null)
            {
                throw PSTraceSource.NewArgumentNullException("commandMetaData");
            }
            return commandMetadata.GetEndBlock();
        }

        public static string GetHelpComments(PSObject help)
        {
            if (help == null)
            {
                throw new ArgumentNullException("help");
            }
            bool flag = false;
            foreach (string str in help.InternalTypeNames)
            {
                if (str.Contains("HelpInfo"))
                {
                    flag = true;
                    break;
                }
            }
            if (!flag)
            {
                throw new InvalidOperationException(ProxyCommandStrings.HelpInfoObjectRequired);
            }
            StringBuilder sb = new StringBuilder();
            AppendContent(sb, ".SYNOPSIS", GetProperty<string>(help, "Synopsis"));
            AppendContent(sb, ".DESCRIPTION", GetProperty<PSObject[]>(help, "Description"));
            PSObject[] property = GetProperty<PSObject[]>(GetProperty<PSObject>(help, "Parameters"), "Parameter");
            if (property != null)
            {
                foreach (PSObject obj3 in property)
                {
                    PSObject obj4 = GetProperty<PSObject>(obj3, "Name");
                    PSObject[] objArray2 = GetProperty<PSObject[]>(obj3, "Description");
                    sb.AppendFormat("\n.PARAMETER {0}\n\n", obj4);
                    foreach (PSObject obj5 in objArray2)
                    {
                        string str3 = GetProperty<string>(obj5, "Text");
                        if (str3 == null)
                        {
                            str3 = obj5.ToString();
                        }
                        if (!string.IsNullOrEmpty(str3))
                        {
                            sb.Append(str3);
                            sb.Append("\n");
                        }
                    }
                }
            }
            PSObject[] objArray3 = GetProperty<PSObject[]>(GetProperty<PSObject>(help, "examples"), "example");
            if (objArray3 != null)
            {
                foreach (PSObject obj7 in objArray3)
                {
                    StringBuilder builder2 = new StringBuilder();
                    PSObject[] objArray4 = GetProperty<PSObject[]>(obj7, "introduction");
                    if (objArray4 != null)
                    {
                        foreach (PSObject obj8 in objArray4)
                        {
                            if (obj8 != null)
                            {
                                builder2.Append(GetObjText(obj8));
                            }
                        }
                    }
                    PSObject obj9 = GetProperty<PSObject>(obj7, "code");
                    if (obj9 != null)
                    {
                        builder2.Append(obj9.ToString());
                    }
                    PSObject[] objArray5 = GetProperty<PSObject[]>(obj7, "remarks");
                    if (objArray5 != null)
                    {
                        builder2.Append("\n");
                        foreach (PSObject obj10 in objArray5)
                        {
                            builder2.Append(GetProperty<string>(obj10, "text").ToString());
                        }
                    }
                    if (builder2.Length > 0)
                    {
                        sb.Append("\n\n.EXAMPLE\n\n");
                        sb.Append(builder2.ToString());
                    }
                }
            }
            PSObject obj11 = GetProperty<PSObject>(help, "alertSet");
            AppendContent(sb, ".NOTES", GetProperty<PSObject[]>(obj11, "alert"));
            PSObject parent = GetProperty<PSObject>(GetProperty<PSObject>(help, "inputTypes"), "inputType");
            AppendType(sb, ".INPUTS", parent);
            PSObject obj15 = GetProperty<PSObject>(GetProperty<PSObject>(help, "returnValues"), "returnValue");
            AppendType(sb, ".OUTPUTS", obj15);
            PSObject[] objArray6 = GetProperty<PSObject[]>(GetProperty<PSObject>(help, "relatedLinks"), "navigationLink");
            if (objArray6 != null)
            {
                foreach (PSObject obj17 in objArray6)
                {
                    AppendContent(sb, ".LINK", GetProperty<PSObject>(obj17, "uri"));
                    AppendContent(sb, ".LINK", GetProperty<PSObject>(obj17, "linkText"));
                }
            }
            AppendContent(sb, ".COMPONENT", GetProperty<PSObject>(help, "Component"));
            AppendContent(sb, ".ROLE", GetProperty<PSObject>(help, "Role"));
            AppendContent(sb, ".FUNCTIONALITY", GetProperty<PSObject>(help, "Functionality"));
            return sb.ToString();
        }

        private static string GetObjText(object obj)
        {
            string property = null;
            PSObject obj2 = obj as PSObject;
            if (obj2 != null)
            {
                property = GetProperty<string>(obj2, "Text");
            }
            if (property == null)
            {
                property = obj.ToString();
            }
            return property;
        }

        public static string GetParamBlock(CommandMetadata commandMetadata)
        {
            if (commandMetadata == null)
            {
                throw PSTraceSource.NewArgumentNullException("commandMetaData");
            }
            return commandMetadata.GetParamBlock();
        }

        public static string GetProcess(CommandMetadata commandMetadata)
        {
            if (commandMetadata == null)
            {
                throw PSTraceSource.NewArgumentNullException("commandMetaData");
            }
            return commandMetadata.GetProcessBlock();
        }

        private static T GetProperty<T>(PSObject obj, string property) where T: class
        {
            T local = default(T);
            if ((obj != null) && (obj.Properties[property] != null))
            {
                local = obj.Properties[property].Value as T;
            }
            return local;
        }
    }
}

