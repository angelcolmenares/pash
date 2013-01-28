namespace System.Management.Automation
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.IO;
    using System.Management.Automation.Internal;
    using System.Management.Automation.Runspaces;
    using System.Runtime.CompilerServices;
    using System.Threading;

    public class CommandInvocationIntrinsics
    {
        private PSCmdlet _cmdlet;
        private System.Management.Automation.ExecutionContext _context;
        private MshCommandRuntime commandRuntime;

        internal CommandInvocationIntrinsics(System.Management.Automation.ExecutionContext context) : this(context, null)
        {
        }

        internal CommandInvocationIntrinsics(System.Management.Automation.ExecutionContext context, PSCmdlet cmdlet)
        {
            this._context = context;
            if (cmdlet != null)
            {
                this._cmdlet = cmdlet;
                this.commandRuntime = cmdlet.CommandRuntime as MshCommandRuntime;
            }
        }

        public string ExpandString(string source)
        {
            if (this._cmdlet != null)
            {
                this._cmdlet.ThrowIfStopping();
            }
            return this._context.Engine.Expand(source);
        }

        public CmdletInfo GetCmdlet(string commandName)
        {
            return GetCmdlet(commandName, this._context);
        }

        internal static CmdletInfo GetCmdlet(string commandName, System.Management.Automation.ExecutionContext context)
        {
            CmdletInfo current = null;
            CommandSearcher searcher = new CommandSearcher(commandName, SearchResolutionOptions.None, CommandTypes.Cmdlet, context);
        Label_000C:
            try
            {
                if (!searcher.MoveNext())
                {
                    return current;
                }
            }
            catch (ArgumentException)
            {
                goto Label_000C;
            }
            catch (PathTooLongException)
            {
                goto Label_000C;
            }
            catch (FileLoadException)
            {
                goto Label_000C;
            }
            catch (MetadataException)
            {
                goto Label_000C;
            }
            catch (FormatException)
            {
                goto Label_000C;
            }
            current = ((IEnumerator) searcher).Current as CmdletInfo;
            goto Label_000C;
        }

        public CmdletInfo GetCmdletByTypeName(string cmdletTypeName)
        {
            if (string.IsNullOrEmpty(cmdletTypeName))
            {
                throw PSTraceSource.NewArgumentNullException("cmdletTypeName");
            }
            Exception exception = null;
            Type implementingType = LanguagePrimitives.ConvertStringToType(cmdletTypeName, out exception);
            if (exception != null)
            {
                throw exception;
            }
            if (implementingType == null)
            {
                return null;
            }
            CmdletAttribute attribute = null;
            foreach (object obj2 in implementingType.GetCustomAttributes(true))
            {
                attribute = obj2 as CmdletAttribute;
                if (attribute != null)
                {
                    break;
                }
            }
            if (attribute == null)
            {
                throw PSTraceSource.NewNotSupportedException();
            }
            string nounName = attribute.NounName;
            return new CmdletInfo(attribute.VerbName + "-" + nounName, implementingType, null, null, this._context);
        }

        public List<CmdletInfo> GetCmdlets()
        {
            return this.GetCmdlets("*");
        }

        public List<CmdletInfo> GetCmdlets(string pattern)
        {
            if (pattern == null)
            {
                throw PSTraceSource.NewArgumentNullException("pattern");
            }
            List<CmdletInfo> list = new List<CmdletInfo>();
            CmdletInfo item = null;
            CommandSearcher searcher = new CommandSearcher(pattern, SearchResolutionOptions.CommandNameIsPattern, CommandTypes.Cmdlet, this._context);
        Label_0025:
            try
            {
                if (!searcher.MoveNext())
                {
                    return list;
                }
            }
            catch (ArgumentException)
            {
                goto Label_0025;
            }
            catch (PathTooLongException)
            {
                goto Label_0025;
            }
            catch (FileLoadException)
            {
                goto Label_0025;
            }
            catch (MetadataException)
            {
                goto Label_0025;
            }
            catch (FormatException)
            {
                goto Label_0025;
            }
            item = ((IEnumerator) searcher).Current as CmdletInfo;
            if (item != null)
            {
                list.Add(item);
            }
            goto Label_0025;
        }

        public CommandInfo GetCommand(string commandName, CommandTypes type)
        {
            CommandInfo info = null;
            try
            {
                CommandOrigin runspace = CommandOrigin.Runspace;
                if (this._cmdlet != null)
                {
                    runspace = this._cmdlet.CommandOrigin;
                }
                info = CommandDiscovery.LookupCommandInfo(commandName, type, SearchResolutionOptions.None, runspace, this._context);
            }
            catch (CommandNotFoundException)
            {
            }
            return info;
        }

        public List<string> GetCommandName(string name, bool nameIsPattern, bool returnFullName)
        {
            if (name == null)
            {
                throw PSTraceSource.NewArgumentNullException("name");
            }
            List<string> list = new List<string>();
            foreach (CommandInfo info in this.GetCommands(name, CommandTypes.All, nameIsPattern))
            {
                if (info.CommandType == CommandTypes.Application)
                {
                    string extension = Path.GetExtension(info.Name);
                    if (!string.IsNullOrEmpty(extension))
                    {
                        foreach (string str2 in CommandDiscovery.PathExtensions)
                        {
                            if (str2.Equals(extension, StringComparison.OrdinalIgnoreCase))
                            {
                                if (returnFullName)
                                {
                                    list.Add(info.Definition);
                                }
                                else
                                {
                                    list.Add(info.Name);
                                }
                            }
                        }
                    }
                }
                else if (info.CommandType == CommandTypes.ExternalScript)
                {
                    if (returnFullName)
                    {
                        list.Add(info.Definition);
                    }
                    else
                    {
                        list.Add(info.Name);
                    }
                }
                else
                {
                    list.Add(info.Name);
                }
            }
            return list;
        }

        public IEnumerable<CommandInfo> GetCommands(string name, CommandTypes commandTypes, bool nameIsPattern)
        {
            if (name == null)
            {
                throw PSTraceSource.NewArgumentNullException("name");
            }
            CommandSearcher iteratorVariable0 = new CommandSearcher(name, nameIsPattern ? (SearchResolutionOptions.CommandNameIsPattern | SearchResolutionOptions.ResolveFunctionPatterns | SearchResolutionOptions.ResolveAliasPatterns) : SearchResolutionOptions.None, commandTypes, this._context);
        Label_0066:
            try
            {
                if (!iteratorVariable0.MoveNext())
                {
                    goto Label_00C2;
                }
            }
            catch (ArgumentException)
            {
                goto Label_0066;
            }
            catch (PathTooLongException)
            {
                goto Label_0066;
            }
            catch (FileLoadException)
            {
                goto Label_0066;
            }
            catch (MetadataException)
            {
                goto Label_0066;
            }
            catch (FormatException)
            {
                goto Label_0066;
            }
            CommandInfo current = ((IEnumerator) iteratorVariable0).Current as CommandInfo;
            if (current != null)
            {
                yield return current;
            }
            goto Label_0066;
        Label_00C2:;
        }

        public Collection<PSObject> InvokeScript(string script)
        {
            return this.InvokeScript(script, true, PipelineResultTypes.None, null, new object[0]);
        }

        public Collection<PSObject> InvokeScript(string script, params object[] args)
        {
            return this.InvokeScript(script, true, PipelineResultTypes.None, args, new object[0]);
        }

        public Collection<PSObject> InvokeScript(SessionState sessionState, ScriptBlock scriptBlock, params object[] args)
        {
            Collection<PSObject> collection;
            if (scriptBlock == null)
            {
                throw PSTraceSource.NewArgumentNullException("scriptBlock");
            }
            if (sessionState == null)
            {
                throw PSTraceSource.NewArgumentNullException("sessionState");
            }
            SessionStateInternal engineSessionState = this._context.EngineSessionState;
            try
            {
                this._context.EngineSessionState = sessionState.Internal;
                collection = this.InvokeScript(scriptBlock, false, PipelineResultTypes.None, null, args);
            }
            finally
            {
                this._context.EngineSessionState = engineSessionState;
            }
            return collection;
        }

        public Collection<PSObject> InvokeScript(bool useLocalScope, ScriptBlock scriptBlock, IList input, params object[] args)
        {
            Collection<PSObject> collection;
            if (scriptBlock == null)
            {
                throw PSTraceSource.NewArgumentNullException("scriptBlock");
            }
            Runspace defaultRunspace = Runspace.DefaultRunspace;
            Runspace.DefaultRunspace = this._context.CurrentRunspace;
            try
            {
                collection = this.InvokeScript(scriptBlock, useLocalScope, PipelineResultTypes.None, input, args);
            }
            finally
            {
                Runspace.DefaultRunspace = defaultRunspace;
            }
            return collection;
        }

        private Collection<PSObject> InvokeScript(ScriptBlock sb, bool useNewScope, PipelineResultTypes writeToPipeline, IList input, params object[] args)
        {
            object obj2;
            if (this._cmdlet != null)
            {
                this._cmdlet.ThrowIfStopping();
            }
            Cmdlet contextCmdlet = null;
            ScriptBlock.ErrorHandlingBehavior writeToExternalErrorPipe = ScriptBlock.ErrorHandlingBehavior.WriteToExternalErrorPipe;
            if ((writeToPipeline & PipelineResultTypes.Output) == PipelineResultTypes.Output)
            {
                contextCmdlet = this._cmdlet;
                writeToPipeline &= ~PipelineResultTypes.Output;
            }
            if ((writeToPipeline & PipelineResultTypes.Error) == PipelineResultTypes.Error)
            {
                writeToExternalErrorPipe = ScriptBlock.ErrorHandlingBehavior.WriteToCurrentErrorPipe;
                writeToPipeline &= ~PipelineResultTypes.Error;
            }
            if (writeToPipeline != PipelineResultTypes.None)
            {
                throw PSTraceSource.NewNotImplementedException();
            }
            if (contextCmdlet != null)
            {
                sb.InvokeUsingCmdlet(contextCmdlet, useNewScope, writeToExternalErrorPipe, AutomationNull.Value, input, AutomationNull.Value, args);
                obj2 = AutomationNull.Value;
            }
            else
            {
                obj2 = sb.DoInvokeReturnAsIs(useNewScope, writeToExternalErrorPipe, AutomationNull.Value, input, AutomationNull.Value, args);
            }
            if (obj2 == AutomationNull.Value)
            {
                return new Collection<PSObject>();
            }
            Collection<PSObject> collection = obj2 as Collection<PSObject>;
            if (collection == null)
            {
                collection = new Collection<PSObject>();
                IEnumerator enumerator = null;
                enumerator = LanguagePrimitives.GetEnumerator(obj2);
                if (enumerator != null)
                {
                    while (enumerator.MoveNext())
                    {
                        object current = enumerator.Current;
                        collection.Add(LanguagePrimitives.AsPSObjectOrNull(current));
                    }
                    return collection;
                }
                collection.Add(LanguagePrimitives.AsPSObjectOrNull(obj2));
            }
            return collection;
        }

        public Collection<PSObject> InvokeScript(string script, bool useNewScope, PipelineResultTypes writeToPipeline, IList input, params object[] args)
        {
            if (script == null)
            {
                throw new ArgumentNullException("script");
            }
            ScriptBlock sb = ScriptBlock.Create(this._context, script);
            return this.InvokeScript(sb, useNewScope, writeToPipeline, input, args);
        }

        public ScriptBlock NewScriptBlock(string scriptText)
        {
            if (this.commandRuntime != null)
            {
                this.commandRuntime.ThrowIfStopping();
            }
            return ScriptBlock.Create(this._context, scriptText);
        }

        public EventHandler<CommandLookupEventArgs> CommandNotFoundAction { get; set; }

        public bool HasErrors
        {
            get
            {
                return this.commandRuntime.PipelineProcessor.ExecutionFailed;
            }
            set
            {
                this.commandRuntime.PipelineProcessor.ExecutionFailed = value;
            }
        }

        public EventHandler<CommandLookupEventArgs> PostCommandLookupAction { get; set; }

        public EventHandler<CommandLookupEventArgs> PreCommandLookupAction { get; set; }

        
    }
}

