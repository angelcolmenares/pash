using Microsoft.Management.Infrastructure;
using Microsoft.PowerShell.Cmdletization;
using Microsoft.PowerShell.Commands.Management;
using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Management.Automation;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Microsoft.PowerShell.Cmdletization.Cim
{
	public sealed class CimCmdletAdapter : SessionBasedCmdletAdapter<CimInstance, CimSession>, IDynamicParameters
	{
		internal const string CreateInstance_MethodName = "cim:CreateInstance";

		internal const string ModifyInstance_MethodName = "cim:ModifyInstance";

		internal const string DeleteInstance_MethodName = "cim:DeleteInstance";

		private const string CimNamespaceParameter = "CimNamespace";

		private bool _throttleLimitIsSetExplicitly;

		private CimCmdletInvocationContext _cmdletInvocationContext;

		private CimCmdletDefinitionContext _cmdletDefinitionContext;

		private static long _jobNumber;

		private readonly static ConditionalWeakTable<CimInstance, CimSession> cimInstanceToSessionOfOrigin;

		private RuntimeDefinedParameterDictionary _dynamicParameters;

		[Alias(new string[] { "Session" })]
		[Parameter]
		[ValidateNotNullOrEmpty]
		public CimSession[] CimSession
		{
			get
			{
				return base.Session;
			}
			set
			{
				base.Session = value;
			}
		}

		internal CimCmdletDefinitionContext CmdletDefinitionContext
		{
			get
			{
				if (this._cmdletDefinitionContext == null)
				{
					this._cmdletDefinitionContext = new CimCmdletDefinitionContext(base.ClassName, base.ClassVersion, base.ModuleVersion, base.Cmdlet.CommandInfo.CommandMetadata.SupportsShouldProcess, base.PrivateData);
				}
				return this._cmdletDefinitionContext;
			}
		}

		internal CimCmdletInvocationContext CmdletInvocationContext
		{
			get
			{
				if (this._cmdletInvocationContext == null)
				{
					this._cmdletInvocationContext = new CimCmdletInvocationContext(this.CmdletDefinitionContext, base.Cmdlet, this.GetDynamicNamespace());
				}
				return this._cmdletInvocationContext;
			}
		}

		internal InvocationInfo CmdletInvocationInfo
		{
			get
			{
				return this.CmdletInvocationContext.CmdletInvocationInfo;
			}
		}

		protected override CimSession DefaultSession
		{
			get
			{
				return this.CmdletInvocationContext.GetDefaultCimSession();
			}
		}

		[Parameter]
		public override int ThrottleLimit
		{
			get
			{
				if (!this._throttleLimitIsSetExplicitly)
				{
					return this.CmdletDefinitionContext.DefaultThrottleLimit;
				}
				else
				{
					return base.ThrottleLimit;
				}
			}
			set
			{
				base.ThrottleLimit = value;
				this._throttleLimitIsSetExplicitly = true;
			}
		}

		static CimCmdletAdapter()
		{
			CimCmdletAdapter.cimInstanceToSessionOfOrigin = new ConditionalWeakTable<CimInstance, CimSession>();
		}

		public CimCmdletAdapter()
		{
		}

		internal static void AssociateSessionOfOriginWithInstance(CimInstance cimInstance, CimSession sessionOfOrigin)
		{
			CimCmdletAdapter.cimInstanceToSessionOfOrigin.GetValue(cimInstance, (CimInstance argument0) => sessionOfOrigin);
		}

		internal override StartableJob CreateInstanceMethodInvocationJob(CimSession session, CimInstance objectInstance, MethodInvocationInfo methodInvocationInfo, bool passThru)
		{
			StartableJob instanceMethodInvocationJob;
			TerminatingErrorTracker tracker = TerminatingErrorTracker.GetTracker(this.CmdletInvocationInfo, false);
			if (!tracker.IsSessionTerminated(session))
			{
				if (this.IsSupportedSession(session, tracker))
				{
					CimJobContext cimJobContext = this.CreateJobContext(session, objectInstance);
					if (!methodInvocationInfo.MethodName.Equals("cim:DeleteInstance", StringComparison.OrdinalIgnoreCase))
					{
						if (!methodInvocationInfo.MethodName.Equals("cim:ModifyInstance", StringComparison.OrdinalIgnoreCase))
						{
							instanceMethodInvocationJob = new InstanceMethodInvocationJob(cimJobContext, passThru, objectInstance, methodInvocationInfo);
						}
						else
						{
							instanceMethodInvocationJob = new ModifyInstanceJob(cimJobContext, passThru, objectInstance, methodInvocationInfo);
						}
					}
					else
					{
						instanceMethodInvocationJob = new DeleteInstanceJob(cimJobContext, passThru, objectInstance, methodInvocationInfo);
					}
					return instanceMethodInvocationJob;
				}
				else
				{
					return null;
				}
			}
			else
			{
				return null;
			}
		}

		private CimJobContext CreateJobContext(CimSession session, object targetObject)
		{
			return new CimJobContext(this.CmdletInvocationContext, session, targetObject);
		}

		internal override StartableJob CreateQueryJob(CimSession session, QueryBuilder baseQuery)
		{
			CimQuery cimQuery = baseQuery as CimQuery;
			if (cimQuery != null)
			{
				TerminatingErrorTracker tracker = TerminatingErrorTracker.GetTracker(this.CmdletInvocationInfo, false);
				if (!tracker.IsSessionTerminated(session))
				{
					if (this.IsSupportedSession(session, tracker))
					{
						CimJobContext cimJobContext = this.CreateJobContext(session, null);
						StartableJob queryJob = cimQuery.GetQueryJob(cimJobContext);
						return queryJob;
					}
					else
					{
						return null;
					}
				}
				else
				{
					return null;
				}
			}
			else
			{
				throw new ArgumentNullException("baseQuery");
			}
		}

		internal override StartableJob CreateStaticMethodInvocationJob(CimSession session, MethodInvocationInfo methodInvocationInfo)
		{
			StartableJob staticMethodInvocationJob;
			TerminatingErrorTracker tracker = TerminatingErrorTracker.GetTracker(this.CmdletInvocationInfo, true);
			if (!tracker.IsSessionTerminated(session))
			{
				if (this.IsSupportedSession(session, tracker))
				{
					CimJobContext cimJobContext = this.CreateJobContext(session, null);
					if (!methodInvocationInfo.MethodName.Equals("cim:CreateInstance", StringComparison.OrdinalIgnoreCase))
					{
						staticMethodInvocationJob = new StaticMethodInvocationJob(cimJobContext, methodInvocationInfo);
					}
					else
					{
						staticMethodInvocationJob = new CreateInstanceJob(cimJobContext, methodInvocationInfo);
					}
					return staticMethodInvocationJob;
				}
				else
				{
					return null;
				}
			}
			else
			{
				return null;
			}
		}

		protected override string GenerateParentJobName()
		{
			long num = Interlocked.Increment(ref CimCmdletAdapter._jobNumber);
			return string.Concat("CimJob", num.ToString(CultureInfo.InvariantCulture));
		}

		private string GetDynamicNamespace()
		{
			RuntimeDefinedParameter runtimeDefinedParameter = null;
			if (this._dynamicParameters != null)
			{
				if (this._dynamicParameters.TryGetValue("CimNamespace", out runtimeDefinedParameter))
				{
					return runtimeDefinedParameter.Value as string;
				}
				else
				{
					return null;
				}
			}
			else
			{
				return null;
			}
		}

		public override QueryBuilder GetQueryBuilder()
		{
			return new CimQuery();
		}

		internal static CimSession GetSessionOfOriginFromCimInstance(CimInstance instance)
		{
			CimSession cimSession = null;
			if (instance != null)
			{
				CimCmdletAdapter.cimInstanceToSessionOfOrigin.TryGetValue(instance, out cimSession);
			}
			return cimSession;
		}

		internal override CimSession GetSessionOfOriginFromInstance(CimInstance instance)
		{
			return CimCmdletAdapter.GetSessionOfOriginFromCimInstance(instance);
		}

		private bool IsSupportedSession(CimSession cimSession, TerminatingErrorTracker terminatingErrorTracker)
		{
			bool flag = false;
			string str;
			bool flag1 = this.CmdletInvocationInfo.BoundParameters.ContainsKey("Confirm");
			bool flag2 = this.CmdletInvocationInfo.BoundParameters.ContainsKey("WhatIf");
			if ((flag1 || flag2) && cimSession.ComputerName != null && !cimSession.ComputerName.Equals("localhost", StringComparison.OrdinalIgnoreCase))
			{
				PSPropertyInfo item = PSObject.AsPSObject(cimSession).Properties["Protocol"];
				if (item != null && item.Value != null && item.Value.ToString().Equals("DCOM", StringComparison.OrdinalIgnoreCase))
				{
					terminatingErrorTracker.MarkSessionAsTerminated(cimSession, out flag);
					if (!flag)
					{
						if (!flag1)
						{
							str = "-WhatIf";
						}
						else
						{
							str = "-Confirm";
						}
						object[] computerName = new object[2];
						computerName[0] = cimSession.ComputerName;
						computerName[1] = str;
						string str1 = string.Format(CultureInfo.InvariantCulture, CmdletizationResources.CimCmdletAdapter_RemoteDcomDoesntSupportExtendedSemantics, computerName);
						Exception notSupportedException = new NotSupportedException(str1);
						ErrorRecord errorRecord = new ErrorRecord(notSupportedException, "NoExtendedSemanticsSupportInRemoteDcomProtocol", ErrorCategory.NotImplemented, cimSession);
						base.Cmdlet.WriteError(errorRecord);
					}
				}
			}
			return true;
		}

		object System.Management.Automation.IDynamicParameters.GetDynamicParameters()
		{
			if (this._dynamicParameters == null)
			{
				this._dynamicParameters = new RuntimeDefinedParameterDictionary();
				if (this.CmdletDefinitionContext.ExposeCimNamespaceParameter)
				{
					Collection<Attribute> attributes = new Collection<Attribute>();
					attributes.Add(new ValidateNotNullOrEmptyAttribute());
					attributes.Add(new ParameterAttribute());
					RuntimeDefinedParameter runtimeDefinedParameter = new RuntimeDefinedParameter("CimNamespace", typeof(string), attributes);
					this._dynamicParameters.Add("CimNamespace", runtimeDefinedParameter);
				}
			}
			return this._dynamicParameters;
		}
	}
}