namespace System.Management.Automation
{
    using System;
    using System.Collections;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Management.Automation.Language;
    using System.Text.RegularExpressions;

    internal static class SwitchOps
    {
        internal static bool ConditionSatisfiedRegex(bool caseSensitive, object condition, IScriptExtent errorPosition, string str, ExecutionContext context)
        {
            string str2;
            bool success;
            RegexOptions options = caseSensitive ? RegexOptions.None : RegexOptions.IgnoreCase;
            try
            {
                Match match;
                Regex regex = condition as Regex;
                if ((regex != null) && (((regex.Options & RegexOptions.IgnoreCase) != RegexOptions.None) != caseSensitive))
                {
                    match = regex.Match(str);
                }
                else
                {
                    str2 = PSObject.ToStringParser(context, condition);
                    match = Regex.Match(str, str2, options);
                    if (match.Success && (match.Groups.Count > 0))
                    {
                        regex = new Regex(str2, options);
                    }
                }
                if (match.Success)
                {
                    GroupCollection groups = match.Groups;
                    if (groups.Count > 0)
                    {
                        Hashtable newValue = new Hashtable(StringComparer.CurrentCultureIgnoreCase);
                        foreach (string str3 in regex.GetGroupNames())
                        {
                            Group group = groups[str3];
                            if (group.Success)
                            {
                                int num;
                                if (int.TryParse(str3, out num))
                                {
                                    newValue.Add(num, group.ToString());
                                }
                                else
                                {
                                    newValue.Add(str3, group.ToString());
                                }
                            }
                        }
                        context.SetVariable(SpecialVariables.MatchesVarPath, newValue);
                    }
                }
                success = match.Success;
            }
            catch (ArgumentException exception)
            {
                str2 = PSObject.ToStringParser(context, condition);
                throw InterpreterError.NewInterpreterExceptionWithInnerException(str2, typeof(RuntimeException), errorPosition, "InvalidRegularExpression", ParserStrings.InvalidRegularExpression, exception, new object[] { str2 });
            }
            return success;
        }

        internal static bool ConditionSatisfiedWildcard(bool caseSensitive, object condition, string str, ExecutionContext context)
        {
            WildcardPattern pattern = condition as WildcardPattern;
            if (pattern != null)
            {
                if (((pattern.Options & WildcardOptions.IgnoreCase) == WildcardOptions.None) != caseSensitive)
                {
                    WildcardOptions options = caseSensitive ? WildcardOptions.None : WildcardOptions.IgnoreCase;
                    pattern = new WildcardPattern(pattern.Pattern, options);
                }
            }
            else
            {
                WildcardOptions options2 = caseSensitive ? WildcardOptions.None : WildcardOptions.IgnoreCase;
                pattern = new WildcardPattern(PSObject.ToStringParser(context, condition), options2);
            }
            return pattern.IsMatch(str);
        }

        internal static string ResolveFilePath(IScriptExtent errorExtent, object obj, ExecutionContext context)
        {
            string str2;
            try
            {
                ProviderInfo info2;
                FileInfo info = obj as FileInfo;
                string str = (info != null) ? info.FullName : PSObject.ToStringParser(context, obj);
                if (string.IsNullOrEmpty(str))
                {
                    throw InterpreterError.NewInterpreterException(str, typeof(RuntimeException), errorExtent, "InvalidFilenameOption", ParserStrings.InvalidFilenameOption, new object[0]);
                }
                SessionState state = new SessionState(context.EngineSessionState);
                Collection<string> resolvedProviderPathFromPSPath = state.Path.GetResolvedProviderPathFromPSPath(str, out info2);
                if (!info2.NameEquals(context.ProviderNames.FileSystem))
                {
                    throw InterpreterError.NewInterpreterException(str, typeof(RuntimeException), errorExtent, "FileOpenError", ParserStrings.FileOpenError, new object[] { info2.FullName });
                }
                if ((resolvedProviderPathFromPSPath == null) || (resolvedProviderPathFromPSPath.Count < 1))
                {
                    throw InterpreterError.NewInterpreterException(str, typeof(RuntimeException), errorExtent, "FileNotFound", ParserStrings.FileNotFound, new object[] { str });
                }
                if (resolvedProviderPathFromPSPath.Count > 1)
                {
                    throw InterpreterError.NewInterpreterException(resolvedProviderPathFromPSPath, typeof(RuntimeException), errorExtent, "AmbiguousPath", ParserStrings.AmbiguousPath, new object[0]);
                }
                str2 = resolvedProviderPathFromPSPath[0];
            }
            catch (RuntimeException exception)
            {
                if ((exception.ErrorRecord != null) && (exception.ErrorRecord.InvocationInfo == null))
                {
                    exception.ErrorRecord.SetInvocationInfo(new InvocationInfo(null, errorExtent, context));
                }
                throw;
            }
            return str2;
        }
    }
}

