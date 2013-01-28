using Microsoft.Management.Infrastructure;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Management.Automation;

namespace Microsoft.Management.Infrastructure.CimCmdlets
{
	[Cmdlet("Remove", "CimInstance", SupportsShouldProcess=true, DefaultParameterSetName="CimInstanceComputerSet", HelpUri="http://go.microsoft.com/fwlink/?LinkId=227964")]
	public class RemoveCimInstanceCommand : CimBaseCommand
	{
		internal const string nameCimSession = "CimSession";

		internal const string nameComputerName = "ComputerName";

		internal const string nameResourceUri = "ResourceUri";

		internal const string nameNamespace = "Namespace";

		internal const string nameCimInstance = "InputObject";

		internal const string nameQuery = "Query";

		internal const string nameQueryDialect = "QueryDialect";

		private CimSession[] cimSession;

		private Uri resourceUri;

		private string[] computername;

		private string nameSpace;

		private uint operationTimeout;

		private CimInstance cimInstance;

		private string query;

		private string querydialect;

		private static Dictionary<string, HashSet<ParameterDefinitionEntry>> parameters;

		private static Dictionary<string, ParameterSetEntry> parameterSets;

		internal CimInstance CimInstance
		{
			get
			{
				return this.cimInstance;
			}
		}

		[Parameter(Mandatory=true, ValueFromPipeline=true, ParameterSetName="CimInstanceSessionSet")]
		[Parameter(Mandatory=true, ValueFromPipeline=true, ParameterSetName="QuerySessionSet")]
		[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
		public CimSession[] CimSession
		{
			get
			{
				return this.cimSession;
			}
			set
			{
				this.cimSession = value;
				base.SetParameter(value, "CimSession");
			}
		}

		[Alias(new string[] { "CN", "ServerName" })]
		[Parameter(ValueFromPipelineByPropertyName=true, ParameterSetName="QueryComputerSet")]
		[Parameter(ParameterSetName="CimInstanceComputerSet")]
		[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
		public string[] ComputerName
		{
			get
			{
				return this.computername;
			}
			set
			{
				this.computername = value;
				base.SetParameter(value, "ComputerName");
			}
		}

		[Alias(new string[] { "CimInstance" })]
		[Parameter(Mandatory=true, Position=0, ValueFromPipeline=true, ParameterSetName="CimInstanceSessionSet")]
		[Parameter(Mandatory=true, Position=0, ValueFromPipeline=true, ParameterSetName="CimInstanceComputerSet")]
		public CimInstance InputObject
		{
			get
			{
				return this.cimInstance;
			}
			set
			{
				this.cimInstance = value;
				base.SetParameter(value, "InputObject");
			}
		}

		[Parameter(Position=1, ValueFromPipelineByPropertyName=true, ParameterSetName="QueryComputerSet")]
		[Parameter(Position=1, ValueFromPipelineByPropertyName=true, ParameterSetName="QuerySessionSet")]
		public string Namespace
		{
			get
			{
				return this.nameSpace;
			}
			set
			{
				this.nameSpace = value;
				base.SetParameter(value, "Namespace");
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

		[Parameter(Mandatory=true, Position=0, ValueFromPipelineByPropertyName=true, ParameterSetName="QueryComputerSet")]
		[Parameter(Mandatory=true, Position=0, ValueFromPipelineByPropertyName=true, ParameterSetName="QuerySessionSet")]
		public string Query
		{
			get
			{
				return this.query;
			}
			set
			{
				this.query = value;
				base.SetParameter(value, "Query");
			}
		}

		[Parameter(ValueFromPipelineByPropertyName=true, ParameterSetName="QueryComputerSet")]
		[Parameter(ValueFromPipelineByPropertyName=true, ParameterSetName="QuerySessionSet")]
		public string QueryDialect
		{
			get
			{
				return this.querydialect;
			}
			set
			{
				this.querydialect = value;
				base.SetParameter(value, "QueryDialect");
			}
		}

		[Parameter(ValueFromPipelineByPropertyName=true, ParameterSetName="CimInstanceComputerSet")]
		[Parameter(ValueFromPipelineByPropertyName=true, ParameterSetName="CimInstanceSessionSet")]
		public Uri ResourceUri
		{
			get
			{
				return this.resourceUri;
			}
			set
			{
				this.resourceUri = value;
				base.SetParameter(value, "ResourceUri");
			}
		}

		static RemoveCimInstanceCommand()
		{
			Dictionary<string, HashSet<ParameterDefinitionEntry>> strs = new Dictionary<string, HashSet<ParameterDefinitionEntry>>();
			HashSet<ParameterDefinitionEntry> parameterDefinitionEntries = new HashSet<ParameterDefinitionEntry>();
			parameterDefinitionEntries.Add(new ParameterDefinitionEntry("CimInstanceSessionSet", true));
			parameterDefinitionEntries.Add(new ParameterDefinitionEntry("QuerySessionSet", true));
			strs.Add("CimSession", parameterDefinitionEntries);
			HashSet<ParameterDefinitionEntry> parameterDefinitionEntries1 = new HashSet<ParameterDefinitionEntry>();
			parameterDefinitionEntries1.Add(new ParameterDefinitionEntry("QueryComputerSet", false));
			parameterDefinitionEntries1.Add(new ParameterDefinitionEntry("CimInstanceComputerSet", false));
			strs.Add("ComputerName", parameterDefinitionEntries1);
			HashSet<ParameterDefinitionEntry> parameterDefinitionEntries2 = new HashSet<ParameterDefinitionEntry>();
			parameterDefinitionEntries2.Add(new ParameterDefinitionEntry("QuerySessionSet", false));
			parameterDefinitionEntries2.Add(new ParameterDefinitionEntry("QueryComputerSet", false));
			strs.Add("Namespace", parameterDefinitionEntries2);
			HashSet<ParameterDefinitionEntry> parameterDefinitionEntries3 = new HashSet<ParameterDefinitionEntry>();
			parameterDefinitionEntries3.Add(new ParameterDefinitionEntry("CimInstanceComputerSet", true));
			parameterDefinitionEntries3.Add(new ParameterDefinitionEntry("CimInstanceSessionSet", true));
			strs.Add("InputObject", parameterDefinitionEntries3);
			HashSet<ParameterDefinitionEntry> parameterDefinitionEntries4 = new HashSet<ParameterDefinitionEntry>();
			parameterDefinitionEntries4.Add(new ParameterDefinitionEntry("QueryComputerSet", true));
			parameterDefinitionEntries4.Add(new ParameterDefinitionEntry("QuerySessionSet", true));
			strs.Add("Query", parameterDefinitionEntries4);
			HashSet<ParameterDefinitionEntry> parameterDefinitionEntries5 = new HashSet<ParameterDefinitionEntry>();
			parameterDefinitionEntries5.Add(new ParameterDefinitionEntry("QuerySessionSet", false));
			parameterDefinitionEntries5.Add(new ParameterDefinitionEntry("QueryComputerSet", false));
			strs.Add("QueryDialect", parameterDefinitionEntries5);
			HashSet<ParameterDefinitionEntry> parameterDefinitionEntries6 = new HashSet<ParameterDefinitionEntry>();
			parameterDefinitionEntries6.Add(new ParameterDefinitionEntry("CimInstanceComputerSet", false));
			parameterDefinitionEntries6.Add(new ParameterDefinitionEntry("CimInstanceSessionSet", false));
			strs.Add("ResourceUri", parameterDefinitionEntries6);
			RemoveCimInstanceCommand.parameters = strs;
			Dictionary<string, ParameterSetEntry> strs1 = new Dictionary<string, ParameterSetEntry>();
			strs1.Add("CimInstanceComputerSet", new ParameterSetEntry(1, true));
			strs1.Add("CimInstanceSessionSet", new ParameterSetEntry(2));
			strs1.Add("QueryComputerSet", new ParameterSetEntry(1));
			strs1.Add("QuerySessionSet", new ParameterSetEntry(2));
			RemoveCimInstanceCommand.parameterSets = strs1;
		}

		public RemoveCimInstanceCommand() : base(RemoveCimInstanceCommand.parameters, RemoveCimInstanceCommand.parameterSets)
		{
			DebugHelper.WriteLogEx();
		}

		protected override void BeginProcessing()
		{
			CimRemoveCimInstance operationAgent = this.GetOperationAgent();
			if (operationAgent == null)
			{
				operationAgent = this.CreateOperationAgent();
			}
			this.CmdletOperation = new CmdletOperationRemoveCimInstance(this, operationAgent);
			base.AtBeginProcess = false;
		}

		private CimRemoveCimInstance CreateOperationAgent()
		{
			CimRemoveCimInstance cimRemoveCimInstance = new CimRemoveCimInstance();
			base.AsyncOperation = cimRemoveCimInstance;
			return cimRemoveCimInstance;
		}

		protected override void EndProcessing()
		{
			CimRemoveCimInstance operationAgent = this.GetOperationAgent();
			if (operationAgent != null)
			{
				operationAgent.ProcessRemainActions(this.CmdletOperation);
			}
		}

		private CimRemoveCimInstance GetOperationAgent()
		{
			return base.AsyncOperation as CimRemoveCimInstance;
		}

		protected override void ProcessRecord()
		{
			base.CheckParameterSet();
			CimRemoveCimInstance operationAgent = this.GetOperationAgent();
			operationAgent.RemoveCimInstance(this);
			operationAgent.ProcessActions(this.CmdletOperation);
		}
	}
}