using Microsoft.Management.Infrastructure;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Management.Automation;

namespace Microsoft.Management.Infrastructure.CimCmdlets
{
	[Cmdlet("Get", "CimClass", DefaultParameterSetName="ComputerSet", HelpUri="http://go.microsoft.com/fwlink/?LinkId=227959")]
	[OutputType(new Type[] { typeof(CimClass) })]
	public class GetCimClassCommand : CimBaseCommand
	{
		internal const string Noun = "CimClass";

		internal const string nameCimSession = "CimSession";

		internal const string nameComputerName = "ComputerName";

		private string className;

		private string nameSpace;

		private uint operationTimeout;

		private CimSession[] cimSession;

		private string[] computerName;

		private string methodName;

		private string propertyName;

		private string qualifierName;

		private static Dictionary<string, HashSet<ParameterDefinitionEntry>> parameters;

		private static Dictionary<string, ParameterSetEntry> parameterSets;

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

		[Parameter(Position=0, ValueFromPipelineByPropertyName=true)]
		public string ClassName
		{
			get
			{
				return this.className;
			}
			set
			{
				this.className = value;
			}
		}

		[Alias(new string[] { "CN", "ServerName" })]
		[Parameter(ValueFromPipelineByPropertyName=true, ParameterSetName="ComputerSet")]
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

		[Parameter(ValueFromPipelineByPropertyName=true)]
		public string MethodName
		{
			get
			{
				return this.methodName;
			}
			set
			{
				this.methodName = value;
			}
		}

		[Parameter(Position=1, ValueFromPipelineByPropertyName=true)]
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

		[Parameter(ValueFromPipelineByPropertyName=true)]
		public string PropertyName
		{
			get
			{
				return this.propertyName;
			}
			set
			{
				this.propertyName = value;
			}
		}

		[Parameter(ValueFromPipelineByPropertyName=true)]
		public string QualifierName
		{
			get
			{
				return this.qualifierName;
			}
			set
			{
				this.qualifierName = value;
			}
		}

		static GetCimClassCommand()
		{
			Dictionary<string, HashSet<ParameterDefinitionEntry>> strs = new Dictionary<string, HashSet<ParameterDefinitionEntry>>();
			HashSet<ParameterDefinitionEntry> parameterDefinitionEntries = new HashSet<ParameterDefinitionEntry>();
			parameterDefinitionEntries.Add(new ParameterDefinitionEntry("SessionSet", true));
			strs.Add("CimSession", parameterDefinitionEntries);
			HashSet<ParameterDefinitionEntry> parameterDefinitionEntries1 = new HashSet<ParameterDefinitionEntry>();
			parameterDefinitionEntries1.Add(new ParameterDefinitionEntry("ComputerSet", false));
			strs.Add("ComputerName", parameterDefinitionEntries1);
			GetCimClassCommand.parameters = strs;
			Dictionary<string, ParameterSetEntry> strs1 = new Dictionary<string, ParameterSetEntry>();
			strs1.Add("SessionSet", new ParameterSetEntry(1));
			strs1.Add("ComputerSet", new ParameterSetEntry(0, true));
			GetCimClassCommand.parameterSets = strs1;
		}

		public GetCimClassCommand() : base(GetCimClassCommand.parameters, GetCimClassCommand.parameterSets)
		{
			DebugHelper.WriteLogEx();
		}

		protected override void BeginProcessing()
		{
			this.CmdletOperation = new CmdletOperationBase(this);
			base.AtBeginProcess = false;
		}

		private CimGetCimClass CreateOperationAgent()
		{
			CimGetCimClass cimGetCimClass = new CimGetCimClass();
			base.AsyncOperation = cimGetCimClass;
			return cimGetCimClass;
		}

		protected override void EndProcessing()
		{
			CimGetCimClass operationAgent = this.GetOperationAgent();
			if (operationAgent != null)
			{
				operationAgent.ProcessRemainActions(this.CmdletOperation);
			}
		}

		private CimGetCimClass GetOperationAgent()
		{
			return base.AsyncOperation as CimGetCimClass;
		}

		protected override void ProcessRecord()
		{
			base.CheckParameterSet();
			CimGetCimClass operationAgent = this.GetOperationAgent();
			if (operationAgent == null)
			{
				operationAgent = this.CreateOperationAgent();
			}
			operationAgent.GetCimClass(this);
			operationAgent.ProcessActions(this.CmdletOperation);
		}
	}
}