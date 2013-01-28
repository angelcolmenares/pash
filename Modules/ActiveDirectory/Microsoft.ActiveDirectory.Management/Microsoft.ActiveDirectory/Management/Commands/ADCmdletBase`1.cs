using Microsoft.ActiveDirectory;
using Microsoft.ActiveDirectory.Management;
using Microsoft.ActiveDirectory.Management.Provider;
using System;
using System.Collections.Generic;
using System.DirectoryServices.Protocols;
using System.Globalization;
using System.Management.Automation;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	public class ADCmdletBase<P> : PSCmdlet, IADCmdletMessageWriter, IADCustomExceptionFiltering, IDynamicParameters
	where P : ADParameterSet, new()
	{
		private const string _debugCategory = "ADCmdletBase";

		protected internal P _cmdletParameters;

		private ADSessionInfo _sessionInfo;

		private ADServerType? _connectedStore;

		private ADRootDSE _cachedRootDSE;

		private ADSessionInfo _pipelineSessionInfo;

		private ADSessionOptions _cmdletSessionOptions;

		private CmdletSessionInfo _cachedCmdletSessionInfo;

		private SafeSessionCache _cmdletCachedSession;

		private ADCmdletCache _cmdletSessionCache;

		private List<string> _warningBuffer;

		private List<ErrorRecord> _errorBuffer;

		private IADExceptionFilter _exceptionFilter;

		private ADRecordExceptionHandler _recordExceptionHandler;

		private CmdletSubroutinePipeline _beginProcessPipeline;

		private CmdletSubroutinePipeline _processRecordPipeline;

		private CmdletSubroutinePipeline _endProcessPipeline;

		private bool _processingRecord;

		private ADCmdletBase<P>.CmdletDispose _cmdletDisposeEvent;

		internal CmdletSubroutinePipeline BeginProcessPipeline
		{
			get
			{
				return this._beginProcessPipeline;
			}
		}

		protected string EffectiveDomainName
		{
			get
			{
				string userDomainName = Environment.UserDomainName;
				if (!string.IsNullOrEmpty(userDomainName))
				{
					return userDomainName;
				}
				else
				{
					throw new ArgumentException(StringResources.RequiresDomainCredentials, "Credential");
				}
			}
		}

		protected string EffectiveUserName
		{
			get
			{
				return Environment.UserName;
			}
		}

		internal CmdletSubroutinePipeline EndProcessPipeline
		{
			get
			{
				return this._endProcessPipeline;
			}
		}

		IADExceptionFilter Microsoft.ActiveDirectory.Management.Commands.IADCustomExceptionFiltering.ExceptionFilter
		{
			get
			{
				return this._exceptionFilter;
			}
		}

		internal CmdletSubroutinePipeline ProcessRecordPipeline
		{
			get
			{
				return this._processRecordPipeline;
			}
		}

		public ADCmdletBase()
		{
			this._cmdletParameters = Activator.CreateInstance<P>();
			this._connectedStore = null;
			this._recordExceptionHandler = new ADRecordExceptionHandler();
			this._beginProcessPipeline = new CmdletSubroutinePipeline();
			this._processRecordPipeline = new CmdletSubroutinePipeline();
			this._endProcessPipeline = new CmdletSubroutinePipeline();
			ADExceptionFilter aDExceptionFilter = new ADExceptionFilter();
			aDExceptionFilter.Add(this._recordExceptionHandler);
			aDExceptionFilter.Add(new ADPipelineExceptionHandler(this));
			aDExceptionFilter.Add(new ADSystemExceptionHandler());
			this._exceptionFilter = aDExceptionFilter;
			ADCmdletBase<P> aDCmdletBase = this;
			this.RegisterDisposeCallback(new ADCmdletBase<P>.CmdletDispose(aDCmdletBase.Dispose));
			this._cmdletSessionCache = new ADCmdletCache();
			this._warningBuffer = new List<string>();
			this._errorBuffer = new List<ErrorRecord>();
		}

		private void ApplyCmdletSessionOptions(ADSessionInfo sessionInfo)
		{
			if (this._cmdletSessionOptions != null)
			{
				this._cmdletSessionOptions.CopyValuesTo(sessionInfo.Options);
			}
		}

		protected sealed override void BeginProcessing()
		{
			DebugLogger.LogInfo("ADCmdletBase", "Entering BeginProcessing");
			bool flag = false;
			try
			{
				try
				{
					this._beginProcessPipeline.Invoke();
				}
				catch (Exception exception1)
				{
					Exception exception = exception1;
					flag = true;
					this.ProcessError(exception);
					flag = false;
				}
			}
			finally
			{
				if (flag)
				{
					this.InvokeDisposeCallback();
				}
				DebugLogger.LogInfo("ADCmdletBase", "Exiting BeginProcessing");
			}
		}

		internal virtual ErrorRecord ConstructErrorRecord(Exception e)
		{
			object obj;
			IADErrorTarget aDErrorTarget = this as IADErrorTarget;
			IHasErrorCode hasErrorCode = e as IHasErrorCode;
			string str = "ActiveDirectoryCmdlet:";
			if (hasErrorCode == null)
			{
				if (e != null)
				{
					str = string.Concat(str, e.GetType().ToString());
				}
			}
			else
			{
				int errorCode = hasErrorCode.ErrorCode;
				str = string.Concat("ActiveDirectoryServer:", errorCode.ToString());
			}
			Exception exception = e;
			string str1 = str;
			if (aDErrorTarget != null)
			{
				obj = aDErrorTarget.CurrentIdentity(e);
			}
			else
			{
				obj = null;
			}
			return ADUtilities.GetErrorRecord(exception, str1, obj);
		}

		private void ConstructSessionCache(ADSessionInfo info)
		{
			if (this._cmdletCachedSession == null)
			{
				this._cmdletCachedSession = new SafeSessionCache(info);
			}
		}

		private ADSessionInfo CreateSessionFromParameters()
		{
			ADSessionInfo aDSessionInfo;
			AuthType authType;
			ADSessionInfo currentDriveSessionInfo = null;
			bool flag = ProviderUtils.IsCurrentDriveAD(base.SessionState);
			if (flag)
			{
				currentDriveSessionInfo = ProviderUtils.GetCurrentDriveSessionInfo(base.SessionState);
			}
			if (!this._cmdletParameters.Contains("Server"))
			{
				if (!flag)
				{
					string str = null;
					aDSessionInfo = new ADSessionInfo(str);
				}
				else
				{
					aDSessionInfo = currentDriveSessionInfo.Copy();
				}
			}
			else
			{
				aDSessionInfo = new ADSessionInfo(this._cmdletParameters["Server"] as string);
			}
			if (!this._cmdletParameters.Contains("Credential"))
			{
				if (flag)
				{
					aDSessionInfo.Credential = currentDriveSessionInfo.Credential;
				}
			}
			else
			{
				aDSessionInfo.Credential = this._cmdletParameters["Credential"] as PSCredential;
			}
			if (!this._cmdletParameters.Contains("AuthType"))
			{
				if (flag)
				{
					aDSessionInfo.AuthType = currentDriveSessionInfo.AuthType;
				}
			}
			else
			{
				ADSessionInfo aDSessionInfo1 = aDSessionInfo;
				if (this.GetAuthType() == ADAuthType.Negotiate)
				{
					authType = AuthType.Negotiate;
				}
				else
				{
					authType = AuthType.Basic;
				}
				aDSessionInfo1.AuthType = authType;
			}
			if (flag)
			{
				aDSessionInfo.Timeout = currentDriveSessionInfo.Timeout;
				aDSessionInfo.Options = currentDriveSessionInfo.Options;
			}
			return aDSessionInfo;
		}

		private void DeleteSessionCache()
		{
			if (this._cmdletCachedSession != null)
			{
				this._cmdletCachedSession.Dispose();
			}
		}

		public virtual void Dispose()
		{
			this.FlushAllOutputBuffers();
			this._cmdletSessionCache.Clear();
			this.DeleteSessionCache();
		}

		protected internal bool DoesServerNameRepresentDomainName(string serverName)
		{
			bool flag = false;
			IntPtr zero = IntPtr.Zero;
			try
			{
				object[] objArray = new object[2];
				objArray[0] = serverName;
				objArray[1] = 0;
				DebugLogger.WriteLine("ADCmdletBase", "calling DsGetDcName for server {0} with flags {1}", objArray);
				int num = UnsafeNativeMethods.DsGetDcName(null, serverName, 0, null, 0, out zero);
				if (num != 0)
				{
					flag = false;
					if (num == 0x3ec)
					{
						object[] objArray1 = new object[1];
						objArray1[0] = 0;
						DebugLogger.LogWarning("ADCmdletBase", "DsGetDCName returned invalid flags error for input: {0}", objArray1);
					}
				}
				else
				{
					flag = true;
				}
			}
			finally
			{
				UnsafeNativeMethods.NetApiBufferFree(zero);
			}
			return flag;
		}

		protected sealed override void EndProcessing()
		{
			DebugLogger.LogInfo("ADCmdletBase", "Entering EndProcessing");
			this._processingRecord = false;
			try
			{
				try
				{
					this._endProcessPipeline.Invoke();
				}
				catch (Exception exception1)
				{
					Exception exception = exception1;
					this.ProcessError(exception);
				}
			}
			finally
			{
				this.InvokeDisposeCallback();
				DebugLogger.LogInfo("ADCmdletBase", "Exiting EndProcessing");
			}
		}

		private void FlushAllOutputBuffers()
		{
			this.FlushWarningBuffer();
			this.FlushErrorBuffer();
		}

		private void FlushErrorBuffer()
		{
			foreach (ErrorRecord errorRecord in this._errorBuffer)
			{
				base.WriteError(errorRecord);
			}
			this._errorBuffer.Clear();
		}

		private void FlushWarningBuffer()
		{
			foreach (string str in this._warningBuffer)
			{
				base.WriteWarning(str);
			}
			this._warningBuffer.Clear();
		}

		internal ADCmdletBase<P>.ExternalDelegates GetADCmdletBaseExternalDelegates()
		{
			return new ADCmdletBase<P>.ExternalDelegates(this);
		}

		private ADAuthType GetAuthType()
		{
			object item = this._cmdletParameters["AuthType"];
			if (item != null)
			{
				return (ADAuthType)item;
			}
			else
			{
				return ADAuthType.Negotiate;
			}
		}

		internal virtual CmdletSessionInfo GetCmdletSessionInfo()
		{
			ADSessionInfo sessionInfo = this.GetSessionInfo();
			this.ConstructSessionCache(this._sessionInfo);
			if (this._cachedCmdletSessionInfo == null || this._cachedCmdletSessionInfo.ADSessionInfo != sessionInfo)
			{
				sessionInfo.ServerType = this.GetConnectedStore();
				this._cachedCmdletSessionInfo = new CmdletSessionInfo(sessionInfo, this.GetRootDSE(), this.GetDefaultQueryPath(), this.GetDefaultPartitionPath(), this.GetDefaultCreationPath(), sessionInfo.ServerType, this._cmdletSessionCache, this, this, this._cmdletParameters);
			}
			return this._cachedCmdletSessionInfo;
		}

		internal virtual ADServerType GetConnectedStore()
		{
			if (!this._connectedStore.HasValue)
			{
				this._connectedStore = new ADServerType?(Utils.ADServerTypeFromRootDSE(this.GetRootDSE()));
			}
			return this._connectedStore.Value;
		}

		protected internal virtual string GetDefaultCreationPath()
		{
			if (!this._cmdletParameters.Contains("Server") && this._pipelineSessionInfo == null && ProviderUtils.IsCurrentDriveAD(base.SessionState))
			{
				string currentDriveLocation = ProviderUtils.GetCurrentDriveLocation(base.SessionState, this.GetSessionInfo());
				if (!string.IsNullOrEmpty(currentDriveLocation))
				{
					return currentDriveLocation;
				}
			}
			return this.GetDefaultCreationPathBase();
		}

		protected internal virtual string GetDefaultCreationPathBase()
		{
			return this.GetDefaultQueryPathBase();
		}

		protected internal virtual string GetDefaultPartitionPath()
		{
			string item;
			string identifyingString = null;
			if (this._cmdletParameters.Contains("Partition"))
			{
				item = this._cmdletParameters["Partition"] as string;
				if (!string.IsNullOrEmpty(item))
				{
					ADForestPartitionInfo.ValidatePartitionDN(this.GetRootDSE(), item);
					return item;
				}
			}
			if (!this._cmdletParameters.Contains("Identity"))
			{
				if (this._cmdletParameters.Contains("Path"))
				{
					identifyingString = (string)this._cmdletParameters["Path"];
				}
			}
			else
			{
				ADObject aDObject = this._cmdletParameters["Identity"] as ADObject;
				if (aDObject != null)
				{
					identifyingString = aDObject.IdentifyingString;
				}
			}
			if (identifyingString != null)
			{
				ADRootDSE rootDSE = this.GetRootDSE();
				item = ADForestPartitionInfo.ExtractPartitionInfo(this.GetRootDSE(), identifyingString, false);
				if (item == null && rootDSE.SessionInfo.ConnectedToGC)
				{
					item = ADForestPartitionInfo.ExtractPartitionInfo(this.GetRootDSE(), identifyingString, true);
				}
				if (item != null)
				{
					return item;
				}
			}
			if (!this._cmdletParameters.Contains("Server") && this._pipelineSessionInfo == null && ProviderUtils.IsCurrentDriveAD(base.SessionState))
			{
				string currentPartitionPath = ProviderUtils.GetCurrentPartitionPath(base.SessionState);
				if (!string.IsNullOrEmpty(currentPartitionPath))
				{
					return currentPartitionPath;
				}
			}
			return this.GetDefaultPartitionPathBase();
		}

		protected internal virtual string GetDefaultPartitionPathBase()
		{
			return this.GetRootDSE().DefaultNamingContext;
		}

		protected internal virtual string GetDefaultQueryPath()
		{
			if (!this._cmdletParameters.Contains("Server") && this._pipelineSessionInfo == null && ProviderUtils.IsCurrentDriveAD(base.SessionState))
			{
				string currentDriveLocation = ProviderUtils.GetCurrentDriveLocation(base.SessionState, this.GetSessionInfo());
				if (this.GetSessionInfo().ConnectedToGC || !string.IsNullOrEmpty(currentDriveLocation))
				{
					return currentDriveLocation;
				}
			}
			return this.GetDefaultQueryPathBase();
		}

		protected internal virtual string GetDefaultQueryPathBase()
		{
			if (!this.GetSessionInfo().ConnectedToGC)
			{
				return this.GetRootDSE().DefaultNamingContext;
			}
			else
			{
				return string.Empty;
			}
		}

		protected internal virtual ADRootDSE GetRootDSE()
		{
			if (this._cachedRootDSE == null)
			{
				ADSessionInfo sessionInfo = this.GetSessionInfo();
				using (ADObjectSearcher aDObjectSearcher = new ADObjectSearcher(sessionInfo))
				{
					this._cachedRootDSE = aDObjectSearcher.GetRootDSE();
					this._cachedRootDSE.SessionInfo = sessionInfo;
				}
				this._connectedStore = new ADServerType?(Utils.ADServerTypeFromRootDSE(this._cachedRootDSE));
			}
			return this._cachedRootDSE;
		}

		internal virtual ADSessionInfo GetSessionInfo()
		{
			if (this._sessionInfo != null || !this.SessionSpecified() && this._pipelineSessionInfo != null)
			{
				if (this.SessionSpecified() || this._pipelineSessionInfo == null)
				{
					this.ApplyCmdletSessionOptions(this._sessionInfo);
					this.ConstructSessionCache(this._sessionInfo);
					return this._sessionInfo;
				}
				else
				{
					this.ApplyCmdletSessionOptions(this._pipelineSessionInfo);
					this.ConstructSessionCache(this._pipelineSessionInfo);
					return this._pipelineSessionInfo;
				}
			}
			else
			{
				this._sessionInfo = this.CreateSessionFromParameters();
				this.ApplyCmdletSessionOptions(this._sessionInfo);
				this.ConstructSessionCache(this._sessionInfo);
				return this._sessionInfo;
			}
		}

		private void InvokeDisposeCallback()
		{
			if (this._cmdletDisposeEvent != null)
			{
				this._cmdletDisposeEvent();
				this._cmdletDisposeEvent = null;
			}
		}

		internal virtual void ProcessError(Exception e)
		{
			if (e as RuntimeException == null)
			{
				IADCustomExceptionFiltering aDCustomExceptionFiltering = this;
				bool flag = true;
				this._recordExceptionHandler.ProcessingRecord = this._processingRecord;
				if (aDCustomExceptionFiltering == null || !aDCustomExceptionFiltering.ExceptionFilter.FilterException(e, ref flag) || flag)
				{
					base.ThrowTerminatingError(this.ConstructErrorRecord(e));
					return;
				}
				else
				{
					this.WriteErrorBuffered(this.ConstructErrorRecord(e));
					return;
				}
			}
			else
			{
				throw e;
			}
		}

		protected sealed override void ProcessRecord()
		{
			DebugLogger.LogInfo("ADCmdletBase", "Entering ProcessRecord");
			bool flag = false;
			this._processingRecord = true;
			try
			{
				try
				{
					this._processRecordPipeline.Invoke();
				}
				catch (Exception exception1)
				{
					Exception exception = exception1;
					flag = true;
					this.ProcessError(exception);
					flag = false;
				}
			}
			finally
			{
				if (flag)
				{
					this.InvokeDisposeCallback();
				}
				DebugLogger.LogInfo("ADCmdletBase", "Exiting ProcessRecord");
			}
		}

		protected void RegisterDisposeCallback(ADCmdletBase<P>.CmdletDispose disposeDelegate)
		{
			if (this._cmdletDisposeEvent != null)
			{
				ADCmdletBase<P> aDCmdletBase = this;
				aDCmdletBase._cmdletDisposeEvent += disposeDelegate;
				return;
			}
			else
			{
				this._cmdletDisposeEvent = disposeDelegate;
				return;
			}
		}

		protected internal bool SessionSpecified()
		{
			if (this._cmdletParameters.Contains("Server"))
			{
				return true;
			}
			else
			{
				return this._cmdletParameters.Contains("Credential");
			}
		}

		internal void SetCmdletSessionOptions(ADSessionOptions sessionOptions)
		{
			if (this._cmdletSessionOptions != null)
			{
				sessionOptions.CopyValuesTo(this._cmdletSessionOptions);
			}
			else
			{
				this._cmdletSessionOptions = sessionOptions;
			}
			this._cachedRootDSE = null;
		}

		private void SetDefaultSessionInfo(ADSessionInfo session)
		{
			this._sessionInfo = session;
			this._cachedRootDSE = null;
		}

		internal virtual void SetPipelinedSessionInfo(ADSessionInfo session)
		{
			this._pipelineSessionInfo = session;
			this._cachedRootDSE = null;
		}

		protected bool ShouldProcessOverride(string target)
		{
			this.FlushAllOutputBuffers();
			return base.ShouldProcess(target);
		}

		protected bool ShouldProcessOverride(string target, string action)
		{
			this.FlushAllOutputBuffers();
			return base.ShouldProcess(target, action);
		}

		protected bool ShouldProcessOverride(string verboseDescription, string verboseWarning, string caption)
		{
			this.FlushAllOutputBuffers();
			return base.ShouldProcess(verboseDescription, verboseWarning, caption);
		}

		protected bool ShouldProcessOverride(string verboseDescription, string verboseWarning, string caption, out ShouldProcessReason shouldProcessReason)
		{
			this.FlushAllOutputBuffers();
			return base.ShouldProcess(verboseDescription, verboseWarning, caption, out shouldProcessReason);
		}

		object System.Management.Automation.IDynamicParameters.GetDynamicParameters()
		{
			return this._cmdletParameters;
		}

		protected void TargetOperationMasterRole(ADOperationMasterRole fsmoRole)
		{
			bool flag;
			ADSessionInfo sessionInfo = this.GetSessionInfo();
			string server = sessionInfo.Server;
			if (server != null)
			{
				flag = this.DoesServerNameRepresentDomainName(server);
			}
			else
			{
				flag = true;
			}
			if (flag)
			{
				string value = null;
				if (fsmoRole == ADOperationMasterRole.PDCEmulator || fsmoRole == ADOperationMasterRole.RIDMaster || fsmoRole == ADOperationMasterRole.InfrastructureMaster)
				{
					using (ADTopologyManagement aDTopologyManagement = new ADTopologyManagement(sessionInfo))
					{
						ADObject domain = aDTopologyManagement.GetDomain();
						ADOperationMasterRole aDOperationMasterRole = fsmoRole;
						switch (aDOperationMasterRole)
						{
							case ADOperationMasterRole.PDCEmulator:
							{
								value = domain["PDCEmulator"].Value as string;
								break;
							}
							case ADOperationMasterRole.RIDMaster:
							{
								value = domain["RIDMaster"].Value as string;
								break;
							}
							case ADOperationMasterRole.InfrastructureMaster:
							{
								value = domain["InfrastructureMaster"].Value as string;
								break;
							}
						}
					}
					if (string.IsNullOrEmpty(value))
					{
						object[] objArray = new object[2];
						objArray[0] = fsmoRole;
						objArray[1] = server;
						throw new ADException(string.Format(CultureInfo.CurrentCulture, StringResources.FSMORoleNotFoundInDomain, objArray));
					}
				}
				else
				{
					if (fsmoRole == ADOperationMasterRole.SchemaMaster || fsmoRole == ADOperationMasterRole.DomainNamingMaster)
					{
						using (ADTopologyManagement aDTopologyManagement1 = new ADTopologyManagement(sessionInfo))
						{
							ADEntity forest = aDTopologyManagement1.GetForest();
							ADOperationMasterRole aDOperationMasterRole1 = fsmoRole;
							switch (aDOperationMasterRole1)
							{
								case ADOperationMasterRole.SchemaMaster:
								{
									value = forest["SchemaMaster"].Value as string;
									break;
								}
								case ADOperationMasterRole.DomainNamingMaster:
								{
									value = forest["DomainNamingMaster"].Value as string;
									break;
								}
							}
						}
						if (string.IsNullOrEmpty(value))
						{
							object[] objArray1 = new object[2];
							objArray1[0] = fsmoRole;
							objArray1[1] = server;
							throw new ADException(string.Format(CultureInfo.CurrentCulture, StringResources.FSMORoleNotFoundInForest, objArray1));
						}
					}
				}
				ADSessionInfo aDSessionInfo = sessionInfo.Copy();
				aDSessionInfo.Server = value;
				if (!this.SessionSpecified())
				{
					this.SetPipelinedSessionInfo(aDSessionInfo);
				}
				else
				{
					this.SetDefaultSessionInfo(aDSessionInfo);
					return;
				}
			}
		}

		public void WriteErrorBuffered(ErrorRecord error)
		{
			this._errorBuffer.Add(error);
		}

		public void WriteErrorImmediate(ErrorRecord error)
		{
			base.WriteError(error);
		}

		public void WriteWarningBuffered(string text)
		{
			this._warningBuffer.Add(text);
		}

		public void WriteWarningImmediate(string text)
		{
			base.WriteWarning(text);
		}

		protected delegate void CmdletDispose();

		internal class ExternalDelegates
		{
			private ADCmdletBase<P> _cmdletInstance;

			public ExternalDelegates(ADCmdletBase<P> cmdletInstance)
			{
				this._cmdletInstance = cmdletInstance;
			}

			public bool AddSessionOptionGlobalCatalogRequiredCSRoutine()
			{
				ADSessionOptions aDSessionOption = new ADSessionOptions();
				aDSessionOption.LocatorFlag = new ADLocatorFlags?(ADLocatorFlags.GCRequired);
				this._cmdletInstance.SetCmdletSessionOptions(aDSessionOption);
				return true;
			}

			public bool AddSessionOptionWindows2008AndAboveRequiredCSRoutine()
			{
				ADSessionOptions aDSessionOption = new ADSessionOptions();
				aDSessionOption.LocatorFlag = new ADLocatorFlags?(ADLocatorFlags.DirectoryServices6Required);
				this._cmdletInstance.SetCmdletSessionOptions(aDSessionOption);
				return true;
			}

			public bool AddSessionOptionWindows2012AndAboveRequiredCSRoutine()
			{
				ADSessionOptions aDSessionOption = new ADSessionOptions();
				aDSessionOption.LocatorFlag = new ADLocatorFlags?(ADLocatorFlags.DirectoryServices8Required);
				this._cmdletInstance.SetCmdletSessionOptions(aDSessionOption);
				return true;
			}

			/* ExternalDelegates */

			public bool AddSessionOptionWritableDCRequiredCSRoutine()
			{
				ADSessionOptions aDSessionOption = new ADSessionOptions();
				aDSessionOption.LocatorFlag = new ADLocatorFlags?(ADLocatorFlags.WriteableRequired);
				this._cmdletInstance.SetCmdletSessionOptions(aDSessionOption);
				return true;
			}

			public bool TargetDomainNamingMasterCSRoutine()
			{
				this._cmdletInstance.TargetOperationMasterRole(ADOperationMasterRole.DomainNamingMaster);
				return true;
			}

			public bool TargetPDCEmulatorCSRoutine()
			{
				this._cmdletInstance.TargetOperationMasterRole(ADOperationMasterRole.PDCEmulator);
				return true;
			}

			public bool TargetSchemaMasterCSRoutine()
			{
				this._cmdletInstance.TargetOperationMasterRole(ADOperationMasterRole.SchemaMaster);
				return true;
			}
		}
	}
}