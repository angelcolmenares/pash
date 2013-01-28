using Microsoft.Management.Odata;
using Microsoft.Management.Odata.Common;
using Microsoft.Management.Odata.Core;
using Microsoft.Management.Odata.Schema;
using Microsoft.Management.Odata.Tracing;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Services;
using System.Data.Services.Providers;
using System.Linq;
using System.Linq.Expressions;
using System.Management.Automation;
using System.Net;
using System.Security.Principal;
using System.Timers;

namespace Microsoft.Management.Odata.PS
{
	internal class PSCommand : ICommand, IDisposable
	{
		private bool disposed;

		private object syncObject;

		private Envelope<PSRunspace, UserContext> runspace;

		private ResourceType entityType;

		private PSCmdletInfo cmdletInfo;

		private Dictionary<string, object> parameters;

		private ulong commonParameterFlag;

		private AsyncDataStore<DSResource> dataStore;

		private System.Management.Automation.PowerShell powerShell;

		private IAsyncResult asynchronousResult;

		private bool isExecutionCompleted;

		private bool isExceedsMaxExecutionTime;

		private PswsTimer timer;

		private PSDataCollection<PSObject> output;

		private CommandType commandType;

		public PSCommand(Envelope<PSRunspace, UserContext> runspace, ResourceType entityType, PSCmdletInfo cmdletInfo, CommandType commandType = (CommandType)1)
		{
			this.syncObject = new object();
			this.runspace = runspace;
			this.cmdletInfo = cmdletInfo;
			this.entityType = entityType;
			this.parameters = new Dictionary<string, object>();
			this.powerShell = null;
			this.timer = new PswsTimer(new ElapsedEventHandler(this.TimerCallback), DataServiceController.Current.Configuration.PowerShell.Quotas.MaxExecutionTime, false, false);
			this.commandType = commandType;
		}

		public bool AddArrayFieldParameter(string fieldParameter, IEnumerable<object> values)
		{
			string str = null;
			fieldParameter.ThrowIfNullOrEmpty("fieldParameter", Resources.FieldParameterNullOrEmpty, new object[0]);
			if (this.cmdletInfo.FieldParameterMapping.TryGetValue(fieldParameter, out str))
			{
				return this.AddArrayParameter(str, values);
			}
			else
			{
				TraceHelper.Current.DebugMessage(string.Concat("Field parameter", fieldParameter, " not found in list of FieldParameterMapping for cmdlet ", this.cmdletInfo.CmdletName));
				return false;
			}
		}

		protected bool AddArrayParameter(string parameter, IEnumerable<object> value)
		{
			if (!this.parameters.ContainsKey(parameter))
			{
				TypeWrapper typeWrapper = new TypeWrapper(this.cmdletInfo.GetParameterType(parameter));
				if (TypeSystem.IsArrayType(typeWrapper.Value))
				{
					this.commonParameterFlag = this.GetCommonParameterSets(parameter);
					this.parameters.Add(parameter, value.ToArray<object>());
					return true;
				}
				else
				{
					TraceHelper.Current.DebugMessage(string.Format(Resources.ParameterNotDefinedAsArray, parameter, this.cmdletInfo.CmdletName));
					return false;
				}
			}
			else
			{
				TraceHelper.Current.DebugMessage(string.Format(Resources.ParameterAlreadyAddedToCommand, parameter, this.cmdletInfo.CmdletName));
				return false;
			}
		}

		public bool AddFieldParameter(string fieldParameter, object value)
		{
			string str = null;
			bool flag;
			fieldParameter.ThrowIfNullOrEmpty("fieldParameter", Resources.FieldParameterNullOrEmpty, new object[0]);
			if (this.cmdletInfo.FieldParameterMapping.TryGetValue(fieldParameter, out str))
			{
				if (this.IsNullValueAllowedForField(fieldParameter) || value != null)
				{
					try
					{
						this.AddParameterInternal(str, value, false);
						flag = true;
					}
					catch (ArgumentException argumentException1)
					{
						ArgumentException argumentException = argumentException1;
						argumentException.Trace("Ignoring exception in AddFieldParameter");
						flag = false;
					}
					return flag;
				}
				else
				{
					throw new ArgumentException(Resources.NullValueNotAllowedForNonNullableProperty, fieldParameter);
				}
			}
			else
			{
				TraceHelper.Current.DebugMessage(string.Concat("Field parameter", fieldParameter, " not found in list of FieldParameterMapping for cmdlet ", this.cmdletInfo.CmdletName));
				return false;
			}
		}

		private void AddImmutableParameters()
		{
			foreach (string key in this.cmdletInfo.ImmutableParameters.Keys)
			{
				this.AddParameter(key, this.cmdletInfo.ImmutableParameters[key], false);
			}
		}

		public void AddParameter(string parameter, object value, bool isOptionParm = true)
		{
			this.ValidateParameter(parameter, value);
			this.AddParameterInternal(parameter, value, isOptionParm);
		}

		protected void AddParameterInternal(string parameter, object value, bool isOptionParam)
		{
			bool flag;
			object[] cmdletName = new object[2];
			cmdletName[0] = parameter;
			cmdletName[1] = this.cmdletInfo.CmdletName;
			ExceptionHelpers.ThrowArgumentExceptionIf("parameter", this.parameters.ContainsKey(parameter), Resources.ParameterAlreadyAddedToCommand, cmdletName);
			string str = "parameter";
			if (!isOptionParam)
			{
				flag = false;
			}
			else
			{
				flag = !this.cmdletInfo.IsValidOption(parameter);
			}
			object[] objArray = new object[2];
			objArray[0] = parameter;
			objArray[1] = this.cmdletInfo.CmdletName;
			ExceptionHelpers.ThrowArgumentExceptionIf(str, flag, Resources.NotValidUrlOption, objArray);
			if (this.ValidateParameterForSwitch(parameter, value))
			{
				this.commonParameterFlag = this.GetCommonParameterSets(parameter);
				this.parameters.Add(parameter, value);
				return;
			}
			else
			{
				return;
			}
		}

		public bool CanFieldBeAdded(string fieldName)
		{
			return this.cmdletInfo.FieldParameterMapping.Keys.Contains<string>(fieldName);
		}

		private static Exception CheckInvocationStateInfoForException(PSInvocationStateInfo invokeStateInfo)
		{
			if (invokeStateInfo.State != PSInvocationState.Failed)
			{
				return null;
			}
			else
			{
				return invokeStateInfo.Reason;
			}
		}

		internal static Exception CheckPowershellForException(string cmdletName, CommandType commandType, System.Management.Automation.PowerShell powerShell)
		{
			Exception streamException = PSCommand.CheckInvocationStateInfoForException(powerShell.InvocationStateInfo);
			if (streamException == null)
			{
				streamException = PSCommand.GetStreamException(cmdletName, powerShell.Streams);
			}
			string commandText = powerShell.Commands.Commands[0].CommandText;
			if (streamException != null)
			{
				if (!(streamException.GetType() == typeof(ItemNotFoundException)) || commandType != CommandType.Read && commandType != CommandType.Delete && commandType != CommandType.Update)
				{
					streamException = new CommandInvocationFailedException(commandText, streamException);
				}
				else
				{
					if (commandType == CommandType.Read || commandType == CommandType.Delete)
					{
						string[] str = new string[7];
						str[0] = "Cmdlet ";
						str[1] = cmdletName;
						str[2] = " generated ItemNotFoundException exception. Command string was ";
						str[3] = commandText;
						str[4] = ". But this is a ";
						str[5] = commandType.ToString();
						str[6] = "operation, so suppressing that message";
						TraceHelper.Current.DebugMessage(string.Concat(str));
						streamException = null;
					}
					else
					{
						string[] strArrays = new string[5];
						strArrays[0] = "Cmdlet ";
						strArrays[1] = cmdletName;
						strArrays[2] = " generated ItemNotFoundException exception. Command string was ";
						strArrays[3] = commandText;
						strArrays[4] = ". But the operation is Update, so throwing DataServiceException with 404";
						TraceHelper.Current.DebugMessage(string.Concat(strArrays));
						streamException = new DataServiceException(0x194, null, Resources.ResourceNotFoundException, null, streamException);
					}
				}
			}
			return streamException;
		}

		private void ConvertCollectionParameters()
		{
			foreach (string collection in new List<string>(this.parameters.Keys))
			{
				this.parameters[collection] = TypeSystem.ConvertEnumerableToCollection(this.parameters[collection], this.cmdletInfo.GetParameterType(collection));
			}
		}

		private void DataAddedEventHandler(object obj, DataAddedEventArgs eventArgs)
		{
			TraceHelper.Current.Correlate();
			lock (this.syncObject)
			{
				if (!this.isExceedsMaxExecutionTime)
				{
					PSDataCollection<PSObject> pSObjects = obj as PSDataCollection<PSObject>;
					if (pSObjects != null)
					{
						DSResource dSResource = SerializerBase.SerializeEntity(pSObjects[eventArgs.Index], this.entityType);
						this.dataStore.Add(dSResource);
					}
				}
				else
				{
					throw new DataServiceException();
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
					this.runspace = null;
				}
				if (this.timer != null)
				{
					this.timer.Dispose();
					this.timer = null;
				}
			}
			this.disposed = true;
		}

		private void ExecutionCompletionEventHandler(IAsyncResult ar)
		{
			Exception dataServiceException;
			lock (this.syncObject)
			{
				this.timer.Stop();
				this.isExecutionCompleted = true;
				if (!this.isExceedsMaxExecutionTime)
				{
					dataServiceException = PSCommand.CheckPowershellForException(this.cmdletInfo.CmdletName, this.commandType, this.powerShell);
				}
				else
				{
					object[] cmdletName = new object[1];
					cmdletName[0] = this.cmdletInfo.CmdletName;
					dataServiceException = new DataServiceException(0x193, ExceptionHelpers.GetDataServiceExceptionMessage(HttpStatusCode.Forbidden, Resources.CmdletExecutionQuotaExceeded, cmdletName));
					TraceHelper.Current.CommandExecutionTimeExceeded(this.cmdletInfo.CmdletName, this.runspace.Borrower.Name, DataServiceController.Current.Configuration.PowerShell.Quotas.MaxExecutionTime, 0);
					DataServiceController.Current.PerfCounters.SystemQuotaViolationsPerSec.Increment();
					DataServiceController.Current.QuotaSystem.SystemQuotaViolation.Increment();
				}
				if (this.output != null && dataServiceException == null)
				{
					foreach (PSObject pSObject in this.output)
					{
						if (pSObject == null)
						{
							continue;
						}
						DSResource dSResource = SerializerBase.SerializeEntity(pSObject, this.entityType);
						this.dataStore.Add(dSResource);
					}
				}
				this.dataStore.Completed(dataServiceException);
				this.powerShell.Trace();
			}
			this.Dispose();
		}

		private ulong GetCommonParameterSets(string parameter)
		{
			ulong num = this.cmdletInfo.FindParameterSets(parameter);
			object[] cmdletName = new object[2];
			cmdletName[0] = parameter;
			cmdletName[1] = this.cmdletInfo.CmdletName;
			ExceptionHelpers.ThrowArgumentExceptionIf("parameter", num == (long)0, Resources.ParameterNotFoundInCommand, cmdletName);
			if (this.parameters.Count > 0)
			{
				ulong num1 = this.commonParameterFlag & num;
				if (num1 != (long)0)
				{
					num = num1;
				}
				else
				{
					object[] objArray = new object[2];
					objArray[0] = parameter;
					objArray[1] = this.cmdletInfo.CmdletName;
					throw new ArgumentException(ExceptionHelpers.GetExceptionMessage(Resources.ParametersAddedFromExclusiveSets, objArray));
				}
			}
			return num;
		}

		private static Exception GetStreamException(string cmdletName, PSDataStreams streams)
		{
			string str;
			string empty;
			if (streams.Error.Count <= 0)
			{
				return null;
			}
			else
			{
				PSDataCollection<ErrorRecord> error = streams.Error;
				string str1 = error[0].ToString();
				if (error.Count >= 2)
				{
					str = error[1].ToString();
				}
				else
				{
					str = string.Empty;
				}
				string str2 = str;
				if (error.Count >= 3)
				{
					empty = error[2].ToString();
				}
				else
				{
					empty = string.Empty;
				}
				string str3 = empty;
				Tracer tracer = new Tracer();
				tracer.CommandInvocationError(cmdletName, (uint)error.Count, str1, str2, str3);
				return error[0].Exception;
			}
		}

		public IEnumerator<DSResource> InvokeAsync(Expression expression, bool noStreamingResponse)
		{
			IEnumerator<DSResource> enumerator;
			this.AddImmutableParameters();
			this.cmdletInfo.VerifyMandatoryParameterAdded(this.parameters.Keys, this.commonParameterFlag);
			this.ConvertCollectionParameters();
			SafeRefCountedContainer<WindowsIdentity> authorizedUserIdentity = DataServiceController.Current.GetAuthorizedUserIdentity(this.runspace.Borrower);
			WindowsImpersonationContext windowsImpersonationContext = authorizedUserIdentity.Instance.Impersonate();
			using (windowsImpersonationContext)
			{
				TraceHelper.Current.BeginOperation0("PowerShell.Create");
				this.powerShell = System.Management.Automation.PowerShell.Create();
				TraceHelper.Current.EndOperation("PowerShell.Create");
				TraceHelper.Current.BeginOperation0("RunspaceContext.Create");
				DefaultRunspaceContext defaultRunspaceContext = DefaultRunspaceContext.Create(this.runspace.Item.Runspace);
				using (defaultRunspaceContext)
				{
					TraceHelper.Current.EndOperation("RunspaceContext.Create");
					EventHandler<DataAddedEventArgs> eventHandler = new EventHandler<DataAddedEventArgs>(this.DataAddedEventHandler);
					if (this.entityType == null)
					{
						eventHandler = null;
					}
					IEnumerator<DSResource> enumerator1 = this.InvokeCmdletAsync(this.powerShell, expression, eventHandler, new AsyncCallback(this.ExecutionCompletionEventHandler), noStreamingResponse);
					this.ExecutionCompletionEventHandler(this.asynchronousResult);
					enumerator = enumerator1;
				}
			}
			return enumerator;
		}

		internal IEnumerator<DSResource> InvokeCmdletAsync(System.Management.Automation.PowerShell powerShell, Expression expression, EventHandler<DataAddedEventArgs> dataAddedEventHandler, AsyncCallback executionCompletionCallback, bool noStreamingResponse)
		{
			Tracer tracer = new Tracer();
			this.dataStore = new AsyncDataStore<DSResource>(expression, noStreamingResponse);
			this.output = new PSDataCollection<PSObject>();
			tracer.CommandInvocationStart(this.cmdletInfo.CmdletName);
			powerShell.Runspace = this.runspace.Item.Runspace;
			powerShell.AddCommand(this.cmdletInfo.CmdletName);
			foreach (string key in this.parameters.Keys)
			{
				if (!this.cmdletInfo.IsSwitch(key))
				{
					powerShell.AddParameter(key, this.parameters[key]);
				}
				else
				{
					powerShell.AddParameter(key);
				}
			}
			this.isExecutionCompleted = false;
			using (OperationTracer operationTracer = new OperationTracer(new Action<string>(TraceHelper.Current.CmdletExecutionStart), new Action<string>(TraceHelper.Current.CmdletExecutionEnd), powerShell.Commands.ToTraceMessage()))
			{
				try
				{
					this.timer.Start();
					powerShell.Invoke<PSObject>(null, this.output, Utils.GetPSInvocationSettings());
				}
				catch (CommandNotFoundException commandNotFoundException1)
				{
					CommandNotFoundException commandNotFoundException = commandNotFoundException1;
					throw new CommandInvocationFailedException(powerShell.Commands.Commands[0].CommandText, commandNotFoundException);
				}
				catch (ParameterBindingException parameterBindingException1)
				{
					ParameterBindingException parameterBindingException = parameterBindingException1;
					throw new CommandInvocationFailedException(powerShell.Commands.Commands[0].CommandText, parameterBindingException);
				}
				catch (CmdletInvocationException cmdletInvocationException1)
				{
					CmdletInvocationException cmdletInvocationException = cmdletInvocationException1;
					throw new CommandInvocationFailedException(powerShell.Commands.Commands[0].CommandText, cmdletInvocationException);
				}
			}
			return new BlockingEnumerator<DSResource>(this.dataStore);
		}

		private bool IsNullValueAllowedForField(string fieldName)
		{
			Func<ResourceProperty, bool> func = null;
			if (this.entityType == null)
			{
				return true;
			}
			else
			{
				ReadOnlyCollection<ResourceProperty> properties = this.entityType.Properties;
				if (func == null)
				{
					func = (ResourceProperty item) => item.Name == fieldName;
				}
				ResourceProperty resourceProperty = properties.FirstOrDefault<ResourceProperty>(func);
				if (resourceProperty != null)
				{
					if (!resourceProperty.ResourceType.IsPrimitive() || resourceProperty.ResourceType.IsNullable())
					{
						return true;
					}
					else
					{
						return false;
					}
				}
				else
				{
					object[] name = new object[2];
					name[0] = fieldName;
					name[1] = this.entityType.Name;
					throw new ArgumentException(ExceptionHelpers.GetExceptionMessage(Resources.PropertyNotFoundInODataResource, name));
				}
			}
		}

		internal string TestHookGetCmdletName()
		{
			return this.cmdletInfo.CmdletName;
		}

		internal Dictionary<string, object> TestHookGetParameters()
		{
			return this.parameters;
		}

		internal System.Management.Automation.PowerShell TestHookGetPowerShell()
		{
			return this.powerShell;
		}

		internal PswsTimer TestHookGetTimer()
		{
			return this.timer;
		}

		internal PSDataCollection<PSObject> TestHookOutputObject()
		{
			return this.output;
		}

		internal void TestHookSetTimeoutFlag()
		{
			this.isExceedsMaxExecutionTime = true;
		}

		private void TimerCallback(object obj, ElapsedEventArgs args)
		{
			lock (this.syncObject)
			{
				if (!this.isExecutionCompleted)
				{
					this.isExceedsMaxExecutionTime = true;
					this.powerShell.Stop();
				}
			}
		}

		private void ValidateParameter(string parameterName, object value)
		{
			parameterName.ThrowIfNullOrEmpty("parameterName", Resources.FieldParameterNullOrEmpty, new object[0]);
			object[] objArray = new object[1];
			objArray[0] = parameterName;
			value.ThrowIfNull("value", Resources.NullPassedForParameter, objArray);
		}

		private bool ValidateParameterForSwitch(string parameter, object value)
		{
			if (!this.cmdletInfo.IsSwitch(parameter))
			{
				return true;
			}
			else
			{
				bool flag = false;
				try
				{
					flag = (bool)TypeConverter.ConvertTo(value, typeof(bool));
				}
				catch (InvalidCastException invalidCastException1)
				{
					InvalidCastException invalidCastException = invalidCastException1;
					object[] cmdletName = new object[2];
					cmdletName[0] = parameter;
					cmdletName[1] = this.cmdletInfo.CmdletName;
					throw new ArgumentException(ExceptionHelpers.GetExceptionMessage(invalidCastException, Resources.NotValidUrlOption, cmdletName), invalidCastException);
				}
				if (!flag)
				{
					TraceHelper.Current.DebugMessage(string.Concat("Switch parameter ", parameter, " with false value is passed. So not adding that in the PowerShell pipeline"));
				}
				return flag;
			}
		}
	}
}