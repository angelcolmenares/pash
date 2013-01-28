namespace System.Management.Automation
{
    using Microsoft.PowerShell;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Management.Automation.Internal;
    using System.Runtime.CompilerServices;
    using System.Text;
    using System.Text.RegularExpressions;

    public class CommandParameterSetInfo
    {
        internal CommandParameterSetInfo(string name, bool isDefaultParameterSet, int parameterSetFlag, MergedCommandParameterMetadata parameterMetadata)
        {
            this.IsDefault = true;
            this.Name = string.Empty;
            if (string.IsNullOrEmpty(name))
            {
                throw PSTraceSource.NewArgumentException("name");
            }
            if (parameterMetadata == null)
            {
                throw PSTraceSource.NewArgumentNullException("parameterMetadata");
            }
            this.Name = name;
            this.IsDefault = isDefaultParameterSet;
            this.Initialize(parameterMetadata, parameterSetFlag);
        }

        private static void AppendFormatCommandParameterInfo(CommandParameterInfo parameter, ref StringBuilder result)
        {
            if (result.Length > 0)
            {
                result.Append(" ");
            }
            if (parameter.ParameterType == typeof(SwitchParameter))
            {
                result.AppendFormat(parameter.IsMandatory ? "-{0}" : "[-{0}]", parameter.Name);
            }
            else
            {
                string parameterTypeString = GetParameterTypeString(parameter.ParameterType, parameter.Attributes);
                if (parameter.IsMandatory)
                {
                    result.AppendFormat((parameter.Position != -2147483648) ? "[-{0}] <{1}>" : "-{0} <{1}>", parameter.Name, parameterTypeString);
                }
                else
                {
                    result.AppendFormat((parameter.Position != -2147483648) ? "[[-{0}] <{1}>]" : "[-{0} <{1}>]", parameter.Name, parameterTypeString);
                }
            }
        }

        internal void GenerateParametersInDisplayOrder(bool isCapabilityWorkflow, Action<CommandParameterInfo> parameterAction, Action<string> commonParameterAction)
        {
            List<CommandParameterInfo> list = new List<CommandParameterInfo>();
            List<CommandParameterInfo> list2 = new List<CommandParameterInfo>();
            List<CommandParameterInfo> list3 = new List<CommandParameterInfo>();
            foreach (CommandParameterInfo info in this.Parameters)
            {
                if (info.Position == -2147483648)
                {
                    if (info.IsMandatory)
                    {
                        list2.Add(info);
                    }
                    else
                    {
                        list3.Add(info);
                    }
                }
                else
                {
                    if (info.Position >= list.Count)
                    {
                        for (int i = list.Count; i <= info.Position; i++)
                        {
                            list.Add(null);
                        }
                    }
                    list[info.Position] = info;
                }
            }
            List<CommandParameterInfo> list4 = new List<CommandParameterInfo>();
            foreach (CommandParameterInfo info2 in list)
            {
                if (info2 != null)
                {
                    if (!CommonParameters.CommonWorkflowParameters.Contains<string>(info2.Name, StringComparer.OrdinalIgnoreCase) || !isCapabilityWorkflow)
                    {
                        parameterAction(info2);
                    }
                    else
                    {
                        list4.Add(info2);
                    }
                }
            }
            foreach (CommandParameterInfo info3 in list2)
            {
                if (info3 != null)
                {
                    parameterAction(info3);
                }
            }
            List<CommandParameterInfo> list5 = new List<CommandParameterInfo>();
            foreach (CommandParameterInfo info4 in list3)
            {
                if (info4 != null)
                {
                    if (!CommonParameters.CommonCommandParameters.Contains<string>(info4.Name, StringComparer.OrdinalIgnoreCase))
                    {
                        if (!CommonParameters.CommonWorkflowParameters.Contains<string>(info4.Name, StringComparer.OrdinalIgnoreCase) || !isCapabilityWorkflow)
                        {
                            parameterAction(info4);
                        }
                        else
                        {
                            list4.Add(info4);
                        }
                    }
                    else
                    {
                        list5.Add(info4);
                    }
                }
            }
            if (list4.Count == CommonParameters.CommonWorkflowParameters.Length)
            {
                commonParameterAction(HelpDisplayStrings.CommonWorkflowParameters);
            }
            else
            {
                foreach (CommandParameterInfo info5 in list4)
                {
                    parameterAction(info5);
                }
            }
            if (list5.Count == CommonParameters.CommonCommandParameters.Length)
            {
                commonParameterAction(HelpDisplayStrings.CommonParameters);
            }
            else
            {
                foreach (CommandParameterInfo info6 in list5)
                {
                    parameterAction(info6);
                }
            }
        }

        internal static string GetParameterTypeString(Type type, IEnumerable<Attribute> attributes)
        {
            PSTypeNameAttribute attribute;
            if ((attributes != null) && ((attribute = attributes.OfType<PSTypeNameAttribute>().FirstOrDefault<PSTypeNameAttribute>()) != null))
            {
                string pSTypeName;
                Match match = Regex.Match(attribute.PSTypeName, @"(.*\.)?(?<NetTypeName>.*)#(.*[/\\])?(?<CimClassName>.*)");
                if (match.Success)
                {
                    pSTypeName = match.Groups["NetTypeName"].Value + "#" + match.Groups["CimClassName"].Value;
                }
                else
                {
                    pSTypeName = attribute.PSTypeName;
                    int num = pSTypeName.LastIndexOfAny(new char[] { '.' });
                    if ((num != -1) && ((num + 1) < pSTypeName.Length))
                    {
                        pSTypeName = pSTypeName.Substring(num + 1);
                    }
                }
                if (type.IsArray && (pSTypeName.IndexOf("[]", StringComparison.OrdinalIgnoreCase) == -1))
                {
                    for (Type type2 = type; type2.IsArray; type2 = type2.GetElementType())
                    {
                        pSTypeName = pSTypeName + "[]";
                    }
                }
                return pSTypeName;
            }
            Type type3 = Nullable.GetUnderlyingType(type) ?? type;
            return ToStringCodeMethods.Type(type3, true);
        }

        private void Initialize(MergedCommandParameterMetadata parameterMetadata, int parameterSetFlag)
        {
            Collection<CommandParameterInfo> list = new Collection<CommandParameterInfo>();
            foreach (MergedCompiledCommandParameter parameter in parameterMetadata.GetParametersInParameterSet(parameterSetFlag))
            {
                if (parameter != null)
                {
                    list.Add(new CommandParameterInfo(parameter.Parameter, parameterSetFlag));
                }
            }
            this.Parameters = new ReadOnlyCollection<CommandParameterInfo>(list);
        }

        public override string ToString()
        {
            return this.ToString(false);
        }

        internal string ToString(bool isCapabilityWorkflow)
        {
            StringBuilder result = new StringBuilder();
            this.GenerateParametersInDisplayOrder(isCapabilityWorkflow, delegate (CommandParameterInfo parameter) {
                AppendFormatCommandParameterInfo(parameter, ref result);
            }, delegate (string str) {
                if (result.Length > 0)
                {
                    result.Append(" ");
                }
                result.AppendFormat("[{0}]", str);
            });
            return result.ToString();
        }

        public bool IsDefault { get; private set; }

        public string Name { get; private set; }

        public ReadOnlyCollection<CommandParameterInfo> Parameters { get; private set; }
    }
}

