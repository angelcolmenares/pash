namespace System.Management.Automation
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Management.Automation.Language;
    using System.Runtime.CompilerServices;

    internal class CompletionContext
    {
        internal bool GetOption(string option, bool @default)
        {
            if ((this.Options != null) && this.Options.ContainsKey(option))
            {
                return LanguagePrimitives.ConvertTo<bool>(this.Options[option]);
            }
            return @default;
        }

        internal IScriptPosition CursorPosition { get; set; }

        internal System.Management.Automation.ExecutionContext ExecutionContext { get; set; }

        internal CompletionExecutionHelper Helper { get; set; }

        internal Hashtable Options { get; set; }

        internal System.Management.Automation.Language.PseudoBindingInfo PseudoBindingInfo { get; set; }

        internal List<Ast> RelatedAsts { get; set; }

        internal int ReplacementIndex { get; set; }

        internal int ReplacementLength { get; set; }

        internal Token TokenAtCursor { get; set; }

        internal Token TokenBeforeCursor { get; set; }

        internal string WordToComplete { get; set; }
    }
}

