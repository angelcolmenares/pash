using Microsoft.Management.Infrastructure;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Management.Automation;

namespace Microsoft.Management.Infrastructure.CimCmdlets
{
	[Cmdlet("Set", "CimInstance", SupportsShouldProcess=true, DefaultParameterSetName="CimInstanceComputerSet", HelpUri="http://go.microsoft.com/fwlink/?LinkId=227962")]
	public class SetCimInstanceCommand : CimBaseCommand
	{
		internal const string nameCimSession = "CimSession";

		internal const string nameComputerName = "ComputerName";

		internal const string nameResourceUri = "ResourceUri";

		internal const string nameNamespace = "Namespace";

		internal const string nameCimInstance = "InputObject";

		internal const string nameQuery = "Query";

		internal const string nameQueryDialect = "QueryDialect";

		internal const string nameProperty = "Property";

		private CimSession[] cimSession;

		private string[] computername;

		private Uri resourceUri;

		private string nameSpace;

		private uint operationTimeout;

		private CimInstance cimInstance;

		private string query;

		private string querydialect;

		private IDictionary property;

		private SwitchParameter passThru;

		private static Dictionary<string, HashSet<ParameterDefinitionEntry>> parameters;

		private static Dictionary<string, ParameterSetEntry> parameterSets;

		internal CimInstance CimInstance
		{
			get
			{
				return this.cimInstance;
			}
		}

		[Parameter(Mandatory=true, ValueFromPipeline=true, ParameterSetName="QuerySessionSet")]
		[Parameter(Mandatory=true, ValueFromPipeline=true, ParameterSetName="CimInstanceSessionSet")]
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
		[Parameter(Mandatory=true, Position=0, ValueFromPipeline=true, ParameterSetName="CimInstanceComputerSet")]
		[Parameter(Mandatory=true, Position=0, ValueFromPipeline=true, ParameterSetName="CimInstanceSessionSet")]
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

		[Parameter(ValueFromPipelineByPropertyName=true, ParameterSetName="QueryComputerSet")]
		[Parameter(ValueFromPipelineByPropertyName=true, ParameterSetName="QuerySessionSet")]
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

		[Parameter]
		[ValidateNotNull]
		public SwitchParameter PassThru
		{
			get
			{
				return this.passThru;
			}
			set
			{
				this.passThru = value;
			}
		}

		[Alias(new string[] { "Arguments" })]
		[Parameter(Mandatory=true, ValueFromPipelineByPropertyName=true, ParameterSetName="QueryComputerSet")]
		[Parameter(Mandatory=true, ValueFromPipelineByPropertyName=true, ParameterSetName="QuerySessionSet")]
		[Parameter(ValueFromPipelineByPropertyName=true, ParameterSetName="CimInstanceSessionSet")]
		[Parameter(ValueFromPipelineByPropertyName=true, ParameterSetName="CimInstanceComputerSet")]
		[SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
		public IDictionary Property
		{
			get
			{
				return this.property;
			}
			set
			{
				this.property = value;
				base.SetParameter(value, "Property");
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

		[Parameter(ValueFromPipelineByPropertyName=true, ParameterSetName="QuerySessionSet")]
		[Parameter(ValueFromPipelineByPropertyName=true, ParameterSetName="QueryComputerSet")]
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

		static SetCimInstanceCommand()
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
			parameterDefinitionEntries6.Add(new ParameterDefinitionEntry("QuerySessionSet", true));
			parameterDefinitionEntries6.Add(new ParameterDefinitionEntry("QueryComputerSet", true));
			parameterDefinitionEntries6.Add(new ParameterDefinitionEntry("CimInstanceSessionSet", false));
			parameterDefinitionEntries6.Add(new ParameterDefinitionEntry("CimInstanceComputerSet", false));
			strs.Add("Property", parameterDefinitionEntries6);
			HashSet<ParameterDefinitionEntry> parameterDefinitionEntries7 = new HashSet<ParameterDefinitionEntry>();
			parameterDefinitionEntries7.Add(new ParameterDefinitionEntry("CimInstanceComputerSet", false));
			parameterDefinitionEntries7.Add(new ParameterDefinitionEntry("CimInstanceSessionSet", false));
			strs.Add("ResourceUri", parameterDefinitionEntries7);
			SetCimInstanceCommand.parameters = strs;
			Dictionary<string, ParameterSetEntry> strs1 = new Dictionary<string, ParameterSetEntry>();
			strs1.Add("QuerySessionSet", new ParameterSetEntry(3));
			strs1.Add("QueryComputerSet", new ParameterSetEntry(2));
			strs1.Add("CimInstanceSessionSet", new ParameterSetEntry(2));
			strs1.Add("CimInstanceComputerSet", new ParameterSetEntry(1, true));
			SetCimInstanceCommand.parameterSets = strs1;
		}

		public SetCimInstanceCommand() : base(SetCimInstanceCommand.parameters, SetCimInstanceCommand.parameterSets)
		{
		}

		protected override void BeginProcessing()
		{
			CimSetCimInstance operationAgent = this.GetOperationAgent();
			if (operationAgent == null)
			{
				operationAgent = this.CreateOperationAgent();
			}
			this.CmdletOperation = new CmdletOperationSetCimInstance(this, operationAgent);
			base.AtBeginProcess = false;
		}

		private CimSetCimInstance CreateOperationAgent()
		{
			CimSetCimInstance cimSetCimInstance = new CimSetCimInstance();
			base.AsyncOperation = cimSetCimInstance;
			return cimSetCimInstance;
		}

		protected override void EndProcessing()
		{
			CimSetCimInstance operationAgent = this.GetOperationAgent();
			if (operationAgent != null)
			{
				operationAgent.ProcessRemainActions(this.CmdletOperation);
			}
		}

		private CimSetCimInstance GetOperationAgent()
		{
			return base.AsyncOperation as CimSetCimInstance;
		}

		protected override void ProcessRecord()
		{
			base.CheckParameterSet();
			CimSetCimInstance operationAgent = this.GetOperationAgent();
			operationAgent.SetCimInstance(this);
			operationAgent.ProcessActions(this.CmdletOperation);
		}
	}
}