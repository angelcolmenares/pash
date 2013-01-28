namespace System.Management.Automation
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.Management.Automation.Language;
    using System.Runtime.CompilerServices;
    using System.Text.RegularExpressions;

    internal class ExportVisitor : AstVisitor
    {
        internal ExportVisitor()
        {
            this.DiscoveredExports = new List<string>();
            this.DiscoveredFunctions = new Dictionary<string, FunctionDefinitionAst>(StringComparer.OrdinalIgnoreCase);
            this.DiscoveredAliases = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            this.DiscoveredModules = new List<RequiredModuleInfo>();
            this.DiscoveredCommandFilters = new List<string>();
        }

        private List<string> ExtractArgumentList(string arguments)
        {
            char[] trimChars = new char[] { '\'', '"', ' ', '\t' };
            List<string> list = new List<string>();
            if (!string.IsNullOrEmpty(arguments))
            {
                foreach (string str in arguments.Split(new char[] { ',' }))
                {
                    list.Add(str.Trim(trimChars));
                }
            }
            return list;
        }

        private string GetParameterByNameOrPosition(string name, int position, CommandAst commandAst)
        {
            Dictionary<string, string> parameters = this.GetParameters(commandAst.CommandElements);
            string str = null;
            if (!parameters.TryGetValue(name, out str))
            {
                parameters.TryGetValue(position.ToString(CultureInfo.InvariantCulture), out str);
            }
            return str;
        }

        private Dictionary<string, string> GetParameters(ReadOnlyCollection<CommandElementAst> commandElements)
        {
            Dictionary<string, string> dictionary = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            bool flag = false;
            string key = null;
            int num = 0;
            foreach (CommandElementAst ast in commandElements)
            {
                if (!flag)
                {
                    flag = true;
                }
                else
                {
                    CommandParameterAst ast2 = ast as CommandParameterAst;
                    if (ast2 != null)
                    {
                        string parameterName = ast2.ParameterName;
                        if (ast2.Argument != null)
                        {
                            dictionary.Add(parameterName, ast2.Argument.ToString());
                            key = null;
                        }
                        else
                        {
                            key = parameterName;
                        }
                    }
                    else if (key != null)
                    {
                        ArrayExpressionAst ast3 = ast as ArrayExpressionAst;
                        if (ast3 != null)
                        {
                            dictionary.Add(key, ast3.SubExpression.ToString());
                        }
                        else
                        {
                            dictionary.Add(key, ast.ToString());
                        }
                        key = null;
                    }
                    else
                    {
                        dictionary.Add(num.ToString(CultureInfo.InvariantCulture), ast.ToString());
                        num++;
                    }
                }
            }
            return dictionary;
        }

        private List<string> ProcessExportedCommandList(string declaration)
        {
            List<string> list = this.ExtractArgumentList(declaration);
            List<string> list2 = new List<string>();
            foreach (string str in list)
            {
                list2.Add(str);
            }
            return list2;
        }

        public override AstVisitAction VisitAssignmentStatement(AssignmentStatementAst assignmentStatementAst)
        {
            if (string.Equals("$env:PATH", assignmentStatementAst.Left.ToString(), StringComparison.OrdinalIgnoreCase) && Regex.IsMatch(assignmentStatementAst.Right.ToString(), @"\$psScriptRoot", RegexOptions.IgnoreCase))
            {
                ModuleIntrinsics.Tracer.WriteLine("Module adds itself to the path.", new object[0]);
                this.AddsSelfToPath = true;
            }
            return AstVisitAction.Continue;
        }

        public override AstVisitAction VisitCommand(CommandAst commandAst)
        {
            string commandName = commandAst.GetCommandName();
            if (string.IsNullOrEmpty(commandName))
            {
                commandName = commandAst.CommandElements[0].ToString().Trim(new char[] { '"', '\'' });
            }
            if (commandAst.InvocationOperator == TokenKind.Dot)
            {
                commandName = Regex.Replace(commandName, @"\$[^\\]*\\", "", RegexOptions.IgnoreCase);
                RequiredModuleInfo item = new RequiredModuleInfo {
                    Name = commandName,
                    CommandsToPostFilter = new List<string>()
                };
                this.DiscoveredModules.Add(item);
                ModuleIntrinsics.Tracer.WriteLine("Module dots " + commandName, new object[0]);
            }
            if (((string.Equals(commandName, "New-Alias", StringComparison.OrdinalIgnoreCase) || string.Equals(commandName, @"Microsoft.PowerShell.Utility\New-Alias", StringComparison.OrdinalIgnoreCase)) || (string.Equals(commandName, "Set-Alias", StringComparison.OrdinalIgnoreCase) || string.Equals(commandName, @"Microsoft.PowerShell.Utility\Set-Alias", StringComparison.OrdinalIgnoreCase))) || (string.Equals(commandName, "nal", StringComparison.OrdinalIgnoreCase) || string.Equals(commandName, "sal", StringComparison.OrdinalIgnoreCase)))
            {
                string str2 = this.GetParameterByNameOrPosition("Name", 0, commandAst);
                string str3 = this.GetParameterByNameOrPosition("Value", 1, commandAst);
                if (!string.IsNullOrEmpty(str2))
                {
                    this.DiscoveredAliases[str2] = str3;
                    ModuleIntrinsics.Tracer.WriteLine("Module defines alias: " + str2 + "=" + str3, new object[0]);
                }
            }
            if (string.Equals(commandName, "Import-Module", StringComparison.OrdinalIgnoreCase) || string.Equals(commandName, "ipmo", StringComparison.OrdinalIgnoreCase))
            {
                List<string> list = new List<string>();
                string str4 = this.GetParameterByNameOrPosition("Function", -1, commandAst);
                if (!string.IsNullOrEmpty(str4))
                {
                    list.AddRange(this.ProcessExportedCommandList(str4));
                }
                string str5 = this.GetParameterByNameOrPosition("Cmdlet", -1, commandAst);
                if (!string.IsNullOrEmpty(str5))
                {
                    list.AddRange(this.ProcessExportedCommandList(str5));
                }
                string str6 = this.GetParameterByNameOrPosition("Alias", -1, commandAst);
                if (!string.IsNullOrEmpty(str6))
                {
                    list.AddRange(this.ProcessExportedCommandList(str6));
                }
                string str7 = this.GetParameterByNameOrPosition("Name", 0, commandAst);
                if (!string.IsNullOrEmpty(str7))
                {
                    foreach (string str8 in str7.Split(new char[] { ',' }))
                    {
                        ModuleIntrinsics.Tracer.WriteLine("Discovered module import: " + str8, new object[0]);
                        RequiredModuleInfo info2 = new RequiredModuleInfo {
                            Name = str8.Trim(),
                            CommandsToPostFilter = list
                        };
                        this.DiscoveredModules.Add(info2);
                    }
                }
            }
            if ((string.Equals(commandName, "Export-ModuleMember", StringComparison.OrdinalIgnoreCase) || string.Equals(commandName, @"Microsoft.PowerShell.Core\Export-ModuleMember", StringComparison.OrdinalIgnoreCase)) || string.Equals(commandName, "$script:ExportModuleMember", StringComparison.OrdinalIgnoreCase))
            {
                List<string> list2 = new List<string>();
                string arguments = this.GetParameterByNameOrPosition("Function", 0, commandAst);
                list2.AddRange(this.ExtractArgumentList(arguments));
                string str10 = this.GetParameterByNameOrPosition("Cmdlet", -1, commandAst);
                list2.AddRange(this.ExtractArgumentList(str10));
                foreach (string str11 in list2)
                {
                    this.DiscoveredCommandFilters.Add(str11);
                    ModuleIntrinsics.Tracer.WriteLine("Discovered explicit export: " + str11, new object[0]);
                    if (!WildcardPattern.ContainsWildcardCharacters(str11) && !this.DiscoveredExports.Contains(str11))
                    {
                        this.DiscoveredExports.Add(str11);
                    }
                }
                list2 = new List<string>();
                string str12 = this.GetParameterByNameOrPosition("Alias", -1, commandAst);
                list2.AddRange(this.ExtractArgumentList(str12));
                foreach (string str13 in list2)
                {
                    this.DiscoveredCommandFilters.Add(str13);
                    if (!WildcardPattern.ContainsWildcardCharacters(str13) && !this.DiscoveredAliases.ContainsKey(str13))
                    {
                        this.DiscoveredAliases.Add(str13, null);
                    }
                }
            }
            if (string.Equals(commandName, "public", StringComparison.OrdinalIgnoreCase) && (commandAst.CommandElements.Count > 2))
            {
                string str14 = commandAst.CommandElements[2].ToString().Trim();
                this.DiscoveredExports.Add(str14);
                this.DiscoveredCommandFilters.Add(str14);
            }
            return AstVisitAction.Continue;
        }

        public override AstVisitAction VisitFunctionDefinition(FunctionDefinitionAst functionDefinitionAst)
        {
            if (!this.DiscoveredFunctions.ContainsKey(functionDefinitionAst.Name))
            {
                this.DiscoveredFunctions.Add(functionDefinitionAst.Name, functionDefinitionAst);
                ModuleIntrinsics.Tracer.WriteLine("Discovered function definition: " + functionDefinitionAst.Name, new object[0]);
            }
            for (Ast ast = functionDefinitionAst.Parent; ast != null; ast = ast.Parent)
            {
                if (ast is FunctionDefinitionAst)
                {
                    return AstVisitAction.Continue;
                }
            }
            this.DiscoveredExports.Add(functionDefinitionAst.Name);
            return AstVisitAction.Continue;
        }

        internal bool AddsSelfToPath { get; set; }

        internal Dictionary<string, string> DiscoveredAliases { get; set; }

        internal List<string> DiscoveredCommandFilters { get; set; }

        internal List<string> DiscoveredExports { get; set; }

        internal Dictionary<string, FunctionDefinitionAst> DiscoveredFunctions { get; set; }

        internal List<RequiredModuleInfo> DiscoveredModules { get; set; }
    }
}

