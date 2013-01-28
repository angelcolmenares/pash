using Microsoft.Management.Infrastructure;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Management.Automation;

namespace Microsoft.Management.Infrastructure.CimCmdlets
{
	[Cmdlet("Get", "CimInstance", DefaultParameterSetName="ClassNameComputerSet", HelpUri="http://go.microsoft.com/fwlink/?LinkId=227961")]
	[OutputType(new Type[] { typeof(CimInstance) })]
	public class GetCimInstanceCommand : CimBaseCommand
	{
		internal const string nameCimInstance = "InputObject";

		internal const string nameCimSession = "CimSession";

		internal const string nameClassName = "ClassName";

		internal const string nameResourceUri = "ResourceUri";

		internal const string nameComputerName = "ComputerName";

		internal const string nameFilter = "Filter";

		internal const string nameKeyOnly = "KeyOnly";

		internal const string nameNamespace = "Namespace";

		internal const string nameOperationTimeoutSec = "OperationTimeoutSec";

		internal const string nameQuery = "Query";

		internal const string nameQueryDialect = "QueryDialect";

		internal const string nameSelectProperties = "Property";

		internal const string nameShallow = "Shallow";

		private CimSession[] cimSession;

		private string className;

		private Uri resourceUri;

		private string[] computerName;

		private SwitchParameter keyOnly;

		private string nameSpace;

		private uint operationTimeout;

		private CimInstance cimInstance;

		private string query;

		private string queryDialect;

		private SwitchParameter shallow;

		private string filter;

		private string[] property;

		private static Dictionary<string, HashSet<ParameterDefinitionEntry>> parameters;

		private static Dictionary<string, ParameterSetEntry> parameterSets;

		internal CimInstance CimInstance
		{
			get
			{
				return this.cimInstance;
			}
		}

		[Parameter(Mandatory=true, ValueFromPipeline=true, ParameterSetName="ResourceUriSessionSet")]
		[Parameter(Mandatory=true, ValueFromPipeline=true, ParameterSetName="CimInstanceSessionSet")]
		[Parameter(Mandatory=true, ValueFromPipeline=true, ParameterSetName="QuerySessionSet")]
		[Parameter(Mandatory=true, ValueFromPipeline=true, ParameterSetName="ClassNameSessionSet")]
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

		[Parameter(Mandatory=true, Position=0, ValueFromPipelineByPropertyName=true, ParameterSetName="ClassNameComputerSet")]
		[Parameter(Mandatory=true, Position=0, ValueFromPipelineByPropertyName=true, ParameterSetName="ClassNameSessionSet")]
		public string ClassName
		{
			get
			{
				return this.className;
			}
			set
			{
				this.className = value;
				base.SetParameter(value, "ClassName");
			}
		}

		[Alias(new string[] { "CN", "ServerName" })]
		[Parameter(ValueFromPipelineByPropertyName=true, ParameterSetName="ClassNameComputerSet")]
		[Parameter(ValueFromPipelineByPropertyName=true, ParameterSetName="ResourceUriComputerSet")]
		[Parameter(ValueFromPipelineByPropertyName=true, ParameterSetName="QueryComputerSet")]
		[Parameter(ParameterSetName="CimInstanceComputerSet")]
		[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
		public string[] ComputerName
		{
			get
			{
				return this.computerName;
			}
			set
			{
				this.computerName = value;
				base.SetParameter(value, "ComputerName");
			}
		}

		[Parameter(ValueFromPipelineByPropertyName=true, ParameterSetName="ResourceUriComputerSet")]
		[Parameter(ValueFromPipelineByPropertyName=true, ParameterSetName="ResourceUriSessionSet")]
		[Parameter(ValueFromPipelineByPropertyName=true, ParameterSetName="ClassNameSessionSet")]
		[Parameter(ValueFromPipelineByPropertyName=true, ParameterSetName="ClassNameComputerSet")]
		public string Filter
		{
			get
			{
				return this.filter;
			}
			set
			{
				this.filter = value;
				base.SetParameter(value, "Filter");
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

		[Parameter(ParameterSetName="ClassNameSessionSet")]
		[Parameter(ParameterSetName="ResourceUriSessionSet")]
		[Parameter(ParameterSetName="ClassNameComputerSet")]
		[Parameter(ParameterSetName="ResourceUriComputerSet")]
		public SwitchParameter KeyOnly
		{
			get
			{
				return this.keyOnly;
			}
			set
			{
				this.keyOnly = value;
				base.SetParameter(value, "KeyOnly");
			}
		}

		[Parameter(ValueFromPipelineByPropertyName=true, ParameterSetName="QueryComputerSet")]
		[Parameter(ValueFromPipelineByPropertyName=true, ParameterSetName="ResourceUriComputerSet")]
		[Parameter(ValueFromPipelineByPropertyName=true, ParameterSetName="QuerySessionSet")]
		[Parameter(ValueFromPipelineByPropertyName=true, ParameterSetName="ResourceUriSessionSet")]
		[Parameter(ValueFromPipelineByPropertyName=true, ParameterSetName="ClassNameComputerSet")]
		[Parameter(ValueFromPipelineByPropertyName=true, ParameterSetName="ClassNameSessionSet")]
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

		[Alias(new string[] { "SelectProperties" })]
		[Parameter(ValueFromPipelineByPropertyName=true, ParameterSetName="ResourceUriComputerSet")]
		[Parameter(ValueFromPipelineByPropertyName=true, ParameterSetName="ClassNameSessionSet")]
		[Parameter(ValueFromPipelineByPropertyName=true, ParameterSetName="ClassNameComputerSet")]
		[Parameter(ValueFromPipelineByPropertyName=true, ParameterSetName="ResourceUriSessionSet")]
		[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
		public string[] Property
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

		[Parameter(Mandatory=true, ValueFromPipelineByPropertyName=true, ParameterSetName="QuerySessionSet")]
		[Parameter(Mandatory=true, ValueFromPipelineByPropertyName=true, ParameterSetName="QueryComputerSet")]
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

		[Parameter(ValueFromPipelineByPropertyName=true, ParameterSetName="ClassNameComputerSet")]
		[Parameter(ValueFromPipelineByPropertyName=true, ParameterSetName="ClassNameSessionSet")]
		[Parameter(ValueFromPipelineByPropertyName=true, ParameterSetName="QuerySessionSet")]
		[Parameter(ValueFromPipelineByPropertyName=true, ParameterSetName="QueryComputerSet")]
		public string QueryDialect
		{
			get
			{
				return this.queryDialect;
			}
			set
			{
				this.queryDialect = value;
				base.SetParameter(value, "QueryDialect");
			}
		}

		[Parameter(ParameterSetName="CimInstanceSessionSet")]
		[Parameter(Mandatory=true, ValueFromPipelineByPropertyName=true, ParameterSetName="ResourceUriSessionSet")]
		[Parameter(Mandatory=true, ValueFromPipelineByPropertyName=true, ParameterSetName="ResourceUriComputerSet")]
		[Parameter(ParameterSetName="CimInstanceComputerSet")]
		[Parameter(ValueFromPipelineByPropertyName=true, ParameterSetName="QueryComputerSet")]
		[Parameter(ValueFromPipelineByPropertyName=true, ParameterSetName="QuerySessionSet")]
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

		internal string[] SelectProperties
		{
			get
			{
				return this.property;
			}
		}

		[Parameter(ParameterSetName="ResourceUriSessionSet")]
		[Parameter(ParameterSetName="ClassNameSessionSet")]
		[Parameter(ParameterSetName="QuerySessionSet")]
		[Parameter(ParameterSetName="ClassNameComputerSet")]
		[Parameter(ParameterSetName="ResourceUriComputerSet")]
		[Parameter(ParameterSetName="QueryComputerSet")]
		public SwitchParameter Shallow
		{
			get
			{
				return this.shallow;
			}
			set
			{
				this.shallow = value;
				base.SetParameter(value, "Shallow");
			}
		}

		static GetCimInstanceCommand()
		{
			Dictionary<string, HashSet<ParameterDefinitionEntry>> strs = new Dictionary<string, HashSet<ParameterDefinitionEntry>>();
			HashSet<ParameterDefinitionEntry> parameterDefinitionEntries = new HashSet<ParameterDefinitionEntry>();
			parameterDefinitionEntries.Add(new ParameterDefinitionEntry("QuerySessionSet", true));
			parameterDefinitionEntries.Add(new ParameterDefinitionEntry("CimInstanceSessionSet", true));
			parameterDefinitionEntries.Add(new ParameterDefinitionEntry("ClassNameSessionSet", true));
			parameterDefinitionEntries.Add(new ParameterDefinitionEntry("ResourceUriSessionSet", true));
			strs.Add("CimSession", parameterDefinitionEntries);
			HashSet<ParameterDefinitionEntry> parameterDefinitionEntries1 = new HashSet<ParameterDefinitionEntry>();
			parameterDefinitionEntries1.Add(new ParameterDefinitionEntry("ResourceUriSessionSet", true));
			parameterDefinitionEntries1.Add(new ParameterDefinitionEntry("ResourceUriComputerSet", true));
			parameterDefinitionEntries1.Add(new ParameterDefinitionEntry("CimInstanceComputerSet", false));
			parameterDefinitionEntries1.Add(new ParameterDefinitionEntry("CimInstanceSessionSet", false));
			parameterDefinitionEntries1.Add(new ParameterDefinitionEntry("QueryComputerSet", false));
			parameterDefinitionEntries1.Add(new ParameterDefinitionEntry("QuerySessionSet", false));
			strs.Add("ResourceUri", parameterDefinitionEntries1);
			HashSet<ParameterDefinitionEntry> parameterDefinitionEntries2 = new HashSet<ParameterDefinitionEntry>();
			parameterDefinitionEntries2.Add(new ParameterDefinitionEntry("ClassNameSessionSet", true));
			parameterDefinitionEntries2.Add(new ParameterDefinitionEntry("ClassNameComputerSet", true));
			strs.Add("ClassName", parameterDefinitionEntries2);
			HashSet<ParameterDefinitionEntry> parameterDefinitionEntries3 = new HashSet<ParameterDefinitionEntry>();
			parameterDefinitionEntries3.Add(new ParameterDefinitionEntry("ClassNameComputerSet", false));
			parameterDefinitionEntries3.Add(new ParameterDefinitionEntry("QueryComputerSet", false));
			parameterDefinitionEntries3.Add(new ParameterDefinitionEntry("CimInstanceComputerSet", false));
			parameterDefinitionEntries3.Add(new ParameterDefinitionEntry("ResourceUriComputerSet", false));
			strs.Add("ComputerName", parameterDefinitionEntries3);
			HashSet<ParameterDefinitionEntry> parameterDefinitionEntries4 = new HashSet<ParameterDefinitionEntry>();
			parameterDefinitionEntries4.Add(new ParameterDefinitionEntry("ClassNameComputerSet", false));
			parameterDefinitionEntries4.Add(new ParameterDefinitionEntry("ClassNameSessionSet", false));
			parameterDefinitionEntries4.Add(new ParameterDefinitionEntry("ResourceUriSessionSet", false));
			parameterDefinitionEntries4.Add(new ParameterDefinitionEntry("ResourceUriComputerSet", false));
			strs.Add("KeyOnly", parameterDefinitionEntries4);
			HashSet<ParameterDefinitionEntry> parameterDefinitionEntries5 = new HashSet<ParameterDefinitionEntry>();
			parameterDefinitionEntries5.Add(new ParameterDefinitionEntry("ClassNameComputerSet", false));
			parameterDefinitionEntries5.Add(new ParameterDefinitionEntry("ClassNameSessionSet", false));
			parameterDefinitionEntries5.Add(new ParameterDefinitionEntry("ResourceUriSessionSet", false));
			parameterDefinitionEntries5.Add(new ParameterDefinitionEntry("ResourceUriComputerSet", false));
			parameterDefinitionEntries5.Add(new ParameterDefinitionEntry("QueryComputerSet", false));
			parameterDefinitionEntries5.Add(new ParameterDefinitionEntry("QuerySessionSet", false));
			strs.Add("Namespace", parameterDefinitionEntries5);
			HashSet<ParameterDefinitionEntry> parameterDefinitionEntries6 = new HashSet<ParameterDefinitionEntry>();
			parameterDefinitionEntries6.Add(new ParameterDefinitionEntry("CimInstanceComputerSet", true));
			parameterDefinitionEntries6.Add(new ParameterDefinitionEntry("CimInstanceSessionSet", true));
			strs.Add("InputObject", parameterDefinitionEntries6);
			HashSet<ParameterDefinitionEntry> parameterDefinitionEntries7 = new HashSet<ParameterDefinitionEntry>();
			parameterDefinitionEntries7.Add(new ParameterDefinitionEntry("QuerySessionSet", true));
			parameterDefinitionEntries7.Add(new ParameterDefinitionEntry("QueryComputerSet", true));
			strs.Add("Query", parameterDefinitionEntries7);
			HashSet<ParameterDefinitionEntry> parameterDefinitionEntries8 = new HashSet<ParameterDefinitionEntry>();
			parameterDefinitionEntries8.Add(new ParameterDefinitionEntry("QuerySessionSet", false));
			parameterDefinitionEntries8.Add(new ParameterDefinitionEntry("QueryComputerSet", false));
			parameterDefinitionEntries8.Add(new ParameterDefinitionEntry("ClassNameSessionSet", false));
			parameterDefinitionEntries8.Add(new ParameterDefinitionEntry("ClassNameComputerSet", false));
			strs.Add("QueryDialect", parameterDefinitionEntries8);
			HashSet<ParameterDefinitionEntry> parameterDefinitionEntries9 = new HashSet<ParameterDefinitionEntry>();
			parameterDefinitionEntries9.Add(new ParameterDefinitionEntry("QuerySessionSet", false));
			parameterDefinitionEntries9.Add(new ParameterDefinitionEntry("QueryComputerSet", false));
			parameterDefinitionEntries9.Add(new ParameterDefinitionEntry("ResourceUriSessionSet", false));
			parameterDefinitionEntries9.Add(new ParameterDefinitionEntry("ResourceUriComputerSet", false));
			parameterDefinitionEntries9.Add(new ParameterDefinitionEntry("ClassNameSessionSet", false));
			parameterDefinitionEntries9.Add(new ParameterDefinitionEntry("ClassNameComputerSet", false));
			strs.Add("Shallow", parameterDefinitionEntries9);
			HashSet<ParameterDefinitionEntry> parameterDefinitionEntries10 = new HashSet<ParameterDefinitionEntry>();
			parameterDefinitionEntries10.Add(new ParameterDefinitionEntry("ClassNameSessionSet", false));
			parameterDefinitionEntries10.Add(new ParameterDefinitionEntry("ClassNameComputerSet", false));
			parameterDefinitionEntries10.Add(new ParameterDefinitionEntry("ResourceUriSessionSet", false));
			parameterDefinitionEntries10.Add(new ParameterDefinitionEntry("ResourceUriComputerSet", false));
			strs.Add("Filter", parameterDefinitionEntries10);
			HashSet<ParameterDefinitionEntry> parameterDefinitionEntries11 = new HashSet<ParameterDefinitionEntry>();
			parameterDefinitionEntries11.Add(new ParameterDefinitionEntry("ClassNameSessionSet", false));
			parameterDefinitionEntries11.Add(new ParameterDefinitionEntry("ClassNameComputerSet", false));
			parameterDefinitionEntries11.Add(new ParameterDefinitionEntry("ResourceUriSessionSet", false));
			parameterDefinitionEntries11.Add(new ParameterDefinitionEntry("ResourceUriComputerSet", false));
			strs.Add("Property", parameterDefinitionEntries11);
			GetCimInstanceCommand.parameters = strs;
			Dictionary<string, ParameterSetEntry> strs1 = new Dictionary<string, ParameterSetEntry>();
			strs1.Add("CimInstanceComputerSet", new ParameterSetEntry(1));
			strs1.Add("CimInstanceSessionSet", new ParameterSetEntry(2));
			strs1.Add("ClassNameComputerSet", new ParameterSetEntry(1, true));
			strs1.Add("ClassNameSessionSet", new ParameterSetEntry(2));
			strs1.Add("QueryComputerSet", new ParameterSetEntry(1));
			strs1.Add("ResourceUriSessionSet", new ParameterSetEntry(2));
			strs1.Add("ResourceUriComputerSet", new ParameterSetEntry(1));
			strs1.Add("QuerySessionSet", new ParameterSetEntry(2));
			GetCimInstanceCommand.parameterSets = strs1;
		}

		public GetCimInstanceCommand() : base(GetCimInstanceCommand.parameters, GetCimInstanceCommand.parameterSets)
		{
			DebugHelper.WriteLogEx();
		}

		protected override void BeginProcessing()
		{
			this.CmdletOperation = new CmdletOperationBase(this);
			base.AtBeginProcess = false;
		}

		private void CheckArgument()
		{
			string parameterSetName = base.ParameterSetName;
			string str = parameterSetName;
			if (parameterSetName != null)
			{
				if (str == "ClassNameComputerSet" || str == "ClassNameSessionSet")
				{
					this.className = ValidationHelper.ValidateArgumentIsValidName("ClassName", this.className);
					this.property = ValidationHelper.ValidateArgumentIsValidName("Property", this.property);
				}
				else
				{
					return;
				}
			}
		}

		private CimGetInstance CreateOperationAgent()
		{
			CimGetInstance cimGetInstance = new CimGetInstance();
			base.AsyncOperation = cimGetInstance;
			return cimGetInstance;
		}

		protected override void EndProcessing()
		{
			CimGetInstance operationAgent = this.GetOperationAgent();
			if (operationAgent != null)
			{
				operationAgent.ProcessRemainActions(this.CmdletOperation);
			}
		}

		private CimGetInstance GetOperationAgent()
		{
			return base.AsyncOperation as CimGetInstance;
		}

		protected override void ProcessRecord()
		{
			base.CheckParameterSet();
			this.CheckArgument();
			CimGetInstance operationAgent = this.GetOperationAgent();
			if (operationAgent == null)
			{
				operationAgent = this.CreateOperationAgent();
			}
			operationAgent.GetCimInstance(this);
			operationAgent.ProcessActions(this.CmdletOperation);
		}
	}
}