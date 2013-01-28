namespace System.Management.Automation
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Management.Automation.Language;
    using System.Runtime.CompilerServices;

    [Serializable]
    internal class ScriptAnalysis
    {
        internal ScriptAnalysis(string path)
        {
            ModuleIntrinsics.Tracer.WriteLine("Analyzing path: " + path, new object[0]);
            string input = File.ReadAllText(path);
            Token[] tokens = null;
            ParseError[] errors = null;
            Ast ast = Parser.ParseInput(input, out tokens, out errors);
            ExportVisitor astVisitor = new ExportVisitor();
            ast.Visit(astVisitor);
            this.DiscoveredExports = astVisitor.DiscoveredExports;
            this.DiscoveredAliases = new Dictionary<string, string>();
            this.DiscoveredModules = astVisitor.DiscoveredModules;
            this.DiscoveredCommandFilters = astVisitor.DiscoveredCommandFilters;
            if (this.DiscoveredCommandFilters.Count == 0)
            {
                this.DiscoveredCommandFilters.Add("*");
            }
            else
            {
                List<WildcardPattern> patterns = new List<WildcardPattern>();
                foreach (string str2 in this.DiscoveredCommandFilters)
                {
                    patterns.Add(new WildcardPattern(str2));
                }
                foreach (string str3 in astVisitor.DiscoveredAliases.Keys)
                {
                    if (SessionStateUtilities.MatchesAnyWildcardPattern(str3, patterns, false))
                    {
                        this.DiscoveredAliases[str3] = astVisitor.DiscoveredAliases[str3];
                    }
                }
            }
            this.AddsSelfToPath = astVisitor.AddsSelfToPath;
        }

        internal bool AddsSelfToPath { get; set; }

        internal Dictionary<string, string> DiscoveredAliases { get; set; }

        internal List<string> DiscoveredCommandFilters { get; set; }

        internal List<string> DiscoveredExports { get; set; }

        internal List<RequiredModuleInfo> DiscoveredModules { get; set; }
    }
}

