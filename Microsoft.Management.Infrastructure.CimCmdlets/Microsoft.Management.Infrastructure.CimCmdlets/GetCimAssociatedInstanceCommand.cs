using Microsoft.Management.Infrastructure;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Management.Automation;

namespace Microsoft.Management.Infrastructure.CimCmdlets
{
	[Cmdlet("Get", "CimAssociatedInstance", DefaultParameterSetName="ComputerSet", HelpUri="http://go.microsoft.com/fwlink/?LinkId=227958")]
	[OutputType(new Type[] { typeof(CimInstance) })]
	public class GetCimAssociatedInstanceCommand : CimBaseCommand
	{
		internal const string Noun = "CimAssociatedInstance";

		internal const string nameCimInstance = "InputObject";

		internal const string nameComputerName = "ComputerName";

		internal const string nameCimSession = "CimSession";

		internal const string nameResourceUri = "ResourceUri";

		private string association;

		private string resultClassName;

		private CimInstance cimInstance;

		private string nameSpace;

		private uint operationTimeout;

		private Uri resourceUri;

		private string[] computerName;

		private CimSession[] cimSession;

		private SwitchParameter keyOnly;

		private static Dictionary<string, HashSet<ParameterDefinitionEntry>> parameters;

		private static Dictionary<string, ParameterSetEntry> parameterSets;

		[Parameter(Position=1, ValueFromPipelineByPropertyName=true)]
		public string Association
		{
			get
			{
				return this.association;
			}
			set
			{
				this.association = value;
			}
		}

		internal CimInstance CimInstance
		{
			get
			{
				return this.cimInstance;
			}
		}

		[Parameter(Mandatory=true, ValueFromPipeline=true, ParameterSetName="SessionSet")]
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
		[Parameter(ParameterSetName="ComputerSet")]
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

		[Alias(new string[] { "CimInstance" })]
		[Parameter(Mandatory=true, Position=0, ValueFromPipeline=true)]
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

		[Parameter]
		public SwitchParameter KeyOnly
		{
			get
			{
				return this.keyOnly;
			}
			set
			{
				this.keyOnly = value;
			}
		}

		[Parameter(ValueFromPipelineByPropertyName=true)]
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
		[Parameter(ValueFromPipelineByPropertyName=true)]
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

		[Parameter]
		public string ResultClassName
		{
			get
			{
				return this.resultClassName;
			}
			set
			{
				this.resultClassName = value;
			}
		}

		static GetCimAssociatedInstanceCommand()
		{
			Dictionary<string, HashSet<ParameterDefinitionEntry>> strs = new Dictionary<string, HashSet<ParameterDefinitionEntry>>();
			HashSet<ParameterDefinitionEntry> parameterDefinitionEntries = new HashSet<ParameterDefinitionEntry>();
			parameterDefinitionEntries.Add(new ParameterDefinitionEntry("ComputerSet", false));
			strs.Add("ComputerName", parameterDefinitionEntries);
			HashSet<ParameterDefinitionEntry> parameterDefinitionEntries1 = new HashSet<ParameterDefinitionEntry>();
			parameterDefinitionEntries1.Add(new ParameterDefinitionEntry("SessionSet", true));
			strs.Add("CimSession", parameterDefinitionEntries1);
			HashSet<ParameterDefinitionEntry> parameterDefinitionEntries2 = new HashSet<ParameterDefinitionEntry>();
			parameterDefinitionEntries2.Add(new ParameterDefinitionEntry("ComputerSet", true));
			parameterDefinitionEntries2.Add(new ParameterDefinitionEntry("SessionSet", true));
			strs.Add("InputObject", parameterDefinitionEntries2);
			HashSet<ParameterDefinitionEntry> parameterDefinitionEntries3 = new HashSet<ParameterDefinitionEntry>();
			parameterDefinitionEntries3.Add(new ParameterDefinitionEntry("ComputerSet", false));
			parameterDefinitionEntries3.Add(new ParameterDefinitionEntry("SessionSet", false));
			strs.Add("ResourceUri", parameterDefinitionEntries3);
			GetCimAssociatedInstanceCommand.parameters = strs;
			Dictionary<string, ParameterSetEntry> strs1 = new Dictionary<string, ParameterSetEntry>();
			strs1.Add("SessionSet", new ParameterSetEntry(2, false));
			strs1.Add("ComputerSet", new ParameterSetEntry(1, true));
			GetCimAssociatedInstanceCommand.parameterSets = strs1;
		}

		public GetCimAssociatedInstanceCommand() : base(GetCimAssociatedInstanceCommand.parameters, GetCimAssociatedInstanceCommand.parameterSets)
		{
			DebugHelper.WriteLogEx();
		}

		protected override void BeginProcessing()
		{
			this.CmdletOperation = new CmdletOperationBase(this);
			base.AtBeginProcess = false;
		}

		private CimGetAssociatedInstance CreateOperationAgent()
		{
			base.AsyncOperation = new CimGetAssociatedInstance();
			return this.GetOperationAgent();
		}

		protected override void EndProcessing()
		{
			CimGetAssociatedInstance operationAgent = this.GetOperationAgent();
			if (operationAgent != null)
			{
				operationAgent.ProcessRemainActions(this.CmdletOperation);
			}
		}

		private CimGetAssociatedInstance GetOperationAgent()
		{
			return base.AsyncOperation as CimGetAssociatedInstance;
		}

		protected override void ProcessRecord()
		{
			base.CheckParameterSet();
			CimGetAssociatedInstance operationAgent = this.GetOperationAgent();
			if (operationAgent == null)
			{
				operationAgent = this.CreateOperationAgent();
			}
			operationAgent.GetCimAssociatedInstance(this);
			operationAgent.ProcessActions(this.CmdletOperation);
		}
	}
}