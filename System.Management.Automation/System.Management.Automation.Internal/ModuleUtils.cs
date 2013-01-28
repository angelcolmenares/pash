namespace System.Management.Automation.Internal
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Management.Automation;
    using System.Management.Automation.Runspaces;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Threading;

    internal static class ModuleUtils
    {
        private static List<string> cachedAvailableModuleFiles = new List<string>();
        private static long pipelineInstanceIdForModuleFileCache = -1L;

        internal static void GetAllAvailableModuleFiles(string directory, ICollection<string> availableModuleFiles)
        {
            RecurseDirectories(directory, subDirectory => GetAllAvailableModuleFiles(subDirectory, availableModuleFiles), false);
            foreach (string str in Directory.GetFiles(directory, "*", SearchOption.TopDirectoryOnly))
            {
                foreach (string str2 in ModuleIntrinsics.PSModuleExtensions)
                {
                    if (Path.GetExtension(str).Equals(str2, StringComparison.OrdinalIgnoreCase))
                    {
                        availableModuleFiles.Add(str);
                        break;
                    }
                }
            }
        }

        internal static List<string> GetDefaultAvailableModuleFiles(bool force, bool preferSystemModulePath, System.Management.Automation.ExecutionContext context)
        {
            Pipeline currentlyRunningPipeline = context.CurrentRunspace.GetCurrentlyRunningPipeline();
            if (!force && (currentlyRunningPipeline != null))
            {
                lock (cachedAvailableModuleFiles)
                {
                    if ((currentlyRunningPipeline.InstanceId == pipelineInstanceIdForModuleFileCache) && (cachedAvailableModuleFiles.Count > 0))
                    {
                        return cachedAvailableModuleFiles;
                    }
                }
            }
            List<string> availableModuleFiles = new List<string>();
            List<string> modulePaths = new List<string>();
            foreach (string str in ModuleIntrinsics.GetModulePath(preferSystemModulePath, context))
            {
				if (Directory.Exists (str))
				{
                	GetDefaultAvailableModuleFiles(str, availableModuleFiles, modulePaths);
				}
            }
            if (currentlyRunningPipeline != null)
            {
                lock (cachedAvailableModuleFiles)
                {
                    pipelineInstanceIdForModuleFileCache = currentlyRunningPipeline.InstanceId;
                    cachedAvailableModuleFiles = availableModuleFiles;
                }
            }
            return availableModuleFiles;
        }

        internal static void GetDefaultAvailableModuleFiles(string directory, ICollection<string> availableModuleFiles, List<string> modulePaths)
        {
            bool flag = false;
            if (modulePaths.Contains(directory))
            {
                flag = true;
            }
            RecurseDirectories(directory, subDirectory => GetDefaultAvailableModuleFiles(subDirectory, availableModuleFiles, modulePaths), !flag);
            foreach (string str in ModuleIntrinsics.PSModuleExtensions)
            {
                string path = Path.Combine(directory, Path.GetFileName(directory)) + str;
                if (File.Exists(path))
                {
                    availableModuleFiles.Add(path);
                    return;
                }
            }
        }

        internal static IEnumerable<CommandInfo> GetMatchingCommands(string pattern, System.Management.Automation.ExecutionContext context, CommandOrigin commandOrigin, bool rediscoverImportedModules = false)
        {
            return new _GetMatchingCommands_d__a(-2) { __3__pattern = pattern, __3__context = context, __3__commandOrigin = commandOrigin, __3__rediscoverImportedModules = rediscoverImportedModules };
        }

        private static void RecurseDirectories(string directory, Action<string> directoryAction, bool doNotRecurseForNestedModules)
        {
            string[] strArray = new string[0];
            string[] strArray2 = new string[0];
            bool flag = false;
            try
            {
				if (Directory.Exists (directory))
				{
	                strArray = Directory.GetDirectories(directory, "*", SearchOption.TopDirectoryOnly);
	                strArray2 = Directory.GetFiles(directory, "*", SearchOption.TopDirectoryOnly);
				}
            }
            catch (IOException)
            {
                return;
            }
            catch (UnauthorizedAccessException)
            {
                return;
            }
            if (doNotRecurseForNestedModules)
            {
                foreach (string str in strArray2)
                {
                    if (Path.GetFileNameWithoutExtension(str).Equals(Path.GetFileNameWithoutExtension(directory), StringComparison.OrdinalIgnoreCase))
                    {
                        foreach (string str2 in ModuleIntrinsics.PSModuleExtensions)
                        {
                            if (Path.GetExtension(str).Equals(str2, StringComparison.OrdinalIgnoreCase))
                            {
                                flag = true;
                                break;
                            }
                        }
                    }
                    if (flag)
                    {
                        break;
                    }
                }
            }
            if (!flag)
            {
                foreach (string str3 in strArray)
                {
                    try
                    {
                        directoryAction(str3);
                    }
                    catch (IOException)
                    {
                    }
                    catch (UnauthorizedAccessException)
                    {
                    }
                }
            }
        }

		private static bool GetMatchingCommands_b__8 (PSModuleInfo module)
		{
			return module.ModuleHasPrivateMembers;
		}


		[CompilerGenerated]
		private sealed class _GetMatchingCommands_d__a : IEnumerable<CommandInfo>, IEnumerable, IEnumerator<CommandInfo>, IEnumerator, IDisposable
		{
			private int __1__state;
			private CommandInfo __2__current;
			public CommandOrigin __3__commandOrigin;
			public System.Management.Automation.ExecutionContext __3__context;
			public string __3__pattern;
			public bool __3__rediscoverImportedModules;
			public List<string>.Enumerator __7__wrap1b;
			public Dictionary<string, CommandInfo>.Enumerator __7__wrap1d;
			public Dictionary<string, List<CommandTypes>>.KeyCollection.Enumerator __7__wrap1f;
			public List<CommandTypes>.Enumerator __7__wrap21;
			private int __l__initialThreadId;
			public CmdletInfo _cmdletInfo_5__c;
			public WildcardPattern _commandPattern_5__b;
			public CommandTypes _commandType_5__18;
			public CommandInfo _current_5__14;
			public CommandInfo _current_5__1a;
			public KeyValuePair<string, CommandInfo> _entry_5__13;
			public string _exportedCommand_5__17;
			public Dictionary<string, List<CommandTypes>> _exportedCommands_5__16;
			public string _moduleName_5__f;
			public string _modulePath_5__e;
			public List<PSModuleInfo> _modules_5__10;
			public string _moduleShortName_5__15;
			public PSModuleInfo _psModule_5__12;
			public bool _shouldExportCommand_5__19;
			public PSModuleInfo _tempModuleInfo_5__11;
			public CommandOrigin commandOrigin;
			public System.Management.Automation.ExecutionContext context;
			public string pattern;
			public bool rediscoverImportedModules;
			public PSModuleAutoLoadingPreference aa__;

			[DebuggerHidden]
			public _GetMatchingCommands_d__a(int __1__state)
			{
				this.__1__state = __1__state;
				this.__l__initialThreadId = Thread.CurrentThread.ManagedThreadId;
			}
			
			private void __m__Finally1c()
			{
				this.__1__state = -1;
				this.__7__wrap1b.Dispose();
			}
			
			private void __m__Finally1e()
			{
				this.__1__state = 1;
				this.__7__wrap1d.Dispose();
			}
			
			private void __m__Finally20()
			{
				this.__1__state = 1;
				this.__7__wrap1f.Dispose();
			}
			
			private void __m__Finally22()
			{
				this.__1__state = 4;
				this.__7__wrap21.Dispose();
			}
			
			public bool MoveNext()
			{
				try
				{
					int num = this.__1__state;
					if (num != 0)
					{
						if (num == 3)
						{
							this.__1__state = 2;
							this.__m__Finally1e();
						}
						if (num == 7)
						{
							this.__1__state = 5;
							this.__m__Finally22();
						}
					}
					else
					{

						this.__1__state = -1;
						this._commandPattern_5__b = new WildcardPattern(this.pattern, WildcardOptions.IgnoreCase);
						this._cmdletInfo_5__c = this.context.SessionState.InvokeCommand.GetCmdlet(@"Microsoft.PowerShell.Core\Get-Module");
						aa__ = CommandDiscovery.GetCommandDiscoveryPreference(this.context, SpecialVariables.PSModuleAutoLoadingPreferenceVarPath, "PSModuleAutoLoadingPreference");
						if ((aa__ != PSModuleAutoLoadingPreference.None) && ((this.commandOrigin == CommandOrigin.Internal) || ((this._cmdletInfo_5__c != null) && (this._cmdletInfo_5__c.Visibility == SessionStateEntryVisibility.Public))))
						{
							this.__7__wrap1b = ModuleUtils.GetDefaultAvailableModuleFiles(true, false, this.context).GetEnumerator();
							this.__1__state = 1;
							while (this.__7__wrap1b.MoveNext())
							{
								this._modulePath_5__e = this.__7__wrap1b.Current;
								this._moduleName_5__f = Path.GetFileNameWithoutExtension(this._modulePath_5__e);
								this._modules_5__10 = this.context.Modules.GetExactMatchModules(this._moduleName_5__f, false, true);
								this._tempModuleInfo_5__11 = null;
								if (this._modules_5__10.Count != 0)
								{
									if (!this.rediscoverImportedModules)
									{
										continue;
									}
									if (this._modules_5__10.Exists(new Predicate<PSModuleInfo>(ModuleUtils.GetMatchingCommands_b__8)))
									{
										continue;
									}
									if (this._modules_5__10.Count == 1)
									{
										this._psModule_5__12 = this._modules_5__10[0];
										this._tempModuleInfo_5__11 = new PSModuleInfo(this._psModule_5__12.Name, this._psModule_5__12.Path, null, null);
										this._tempModuleInfo_5__11.SetModuleBase(this._psModule_5__12.ModuleBase);
										this.__7__wrap1d = this._psModule_5__12.ExportedCommands.GetEnumerator();
										this.__1__state = 2;
										while (this.__7__wrap1d.MoveNext())
										{
											this._entry_5__13 = this.__7__wrap1d.Current;
											if (!this._commandPattern_5__b.IsMatch(this._entry_5__13.Value.Name))
											{
												continue;
											}
											this._current_5__14 = null;
											switch (this._entry_5__13.Value.CommandType)
											{
											case CommandTypes.Alias:
												this._current_5__14 = new AliasInfo(this._entry_5__13.Value.Name, null, this.context);
												break;
												
											case CommandTypes.Function:
												this._current_5__14 = new FunctionInfo(this._entry_5__13.Value.Name, ScriptBlock.Create(""), this.context);
												break;
												
											case CommandTypes.Filter:
												this._current_5__14 = new FilterInfo(this._entry_5__13.Value.Name, ScriptBlock.Create(""), this.context);
												break;
												
											case CommandTypes.Cmdlet:
												this._current_5__14 = new CmdletInfo(this._entry_5__13.Value.Name, null, null, null, this.context);
												break;
												
											case CommandTypes.Workflow:
												this._current_5__14 = new WorkflowInfo(this._entry_5__13.Value.Name, ScriptBlock.Create(""), this.context);
												break;
											}
											this._current_5__14.SetModule(this._tempModuleInfo_5__11);
											this.__2__current = this._current_5__14;
											this.__1__state = 3;
											return true;
											this.__1__state = 2;
										}
										this.__m__Finally1e();
										continue;
									}
								}
								this._moduleShortName_5__15 = Path.GetFileNameWithoutExtension(this._modulePath_5__e);
								this._exportedCommands_5__16 = AnalysisCache.GetExportedCommands(this._modulePath_5__e, false, this.context);
								if (this._exportedCommands_5__16 != null)
								{
									this._tempModuleInfo_5__11 = new PSModuleInfo(this._moduleShortName_5__15, this._modulePath_5__e, null, null);
									if (InitialSessionState.IsEngineModule(this._moduleShortName_5__15))
									{
										this._tempModuleInfo_5__11.SetModuleBase(Utils.GetApplicationBase(Utils.DefaultPowerShellShellID));
									}
									this.__7__wrap1f = this._exportedCommands_5__16.Keys.GetEnumerator();
									this.__1__state = 4;
									while (this.__7__wrap1f.MoveNext())
									{
										this._exportedCommand_5__17 = this.__7__wrap1f.Current;
										if (this._commandPattern_5__b.IsMatch(this._exportedCommand_5__17))
										{
											this.__7__wrap21 = this._exportedCommands_5__16[this._exportedCommand_5__17].GetEnumerator();
											this.__1__state = 5;
											while (this.__7__wrap21.MoveNext())
											{
												this._commandType_5__18 = this.__7__wrap21.Current;
												this._shouldExportCommand_5__19 = true;
												if ((this.context.InitialSessionState != null) && (this.commandOrigin == CommandOrigin.Runspace))
												{
													foreach (SessionStateCommandEntry entry in this.context.InitialSessionState.Commands[this._exportedCommand_5__17])
													{
														string b = null;
														if (entry.Module != null)
														{
															b = entry.Module.Name;
														}
														else if (entry.PSSnapIn != null)
														{
															b = entry.PSSnapIn.Name;
														}
														if (string.Equals(this._moduleShortName_5__15, b, StringComparison.OrdinalIgnoreCase) && (entry.Visibility == SessionStateEntryVisibility.Private))
														{
															this._shouldExportCommand_5__19 = false;
														}
													}
												}
												if (!this._shouldExportCommand_5__19)
												{
													continue;
												}
												this._current_5__1a = null;
												switch (this._commandType_5__18)
												{
												case CommandTypes.Alias:
													this._current_5__1a = new AliasInfo(this._exportedCommand_5__17, null, this.context);
													break;
													
												case CommandTypes.Function:
													this._current_5__1a = new FunctionInfo(this._exportedCommand_5__17, ScriptBlock.Create(""), this.context);
													break;
													
												case CommandTypes.Cmdlet:
													this._current_5__1a = new CmdletInfo(this._exportedCommand_5__17, null, null, null, this.context);
													break;
													
												case CommandTypes.Workflow:
													this._current_5__1a = new WorkflowInfo(this._exportedCommand_5__17, ScriptBlock.Create(""), this.context);
													break;
												}
												if (this._current_5__1a != null)
												{
													this._current_5__1a.SetModule(this._tempModuleInfo_5__11);
												}
												this.__2__current = this._current_5__1a;
												this.__1__state = 7;
												return true;
											Label_060F:
													this.__1__state = 5;
											}
											this.__m__Finally22();
										}
									}
									this.__m__Finally20();
								}
							}
							this.__m__Finally1c();
						}
					}
					return false;
				}
				finally
				{
					(this as IDisposable).Dispose();
				}
			}
			
			[DebuggerHidden]
			IEnumerator<CommandInfo> IEnumerable<CommandInfo>.GetEnumerator()
			{
				ModuleUtils._GetMatchingCommands_d__a _a;
				if ((Thread.CurrentThread.ManagedThreadId == this.__l__initialThreadId) && (this.__1__state == -2))
				{
					this.__1__state = 0;
					_a = this;
				}
				else
				{
					_a = new ModuleUtils._GetMatchingCommands_d__a(0);
				}
				_a.pattern = this.__3__pattern;
				_a.context = this.__3__context;
				_a.commandOrigin = this.__3__commandOrigin;
				_a.rediscoverImportedModules = this.__3__rediscoverImportedModules;
				return _a;
			}
			
			[DebuggerHidden]
			IEnumerator IEnumerable.GetEnumerator()
			{
				return (this as IEnumerable<CommandInfo>).GetEnumerator();
			}
			
			[DebuggerHidden]
			void IEnumerator.Reset()
			{
				throw new NotSupportedException();
			}
			
			void IDisposable.Dispose()
			{
				switch (this.__1__state)
				{
				case 1:
				case 2:
				case 3:
				case 4:
				case 5:
				case 7:
					try
					{
						switch (this.__1__state)
						{
						case 2:
						case 3:
							try
							{
							}
							finally
							{
								this.__m__Finally1e();
							}
							break;
							
						case 4:
						case 5:
						case 7:
							try
							{
								switch (this.__1__state)
								{
								case 5:
								case 7:
									try
									{
									}
									finally
									{
										this.__m__Finally22();
									}
									return;
								}
							}
							finally
							{
								this.__m__Finally20();
							}
							return;
						}
					}
					finally
					{
						this.__m__Finally1c();
					}
					break;
					
				case 6:
					break;
					
				default:
					return;
				}
			}
			
			CommandInfo IEnumerator<CommandInfo>.Current
			{
				[DebuggerHidden]
				get
				{
					return this.__2__current;
				}
			}
			
			object IEnumerator.Current
			{
				[DebuggerHidden]
				get
				{
					return this.__2__current;
				}
			}
		}
        
    }
}

