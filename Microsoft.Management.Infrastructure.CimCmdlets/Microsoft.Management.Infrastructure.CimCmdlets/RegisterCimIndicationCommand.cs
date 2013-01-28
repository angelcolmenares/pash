using Microsoft.Management.Infrastructure;
using Microsoft.PowerShell.Commands;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Management.Automation;

namespace Microsoft.Management.Infrastructure.CimCmdlets
{
	[Cmdlet("Register", "CimIndicationEvent", DefaultParameterSetName="ClassNameComputerSet", HelpUri="http://go.microsoft.com/fwlink/?LinkId=227960")]
	public class RegisterCimIndicationCommand : ObjectEventRegistrationBase
	{
		internal const string nameClassName = "ClassName";

		internal const string nameQuery = "Query";

		internal const string nameQueryDialect = "QueryDialect";

		internal const string nameCimSession = "CimSession";

		internal const string nameComputerName = "ComputerName";

		private string nameSpace;

		private string className;

		private string query;

		private string queryDialect;

		private uint operationTimeout;

		private CimSession cimSession;

		private string computername;

		private ParameterBinder parameterBinder;

		private static Dictionary<string, HashSet<ParameterDefinitionEntry>> parameters;

		private static Dictionary<string, ParameterSetEntry> parameterSets;

		[Parameter(Mandatory=true, ParameterSetName="QueryExpressionSessionSet")]
		[Parameter(Mandatory=true, ParameterSetName="ClassNameSessionSet")]
		public CimSession CimSession
		{
			get
			{
				return this.cimSession;
			}
			set
			{
				this.cimSession = value;
				this.SetParameter(value, "CimSession");
			}
		}

		[Parameter(Mandatory=true, Position=0, ParameterSetName="ClassNameComputerSet")]
		[Parameter(Mandatory=true, Position=0, ParameterSetName="ClassNameSessionSet")]
		public string ClassName
		{
			get
			{
				return this.className;
			}
			set
			{
				this.className = value;
				this.SetParameter(value, "ClassName");
			}
		}

		[Alias(new string[] { "CN", "ServerName" })]
		[Parameter(ParameterSetName="ClassNameComputerSet")]
		[Parameter(ParameterSetName="QueryExpressionComputerSet")]
		public string ComputerName
		{
			get
			{
				return this.computername;
			}
			set
			{
				this.computername = value;
				this.SetParameter(value, "ComputerName");
			}
		}

		[Parameter]
		public string Namespace
		{
			get
			{
				return this.nameSpace;
			}
			set
			{
				this.nameSpace = value;
			}
		}

		[Alias(new string[] { "OT" })]
		[Parameter]
		public uint OperationTimeoutSec
		{
			get
			{
				return this.operationTimeout;
			}
			set
			{
				this.operationTimeout = value;
			}
		}

		[Parameter(Mandatory=true, Position=0, ParameterSetName="QueryExpressionSessionSet")]
		[Parameter(Mandatory=true, Position=0, ParameterSetName="QueryExpressionComputerSet")]
		public string Query
		{
			get
			{
				return this.query;
			}
			set
			{
				this.query = value;
				this.SetParameter(value, "Query");
			}
		}

		[Parameter(ParameterSetName="QueryExpressionComputerSet")]
		[Parameter(ParameterSetName="QueryExpressionSessionSet")]
		public string QueryDialect
		{
			get
			{
				return this.queryDialect;
			}
			set
			{
				this.queryDialect = value;
				this.SetParameter(value, "QueryDialect");
			}
		}

		static RegisterCimIndicationCommand()
		{
			Dictionary<string, HashSet<ParameterDefinitionEntry>> strs = new Dictionary<string, HashSet<ParameterDefinitionEntry>>();
			HashSet<ParameterDefinitionEntry> parameterDefinitionEntries = new HashSet<ParameterDefinitionEntry>();
			parameterDefinitionEntries.Add(new ParameterDefinitionEntry("ClassNameSessionSet", true));
			parameterDefinitionEntries.Add(new ParameterDefinitionEntry("ClassNameComputerSet", true));
			strs.Add("ClassName", parameterDefinitionEntries);
			HashSet<ParameterDefinitionEntry> parameterDefinitionEntries1 = new HashSet<ParameterDefinitionEntry>();
			parameterDefinitionEntries1.Add(new ParameterDefinitionEntry("QueryExpressionSessionSet", true));
			parameterDefinitionEntries1.Add(new ParameterDefinitionEntry("QueryExpressionComputerSet", true));
			strs.Add("Query", parameterDefinitionEntries1);
			HashSet<ParameterDefinitionEntry> parameterDefinitionEntries2 = new HashSet<ParameterDefinitionEntry>();
			parameterDefinitionEntries2.Add(new ParameterDefinitionEntry("QueryExpressionSessionSet", false));
			parameterDefinitionEntries2.Add(new ParameterDefinitionEntry("QueryExpressionComputerSet", false));
			strs.Add("QueryDialect", parameterDefinitionEntries2);
			HashSet<ParameterDefinitionEntry> parameterDefinitionEntries3 = new HashSet<ParameterDefinitionEntry>();
			parameterDefinitionEntries3.Add(new ParameterDefinitionEntry("QueryExpressionSessionSet", true));
			parameterDefinitionEntries3.Add(new ParameterDefinitionEntry("ClassNameSessionSet", true));
			strs.Add("CimSession", parameterDefinitionEntries3);
			HashSet<ParameterDefinitionEntry> parameterDefinitionEntries4 = new HashSet<ParameterDefinitionEntry>();
			parameterDefinitionEntries4.Add(new ParameterDefinitionEntry("QueryExpressionComputerSet", false));
			parameterDefinitionEntries4.Add(new ParameterDefinitionEntry("ClassNameComputerSet", false));
			strs.Add("ComputerName", parameterDefinitionEntries4);
			RegisterCimIndicationCommand.parameters = strs;
			Dictionary<string, ParameterSetEntry> strs1 = new Dictionary<string, ParameterSetEntry>();
			strs1.Add("QueryExpressionSessionSet", new ParameterSetEntry(2));
			strs1.Add("QueryExpressionComputerSet", new ParameterSetEntry(1));
			strs1.Add("ClassNameSessionSet", new ParameterSetEntry(2));
			strs1.Add("ClassNameComputerSet", new ParameterSetEntry(1, true));
			RegisterCimIndicationCommand.parameterSets = strs1;
		}

		public RegisterCimIndicationCommand()
		{
			this.parameterBinder = new ParameterBinder(RegisterCimIndicationCommand.parameters, RegisterCimIndicationCommand.parameterSets);
		}

		private void CheckArgument()
		{
			this.className = ValidationHelper.ValidateArgumentIsValidName("ClassName", this.className);
		}

		protected override void EndProcessing()
		{
			DebugHelper.WriteLogEx();
			base.EndProcessing();
			PSEventSubscriber newSubscriber = base.NewSubscriber;
			if (newSubscriber != null)
			{
				DebugHelper.WriteLog("RegisterCimIndicationCommand::EndProcessing subscribe to Unsubscribed event", 4);
				newSubscriber.Unsubscribed += new PSEventUnsubscribedEventHandler(RegisterCimIndicationCommand.newSubscriber_Unsubscribed);
			}
		}

		protected override object GetSourceObject()
		{
			CimIndicationWatcher cimIndicationWatcher = null;
			string parameterSet = null;
			try
			{
				parameterSet = this.parameterBinder.GetParameterSet();
			}
			finally
			{
				this.parameterBinder.reset();
			}
			string empty = string.Empty;
			string str = parameterSet;
			string str1 = str;
			if (str != null)
			{
				if (str1 == "QueryExpressionSessionSet" || str1 == "QueryExpressionComputerSet")
				{
					empty = this.Query;
				}
				else
				{
					if (str1 == "ClassNameSessionSet" || str1 == "ClassNameComputerSet")
					{
						this.CheckArgument();
						object[] className = new object[1];
						className[0] = this.ClassName;
						empty = string.Format(CultureInfo.CurrentCulture, "Select * from {0}", className);
					}
				}
			}
			string str2 = parameterSet;
			string str3 = str2;
			if (str2 != null)
			{
				if (str3 == "QueryExpressionSessionSet" || str3 == "ClassNameSessionSet")
				{
					cimIndicationWatcher = new CimIndicationWatcher(this.CimSession, this.Namespace, this.QueryDialect, empty, this.OperationTimeoutSec);
				}
				else
				{
					if (str3 == "QueryExpressionComputerSet" || str3 == "ClassNameComputerSet")
					{
						cimIndicationWatcher = new CimIndicationWatcher(this.ComputerName, this.Namespace, this.QueryDialect, empty, this.OperationTimeoutSec);
					}
				}
			}
			if (cimIndicationWatcher != null)
			{
				cimIndicationWatcher.SetCmdlet(this);
			}
			return cimIndicationWatcher;
		}

		protected override string GetSourceObjectEventName()
		{
			return "CimIndicationArrived";
		}

		private static void newSubscriber_Unsubscribed(object sender, PSEventUnsubscribedEventArgs e)
		{
			DebugHelper.WriteLogEx();
			CimIndicationWatcher cimIndicationWatcher = sender as CimIndicationWatcher;
			if (cimIndicationWatcher != null)
			{
				cimIndicationWatcher.Stop();
			}
		}

		private void SetParameter(object value, string parameterName)
		{
			if (value != null)
			{
				this.parameterBinder.SetParameter(parameterName, true);
				return;
			}
			else
			{
				return;
			}
		}
	}
}