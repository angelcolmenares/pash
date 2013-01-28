using System;
using System.Collections;
using System.Management.Automation;
using System.Management.Automation.Host;
using System.Management.Automation.Tracing;

namespace Microsoft.PowerShell.Activities
{
	internal class DirectExecutionActivitiesCommandRuntime : ICommandRuntime
	{
		private PSDataCollection<PSObject> _output;

		private ActivityImplementationContext _implementationContext;

		private Type _cmdletType;

		public PSTransactionContext CurrentPSTransaction
		{
			get
			{
				throw new InvalidOperationException();
			}
		}

		public PSDataCollection<DebugRecord> Debug
		{
			get;
			set;
		}

		public PSDataCollection<ErrorRecord> Error
		{
			get;
			set;
		}

		public PSHost Host
		{
			get
			{
				return null;
			}
		}

		public PSDataCollection<ProgressRecord> Progress
		{
			get;
			set;
		}

		public PSDataCollection<VerboseRecord> Verbose
		{
			get;
			set;
		}

		public PSDataCollection<WarningRecord> Warning
		{
			get;
			set;
		}

		public DirectExecutionActivitiesCommandRuntime(PSDataCollection<PSObject> output, ActivityImplementationContext implementationContext, Type cmdletType)
		{
			if (output != null)
			{
				if (implementationContext != null)
				{
					if (cmdletType != null)
					{
						this._output = output;
						this._implementationContext = implementationContext;
						this._cmdletType = cmdletType;
						return;
					}
					else
					{
						throw new ArgumentNullException("cmdletType");
					}
				}
				else
				{
					throw new ArgumentNullException("implementationContext");
				}
			}
			else
			{
				throw new ArgumentNullException("output");
			}
		}

		public bool ShouldContinue(string query, string caption)
		{
			return true;
		}

		public bool ShouldContinue(string query, string caption, ref bool yesToAll, ref bool noToAll)
		{
			return true;
		}

		public bool ShouldProcess(string target)
		{
			return true;
		}

		public bool ShouldProcess(string target, string action)
		{
			return true;
		}

		public bool ShouldProcess(string verboseDescription, string verboseWarning, string caption)
		{
			return true;
		}

		public bool ShouldProcess(string verboseDescription, string verboseWarning, string caption, out ShouldProcessReason shouldProcessReason)
		{
			shouldProcessReason = ShouldProcessReason.None;
			return true;
		}

		public void ThrowTerminatingError(ErrorRecord errorRecord)
		{
			if (errorRecord.Exception == null)
			{
				throw new InvalidOperationException(errorRecord.ToString());
			}
			else
			{
				throw errorRecord.Exception;
			}
		}

		public bool TransactionAvailable()
		{
			return false;
		}

		public void WriteCommandDetail(string text)
		{
			PowerShellTraceSource traceSource = PowerShellTraceSourceFactory.GetTraceSource();
			traceSource.WriteMessage(text);
		}

		public void WriteDebug(string text)
		{
			if (this.Debug != null)
			{
				if (text != null)
				{
					this.Debug.Add(new DebugRecord(text));
				}
				return;
			}
			else
			{
				return;
			}
		}

		public void WriteError(ErrorRecord errorRecord)
		{
			ActionPreference value;
			if (this.Error != null)
			{
				ErrorRecord errorRecord1 = new ErrorRecord(errorRecord.Exception, string.Concat(errorRecord.FullyQualifiedErrorId, (char)44, this._cmdletType.FullName), errorRecord.CategoryInfo.Category, errorRecord.TargetObject);
				ActionPreference? errorAction = this._implementationContext.ErrorAction;
				if (!errorAction.HasValue)
				{
					value = ActionPreference.Continue;
				}
				else
				{
					ActionPreference? nullable = this._implementationContext.ErrorAction;
					value = nullable.Value;
				}
				ActionPreference actionPreference = value;
				ActionPreference actionPreference1 = actionPreference;
				switch (actionPreference1)
				{
					case ActionPreference.SilentlyContinue:
					case ActionPreference.Ignore:
					{
						return;
					}
					case ActionPreference.Stop:
					{
						this.ThrowTerminatingError(errorRecord1);
						return;
					}
					case ActionPreference.Continue:
					case ActionPreference.Inquire:
					{
						if (errorRecord == null)
						{
							return;
						}
						this.Error.Add(errorRecord1);
						return;
					}
					default:
					{
						return;
					}
				}
			}
			else
			{
				return;
			}
		}

		public void WriteObject(object sendToPipeline)
		{
			this._output.Add(PSObject.AsPSObject(sendToPipeline));
		}

		public void WriteObject(object sendToPipeline, bool enumerateCollection)
		{
			if (!enumerateCollection)
			{
				this.WriteObject(sendToPipeline);
				return;
			}
			else
			{
				IEnumerator enumerator = LanguagePrimitives.GetEnumerator(sendToPipeline);
				if (enumerator != null)
				{
					while (enumerator.MoveNext())
					{
						this.WriteObject(enumerator.Current);
					}
					return;
				}
				else
				{
					this.WriteObject(sendToPipeline);
					return;
				}
			}
		}

		public void WriteProgress(ProgressRecord progressRecord)
		{
			this.WriteProgress((long)1, progressRecord);
		}

		public void WriteProgress(long sourceId, ProgressRecord progressRecord)
		{
			if (this.Progress != null)
			{
				if (progressRecord != null)
				{
					this.Progress.Add(progressRecord);
				}
				return;
			}
			else
			{
				return;
			}
		}

		public void WriteVerbose(string text)
		{
			bool hasValue;
			bool? verbose = this._implementationContext.Verbose;
			if (!verbose.GetValueOrDefault())
			{
				hasValue = true;
			}
			else
			{
				hasValue = !verbose.HasValue;
			}
			if (!hasValue)
			{
				if (this.Verbose != null)
				{
					if (text != null)
					{
						this.Verbose.Add(new VerboseRecord(text));
					}
					return;
				}
				else
				{
					return;
				}
			}
			else
			{
				return;
			}
		}

		public void WriteWarning(string text)
		{
			bool hasValue;
			ActionPreference? warningAction = this._implementationContext.WarningAction;
			if (warningAction.GetValueOrDefault() != ActionPreference.Continue)
			{
				hasValue = true;
			}
			else
			{
				hasValue = !warningAction.HasValue;
			}
			if (!hasValue)
			{
				if (this.Warning != null)
				{
					if (text != null)
					{
						this.Warning.Add(new WarningRecord(text));
					}
					return;
				}
				else
				{
					return;
				}
			}
			else
			{
				return;
			}
		}
	}
}