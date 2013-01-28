namespace Microsoft.PowerShell.Cmdletization
{
    using Microsoft.PowerShell.Cmdletization.Xml;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Management.Automation;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Xml;
    using System.Xml.Schema;
    using System.Xml.Serialization;

    internal sealed class ScriptWriter
    {
        private static object _enumCompilationLock = new object();
        private Dictionary<CommonMethodMetadata, int> _staticMethodMetadataToUniqueId = new Dictionary<CommonMethodMetadata, int>();
        private readonly List<string> aliasesToExport = new List<string>();
        private const string CmdletBeginBlockTemplate = "\r\nfunction {0}\r\n{{\r\n    {1}\r\n    {2}\r\n    param(\r\n    {3})\r\n\r\n    DynamicParam {{\r\n        try \r\n        {{\r\n            if (-not $__cmdletization_exceptionHasBeenThrown)\r\n            {{\r\n                $__cmdletization_objectModelWrapper = Microsoft.PowerShell.Utility\\New-Object $script:ObjectModelWrapper\r\n                $__cmdletization_objectModelWrapper.Initialize($PSCmdlet, $script:ClassName, $script:ClassVersion, $script:ModuleVersion, $script:PrivateData)\r\n\r\n                if ($__cmdletization_objectModelWrapper -is [System.Management.Automation.IDynamicParameters])\r\n                {{\r\n                    ([System.Management.Automation.IDynamicParameters]$__cmdletization_objectModelWrapper).GetDynamicParameters()\r\n                }}\r\n            }}\r\n        }}\r\n        catch\r\n        {{\r\n            $__cmdletization_exceptionHasBeenThrown = $true\r\n            throw\r\n        }}\r\n    }}\r\n\r\n    Begin {{\r\n        $__cmdletization_exceptionHasBeenThrown = $false\r\n        try \r\n        {{\r\n            __cmdletization_BindCommonParameters $__cmdletization_objectModelWrapper $PSBoundParameters\r\n            $__cmdletization_objectModelWrapper.BeginProcessing()\r\n        }}\r\n        catch\r\n        {{\r\n            $__cmdletization_exceptionHasBeenThrown = $true\r\n            throw\r\n        }}\r\n    }}\r\n        ";
        private const string CmdletEndBlockTemplate = "\r\n    End {{\r\n        try\r\n        {{\r\n            if (-not $__cmdletization_exceptionHasBeenThrown)\r\n            {{\r\n                $__cmdletization_objectModelWrapper.EndProcessing()\r\n            }}\r\n        }}\r\n        catch\r\n        {{\r\n            throw\r\n        }}\r\n    }}\r\n\r\n    {0}\r\n}}\r\nMicrosoft.PowerShell.Core\\Export-ModuleMember -Function '{1}'\r\n        ";
        private readonly PowerShellMetadata cmdletizationMetadata;
        private const string CmdletProcessBlockTemplate = "\r\n    Process {{\r\n        try \r\n        {{\r\n            if (-not $__cmdletization_exceptionHasBeenThrown)\r\n            {{\r\n{0}\r\n            }}\r\n        }}\r\n        catch\r\n        {{\r\n            $__cmdletization_exceptionHasBeenThrown = $true\r\n            throw\r\n        }}\r\n    }}\r\n        ";
        private readonly List<string> functionsToExport = new List<string>();
        private readonly GenerationOptions generationOptions;
        private const string HeaderTemplate = "\r\n#requires -version 3.0\r\n\r\nif ($(Microsoft.PowerShell.Core\\Get-Command Set-StrictMode -Module Microsoft.PowerShell.Core)) {{ Microsoft.PowerShell.Core\\Set-StrictMode -Off }}\r\n\r\n$script:MyModule = $MyInvocation.MyCommand.ScriptBlock.Module\r\n\r\n$script:ClassName = '{0}'\r\n$script:ClassVersion = '{1}'\r\n$script:ModuleVersion = '{2}'\r\n$script:ObjectModelWrapper = '{3}'\r\n\r\n$script:PrivateData = Microsoft.PowerShell.Utility\\New-Object 'System.Collections.Generic.Dictionary[string,string]'\r\n\r\nMicrosoft.PowerShell.Core\\Export-ModuleMember -Function @()\r\n        ";
        private const string InputObjectQueryParameterSetName = "InputObject (cdxml)";
        private const string InstanceCommonParameterSetTemplate = "{1}";
        private const string InstanceMethodParameterSetTemplate = "{2}";
        private const string InstanceQueryParameterSetTemplate = "{0}";
        private readonly InvocationInfo invocationInfo;
        private readonly string moduleName;
        private readonly Type objectInstanceType;
        private readonly Type objectModelWrapper;
        internal const string PrivateDataKey_ClassName = "ClassName";
        internal const string PrivateDataKey_CmdletsOverObjects = "CmdletsOverObjects";
        internal const string PrivateDataKey_DefaultSession = "DefaultSession";
        internal const string PrivateDataKey_ObjectModelWrapper = "CmdletAdapter";
        private const string SingleQueryParameterSetName = "Query (cdxml)";
        private const string StaticCommonParameterSetTemplate = "{1}";
        private const string StaticMethodParameterSetTemplate = "{0}";
        private static readonly XmlReaderSettings xmlReaderSettings = new XmlReaderSettings();

        static ScriptWriter()
        {
            xmlReaderSettings.CheckCharacters = true;
            xmlReaderSettings.CloseInput = false;
            xmlReaderSettings.ConformanceLevel = ConformanceLevel.Document;
            xmlReaderSettings.IgnoreComments = true;
            xmlReaderSettings.IgnoreProcessingInstructions = true;
            xmlReaderSettings.IgnoreWhitespace = false;
            xmlReaderSettings.MaxCharactersFromEntities = 0x4000L;
            xmlReaderSettings.MaxCharactersInDocument = 0x8000000L;
            xmlReaderSettings.DtdProcessing = DtdProcessing.Parse;
            xmlReaderSettings.XmlResolver = null;
            xmlReaderSettings.ValidationFlags = XmlSchemaValidationFlags.ProcessIdentityConstraints | XmlSchemaValidationFlags.ReportValidationWarnings;
            xmlReaderSettings.ValidationType = ValidationType.Schema;
            XmlReader schemaDocument = XmlReader.Create(new StringReader(CmdletizationCoreResources.Xml_cmdletsOverObjectsXsd), xmlReaderSettings);
            xmlReaderSettings.Schemas = new XmlSchemaSet();
            xmlReaderSettings.Schemas.Add(null, schemaDocument);
            xmlReaderSettings.Schemas.XmlResolver = null;
        }

        internal ScriptWriter(TextReader cmdletizationXmlReader, string moduleName, string defaultObjectModelWrapper, InvocationInfo invocationInfo, GenerationOptions generationOptions)
        {
            XmlReader xmlReader = XmlReader.Create(cmdletizationXmlReader, xmlReaderSettings);
            try
            {
                XmlSerializer serializer = new PowerShellMetadataSerializer();
                this.cmdletizationMetadata = (PowerShellMetadata) serializer.Deserialize(xmlReader);
            }
            catch (InvalidOperationException exception)
            {
                XmlSchemaException innerException = exception.InnerException as XmlSchemaException;
                if (innerException != null)
                {
                    throw new XmlException(innerException.Message, innerException, innerException.LineNumber, innerException.LinePosition);
                }
                XmlException exception3 = exception.InnerException as XmlException;
                if (exception3 != null)
                {
                    throw exception3;
                }
                if (exception.InnerException != null)
                {
                    throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, CmdletizationCoreResources.ScriptWriter_ConcatenationOfDeserializationExceptions, new object[] { exception.Message, exception.InnerException.Message }), exception.InnerException);
                }
                throw;
            }
            string valueToConvert = this.cmdletizationMetadata.Class.CmdletAdapter ?? defaultObjectModelWrapper;
            this.objectModelWrapper = (Type) LanguagePrimitives.ConvertTo(valueToConvert, typeof(Type), CultureInfo.InvariantCulture);
            if (this.objectModelWrapper.IsGenericType)
            {
                throw new XmlException(string.Format(CultureInfo.CurrentCulture, CmdletizationCoreResources.ScriptWriter_ObjectModelWrapperIsStillGeneric, new object[] { valueToConvert }));
            }
            Type objectModelWrapper = this.objectModelWrapper;
            while (!objectModelWrapper.IsGenericType || (objectModelWrapper.GetGenericTypeDefinition() != typeof(CmdletAdapter<>)))
            {
                objectModelWrapper = objectModelWrapper.BaseType;
                if (objectModelWrapper.Equals(typeof(object)))
                {
                    throw new XmlException(string.Format(CultureInfo.CurrentCulture, CmdletizationCoreResources.ScriptWriter_ObjectModelWrapperNotDerivedFromObjectModelWrapper, new object[] { valueToConvert, typeof(CmdletAdapter<>).FullName }));
                }
            }
            this.objectInstanceType = objectModelWrapper.GetGenericArguments()[0];
            this.moduleName = moduleName;
            this.invocationInfo = invocationInfo;
            this.generationOptions = generationOptions;
        }

        private static void AddPassThruParameter(IDictionary<string, ParameterMetadata> commonParameters, InstanceCmdletMetadata instanceCmdletMetadata)
        {
            bool flag = false;
            if (instanceCmdletMetadata.Method.Parameters != null)
            {
                foreach (InstanceMethodParameterMetadata metadata in instanceCmdletMetadata.Method.Parameters)
                {
                    if ((metadata.CmdletOutputMetadata != null) && (metadata.CmdletOutputMetadata.ErrorCode == null))
                    {
                        flag = true;
                        break;
                    }
                }
            }
            if (((instanceCmdletMetadata.Method.ReturnValue != null) && (instanceCmdletMetadata.Method.ReturnValue.CmdletOutputMetadata != null)) && (instanceCmdletMetadata.Method.ReturnValue.CmdletOutputMetadata.ErrorCode == null))
            {
                flag = true;
            }
            if (!flag)
            {
                ParameterMetadata metadata2 = new ParameterMetadata("PassThru", typeof(SwitchParameter));
                metadata2.ParameterSets.Clear();
                ParameterSetMetadata metadata3 = new ParameterSetMetadata(-2147483648, 0, null);
                metadata2.ParameterSets.Add("__AllParameterSets", metadata3);
                commonParameters.Add(metadata2.Name, metadata2);
            }
        }

        private static void CompileEnum(EnumMetadataEnum enumMetadata)
        {
            try
            {
                string enumFullName = EnumWriter.GetEnumFullName(enumMetadata);
                lock (_enumCompilationLock)
                {
                    Type type;
                    if (!LanguagePrimitives.TryConvertTo<Type>(enumFullName, CultureInfo.InvariantCulture, out type))
                    {
                        EnumWriter.Compile(enumMetadata);
                    }
                }
            }
            catch (Exception exception)
            {
                throw new XmlException(string.Format(CultureInfo.InvariantCulture, CmdletizationCoreResources.ScriptWriter_InvalidEnum, new object[] { enumMetadata.EnumName, exception.Message }), exception);
            }
        }

        private static void EnsureOrderOfPositionalParameters(Dictionary<string, ParameterMetadata> beforeParameters, Dictionary<string, ParameterMetadata> afterParameters)
        {
            int num = -2147483648;
            foreach (ParameterMetadata metadata in beforeParameters.Values)
            {
                foreach (ParameterSetMetadata metadata2 in metadata.ParameterSets.Values)
                {
                    num = Math.Max(metadata2.Position, num);
                }
            }
            int num2 = 0x7fffffff;
            foreach (ParameterMetadata metadata3 in afterParameters.Values)
            {
                foreach (ParameterSetMetadata metadata4 in metadata3.ParameterSets.Values)
                {
                    if (metadata4.Position != -2147483648)
                    {
                        num2 = Math.Min(metadata4.Position, num2);
                    }
                }
            }
            if ((num >= 0) && (num2 <= num))
            {
                int num3 = 0x3e9 - (num2 % 0x3e8);
                foreach (ParameterMetadata metadata5 in afterParameters.Values)
                {
                    foreach (ParameterSetMetadata metadata6 in metadata5.ParameterSets.Values)
                    {
                        if (metadata6.Position != -2147483648)
                        {
                            metadata6.Position += num3;
                        }
                    }
                }
            }
        }

        private static string EscapeModuleNameForHelpComment(string name)
        {
            StringBuilder builder = new StringBuilder(name.Length);
            foreach (char ch in name)
            {
                if ((("\"'`$#".IndexOf(ch) == -1) && !char.IsControl(ch)) && !char.IsWhiteSpace(ch))
                {
                    builder.Append(ch);
                }
            }
            return builder.ToString();
        }

        private ParameterMetadata GenerateAssociationClause(IEnumerable<string> commonParameterSets, IEnumerable<string> queryParameterSets, IEnumerable<string> methodParameterSets, Association associationMetadata, AssociationAssociatedInstance associatedInstanceMetadata, TextWriter output)
        {
            ParameterMetadata cmdletParameterMetadata = this.GetParameter(queryParameterSets, associationMetadata.SourceRole, associatedInstanceMetadata.Type, associatedInstanceMetadata.CmdletParameterMetadata);
            cmdletParameterMetadata.Attributes.Add(new ValidateNotNullAttribute());
            this.GenerateIfBoundParameter(commonParameterSets, methodParameterSets, cmdletParameterMetadata, output);
            output.WriteLine("    $__cmdletization_queryBuilder.FilterByAssociatedInstance(${{{0}}}, '{1}', '{2}', '{3}', '{4}')", new object[] { CommandMetadata.EscapeVariableName(cmdletParameterMetadata.Name), CommandMetadata.EscapeSingleQuotedString(associationMetadata.Association1), CommandMetadata.EscapeSingleQuotedString(associationMetadata.SourceRole), CommandMetadata.EscapeSingleQuotedString(associationMetadata.ResultRole), GetBehaviorWhenNoMatchesFound(associatedInstanceMetadata.CmdletParameterMetadata) });
            output.WriteLine("    }");
            return cmdletParameterMetadata;
        }

        private void GenerateIfBoundParameter(IEnumerable<string> commonParameterSets, IEnumerable<string> methodParameterSets, ParameterMetadata cmdletParameterMetadata, TextWriter output)
        {
            output.Write("    if ($PSBoundParameters.ContainsKey('{0}') -and (@(", CommandMetadata.EscapeSingleQuotedString(cmdletParameterMetadata.Name));
            bool flag = true;
            foreach (string str in cmdletParameterMetadata.ParameterSets.Keys)
            {
                foreach (string str2 in MultiplyParameterSets(str, "{0}", new IEnumerable<string>[] { commonParameterSets, methodParameterSets }))
                {
                    if (!flag)
                    {
                        output.Write(", ");
                    }
                    flag = false;
                    output.Write("'{0}'", CommandMetadata.EscapeSingleQuotedString(str2));
                }
            }
            output.WriteLine(") -contains $PSCmdlet.ParameterSetName )) {");
        }

        private void GenerateMethodParametersProcessing(StaticCmdletMetadata staticCmdlet, IEnumerable<string> commonParameterSets, out string scriptCode, out Dictionary<string, ParameterMetadata> methodParameters, out string outputTypeAttributeDeclaration)
        {
            methodParameters = new Dictionary<string, ParameterMetadata>(StringComparer.OrdinalIgnoreCase);
            StringBuilder builder = new StringBuilder();
            StringWriter output = new StringWriter(CultureInfo.InvariantCulture);
            output.WriteLine(@"      $__cmdletization_methodParameters = Microsoft.PowerShell.Utility\New-Object 'System.Collections.Generic.List[Microsoft.PowerShell.Cmdletization.MethodParameter]'");
            output.WriteLine();
            bool flag = staticCmdlet.Method.Length > 1;
            if (flag)
            {
                output.WriteLine("      switch -exact ($PSCmdlet.ParameterSetName) { ");
            }
            foreach (StaticMethodMetadata metadata in staticCmdlet.Method)
            {
                if (flag)
                {
                    output.Write("        { @(");
                    bool flag2 = true;
                    foreach (string str in MultiplyParameterSets(this.GetMethodParameterSet(metadata), "{0}", new IEnumerable<string>[] { commonParameterSets }))
                    {
                        if (!flag2)
                        {
                            output.Write(", ");
                        }
                        flag2 = false;
                        output.Write("'{0}'", CommandMetadata.EscapeSingleQuotedString(str));
                    }
                    output.WriteLine(") -contains $_ } {");
                }
                List<Type> list = new List<Type>();
                List<string> list2 = new List<string>();
                if (metadata.Parameters != null)
                {
                    foreach (StaticMethodParameterMetadata metadata2 in metadata.Parameters)
                    {
                        MethodParameterBindings bindings;
                        string cmdletParameterName = null;
                        if (metadata2.CmdletParameterMetadata != null)
                        {
                            ParameterMetadata metadata4;
                            string methodParameterSet = this.GetMethodParameterSet(metadata);
                            ParameterMetadata metadata3 = this.GetParameter(methodParameterSet, metadata2.ParameterName, metadata2.Type, metadata2.CmdletParameterMetadata);
                            cmdletParameterName = metadata3.Name;
                            if (methodParameters.TryGetValue(metadata3.Name, out metadata4))
                            {
                                try
                                {
                                    metadata4.ParameterSets.Add(methodParameterSet, metadata3.ParameterSets[methodParameterSet]);
                                    goto Label_01D8;
                                }
                                catch (ArgumentException exception)
                                {
                                    throw new XmlException(string.Format(CultureInfo.InvariantCulture, CmdletizationCoreResources.ScriptWriter_DuplicateQueryParameterName, new object[] { "<StaticCmdlets>...<Cmdlet>...<Method>", metadata3.Name }), exception);
                                }
                            }
                            methodParameters.Add(metadata3.Name, metadata3);
                        }
                    Label_01D8:
                        bindings = GetMethodParameterKind(metadata2);
                        Type dotNetType = this.GetDotNetType(metadata2.Type);
                        GenerateSingleMethodParameterProcessing(output, "        ", cmdletParameterName, dotNetType, metadata2.Type.ETSType, metadata2.DefaultValue, metadata2.ParameterName, bindings);
                        if (MethodParameterBindings.Out == (bindings & MethodParameterBindings.Out))
                        {
                            list.Add(dotNetType);
                            list2.Add(metadata2.Type.ETSType);
                        }
                    }
                }
                if (metadata.ReturnValue != null)
                {
                    MethodParameterBindings methodParameterKind = GetMethodParameterKind(metadata.ReturnValue);
                    Type item = this.GetDotNetType(metadata.ReturnValue.Type);
                    output.WriteLine(@"      $__cmdletization_returnValue = Microsoft.PowerShell.Utility\New-Object Microsoft.PowerShell.Cmdletization.MethodParameter -Property @{{ Name = 'ReturnValue'; ParameterType = '{0}'; Bindings = '{1}'; Value = $null; IsValuePresent = $false }}", CommandMetadata.EscapeSingleQuotedString(item.FullName), CommandMetadata.EscapeSingleQuotedString(methodParameterKind.ToString()));
                    if (!string.IsNullOrEmpty(metadata.ReturnValue.Type.ETSType))
                    {
                        output.WriteLine("      $__cmdletization_methodParameter.ParameterTypeName = '{0}'", CommandMetadata.EscapeSingleQuotedString(metadata.ReturnValue.Type.ETSType));
                    }
                    if (MethodParameterBindings.Out == (methodParameterKind & MethodParameterBindings.Out))
                    {
                        list.Add(item);
                        list2.Add(metadata.ReturnValue.Type.ETSType);
                    }
                }
                else
                {
                    output.WriteLine("      $__cmdletization_returnValue = $null");
                }
                output.WriteLine(@"      $__cmdletization_methodInvocationInfo = Microsoft.PowerShell.Utility\New-Object Microsoft.PowerShell.Cmdletization.MethodInvocationInfo @('{0}', $__cmdletization_methodParameters, $__cmdletization_returnValue)", CommandMetadata.EscapeSingleQuotedString(metadata.MethodName));
                output.WriteLine("      $__cmdletization_objectModelWrapper.ProcessRecord($__cmdletization_methodInvocationInfo)");
                if (flag)
                {
                    output.WriteLine("        }");
                }
                if (list.Count == 1)
                {
                    builder.AppendFormat(CultureInfo.InvariantCulture, "[OutputType([{0}])]", new object[] { list[0].FullName });
                    if ((list2.Count == 1) && !string.IsNullOrEmpty(list2[0]))
                    {
                        builder.AppendFormat(CultureInfo.InvariantCulture, "[OutputType('{0}')]", new object[] { CommandMetadata.EscapeSingleQuotedString(list2[0]) });
                    }
                }
            }
            if (flag)
            {
                output.WriteLine("    }");
            }
            scriptCode = output.ToString();
            outputTypeAttributeDeclaration = builder.ToString();
        }

        private void GenerateMethodParametersProcessing(InstanceCmdletMetadata instanceCmdlet, IEnumerable<string> commonParameterSets, IEnumerable<string> queryParameterSets, out string scriptCode, out Dictionary<string, ParameterMetadata> methodParameters, out string outputTypeAttributeDeclaration)
        {
            methodParameters = new Dictionary<string, ParameterMetadata>(StringComparer.OrdinalIgnoreCase);
            outputTypeAttributeDeclaration = string.Empty;
            StringWriter output = new StringWriter(CultureInfo.InvariantCulture);
            output.WriteLine(@"    $__cmdletization_methodParameters = Microsoft.PowerShell.Utility\New-Object System.Collections.Generic.List[Microsoft.PowerShell.Cmdletization.MethodParameter]");
            output.WriteLine("    switch -exact ($PSCmdlet.ParameterSetName) { ");
            InstanceMethodMetadata method = instanceCmdlet.Method;
            output.Write("        { @(");
            bool flag = true;
            foreach (string str in MultiplyParameterSets(this.GetMethodParameterSet(method), "{2}", new IEnumerable<string>[] { commonParameterSets, queryParameterSets }))
            {
                if (!flag)
                {
                    output.Write(", ");
                }
                flag = false;
                output.Write("'{0}'", CommandMetadata.EscapeSingleQuotedString(str));
            }
            output.WriteLine(") -contains $_ } {");
            List<Type> list = new List<Type>();
            List<string> list2 = new List<string>();
            if (method.Parameters != null)
            {
                foreach (InstanceMethodParameterMetadata metadata2 in method.Parameters)
                {
                    string cmdletParameterName = null;
                    if (metadata2.CmdletParameterMetadata != null)
                    {
                        ParameterMetadata metadata3 = this.GetParameter(this.GetMethodParameterSet(method), metadata2.ParameterName, metadata2.Type, metadata2.CmdletParameterMetadata);
                        cmdletParameterName = metadata3.Name;
                        try
                        {
                            methodParameters.Add(metadata3.Name, metadata3);
                        }
                        catch (ArgumentException exception)
                        {
                            throw new XmlException(string.Format(CultureInfo.InvariantCulture, CmdletizationCoreResources.ScriptWriter_DuplicateQueryParameterName, new object[] { "<InstanceCmdlets>...<Cmdlet>", metadata3.Name }), exception);
                        }
                    }
                    MethodParameterBindings methodParameterKind = GetMethodParameterKind(metadata2);
                    Type dotNetType = this.GetDotNetType(metadata2.Type);
                    GenerateSingleMethodParameterProcessing(output, "          ", cmdletParameterName, dotNetType, metadata2.Type.ETSType, metadata2.DefaultValue, metadata2.ParameterName, methodParameterKind);
                    if (MethodParameterBindings.Out == (methodParameterKind & MethodParameterBindings.Out))
                    {
                        list.Add(dotNetType);
                        list2.Add(metadata2.Type.ETSType);
                    }
                }
            }
            if (method.ReturnValue != null)
            {
                MethodParameterBindings bindings2 = GetMethodParameterKind(method.ReturnValue);
                Type item = this.GetDotNetType(method.ReturnValue.Type);
                output.WriteLine(@"      $__cmdletization_returnValue = Microsoft.PowerShell.Utility\New-Object Microsoft.PowerShell.Cmdletization.MethodParameter -Property @{{ Name = 'ReturnValue'; ParameterType = '{0}'; Bindings = '{1}'; Value = $null; IsValuePresent = $false }}", CommandMetadata.EscapeSingleQuotedString(item.FullName), CommandMetadata.EscapeSingleQuotedString(bindings2.ToString()));
                if (!string.IsNullOrEmpty(method.ReturnValue.Type.ETSType))
                {
                    output.WriteLine("      $__cmdletization_methodParameter.ParameterTypeName = '{0}'", CommandMetadata.EscapeSingleQuotedString(method.ReturnValue.Type.ETSType));
                }
                if (MethodParameterBindings.Out == (bindings2 & MethodParameterBindings.Out))
                {
                    list.Add(item);
                    list2.Add(method.ReturnValue.Type.ETSType);
                }
            }
            else
            {
                output.WriteLine("      $__cmdletization_returnValue = $null");
            }
            output.WriteLine(@"      $__cmdletization_methodInvocationInfo = Microsoft.PowerShell.Utility\New-Object Microsoft.PowerShell.Cmdletization.MethodInvocationInfo @('{0}', $__cmdletization_methodParameters, $__cmdletization_returnValue)", CommandMetadata.EscapeSingleQuotedString(method.MethodName));
            if (list.Count == 0)
            {
                output.WriteLine("      $__cmdletization_passThru = $PSBoundParameters.ContainsKey('PassThru') -and $PassThru");
            }
            else
            {
                output.WriteLine("      $__cmdletization_passThru = $false");
            }
            output.WriteLine("            if ($PSBoundParameters.ContainsKey('InputObject')) {");
            output.WriteLine("                foreach ($x in $InputObject) { $__cmdletization_objectModelWrapper.ProcessRecord($x, $__cmdletization_methodInvocationInfo, $__cmdletization_PassThru) }");
            output.WriteLine("            } else {");
            output.WriteLine("                $__cmdletization_objectModelWrapper.ProcessRecord($__cmdletization_queryBuilder, $__cmdletization_methodInvocationInfo, $__cmdletization_PassThru)");
            output.WriteLine("            }");
            output.WriteLine("        }");
            output.WriteLine("    }");
            scriptCode = output.ToString();
            if (list.Count == 0)
            {
                outputTypeAttributeDeclaration = this.GetOutputAttributeForGetCmdlet();
            }
            else if (list.Count == 1)
            {
                outputTypeAttributeDeclaration = string.Format(CultureInfo.InvariantCulture, "[OutputType([{0}])]", new object[] { list[0].FullName });
                if ((list2.Count == 1) && !string.IsNullOrEmpty(list2[0]))
                {
                    outputTypeAttributeDeclaration = outputTypeAttributeDeclaration + string.Format(CultureInfo.InvariantCulture, "[OutputType('{0}')]", new object[] { CommandMetadata.EscapeSingleQuotedString(list2[0]) });
                }
            }
        }

        private ParameterMetadata GenerateOptionClause(IEnumerable<string> commonParameterSets, IEnumerable<string> queryParameterSets, IEnumerable<string> methodParameterSets, QueryOption queryOptionMetadata, TextWriter output)
        {
            ParameterMetadata cmdletParameterMetadata = this.GetParameter(queryParameterSets, queryOptionMetadata.OptionName, queryOptionMetadata.Type, queryOptionMetadata.CmdletParameterMetadata);
            this.GenerateIfBoundParameter(commonParameterSets, methodParameterSets, cmdletParameterMetadata, output);
            output.WriteLine("    $__cmdletization_queryBuilder.AddQueryOption('{0}', ${{{1}}})", CommandMetadata.EscapeSingleQuotedString(queryOptionMetadata.OptionName), CommandMetadata.EscapeVariableName(cmdletParameterMetadata.Name));
            output.WriteLine("    }");
            return cmdletParameterMetadata;
        }

        private ParameterMetadata GenerateQueryClause(IEnumerable<string> commonParameterSets, IEnumerable<string> queryParameterSets, IEnumerable<string> methodParameterSets, string queryBuilderMethodName, PropertyMetadata property, PropertyQuery query, TextWriter output)
        {
            ParameterMetadata cmdletParameterMetadata = this.GetParameter(queryParameterSets, property.PropertyName, property.Type, query.CmdletParameterMetadata);
            WildcardablePropertyQuery query2 = query as WildcardablePropertyQuery;
            if ((query2 != null) && !cmdletParameterMetadata.SwitchParameter)
            {
                if (cmdletParameterMetadata.ParameterType == null)
                {
                    cmdletParameterMetadata.ParameterType = typeof(object);
                }
                cmdletParameterMetadata.ParameterType = cmdletParameterMetadata.ParameterType.MakeArrayType();
            }
            this.GenerateIfBoundParameter(commonParameterSets, methodParameterSets, cmdletParameterMetadata, output);
            string str = (query2 == null) ? "__cmdletization_value" : "__cmdletization_values";
            if (query2 == null)
            {
                output.WriteLine("        [object]${0} = ${{{1}}}", str, CommandMetadata.EscapeVariableName(cmdletParameterMetadata.Name));
            }
            else
            {
                output.WriteLine("        ${0} = @(${{{1}}})", str, CommandMetadata.EscapeVariableName(cmdletParameterMetadata.Name));
            }
            output.Write("        $__cmdletization_queryBuilder.{0}('{1}', ${2}", queryBuilderMethodName, CommandMetadata.EscapeSingleQuotedString(property.PropertyName), str);
            if (query2 == null)
            {
                output.WriteLine(", '{0}')", GetBehaviorWhenNoMatchesFound(query.CmdletParameterMetadata));
            }
            else
            {
                bool flag = (!query2.AllowGlobbingSpecified && cmdletParameterMetadata.ParameterType.Equals(typeof(string[]))) || (query2.AllowGlobbingSpecified && query2.AllowGlobbing);
                output.WriteLine(", {0}, '{1}')", flag ? "$true" : "$false", GetBehaviorWhenNoMatchesFound(query.CmdletParameterMetadata));
            }
            output.WriteLine("    }");
            return cmdletParameterMetadata;
        }

        private void GenerateQueryParametersProcessing(InstanceCmdletMetadata instanceCmdlet, IEnumerable<string> commonParameterSets, IEnumerable<string> queryParameterSets, IEnumerable<string> methodParameterSets, out string scriptCode, out Dictionary<string, ParameterMetadata> queryParameters)
        {
            queryParameters = new Dictionary<string, ParameterMetadata>(StringComparer.OrdinalIgnoreCase);
            StringWriter output = new StringWriter(CultureInfo.InvariantCulture);
            output.WriteLine("    $__cmdletization_queryBuilder = $__cmdletization_objectModelWrapper.GetQueryBuilder()");
            GetCmdletParameters getCmdletParameters = this.GetGetCmdletParameters(instanceCmdlet);
            if (getCmdletParameters.QueryableProperties != null)
            {
                foreach (PropertyMetadata metadata in from p in getCmdletParameters.QueryableProperties
                    where p.Items != null
                    select p)
                {
                    for (int i = 0; i < metadata.Items.Length; i++)
                    {
                        string str;
                        switch (metadata.ItemsElementName[i])
                        {
                            case ItemsChoiceType.ExcludeQuery:
                                str = "ExcludeByProperty";
                                break;

                            case ItemsChoiceType.MaxValueQuery:
                                str = "FilterByMaxPropertyValue";
                                break;

                            case ItemsChoiceType.MinValueQuery:
                                str = "FilterByMinPropertyValue";
                                break;

                            case ItemsChoiceType.RegularQuery:
                                str = "FilterByProperty";
                                break;

                            default:
                                str = "NotAValidMethod";
                                break;
                        }
                        ParameterMetadata metadata2 = this.GenerateQueryClause(commonParameterSets, queryParameterSets, methodParameterSets, str, metadata, metadata.Items[i], output);
                        switch (metadata.ItemsElementName[i])
                        {
                            case ItemsChoiceType.ExcludeQuery:
                            case ItemsChoiceType.RegularQuery:
                                metadata2.Attributes.Add(new ValidateNotNullAttribute());
                                break;
                        }
                        try
                        {
                            queryParameters.Add(metadata2.Name, metadata2);
                        }
                        catch (ArgumentException exception)
                        {
                            throw new XmlException(string.Format(CultureInfo.InvariantCulture, CmdletizationCoreResources.ScriptWriter_DuplicateQueryParameterName, new object[] { "<GetCmdletParameters>", metadata2.Name }), exception);
                        }
                    }
                }
            }
            if (getCmdletParameters.QueryableAssociations != null)
            {
                foreach (Association association in from a in getCmdletParameters.QueryableAssociations
                    where a.AssociatedInstance != null
                    select a)
                {
                    ParameterMetadata metadata3 = this.GenerateAssociationClause(commonParameterSets, queryParameterSets, methodParameterSets, association, association.AssociatedInstance, output);
                    try
                    {
                        queryParameters.Add(metadata3.Name, metadata3);
                    }
                    catch (ArgumentException exception2)
                    {
                        throw new XmlException(string.Format(CultureInfo.InvariantCulture, CmdletizationCoreResources.ScriptWriter_DuplicateQueryParameterName, new object[] { "<GetCmdletParameters>", metadata3.Name }), exception2);
                    }
                }
            }
            if (getCmdletParameters.QueryOptions != null)
            {
                foreach (QueryOption option in getCmdletParameters.QueryOptions)
                {
                    ParameterMetadata metadata4 = this.GenerateOptionClause(commonParameterSets, queryParameterSets, methodParameterSets, option, output);
                    try
                    {
                        queryParameters.Add(metadata4.Name, metadata4);
                    }
                    catch (ArgumentException exception3)
                    {
                        throw new XmlException(string.Format(CultureInfo.InvariantCulture, CmdletizationCoreResources.ScriptWriter_DuplicateQueryParameterName, new object[] { "<GetCmdletParameters>", metadata4.Name }), exception3);
                    }
                }
            }
            if (instanceCmdlet != null)
            {
                string str5;
                ParameterMetadata metadata5 = new ParameterMetadata("InputObject", this.objectInstanceType.MakeArrayType());
                ParameterSetMetadata.ParameterFlags valueFromPipeline = ParameterSetMetadata.ParameterFlags.ValueFromPipeline;
                if (queryParameters.Count > 0)
                {
                    valueFromPipeline |= ParameterSetMetadata.ParameterFlags.Mandatory;
                }
                if (this.objectModelWrapper.FullName.Equals("Microsoft.PowerShell.Cmdletization.Cim.CimCmdletAdapter"))
                {
                    int num2 = this.cmdletizationMetadata.Class.ClassName.LastIndexOf('\\');
                    int num3 = this.cmdletizationMetadata.Class.ClassName.LastIndexOf('/');
                    int num4 = Math.Max(num2, num3);
                    string str6 = this.cmdletizationMetadata.Class.ClassName.Substring(num4 + 1, (this.cmdletizationMetadata.Class.ClassName.Length - num4) - 1);
                    str5 = string.Format(CultureInfo.InvariantCulture, "{0}#{1}", new object[] { this.objectInstanceType.FullName, str6 });
                }
                else
                {
                    str5 = string.Format(CultureInfo.InvariantCulture, "{0}#{1}", new object[] { this.objectInstanceType.FullName, this.cmdletizationMetadata.Class.ClassName });
                }
                metadata5.Attributes.Add(new PSTypeNameAttribute(str5));
                metadata5.Attributes.Add(new ValidateNotNullAttribute());
                metadata5.ParameterSets.Clear();
                ParameterSetMetadata metadata6 = new ParameterSetMetadata(-2147483648, valueFromPipeline, null);
                metadata5.ParameterSets.Add("InputObject (cdxml)", metadata6);
                queryParameters.Add(metadata5.Name, metadata5);
            }
            output.WriteLine();
            scriptCode = output.ToString();
        }

        private static void GenerateSingleMethodParameterProcessing(TextWriter output, string prefix, string cmdletParameterName, Type cmdletParameterType, string etsParameterTypeName, string cmdletParameterDefaultValue, string methodParameterName, MethodParameterBindings methodParameterBindings)
        {
            string fullName = (cmdletParameterType ?? typeof(object)).FullName;
            if (cmdletParameterDefaultValue != null)
            {
                output.WriteLine("{0}[object]$__cmdletization_defaultValue = [System.Management.Automation.LanguagePrimitives]::ConvertTo('{1}', '{2}')", prefix, CommandMetadata.EscapeSingleQuotedString(cmdletParameterDefaultValue), CommandMetadata.EscapeSingleQuotedString(fullName));
                output.WriteLine("{0}[object]$__cmdletization_defaultValueIsPresent = $true", prefix);
            }
            else
            {
                output.WriteLine("{0}[object]$__cmdletization_defaultValue = $null", prefix);
                output.WriteLine("{0}[object]$__cmdletization_defaultValueIsPresent = $false", prefix);
            }
            if (MethodParameterBindings.In == (methodParameterBindings & MethodParameterBindings.In))
            {
                output.WriteLine("{0}if ($PSBoundParameters.ContainsKey('{1}')) {{", prefix, CommandMetadata.EscapeSingleQuotedString(cmdletParameterName));
                output.WriteLine("{0}  [object]$__cmdletization_value = ${{{1}}}", prefix, CommandMetadata.EscapeVariableName(cmdletParameterName));
                output.WriteLine(@"{0}  $__cmdletization_methodParameter = Microsoft.PowerShell.Utility\New-Object Microsoft.PowerShell.Cmdletization.MethodParameter -Property @{{Name = '{1}'; ParameterType = '{2}'; Bindings = '{3}'; Value = $__cmdletization_value; IsValuePresent = $true}}", new object[] { prefix, CommandMetadata.EscapeSingleQuotedString(methodParameterName), CommandMetadata.EscapeSingleQuotedString(fullName), CommandMetadata.EscapeSingleQuotedString(methodParameterBindings.ToString()) });
                output.WriteLine("{0}}} else {{", prefix);
            }
            output.WriteLine(@"{0}  $__cmdletization_methodParameter = Microsoft.PowerShell.Utility\New-Object Microsoft.PowerShell.Cmdletization.MethodParameter -Property @{{Name = '{1}'; ParameterType = '{2}'; Bindings = '{3}'; Value = $__cmdletization_defaultValue; IsValuePresent = $__cmdletization_defaultValueIsPresent}}", new object[] { prefix, CommandMetadata.EscapeSingleQuotedString(methodParameterName), CommandMetadata.EscapeSingleQuotedString(fullName), CommandMetadata.EscapeSingleQuotedString(methodParameterBindings.ToString()) });
            if (MethodParameterBindings.In == (methodParameterBindings & MethodParameterBindings.In))
            {
                output.WriteLine("{0}}}", prefix);
            }
            if (!string.IsNullOrEmpty(etsParameterTypeName))
            {
                output.WriteLine("{0}$__cmdletization_methodParameter.ParameterTypeName = '{1}'", prefix, CommandMetadata.EscapeSingleQuotedString(etsParameterTypeName));
            }
            output.WriteLine("{0}$__cmdletization_methodParameters.Add($__cmdletization_methodParameter)", prefix);
            output.WriteLine();
        }

        private static BehaviorOnNoMatch GetBehaviorWhenNoMatchesFound(CmdletParameterMetadataForGetCmdletFilteringParameter cmdletParameterMetadata)
        {
            if ((cmdletParameterMetadata == null) || !cmdletParameterMetadata.ErrorOnNoMatchSpecified)
            {
                return BehaviorOnNoMatch.Default;
            }
            if (cmdletParameterMetadata.ErrorOnNoMatch)
            {
                return BehaviorOnNoMatch.ReportErrors;
            }
            return BehaviorOnNoMatch.SilentlyContinue;
        }

        private string GetCmdletName(CommonCmdletMetadata cmdletMetadata)
        {
            string str = cmdletMetadata.Noun ?? this.cmdletizationMetadata.Class.DefaultNoun;
            return (cmdletMetadata.Verb + "-" + str);
        }

        private static List<List<string>> GetCombinations(params IEnumerable<string>[] x)
        {
            if (x.Length == 1)
            {
                List<List<string>> list = new List<List<string>>();
                foreach (string str in x[0])
                {
                    List<string> list2;
                    list2 = new List<string> {
                        str
                    };
                }
                return list;
            }
            IEnumerable<string>[] destinationArray = new IEnumerable<string>[x.Length - 1];
            Array.Copy(x, 0, destinationArray, 0, destinationArray.Length);
            List<List<string>> combinations = GetCombinations(destinationArray);
            List<List<string>> list4 = new List<List<string>>();
            foreach (List<string> list5 in combinations)
            {
                foreach (string str2 in x[x.Length - 1])
                {
                    List<string> list6;
                    list6 = new List<string>(list5) {
                        str2
                    };
                }
            }
            return list4;
        }

        private CommandMetadata GetCommandMetadata(CommonCmdletMetadata cmdletMetadata)
        {
            string defaultParameterSetName = null;
            StaticCmdletMetadataCmdletMetadata metadata = cmdletMetadata as StaticCmdletMetadataCmdletMetadata;
            if ((metadata != null) && !string.IsNullOrEmpty(metadata.DefaultCmdletParameterSet))
            {
                defaultParameterSetName = metadata.DefaultCmdletParameterSet;
            }
            System.Management.Automation.ConfirmImpact none = System.Management.Automation.ConfirmImpact.None;
            if (cmdletMetadata.ConfirmImpactSpecified)
            {
                none = (System.Management.Automation.ConfirmImpact) cmdletMetadata.ConfirmImpact;
            }
            Dictionary<string, ParameterMetadata> parameters = new Dictionary<string, ParameterMetadata>(StringComparer.OrdinalIgnoreCase);
            CommandMetadata metadata2 = new CommandMetadata(this.GetCmdletName(cmdletMetadata), CommandTypes.Cmdlet, true, defaultParameterSetName, none != System.Management.Automation.ConfirmImpact.None, none, false, false, false, parameters);
            if (!string.IsNullOrEmpty(cmdletMetadata.HelpUri))
            {
                metadata2.HelpUri = cmdletMetadata.HelpUri;
            }
            return metadata2;
        }

        private Dictionary<string, ParameterMetadata> GetCommonParameters()
        {
            Dictionary<string, ParameterMetadata> commonParameters = new Dictionary<string, ParameterMetadata>(StringComparer.OrdinalIgnoreCase);
            InternalParameterMetadata metadata = new InternalParameterMetadata(this.objectModelWrapper, false);
            foreach (CompiledCommandParameter parameter in metadata.BindableParameters.Values)
            {
                ParameterMetadata metadata2 = new ParameterMetadata(parameter);
                foreach (ParameterSetMetadata metadata3 in metadata2.ParameterSets.Values)
                {
                    if (metadata3.ValueFromPipeline)
                    {
                        throw new XmlException(string.Format(CultureInfo.InvariantCulture, CmdletizationCoreResources.ScriptWriter_ObjectModelWrapperUsesIgnoredParameterMetadata, new object[] { this.objectModelWrapper.FullName, metadata2.Name, "ValueFromPipeline" }));
                    }
                    if (metadata3.ValueFromPipelineByPropertyName)
                    {
                        throw new XmlException(string.Format(CultureInfo.InvariantCulture, CmdletizationCoreResources.ScriptWriter_ObjectModelWrapperUsesIgnoredParameterMetadata, new object[] { this.objectModelWrapper.FullName, metadata2.Name, "ValueFromPipelineByPropertyName" }));
                    }
                    if (metadata3.ValueFromRemainingArguments)
                    {
                        throw new XmlException(string.Format(CultureInfo.InvariantCulture, CmdletizationCoreResources.ScriptWriter_ObjectModelWrapperUsesIgnoredParameterMetadata, new object[] { this.objectModelWrapper.FullName, metadata2.Name, "ValueFromRemainingArguments" }));
                    }
                    metadata3.ValueFromPipeline = false;
                    metadata3.ValueFromPipelineByPropertyName = false;
                    metadata3.ValueFromRemainingArguments = false;
                }
                commonParameters.Add(metadata2.Name, metadata2);
            }
            List<string> commonParameterSets = GetCommonParameterSets(commonParameters);
            if (commonParameterSets.Count > 1)
            {
                throw new XmlException(string.Format(CultureInfo.InvariantCulture, CmdletizationCoreResources.ScriptWriter_ObjectModelWrapperDefinesMultipleParameterSets, new object[] { this.objectModelWrapper.FullName }));
            }
            foreach (ParameterMetadata metadata4 in commonParameters.Values)
            {
                if ((metadata4.ParameterSets.Count == 1) && metadata4.ParameterSets.ContainsKey("__AllParameterSets"))
                {
                    ParameterSetMetadata metadata5 = metadata4.ParameterSets["__AllParameterSets"];
                    metadata4.ParameterSets.Clear();
                    foreach (string str5 in commonParameterSets)
                    {
                        metadata4.ParameterSets.Add(str5, metadata5);
                    }
                }
            }
            return commonParameters;
        }

        private static List<string> GetCommonParameterSets(Dictionary<string, ParameterMetadata> commonParameters)
        {
            Dictionary<string, object> dictionary = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            foreach (ParameterMetadata metadata in commonParameters.Values)
            {
                foreach (string str in metadata.ParameterSets.Keys)
                {
                    if (!str.Equals("__AllParameterSets"))
                    {
                        dictionary[str] = null;
                    }
                }
            }
            if (dictionary.Count == 0)
            {
                dictionary.Add("__AllParameterSets", null);
            }
            List<string> list = new List<string>(dictionary.Keys);
            list.Sort(StringComparer.Ordinal);
            return list;
        }

        private Type GetDotNetType(TypeMetadata typeMetadata)
        {
            string pSType;
            List<EnumMetadataEnum> list = (from e in this.cmdletizationMetadata.Enums ?? Enumerable.Empty<EnumMetadataEnum>()
                where Regex.IsMatch(typeMetadata.PSType, string.Format(CultureInfo.InvariantCulture, @"\b{0}\b", new object[] { Regex.Escape(e.EnumName) }), RegexOptions.CultureInvariant)
                select e).ToList<EnumMetadataEnum>();
            EnumMetadataEnum enumMetadata = (list.Count == 1) ? list[0] : null;
            if (enumMetadata != null)
            {
                pSType = typeMetadata.PSType.Replace(enumMetadata.EnumName, EnumWriter.GetEnumFullName(enumMetadata));
            }
            else
            {
                pSType = typeMetadata.PSType;
            }
            return (Type) LanguagePrimitives.ConvertTo(pSType, typeof(Type), CultureInfo.InvariantCulture);
        }

        private CommonCmdletMetadata GetGetCmdletMetadata()
        {
            if (this.cmdletizationMetadata.Class.InstanceCmdlets.GetCmdlet != null)
            {
                return this.cmdletizationMetadata.Class.InstanceCmdlets.GetCmdlet.CmdletMetadata;
            }
            return new CommonCmdletMetadata { Noun = this.cmdletizationMetadata.Class.DefaultNoun, Verb = "Get" };
        }

        private GetCmdletParameters GetGetCmdletParameters(InstanceCmdletMetadata instanceCmdlet)
        {
            if (instanceCmdlet == null)
            {
                if ((this.cmdletizationMetadata.Class.InstanceCmdlets.GetCmdlet != null) && (this.cmdletizationMetadata.Class.InstanceCmdlets.GetCmdlet.GetCmdletParameters != null))
                {
                    return this.cmdletizationMetadata.Class.InstanceCmdlets.GetCmdlet.GetCmdletParameters;
                }
            }
            else if (instanceCmdlet.GetCmdletParameters != null)
            {
                return instanceCmdlet.GetCmdletParameters;
            }
            return this.cmdletizationMetadata.Class.InstanceCmdlets.GetCmdletParameters;
        }

        private string GetHelpDirectiveForExternalHelp()
        {
            StringBuilder builder = new StringBuilder();
            if (GenerationOptions.HelpXml == (this.generationOptions & GenerationOptions.HelpXml))
            {
                builder.AppendFormat(CultureInfo.InvariantCulture, "# .EXTERNALHELP {0}.cdxml-Help.xml", new object[] { EscapeModuleNameForHelpComment(this.moduleName) });
            }
            return builder.ToString();
        }

        private static MethodParameterBindings GetMethodParameterKind(CommonMethodMetadataReturnValue returnValue)
        {
            MethodParameterBindings bindings = 0;
            if (returnValue.CmdletOutputMetadata == null)
            {
                return bindings;
            }
            if (returnValue.CmdletOutputMetadata.ErrorCode == null)
            {
                return (bindings | MethodParameterBindings.Out);
            }
            return (bindings | MethodParameterBindings.Error);
        }

        private static MethodParameterBindings GetMethodParameterKind(InstanceMethodParameterMetadata methodParameter)
        {
            MethodParameterBindings bindings = 0;
            if (methodParameter.CmdletParameterMetadata != null)
            {
                bindings |= MethodParameterBindings.In;
            }
            if (methodParameter.CmdletOutputMetadata == null)
            {
                return bindings;
            }
            if (methodParameter.CmdletOutputMetadata.ErrorCode == null)
            {
                return (bindings | MethodParameterBindings.Out);
            }
            return (bindings | MethodParameterBindings.Error);
        }

        private static MethodParameterBindings GetMethodParameterKind(StaticMethodParameterMetadata methodParameter)
        {
            MethodParameterBindings bindings = 0;
            if (methodParameter.CmdletParameterMetadata != null)
            {
                bindings |= MethodParameterBindings.In;
            }
            if (methodParameter.CmdletOutputMetadata == null)
            {
                return bindings;
            }
            if (methodParameter.CmdletOutputMetadata.ErrorCode == null)
            {
                return (bindings | MethodParameterBindings.Out);
            }
            return (bindings | MethodParameterBindings.Error);
        }

        private string GetMethodParameterSet(CommonMethodMetadata methodMetadata)
        {
            int count;
            if (!this._staticMethodMetadataToUniqueId.TryGetValue(methodMetadata, out count))
            {
                count = this._staticMethodMetadataToUniqueId.Count;
                this._staticMethodMetadataToUniqueId.Add(methodMetadata, count);
            }
            return (methodMetadata.MethodName + count);
        }

        private string GetMethodParameterSet(StaticMethodMetadata staticMethod)
        {
            return (staticMethod.CmdletParameterSet ?? this.GetMethodParameterSet((CommonMethodMetadata) staticMethod));
        }

        private List<string> GetMethodParameterSets(InstanceCmdletMetadata instanceCmdlet)
        {
            Dictionary<string, object> dictionary = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            InstanceMethodMetadata method = instanceCmdlet.Method;
            string methodParameterSet = this.GetMethodParameterSet(method);
            dictionary.Add(methodParameterSet, null);
            return new List<string>(dictionary.Keys);
        }

        private List<string> GetMethodParameterSets(StaticCmdletMetadata staticCmdlet)
        {
            Dictionary<string, object> dictionary = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            foreach (StaticMethodMetadata metadata in staticCmdlet.Method)
            {
                string methodParameterSet = this.GetMethodParameterSet(metadata);
                if (dictionary.ContainsKey(methodParameterSet))
                {
                    throw new XmlException(string.Format(CultureInfo.InvariantCulture, CmdletizationCoreResources.ScriptWriter_DuplicateParameterSetInStaticCmdlet, new object[] { this.GetCmdletName(staticCmdlet.CmdletMetadata), methodParameterSet }));
                }
                dictionary.Add(methodParameterSet, null);
            }
            return new List<string>(dictionary.Keys);
        }

        private string GetOutputAttributeForGetCmdlet()
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendFormat(CultureInfo.InvariantCulture, "[OutputType([{0}])]", new object[] { CommandMetadata.EscapeSingleQuotedString(this.objectInstanceType.FullName) });
            builder.AppendLine();
            builder.AppendFormat(CultureInfo.InvariantCulture, "[OutputType('{0}#{1}')]", new object[] { CommandMetadata.EscapeSingleQuotedString(this.objectInstanceType.FullName), CommandMetadata.EscapeSingleQuotedString(this.cmdletizationMetadata.Class.ClassName) });
            builder.AppendLine();
            return builder.ToString();
        }

        private ParameterMetadata GetParameter(IEnumerable<string> queryParameterSets, string objectModelParameterName, TypeMetadata parameterType, CmdletParameterMetadataForGetCmdletParameter parameterCmdletization)
        {
            ParameterMetadata metadata = this.GetParameter("__AllParameterSets", objectModelParameterName, parameterType, parameterCmdletization, ((parameterCmdletization != null) && parameterCmdletization.ValueFromPipelineSpecified) && parameterCmdletization.ValueFromPipeline, ((parameterCmdletization != null) && parameterCmdletization.ValueFromPipelineByPropertyNameSpecified) && parameterCmdletization.ValueFromPipelineByPropertyName);
            ParameterSetMetadata metadata2 = metadata.ParameterSets["__AllParameterSets"];
            metadata.ParameterSets.Clear();
            if (((parameterCmdletization != null) && (parameterCmdletization.CmdletParameterSets != null)) && (parameterCmdletization.CmdletParameterSets.Length > 0))
            {
                queryParameterSets = parameterCmdletization.CmdletParameterSets;
            }
            foreach (string str in queryParameterSets)
            {
                if (!str.Equals("InputObject (cdxml)", StringComparison.OrdinalIgnoreCase))
                {
                    metadata.ParameterSets.Add(str, metadata2);
                }
            }
            return metadata;
        }

        private ParameterMetadata GetParameter(string parameterSetName, string objectModelParameterName, TypeMetadata parameterType, CmdletParameterMetadataForInstanceMethodParameter parameterCmdletization)
        {
            return this.GetParameter(parameterSetName, objectModelParameterName, parameterType, parameterCmdletization, false, ((parameterCmdletization != null) && parameterCmdletization.ValueFromPipelineByPropertyNameSpecified) && parameterCmdletization.ValueFromPipelineByPropertyName);
        }

        private ParameterMetadata GetParameter(string parameterSetName, string objectModelParameterName, TypeMetadata parameterType, CmdletParameterMetadataForStaticMethodParameter parameterCmdletization)
        {
            return this.GetParameter(parameterSetName, objectModelParameterName, parameterType, parameterCmdletization, ((parameterCmdletization != null) && parameterCmdletization.ValueFromPipelineSpecified) && parameterCmdletization.ValueFromPipeline, ((parameterCmdletization != null) && parameterCmdletization.ValueFromPipelineByPropertyNameSpecified) && parameterCmdletization.ValueFromPipelineByPropertyName);
        }

        private ParameterMetadata GetParameter(string parameterSetName, string objectModelParameterName, TypeMetadata parameterTypeMetadata, CmdletParameterMetadata parameterCmdletization, bool isValueFromPipeline, bool isValueFromPipelineByPropertyName)
        {
            string pSName;
            if ((parameterCmdletization != null) && !string.IsNullOrEmpty(parameterCmdletization.PSName))
            {
                pSName = parameterCmdletization.PSName;
            }
            else
            {
                pSName = objectModelParameterName;
            }
            ParameterMetadata metadata = new ParameterMetadata(pSName) {
                ParameterType = this.GetDotNetType(parameterTypeMetadata)
            };
            if (parameterTypeMetadata.ETSType != null)
            {
                metadata.Attributes.Add(new PSTypeNameAttribute(parameterTypeMetadata.ETSType));
            }
            if (parameterCmdletization != null)
            {
                if (parameterCmdletization.Aliases != null)
                {
                    foreach (string str2 in parameterCmdletization.Aliases)
                    {
                        if (!string.IsNullOrEmpty(str2))
                        {
                            metadata.Aliases.Add(str2);
                        }
                    }
                }
                if (parameterCmdletization.AllowEmptyCollection != null)
                {
                    metadata.Attributes.Add(new AllowEmptyCollectionAttribute());
                }
                if (parameterCmdletization.AllowEmptyString != null)
                {
                    metadata.Attributes.Add(new AllowEmptyStringAttribute());
                }
                if (parameterCmdletization.AllowNull != null)
                {
                    metadata.Attributes.Add(new AllowNullAttribute());
                }
                if (parameterCmdletization.ValidateCount != null)
                {
                    int minLength = (int) LanguagePrimitives.ConvertTo(parameterCmdletization.ValidateCount.Min, typeof(int), CultureInfo.InvariantCulture);
                    int maxLength = (int) LanguagePrimitives.ConvertTo(parameterCmdletization.ValidateCount.Max, typeof(int), CultureInfo.InvariantCulture);
                    metadata.Attributes.Add(new ValidateCountAttribute(minLength, maxLength));
                }
                if (parameterCmdletization.ValidateLength != null)
                {
                    int num3 = (int) LanguagePrimitives.ConvertTo(parameterCmdletization.ValidateLength.Min, typeof(int), CultureInfo.InvariantCulture);
                    int num4 = (int) LanguagePrimitives.ConvertTo(parameterCmdletization.ValidateLength.Max, typeof(int), CultureInfo.InvariantCulture);
                    metadata.Attributes.Add(new ValidateLengthAttribute(num3, num4));
                }
                if (parameterCmdletization.ValidateNotNull != null)
                {
                    metadata.Attributes.Add(new ValidateNotNullAttribute());
                }
                if (parameterCmdletization.ValidateNotNullOrEmpty != null)
                {
                    metadata.Attributes.Add(new ValidateNotNullOrEmptyAttribute());
                }
                if (parameterCmdletization.ValidateRange != null)
                {
                    Type type2;
                    Type parameterType = metadata.ParameterType;
                    if (parameterType == null)
                    {
                        type2 = typeof(string);
                    }
                    else
                    {
                        type2 = parameterType.HasElementType ? parameterType.GetElementType() : parameterType;
                    }
                    object minRange = LanguagePrimitives.ConvertTo(parameterCmdletization.ValidateRange.Min, type2, CultureInfo.InvariantCulture);
                    object maxRange = LanguagePrimitives.ConvertTo(parameterCmdletization.ValidateRange.Max, type2, CultureInfo.InvariantCulture);
                    metadata.Attributes.Add(new ValidateRangeAttribute(minRange, maxRange));
                }
                if (parameterCmdletization.ValidateSet != null)
                {
                    List<string> list = new List<string>();
                    foreach (string str3 in parameterCmdletization.ValidateSet)
                    {
                        list.Add(str3);
                    }
                    metadata.Attributes.Add(new ValidateSetAttribute(list.ToArray()));
                }
            }
            int position = -2147483648;
            ParameterSetMetadata.ParameterFlags flags = 0;
            if (parameterCmdletization != null)
            {
                if (!string.IsNullOrEmpty(parameterCmdletization.Position))
                {
                    position = (int) LanguagePrimitives.ConvertTo(parameterCmdletization.Position, typeof(int), CultureInfo.InvariantCulture);
                }
                if (parameterCmdletization.IsMandatorySpecified && parameterCmdletization.IsMandatory)
                {
                    flags |= ParameterSetMetadata.ParameterFlags.Mandatory;
                }
            }
            if (isValueFromPipeline)
            {
                flags |= ParameterSetMetadata.ParameterFlags.ValueFromPipeline;
            }
            if (isValueFromPipelineByPropertyName)
            {
                flags |= ParameterSetMetadata.ParameterFlags.ValueFromPipelineByPropertyName;
            }
            metadata.ParameterSets.Add(parameterSetName, new ParameterSetMetadata(position, flags, null));
            return metadata;
        }

        private List<string> GetQueryParameterSets(InstanceCmdletMetadata instanceCmdlet)
        {
            Dictionary<string, object> dictionary = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            List<CmdletParameterMetadataForGetCmdletParameter> list = new List<CmdletParameterMetadataForGetCmdletParameter>();
            bool flag = false;
            GetCmdletParameters getCmdletParameters = this.GetGetCmdletParameters(instanceCmdlet);
            if (getCmdletParameters.QueryableProperties != null)
            {
                foreach (PropertyMetadata metadata in getCmdletParameters.QueryableProperties)
                {
                    if (metadata.Items != null)
                    {
                        foreach (PropertyQuery query in metadata.Items)
                        {
                            flag = true;
                            if (query.CmdletParameterMetadata != null)
                            {
                                list.Add(query.CmdletParameterMetadata);
                            }
                        }
                    }
                }
            }
            if (getCmdletParameters.QueryableAssociations != null)
            {
                foreach (Association association in getCmdletParameters.QueryableAssociations)
                {
                    if (association.AssociatedInstance != null)
                    {
                        flag = true;
                        if (association.AssociatedInstance.CmdletParameterMetadata != null)
                        {
                            list.Add(association.AssociatedInstance.CmdletParameterMetadata);
                        }
                    }
                }
            }
            if (getCmdletParameters.QueryOptions != null)
            {
                foreach (QueryOption option in getCmdletParameters.QueryOptions)
                {
                    flag = true;
                    if (option.CmdletParameterMetadata != null)
                    {
                        list.Add(option.CmdletParameterMetadata);
                    }
                }
            }
            foreach (CmdletParameterMetadataForGetCmdletParameter parameter in list)
            {
                if (parameter.CmdletParameterSets != null)
                {
                    foreach (string str in parameter.CmdletParameterSets)
                    {
                        dictionary[str] = null;
                    }
                }
            }
            if (flag && (dictionary.Count == 0))
            {
                dictionary.Add("Query (cdxml)", null);
                getCmdletParameters.DefaultCmdletParameterSet = "Query (cdxml)";
            }
            if (instanceCmdlet != null)
            {
                dictionary.Add("InputObject (cdxml)", null);
            }
            return new List<string>(dictionary.Keys);
        }

        private static void MultiplyParameterSets(Dictionary<string, ParameterMetadata> parameters, string parameterSetNameTemplate, params IEnumerable<string>[] otherParameterSets)
        {
            List<List<string>> combinations = GetCombinations(otherParameterSets);
            foreach (ParameterMetadata metadata in parameters.Values)
            {
                List<KeyValuePair<string, ParameterSetMetadata>> list2 = new List<KeyValuePair<string, ParameterSetMetadata>>(metadata.ParameterSets);
                metadata.ParameterSets.Clear();
                foreach (KeyValuePair<string, ParameterSetMetadata> pair in list2)
                {
                    foreach (List<string> list3 in combinations)
                    {
                        string[] array = new string[otherParameterSets.Length + 1];
                        array[0] = pair.Key;
                        list3.CopyTo(array, 1);
                        string key = string.Format(CultureInfo.InvariantCulture, parameterSetNameTemplate, array);
                        metadata.ParameterSets.Add(key, pair.Value);
                    }
                }
            }
        }

        private static IEnumerable<string> MultiplyParameterSets(string mainParameterSet, string parameterSetNameTemplate, params IEnumerable<string>[] otherParameterSets)
        {
            List<string> list = new List<string>();
            foreach (List<string> list3 in GetCombinations(otherParameterSets))
            {
                string[] array = new string[otherParameterSets.Length + 1];
                array[0] = mainParameterSet;
                list3.CopyTo(array, 1);
                string item = string.Format(CultureInfo.InvariantCulture, parameterSetNameTemplate, array);
                list.Add(item);
            }
            return list;
        }

        internal void PopulatePSModuleInfo(PSModuleInfo moduleInfo)
        {
            moduleInfo.SetModuleType(ModuleType.Cim);
            moduleInfo.SetVersion(new Version(this.cmdletizationMetadata.Class.Version));
            Hashtable hashtable = new Hashtable(StringComparer.OrdinalIgnoreCase);
            hashtable.Add("ClassName", this.cmdletizationMetadata.Class.ClassName);
            hashtable.Add("CmdletAdapter", this.objectModelWrapper);
            Hashtable hashtable2 = new Hashtable(StringComparer.OrdinalIgnoreCase);
            hashtable2.Add("CmdletsOverObjects", hashtable);
            moduleInfo.PrivateData = hashtable2;
        }

        internal void ReportExportedCommands(PSModuleInfo moduleInfo)
        {
            if (moduleInfo.ExportedCommands.Count == 0)
            {
                moduleInfo.DeclaredAliasExports = new Collection<string>();
                moduleInfo.DeclaredFunctionExports = new Collection<string>();
                IEnumerable<CommonCmdletMetadata> collection = Enumerable.Empty<CommonCmdletMetadata>();
                if (this.cmdletizationMetadata.Class.InstanceCmdlets != null)
                {
                    collection = collection.Append<CommonCmdletMetadata>(this.GetGetCmdletMetadata());
                    if (this.cmdletizationMetadata.Class.InstanceCmdlets.Cmdlet != null)
                    {
                        collection = collection.Concat<CommonCmdletMetadata>(from c in this.cmdletizationMetadata.Class.InstanceCmdlets.Cmdlet select c.CmdletMetadata);
                    }
                }
                if (this.cmdletizationMetadata.Class.StaticCmdlets != null)
                {
                    collection = collection.Concat<CommonCmdletMetadata>((IEnumerable<CommonCmdletMetadata>) (from c in this.cmdletizationMetadata.Class.StaticCmdlets select c.CmdletMetadata));
                }
                foreach (CommonCmdletMetadata metadata in collection)
                {
                    if (metadata.Aliases != null)
                    {
                        foreach (string str in metadata.Aliases)
                        {
                            moduleInfo.DeclaredAliasExports.Add(str);
                        }
                    }
                    CommandMetadata commandMetadata = this.GetCommandMetadata(metadata);
                    moduleInfo.DeclaredFunctionExports.Add(commandMetadata.Name);
                }
            }
        }

        private void SetParameters(CommandMetadata commandMetadata, params Dictionary<string, ParameterMetadata>[] allParameters)
        {
            commandMetadata.Parameters.Clear();
            foreach (Dictionary<string, ParameterMetadata> dictionary in allParameters)
            {
                foreach (KeyValuePair<string, ParameterMetadata> pair in dictionary)
                {
                    if (commandMetadata.Parameters.ContainsKey(pair.Key))
                    {
                        if (this.GetCommonParameters().ContainsKey(pair.Key))
                        {
                            throw new XmlException(string.Format(CultureInfo.InvariantCulture, CmdletizationCoreResources.ScriptWriter_ParameterNameConflictsWithCommonParameters, new object[] { pair.Key, commandMetadata.Name, this.objectModelWrapper.FullName }));
                        }
                        throw new XmlException(string.Format(CultureInfo.InvariantCulture, CmdletizationCoreResources.ScriptWriter_ParameterNameConflictsWithQueryParameters, new object[] { pair.Key, commandMetadata.Name, "<GetCmdletParameters>" }));
                    }
                    commandMetadata.Parameters.Add(pair.Key, pair.Value);
                }
            }
        }

        private void WriteBindCommonParametersFunction(TextWriter output)
        {
            output.WriteLine("\r\nfunction __cmdletization_BindCommonParameters\r\n{\r\n    param(\r\n        $__cmdletization_objectModelWrapper,\r\n        $myPSBoundParameters\r\n    )       \r\n                ");
            foreach (ParameterMetadata metadata in this.GetCommonParameters().Values)
            {
                output.WriteLine("\r\n        if ($myPSBoundParameters.ContainsKey('{0}')) {{ \r\n            $__cmdletization_objectModelWrapper.PSObject.Properties['{0}'].Value = $myPSBoundParameters['{0}'] \r\n        }}\r\n                    ", CommandMetadata.EscapeSingleQuotedString(metadata.Name));
            }
            output.WriteLine("\r\n}\r\n                ");
        }

        private void WriteCmdlet(TextWriter output, InstanceCmdletMetadata instanceCmdlet)
        {
            Dictionary<string, ParameterMetadata> dictionary2;
            string str;
            Dictionary<string, ParameterMetadata> dictionary3;
            string str2;
            string str3;
            this.WriteCmdletAliases(output, instanceCmdlet.CmdletMetadata);
            Dictionary<string, ParameterMetadata> commonParameters = this.GetCommonParameters();
            List<string> commonParameterSets = GetCommonParameterSets(commonParameters);
            List<string> methodParameterSets = this.GetMethodParameterSets(instanceCmdlet);
            List<string> queryParameterSets = this.GetQueryParameterSets(instanceCmdlet);
            this.GenerateQueryParametersProcessing(instanceCmdlet, commonParameterSets, queryParameterSets, methodParameterSets, out str, out dictionary2);
            this.GenerateMethodParametersProcessing(instanceCmdlet, commonParameterSets, queryParameterSets, out str2, out dictionary3, out str3);
            CommandMetadata commandMetadata = this.GetCommandMetadata(instanceCmdlet.CmdletMetadata);
            GetCmdletParameters getCmdletParameters = this.GetGetCmdletParameters(instanceCmdlet);
            if (!string.IsNullOrEmpty(getCmdletParameters.DefaultCmdletParameterSet))
            {
                commandMetadata.DefaultParameterSetName = getCmdletParameters.DefaultCmdletParameterSet;
            }
            else if (queryParameterSets.Count == 1)
            {
                commandMetadata.DefaultParameterSetName = queryParameterSets.Single<string>();
            }
            AddPassThruParameter(commonParameters, instanceCmdlet);
            MultiplyParameterSets(commonParameters, "{1}", new IEnumerable<string>[] { queryParameterSets, methodParameterSets });
            MultiplyParameterSets(dictionary2, "{0}", new IEnumerable<string>[] { commonParameterSets, methodParameterSets });
            MultiplyParameterSets(dictionary3, "{2}", new IEnumerable<string>[] { commonParameterSets, queryParameterSets });
            EnsureOrderOfPositionalParameters(commonParameters, dictionary2);
            EnsureOrderOfPositionalParameters(dictionary2, dictionary3);
            this.SetParameters(commandMetadata, new Dictionary<string, ParameterMetadata>[] { dictionary2, dictionary3, commonParameters });
            output.WriteLine("\r\nfunction {0}\r\n{{\r\n    {1}\r\n    {2}\r\n    param(\r\n    {3})\r\n\r\n    DynamicParam {{\r\n        try \r\n        {{\r\n            if (-not $__cmdletization_exceptionHasBeenThrown)\r\n            {{\r\n                $__cmdletization_objectModelWrapper = Microsoft.PowerShell.Utility\\New-Object $script:ObjectModelWrapper\r\n                $__cmdletization_objectModelWrapper.Initialize($PSCmdlet, $script:ClassName, $script:ClassVersion, $script:ModuleVersion, $script:PrivateData)\r\n\r\n                if ($__cmdletization_objectModelWrapper -is [System.Management.Automation.IDynamicParameters])\r\n                {{\r\n                    ([System.Management.Automation.IDynamicParameters]$__cmdletization_objectModelWrapper).GetDynamicParameters()\r\n                }}\r\n            }}\r\n        }}\r\n        catch\r\n        {{\r\n            $__cmdletization_exceptionHasBeenThrown = $true\r\n            throw\r\n        }}\r\n    }}\r\n\r\n    Begin {{\r\n        $__cmdletization_exceptionHasBeenThrown = $false\r\n        try \r\n        {{\r\n            __cmdletization_BindCommonParameters $__cmdletization_objectModelWrapper $PSBoundParameters\r\n            $__cmdletization_objectModelWrapper.BeginProcessing()\r\n        }}\r\n        catch\r\n        {{\r\n            $__cmdletization_exceptionHasBeenThrown = $true\r\n            throw\r\n        }}\r\n    }}\r\n        ", new object[] { commandMetadata.Name, ProxyCommand.GetCmdletBindingAttribute(commandMetadata), str3, ProxyCommand.GetParamBlock(commandMetadata) });
            output.WriteLine("\r\n    Process {{\r\n        try \r\n        {{\r\n            if (-not $__cmdletization_exceptionHasBeenThrown)\r\n            {{\r\n{0}\r\n            }}\r\n        }}\r\n        catch\r\n        {{\r\n            $__cmdletization_exceptionHasBeenThrown = $true\r\n            throw\r\n        }}\r\n    }}\r\n        ", str + "\r\n" + str2);
            output.WriteLine("\r\n    End {{\r\n        try\r\n        {{\r\n            if (-not $__cmdletization_exceptionHasBeenThrown)\r\n            {{\r\n                $__cmdletization_objectModelWrapper.EndProcessing()\r\n            }}\r\n        }}\r\n        catch\r\n        {{\r\n            throw\r\n        }}\r\n    }}\r\n\r\n    {0}\r\n}}\r\nMicrosoft.PowerShell.Core\\Export-ModuleMember -Function '{1}'\r\n        ", this.GetHelpDirectiveForExternalHelp(), CommandMetadata.EscapeSingleQuotedString(commandMetadata.Name));
            this.functionsToExport.Add(commandMetadata.Name);
        }

        private void WriteCmdlet(TextWriter output, StaticCmdletMetadata staticCmdlet)
        {
            Dictionary<string, ParameterMetadata> dictionary2;
            string str;
            string str2;
            this.WriteCmdletAliases(output, staticCmdlet.CmdletMetadata);
            Dictionary<string, ParameterMetadata> commonParameters = this.GetCommonParameters();
            List<string> commonParameterSets = GetCommonParameterSets(commonParameters);
            this.GenerateMethodParametersProcessing(staticCmdlet, commonParameterSets, out str, out dictionary2, out str2);
            List<string> methodParameterSets = this.GetMethodParameterSets(staticCmdlet);
            CommandMetadata commandMetadata = this.GetCommandMetadata(staticCmdlet.CmdletMetadata);
            if (!string.IsNullOrEmpty(commandMetadata.DefaultParameterSetName))
            {
                commandMetadata.DefaultParameterSetName = string.Format(CultureInfo.InvariantCulture, "{0}", new object[] { commandMetadata.DefaultParameterSetName, commonParameterSets[0] });
            }
            MultiplyParameterSets(commonParameters, "{1}", new IEnumerable<string>[] { methodParameterSets });
            MultiplyParameterSets(dictionary2, "{0}", new IEnumerable<string>[] { commonParameterSets });
            EnsureOrderOfPositionalParameters(commonParameters, dictionary2);
            this.SetParameters(commandMetadata, new Dictionary<string, ParameterMetadata>[] { dictionary2, commonParameters });
            output.WriteLine("\r\nfunction {0}\r\n{{\r\n    {1}\r\n    {2}\r\n    param(\r\n    {3})\r\n\r\n    DynamicParam {{\r\n        try \r\n        {{\r\n            if (-not $__cmdletization_exceptionHasBeenThrown)\r\n            {{\r\n                $__cmdletization_objectModelWrapper = Microsoft.PowerShell.Utility\\New-Object $script:ObjectModelWrapper\r\n                $__cmdletization_objectModelWrapper.Initialize($PSCmdlet, $script:ClassName, $script:ClassVersion, $script:ModuleVersion, $script:PrivateData)\r\n\r\n                if ($__cmdletization_objectModelWrapper -is [System.Management.Automation.IDynamicParameters])\r\n                {{\r\n                    ([System.Management.Automation.IDynamicParameters]$__cmdletization_objectModelWrapper).GetDynamicParameters()\r\n                }}\r\n            }}\r\n        }}\r\n        catch\r\n        {{\r\n            $__cmdletization_exceptionHasBeenThrown = $true\r\n            throw\r\n        }}\r\n    }}\r\n\r\n    Begin {{\r\n        $__cmdletization_exceptionHasBeenThrown = $false\r\n        try \r\n        {{\r\n            __cmdletization_BindCommonParameters $__cmdletization_objectModelWrapper $PSBoundParameters\r\n            $__cmdletization_objectModelWrapper.BeginProcessing()\r\n        }}\r\n        catch\r\n        {{\r\n            $__cmdletization_exceptionHasBeenThrown = $true\r\n            throw\r\n        }}\r\n    }}\r\n        ", new object[] { commandMetadata.Name, ProxyCommand.GetCmdletBindingAttribute(commandMetadata), str2, ProxyCommand.GetParamBlock(commandMetadata) });
            output.WriteLine("\r\n    Process {{\r\n        try \r\n        {{\r\n            if (-not $__cmdletization_exceptionHasBeenThrown)\r\n            {{\r\n{0}\r\n            }}\r\n        }}\r\n        catch\r\n        {{\r\n            $__cmdletization_exceptionHasBeenThrown = $true\r\n            throw\r\n        }}\r\n    }}\r\n        ", str);
            output.WriteLine("\r\n    End {{\r\n        try\r\n        {{\r\n            if (-not $__cmdletization_exceptionHasBeenThrown)\r\n            {{\r\n                $__cmdletization_objectModelWrapper.EndProcessing()\r\n            }}\r\n        }}\r\n        catch\r\n        {{\r\n            throw\r\n        }}\r\n    }}\r\n\r\n    {0}\r\n}}\r\nMicrosoft.PowerShell.Core\\Export-ModuleMember -Function '{1}'\r\n        ", this.GetHelpDirectiveForExternalHelp(), CommandMetadata.EscapeSingleQuotedString(commandMetadata.Name));
            this.functionsToExport.Add(commandMetadata.Name);
        }

        private void WriteCmdletAliases(TextWriter output, CommonCmdletMetadata cmdletMetadata)
        {
            string cmdletName = this.GetCmdletName(cmdletMetadata);
            if (cmdletMetadata.Aliases != null)
            {
                foreach (string str2 in cmdletMetadata.Aliases)
                {
                    output.WriteLine(@"Microsoft.PowerShell.Utility\Set-Alias -Name '{0}' -Value '{1}' -Force -Scope script", CommandMetadata.EscapeSingleQuotedString(str2), CommandMetadata.EscapeSingleQuotedString(cmdletName));
                    output.WriteLine(@"Microsoft.PowerShell.Core\Export-ModuleMember -Alias '{0}'", CommandMetadata.EscapeSingleQuotedString(str2));
                    this.aliasesToExport.Add(str2);
                }
            }
        }

        private void WriteGetCmdlet(TextWriter output)
        {
            Dictionary<string, ParameterMetadata> dictionary2;
            string str;
            Dictionary<string, ParameterMetadata> commonParameters = this.GetCommonParameters();
            List<string> commonParameterSets = GetCommonParameterSets(commonParameters);
            List<string> methodParameterSets = new List<string> {
                string.Empty
            };
            List<string> queryParameterSets = this.GetQueryParameterSets(null);
            this.GenerateQueryParametersProcessing(null, commonParameterSets, queryParameterSets, methodParameterSets, out str, out dictionary2);
            CommonCmdletMetadata getCmdletMetadata = this.GetGetCmdletMetadata();
            CommandMetadata commandMetadata = this.GetCommandMetadata(getCmdletMetadata);
            this.WriteCmdletAliases(output, getCmdletMetadata);
            GetCmdletParameters getCmdletParameters = this.GetGetCmdletParameters(null);
            if (!string.IsNullOrEmpty(getCmdletParameters.DefaultCmdletParameterSet))
            {
                commandMetadata.DefaultParameterSetName = getCmdletParameters.DefaultCmdletParameterSet;
            }
            MultiplyParameterSets(commonParameters, "{1}", new IEnumerable<string>[] { queryParameterSets, methodParameterSets });
            MultiplyParameterSets(dictionary2, "{0}", new IEnumerable<string>[] { commonParameterSets, methodParameterSets });
            EnsureOrderOfPositionalParameters(commonParameters, dictionary2);
            this.SetParameters(commandMetadata, new Dictionary<string, ParameterMetadata>[] { dictionary2, commonParameters });
            output.WriteLine("\r\nfunction {0}\r\n{{\r\n    {1}\r\n    {2}\r\n    param(\r\n    {3})\r\n\r\n    DynamicParam {{\r\n        try \r\n        {{\r\n            if (-not $__cmdletization_exceptionHasBeenThrown)\r\n            {{\r\n                $__cmdletization_objectModelWrapper = Microsoft.PowerShell.Utility\\New-Object $script:ObjectModelWrapper\r\n                $__cmdletization_objectModelWrapper.Initialize($PSCmdlet, $script:ClassName, $script:ClassVersion, $script:ModuleVersion, $script:PrivateData)\r\n\r\n                if ($__cmdletization_objectModelWrapper -is [System.Management.Automation.IDynamicParameters])\r\n                {{\r\n                    ([System.Management.Automation.IDynamicParameters]$__cmdletization_objectModelWrapper).GetDynamicParameters()\r\n                }}\r\n            }}\r\n        }}\r\n        catch\r\n        {{\r\n            $__cmdletization_exceptionHasBeenThrown = $true\r\n            throw\r\n        }}\r\n    }}\r\n\r\n    Begin {{\r\n        $__cmdletization_exceptionHasBeenThrown = $false\r\n        try \r\n        {{\r\n            __cmdletization_BindCommonParameters $__cmdletization_objectModelWrapper $PSBoundParameters\r\n            $__cmdletization_objectModelWrapper.BeginProcessing()\r\n        }}\r\n        catch\r\n        {{\r\n            $__cmdletization_exceptionHasBeenThrown = $true\r\n            throw\r\n        }}\r\n    }}\r\n        ", new object[] { commandMetadata.Name, ProxyCommand.GetCmdletBindingAttribute(commandMetadata), this.GetOutputAttributeForGetCmdlet(), ProxyCommand.GetParamBlock(commandMetadata) });
            output.WriteLine("\r\n    Process {{\r\n        try \r\n        {{\r\n            if (-not $__cmdletization_exceptionHasBeenThrown)\r\n            {{\r\n{0}\r\n            }}\r\n        }}\r\n        catch\r\n        {{\r\n            $__cmdletization_exceptionHasBeenThrown = $true\r\n            throw\r\n        }}\r\n    }}\r\n        ", str + "\r\n    $__cmdletization_objectModelWrapper.ProcessRecord($__cmdletization_queryBuilder)");
            output.WriteLine("\r\n    End {{\r\n        try\r\n        {{\r\n            if (-not $__cmdletization_exceptionHasBeenThrown)\r\n            {{\r\n                $__cmdletization_objectModelWrapper.EndProcessing()\r\n            }}\r\n        }}\r\n        catch\r\n        {{\r\n            throw\r\n        }}\r\n    }}\r\n\r\n    {0}\r\n}}\r\nMicrosoft.PowerShell.Core\\Export-ModuleMember -Function '{1}'\r\n        ", this.GetHelpDirectiveForExternalHelp(), CommandMetadata.EscapeSingleQuotedString(commandMetadata.Name));
            this.functionsToExport.Add(commandMetadata.Name);
        }

        private void WriteModulePreamble(TextWriter output)
        {
            object[] arg = new object[] { CommandMetadata.EscapeSingleQuotedString(this.cmdletizationMetadata.Class.ClassName), CommandMetadata.EscapeSingleQuotedString(this.cmdletizationMetadata.Class.ClassVersion ?? string.Empty), CommandMetadata.EscapeSingleQuotedString(new Version(this.cmdletizationMetadata.Class.Version).ToString()), CommandMetadata.EscapeSingleQuotedString(this.objectModelWrapper.FullName) };
            output.WriteLine("\r\n#requires -version 3.0\r\n\r\nif ($(Microsoft.PowerShell.Core\\Get-Command Set-StrictMode -Module Microsoft.PowerShell.Core)) {{ Microsoft.PowerShell.Core\\Set-StrictMode -Off }}\r\n\r\n$script:MyModule = $MyInvocation.MyCommand.ScriptBlock.Module\r\n\r\n$script:ClassName = '{0}'\r\n$script:ClassVersion = '{1}'\r\n$script:ModuleVersion = '{2}'\r\n$script:ObjectModelWrapper = '{3}'\r\n\r\n$script:PrivateData = Microsoft.PowerShell.Utility\\New-Object 'System.Collections.Generic.Dictionary[string,string]'\r\n\r\nMicrosoft.PowerShell.Core\\Export-ModuleMember -Function @()\r\n        ", arg);
            if (this.cmdletizationMetadata.Class.CmdletAdapterPrivateData != null)
            {
                foreach (ClassMetadataData data in this.cmdletizationMetadata.Class.CmdletAdapterPrivateData)
                {
                    output.WriteLine("$script:PrivateData.Add('{0}', '{1}')", CommandMetadata.EscapeSingleQuotedString(data.Name), CommandMetadata.EscapeSingleQuotedString(data.Value));
                }
            }
        }

        internal void WriteScriptModule(TextWriter output)
        {
            this.WriteModulePreamble(output);
            this.WriteBindCommonParametersFunction(output);
            if (this.cmdletizationMetadata.Enums != null)
            {
                foreach (EnumMetadataEnum enum2 in this.cmdletizationMetadata.Enums)
                {
                    CompileEnum(enum2);
                }
            }
            if (this.cmdletizationMetadata.Class.StaticCmdlets != null)
            {
                foreach (StaticCmdletMetadata metadata in this.cmdletizationMetadata.Class.StaticCmdlets)
                {
                    this.WriteCmdlet(output, metadata);
                }
            }
            if (this.cmdletizationMetadata.Class.InstanceCmdlets != null)
            {
                this.WriteGetCmdlet(output);
                if (this.cmdletizationMetadata.Class.InstanceCmdlets.Cmdlet != null)
                {
                    foreach (InstanceCmdletMetadata metadata2 in this.cmdletizationMetadata.Class.InstanceCmdlets.Cmdlet)
                    {
                        this.WriteCmdlet(output, metadata2);
                    }
                }
            }
        }

        [Flags]
        internal enum GenerationOptions
        {
            FormatPs1Xml = 2,
            HelpXml = 4,
            TypesPs1Xml = 1
        }
    }
}

