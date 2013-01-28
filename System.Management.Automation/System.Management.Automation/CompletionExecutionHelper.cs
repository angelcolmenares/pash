namespace System.Management.Automation
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Management.Automation.Runspaces;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    internal class CompletionExecutionHelper
    {
        internal CompletionExecutionHelper(PowerShell powershell)
        {
            if (powershell == null)
            {
                throw PSTraceSource.NewArgumentNullException("powershell");
            }
            this.CurrentPowerShell = powershell;
        }

        internal Collection<PSObject> ExecuteCommand(string command)
        {
            Exception exception;
            return this.ExecuteCommand(command, true, out exception, null);
        }

        internal Collection<PSObject> ExecuteCommand(string command, bool isScript, out Exception exceptionThrown, Hashtable args)
        {
            exceptionThrown = null;
            if (this.CancelTabCompletion)
            {
                return new Collection<PSObject>();
            }
            this.CurrentPowerShell.AddCommand(command);
            Command command2 = new Command(command, isScript);
            if (args != null)
            {
                foreach (DictionaryEntry entry in args)
                {
                    command2.Parameters.Add((string) entry.Key, entry.Value);
                }
            }
            Collection<PSObject> collection = null;
            try
            {
                if (this.IsStopped)
                {
                    collection = new Collection<PSObject>();
                    this.CancelTabCompletion = true;
                }
            }
            catch (Exception exception)
            {
                CommandProcessorBase.CheckForSevereException(exception);
                exceptionThrown = exception;
            }
            return collection;
        }

        internal bool ExecuteCommandAndGetResultAsBool()
        {
            Exception exception;
            Collection<PSObject> collection = this.ExecuteCurrentPowerShell(out exception, null);
            if (((exception != null) || (collection == null)) || (collection.Count == 0))
            {
                return false;
            }
            if (collection.Count <= 1)
            {
                return LanguagePrimitives.IsTrue(collection[0]);
            }
            return true;
        }

        internal string ExecuteCommandAndGetResultAsString()
        {
            Exception exception;
            Collection<PSObject> collection = this.ExecuteCurrentPowerShell(out exception, null);
            if (((exception != null) || (collection == null)) || (collection.Count == 0))
            {
                return null;
            }
            if (collection[0] == null)
            {
                return string.Empty;
            }
            return SafeToString(collection[0]);
        }

        internal Collection<PSObject> ExecuteCurrentPowerShell(out Exception exceptionThrown, IEnumerable input = null)
        {
            exceptionThrown = null;
            if (this.CancelTabCompletion)
            {
                return new Collection<PSObject>();
            }
            Collection<PSObject> collection = null;
            try
            {
                collection = this.CurrentPowerShell.Invoke(input);
                if (this.IsStopped)
                {
                    collection = new Collection<PSObject>();
                    this.CancelTabCompletion = true;
                }
            }
            catch (Exception exception)
            {
                CommandProcessorBase.CheckForSevereException(exception);
                exceptionThrown = exception;
            }
            finally
            {
                this.CurrentPowerShell.Commands.Clear();
            }
            return collection;
        }

        internal static void SafeAddToStringList(List<string> list, object obj)
        {
            if (list != null)
            {
                string str = SafeToString(obj);
                if (!string.IsNullOrEmpty(str))
                {
                    list.Add(str);
                }
            }
        }

        internal static string SafeToString(object obj)
        {
            if (obj == null)
            {
                return string.Empty;
            }
            try
            {
                string str;
                PSObject obj2 = obj as PSObject;
                if (obj2 != null)
                {
                    object baseObject = obj2.BaseObject;
                    if ((baseObject != null) && !(baseObject is PSCustomObject))
                    {
                        str = baseObject.ToString();
                    }
                    else
                    {
                        str = obj2.ToString();
                    }
                }
                else
                {
                    str = obj.ToString();
                }
                return str;
            }
            catch (Exception exception)
            {
                CommandProcessorBase.CheckForSevereException(exception);
                return string.Empty;
            }
        }

        internal bool CancelTabCompletion { get; set; }

        internal PowerShell CurrentPowerShell { get; set; }

        internal bool IsRunning
        {
            get
            {
                return (this.CurrentPowerShell.InvocationStateInfo.State == PSInvocationState.Running);
            }
        }

        internal bool IsStopped
        {
            get
            {
                return (this.CurrentPowerShell.InvocationStateInfo.State == PSInvocationState.Stopped);
            }
        }
    }
}

