namespace System.Management.Automation.Help
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.Management.Automation;
    using System.Management.Automation.Internal;
    using System.Management.Automation.Runspaces;
    using System.Runtime.InteropServices;
    using System.Text;

    internal class DefaultCommandHelpObjectBuilder
    {
        internal static string TypeNameForDefaultHelp = "ExtendedCmdletHelpInfo";

        private static void AddAliasesProperties(PSObject obj, string name, ExecutionContext context)
        {
            StringBuilder builder = new StringBuilder();
            bool flag = false;
            if (context != null)
            {
                foreach (string str in context.SessionState.Internal.GetAliasesByCommandName(name))
                {
                    flag = true;
                    builder.AppendLine(str);
                }
            }
            if (!flag)
            {
                builder.AppendLine(StringUtil.Format(HelpDisplayStrings.None, new object[0]));
            }
            obj.Properties.Add(new PSNoteProperty("aliases", builder.ToString()));
        }

        internal static void AddDetailsProperties(PSObject obj, string name, string noun, string verb, string typeNameForHelp, string synopsis = null)
        {
            PSObject obj2 = new PSObject();
            obj2.TypeNames.Clear();
            obj2.TypeNames.Add(string.Format(CultureInfo.InvariantCulture, "{0}#details", new object[] { typeNameForHelp }));
            obj2.Properties.Add(new PSNoteProperty("name", name));
            obj2.Properties.Add(new PSNoteProperty("noun", noun));
            obj2.Properties.Add(new PSNoteProperty("verb", verb));
            if (!string.IsNullOrEmpty(synopsis))
            {
                PSObject obj3 = new PSObject();
                obj3.TypeNames.Clear();
                obj3.TypeNames.Add("MamlParaTextItem");
                obj3.Properties.Add(new PSNoteProperty("Text", synopsis));
                obj2.Properties.Add(new PSNoteProperty("Description", obj3));
            }
            obj.Properties.Add(new PSNoteProperty("details", obj2));
        }

        internal static void AddInputTypesProperties(PSObject obj, Dictionary<string, ParameterMetadata> parameters)
        {
            Collection<string> collection = new Collection<string>();
            if (parameters != null)
            {
                foreach (KeyValuePair<string, ParameterMetadata> pair in parameters)
                {
                    foreach (ParameterAttribute attribute in GetParameterAttribute(pair.Value.Attributes))
                    {
                        if (((attribute.ValueFromPipeline || attribute.ValueFromPipelineByPropertyName) || attribute.ValueFromRemainingArguments) && !collection.Contains(pair.Value.ParameterType.FullName))
                        {
                            collection.Add(pair.Value.ParameterType.FullName);
                        }
                    }
                }
            }
            if (collection.Count == 0)
            {
                collection.Add(StringUtil.Format(HelpDisplayStrings.None, new object[0]));
            }
            StringBuilder builder = new StringBuilder();
            foreach (string str in collection)
            {
                builder.AppendLine(str);
            }
            PSObject obj2 = new PSObject();
            obj2.TypeNames.Clear();
            obj2.TypeNames.Add(string.Format(CultureInfo.InvariantCulture, "{0}#inputTypes", new object[] { TypeNameForDefaultHelp }));
            PSObject obj3 = new PSObject();
            obj3.TypeNames.Clear();
            obj3.TypeNames.Add(string.Format(CultureInfo.InvariantCulture, "{0}#inputType", new object[] { TypeNameForDefaultHelp }));
            PSObject obj4 = new PSObject();
            obj4.TypeNames.Clear();
            obj4.TypeNames.Add(string.Format(CultureInfo.InvariantCulture, "{0}#type", new object[] { TypeNameForDefaultHelp }));
            obj4.Properties.Add(new PSNoteProperty("name", builder.ToString()));
            obj3.Properties.Add(new PSNoteProperty("type", obj4));
            obj2.Properties.Add(new PSNoteProperty("inputType", obj3));
            obj.Properties.Add(new PSNoteProperty("inputTypes", obj2));
        }

        private static void AddOutputTypesProperties(PSObject obj, ReadOnlyCollection<PSTypeName> outputTypes)
        {
            PSObject obj2 = new PSObject();
            obj2.TypeNames.Clear();
            obj2.TypeNames.Add(string.Format(CultureInfo.InvariantCulture, "{0}#returnValues", new object[] { TypeNameForDefaultHelp }));
            PSObject obj3 = new PSObject();
            obj3.TypeNames.Clear();
            obj3.TypeNames.Add(string.Format(CultureInfo.InvariantCulture, "{0}#returnValue", new object[] { TypeNameForDefaultHelp }));
            PSObject obj4 = new PSObject();
            obj4.TypeNames.Clear();
            obj4.TypeNames.Add(string.Format(CultureInfo.InvariantCulture, "{0}#type", new object[] { TypeNameForDefaultHelp }));
            if (outputTypes.Count == 0)
            {
                obj4.Properties.Add(new PSNoteProperty("name", "System.Object"));
            }
            else
            {
                StringBuilder builder = new StringBuilder();
                foreach (PSTypeName name in outputTypes)
                {
                    builder.AppendLine(name.Name);
                }
                obj4.Properties.Add(new PSNoteProperty("name", builder.ToString()));
            }
            obj3.Properties.Add(new PSNoteProperty("type", obj4));
            obj2.Properties.Add(new PSNoteProperty("returnValue", obj3));
            obj.Properties.Add(new PSNoteProperty("returnValues", obj2));
        }

        private static void AddParameterProperties(PSObject obj, string name, Collection<string> aliases, bool dynamic, Type type, Collection<Attribute> attributes, string parameterSetName = null)
        {
            Collection<ParameterAttribute> parameterAttribute = GetParameterAttribute(attributes);
            obj.Properties.Add(new PSNoteProperty("name", name));
            if (parameterAttribute.Count == 0)
            {
                obj.Properties.Add(new PSNoteProperty("required", ""));
                obj.Properties.Add(new PSNoteProperty("pipelineInput", ""));
                obj.Properties.Add(new PSNoteProperty("isDynamic", ""));
                obj.Properties.Add(new PSNoteProperty("parameterSetName", ""));
                obj.Properties.Add(new PSNoteProperty("description", ""));
                obj.Properties.Add(new PSNoteProperty("position", ""));
                obj.Properties.Add(new PSNoteProperty("aliases", ""));
            }
            else
            {
                ParameterAttribute paramAttrib = parameterAttribute[0];
                if (!string.IsNullOrEmpty(parameterSetName))
                {
                    foreach (ParameterAttribute attribute2 in parameterAttribute)
                    {
                        if (string.Equals(attribute2.ParameterSetName, parameterSetName, StringComparison.OrdinalIgnoreCase))
                        {
                            paramAttrib = attribute2;
                            break;
                        }
                    }
                }
                obj.Properties.Add(new PSNoteProperty("required", paramAttrib.Mandatory.ToString().ToLower(CultureInfo.CurrentCulture)));
                obj.Properties.Add(new PSNoteProperty("pipelineInput", GetPipelineInputString(paramAttrib)));
                obj.Properties.Add(new PSNoteProperty("isDynamic", dynamic.ToString().ToLower(CultureInfo.CurrentCulture)));
                if (paramAttrib.ParameterSetName.Equals("__AllParameterSets", StringComparison.OrdinalIgnoreCase))
                {
                    obj.Properties.Add(new PSNoteProperty("parameterSetName", StringUtil.Format(HelpDisplayStrings.AllParameterSetsName, new object[0])));
                }
                else
                {
                    StringBuilder builder = new StringBuilder();
                    for (int i = 0; i < parameterAttribute.Count; i++)
                    {
                        builder.Append(parameterAttribute[i].ParameterSetName);
                        if (i != (parameterAttribute.Count - 1))
                        {
                            builder.Append(", ");
                        }
                    }
                    obj.Properties.Add(new PSNoteProperty("parameterSetName", builder.ToString()));
                }
                if (paramAttrib.HelpMessage != null)
                {
                    StringBuilder builder2 = new StringBuilder();
                    builder2.AppendLine(paramAttrib.HelpMessage);
                    obj.Properties.Add(new PSNoteProperty("description", builder2.ToString()));
                }
                if (type != typeof(SwitchParameter))
                {
                    AddParameterValueProperties(obj, type, attributes);
                }
                AddParameterTypeProperties(obj, type, attributes);
                if (paramAttrib.Position == -2147483648)
                {
                    obj.Properties.Add(new PSNoteProperty("position", StringUtil.Format(HelpDisplayStrings.NamedParameter, new object[0])));
                }
                else
                {
                    obj.Properties.Add(new PSNoteProperty("position", paramAttrib.Position.ToString(CultureInfo.InvariantCulture)));
                }
                if (aliases.Count == 0)
                {
                    obj.Properties.Add(new PSNoteProperty("aliases", StringUtil.Format(HelpDisplayStrings.None, new object[0])));
                }
                else
                {
                    StringBuilder builder3 = new StringBuilder();
                    for (int j = 0; j < aliases.Count; j++)
                    {
                        builder3.Append(aliases[j]);
                        if (j != (aliases.Count - 1))
                        {
                            builder3.Append(", ");
                        }
                    }
                    obj.Properties.Add(new PSNoteProperty("aliases", builder3.ToString()));
                }
            }
        }

        internal static void AddParametersProperties(PSObject obj, Dictionary<string, ParameterMetadata> parameters, bool common, bool commonWorkflow, string typeNameForHelp)
        {
            PSObject obj2 = new PSObject();
            obj2.TypeNames.Clear();
            obj2.TypeNames.Add(string.Format(CultureInfo.InvariantCulture, "{0}#parameters", new object[] { typeNameForHelp }));
            ArrayList list = new ArrayList();
            ArrayList list2 = new ArrayList();
            if (parameters != null)
            {
                foreach (KeyValuePair<string, ParameterMetadata> pair in parameters)
                {
                    list2.Add(pair.Key);
                }
            }
            list2.Sort(StringComparer.Ordinal);
            foreach (string str in list2)
            {
                if ((!commonWorkflow || !IsCommonWorkflowParameter(str)) && (!common || !IsCommonParameter(str)))
                {
                    PSObject obj3 = new PSObject();
                    obj3.TypeNames.Clear();
                    obj3.TypeNames.Add(string.Format(CultureInfo.InvariantCulture, "{0}#parameter", new object[] { TypeNameForDefaultHelp }));
                    AddParameterProperties(obj3, str, parameters[str].Aliases, parameters[str].IsDynamic, parameters[str].ParameterType, parameters[str].Attributes, null);
                    list.Add(obj3);
                }
            }
            obj2.Properties.Add(new PSNoteProperty("parameter", list.ToArray()));
            obj.Properties.Add(new PSNoteProperty("parameters", obj2));
        }

        private static void AddParameterTypeProperties(PSObject obj, Type parameterType, IEnumerable<Attribute> attributes)
        {
            PSObject obj2 = new PSObject();
            obj2.TypeNames.Clear();
            obj2.TypeNames.Add(string.Format(CultureInfo.InvariantCulture, "{0}#type", new object[] { TypeNameForDefaultHelp }));
            string parameterTypeString = CommandParameterSetInfo.GetParameterTypeString(parameterType, attributes);
            obj2.Properties.Add(new PSNoteProperty("name", parameterTypeString));
            obj.Properties.Add(new PSNoteProperty("type", obj2));
        }

        private static void AddParameterValueGroupProperties(PSObject obj, string[] values)
        {
            PSObject obj2 = new PSObject();
            obj2.TypeNames.Clear();
            obj2.TypeNames.Add(string.Format(CultureInfo.InvariantCulture, "{0}#parameterValueGroup", new object[] { TypeNameForDefaultHelp }));
            ArrayList list = new ArrayList(values);
            obj2.Properties.Add(new PSNoteProperty("parameterValue", list.ToArray()));
            obj.Properties.Add(new PSNoteProperty("parameterValueGroup", obj2));
        }

        private static void AddParameterValueProperties(PSObject obj, Type parameterType, IEnumerable<Attribute> attributes)
        {
            PSObject obj2;
            if (parameterType != null)
            {
                Nullable.GetUnderlyingType(parameterType);
                obj2 = new PSObject(string.Copy(CommandParameterSetInfo.GetParameterTypeString(parameterType, attributes)));
                obj2.Properties.Add(new PSNoteProperty("variableLength", parameterType.IsArray));
            }
            else
            {
                obj2 = new PSObject("System.Object");
                obj2.Properties.Add(new PSNoteProperty("variableLength", StringUtil.Format(HelpDisplayStrings.FalseShort, new object[0])));
            }
            obj2.Properties.Add(new PSNoteProperty("required", "true"));
            obj.Properties.Add(new PSNoteProperty("parameterValue", obj2));
        }

        internal static void AddRelatedLinksProperties(PSObject obj, string relatedLink)
        {
            if (!string.IsNullOrEmpty(relatedLink))
            {
                PSObject obj2 = new PSObject();
                obj2.TypeNames.Clear();
                obj2.TypeNames.Add(string.Format(CultureInfo.InvariantCulture, "{0}#navigationLinks", new object[] { TypeNameForDefaultHelp }));
                obj2.Properties.Add(new PSNoteProperty("uri", relatedLink));
                List<PSObject> list = new List<PSObject> {
                    obj2
                };
                PSNoteProperty property = obj.Properties["relatedLinks"] as PSNoteProperty;
                if ((property != null) && (property.Value != null))
                {
                    PSNoteProperty property2 = PSObject.AsPSObject(property.Value).Properties["navigationLink"] as PSNoteProperty;
                    if ((property2 != null) && (property2.Value != null))
                    {
                        PSObject item = property2.Value as PSObject;
                        if (item != null)
                        {
                            list.Add(item);
                        }
                        else
                        {
                            PSObject[] objArray = property2.Value as PSObject[];
                            if (objArray != null)
                            {
                                foreach (PSObject obj5 in objArray)
                                {
                                    list.Add(obj5);
                                }
                            }
                        }
                    }
                }
                PSObject obj6 = new PSObject();
                obj6.TypeNames.Clear();
                obj6.TypeNames.Add(string.Format(CultureInfo.InvariantCulture, "{0}#relatedLinks", new object[] { TypeNameForDefaultHelp }));
                obj6.Properties.Add(new PSNoteProperty("navigationLink", list.ToArray()));
                obj.Properties.Add(new PSNoteProperty("relatedLinks", obj6));
            }
        }

        private static void AddRemarksProperties(PSObject obj, string cmdletName, string helpUri)
        {
            if (string.IsNullOrEmpty(helpUri))
            {
                obj.Properties.Add(new PSNoteProperty("remarks", StringUtil.Format(HelpDisplayStrings.GetLatestHelpContentWithoutHelpUri, cmdletName)));
            }
            else
            {
                obj.Properties.Add(new PSNoteProperty("remarks", StringUtil.Format(HelpDisplayStrings.GetLatestHelpContent, cmdletName, helpUri)));
            }
        }

        private static void AddSyntaxItemProperties(PSObject obj, string cmdletName, ReadOnlyCollection<CommandParameterSetInfo> parameterSets, bool common, bool commonWorkflow, string typeNameForHelp)
        {
            ArrayList list = new ArrayList();
            foreach (CommandParameterSetInfo info in parameterSets)
            {
                PSObject obj2 = new PSObject();
                obj2.TypeNames.Clear();
                obj2.TypeNames.Add(string.Format(CultureInfo.InvariantCulture, "{0}#syntaxItem", new object[] { typeNameForHelp }));
                obj2.Properties.Add(new PSNoteProperty("name", cmdletName));
                obj2.Properties.Add(new PSNoteProperty("CommonParameters", common));
                obj2.Properties.Add(new PSNoteProperty("WorkflowCommonParameters", commonWorkflow));
                Collection<CommandParameterInfo> parameters = new Collection<CommandParameterInfo>();
                info.GenerateParametersInDisplayOrder(commonWorkflow, new Action<CommandParameterInfo>(parameters.Add), delegate (string param0) {
                });
                AddSyntaxParametersProperties(obj2, parameters, common, commonWorkflow, info.Name);
                list.Add(obj2);
            }
            obj.Properties.Add(new PSNoteProperty("syntaxItem", list.ToArray()));
        }

        private static void AddSyntaxParametersProperties(PSObject obj, IEnumerable<CommandParameterInfo> parameters, bool common, bool commonWorkflow, string parameterSetName)
        {
            ArrayList list = new ArrayList();
            foreach (CommandParameterInfo info in parameters)
            {
                if ((!commonWorkflow || !IsCommonWorkflowParameter(info.Name)) && (!common || !IsCommonParameter(info.Name)))
                {
                    PSObject obj2 = new PSObject();
                    obj2.TypeNames.Clear();
                    obj2.TypeNames.Add(string.Format(CultureInfo.InvariantCulture, "{0}#parameter", new object[] { TypeNameForDefaultHelp }));
                    Collection<Attribute> attributes = new Collection<Attribute>(info.Attributes);
                    AddParameterProperties(obj2, info.Name, new Collection<string>(info.Aliases), info.IsDynamic, info.ParameterType, attributes, parameterSetName);
                    Collection<ValidateSetAttribute> validateSetAttribute = GetValidateSetAttribute(attributes);
                    List<string> list2 = new List<string>();
                    foreach (ValidateSetAttribute attribute in validateSetAttribute)
                    {
                        foreach (string str in attribute.ValidValues)
                        {
                            list2.Add(str);
                        }
                    }
                    if (list2.Count != 0)
                    {
                        AddParameterValueGroupProperties(obj2, list2.ToArray());
                    }
                    else if (info.ParameterType.IsEnum && (info.ParameterType.GetEnumNames() != null))
                    {
                        AddParameterValueGroupProperties(obj2, info.ParameterType.GetEnumNames());
                    }
                    else if (info.ParameterType.IsArray)
                    {
                        if (info.ParameterType.GetElementType().IsEnum && (info.ParameterType.GetElementType().GetEnumNames() != null))
                        {
                            AddParameterValueGroupProperties(obj2, info.ParameterType.GetElementType().GetEnumNames());
                        }
                    }
                    else if (info.ParameterType.IsGenericType)
                    {
                        Type[] genericArguments = info.ParameterType.GetGenericArguments();
                        if (genericArguments.Length != 0)
                        {
                            Type type = genericArguments[0];
                            if (type.IsEnum && (type.GetEnumNames() != null))
                            {
                                AddParameterValueGroupProperties(obj2, type.GetEnumNames());
                            }
                            else if ((type.IsArray && type.GetElementType().IsEnum) && (type.GetElementType().GetEnumNames() != null))
                            {
                                AddParameterValueGroupProperties(obj2, type.GetElementType().GetEnumNames());
                            }
                        }
                    }
                    list.Add(obj2);
                }
            }
            obj.Properties.Add(new PSNoteProperty("parameter", list.ToArray()));
        }

        internal static void AddSyntaxProperties(PSObject obj, string cmdletName, ReadOnlyCollection<CommandParameterSetInfo> parameterSets, bool common, bool commonWorkflow, string typeNameForHelp)
        {
            PSObject obj2 = new PSObject();
            obj2.TypeNames.Clear();
            obj2.TypeNames.Add(string.Format(CultureInfo.InvariantCulture, "{0}#syntax", new object[] { typeNameForHelp }));
            AddSyntaxItemProperties(obj2, cmdletName, parameterSets, common, commonWorkflow, typeNameForHelp);
            obj.Properties.Add(new PSNoteProperty("Syntax", obj2));
        }

        private static Collection<ParameterAttribute> GetParameterAttribute(Collection<Attribute> attributes)
        {
            Collection<ParameterAttribute> collection = new Collection<ParameterAttribute>();
            foreach (Attribute attribute in attributes)
            {
                ParameterAttribute item = attribute as ParameterAttribute;
                if (item != null)
                {
                    collection.Add(item);
                }
            }
            return collection;
        }

        private static string GetPipelineInputString(ParameterAttribute paramAttrib)
        {
            ArrayList list = new ArrayList();
            if (paramAttrib.ValueFromPipeline)
            {
                list.Add(StringUtil.Format(HelpDisplayStrings.PipelineByValue, new object[0]));
            }
            if (paramAttrib.ValueFromPipelineByPropertyName)
            {
                list.Add(StringUtil.Format(HelpDisplayStrings.PipelineByPropertyName, new object[0]));
            }
            if (paramAttrib.ValueFromRemainingArguments)
            {
                list.Add(StringUtil.Format(HelpDisplayStrings.PipelineFromRemainingArguments, new object[0]));
            }
            if (list.Count == 0)
            {
                return StringUtil.Format(HelpDisplayStrings.FalseShort, new object[0]);
            }
            StringBuilder builder = new StringBuilder();
            builder.Append(StringUtil.Format(HelpDisplayStrings.TrueShort, new object[0]));
            builder.Append(" (");
            for (int i = 0; i < list.Count; i++)
            {
                builder.Append((string) list[i]);
                if (i != (list.Count - 1))
                {
                    builder.Append(", ");
                }
            }
            builder.Append(")");
            return builder.ToString();
        }

        internal static PSObject GetPSObjectFromCmdletInfo(CommandInfo input)
        {
            CommandInfo info = input.CreateGetCommandCopy(null);
            PSObject obj2 = new PSObject();
            obj2.TypeNames.Clear();
            obj2.TypeNames.Add(string.Format(CultureInfo.InvariantCulture, "{0}#{1}#command", new object[] { TypeNameForDefaultHelp, info.ModuleName }));
            obj2.TypeNames.Add(string.Format(CultureInfo.InvariantCulture, "{0}#{1}", new object[] { TypeNameForDefaultHelp, info.ModuleName }));
            obj2.TypeNames.Add(TypeNameForDefaultHelp);
            obj2.TypeNames.Add("CmdletHelpInfo");
            obj2.TypeNames.Add("HelpInfo");
            if (info is CmdletInfo)
            {
                CmdletInfo info2 = info as CmdletInfo;
                bool flag = false;
                bool flag2 = false;
                if (info2.Parameters != null)
                {
                    flag = HasCommonParameters(info2.Parameters);
                    flag2 = (info2.CommandType & CommandTypes.Workflow) == CommandTypes.Workflow;
                }
                obj2.Properties.Add(new PSNoteProperty("CommonParameters", flag));
                obj2.Properties.Add(new PSNoteProperty("WorkflowCommonParameters", flag2));
                AddDetailsProperties(obj2, info2.Name, info2.Noun, info2.Verb, TypeNameForDefaultHelp, null);
                AddSyntaxProperties(obj2, info2.Name, info2.ParameterSets, flag, flag2, TypeNameForDefaultHelp);
                AddParametersProperties(obj2, info2.Parameters, flag, flag2, TypeNameForDefaultHelp);
                AddInputTypesProperties(obj2, info2.Parameters);
                AddRelatedLinksProperties(obj2, info.CommandMetadata.HelpUri);
                try
                {
                    AddOutputTypesProperties(obj2, info2.OutputType);
                }
                catch (PSInvalidOperationException)
                {
                    AddOutputTypesProperties(obj2, new ReadOnlyCollection<PSTypeName>(new List<PSTypeName>()));
                }
                AddAliasesProperties(obj2, info2.Name, info2.Context);
                if (HasHelpInfoUri(info2.Module, info2.ModuleName))
                {
                    AddRemarksProperties(obj2, info2.Name, info2.CommandMetadata.HelpUri);
                }
                else
                {
                    obj2.Properties.Add(new PSNoteProperty("remarks", HelpDisplayStrings.None));
                }
                obj2.Properties.Add(new PSNoteProperty("PSSnapIn", info2.PSSnapIn));
            }
            else if (info is FunctionInfo)
            {
                FunctionInfo info3 = info as FunctionInfo;
                bool flag3 = HasCommonParameters(info3.Parameters);
                bool flag4 = (info.CommandType & CommandTypes.Workflow) == CommandTypes.Workflow;
                obj2.Properties.Add(new PSNoteProperty("CommonParameters", flag3));
                obj2.Properties.Add(new PSNoteProperty("WorkflowCommonParameters", flag4));
                AddDetailsProperties(obj2, info3.Name, string.Empty, string.Empty, TypeNameForDefaultHelp, null);
                AddSyntaxProperties(obj2, info3.Name, info3.ParameterSets, flag3, flag4, TypeNameForDefaultHelp);
                AddParametersProperties(obj2, info3.Parameters, flag3, flag4, TypeNameForDefaultHelp);
                AddInputTypesProperties(obj2, info3.Parameters);
                AddRelatedLinksProperties(obj2, info3.CommandMetadata.HelpUri);
                try
                {
                    AddOutputTypesProperties(obj2, info3.OutputType);
                }
                catch (PSInvalidOperationException)
                {
                    AddOutputTypesProperties(obj2, new ReadOnlyCollection<PSTypeName>(new List<PSTypeName>()));
                }
                AddAliasesProperties(obj2, info3.Name, info3.Context);
                if (HasHelpInfoUri(info3.Module, info3.ModuleName))
                {
                    AddRemarksProperties(obj2, info3.Name, info3.CommandMetadata.HelpUri);
                }
                else
                {
                    obj2.Properties.Add(new PSNoteProperty("remarks", HelpDisplayStrings.None));
                }
            }
            obj2.Properties.Add(new PSNoteProperty("alertSet", null));
            obj2.Properties.Add(new PSNoteProperty("description", null));
            obj2.Properties.Add(new PSNoteProperty("examples", null));
            obj2.Properties.Add(new PSNoteProperty("Synopsis", info.Syntax));
            obj2.Properties.Add(new PSNoteProperty("ModuleName", info.ModuleName));
            obj2.Properties.Add(new PSNoteProperty("nonTerminatingErrors", string.Empty));
            obj2.Properties.Add(new PSNoteProperty("xmlns:command", "http://schemas.microsoft.com/maml/dev/command/2004/10"));
            obj2.Properties.Add(new PSNoteProperty("xmlns:dev", "http://schemas.microsoft.com/maml/dev/2004/10"));
            obj2.Properties.Add(new PSNoteProperty("xmlns:maml", "http://schemas.microsoft.com/maml/2004/10"));
            return obj2;
        }

        private static Collection<ValidateSetAttribute> GetValidateSetAttribute(Collection<Attribute> attributes)
        {
            Collection<ValidateSetAttribute> collection = new Collection<ValidateSetAttribute>();
            foreach (Attribute attribute in attributes)
            {
                ValidateSetAttribute item = attribute as ValidateSetAttribute;
                if (item != null)
                {
                    collection.Add(item);
                }
            }
            return collection;
        }

        internal static bool HasCommonParameters(Dictionary<string, ParameterMetadata> parameters)
        {
            Collection<string> collection = new Collection<string>();
            foreach (KeyValuePair<string, ParameterMetadata> pair in parameters)
            {
                if (IsCommonParameter(pair.Value.Name))
                {
                    collection.Add(pair.Value.Name);
                }
            }
            return (collection.Count == CommonParameters.CommonCommandParameters.Length);
        }

        private static bool HasHelpInfoUri(PSModuleInfo module, string moduleName)
        {
            if (!string.IsNullOrEmpty(moduleName) && moduleName.Equals(InitialSessionState.CoreModule, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
            if (module == null)
            {
                return false;
            }
            return !string.IsNullOrEmpty(module.HelpInfoUri);
        }

        private static bool IsCommonParameter(string name)
        {
            foreach (string str in CommonParameters.CommonCommandParameters)
            {
                if (name == str)
                {
                    return true;
                }
            }
            return false;
        }

        private static bool IsCommonWorkflowParameter(string name)
        {
            foreach (string str in CommonParameters.CommonWorkflowParameters)
            {
                if (name == str)
                {
                    return true;
                }
            }
            return false;
        }
    }
}

