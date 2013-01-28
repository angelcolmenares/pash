using Microsoft.ActiveDirectory.Management;
using Microsoft.ActiveDirectory.Management.Provider;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Text;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	[Cmdlet("Get", "ADDomainController", HelpUri="http://go.microsoft.com/fwlink/?LinkId=219306", DefaultParameterSetName="Identity")]
	public class GetADDomainController : ADGetCmdletBase<GetADDomainControllerParameterSet, ADDomainControllerFactory<ADDomainController>, ADDomainController>
	{
		private const string _debugCategory = "GetADDomainController";

		public GetADDomainController()
		{
			base.BeginProcessPipeline.InsertAtStart(new CmdletSubroutine(this.GetADDCBeginIdentityCSRoutine));
			base.BeginProcessPipeline.InsertAtStart(new CmdletSubroutine(this.GetADDCBeginFilterCSRoutine));
			base.BeginProcessPipeline.InsertAtStart(new CmdletSubroutine(this.GetADDCBeginDiscoverCSRoutine));
			base.ProcessRecordPipeline.InsertAtStart(new CmdletSubroutine(this.GetADDCProcessCSRoutine));
		}

		internal string[] ConstructPropertyListFromSet(AttributeSet set)
		{
			AttributeSetRequest attributeSetRequest = this._factory.ConstructAttributeSetRequest(set);
			List<string> strs = new List<string>(attributeSetRequest.ExtendedAttributes);
			return strs.ToArray();
		}

		private bool GetADDCBeginDiscoverCSRoutine()
		{
			bool flag;
			if (base.ParameterSetName == "DiscoverByService")
			{
				ADDiscoverDomainControllerOptions aDDiscoverDomainControllerOption = ADDiscoverDomainControllerOptions.None;
				ADDiscoverableService[] service = this._cmdletParameters.Service;
				ADMinimumDirectoryServiceVersion? item = (ADMinimumDirectoryServiceVersion?)(this._cmdletParameters["MinimumDirectoryServiceVersion"] as ADMinimumDirectoryServiceVersion?);
				string str = this._cmdletParameters["SiteName"] as string;
				string item1 = this._cmdletParameters["DomainName"] as string;
				if (this._cmdletParameters.GetSwitchParameterBooleanValue("NextClosestSite"))
				{
					aDDiscoverDomainControllerOption = aDDiscoverDomainControllerOption | ADDiscoverDomainControllerOptions.TryNextClosestSite;
				}
				if (this._cmdletParameters.GetSwitchParameterBooleanValue("ForceDiscover"))
				{
					aDDiscoverDomainControllerOption = aDDiscoverDomainControllerOption | ADDiscoverDomainControllerOptions.ForceDiscover;
				}
				if (this._cmdletParameters.GetSwitchParameterBooleanValue("AvoidSelf"))
				{
					aDDiscoverDomainControllerOption = aDDiscoverDomainControllerOption | ADDiscoverDomainControllerOptions.AvoidSelf;
				}
				if (this._cmdletParameters.GetSwitchParameterBooleanValue("Writable"))
				{
					aDDiscoverDomainControllerOption = aDDiscoverDomainControllerOption | ADDiscoverDomainControllerOptions.Writable;
				}
				try
				{
					ADEntity aDEntity = DomainControllerUtil.DiscoverDomainController(str, item1, service, aDDiscoverDomainControllerOption | ADDiscoverDomainControllerOptions.ReturnDnsName, item);
					try
					{
						ADDiscoverDomainControllerOptions aDDiscoverDomainControllerOption1 = aDDiscoverDomainControllerOption;
						aDDiscoverDomainControllerOption1 = aDDiscoverDomainControllerOption1 & (ADDiscoverDomainControllerOptions.AvoidSelf | ADDiscoverDomainControllerOptions.TryNextClosestSite | ADDiscoverDomainControllerOptions.Writable | ADDiscoverDomainControllerOptions.ReturnDnsName | ADDiscoverDomainControllerOptions.ReturnFlatName);
						ADEntity aDEntity1 = DomainControllerUtil.DiscoverDomainController(str, item1, service, aDDiscoverDomainControllerOption1 | ADDiscoverDomainControllerOptions.ReturnFlatName, item);
						if (aDEntity.InternalProperties.Contains("DiscoveryInternalPropertyDCAddress") && aDEntity1.InternalProperties.Contains("DiscoveryInternalPropertyDCAddress") && string.Equals((string)aDEntity.InternalProperties["DiscoveryInternalPropertyDCAddress"].Value, (string)aDEntity1.InternalProperties["DiscoveryInternalPropertyDCAddress"].Value))
						{
							aDEntity.Add("Name", aDEntity1["Name"].Value);
						}
					}
					catch (ADException aDException1)
					{
						ADException aDException = aDException1;
						DebugLogger.LogError("GetADDomainController", aDException.ToString());
					}
					CmdletSessionInfo cmdletSessionInfo = new CmdletSessionInfo();
					this._factory.SetCmdletSessionInfo(cmdletSessionInfo);
					ADDomainController aDDomainController = this._factory.Construct(aDEntity, new AttributeSetRequest(true));
					base.WriteObject(aDDomainController);
					return false;
				}
				catch (ADException aDException3)
				{
					ADException aDException2 = aDException3;
					int errorCode = aDException2.ErrorCode;
					base.WriteError(new ErrorRecord(aDException2, string.Concat("GetADDomainController:BeginProcessingOverride:DiscoverDC:", errorCode.ToString()), ErrorCategory.ObjectNotFound, null));
					flag = false;
				}
				return flag;
			}
			else
			{
				return true;
			}
		}

		private bool GetADDCBeginFilterCSRoutine()
		{
			if (base.ParameterSetName == "Filter")
			{
				StringBuilder stringBuilder = null;
				this._cmdletParameters["Properties"] = this.ConstructPropertyListFromSet(AttributeSet.Extended);
				this._factory.SetCmdletSessionInfo(this.GetCmdletSessionInfo());
				this.ValidateParameters();
				base.BuildPropertySet();
				string str = this._cmdletParameters["Filter"].ToString();
				if (str != "objectClass -like \"*\"")
				{
					var f = this._factory;
					ConvertSearchFilterDelegate convertSearchFilterDelegate = new ConvertSearchFilterDelegate(f.BuildSearchFilter);
					VariableExpressionConverter variableExpressionConverter = new VariableExpressionConverter(new EvaluateVariableDelegate(this.EvaluateFilterVariable));
					QueryParser queryParser = new QueryParser(str, variableExpressionConverter, convertSearchFilterDelegate);
					str = ADOPathUtil.ChangeNodeToWhereFilterSyntax(queryParser.FilterExpressionTree);
					DebugLogger.LogInfo("GetADDomainController", string.Format("Filter: Converted where filter: {0}", str));
					stringBuilder = new StringBuilder("Where-Object -inputObject $args[0] -filterScript { ");
					stringBuilder.Append(str);
					stringBuilder.Append(" } ");
				}
				else
				{
					DebugLogger.LogInfo("GetADDomainController", string.Format("Filter: Found MatchAnyObject filter: {0}", str));
				}
				IEnumerable<ADDomainController> allDomainControllers = this._factory.GetAllDomainControllers(this._propertiesRequested);
				if (allDomainControllers != null)
				{
					if (stringBuilder != null)
					{
						foreach (ADDomainController allDomainController in allDomainControllers)
						{
							try
							{
								object[] objArray = new object[1];
								objArray[0] = allDomainController;
								base.InvokeCommand.InvokeScript(stringBuilder.ToString(), false, PipelineResultTypes.Output, null, objArray);
							}
							catch (RuntimeException runtimeException1)
							{
								RuntimeException runtimeException = runtimeException1;
								object[] message = new object[1];
								message[0] = runtimeException.Message;
								string str1 = string.Format(CultureInfo.CurrentCulture, "Filtering failed:  {0}", message);
								DebugLogger.LogError("GetADDomainController", str1);
								base.WriteError(new ErrorRecord(runtimeException, "0", ErrorCategory.ReadError, str));
							}
						}
					}
					else
					{
						foreach (ADDomainController aDDomainController in allDomainControllers)
						{
							base.WriteObject(aDDomainController);
						}
					}
					return false;
				}
				else
				{
					return false;
				}
			}
			else
			{
				return true;
			}
		}

		private bool GetADDCBeginIdentityCSRoutine()
		{
			this._cmdletParameters["Properties"] = this.ConstructPropertyListFromSet(AttributeSet.Extended);
			this.ValidateParameters();
			return true;
		}

		private bool GetADDCProcessCSRoutine()
		{
			if (!(base.ParameterSetName != "DiscoverByService") || !(base.ParameterSetName != "Filter"))
			{
				return false;
			}
			else
			{
				return true;
			}
		}

		protected internal override string GetDefaultPartitionPath()
		{
			return string.Empty;
		}

		protected internal override void ValidateParameters()
		{
			CmdletSessionInfo cmdletSessionInfo = this.GetCmdletSessionInfo();
			if (base.ParameterSetName == "Identity" && this._cmdletParameters["Identity"] == null)
			{
				if (!ProviderUtils.IsCurrentDriveAD(base.SessionState))
				{
					this._cmdletParameters["Identity"] = new ADDomainController(cmdletSessionInfo.ADRootDSE.DNSHostName);
				}
				else
				{
					this._cmdletParameters["Identity"] = new ADDomainController(((ADDriveInfo)base.SessionState.Drive.Current).Session.RootDSE.DNSHostName);
					return;
				}
			}
		}
	}
}