namespace System.Management.Automation
{
    using Microsoft.CSharp.RuntimeBinder;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.Linq.Expressions;
    using System.Management.Automation.Language;
    using System.Management.Automation.Remoting.Client;
    using System.Management.Automation.Runspaces;
    using System.Management.Automation.Runspaces.Internal;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text.RegularExpressions;

    public class CommandCompletion
    {
		internal static readonly IList<CompletionResult> EmptyCompletionResult = new CompletionResult[0];
        private static readonly CommandCompletion EmptyCommandCompletion = new CommandCompletion(new Collection<CompletionResult>(EmptyCompletionResult), -1, -1, -1);
        
        internal CommandCompletion(Collection<CompletionResult> matches, int currentMatchIndex, int replacementIndex, int replacementLength)
        {
            this.CompletionMatches = matches;
            this.CurrentMatchIndex = currentMatchIndex;
            this.ReplacementIndex = replacementIndex;
            this.ReplacementLength = replacementLength;
        }

        private static CommandCompletion CallScriptWithAstParameterSet(Ast ast, Token[] tokens, IScriptPosition cursorPosition, Hashtable options, PowerShell powershell)
        {
            try
            {
                powershell.Commands.Clear();
                powershell.AddCommand("TabExpansion2").AddArgument(ast).AddArgument(tokens).AddArgument(cursorPosition).AddArgument(options);
                Collection<PSObject> collection = powershell.Invoke();
                if (collection == null)
                {
                    return EmptyCommandCompletion;
                }
                if (collection.Count == 1)
                {
                    CommandCompletion completion = PSObject.Base(collection[0]) as CommandCompletion;
                    if (completion != null)
                    {
                        return completion;
                    }
                }
            }
            catch (Exception exception)
            {
                CommandProcessorBase.CheckForSevereException(exception);
            }
            finally
            {
                powershell.Commands.Clear();
            }
            return EmptyCommandCompletion;
        }

        private static CommandCompletion CallScriptWithStringParameterSet(string input, int cursorIndex, Hashtable options, PowerShell powershell)
        {
            try
            {
                powershell.Commands.Clear();
                powershell.AddCommand("TabExpansion2").AddArgument(input).AddArgument(cursorIndex).AddArgument(options);
                Collection<PSObject> collection = powershell.Invoke();
                if (collection == null)
                {
                    return EmptyCommandCompletion;
                }
                if (collection.Count == 1)
                {
                    CommandCompletion completion = PSObject.Base(collection[0]) as CommandCompletion;
                    if (completion != null)
                    {
                        return completion;
                    }
                }
            }
            catch (Exception exception)
            {
                CommandProcessorBase.CheckForSevereException(exception);
            }
            finally
            {
                powershell.Commands.Clear();
            }
            return EmptyCommandCompletion;
        }

        private static void CheckScriptCallOnRemoteRunspace(RemoteRunspace remoteRunspace)
        {
            RemoteRunspacePoolInternal remoteRunspacePoolInternal = remoteRunspace.RunspacePool.RemoteRunspacePoolInternal;
            if (remoteRunspacePoolInternal != null)
            {
                BaseClientSessionTransportManager transportManager = remoteRunspacePoolInternal.DataStructureHandler.TransportManager;
                if ((transportManager != null) && (transportManager.TypeTable == null))
                {
                    throw PSTraceSource.NewInvalidOperationException("TabCompletionStrings", "CannotDeserializeTabCompletionResult", new object[0]);
                }
            }
        }

        public static CommandCompletion CompleteInput(string input, int cursorIndex, Hashtable options)
        {
            if (input == null)
            {
                return EmptyCommandCompletion;
            }
            Tuple<Ast, Token[], IScriptPosition> tuple = MapStringInputToParsedInput(input, cursorIndex);
            return CompleteInputImpl(tuple.Item1, tuple.Item2, tuple.Item3, options);
        }

        public static CommandCompletion CompleteInput(Ast ast, Token[] tokens, IScriptPosition positionOfCursor, Hashtable options)
        {
            if (ast == null)
            {
                throw PSTraceSource.NewArgumentNullException("ast");
            }
            if (tokens == null)
            {
                throw PSTraceSource.NewArgumentNullException("tokens");
            }
            if (positionOfCursor == null)
            {
                throw PSTraceSource.NewArgumentNullException("positionOfCursor");
            }
            return CompleteInputImpl(ast, tokens, positionOfCursor, options);
        }

        public static CommandCompletion CompleteInput(string input, int cursorIndex, Hashtable options, PowerShell powershell)
        {
            if (input == null)
            {
                return EmptyCommandCompletion;
            }
            if (cursorIndex > input.Length)
            {
                throw PSTraceSource.NewArgumentException("cursorIndex");
            }
            if (powershell == null)
            {
                throw PSTraceSource.NewArgumentNullException("powershell");
            }
			int num;
			int num2;
            if (!powershell.IsChild)
            {
                RemoteRunspace remoteRunspace = powershell.Runspace as RemoteRunspace;
                if (remoteRunspace != null)
                {
                    CheckScriptCallOnRemoteRunspace(remoteRunspace);
                    if (remoteRunspace.GetCapabilities().Equals(RunspaceCapability.Default))
                    {
                        powershell.Commands.Clear();
                        return new CommandCompletion(new Collection<CompletionResult>(InvokeLegacyTabExpansion(powershell, input, cursorIndex, true, out num, out num2) ?? EmptyCompletionResult), -1, num, num2);
                    }
                }
            }
			return new CommandCompletion(new Collection<CompletionResult>(InvokeLegacyTabExpansion(powershell, input, cursorIndex, true, out num, out num2) ?? EmptyCompletionResult), -1, num, num2);
			//TODO: Trying with legacy

			//return CallScriptWithStringParameterSet(input, cursorIndex, options, powershell);
        }

        public static CommandCompletion CompleteInput(Ast ast, Token[] tokens, IScriptPosition cursorPosition, Hashtable options, PowerShell powershell)
        {
            if (ast == null)
            {
                throw PSTraceSource.NewArgumentNullException("ast");
            }
            if (tokens == null)
            {
                throw PSTraceSource.NewArgumentNullException("tokens");
            }
            if (cursorPosition == null)
            {
                throw PSTraceSource.NewArgumentNullException("cursorPosition");
            }
            if (powershell == null)
            {
                throw PSTraceSource.NewArgumentNullException("powershell");
            }
            if (!powershell.IsChild)
            {
                RemoteRunspace remoteRunspace = powershell.Runspace as RemoteRunspace;
                if (remoteRunspace != null)
                {
                    CheckScriptCallOnRemoteRunspace(remoteRunspace);
                    if (remoteRunspace.GetCapabilities().Equals(RunspaceCapability.Default))
                    {
                        int num;
                        int num2;
                        powershell.Commands.Clear();
                        Tuple<string, int, int> inputAndCursorFromAst = GetInputAndCursorFromAst(cursorPosition);
                        return new CommandCompletion(new Collection<CompletionResult>(InvokeLegacyTabExpansion(powershell, inputAndCursorFromAst.Item1, inputAndCursorFromAst.Item2, true, out num, out num2) ?? EmptyCompletionResult), -1, num + inputAndCursorFromAst.Item3, num2);
                    }
                    string text = ast.Extent.Text;
                    int offset = ((InternalScriptPosition) cursorPosition).Offset;
                    return CallScriptWithStringParameterSet(text, offset, options, powershell);
                }
            }
            return CallScriptWithAstParameterSet(ast, tokens, cursorPosition, options, powershell);
        }

        private static CommandCompletion CompleteInputImpl(Ast ast, Token[] tokens, IScriptPosition positionOfCursor, Hashtable options)
        {
            PowerShell powershell = PowerShell.Create(RunspaceMode.CurrentRunspace);
            int replacementIndex = -1;
            int replacementLength = -1;
            List<CompletionResult> list = null;
            if (NeedToInvokeLegacyTabExpansion(powershell))
            {
                Tuple<string, int, int> inputAndCursorFromAst = GetInputAndCursorFromAst(positionOfCursor);
                list = InvokeLegacyTabExpansion(powershell, inputAndCursorFromAst.Item1, inputAndCursorFromAst.Item2, false, out replacementIndex, out replacementLength);
                replacementIndex += inputAndCursorFromAst.Item3;
            }
            if ((list == null) || (list.Count == 0))
            {
                ExecutionContext executionContextFromTLS = LocalPipeline.GetExecutionContextFromTLS();
                MutableTuple tuple2 = null;
                foreach (CallStackFrame frame in executionContextFromTLS.Debugger.GetCallStack())
                {
                    dynamic obj2 = PSObject.AsPSObject(frame);
                    var site1 = CallSite<Func<CallSite, object, bool>>.Create(Binder.UnaryOperation(CSharpBinderFlags.None, ExpressionType.IsTrue, typeof(CommandCompletion), new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) }));
                    if (site1.Target(site1, obj2.Command.Equals("TabExpansion2", StringComparison.OrdinalIgnoreCase)))
                    {
                        tuple2 = frame.FunctionContext._localsTuple;
                        break;
                    }
                }
                SessionStateScope currentScope = null;
                if (tuple2 != null)
                {
                    currentScope = executionContextFromTLS.EngineSessionState.CurrentScope;
                    SessionStateScope parent = executionContextFromTLS.EngineSessionState.CurrentScope;
                    while ((parent != null) && (parent.LocalsTuple != tuple2))
                    {
                        parent = parent.Parent;
                    }
                    if (parent != null)
                    {
                        executionContextFromTLS.EngineSessionState.CurrentScope = parent.Parent;
                    }
                }
                try
                {
                    list = new CompletionAnalysis(ast, tokens, positionOfCursor, options).GetResults(powershell, out replacementIndex, out replacementLength);
                }
                finally
                {
                    if (currentScope != null)
                    {
                        executionContextFromTLS.EngineSessionState.CurrentScope = currentScope;
                    }
                }
            }
            return new CommandCompletion(new Collection<CompletionResult>(list ?? EmptyCompletionResult), -1, replacementIndex, replacementLength);
        }

        private static Tuple<string, int, int> GetInputAndCursorFromAst(IScriptPosition cursorPosition)
        {
            string line = cursorPosition.Line;
            int num = cursorPosition.ColumnNumber - 1;
            int num2 = cursorPosition.Offset - num;
            return Tuple.Create<string, int, int>(line.Substring(0, num), num, num2);
        }

        public CompletionResult GetNextResult(bool forward)
        {
            CompletionResult result = null;
            int count = this.CompletionMatches.Count;
            if (count <= 0)
            {
                return result;
            }
            this.CurrentMatchIndex += forward ? 1 : -1;
            if (this.CurrentMatchIndex >= count)
            {
                this.CurrentMatchIndex = 0;
            }
            else if (this.CurrentMatchIndex < 0)
            {
                this.CurrentMatchIndex = count - 1;
            }
            return this.CompletionMatches[this.CurrentMatchIndex];
        }

        private static List<CompletionResult> InvokeLegacyTabExpansion(PowerShell powershell, string input, int cursorIndex, bool remoteToWin7, out int replacementIndex, out int replacementLength)
        {
            List<CompletionResult> list = null;
            char ch;
            Exception exception;
            string sentence = (cursorIndex != input.Length) ? input.Substring(0, cursorIndex) : input;
            string str2 = LastWordFinder.FindLastWord(sentence, out replacementIndex, out ch);
            replacementLength = sentence.Length - replacementIndex;
            CompletionExecutionHelper helper = new CompletionExecutionHelper(powershell);
            powershell.AddCommand("TabExpansion").AddArgument(sentence).AddArgument(str2);
            Collection<PSObject> collection = helper.ExecuteCurrentPowerShell(out exception, null);
            if (collection != null)
            {
                list = new List<CompletionResult>();
                foreach (PSObject obj2 in collection)
                {
                    CompletionResult item = PSObject.Base(obj2) as CompletionResult;
                    if (item == null)
                    {
                        string completionText = obj2.ToString();
                        if (((ch != '\0') && (completionText.Length > 2)) && (completionText[0] != ch))
                        {
                            completionText = ch + completionText + ch;
                        }
                        item = new CompletionResult(completionText);
                    }
                    list.Add(item);
                }
            }
            if (remoteToWin7 && ((list == null) || (list.Count == 0)))
            {
                string quote = (ch == '\0') ? string.Empty : ch.ToString();
                list = PSv2CompletionCompleter.PSv2GenerateMatchSetOfFiles(helper, str2, replacementIndex == 0, quote);
                List<CompletionResult> list2 = PSv2CompletionCompleter.PSv2GenerateMatchSetOfCmdlets(helper, str2, quote, replacementIndex == 0);
                if ((list2 != null) && (list2.Count > 0))
                {
                    list.AddRange(list2);
                }
            }
            return list;
        }

        public static Tuple<Ast, Token[], IScriptPosition> MapStringInputToParsedInput(string input, int cursorIndex)
        {
            Token[] tokenArray;
            ParseError[] errorArray;
            if (cursorIndex > input.Length)
            {
                throw PSTraceSource.NewArgumentException("cursorIndex");
            }
            ScriptBlockAst ast = Parser.ParseInput(input, out tokenArray, out errorArray);
            IScriptPosition position = ((InternalScriptPosition) ast.Extent.StartScriptPosition).CloneWithNewOffset(cursorIndex);
            return Tuple.Create<Ast, Token[], IScriptPosition>(ast, tokenArray, position);
        }

        private static bool NeedToInvokeLegacyTabExpansion(PowerShell powershell)
        {
            ExecutionContext contextFromTLS = powershell.GetContextFromTLS();
            return ((contextFromTLS.EngineSessionState.GetFunction("TabExpansion") != null) || (contextFromTLS.EngineSessionState.GetAlias("TabExpansion") != null));
        }

        public Collection<CompletionResult> CompletionMatches { get; private set; }

        public int CurrentMatchIndex { get; set; }

        public int ReplacementIndex { get; set; }

        public int ReplacementLength { get; set; }

        private class LastWordFinder
        {
            private int replacementIndex = 0;
            private readonly string sentence;
            private int sentenceIndex;
            private bool sequenceDueToEnd;
            private char[] wordBuffer;
            private int wordBufferIndex;

            private LastWordFinder(string sentence)
            {
                this.sentence = sentence;
            }

            private void Consume(char c)
            {
                this.wordBuffer[this.wordBufferIndex++] = c;
            }

            private string FindLastWord(out int replacementIndexOut, out char closingQuote)
            {
                bool inQuote = false;
                bool inOppositeQuote = false;
                this.ReplacementIndex = 0;
                this.sentenceIndex = 0;
                while (this.sentenceIndex < this.sentence.Length)
                {
                    char c = this.sentence[this.sentenceIndex];
                    switch (c)
                    {
                        case '\'':
                            this.HandleQuote(ref inQuote, ref inOppositeQuote, c);
                            break;

                        case '"':
                            this.HandleQuote(ref inOppositeQuote, ref inQuote, c);
                            break;

                        case '`':
                            this.Consume(c);
                            if (++this.sentenceIndex < this.sentence.Length)
                            {
                                this.Consume(this.sentence[this.sentenceIndex]);
                            }
                            break;

                        default:
                            if (IsWhitespace(c))
                            {
                                if (this.sequenceDueToEnd)
                                {
                                    this.sequenceDueToEnd = false;
                                    if (inQuote)
                                    {
                                        inQuote = false;
                                    }
                                    if (inOppositeQuote)
                                    {
                                        inOppositeQuote = false;
                                    }
                                    this.ReplacementIndex = this.sentenceIndex + 1;
                                }
                                else if (inQuote || inOppositeQuote)
                                {
                                    this.Consume(c);
                                }
                                else
                                {
                                    this.ReplacementIndex = this.sentenceIndex + 1;
                                }
                            }
                            else
                            {
                                this.Consume(c);
                            }
                            break;
                    }
                    this.sentenceIndex++;
                }
                string str = new string(this.wordBuffer, 0, this.wordBufferIndex);
                closingQuote = inQuote ? '\'' : (inOppositeQuote ? '"' : '\0');
                replacementIndexOut = this.ReplacementIndex;
                return str;
            }

            internal static string FindLastWord(string sentence, out int replacementIndexOut, out char closingQuote)
            {
                return new CommandCompletion.LastWordFinder(sentence).FindLastWord(out replacementIndexOut, out closingQuote);
            }

            private void HandleQuote(ref bool inQuote, ref bool inOppositeQuote, char c)
            {
                if (inOppositeQuote)
                {
                    this.Consume(c);
                }
                else if (inQuote)
                {
                    if (this.sequenceDueToEnd)
                    {
                        this.ReplacementIndex = this.sentenceIndex + 1;
                    }
                    this.sequenceDueToEnd = !this.sequenceDueToEnd;
                }
                else
                {
                    inQuote = true;
                    this.ReplacementIndex = this.sentenceIndex;
                }
            }

            private static bool IsWhitespace(char c)
            {
                if (c != ' ')
                {
                    return (c == '\t');
                }
                return true;
            }

            private int ReplacementIndex
            {
                get
                {
                    return this.replacementIndex;
                }
                set
                {
                    this.wordBuffer = new char[this.sentence.Length];
                    this.wordBufferIndex = 0;
                    this.replacementIndex = value;
                }
            }
        }

        private static class PSv2CompletionCompleter
        {
            private static readonly char[] CharsRequiringQuotedString = "`&@'#{}()$,;|<> \t".ToCharArray();
            private static readonly Regex CmdletTabRegex = new Regex(@"^[\w\*\?]+-[\w\*\?]*");

            private static void AddCommandResult(CommandAndName commandAndName, bool useFullName, bool completingAtStartOfLine, string quote, List<CompletionResult> results)
            {
                string completionText = useFullName ? commandAndName.CommandName.FullName : commandAndName.CommandName.ShortName;
                string str2 = AddQuoteIfNecessary(completionText, quote, completingAtStartOfLine);
                CommandTypes? nullable = SafeGetProperty<CommandTypes?>(commandAndName.Command, "CommandType");
                if (nullable.HasValue)
                {
                    string str3;
                    string listItemText = SafeGetProperty<string>(commandAndName.Command, "Name");
                    if ((((CommandTypes) nullable.Value) == CommandTypes.Cmdlet) || (((CommandTypes) nullable.Value) == CommandTypes.Application))
                    {
                        str3 = SafeGetProperty<string>(commandAndName.Command, "Definition");
                    }
                    else
                    {
                        str3 = listItemText;
                    }
                    results.Add(new CompletionResult(str2, listItemText, CompletionResultType.Command, str3));
                }
            }

            private static string AddQuoteIfNecessary(string completionText, string quote, bool completingAtStartOfLine)
            {
                if (completionText.IndexOfAny(CharsRequiringQuotedString) != -1)
                {
                    bool flag = (quote.Length == 0) && completingAtStartOfLine;
                    string str = (quote.Length == 0) ? "'" : quote;
                    completionText = (str == "'") ? completionText.Replace("'", "''") : completionText;
                    completionText = str + completionText + str;
                    completionText = flag ? ("& " + completionText) : completionText;
                    return completionText;
                }
                completionText = quote + completionText + quote;
                return completionText;
            }

            private static IEnumerable<PathItemAndConvertedPath> CombineMatchSets(List<PathItemAndConvertedPath> s1, List<PathItemAndConvertedPath> s2)
            {
                if ((s1 == null) || (s1.Count < 1))
                {
                    return s2;
                }
                if ((s2 == null) || (s2.Count < 1))
                {
                    return s1;
                }
                List<PathItemAndConvertedPath> list = new List<PathItemAndConvertedPath>();
                list.AddRange(s1);
                int num = 0;
                int num2 = 0;
                while (num < s2.Count)
                {
                    if ((num2 < s1.Count) && (string.Compare(s2[num].Path, s1[num2].Path, false, CultureInfo.CurrentCulture) == 0))
                    {
                        num2++;
                    }
                    else
                    {
                        list.Add(s2[num]);
                    }
                    num++;
                }
                return list;
            }

            private static void PrependSnapInNameForSameCmdletNames(CommandAndName[] cmdlets, bool completingAtStartOfLine, string quote, List<CompletionResult> results)
            {
                CommandAndName name;
                int index = 0;
                bool useFullName = false;
            Label_0004:
                name = cmdlets[index];
                int num2 = index + 1;
                if (num2 >= cmdlets.Length)
                {
                    AddCommandResult(name, useFullName, completingAtStartOfLine, quote, results);
                }
                else
                {
                    CommandAndName name2 = cmdlets[num2];
                    if (string.Compare(name.CommandName.ShortName, name2.CommandName.ShortName, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        AddCommandResult(name, true, completingAtStartOfLine, quote, results);
                        useFullName = true;
                    }
                    else
                    {
                        AddCommandResult(name, useFullName, completingAtStartOfLine, quote, results);
                        useFullName = false;
                    }
                    index++;
                    goto Label_0004;
                }
            }

            private static List<PathItemAndConvertedPath> PSv2FindMatches(CompletionExecutionHelper helper, string path, bool shouldFullyQualifyPaths)
            {
                Exception exception;
                List<PathItemAndConvertedPath> list = new List<PathItemAndConvertedPath>();
                PowerShell currentPowerShell = helper.CurrentPowerShell;
                if (!shouldFullyQualifyPaths)
                {
                    currentPowerShell.AddScript(string.Format(CultureInfo.InvariantCulture, "& {{ trap {{ continue }} ; resolve-path {0} -Relative -WarningAction SilentlyContinue | %{{,($_,(get-item $_ -WarningAction SilentlyContinue),(convert-path $_ -WarningAction SilentlyContinue))}} }}", new object[] { path }));
                }
                else
                {
                    currentPowerShell.AddScript(string.Format(CultureInfo.InvariantCulture, "& {{ trap {{ continue }} ; resolve-path {0} -WarningAction SilentlyContinue | %{{,($_,(get-item $_ -WarningAction SilentlyContinue),(convert-path $_ -WarningAction SilentlyContinue))}} }}", new object[] { path }));
                }
                Collection<PSObject> collection = helper.ExecuteCurrentPowerShell(out exception, null);
                if ((collection == null) || (collection.Count == 0))
                {
                    return null;
                }
                foreach (PSObject obj2 in collection)
                {
                    IList baseObject = obj2.BaseObject as IList;
                    if ((baseObject != null) && (baseObject.Count == 3))
                    {
                        object obj3 = baseObject[0];
                        PSObject item = baseObject[1] as PSObject;
                        object obj5 = baseObject[1];
                        if (((obj3 != null) && (item != null)) && (obj5 != null))
                        {
                            list.Add(new PathItemAndConvertedPath(CompletionExecutionHelper.SafeToString(obj3), item, CompletionExecutionHelper.SafeToString(obj5)));
                        }
                    }
                }
                if (list.Count == 0)
                {
                    return null;
                }
                list.Sort((Comparison<PathItemAndConvertedPath>) ((x, y) => string.Compare(x.Path, y.Path, CultureInfo.CurrentCulture, CompareOptions.IgnoreCase)));
                return list;
            }

            internal static List<CompletionResult> PSv2GenerateMatchSetOfCmdlets(CompletionExecutionHelper helper, string lastWord, string quote, bool completingAtStartOfLine)
            {
                bool flag;
                List<CompletionResult> results = new List<CompletionResult>();
                if (PSv2IsCommandLikeCmdlet(lastWord, out flag))
                {
                    Exception exception;
                    helper.CurrentPowerShell.AddCommand("Get-Command").AddParameter("Name", lastWord + "*").AddCommand("Sort-Object").AddParameter("Property", "Name");
                    Collection<PSObject> collection = helper.ExecuteCurrentPowerShell(out exception, null);
                    if ((collection == null) || (collection.Count <= 0))
                    {
                        return results;
                    }
                    CommandAndName[] cmdlets = new CommandAndName[collection.Count];
                    for (int i = 0; i < collection.Count; i++)
                    {
                        PSObject psObject = collection[i];
                        string fullName = CmdletInfo.GetFullName(psObject);
                        cmdlets[i] = new CommandAndName(psObject, PSSnapinQualifiedName.GetInstance(fullName));
                    }
                    if (flag)
                    {
                        foreach (CommandAndName name in cmdlets)
                        {
                            AddCommandResult(name, true, completingAtStartOfLine, quote, results);
                        }
                        return results;
                    }
                    PrependSnapInNameForSameCmdletNames(cmdlets, completingAtStartOfLine, quote, results);
                }
                return results;
            }

            internal static List<CompletionResult> PSv2GenerateMatchSetOfFiles(CompletionExecutionHelper helper, string lastWord, bool completingAtStartOfLine, string quote)
            {
                List<CompletionResult> list = new List<CompletionResult>();
                lastWord = lastWord ?? string.Empty;
                bool flag = string.IsNullOrEmpty(lastWord);
                bool flag2 = !flag && lastWord.EndsWith("*", StringComparison.Ordinal);
                bool flag3 = WildcardPattern.ContainsWildcardCharacters(lastWord);
                string str = lastWord + "*";
                bool shouldFullyQualifyPaths = PSv2ShouldFullyQualifyPathsPath(helper, lastWord);
                bool flag5 = lastWord.StartsWith(@"\\", StringComparison.Ordinal) || lastWord.StartsWith("//", StringComparison.Ordinal);
                List<PathItemAndConvertedPath> list2 = null;
                List<PathItemAndConvertedPath> list3 = null;
                if (flag3 && !flag)
                {
                    list2 = PSv2FindMatches(helper, lastWord, shouldFullyQualifyPaths);
                }
                if (!flag2)
                {
                    list3 = PSv2FindMatches(helper, str, shouldFullyQualifyPaths);
                }
                IEnumerable<PathItemAndConvertedPath> enumerable = CombineMatchSets(list2, list3);
                if (enumerable != null)
                {
                    foreach (PathItemAndConvertedPath path in enumerable)
                    {
                        string str2 = WildcardPattern.Escape(path.Path);
                        string str3 = WildcardPattern.Escape(path.ConvertedPath);
                        string completionText = flag5 ? str3 : str2;
                        completionText = AddQuoteIfNecessary(completionText, quote, completingAtStartOfLine);
                        bool? nullable = SafeGetProperty<bool?>(path.Item, "PSIsContainer");
                        string listItemText = SafeGetProperty<string>(path.Item, "PSChildName");
                        string toolTip = CompletionExecutionHelper.SafeToString(path.ConvertedPath);
                        if (nullable.HasValue && !string.IsNullOrEmpty (listItemText) && toolTip != null)
                        {
                            CompletionResultType resultType = nullable.Value ? CompletionResultType.ProviderContainer : CompletionResultType.ProviderItem;
                            list.Add(new CompletionResult(completionText, listItemText, resultType, toolTip));
                        }
                    }
                }
                return list;
            }

            private static bool PSv2IsCommandLikeCmdlet(string lastWord, out bool isSnapinSpecified)
            {
                isSnapinSpecified = false;
                string[] strArray = lastWord.Split(new char[] { '\\' });
                if (strArray.Length == 1)
                {
                    return CmdletTabRegex.IsMatch(lastWord);
                }
                if (strArray.Length == 2)
                {
                    isSnapinSpecified = PSSnapInInfo.IsPSSnapinIdValid(strArray[0]);
                    if (isSnapinSpecified)
                    {
                        return CmdletTabRegex.IsMatch(strArray[1]);
                    }
                }
                return false;
            }

            private static bool PSv2ShouldFullyQualifyPathsPath(CompletionExecutionHelper helper, string lastWord)
            {
                if ((lastWord.StartsWith("~", StringComparison.OrdinalIgnoreCase) || lastWord.StartsWith(@"\", StringComparison.OrdinalIgnoreCase)) || lastWord.StartsWith("/", StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
                helper.CurrentPowerShell.AddCommand("Split-Path").AddParameter("Path", lastWord).AddParameter("IsAbsolute", true);
                return helper.ExecuteCommandAndGetResultAsBool();
            }

            private static T SafeGetProperty<T>(PSObject psObject, string propertyName)
            {
                if (psObject != null)
                {
                    T local;
                    PSPropertyInfo info = psObject.Properties[propertyName];
                    if (info == null)
                    {
                        return default(T);
                    }
                    object valueToConvert = info.Value;
                    if (valueToConvert == null)
                    {
                        return default(T);
                    }
                    if (LanguagePrimitives.TryConvertTo<T>(valueToConvert, out local))
                    {
                        return local;
                    }
                }
                return default(T);
            }

            [StructLayout(LayoutKind.Sequential)]
            private struct CommandAndName
            {
                internal readonly PSObject Command;
                internal readonly PSSnapinQualifiedName CommandName;
                internal CommandAndName(PSObject command, PSSnapinQualifiedName commandName)
                {
                    this.Command = command;
                    this.CommandName = commandName;
                }
            }

            [StructLayout(LayoutKind.Sequential)]
            private struct PathItemAndConvertedPath
            {
                internal readonly string Path;
                internal readonly PSObject Item;
                internal readonly string ConvertedPath;
                internal PathItemAndConvertedPath(string path, PSObject item, string convertedPath)
                {
                    this.Path = path;
                    this.Item = item;
                    this.ConvertedPath = convertedPath;
                }
            }
        }
    }
}

