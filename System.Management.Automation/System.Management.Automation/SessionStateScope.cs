namespace System.Management.Automation
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Linq;
    using System.Management.Automation.Internal;
    using System.Management.Automation.Runspaces;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Threading;

    internal sealed class SessionStateScope
    {
        private Dictionary<string, AliasInfo> _alias;
        private SessionStateCapacityVariable _aliasCapacity;
        private Dictionary<string, List<CmdletInfo>> _allScopeCmdlets = new Dictionary<string, List<CmdletInfo>>(StringComparer.OrdinalIgnoreCase);
        private Dictionary<string, FunctionInfo> _allScopeFunctions;
        private Dictionary<string, PSDriveInfo> _automountedDrives;
        private Dictionary<string, List<CmdletInfo>> _cmdlets = new Dictionary<string, List<CmdletInfo>>(StringComparer.OrdinalIgnoreCase);
        private readonly Stack<MutableTuple> _dottedScopes = new Stack<MutableTuple>();
        private SessionStateCapacityVariable _driveCapacity;
        private Dictionary<string, PSDriveInfo> _drives;
        private SessionStateCapacityVariable _errorCapacity;
        private static readonly PSVariable _falseVar = new PSVariable("false", false, ScopedItemOptions.AllScope | ScopedItemOptions.Constant, "Boolean False");
        private SessionStateCapacityVariable _functionCapacity;
        private Dictionary<string, FunctionInfo> _functions;
        private static readonly NullVariable _nullVar = new NullVariable();
        private SessionStateScope _scriptScope;
        private static readonly PSVariable _trueVar = new PSVariable("true", true, ScopedItemOptions.AllScope | ScopedItemOptions.Constant, "Boolean True");
        private SessionStateCapacityVariable _variableCapacity;
        private Dictionary<string, PSVariable> _variables;
        private Dictionary<string, List<string>> commandsToAliasesCache = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

        internal SessionStateScope(SessionStateScope parentScope)
        {
            this.ScopeOrigin = CommandOrigin.Internal;
            this.Parent = parentScope;
            if (parentScope != null)
            {
                this._scriptScope = parentScope.ScriptScope;
            }
            else
            {
                this._scriptScope = this;
            }
        }

        private void AddAliasToCache(string alias, string value)
        {
            List<string> list;
            if (!this.commandsToAliasesCache.TryGetValue(value, out list))
            {
                List<string> list2 = new List<string> {
                    alias
                };
                this.commandsToAliasesCache.Add(value, list2);
            }
            else if (!list.Contains<string>(alias, StringComparer.OrdinalIgnoreCase))
            {
                list.Add(alias);
            }
        }

        internal CmdletInfo AddCmdletToCache(string name, CmdletInfo cmdlet, CommandOrigin origin, System.Management.Automation.ExecutionContext context)
        {
            bool flag = false;
            try
            {
                List<CmdletInfo> list;
                if (!this.GetCmdlets().TryGetValue(name, out list))
                {
                    list = new List<CmdletInfo> {
                        cmdlet
                    };
                    this.GetCmdlets().Add(name, list);
                    if ((cmdlet.Options & ScopedItemOptions.AllScope) != ScopedItemOptions.None)
                    {
                        this.GetAllScopeCmdlets()[name].Insert(0, cmdlet);
                    }
                }
                else
                {
                    if (!string.IsNullOrEmpty(cmdlet.ModuleName))
                    {
                        foreach (CmdletInfo info in list)
                        {
                            if (string.Equals(cmdlet.FullName, info.FullName, StringComparison.OrdinalIgnoreCase))
                            {
                                if (cmdlet.ImplementingType == info.ImplementingType)
                                {
                                    return null;
                                }
                                flag = true;
                                break;
                            }
                        }
                    }
                    else
                    {
                        foreach (CmdletInfo info2 in list)
                        {
                            if (cmdlet.ImplementingType == info2.ImplementingType)
                            {
                                return null;
                            }
                            flag = true;
                            break;
                        }
                    }
                    if (!flag)
                    {
                        list.Insert(0, cmdlet);
                    }
                }
            }
            catch (ArgumentException)
            {
                flag = true;
            }
            if (flag)
            {
                throw PSTraceSource.NewNotSupportedException("DiscoveryExceptions", "DuplicateCmdletName", new object[] { cmdlet.Name });
            }
            return this.GetCmdlets()[name][0];
        }

        internal void AddSessionStateScopeDefaultVariables()
        {
            if (this.Parent == null)
            {
                this._variables.Add(_nullVar.Name, _nullVar);
                this._variables.Add(_falseVar.Name, _falseVar);
                this._variables.Add(_trueVar.Name, _trueVar);
            }
            else
            {
                foreach (PSVariable variable in this.Parent.GetPrivateVariables().Values)
                {
                    if (variable.IsAllScope)
                    {
                        this._variables.Add(variable.Name, variable);
                    }
                }
            }
            string variableName = "MaximumErrorCount";
            this._errorCapacity = this.CreateCapacityVariable(variableName, 0x100, 0x8000, 0x100, SessionStateStrings.MaxErrorCountDescription);
            this._variables.Add(variableName, this._errorCapacity);
            variableName = "MaximumVariableCount";
            this._variableCapacity = this.CreateCapacityVariable(variableName, 0x1000, 0x8000, 0x400, SessionStateStrings.MaxVariableCountDescription);
            this._variables.Add(variableName, this._variableCapacity);
            variableName = "MaximumFunctionCount";
            this._functionCapacity = this.CreateCapacityVariable(variableName, 0x1000, 0x8000, 0x400, SessionStateStrings.MaxFunctionCountDescription);
            this._variables.Add(variableName, this._functionCapacity);
            variableName = "MaximumAliasCount";
            this._aliasCapacity = this.CreateCapacityVariable(variableName, 0x1000, 0x8000, 0x400, SessionStateStrings.MaxAliasCountDescription);
            this._variables.Add(variableName, this._aliasCapacity);
            variableName = "MaximumDriveCount";
            this._driveCapacity = this.CreateCapacityVariable(variableName, 0x1000, 0x8000, 0x400, SessionStateStrings.MaxDriveCountDescription);
            this._variables.Add(variableName, this._driveCapacity);
        }

        private SessionStateCapacityVariable CreateCapacityVariable(string variableName, int defaultCapacity, int maxCapacity, int minCapacity, string descriptionResourceString)
        {
            SessionStateCapacityVariable sharedCapacityVariable = null;
            if (this.Parent != null)
            {
                sharedCapacityVariable = this.Parent.GetVariable(variableName) as SessionStateCapacityVariable;
            }
            if (sharedCapacityVariable == null)
            {
                sharedCapacityVariable = new SessionStateCapacityVariable(variableName, defaultCapacity, maxCapacity, minCapacity, ScopedItemOptions.None);
            }
            else
            {
                sharedCapacityVariable = new SessionStateCapacityVariable(variableName, sharedCapacityVariable, ScopedItemOptions.None);
            }
            if (string.IsNullOrEmpty(sharedCapacityVariable.Description))
            {
                sharedCapacityVariable.Description = descriptionResourceString;
            }
            return sharedCapacityVariable;
        }

        private static FunctionInfo CreateFunction(string name, ScriptBlock function, FunctionInfo originalFunction, ScopedItemOptions options, System.Management.Automation.ExecutionContext context, string helpFile)
        {
            if (options == ScopedItemOptions.Unspecified)
            {
                options = ScopedItemOptions.None;
            }
            if (originalFunction is FilterInfo)
            {
                return new FilterInfo(name, (FilterInfo) originalFunction);
            }
            if (originalFunction is WorkflowInfo)
            {
                return new WorkflowInfo(name, (WorkflowInfo) originalFunction);
            }
            if (originalFunction != null)
            {
                return new FunctionInfo(name, originalFunction);
            }
            if (function.IsFilter)
            {
                return new FilterInfo(name, function, options, context, helpFile);
            }
            return new FunctionInfo(name, function, options, context, helpFile);
        }

        internal AliasInfo GetAlias(string name)
        {
            AliasInfo info;
            this.GetAliases().TryGetValue(name, out info);
            return info;
        }

        private Dictionary<string, AliasInfo> GetAliases()
        {
            if (this._alias == null)
            {
                this._alias = new Dictionary<string, AliasInfo>(StringComparer.OrdinalIgnoreCase);
                if (this.Parent != null)
                {
                    foreach (AliasInfo info in this.Parent.GetAliases().Values)
                    {
                        if ((info.Options & ScopedItemOptions.AllScope) != ScopedItemOptions.None)
                        {
                            this._alias.Add(info.Name, info);
                        }
                    }
                }
            }
            return this._alias;
        }

        internal IEnumerable<string> GetAliasesByCommandName(string command)
        {
            List<string> iteratorVariable0;
            if (this.commandsToAliasesCache.TryGetValue(command, out iteratorVariable0))
            {
                foreach (string iteratorVariable1 in iteratorVariable0)
                {
                    yield return iteratorVariable1;
                }
            }
        }

        private Dictionary<string, List<CmdletInfo>> GetAllScopeCmdlets()
        {
            lock (this.AllScopeCmdletCache)
            {
                if (this.AllScopeCmdletCache == null)
                {
                    if ((this.Parent != null) && (this.Parent.AllScopeCmdletCache != null))
                    {
                        return this.Parent.AllScopeCmdletCache;
                    }
                    this._allScopeCmdlets = new Dictionary<string, List<CmdletInfo>>(StringComparer.OrdinalIgnoreCase);
                }
            }
            return this.AllScopeCmdletCache;
        }

        private Dictionary<string, FunctionInfo> GetAllScopeFunctions()
        {
            if (this._allScopeFunctions == null)
            {
                if ((this.Parent != null) && (this.Parent._allScopeFunctions != null))
                {
                    return this.Parent._allScopeFunctions;
                }
                this._allScopeFunctions = new Dictionary<string, FunctionInfo>(StringComparer.OrdinalIgnoreCase);
            }
            return this._allScopeFunctions;
        }

        internal object GetAutomaticVariableValue(AutomaticVariable variable)
        {
            int index = (int) variable;
            foreach (MutableTuple tuple in this._dottedScopes)
            {
                if (tuple.IsValueSet(index))
                {
                    return tuple.GetValue(index);
                }
            }
            if ((this.LocalsTuple != null) && this.LocalsTuple.IsValueSet(index))
            {
                return this.LocalsTuple.GetValue(index);
            }
            return AutomationNull.Value;
        }

        private Dictionary<string, PSDriveInfo> GetAutomountedDrives()
        {
            if (this._automountedDrives == null)
            {
                this._automountedDrives = new Dictionary<string, PSDriveInfo>(StringComparer.OrdinalIgnoreCase);
            }
            return this._automountedDrives;
        }

        internal CmdletInfo GetCmdlet(string name)
        {
            CmdletInfo info = null;
            List<CmdletInfo> list;
            if ((this.GetCmdlets().TryGetValue(name, out list) && (list != null)) && (list.Count > 0))
            {
                info = list[0];
            }
            return info;
        }

        private Dictionary<string, List<CmdletInfo>> GetCmdlets()
        {
            lock (this.CmdletCache)
            {
                if (this.CmdletCache == null)
                {
                    this._cmdlets = new Dictionary<string, List<CmdletInfo>>(StringComparer.OrdinalIgnoreCase);
                    if ((this.Parent != null) && (this.Parent.AllScopeCmdletCache != null))
                    {
                        foreach (KeyValuePair<string, List<CmdletInfo>> pair in this.Parent.AllScopeCmdletCache)
                        {
                            this._cmdlets.Add(pair.Key, pair.Value);
                        }
                    }
                }
            }
            return this.CmdletCache;
        }

        internal PSDriveInfo GetDrive (string name)
		{
			if (name == null) {
				throw PSTraceSource.NewArgumentNullException ("name");
			}
			PSDriveInfo info = null;
			if (OSHelper.IsUnix && name == "Microsoft.PowerShell.Core\\FileSystem") {
				name = "/";
			}
            if (this.GetDrives().ContainsKey(name))
            {
                return this.GetDrives()[name];
            }
            if (this.GetAutomountedDrives().ContainsKey(name))
            {
                info = this.GetAutomountedDrives()[name];
            }
            return info;
        }

        private Dictionary<string, PSDriveInfo> GetDrives()
        {
            if (this._drives == null)
            {
                this._drives = new Dictionary<string, PSDriveInfo>(StringComparer.OrdinalIgnoreCase);
            }
            return this._drives;
        }

        internal FunctionInfo GetFunction(string name)
        {
            FunctionInfo info;
            this.GetFunctions().TryGetValue(name, out info);
            return info;
        }

        private Dictionary<string, FunctionInfo> GetFunctions()
        {
            if (this._functions == null)
            {
                this._functions = new Dictionary<string, FunctionInfo>(StringComparer.OrdinalIgnoreCase);
                if ((this.Parent != null) && (this.Parent._allScopeFunctions != null))
                {
                    foreach (FunctionInfo info in this.Parent._allScopeFunctions.Values)
                    {
                        this._functions.Add(info.Name, info);
                    }
                }
            }
            return this._functions;
        }

        private Dictionary<string, PSVariable> GetPrivateVariables()
        {
            if (this._variables == null)
            {
                this._variables = new Dictionary<string, PSVariable>(StringComparer.OrdinalIgnoreCase);
                this.AddSessionStateScopeDefaultVariables();
            }
            return this._variables;
        }

        internal PSVariable GetVariable(string name)
        {
            return this.GetVariable(name, this.ScopeOrigin);
        }

        internal PSVariable GetVariable(string name, CommandOrigin origin)
        {
            PSVariable variable;
            this.TryGetVariable(name, origin, false, out variable);
            return variable;
        }

        private static bool IsFunctionOptionSet(FunctionInfo function, ScopedItemOptions options)
        {
            return ((function.Options & options) != ScopedItemOptions.None);
        }

        internal void NewDrive(PSDriveInfo newDrive)
        {
            if (newDrive == null)
            {
                throw PSTraceSource.NewArgumentNullException("newDrive");
            }
            if (this.GetDrives().ContainsKey(newDrive.Name))
            {
                SessionStateException exception = new SessionStateException(newDrive.Name, SessionStateCategory.Drive, "DriveAlreadyExists", SessionStateStrings.DriveAlreadyExists, ErrorCategory.ResourceExists, new object[0]);
                throw exception;
            }
            if (!newDrive.IsAutoMounted && (this.GetDrives().Count > (this.DriveCapacity.FastValue - 1)))
            {
                SessionStateOverflowException exception2 = new SessionStateOverflowException(newDrive.Name, SessionStateCategory.Drive, "DriveOverflow", SessionStateStrings.DriveOverflow, new object[] { this.DriveCapacity.FastValue });
                throw exception2;
            }
            if (!newDrive.IsAutoMounted)
            {
                this.GetDrives().Add(newDrive.Name, newDrive);
            }
            else if (!this.GetAutomountedDrives().ContainsKey(newDrive.Name))
            {
                this.GetAutomountedDrives().Add(newDrive.Name, newDrive);
            }
        }

        internal PSVariable NewVariable(PSVariable newVariable, bool force, SessionStateInternal sessionState)
        {
            PSVariable variable;
            bool flag = this.TryGetVariable(newVariable.Name, this.ScopeOrigin, true, out variable);
            if (flag)
            {
                if (((variable == null) || variable.IsConstant) || (!force && variable.IsReadOnly))
                {
                    SessionStateUnauthorizedAccessException exception = new SessionStateUnauthorizedAccessException(newVariable.Name, SessionStateCategory.Variable, "VariableNotWritable", SessionStateStrings.VariableNotWritable);
                    throw exception;
                }
                if (variable is LocalVariable)
                {
                    SessionStateUnauthorizedAccessException exception2 = new SessionStateUnauthorizedAccessException(newVariable.Name, SessionStateCategory.Variable, "VariableNotWritableRare", SessionStateStrings.VariableNotWritableRare);
                    throw exception2;
                }
                if (!object.ReferenceEquals(newVariable, variable))
                {
                    variable.WasRemoved = true;
                    variable = newVariable;
                }
            }
            else
            {
                variable = newVariable;
            }
            if (!flag && (this._variables.Count > (this.VariableCapacity.FastValue - 1)))
            {
                SessionStateOverflowException exception3 = new SessionStateOverflowException(newVariable.Name, SessionStateCategory.Variable, "VariableOverflow", SessionStateStrings.VariableOverflow, new object[] { this.VariableCapacity.FastValue });
                throw exception3;
            }
            if (System.Management.Automation.ExecutionContext.HasEverUsedConstrainedLanguage)
            {
                System.Management.Automation.ExecutionContext executionContextFromTLS = LocalPipeline.GetExecutionContextFromTLS();
                if (((executionContextFromTLS != null) && (executionContextFromTLS.LanguageMode == PSLanguageMode.ConstrainedLanguage)) && ((variable.Options & ScopedItemOptions.AllScope) == ScopedItemOptions.AllScope))
                {
                    throw new PSNotSupportedException();
                }
            }
            this._variables[variable.Name] = variable;
            variable.SessionState = sessionState;
            return variable;
        }

        internal void RemoveAlias(string name, bool force)
        {
            if (this.GetAliases().ContainsKey(name))
            {
                AliasInfo info = this.GetAliases()[name];
                if (((info.Options & ScopedItemOptions.Constant) != ScopedItemOptions.None) || (!force && ((info.Options & ScopedItemOptions.ReadOnly) != ScopedItemOptions.None)))
                {
                    SessionStateUnauthorizedAccessException exception = new SessionStateUnauthorizedAccessException(name, SessionStateCategory.Alias, "AliasNotRemovable", SessionStateStrings.AliasNotRemovable);
                    throw exception;
                }
                this.RemoveAliasFromCache(info.Name, info.Definition);
            }
            this.GetAliases().Remove(name);
        }

        private void RemoveAliasFromCache(string alias, string value)
        {
            List<string> list;
            Func<string, bool> predicate = null;
            if (this.commandsToAliasesCache.TryGetValue(value, out list))
            {
                if (list.Count <= 1)
                {
                    this.commandsToAliasesCache.Remove(value);
                }
                else
                {
                    if (predicate == null)
                    {
                        predicate = item => item.Equals(alias, StringComparison.OrdinalIgnoreCase);
                    }
                    string str = list.FirstOrDefault<string>(predicate);
                    if (str != null)
                    {
                        list.Remove(str);
                    }
                }
            }
        }

        internal void RemoveAllDrives()
        {
            this.GetDrives().Clear();
            this.GetAutomountedDrives().Clear();
        }

        internal void RemoveCmdlet(string name, int index, bool force)
        {
            List<CmdletInfo> list;
            if (this.GetCmdlets().TryGetValue(name, out list))
            {
                CmdletInfo info = list[index];
                if ((info.Options & ScopedItemOptions.AllScope) != ScopedItemOptions.None)
                {
                    this.GetAllScopeCmdlets()[name].RemoveAt(index);
                }
                list.RemoveAt(index);
                if (list.Count == 0)
                {
                    this.GetCmdlets().Remove(name);
                }
            }
        }

        internal void RemoveCmdletEntry(string name, bool force)
        {
            if (this.GetCmdlets().ContainsKey(name))
            {
                this.GetCmdlets().Remove(name);
            }
        }

        internal void RemoveDrive(PSDriveInfo drive)
        {
            if (drive == null)
            {
                throw PSTraceSource.NewArgumentNullException("drive");
            }
            if (this._drives != null)
            {
                if (this.GetDrives().ContainsKey(drive.Name))
                {
                    this.GetDrives().Remove(drive.Name);
                }
                else if (this.GetAutomountedDrives().ContainsKey(drive.Name))
                {
                    this.GetAutomountedDrives()[drive.Name].IsAutoMountedManuallyRemoved = true;
                    if (drive.IsNetworkDrive)
                    {
                        this.GetAutomountedDrives().Remove(drive.Name);
                    }
                }
            }
        }

        internal void RemoveFunction(string name, bool force)
        {
            if (this.GetFunctions().ContainsKey(name))
            {
                FunctionInfo function = this.GetFunctions()[name];
                if (IsFunctionOptionSet(function, ScopedItemOptions.Constant) || (!force && IsFunctionOptionSet(function, ScopedItemOptions.ReadOnly)))
                {
                    SessionStateUnauthorizedAccessException exception = new SessionStateUnauthorizedAccessException(name, SessionStateCategory.Function, "FunctionNotRemovable", SessionStateStrings.FunctionNotRemovable);
                    throw exception;
                }
                if (IsFunctionOptionSet(function, ScopedItemOptions.AllScope))
                {
                    this.GetAllScopeFunctions().Remove(name);
                }
            }
            this.GetFunctions().Remove(name);
        }

        internal void RemoveVariable(string name, bool force)
        {
            PSVariable variable = this.GetVariable(name);
            if (variable.IsConstant || (variable.IsReadOnly && !force))
            {
                SessionStateUnauthorizedAccessException exception = new SessionStateUnauthorizedAccessException(name, SessionStateCategory.Variable, "VariableNotRemovable", SessionStateStrings.VariableNotRemovable);
                throw exception;
            }
            if (variable is SessionStateCapacityVariable)
            {
                SessionStateUnauthorizedAccessException exception2 = new SessionStateUnauthorizedAccessException(name, SessionStateCategory.Variable, "VariableNotRemovableSystem", SessionStateStrings.VariableNotRemovableSystem);
                throw exception2;
            }
            if (variable is LocalVariable)
            {
                SessionStateUnauthorizedAccessException exception3 = new SessionStateUnauthorizedAccessException(name, SessionStateCategory.Variable, "VariableNotRemovableRare", SessionStateStrings.VariableNotRemovableRare);
                throw exception3;
            }
            this._variables.Remove(name);
            variable.WasRemoved = true;
        }

        internal AliasInfo SetAliasItem(AliasInfo aliasToSet, bool force, CommandOrigin origin = CommandOrigin.Internal)
        {
            if (!this.GetAliases().ContainsKey(aliasToSet.Name))
            {
                if (this.GetAliases().Count > (this.AliasCapacity.FastValue - 1))
                {
                    SessionStateOverflowException exception = new SessionStateOverflowException(aliasToSet.Name, SessionStateCategory.Alias, "AliasOverflow", SessionStateStrings.AliasOverflow, new object[] { this.AliasCapacity.FastValue });
                    throw exception;
                }
                this.GetAliases()[aliasToSet.Name] = aliasToSet;
            }
            else
            {
                AliasInfo valueToCheck = this.GetAliases()[aliasToSet.Name];
                SessionState.ThrowIfNotVisible(origin, valueToCheck);
                if (((valueToCheck.Options & ScopedItemOptions.Constant) != ScopedItemOptions.None) || (((valueToCheck.Options & ScopedItemOptions.ReadOnly) != ScopedItemOptions.None) && !force))
                {
                    SessionStateUnauthorizedAccessException exception2 = new SessionStateUnauthorizedAccessException(aliasToSet.Name, SessionStateCategory.Alias, "AliasNotWritable", SessionStateStrings.AliasNotWritable);
                    throw exception2;
                }
                if (((aliasToSet.Options & ScopedItemOptions.AllScope) == ScopedItemOptions.None) && ((valueToCheck.Options & ScopedItemOptions.AllScope) != ScopedItemOptions.None))
                {
                    SessionStateUnauthorizedAccessException exception3 = new SessionStateUnauthorizedAccessException(aliasToSet.Name, SessionStateCategory.Alias, "AliasAllScopeOptionCannotBeRemoved", SessionStateStrings.AliasAllScopeOptionCannotBeRemoved);
                    throw exception3;
                }
                this.RemoveAliasFromCache(valueToCheck.Name, valueToCheck.Definition);
                this.GetAliases()[aliasToSet.Name] = aliasToSet;
            }
            this.AddAliasToCache(aliasToSet.Name, aliasToSet.Definition);
            return this.GetAliases()[aliasToSet.Name];
        }

        internal AliasInfo SetAliasValue(string name, string value, System.Management.Automation.ExecutionContext context, bool force, CommandOrigin origin)
        {
            if (!this.GetAliases().ContainsKey(name))
            {
                if (this.GetAliases().Count > (this.AliasCapacity.FastValue - 1))
                {
                    SessionStateOverflowException exception = new SessionStateOverflowException(name, SessionStateCategory.Alias, "AliasOverflow", SessionStateStrings.AliasOverflow, new object[] { this.AliasCapacity.FastValue });
                    throw exception;
                }
                this.GetAliases()[name] = new AliasInfo(name, value, context);
            }
            else
            {
                AliasInfo valueToCheck = this.GetAliases()[name];
                if (((valueToCheck.Options & ScopedItemOptions.Constant) != ScopedItemOptions.None) || (!force && ((valueToCheck.Options & ScopedItemOptions.ReadOnly) != ScopedItemOptions.None)))
                {
                    SessionStateUnauthorizedAccessException exception2 = new SessionStateUnauthorizedAccessException(name, SessionStateCategory.Alias, "AliasNotWritable", SessionStateStrings.AliasNotWritable);
                    throw exception2;
                }
                SessionState.ThrowIfNotVisible(origin, valueToCheck);
                this.RemoveAliasFromCache(valueToCheck.Name, valueToCheck.Definition);
                if (force)
                {
                    this.GetAliases().Remove(name);
                    valueToCheck = new AliasInfo(name, value, context);
                    this.GetAliases()[name] = valueToCheck;
                }
                else
                {
                    valueToCheck.SetDefinition(value, false);
                }
            }
            this.AddAliasToCache(name, value);
            return this.GetAliases()[name];
        }

        internal AliasInfo SetAliasValue(string name, string value, ScopedItemOptions options, System.Management.Automation.ExecutionContext context, bool force, CommandOrigin origin)
        {
            if (!this.GetAliases().ContainsKey(name))
            {
                if (this.GetAliases().Count > (this.AliasCapacity.FastValue - 1))
                {
                    SessionStateOverflowException exception = new SessionStateOverflowException(name, SessionStateCategory.Alias, "AliasOverflow", SessionStateStrings.AliasOverflow, new object[] { this.AliasCapacity.FastValue });
                    throw exception;
                }
                AliasInfo info = new AliasInfo(name, value, context, options);
                this.GetAliases()[name] = info;
            }
            else
            {
                AliasInfo valueToCheck = this.GetAliases()[name];
                if (((valueToCheck.Options & ScopedItemOptions.Constant) != ScopedItemOptions.None) || (!force && ((valueToCheck.Options & ScopedItemOptions.ReadOnly) != ScopedItemOptions.None)))
                {
                    SessionStateUnauthorizedAccessException exception2 = new SessionStateUnauthorizedAccessException(name, SessionStateCategory.Alias, "AliasNotWritable", SessionStateStrings.AliasNotWritable);
                    throw exception2;
                }
                if ((options & ScopedItemOptions.Constant) != ScopedItemOptions.None)
                {
                    SessionStateUnauthorizedAccessException exception3 = new SessionStateUnauthorizedAccessException(name, SessionStateCategory.Alias, "AliasCannotBeMadeConstant", SessionStateStrings.AliasCannotBeMadeConstant);
                    throw exception3;
                }
                if (((options & ScopedItemOptions.AllScope) == ScopedItemOptions.None) && ((valueToCheck.Options & ScopedItemOptions.AllScope) != ScopedItemOptions.None))
                {
                    SessionStateUnauthorizedAccessException exception4 = new SessionStateUnauthorizedAccessException(name, SessionStateCategory.Alias, "AliasAllScopeOptionCannotBeRemoved", SessionStateStrings.AliasAllScopeOptionCannotBeRemoved);
                    throw exception4;
                }
                SessionState.ThrowIfNotVisible(origin, valueToCheck);
                this.RemoveAliasFromCache(valueToCheck.Name, valueToCheck.Definition);
                if (force)
                {
                    this.GetAliases().Remove(name);
                    valueToCheck = new AliasInfo(name, value, context, options);
                    this.GetAliases()[name] = valueToCheck;
                }
                else
                {
                    valueToCheck.Options = options;
                    valueToCheck.SetDefinition(value, false);
                }
            }
            this.AddAliasToCache(name, value);
            return this.GetAliases()[name];
        }

        internal FunctionInfo SetFunction(string name, ScriptBlock function, bool force, CommandOrigin origin, System.Management.Automation.ExecutionContext context)
        {
            return this.SetFunction(name, function, null, ScopedItemOptions.Unspecified, force, origin, context);
        }

        internal FunctionInfo SetFunction(string name, ScriptBlock function, FunctionInfo originalFunction, bool force, CommandOrigin origin, System.Management.Automation.ExecutionContext context)
        {
            return this.SetFunction(name, function, originalFunction, ScopedItemOptions.Unspecified, force, origin, context);
        }

        internal FunctionInfo SetFunction(string name, ScriptBlock function, FunctionInfo originalFunction, ScopedItemOptions options, bool force, CommandOrigin origin, System.Management.Automation.ExecutionContext context)
        {
            return this.SetFunction(name, function, originalFunction, options, force, origin, context, null);
        }

        internal FunctionInfo SetFunction(string name, ScriptBlock function, FunctionInfo originalFunction, ScopedItemOptions options, bool force, CommandOrigin origin, System.Management.Automation.ExecutionContext context, string helpFile)
        {
            return this.SetFunction(name, function, originalFunction, options, force, origin, context, helpFile, new Func<string, ScriptBlock, FunctionInfo, ScopedItemOptions, System.Management.Automation.ExecutionContext, string, FunctionInfo>(SessionStateScope.CreateFunction));
        }

        internal FunctionInfo SetFunction(string name, ScriptBlock function, FunctionInfo originalFunction, ScopedItemOptions options, bool force, CommandOrigin origin, System.Management.Automation.ExecutionContext context, string helpFile, Func<string, ScriptBlock, FunctionInfo, ScopedItemOptions, System.Management.Automation.ExecutionContext, string, FunctionInfo> functionFactory)
        {
            if (!this.GetFunctions().ContainsKey(name))
            {
                if (this.GetFunctions().Count > (this.FunctionCapacity.FastValue - 1))
                {
                    SessionStateOverflowException exception = new SessionStateOverflowException(name, SessionStateCategory.Function, "FunctionOverflow", SessionStateStrings.FunctionOverflow, new object[] { this.FunctionCapacity.FastValue });
                    throw exception;
                }
                FunctionInfo info = functionFactory(name, function, originalFunction, options, context, helpFile);
                this.GetFunctions()[name] = info;
                if (IsFunctionOptionSet(info, ScopedItemOptions.AllScope))
                {
                    this.GetAllScopeFunctions()[name] = info;
                }
            }
            else
            {
                FunctionInfo valueToCheck = this.GetFunctions()[name];
                SessionState.ThrowIfNotVisible(origin, valueToCheck);
                if (IsFunctionOptionSet(valueToCheck, ScopedItemOptions.Constant) || (!force && IsFunctionOptionSet(valueToCheck, ScopedItemOptions.ReadOnly)))
                {
                    SessionStateUnauthorizedAccessException exception2 = new SessionStateUnauthorizedAccessException(name, SessionStateCategory.Function, "FunctionNotWritable", SessionStateStrings.FunctionNotWritable);
                    throw exception2;
                }
                if ((options & ScopedItemOptions.Constant) != ScopedItemOptions.None)
                {
                    SessionStateUnauthorizedAccessException exception3 = new SessionStateUnauthorizedAccessException(name, SessionStateCategory.Function, "FunctionCannotBeMadeConstant", SessionStateStrings.FunctionCannotBeMadeConstant);
                    throw exception3;
                }
                if (((options & ScopedItemOptions.AllScope) == ScopedItemOptions.None) && IsFunctionOptionSet(valueToCheck, ScopedItemOptions.AllScope))
                {
                    SessionStateUnauthorizedAccessException exception4 = new SessionStateUnauthorizedAccessException(name, SessionStateCategory.Function, "FunctionAllScopeOptionCannotBeRemoved", SessionStateStrings.FunctionAllScopeOptionCannotBeRemoved);
                    throw exception4;
                }
                FunctionInfo info3 = valueToCheck;
                FunctionInfo newFunction = null;
                if (info3 != null)
                {
                    newFunction = functionFactory(name, function, originalFunction, options, context, helpFile);
                    if (!info3.GetType().Equals(newFunction.GetType()) || (((info3.Options & ScopedItemOptions.ReadOnly) != ScopedItemOptions.None) && force))
                    {
                        this.GetFunctions()[name] = newFunction;
                    }
                    else
                    {
                        bool flag2 = force || ((options & ScopedItemOptions.ReadOnly) == ScopedItemOptions.None);
                        info3.Update(newFunction, flag2, options, helpFile);
                    }
                }
            }
            return this.GetFunctions()[name];
        }

        internal PSVariable SetVariable(string name, object value, bool asValue, bool force, SessionStateInternal sessionState, CommandOrigin origin = CommandOrigin.Internal, bool fastPath = false)
        {
            PSVariable variable;
            PSVariable variable2 = value as PSVariable;
            if (fastPath)
            {
                if (this.Parent != null)
                {
                    throw new NotImplementedException("fastPath");
                }
                variable = new PSVariable(name, variable2.Value, variable2.Options, variable2.Attributes) {
                    Description = variable2.Description
                };
                this.GetPrivateVariables()[name] = variable;
                return variable;
            }
            bool flag = this.TryGetVariable(name, origin, true, out variable);
            if (!asValue && (variable2 != null))
            {
                if (flag)
                {
                    if (((variable == null) || variable.IsConstant) || (!force && variable.IsReadOnly))
                    {
                        SessionStateUnauthorizedAccessException exception = new SessionStateUnauthorizedAccessException(name, SessionStateCategory.Variable, "VariableNotWritable", SessionStateStrings.VariableNotWritable);
                        throw exception;
                    }
                    if ((variable is LocalVariable) && (variable2.Attributes.Any<Attribute>() || (variable2.Options != variable.Options)))
                    {
                        SessionStateUnauthorizedAccessException exception2 = new SessionStateUnauthorizedAccessException(name, SessionStateCategory.Variable, "VariableNotWritableRare", SessionStateStrings.VariableNotWritableRare);
                        throw exception2;
                    }
                    if (variable.IsReadOnly && force)
                    {
                        this._variables.Remove(name);
                        flag = false;
                        variable = new PSVariable(name, variable2.Value, variable2.Options, variable2.Attributes) {
                            Description = variable2.Description
                        };
                    }
                    else
                    {
                        variable.Attributes.Clear();
                        variable.Value = variable2.Value;
                        variable.Options = variable2.Options;
                        variable.Description = variable2.Description;
                        foreach (Attribute attribute in variable2.Attributes)
                        {
                            variable.Attributes.Add(attribute);
                        }
                    }
                }
                else
                {
                    variable = variable2;
                }
            }
            else if (variable != null)
            {
                variable.Value = value;
            }
            else
            {
                variable = (this.LocalsTuple != null) ? (this.LocalsTuple.TrySetVariable(name, value)) ?? new PSVariable(name, value) : new PSVariable(name, value);
            }
            if (!flag && (this._variables.Count > (this.VariableCapacity.FastValue - 1)))
            {
                SessionStateOverflowException exception3 = new SessionStateOverflowException(name, SessionStateCategory.Variable, "VariableOverflow", SessionStateStrings.VariableOverflow, new object[] { this.VariableCapacity.FastValue });
                throw exception3;
            }
            if (System.Management.Automation.ExecutionContext.HasEverUsedConstrainedLanguage)
            {
                System.Management.Automation.ExecutionContext executionContextFromTLS = LocalPipeline.GetExecutionContextFromTLS();
                if (((executionContextFromTLS != null) && (executionContextFromTLS.LanguageMode == PSLanguageMode.ConstrainedLanguage)) && ((variable.Options & ScopedItemOptions.AllScope) == ScopedItemOptions.AllScope))
                {
                    /* TODO: Review how to  get around this: */ /* throw new PSNotSupportedException(); */
                }
            }
            this._variables[name] = variable;
            variable.SessionState = sessionState;
            return variable;
        }

        internal bool TryGetLocalVariableFromTuple(string name, bool fromNewOrSet, out PSVariable result)
        {
            foreach (MutableTuple tuple in this._dottedScopes)
            {
                if (tuple.TryGetLocalVariable(name, fromNewOrSet, out result))
                {
                    return true;
                }
            }
            result = null;
            return ((this.LocalsTuple != null) && this.LocalsTuple.TryGetLocalVariable(name, fromNewOrSet, out result));
        }

        internal bool TryGetVariable(string name, CommandOrigin origin, bool fromNewOrSet, out PSVariable variable)
        {
            if (this.TryGetLocalVariableFromTuple(name, fromNewOrSet, out variable))
            {
                SessionState.ThrowIfNotVisible(origin, variable);
                return true;
            }
            if (this.GetPrivateVariables().TryGetValue(name, out variable))
            {
                SessionState.ThrowIfNotVisible(origin, variable);
                return true;
            }
            return false;
        }

        internal bool TrySetLocalParameterValue(string name, object value)
        {
            foreach (MutableTuple tuple in this._dottedScopes)
            {
                if (tuple.TrySetParameter(name, value))
                {
                    return true;
                }
            }
            return ((this.LocalsTuple != null) && this.LocalsTuple.TrySetParameter(name, value));
        }

        private SessionStateCapacityVariable AliasCapacity
        {
            get
            {
                this.GetPrivateVariables();
                return this._aliasCapacity;
            }
        }

        internal IEnumerable<AliasInfo> AliasTable
        {
            get
            {
                return this.GetAliases().Values;
            }
        }

        internal Dictionary<string, List<CmdletInfo>> AllScopeCmdletCache
        {
            get
            {
                return this._allScopeCmdlets;
            }
        }

        internal Dictionary<string, List<CmdletInfo>> CmdletCache
        {
            get
            {
                return this._cmdlets;
            }
        }

        internal Dictionary<string, List<CmdletInfo>> CmdletTable
        {
            get
            {
                return this.GetCmdlets();
            }
        }

        internal Stack<MutableTuple> DottedScopes
        {
            get
            {
                return this._dottedScopes;
            }
        }

        private SessionStateCapacityVariable DriveCapacity
        {
            get
            {
                this.GetPrivateVariables();
                return this._driveCapacity;
            }
        }

        internal IEnumerable<PSDriveInfo> Drives
        {
            get
            {
                Collection<PSDriveInfo> collection = new Collection<PSDriveInfo>();
                foreach (PSDriveInfo info in this.GetDrives().Values)
                {
                    collection.Add(info);
                }
                foreach (PSDriveInfo info2 in this.GetAutomountedDrives().Values)
                {
                    if (!info2.IsAutoMountedManuallyRemoved)
                    {
                        collection.Add(info2);
                    }
                }
                return collection;
            }
        }

        internal SessionStateCapacityVariable ErrorCapacity
        {
            get
            {
                this.GetPrivateVariables();
                return this._errorCapacity;
            }
        }

        private SessionStateCapacityVariable FunctionCapacity
        {
            get
            {
                this.GetPrivateVariables();
                return this._functionCapacity;
            }
        }

        internal Dictionary<string, FunctionInfo> FunctionTable
        {
            get
            {
                return this.GetFunctions();
            }
        }

        internal MutableTuple LocalsTuple { get; set; }

        internal SessionStateScope Parent { get; set; }

        internal CommandOrigin ScopeOrigin { get; set; }

        internal SessionStateScope ScriptScope
        {
            get
            {
                return this._scriptScope;
            }
            set
            {
                this._scriptScope = value;
            }
        }

        internal Version StrictModeVersion { get; set; }

        internal SessionStateCapacityVariable VariableCapacity
        {
            get
            {
                this.GetPrivateVariables();
                return this._variableCapacity;
            }
        }

        internal IDictionary<string, PSVariable> Variables
        {
            get
            {
                return this.GetPrivateVariables();
            }
        }

        
    }
}

