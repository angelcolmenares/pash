namespace System.Management.Automation
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Management.Automation.Help;
    using System.Management.Automation.Language;
    using System.Management.Automation.Runspaces;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Xml;

    internal class HelpCommentsParser
    {
        private readonly List<string> _examples;
        private readonly List<string> _inputs;
        private readonly List<string> _links;
        private readonly List<string> _outputs;
        private readonly Dictionary<string, string> _parameters;
        private readonly CommentHelpInfo _sections;
        private const string blankline = @"^\s*$";
        private CommandMetadata commandMetadata;
        private string commandName;
        internal static readonly string commandURI = "http://schemas.microsoft.com/maml/dev/command/2004/10";
        internal const int CommentBlockProximity = 2;
        internal static readonly string devURI = "http://schemas.microsoft.com/maml/dev/2004/10";
        private const string directive = @"^\s*\.(\w+)(\s+(\S.*))?\s*$";
        private XmlDocument doc;
        internal bool isExternalHelpSet;
        internal static readonly string mamlURI = "http://schemas.microsoft.com/maml/2004/10";
        private List<string> parameterDescriptions;
        internal static readonly string ProviderHelpCommandXPath = "/helpItems/providerHelp/CmdletHelpPaths/CmdletHelpPath{0}/command:command[command:details/command:verb='{1}' and command:details/command:noun='{2}']";
        private ScriptBlock scriptBlock;

        private HelpCommentsParser()
        {
            this._sections = new CommentHelpInfo();
            this._parameters = new Dictionary<string, string>();
            this._examples = new List<string>();
            this._inputs = new List<string>();
            this._outputs = new List<string>();
            this._links = new List<string>();
        }

        private HelpCommentsParser(List<string> parameterDescriptions)
        {
            this._sections = new CommentHelpInfo();
            this._parameters = new Dictionary<string, string>();
            this._examples = new List<string>();
            this._inputs = new List<string>();
            this._outputs = new List<string>();
            this._links = new List<string>();
            this.parameterDescriptions = parameterDescriptions;
        }

        private HelpCommentsParser(CommandInfo commandInfo, List<string> parameterDescriptions)
        {
            this._sections = new CommentHelpInfo();
            this._parameters = new Dictionary<string, string>();
            this._examples = new List<string>();
            this._inputs = new List<string>();
            this._outputs = new List<string>();
            this._links = new List<string>();
            FunctionInfo info = commandInfo as FunctionInfo;
            if (info != null)
            {
                this.scriptBlock = info.ScriptBlock;
                this.commandName = info.Name;
            }
            else
            {
                ExternalScriptInfo info2 = commandInfo as ExternalScriptInfo;
                if (info2 != null)
                {
                    this.scriptBlock = info2.ScriptBlock;
                    this.commandName = info2.Path;
                }
            }
            this.commandMetadata = commandInfo.CommandMetadata;
            this.parameterDescriptions = parameterDescriptions;
        }

        internal bool AnalyzeCommentBlock(List<System.Management.Automation.Language.Token> comments)
        {
            if ((comments == null) || (comments.Count == 0))
            {
                return false;
            }
            List<string> commentLines = new List<string>();
            foreach (System.Management.Automation.Language.Token token in comments)
            {
                CollectCommentText(token, commentLines);
            }
            return this.AnalyzeCommentBlock(commentLines);
        }

        private bool AnalyzeCommentBlock(List<string> commentLines)
        {
            bool flag = false;
            for (int i = 0; i < commentLines.Count; i++)
            {
                Match match = Regex.Match(commentLines[i], @"^\s*\.(\w+)(\s+(\S.*))?\s*$");
                if (!match.Success)
                {
                    goto Label_038F;
                }
                flag = true;
                if (!match.Groups[3].Success)
                {
                    goto Label_019E;
                }
                string str3 = match.Groups[1].Value.ToUpperInvariant();
                if (str3 == null)
                {
                    goto Label_019C;
                }
                if (!(str3 == "PARAMETER"))
                {
                    if (str3 == "FORWARDHELPTARGETNAME")
                    {
                        goto Label_00FD;
                    }
                    if (str3 == "FORWARDHELPCATEGORY")
                    {
                        goto Label_0123;
                    }
                    if (str3 == "REMOTEHELPRUNSPACE")
                    {
                        goto Label_0149;
                    }
                    if (str3 == "EXTERNALHELP")
                    {
                        goto Label_016F;
                    }
                    goto Label_019C;
                }
                string key = match.Groups[3].Value.ToUpperInvariant().Trim();
                string section = GetSection(commentLines, ref i);
                if (!this._parameters.ContainsKey(key))
                {
                    this._parameters.Add(key, section);
                }
                continue;
            Label_00FD:
                this._sections.ForwardHelpTargetName = match.Groups[3].Value.Trim();
                continue;
            Label_0123:
                this._sections.ForwardHelpCategory = match.Groups[3].Value.Trim();
                continue;
            Label_0149:
                this._sections.RemoteHelpRunspace = match.Groups[3].Value.Trim();
                continue;
            Label_016F:
                this._sections.MamlHelpFile = match.Groups[3].Value.Trim();
                this.isExternalHelpSet = true;
                continue;
            Label_019C:
                return false;
            Label_019E:
                switch (match.Groups[1].Value.ToUpperInvariant())
                {
                    case "SYNOPSIS":
                    {
                        this._sections.Synopsis = GetSection(commentLines, ref i);
                        continue;
                    }
                    case "DESCRIPTION":
                    {
                        this._sections.Description = GetSection(commentLines, ref i);
                        continue;
                    }
                    case "NOTES":
                    {
                        this._sections.Notes = GetSection(commentLines, ref i);
                        continue;
                    }
                    case "LINK":
                    {
                        this._links.Add(GetSection(commentLines, ref i).Trim());
                        continue;
                    }
                    case "EXAMPLE":
                    {
                        this._examples.Add(GetSection(commentLines, ref i));
                        continue;
                    }
                    case "INPUTS":
                    {
                        this._inputs.Add(GetSection(commentLines, ref i));
                        continue;
                    }
                    case "OUTPUTS":
                    {
                        this._outputs.Add(GetSection(commentLines, ref i));
                        continue;
                    }
                    case "COMPONENT":
                    {
                        this._sections.Component = GetSection(commentLines, ref i).Trim();
                        continue;
                    }
                    case "ROLE":
                    {
                        this._sections.Role = GetSection(commentLines, ref i).Trim();
                        continue;
                    }
                    case "FUNCTIONALITY":
                    {
                        this._sections.Functionality = GetSection(commentLines, ref i).Trim();
                        continue;
                    }
                    default:
                        return false;
                }
            Label_038F:
                if (!Regex.IsMatch(commentLines[i], @"^\s*$"))
                {
                    return false;
                }
            }
            this._sections.Examples = new ReadOnlyCollection<string>(this._examples);
            this._sections.Inputs = new ReadOnlyCollection<string>(this._inputs);
            this._sections.Outputs = new ReadOnlyCollection<string>(this._outputs);
            this._sections.Links = new ReadOnlyCollection<string>(this._links);
            this._sections.Parameters = new Dictionary<string, string>(this._parameters);
            return flag;
        }

        private void BuildSyntaxForParameterSet(XmlElement command, XmlElement syntax, MergedCommandParameterMetadata parameterMetadata, int i)
        {
            XmlElement newChild = this.doc.CreateElement("command:syntaxItem", commandURI);
            XmlElement element2 = this.doc.CreateElement("maml:name", mamlURI);
            XmlText text = this.doc.CreateTextNode(this.commandName);
            newChild.AppendChild(element2).AppendChild(text);
            foreach (MergedCompiledCommandParameter parameter in parameterMetadata.GetParametersInParameterSet(((int) 1) << i))
            {
                if (parameter.BinderAssociation != ParameterBinderAssociation.CommonParameters)
                {
                    CompiledCommandParameter parameter2 = parameter.Parameter;
                    ParameterSetSpecificMetadata parameterSetData = parameter2.GetParameterSetData(((int) 1) << i);
                    string parameterDescription = this.GetParameterDescription(parameter2.Name);
                    bool supportsWildcards = (from attribute in parameter2.CompiledAttributes
                        where attribute is SupportsWildcardsAttribute
                        select attribute).Any<System.Attribute>();
                    XmlElement element3 = this.BuildXmlForParameter(parameter2.Name, parameterSetData.IsMandatory, parameterSetData.ValueFromPipeline, parameterSetData.ValueFromPipelineByPropertyName, parameterSetData.IsPositional ? ((1 + parameterSetData.Position)).ToString(CultureInfo.InvariantCulture) : "named", parameter2.Type, parameterDescription, supportsWildcards, "", true);
                    newChild.AppendChild(element3);
                }
            }
            command.AppendChild(syntax).AppendChild(newChild);
        }

        private XmlElement BuildXmlForParameter(string parameterName, bool isMandatory, bool valueFromPipeline, bool valueFromPipelineByPropertyName, string position, Type type, string description, bool supportsWildcards, string defaultValue, bool forSyntax)
        {
            string str;
            XmlElement element = this.doc.CreateElement("command:parameter", commandURI);
            element.SetAttribute("required", isMandatory ? "true" : "false");
            element.SetAttribute("globbing", supportsWildcards ? "true" : "false");
            if (valueFromPipeline && valueFromPipelineByPropertyName)
            {
                str = "true (ByValue, ByPropertyName)";
            }
            else if (valueFromPipeline)
            {
                str = "true (ByValue)";
            }
            else if (valueFromPipelineByPropertyName)
            {
                str = "true (ByPropertyName)";
            }
            else
            {
                str = "false";
            }
            element.SetAttribute("pipelineInput", str);
            element.SetAttribute("position", position);
            XmlElement newChild = this.doc.CreateElement("maml:name", mamlURI);
            XmlText text = this.doc.CreateTextNode(parameterName);
            element.AppendChild(newChild).AppendChild(text);
            if (!string.IsNullOrEmpty(description))
            {
                XmlElement element3 = this.doc.CreateElement("maml:description", mamlURI);
                XmlElement element4 = this.doc.CreateElement("maml:para", mamlURI);
                XmlText text2 = this.doc.CreateTextNode(description);
                element.AppendChild(element3).AppendChild(element4).AppendChild(text2);
            }
            if (type == null)
            {
                type = typeof(object);
            }
            if (type.IsEnum)
            {
                XmlElement element5 = this.doc.CreateElement("command:parameterValueGroup", commandURI);
                foreach (string str2 in Enum.GetNames(type))
                {
                    XmlElement element6 = this.doc.CreateElement("command:parameterValue", commandURI);
                    element6.SetAttribute("required", "false");
                    XmlText text3 = this.doc.CreateTextNode(str2);
                    element5.AppendChild(element6).AppendChild(text3);
                }
                element.AppendChild(element5);
            }
            else
            {
                bool flag = type == typeof(SwitchParameter);
                if (!forSyntax || !flag)
                {
                    XmlElement element7 = this.doc.CreateElement("command:parameterValue", commandURI);
                    element7.SetAttribute("required", flag ? "false" : "true");
                    XmlText text4 = this.doc.CreateTextNode(type.Name);
                    element.AppendChild(element7).AppendChild(text4);
                }
            }
            if (!forSyntax)
            {
                XmlElement element8 = this.doc.CreateElement("dev:type", devURI);
                XmlElement element9 = this.doc.CreateElement("maml:name", mamlURI);
                XmlText text5 = this.doc.CreateTextNode(type.Name);
                element.AppendChild(element8).AppendChild(element9).AppendChild(text5);
                XmlElement element10 = this.doc.CreateElement("dev:defaultValue", devURI);
                XmlText text6 = this.doc.CreateTextNode(defaultValue);
                element.AppendChild(element10).AppendChild(text6);
            }
            return element;
        }

        internal XmlDocument BuildXmlFromComments()
        {
            this.doc = new XmlDocument();
            XmlElement newChild = this.doc.CreateElement("command:command", commandURI);
            newChild.SetAttribute("xmlns:maml", mamlURI);
            newChild.SetAttribute("xmlns:command", commandURI);
            newChild.SetAttribute("xmlns:dev", devURI);
            this.doc.AppendChild(newChild);
            XmlElement element2 = this.doc.CreateElement("command:details", commandURI);
            newChild.AppendChild(element2);
            XmlElement element3 = this.doc.CreateElement("command:name", commandURI);
            XmlText text = this.doc.CreateTextNode(this.commandName);
            element2.AppendChild(element3).AppendChild(text);
            if (!string.IsNullOrEmpty(this._sections.Synopsis))
            {
                XmlElement element4 = this.doc.CreateElement("maml:description", mamlURI);
                XmlElement element5 = this.doc.CreateElement("maml:para", mamlURI);
                XmlText text2 = this.doc.CreateTextNode(this._sections.Synopsis);
                element2.AppendChild(element4).AppendChild(element5).AppendChild(text2);
            }
            this.DetermineParameterDescriptions();
            XmlElement syntax = this.doc.CreateElement("command:syntax", commandURI);
            MergedCommandParameterMetadata staticCommandParameterMetadata = this.commandMetadata.StaticCommandParameterMetadata;
            if (staticCommandParameterMetadata.ParameterSetCount > 0)
            {
                for (int i = 0; i < staticCommandParameterMetadata.ParameterSetCount; i++)
                {
                    this.BuildSyntaxForParameterSet(newChild, syntax, staticCommandParameterMetadata, i);
                }
            }
            else
            {
                this.BuildSyntaxForParameterSet(newChild, syntax, staticCommandParameterMetadata, 0x7fffffff);
            }
            XmlElement element7 = this.doc.CreateElement("command:parameters", commandURI);
            foreach (KeyValuePair<string, MergedCompiledCommandParameter> pair in staticCommandParameterMetadata.BindableParameters)
            {
                MergedCompiledCommandParameter parameter = pair.Value;
                if (parameter.BinderAssociation != ParameterBinderAssociation.CommonParameters)
                {
                    ParameterSetSpecificMetadata parameterSetData;
                    string key = pair.Key;
                    string parameterDescription = this.GetParameterDescription(key);
                    bool isMandatory = false;
                    bool valueFromPipeline = false;
                    bool valueFromPipelineByPropertyName = false;
                    string position = "named";
                    int num2 = 0;
                    CompiledCommandParameter parameter2 = parameter.Parameter;
                    parameter2.ParameterSetData.TryGetValue("__AllParameterSets", out parameterSetData);
                    while ((parameterSetData == null) && (num2 < 0x20))
                    {
                        parameterSetData = parameter2.GetParameterSetData(((int) 1) << num2++);
                    }
                    if (parameterSetData != null)
                    {
                        isMandatory = parameterSetData.IsMandatory;
                        valueFromPipeline = parameterSetData.ValueFromPipeline;
                        valueFromPipelineByPropertyName = parameterSetData.ValueFromPipelineByPropertyName;
                        position = parameterSetData.IsPositional ? ((1 + parameterSetData.Position)).ToString(CultureInfo.InvariantCulture) : "named";
                    }
                    Collection<System.Attribute> compiledAttributes = parameter2.CompiledAttributes;
                    bool supportsWildcards = compiledAttributes.OfType<SupportsWildcardsAttribute>().Any<SupportsWildcardsAttribute>();
                    string help = "";
                    object obj2 = null;
                    PSDefaultValueAttribute attribute = compiledAttributes.OfType<PSDefaultValueAttribute>().FirstOrDefault<PSDefaultValueAttribute>();
                    if (attribute != null)
                    {
                        help = attribute.Help;
                        if (string.IsNullOrEmpty(help))
                        {
                            obj2 = attribute.Value;
                        }
                    }
                    if (string.IsNullOrEmpty(help))
                    {
                        RuntimeDefinedParameter parameter3;
                        if ((obj2 == null) && this.scriptBlock.RuntimeDefinedParameters.TryGetValue(key, out parameter3))
                        {
                            obj2 = parameter3.Value;
                        }
                        Compiler.DefaultValueExpressionWrapper wrapper = obj2 as Compiler.DefaultValueExpressionWrapper;
                        if (wrapper != null)
                        {
                            help = wrapper.Expression.Extent.Text;
                        }
                        else if (obj2 != null)
                        {
                            help = PSObject.ToStringParser(null, obj2);
                        }
                    }
                    XmlElement element8 = this.BuildXmlForParameter(key, isMandatory, valueFromPipeline, valueFromPipelineByPropertyName, position, parameter2.Type, parameterDescription, supportsWildcards, help, false);
                    element7.AppendChild(element8);
                }
            }
            newChild.AppendChild(element7);
            if (!string.IsNullOrEmpty(this._sections.Description))
            {
                XmlElement element9 = this.doc.CreateElement("maml:description", mamlURI);
                XmlElement element10 = this.doc.CreateElement("maml:para", mamlURI);
                XmlText text3 = this.doc.CreateTextNode(this._sections.Description);
                newChild.AppendChild(element9).AppendChild(element10).AppendChild(text3);
            }
            if (!string.IsNullOrEmpty(this._sections.Notes))
            {
                XmlElement element11 = this.doc.CreateElement("maml:alertSet", mamlURI);
                XmlElement element12 = this.doc.CreateElement("maml:alert", mamlURI);
                XmlElement element13 = this.doc.CreateElement("maml:para", mamlURI);
                XmlText text4 = this.doc.CreateTextNode(this._sections.Notes);
                newChild.AppendChild(element11).AppendChild(element12).AppendChild(element13).AppendChild(text4);
            }
            if (this._examples.Count > 0)
            {
                XmlElement element14 = this.doc.CreateElement("command:examples", commandURI);
                int num3 = 1;
                foreach (string str5 in this._examples)
                {
                    string str7;
                    string str8;
                    string str9;
                    XmlElement element15 = this.doc.CreateElement("command:example", commandURI);
                    XmlElement element16 = this.doc.CreateElement("maml:title", mamlURI);
                    string str6 = string.Format(CultureInfo.InvariantCulture, "\t\t\t\t-------------------------- {0} {1} --------------------------", new object[] { HelpDisplayStrings.ExampleUpperCase, num3++ });
                    XmlText text5 = this.doc.CreateTextNode(str6);
                    element15.AppendChild(element16).AppendChild(text5);
                    GetExampleSections(str5, out str7, out str8, out str9);
                    XmlElement element17 = this.doc.CreateElement("maml:introduction", mamlURI);
                    XmlElement element18 = this.doc.CreateElement("maml:para", mamlURI);
                    XmlText text6 = this.doc.CreateTextNode(str7);
                    element15.AppendChild(element17).AppendChild(element18).AppendChild(text6);
                    XmlElement element19 = this.doc.CreateElement("dev:code", devURI);
                    XmlText text7 = this.doc.CreateTextNode(str8);
                    element15.AppendChild(element19).AppendChild(text7);
                    XmlElement element20 = this.doc.CreateElement("dev:remarks", devURI);
                    XmlElement element21 = this.doc.CreateElement("maml:para", mamlURI);
                    XmlText text8 = this.doc.CreateTextNode(str9);
                    element15.AppendChild(element20).AppendChild(element21).AppendChild(text8);
                    for (int j = 0; j < 4; j++)
                    {
                        element20.AppendChild(this.doc.CreateElement("maml:para", mamlURI));
                    }
                    element14.AppendChild(element15);
                }
                newChild.AppendChild(element14);
            }
            if (this._inputs.Count > 0)
            {
                XmlElement element22 = this.doc.CreateElement("command:inputTypes", commandURI);
                foreach (string str10 in this._inputs)
                {
                    XmlElement element23 = this.doc.CreateElement("command:inputType", commandURI);
                    XmlElement element24 = this.doc.CreateElement("dev:type", devURI);
                    XmlElement element25 = this.doc.CreateElement("maml:name", mamlURI);
                    XmlText text9 = this.doc.CreateTextNode(str10);
                    element22.AppendChild(element23).AppendChild(element24).AppendChild(element25).AppendChild(text9);
                }
                newChild.AppendChild(element22);
            }
            IEnumerable outputType = null;
            if (this._outputs.Count > 0)
            {
                outputType = this._outputs;
            }
            else if (this.scriptBlock.OutputType.Count > 0)
            {
                outputType = this.scriptBlock.OutputType;
            }
            if (outputType != null)
            {
                XmlElement element26 = this.doc.CreateElement("command:returnValues", commandURI);
                foreach (object obj3 in outputType)
                {
                    XmlElement element27 = this.doc.CreateElement("command:returnValue", commandURI);
                    XmlElement element28 = this.doc.CreateElement("dev:type", devURI);
                    XmlElement element29 = this.doc.CreateElement("maml:name", mamlURI);
                    string str11 = (obj3 as string) ?? ((PSTypeName) obj3).Name;
                    XmlText text10 = this.doc.CreateTextNode(str11);
                    element26.AppendChild(element27).AppendChild(element28).AppendChild(element29).AppendChild(text10);
                }
                newChild.AppendChild(element26);
            }
            if (this._links.Count > 0)
            {
                XmlElement element30 = this.doc.CreateElement("maml:relatedLinks", mamlURI);
                foreach (string str12 in this._links)
                {
                    XmlElement element31 = this.doc.CreateElement("maml:navigationLink", mamlURI);
                    string qualifiedName = Uri.IsWellFormedUriString(Uri.EscapeUriString(str12), UriKind.Absolute) ? "maml:uri" : "maml:linkText";
                    XmlElement element32 = this.doc.CreateElement(qualifiedName, mamlURI);
                    XmlText text11 = this.doc.CreateTextNode(str12);
                    element30.AppendChild(element31).AppendChild(element32).AppendChild(text11);
                }
                newChild.AppendChild(element30);
            }
            return this.doc;
        }

        private static void CollectCommentText(System.Management.Automation.Language.Token comment, List<string> commentLines)
        {
            CollectCommentText(comment.Text, commentLines);
        }

        private static void CollectCommentText(string text, List<string> commentLines)
        {
            int startIndex = 0;
            if (text[0] != '<')
            {
                while (startIndex < text.Length)
                {
                    if (text[startIndex] != '#')
                    {
                        break;
                    }
                    startIndex++;
                }
            }
            else
            {
                int num2 = 2;
                startIndex = 2;
                while (startIndex < (text.Length - 2))
                {
                    if (text[startIndex] == '\n')
                    {
                        commentLines.Add(text.Substring(num2, startIndex - num2));
                        num2 = startIndex + 1;
                    }
                    else if (text[startIndex] == '\r')
                    {
                        commentLines.Add(text.Substring(num2, startIndex - num2));
                        if (text[startIndex + 1] == '\n')
                        {
                            startIndex++;
                        }
                        num2 = startIndex + 1;
                    }
                    startIndex++;
                }
                commentLines.Add(text.Substring(num2, startIndex - num2));
                return;
            }
            commentLines.Add(text.Substring(startIndex));
        }

        internal static HelpInfo CreateFromComments(ExecutionContext context, CommandInfo commandInfo, HelpCommentsParser helpCommentsParser, bool dontSearchOnRemoteComputer)
        {
            if (!dontSearchOnRemoteComputer)
            {
                RemoteHelpInfo remoteHelpInfo = helpCommentsParser.GetRemoteHelpInfo(context, commandInfo);
                if (remoteHelpInfo != null)
                {
                    if (remoteHelpInfo.GetUriForOnlineHelp() == null)
                    {
                        DefaultCommandHelpObjectBuilder.AddRelatedLinksProperties(remoteHelpInfo.FullHelp, commandInfo.CommandMetadata.HelpUri);
                    }
                    return remoteHelpInfo;
                }
            }
            XmlDocument document = helpCommentsParser.BuildXmlFromComments();
            HelpCategory helpCategory = commandInfo.HelpCategory;
            MamlCommandHelpInfo helpInfo = MamlCommandHelpInfo.Load(document.DocumentElement, helpCategory);
            if (helpInfo != null)
            {
                helpCommentsParser.SetAdditionalData(helpInfo);
                if (!string.IsNullOrEmpty(helpCommentsParser._sections.ForwardHelpTargetName) || !string.IsNullOrEmpty(helpCommentsParser._sections.ForwardHelpCategory))
                {
                    if (string.IsNullOrEmpty(helpCommentsParser._sections.ForwardHelpTargetName))
                    {
                        helpInfo.ForwardTarget = helpInfo.Name;
                    }
                    else
                    {
                        helpInfo.ForwardTarget = helpCommentsParser._sections.ForwardHelpTargetName;
                    }
                    if (!string.IsNullOrEmpty(helpCommentsParser._sections.ForwardHelpCategory))
                    {
                        try
                        {
                            helpInfo.ForwardHelpCategory = (HelpCategory) Enum.Parse(typeof(HelpCategory), helpCommentsParser._sections.ForwardHelpCategory, true);
                        }
                        catch (ArgumentException)
                        {
                        }
                    }
                    else
                    {
                        helpInfo.ForwardHelpCategory = HelpCategory.Workflow | HelpCategory.ExternalScript | HelpCategory.Filter | HelpCategory.Function | HelpCategory.ScriptCommand | HelpCategory.Cmdlet | HelpCategory.Alias;
                    }
                }
                WorkflowInfo info3 = commandInfo as WorkflowInfo;
                if (info3 != null)
                {
                    bool flag = DefaultCommandHelpObjectBuilder.HasCommonParameters(commandInfo.Parameters);
                    bool flag2 = (commandInfo.CommandType & CommandTypes.Workflow) == CommandTypes.Workflow;
                    helpInfo.FullHelp.Properties.Add(new PSNoteProperty("CommonParameters", flag));
                    helpInfo.FullHelp.Properties.Add(new PSNoteProperty("WorkflowCommonParameters", flag2));
                    DefaultCommandHelpObjectBuilder.AddDetailsProperties(helpInfo.FullHelp, info3.Name, info3.Noun, info3.Verb, "MamlCommandHelpInfo", helpInfo.Synopsis);
                    DefaultCommandHelpObjectBuilder.AddSyntaxProperties(helpInfo.FullHelp, info3.Name, info3.ParameterSets, flag, flag2, "MamlCommandHelpInfo");
                }
                if (helpInfo.GetUriForOnlineHelp() == null)
                {
                    DefaultCommandHelpObjectBuilder.AddRelatedLinksProperties(helpInfo.FullHelp, commandInfo.CommandMetadata.HelpUri);
                }
            }
            return helpInfo;
        }

        internal static HelpInfo CreateFromComments(ExecutionContext context, CommandInfo commandInfo, List<System.Management.Automation.Language.Token> comments, List<string> parameterDescriptions, bool dontSearchOnRemoteComputer, out string helpFile, out string helpUriFromDotLink)
        {
            HelpCommentsParser helpCommentsParser = new HelpCommentsParser(commandInfo, parameterDescriptions);
            helpCommentsParser.AnalyzeCommentBlock(comments);
            if ((helpCommentsParser._sections.Links != null) && (helpCommentsParser._sections.Links.Count != 0))
            {
                helpUriFromDotLink = helpCommentsParser._sections.Links[0];
            }
            else
            {
                helpUriFromDotLink = null;
            }
            helpFile = helpCommentsParser.GetHelpFile(commandInfo);
            if (((comments.Count == 1) && helpCommentsParser.isExternalHelpSet) && (helpFile == null))
            {
                return null;
            }
            return CreateFromComments(context, commandInfo, helpCommentsParser, dontSearchOnRemoteComputer);
        }

        private void DetermineParameterDescriptions()
        {
            int num = 0;
            foreach (string str in this.commandMetadata.StaticCommandParameterMetadata.BindableParameters.Keys)
            {
                string str2;
                if (!this._parameters.TryGetValue(str.ToUpperInvariant(), out str2) && (num < this.parameterDescriptions.Count))
                {
                    this._parameters.Add(str.ToUpperInvariant(), this.parameterDescriptions[num]);
                }
                num++;
            }
        }

        private static int FirstTokenInExtent(System.Management.Automation.Language.Token[] tokens, IScriptExtent extent, int startIndex = 0)
        {
            int index = startIndex;
            while (index < tokens.Length)
            {
                if (!tokens[index].Extent.IsBefore(extent))
                {
                    return index;
                }
                index++;
            }
            return index;
        }

        private static List<System.Management.Automation.Language.Token> GetCommentBlock(System.Management.Automation.Language.Token[] tokens, ref int startIndex)
        {
            List<System.Management.Automation.Language.Token> list = new List<System.Management.Automation.Language.Token>();
            int num = 0x7fffffff;
            for (int i = startIndex; i < tokens.Length; i++)
            {
                System.Management.Automation.Language.Token item = tokens[i];
                if (item.Extent.StartLineNumber > num)
                {
                    startIndex = i;
                    return list;
                }
                if (item.Kind == TokenKind.Comment)
                {
                    list.Add(item);
                    num = item.Extent.EndLineNumber + 1;
                }
                else if (item.Kind != TokenKind.NewLine)
                {
                    startIndex = i;
                    return list;
                }
            }
            return list;
        }

        private static void GetExampleSections(string content, out string prompt_str, out string code_str, out string remarks_str)
        {
            prompt_str = code_str = "";
            StringBuilder builder = new StringBuilder();
            int num = 1;
            foreach (char ch in content)
            {
                if ((ch == '>') && (num == 1))
                {
                    builder.Append(ch);
                    prompt_str = builder.ToString().Trim();
                    builder = new StringBuilder();
                    num++;
                }
                else if ((ch == '\n') && (num < 3))
                {
                    if (num == 1)
                    {
                        prompt_str = @"C:\PS>";
                    }
                    code_str = builder.ToString().Trim();
                    builder = new StringBuilder();
                    num = 3;
                }
                else
                {
                    builder.Append(ch);
                }
            }
            if (num == 1)
            {
                prompt_str = @"C:\PS>";
                code_str = builder.ToString().Trim();
                remarks_str = "";
            }
            else
            {
                remarks_str = builder.ToString();
            }
        }

        internal static Tuple<List<System.Management.Automation.Language.Token>, List<string>> GetHelpCommentTokens(IParameterMetadataProvider ipmp, Dictionary<Ast, System.Management.Automation.Language.Token[]> scriptBlockTokenCache)
        {
            int num;
            int num2;
            int num3;
            List<System.Management.Automation.Language.Token> commentBlock;
            Ast ast = (Ast) ipmp;
            Ast key = ast;
            while (key.Parent != null)
            {
                key = key.Parent;
            }
            System.Management.Automation.Language.Token[] tokenArray = null;
            scriptBlockTokenCache.TryGetValue(key, out tokenArray);
            if (tokenArray == null)
            {
                ParseError[] errorArray;
                Parser.ParseInput(key.Extent.Text, out tokenArray, out errorArray);
                scriptBlockTokenCache[key] = tokenArray;
            }
            FunctionDefinitionAst ast3 = ast as FunctionDefinitionAst;
            if (ast3 != null)
            {
                int tokenIndex = num = FirstTokenInExtent(tokenArray, ast.Extent, 0);
                commentBlock = GetPrecedingCommentBlock(tokenArray, tokenIndex, 2);
                if (IsCommentHelpText(commentBlock))
                {
                    return Tuple.Create<List<System.Management.Automation.Language.Token>, List<string>>(commentBlock, GetParameterComments(tokenArray, ipmp, num));
                }
                num2 = FirstTokenInExtent(tokenArray, ast3.Body.Extent, 0) + 1;
                num3 = LastTokenInExtent(tokenArray, ast.Extent, tokenIndex);
            }
            else if (ast == key)
            {
                num2 = num = 0;
                num3 = tokenArray.Length - 1;
            }
            else
            {
                num2 = num = FirstTokenInExtent(tokenArray, ast.Extent, 0) - 1;
                num3 = LastTokenInExtent(tokenArray, ast.Extent, num2);
            }
            do
            {
                commentBlock = GetCommentBlock(tokenArray, ref num2);
                if (commentBlock.Count == 0)
                {
                    goto Label_0195;
                }
            }
            while (!IsCommentHelpText(commentBlock));
            if (ast == key)
            {
                NamedBlockAst endBlock = ((ScriptBlockAst) ast).EndBlock;
                if ((endBlock == null) || !endBlock.Unnamed)
                {
                    return Tuple.Create<List<System.Management.Automation.Language.Token>, List<string>>(commentBlock, GetParameterComments(tokenArray, ipmp, num));
                }
                StatementAst ast5 = endBlock.Statements.FirstOrDefault<StatementAst>();
                if (ast5 is FunctionDefinitionAst)
                {
                    int num5 = ast5.Extent.StartLineNumber - commentBlock.Last<System.Management.Automation.Language.Token>().Extent.EndLineNumber;
                    if (num5 > 2)
                    {
                        return Tuple.Create<List<System.Management.Automation.Language.Token>, List<string>>(commentBlock, GetParameterComments(tokenArray, ipmp, num));
                    }
                    goto Label_0195;
                }
            }
            return Tuple.Create<List<System.Management.Automation.Language.Token>, List<string>>(commentBlock, GetParameterComments(tokenArray, ipmp, num));
        Label_0195:
            commentBlock = GetPrecedingCommentBlock(tokenArray, num3, tokenArray[num3].Extent.StartLineNumber);
            if (IsCommentHelpText(commentBlock))
            {
                return Tuple.Create<List<System.Management.Automation.Language.Token>, List<string>>(commentBlock, GetParameterComments(tokenArray, ipmp, num));
            }
            return null;
        }

        internal static CommentHelpInfo GetHelpContents(List<System.Management.Automation.Language.Token> comments, List<string> parameterDescriptions)
        {
            HelpCommentsParser parser = new HelpCommentsParser(parameterDescriptions);
            parser.AnalyzeCommentBlock(comments);
            return parser._sections;
        }

        internal string GetHelpFile(CommandInfo commandInfo)
        {
            if (this._sections.MamlHelpFile == null)
            {
                return null;
            }
            string mamlHelpFile = this._sections.MamlHelpFile;
            Collection<string> searchPaths = new Collection<string>();
            string file = ((IScriptCommandInfo) commandInfo).ScriptBlock.File;
            if (!string.IsNullOrEmpty(file))
            {
                mamlHelpFile = Path.Combine(Path.GetDirectoryName(file), this._sections.MamlHelpFile);
            }
            else if (commandInfo.Module != null)
            {
                mamlHelpFile = Path.Combine(Path.GetDirectoryName(commandInfo.Module.Path), this._sections.MamlHelpFile);
            }
            return MUIFileSearcher.LocateFile(mamlHelpFile, searchPaths);
        }

        private static List<string> GetParameterComments(System.Management.Automation.Language.Token[] tokens, IParameterMetadataProvider ipmp, int startIndex)
        {
            List<string> list = new List<string>();
            ReadOnlyCollection<ParameterAst> parameters = ipmp.Parameters;
            if ((parameters != null) && (parameters.Count != 0))
            {
                foreach (ParameterAst ast in parameters)
                {
                    List<string> commentLines = new List<string>();
                    int tokenIndex = FirstTokenInExtent(tokens, ast.Extent, startIndex);
                    List<System.Management.Automation.Language.Token> commentBlock = GetPrecedingCommentBlock(tokens, tokenIndex, 2);
                    if (commentBlock != null)
                    {
                        foreach (System.Management.Automation.Language.Token token in commentBlock)
                        {
                            CollectCommentText(token, commentLines);
                        }
                    }
                    int num2 = LastTokenInExtent(tokens, ast.Extent, tokenIndex);
                    for (int i = tokenIndex; i < num2; i++)
                    {
                        if (tokens[i].Kind == TokenKind.Comment)
                        {
                            CollectCommentText(tokens[i], commentLines);
                        }
                    }
                    num2++;
                    commentBlock = GetCommentBlock(tokens, ref num2);
                    if (commentBlock != null)
                    {
                        foreach (System.Management.Automation.Language.Token token2 in commentBlock)
                        {
                            CollectCommentText(token2, commentLines);
                        }
                    }
                    int num4 = -1;
                    list.Add(GetSection(commentLines, ref num4));
                }
            }
            return list;
        }

        private string GetParameterDescription(string parameterName)
        {
            string str;
            this._parameters.TryGetValue(parameterName.ToUpperInvariant(), out str);
            return str;
        }

        private static List<System.Management.Automation.Language.Token> GetPrecedingCommentBlock(System.Management.Automation.Language.Token[] tokens, int tokenIndex, int proximity)
        {
            List<System.Management.Automation.Language.Token> list = new List<System.Management.Automation.Language.Token>();
            int num = tokens[tokenIndex].Extent.StartLineNumber - proximity;
            for (int i = tokenIndex - 1; i >= 0; i--)
            {
                System.Management.Automation.Language.Token item = tokens[i];
                if (item.Extent.EndLineNumber < num)
                {
                    break;
                }
                if (item.Kind == TokenKind.Comment)
                {
                    list.Add(item);
                    num = item.Extent.StartLineNumber - 1;
                }
                else if (item.Kind != TokenKind.NewLine)
                {
                    break;
                }
            }
            list.Reverse();
            return list;
        }

        internal RemoteHelpInfo GetRemoteHelpInfo(ExecutionContext context, CommandInfo commandInfo)
        {
            PSSession session;
            if (string.IsNullOrEmpty(this._sections.ForwardHelpTargetName) || string.IsNullOrEmpty(this._sections.RemoteHelpRunspace))
            {
                return null;
            }
            IScriptCommandInfo info = (IScriptCommandInfo) commandInfo;
            object valueToConvert = info.ScriptBlock.SessionState.PSVariable.GetValue(this._sections.RemoteHelpRunspace);
            if ((valueToConvert == null) || !LanguagePrimitives.TryConvertTo<PSSession>(valueToConvert, out session))
            {
                throw new InvalidOperationException(HelpErrors.RemoteRunspaceNotAvailable);
            }
            return new RemoteHelpInfo(context, (RemoteRunspace) session.Runspace, commandInfo.Name, this._sections.ForwardHelpTargetName, this._sections.ForwardHelpCategory, commandInfo.HelpCategory);
        }

        private static string GetSection(List<string> commentLines, ref int i)
        {
            bool flag = false;
            int num = 0;
            StringBuilder builder = new StringBuilder();
            i++;
            while (i < commentLines.Count)
            {
                string input = commentLines[i];
                if (flag || !Regex.IsMatch(input, @"^\s*$"))
                {
                    if (Regex.IsMatch(input, @"^\s*\.(\w+)(\s+(\S.*))?\s*$"))
                    {
                        i--;
                        break;
                    }
                    if (!flag)
                    {
                        for (int j = 0; (j < input.Length) && (((input[j] == ' ') || (input[j] == '\t')) || (input[j] == '\x00a0')); j++)
                        {
                            num++;
                        }
                    }
                    flag = true;
                    int startIndex = 0;
                    while (((startIndex < input.Length) && (startIndex < num)) && (((input[startIndex] == ' ') || (input[startIndex] == '\t')) || (input[startIndex] == '\x00a0')))
                    {
                        startIndex++;
                    }
                    builder.Append(input.Substring(startIndex));
                    builder.Append('\n');
                }
                i++;
            }
            return builder.ToString();
        }

        internal static bool IsCommentHelpText(List<System.Management.Automation.Language.Token> commentBlock)
        {
            if ((commentBlock == null) || (commentBlock.Count == 0))
            {
                return false;
            }
            HelpCommentsParser parser = new HelpCommentsParser();
            return parser.AnalyzeCommentBlock(commentBlock);
        }

        private static int LastTokenInExtent(System.Management.Automation.Language.Token[] tokens, IScriptExtent extent, int startIndex)
        {
            int index = startIndex;
            while (index < tokens.Length)
            {
                if (tokens[index].Extent.IsAfter(extent))
                {
                    break;
                }
                index++;
            }
            return (index - 1);
        }

        internal void SetAdditionalData(MamlCommandHelpInfo helpInfo)
        {
            helpInfo.SetAdditionalDataFromHelpComment(this._sections.Component, this._sections.Functionality, this._sections.Role);
        }
    }
}

