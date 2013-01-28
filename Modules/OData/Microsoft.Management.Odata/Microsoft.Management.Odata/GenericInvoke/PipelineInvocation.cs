using Microsoft.Management.Odata;
using Microsoft.Management.Odata.Common;
using Microsoft.Management.Odata.Core;
using Microsoft.Management.Odata.PS;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Services;
using System.Data.Services.Providers;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Management.Automation;
using System.Management.Automation.Language;
using System.Management.Automation.Runspaces;
using System.Security.Principal;
using System.Threading;
using System.Timers;

namespace Microsoft.Management.Odata.GenericInvoke
{
	internal class PipelineInvocation : ICommand, IDisposable
	{
		public const int MaxWaitMsec = 0x7530;

		public const int MaxWaitDuringCancel = 0x1d4c0;

		private bool disposed;

		private Envelope<PSRunspace, UserContext> runspace;

		private System.Management.Automation.PowerShell powerShell;

		private SafeRefCountedContainer<WindowsIdentity> executionToken;

		private ResourceType entityType;

		private PSDataCollection<PSObject> outputObjects;

		private ManualResetEvent finished;

		private PswsTimer timer;

		private bool outputQuotaExceeded;

		private bool userCacheLocked;

		public string Command
		{
			get;
			private set;
		}

		public Collection<ErrorRecord> Errors
		{
			get;
			private set;
		}

		public DateTime ExpirationTime
		{
			get;
			private set;
		}

		public Guid ID
		{
			get;
			private set;
		}

		public string Output
		{
			get;
			private set;
		}

		public string OutputFormat
		{
			get;
			private set;
		}

		public PipelineState Status
		{
			get;
			private set;
		}

		public AsyncCallback TestHookCompletionCallback
		{
			get;
			set;
		}

		public int WaitMsec
		{
			get;
			private set;
		}

		public PipelineInvocation(Envelope<PSRunspace, UserContext> runspace, ResourceType entityType)
		{
			UserData userDatum = null;
			this.runspace = runspace;
			this.entityType = entityType;
			this.ID = Guid.NewGuid();
			this.Status = PipelineState.Executing;
			this.WaitMsec = 0;
			this.OutputFormat = null;
			this.ExpirationTime = DateTimeHelper.UtcNow;
			this.outputQuotaExceeded = false;
			this.outputObjects = new PSDataCollection<PSObject>();
			this.Errors = new Collection<ErrorRecord>();
			this.finished = new ManualResetEvent(false);
			this.TestHookCompletionCallback = new AsyncCallback(this.AsyncCallback);
			if (runspace != null)
			{
				this.executionToken = DataServiceController.Current.GetAuthorizedUserIdentity(runspace.Borrower);
				this.executionToken.AddRef();
				DataServiceController.Current.UserDataCache.TryLockKey(runspace.Borrower, out userDatum);
				this.userCacheLocked = true;
			}
			this.timer = new PswsTimer(new ElapsedEventHandler(this.TimerCallback), DataServiceController.Current.Configuration.PowerShell.Quotas.MaxExecutionTime, false, false);
		}

		public bool AddArrayFieldParameter(string parameter, IEnumerable<object> values)
		{
			object[] objArray = new object[2];
			objArray[0] = "AddArrayFieldParameter";
			objArray[1] = "PipelineInvocation";
			throw new NotImplementedException(ExceptionHelpers.GetExceptionMessage(Resources.NotImplementedExceptionMessage, objArray));
		}

		public bool AddFieldParameter(string parameter, object value)
		{
			Func<ResourceProperty, bool> func = null;
			if (!string.Equals(parameter, "Command", StringComparison.Ordinal))
			{
				if (!string.Equals(parameter, "OutputFormat", StringComparison.Ordinal))
				{
					if (!string.Equals(parameter, "WaitMsec", StringComparison.Ordinal))
					{
						ReadOnlyCollection<ResourceProperty> properties = this.entityType.Properties;
						if (func == null)
						{
							func = (ResourceProperty it) => string.Equals(it.Name, parameter, StringComparison.Ordinal);
						}
						if (properties.FirstOrDefault<ResourceProperty>(func) == null)
						{
							object[] name = new object[2];
							name[0] = this.entityType.Name;
							name[1] = parameter;
							throw new ArgumentException(ExceptionHelpers.GetExceptionMessage(Resources.ReadOnlyProperty, name));
						}
						else
						{
							return false;
						}
					}
					else
					{
						this.WaitMsec = (new BoundedPositiveInteger((int)value, 0x7530, true)).Value;
						return true;
					}
				}
				else
				{
					string str = value as string;
					if (str != null)
					{
						if (PipelineInvocation.ValidateOutputFormat(str))
						{
							this.OutputFormat = str;
							return true;
						}
						else
						{
							object[] objArray = new object[2];
							objArray[0] = parameter;
							objArray[1] = value;
							throw new ArgumentException(ExceptionHelpers.GetExceptionMessage(Resources.InvalidParameterValue, objArray));
						}
					}
					else
					{
						object[] objArray1 = new object[2];
						objArray1[0] = parameter;
						objArray1[1] = value;
						throw new ArgumentException(ExceptionHelpers.GetExceptionMessage(Resources.InvalidParameterValue, objArray1));
					}
				}
			}
			else
			{
				if (value == null || value as string == null)
				{
					object[] objArray2 = new object[2];
					objArray2[0] = parameter;
					objArray2[1] = value;
					throw new ArgumentException(ExceptionHelpers.GetExceptionMessage(Resources.InvalidParameterValue, objArray2));
				}
				else
				{
					this.ValidateAndBuildPipeline(value as string);
					this.Command = value as string;
					return true;
				}
			}
		}

		public void AddParameter(string parameter, object value, bool isOption = true)
		{
			throw new NotImplementedException();
		}

		internal void AsyncCallback(IAsyncResult result)
		{
			TraceHelper.Current.DebugMessage("Entering AsyncCallback");
			this.Trace("AsyncCallback entering");
			lock (this)
			{
				try
				{
					this.powerShell.EndInvoke(result);
				}
				catch (PipelineStoppedException pipelineStoppedException)
				{
					object[] command = new object[1];
					command[0] = this.Command;
					string str = string.Format(CultureInfo.CurrentCulture, Resources.CmdletExecutionQuotaExceeded, command);
					ErrorRecord errorRecord = new ErrorRecord(new DataServiceException(0x193, str), null, ErrorCategory.OperationTimeout, null);
					errorRecord.ErrorDetails = new ErrorDetails(str);
					this.Errors.Add(errorRecord);
				}
				catch (RuntimeException runtimeException1)
				{
					RuntimeException runtimeException = runtimeException1;
					this.Errors.Add(runtimeException.ErrorRecord);
				}
				catch (Exception exception1)
				{
					Exception exception = exception1;
					exception.Trace("unrecognized exception");
				}
				Collection<ErrorRecord> errorRecords = this.powerShell.Streams.Error.ReadAll();
				foreach (ErrorRecord error in this.Errors)
				{
					errorRecords.Add(error);
				}
				this.Errors = errorRecords;
				this.UpdateStatus(this.powerShell.InvocationStateInfo.State, this.Errors);
				this.finished.Set();
			}
			this.Trace("AsyncCallback before exiting");
			TraceHelper.Current.DebugMessage("Exiting AsyncCallback");
		}

		public bool CanFieldBeAdded(string fieldName)
		{
			string[] strArrays = new string[3];
			strArrays[0] = "Command";
			strArrays[1] = "OutputFormat";
			strArrays[2] = "WaitMsec";
			string[] strArrays1 = strArrays;
			return strArrays1.Contains<string>(fieldName);
		}

		internal void DataAddedEventHandler(object obj, DataAddedEventArgs eventArgs)
		{
			int length;
			using (OperationTracer operationTracer = new OperationTracer("Invoke.OutputAddedCallback"))
			{
				lock (this)
				{
					if (obj == this.outputObjects)
					{
						PSObject item = this.outputObjects[eventArgs.Index];
						string str = PipelineInvocation.TranslatePSObjectToString(item, this.OutputFormat);
						int maxResponseChars = DataServiceController.Current.Configuration.Invocation.MaxResponseChars;
						if (this.Output != null)
						{
							length = this.Output.Length;
						}
						else
						{
							length = 0;
						}
						int num = maxResponseChars - length;
						if (num < str.Length)
						{
							if (!this.outputQuotaExceeded)
							{
								this.outputQuotaExceeded = true;
								if (num > 0)
								{
									PipelineInvocation pipelineInvocation = this;
									pipelineInvocation.Output = string.Concat(pipelineInvocation.Output, str.Substring(0, num));
								}
								object[] objArray = new object[1];
								objArray[0] = DataServiceController.Current.Configuration.Invocation.MaxResponseChars;
								string str1 = string.Format(CultureInfo.CurrentCulture, Resources.InvocationOutputQuotaExceeded, objArray);
								ErrorRecord errorRecord = new ErrorRecord(new DataServiceException(0x193, str1), null, ErrorCategory.PermissionDenied, null);
								errorRecord.ErrorDetails = new ErrorDetails(str1);
								this.Errors.Add(errorRecord);
							}
							this.outputObjects.Clear();
						}
						else
						{
							PipelineInvocation pipelineInvocation1 = this;
							pipelineInvocation1.Output = string.Concat(pipelineInvocation1.Output, str);
						}
					}
					else
					{
						return;
					}
				}
			}
		}

		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposeManagedResources)
		{
			if (!this.disposed && disposeManagedResources)
			{
				if (this.runspace != null)
				{
					this.runspace.Store.Return(this.runspace);
				}
				if (this.executionToken != null)
				{
					this.executionToken.Release();
					this.executionToken = null;
				}
				if (this.timer != null)
				{
					this.timer.Dispose();
					this.timer = null;
				}
				if (this.userCacheLocked)
				{
					DataServiceController.Current.UserDataCache.TryUnlockKey(this.runspace.Borrower);
					this.userCacheLocked = false;
				}
			}
			this.disposed = true;
		}

		public bool Interrupt()
		{
			bool flag;
			lock (this)
			{
				if (this.powerShell != null)
				{
					this.powerShell.BeginStop(null, null);
					bool flag1 = this.finished.WaitOne(0x1d4c0);
					return flag1;
				}
				else
				{
					flag = true;
				}
			}
			return flag;
		}

		public IEnumerator<DSResource> InvokeAsync(Expression expression, bool noStreamingResponse)
		{
			IEnumerator<DSResource> enumerator;
			using (OperationTracer operationTracer = new OperationTracer("Entering Pipeline.InvokeAsync"))
			{
				MonitorLock monitorLock = new MonitorLock(this);
				MonitorLock monitorLock1 = monitorLock;
				using (monitorLock)
				{
					if (this.Command == null || this.OutputFormat == null)
					{
						throw new DataServiceException(ExceptionHelpers.GetExceptionMessage(Resources.InvalidInvocationData, new object[0]));
					}
					else
					{
						this.ExpirationTime = DateTimeHelper.UtcNow;
						DateTime expirationTime = this.ExpirationTime;
						this.ExpirationTime = expirationTime.AddMilliseconds((double)this.WaitMsec);
						DateTime dateTime = this.ExpirationTime;
						this.ExpirationTime = dateTime.AddSeconds((double)DataServiceController.Current.Configuration.Invocation.Lifetime);
						WindowsImpersonationContext windowsImpersonationContext = this.executionToken.Instance.Impersonate();
						using (windowsImpersonationContext)
						{
							using (OperationTracer operationTracer1 = new OperationTracer("PowerShell.Create"))
							{
								ScriptBlockAst scriptBlockAst = this.ValidateAndBuildPipeline(string.Concat(this.Command, "| ConvertTo-", this.OutputFormat));
								Runspace.DefaultRunspace = this.runspace.Item.Runspace;
								this.powerShell = scriptBlockAst.GetScriptBlock().GetPowerShell(new object[0]);
								this.powerShell.Runspace = this.runspace.Item.Runspace;
								Runspace.DefaultRunspace = null;
							}
							TraceHelper.Current.BeginOperation0("RunspaceContext.Create");
							DefaultRunspaceContext defaultRunspaceContext = DefaultRunspaceContext.Create(this.runspace.Item.Runspace);
							using (defaultRunspaceContext)
							{
								TraceHelper.Current.EndOperation("RunspaceContext.Create");
								TraceHelper.Current.PipelineStart(this.ID, this.Command, this.OutputFormat);
								this.outputObjects.DataAdded += new EventHandler<DataAddedEventArgs>(this.DataAddedEventHandler);
								this.powerShell.BeginInvoke<PSObject, PSObject>(null, this.outputObjects, Utils.GetPSInvocationSettings(), this.TestHookCompletionCallback, null);
								monitorLock1.Exit();
								this.finished.WaitOne(this.WaitMsec);
								this.timer.Start();
								monitorLock1.Enter();
								List<DSResource> dSResources = new List<DSResource>();
								dSResources.Add(this.MakeDsResource());
								TraceHelper.Current.DebugMessage("Exiting Pipeline.InvokeAsync");
								enumerator = dSResources.AsEnumerable<DSResource>().GetEnumerator();
							}
						}
					}
				}
			}
			return enumerator;
		}

		internal DSResource MakeDsResource()
		{
			TraceHelper.Current.DebugMessage("MakeDsResource entering");
			this.Trace("Inside MakeDsResource");
			DSResource dSResource = SerializerBase.SerializeEntity(this, this.entityType);
			TraceHelper.Current.DebugMessage("MakeDsResource exiting");
			return dSResource;
		}

		internal ManualResetEvent TestHookGetFinishedEvent()
		{
			return this.finished;
		}

		internal PswsTimer TestHookGetTimer()
		{
			return this.timer;
		}

		private void TimerCallback(object obj, ElapsedEventArgs args)
		{
			lock (this)
			{
				if (this.powerShell.InvocationStateInfo.State == PSInvocationState.Running)
				{
					System.Management.Automation.PowerShell powerShell = this.powerShell;
					powerShell.BeginStop((IAsyncResult ar) => {
					}
					, this);
				}
			}
		}

		internal void Trace(string context)
		{
			int count;
			if (this.Errors == null || this.Errors.Count <= 0)
			{
				count = 0;
			}
			else
			{
				count = this.Errors.Count;
			}
			int num = count;
			string empty = string.Empty;
			if (num > 0)
			{
				if (this.Errors[0].Exception != null)
				{
					empty = this.Errors[0].Exception.Message;
				}
				if (empty == string.Empty && this.Errors[0].ErrorDetails != null)
				{
					empty = this.Errors[0].ErrorDetails.Message;
				}
			}
			Guid d = this.ID;
			DateTime expirationTime = this.ExpirationTime;
			TraceHelper.Current.InvocationInstance(context, this.GetHashCode(), d.ToString(), this.Command, this.OutputFormat, expirationTime.ToString(), this.Status.ToString(), num, empty);
		}

		internal static string TranslatePSObjectToString(PSObject pso, string format)
		{
			if (string.Equals(format, "xml", StringComparison.OrdinalIgnoreCase))
			{
				PSMemberInfoCollection<PSPropertyInfo> properties = pso.Properties;
				PSPropertyInfo pSPropertyInfo = properties.FirstOrDefault<PSPropertyInfo>((PSPropertyInfo item) => string.Equals(item.Name, "InnerXml", StringComparison.OrdinalIgnoreCase));
				if (pSPropertyInfo != null)
				{
					return pSPropertyInfo.Value.ToString();
				}
			}
			return pso.ToString();
		}

		internal void UpdateStatus(PSInvocationState state, Collection<ErrorRecord> errors)
		{
			if ((errors == null || errors.Count == 0) && state == PSInvocationState.Completed)
			{
				this.Status = PipelineState.Completed;
				TraceHelper.Current.PipelineComplete(this.ID);
				return;
			}
			else
			{
				this.Status = PipelineState.Error;
				if (errors == null || errors.Count < 1)
				{
					TraceHelper.Current.OperationalPipelineError(this.ID, this.Command, 0, Resources.UnknownPipelineError);
					TraceHelper.Current.PipelineError(this.ID, this.Command, 0, Resources.UnknownPipelineError);
					return;
				}
				else
				{
					TraceHelper.Current.OperationalPipelineError(this.ID, this.Command, errors.Count, errors[0].ToString());
					TraceHelper.Current.PipelineError(this.ID, this.Command, errors.Count, errors[0].ToString());
					return;
				}
			}
		}

		internal ScriptBlockAst ValidateAndBuildPipeline(string rawString)
		{
			Token[] tokenArray = null;
			ParseError[] parseErrorArray = null;
			bool length;
			ScriptBlockAst scriptBlockAst = Parser.ParseInput(rawString, out tokenArray, out parseErrorArray);
			string str = "rawString";
			if (parseErrorArray == null)
			{
				length = false;
			}
			else
			{
				length = (int)parseErrorArray.Length != 0;
			}
			object[] objArray = new object[1];
			objArray[0] = rawString;
			ExceptionHelpers.ThrowArgumentExceptionIf(str, length, Resources.InvalidPipeline, objArray);
			return scriptBlockAst;
		}

		internal static bool ValidateOutputFormat(string format)
		{
			string str = format;
			int num = 0;
			while (num < str.Length)
			{
				char chr = str[num];
				if (char.IsLetterOrDigit(chr) || chr == '\u005F')
				{
					num++;
				}
				else
				{
					bool flag = false;
					return flag;
				}
			}
			return true;
		}
	}
}