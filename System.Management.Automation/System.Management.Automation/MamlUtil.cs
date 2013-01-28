namespace System.Management.Automation
{
    using System;
    using System.Collections.Generic;
    using System.Management.Automation.Help;

    internal class MamlUtil
    {
        internal static void AddCommonProperties(PSObject maml1, PSObject maml2)
        {
            if (maml1.Properties["PSSnapIn"] == null)
            {
                PSPropertyInfo info = maml2.Properties["PSSnapIn"];
                if (info != null)
                {
                    maml1.Properties.Add(new PSNoteProperty("PSSnapIn", info.Value));
                }
            }
            if (maml1.Properties["ModuleName"] == null)
            {
                PSPropertyInfo info2 = maml2.Properties["ModuleName"];
                if (info2 != null)
                {
                    maml1.Properties.Add(new PSNoteProperty("ModuleName", info2.Value));
                }
            }
        }

        internal static void EnsurePropertyInfoPathExists(PSObject psObject, string[] path)
        {
            if (path.Length > 0)
            {
                for (int i = 0; i < path.Length; i++)
                {
                    string name = path[i];
                    PSPropertyInfo member = psObject.Properties[name];
                    if (member == null)
                    {
                        object obj2 = (i < (path.Length - 1)) ? new PSObject() : null;
                        member = new PSNoteProperty(name, obj2);
                        psObject.Properties.Add(member);
                    }
                    if (i == (path.Length - 1))
                    {
                        return;
                    }
                    if ((member.Value == null) || !(member.Value is PSObject))
                    {
                        member.Value = new PSObject();
                    }
                    psObject = (PSObject) member.Value;
                }
            }
        }

        internal static PSPropertyInfo GetProperyInfo(PSObject psObject, string[] path)
        {
            if (path.Length > 0)
            {
                for (int i = 0; i < path.Length; i++)
                {
                    string str = path[i];
                    PSPropertyInfo info = psObject.Properties[str];
                    if (i == (path.Length - 1))
                    {
                        return info;
                    }
                    if ((info == null) || !(info.Value is PSObject))
                    {
                        return null;
                    }
                    psObject = (PSObject) info.Value;
                }
            }
            return null;
        }

        internal static void OverrideName(PSObject maml1, PSObject maml2)
        {
            PrependPropertyValue(maml1, maml2, new string[] { "Name" }, true);
            PrependPropertyValue(maml1, maml2, new string[] { "Details", "Name" }, true);
        }

        internal static void OverrideParameters(PSObject maml1, PSObject maml2)
        {
            string[] path = new string[] { "Parameters", "Parameter" };
            List<object> list = new List<object>();
            PSPropertyInfo properyInfo = GetProperyInfo(maml2, path);
            Array array = properyInfo.Value as Array;
            if (array != null)
            {
                list.AddRange(array as IEnumerable<object>);
            }
            else
            {
                list.Add(PSObject.AsPSObject(properyInfo.Value));
            }
            EnsurePropertyInfoPathExists(maml1, path);
            PSPropertyInfo info2 = GetProperyInfo(maml1, path);
            List<object> list2 = new List<object>();
            array = info2.Value as Array;
            if (array != null)
            {
                list2.AddRange(array as IEnumerable<object>);
            }
            else
            {
                list2.Add(PSObject.AsPSObject(info2.Value));
            }
            for (int i = 0; i < list.Count; i++)
            {
                PSObject obj2 = PSObject.AsPSObject(list[i]);
                string result = "";
                PSPropertyInfo info3 = obj2.Properties["Name"];
                if ((info3 == null) || LanguagePrimitives.TryConvertTo<string>(info3.Value, out result))
                {
                    bool flag = false;
                    foreach (PSObject obj3 in list2)
                    {
                        string str2 = "";
                        PSPropertyInfo info4 = obj3.Properties["Name"];
                        if (((info4 == null) || LanguagePrimitives.TryConvertTo<string>(info4.Value, out str2)) && str2.Equals(result, StringComparison.OrdinalIgnoreCase))
                        {
                            flag = true;
                        }
                    }
                    if (!flag)
                    {
                        list2.Add(list[i]);
                    }
                }
            }
            if (list2.Count == 1)
            {
                info2.Value = list2[0];
            }
            else if (list2.Count >= 2)
            {
                info2.Value = list2.ToArray();
            }
        }

        internal static void OverridePSTypeNames(PSObject maml1, PSObject maml2)
        {
            foreach (string str in maml2.TypeNames)
            {
                if (str.StartsWith(DefaultCommandHelpObjectBuilder.TypeNameForDefaultHelp, StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }
            }
            maml1.TypeNames.Clear();
            foreach (string str2 in maml2.TypeNames)
            {
                maml1.TypeNames.Add(str2);
            }
        }

        internal static void PrependDetailedDescription(PSObject maml1, PSObject maml2)
        {
            PrependPropertyValue(maml1, maml2, new string[] { "Description" }, false);
        }

        internal static void PrependNotes(PSObject maml1, PSObject maml2)
        {
            PrependPropertyValue(maml1, maml2, new string[] { "AlertSet", "Alert" }, false);
        }

        internal static void PrependPropertyValue(PSObject maml1, PSObject maml2, string[] path, bool shouldOverride)
        {
            List<object> list = new List<object>();
            PSPropertyInfo properyInfo = GetProperyInfo(maml2, path);
            if (properyInfo != null)
            {
                if (properyInfo.Value is Array)
                {
                    list.AddRange(properyInfo.Value as IEnumerable<object>);
                }
                else
                {
                    list.Add(properyInfo.Value);
                }
            }
            EnsurePropertyInfoPathExists(maml1, path);
            PSPropertyInfo info2 = GetProperyInfo(maml1, path);
            if (info2 != null)
            {
                if (!shouldOverride)
                {
                    if (info2.Value is Array)
                    {
                        list.AddRange(info2.Value as IEnumerable<object>);
                    }
                    else
                    {
                        list.Add(info2.Value);
                    }
                }
                if (list.Count == 1)
                {
                    info2.Value = list[0];
                }
                else if (list.Count >= 2)
                {
                    info2.Value = list.ToArray();
                }
            }
        }

        internal static void PrependSyntax(PSObject maml1, PSObject maml2)
        {
            PrependPropertyValue(maml1, maml2, new string[] { "Syntax", "SyntaxItem" }, false);
        }
    }
}

