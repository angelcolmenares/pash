using Microsoft.Management.Infrastructure;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Management.Automation;

namespace Microsoft.Management.Infrastructure.CimCmdlets
{
	[Cmdlet("Invoke", "CimMethod", SupportsShouldProcess=true, DefaultParameterSetName="ClassNameComputerSet", HelpUri="http://go.microsoft.com/fwlink/?LinkId=227965")]
	public class InvokeCimMethodCommand : CimBaseCommand
	{
		internal const string nameClassName = "ClassName";

		internal const string nameCimClass = "CimClass";

		internal const string nameQuery = "Query";

		internal const string nameResourceUri = "ResourceUri";

		internal const string nameQueryDialect = "QueryDialect";

		internal const string nameCimInstance = "InputObject";

		internal const string nameComputerName = "ComputerName";

		internal const string nameCimSession = "CimSession";

		internal const string nameArguments = "Arguments";

		internal const string nameMethodName = "MethodName";

		internal const string nameNamespace = "Namespace";

		private string className;

		private Uri resourceUri;

		private CimClass cimClass;

		private string query;

		private string queryDialect;

		private CimInstance cimInstance;

		private string[] computerName;

		private CimSession[] cimSession;

		private IDictionary arguments;

		private string methodName;

		private string nameSpace;

		private uint operationTimeout;

		private static Dictionary<string, HashSet<ParameterDefinitionEntry>> parameters;

		private static Dictionary<string, ParameterSetEntry> parameterSets;

		[Parameter(Position=1, ValueFromPipelineByPropertyName=true)]
		[SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
		public IDictionary Arguments
		{
			get
			{
				return this.arguments;
			}
			set
			{
				this.arguments = value;
			}
		}

		[Parameter(Mandatory=true, Position=0, ValueFromPipeline=true, ParameterSetName="CimClassSessionSet")]
		[Parameter(Mandatory=true, Position=0, ValueFromPipeline=true, ParameterSetName="CimClassComputerSet")]
		public CimClass CimClass
		{
			get
			{
				return this.cimClass;
			}
			set
			{
				this.cimClass = value;
				base.SetParameter(value, "CimClass");
			}
		}

		internal CimInstance CimInstance
		{
			get
			{
				return this.cimInstance;
			}
		}

		[Parameter(Mandatory=true, ValueFromPipeline=true, ParameterSetName="CimClassSessionSet")]
		[Parameter(Mandatory=true, ValueFromPipeline=true, ParameterSetName="CimInstanceSessionSet")]
		[Parameter(Mandatory=true, ValueFromPipeline=true, ParameterSetName="ClassNameSessionSet")]
		[Parameter(Mandatory=true, ValueFromPipeline=true, ParameterSetName="QuerySessionSet")]
		[Parameter(Mandatory=true, ValueFromPipelineByPropertyName=true, ParameterSetName="ResourceUriSessionSet")]
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

		[Alias(new string[] { "Class" })]
		[Parameter(Mandatory=true, Position=0, ValueFromPipelineByPropertyName=true, ParameterSetName="ClassNameSessionSet")]
		[Parameter(Mandatory=true, Position=0, ValueFromPipelineByPropertyName=true, ParameterSetName="ClassNameComputerSet")]
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
		[Parameter(ValueFromPipelineByPropertyName=true, ParameterSetName="ResourceUriComputerSet")]
		[Parameter(ValueFromPipelineByPropertyName=true, ParameterSetName="CimClassComputerSet")]
		[Parameter(ParameterSetName="CimInstanceComputerSet")]
		[Parameter(ValueFromPipelineByPropertyName=true, ParameterSetName="ClassNameComputerSet")]
		[Parameter(ValueFromPipelineByPropertyName=true, ParameterSetName="QueryComputerSet")]
		[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
		public string[] ComputerName
		{
			get
			{
				return this.computerName;
			}
			set
			{
				DebugHelper.WriteLogEx();
				this.computerName = value;
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

		[Alias(new string[] { "Name" })]
		[Parameter(Mandatory=true, Position=2, ValueFromPipelineByPropertyName=true)]
		public string MethodName
		{
			get
			{
				return this.methodName;
			}
			set
			{
				this.methodName = value;
				base.SetParameter(value, "MethodName");
			}
		}

		[Parameter(ValueFromPipelineByPropertyName=true, ParameterSetName="ClassNameSessionSet")]
		[Parameter(ValueFromPipelineByPropertyName=true, ParameterSetName="QuerySessionSet")]
		[Parameter(ValueFromPipelineByPropertyName=true, ParameterSetName="ClassNameComputerSet")]
		[Parameter(ValueFromPipelineByPropertyName=true, ParameterSetName="QueryComputerSet")]
		[Parameter(ValueFromPipelineByPropertyName=true, ParameterSetName="ResourceUriComputerSet")]
		[Parameter(ValueFromPipelineByPropertyName=true, ParameterSetName="ResourceUriSessionSet")]
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

		[Parameter(ValueFromPipelineByPropertyName=true, ParameterSetName="QueryComputerSet")]
		[Parameter(ValueFromPipelineByPropertyName=true, ParameterSetName="QuerySessionSet")]
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

		[Parameter(ValueFromPipelineByPropertyName=true, ParameterSetName="CimInstanceSessionSet")]
		[Parameter(Mandatory=true, ValueFromPipelineByPropertyName=true, ParameterSetName="ResourceUriComputerSet")]
		[Parameter(ValueFromPipelineByPropertyName=true, ParameterSetName="CimInstanceComputerSet")]
		[Parameter(Mandatory=true, ValueFromPipelineByPropertyName=true, ParameterSetName="ResourceUriSessionSet")]
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

		static InvokeCimMethodCommand()
		{
			Dictionary<string, HashSet<ParameterDefinitionEntry>> strs = new Dictionary<string, HashSet<ParameterDefinitionEntry>>();
			HashSet<ParameterDefinitionEntry> parameterDefinitionEntries = new HashSet<ParameterDefinitionEntry>();
			parameterDefinitionEntries.Add(new ParameterDefinitionEntry("ClassNameComputerSet", true));
			parameterDefinitionEntries.Add(new ParameterDefinitionEntry("ClassNameSessionSet", true));
			strs.Add("ClassName", parameterDefinitionEntries);
			HashSet<ParameterDefinitionEntry> parameterDefinitionEntries1 = new HashSet<ParameterDefinitionEntry>();
			parameterDefinitionEntries1.Add(new ParameterDefinitionEntry("CimClassComputerSet", true));
			parameterDefinitionEntries1.Add(new ParameterDefinitionEntry("CimClassSessionSet", true));
			strs.Add("CimClass", parameterDefinitionEntries1);
			HashSet<ParameterDefinitionEntry> parameterDefinitionEntries2 = new HashSet<ParameterDefinitionEntry>();
			parameterDefinitionEntries2.Add(new ParameterDefinitionEntry("QueryComputerSet", true));
			parameterDefinitionEntries2.Add(new ParameterDefinitionEntry("QuerySessionSet", true));
			strs.Add("Query", parameterDefinitionEntries2);
			HashSet<ParameterDefinitionEntry> parameterDefinitionEntries3 = new HashSet<ParameterDefinitionEntry>();
			parameterDefinitionEntries3.Add(new ParameterDefinitionEntry("QueryComputerSet", false));
			parameterDefinitionEntries3.Add(new ParameterDefinitionEntry("QuerySessionSet", false));
			strs.Add("QueryDialect", parameterDefinitionEntries3);
			HashSet<ParameterDefinitionEntry> parameterDefinitionEntries4 = new HashSet<ParameterDefinitionEntry>();
			parameterDefinitionEntries4.Add(new ParameterDefinitionEntry("CimInstanceComputerSet", true));
			parameterDefinitionEntries4.Add(new ParameterDefinitionEntry("CimInstanceSessionSet", true));
			strs.Add("InputObject", parameterDefinitionEntries4);
			HashSet<ParameterDefinitionEntry> parameterDefinitionEntries5 = new HashSet<ParameterDefinitionEntry>();
			parameterDefinitionEntries5.Add(new ParameterDefinitionEntry("ClassNameComputerSet", false));
			parameterDefinitionEntries5.Add(new ParameterDefinitionEntry("QueryComputerSet", false));
			parameterDefinitionEntries5.Add(new ParameterDefinitionEntry("CimInstanceComputerSet", false));
			parameterDefinitionEntries5.Add(new ParameterDefinitionEntry("CimClassComputerSet", false));
			parameterDefinitionEntries5.Add(new ParameterDefinitionEntry("ResourceUriComputerSet", false));
			strs.Add("ComputerName", parameterDefinitionEntries5);
			HashSet<ParameterDefinitionEntry> parameterDefinitionEntries6 = new HashSet<ParameterDefinitionEntry>();
			parameterDefinitionEntries6.Add(new ParameterDefinitionEntry("CimInstanceSessionSet", true));
			parameterDefinitionEntries6.Add(new ParameterDefinitionEntry("QuerySessionSet", true));
			parameterDefinitionEntries6.Add(new ParameterDefinitionEntry("ClassNameSessionSet", true));
			parameterDefinitionEntries6.Add(new ParameterDefinitionEntry("CimClassSessionSet", true));
			parameterDefinitionEntries6.Add(new ParameterDefinitionEntry("ResourceUriSessionSet", true));
			strs.Add("CimSession", parameterDefinitionEntries6);
			HashSet<ParameterDefinitionEntry> parameterDefinitionEntries7 = new HashSet<ParameterDefinitionEntry>();
			parameterDefinitionEntries7.Add(new ParameterDefinitionEntry("ClassNameComputerSet", true));
			parameterDefinitionEntries7.Add(new ParameterDefinitionEntry("ClassNameSessionSet", true));
			parameterDefinitionEntries7.Add(new ParameterDefinitionEntry("QueryComputerSet", true));
			parameterDefinitionEntries7.Add(new ParameterDefinitionEntry("QuerySessionSet", true));
			parameterDefinitionEntries7.Add(new ParameterDefinitionEntry("CimInstanceComputerSet", true));
			parameterDefinitionEntries7.Add(new ParameterDefinitionEntry("CimInstanceSessionSet", true));
			parameterDefinitionEntries7.Add(new ParameterDefinitionEntry("CimClassComputerSet", true));
			parameterDefinitionEntries7.Add(new ParameterDefinitionEntry("CimClassSessionSet", true));
			parameterDefinitionEntries7.Add(new ParameterDefinitionEntry("ResourceUriSessionSet", true));
			parameterDefinitionEntries7.Add(new ParameterDefinitionEntry("ResourceUriComputerSet", true));
			strs.Add("MethodName", parameterDefinitionEntries7);
			HashSet<ParameterDefinitionEntry> parameterDefinitionEntries8 = new HashSet<ParameterDefinitionEntry>();
			parameterDefinitionEntries8.Add(new ParameterDefinitionEntry("ClassNameComputerSet", false));
			parameterDefinitionEntries8.Add(new ParameterDefinitionEntry("ClassNameSessionSet", false));
			parameterDefinitionEntries8.Add(new ParameterDefinitionEntry("QueryComputerSet", false));
			parameterDefinitionEntries8.Add(new ParameterDefinitionEntry("QuerySessionSet", false));
			parameterDefinitionEntries8.Add(new ParameterDefinitionEntry("ResourceUriSessionSet", false));
			parameterDefinitionEntries8.Add(new ParameterDefinitionEntry("ResourceUriComputerSet", false));
			strs.Add("Namespace", parameterDefinitionEntries8);
			HashSet<ParameterDefinitionEntry> parameterDefinitionEntries9 = new HashSet<ParameterDefinitionEntry>();
			parameterDefinitionEntries9.Add(new ParameterDefinitionEntry("CimInstanceComputerSet", false));
			parameterDefinitionEntries9.Add(new ParameterDefinitionEntry("CimInstanceSessionSet", false));
			parameterDefinitionEntries9.Add(new ParameterDefinitionEntry("ResourceUriSessionSet", true));
			parameterDefinitionEntries9.Add(new ParameterDefinitionEntry("ResourceUriComputerSet", true));
			strs.Add("ResourceUri", parameterDefinitionEntries9);
			InvokeCimMethodCommand.parameters = strs;
			Dictionary<string, ParameterSetEntry> strs1 = new Dictionary<string, ParameterSetEntry>();
			strs1.Add("ClassNameComputerSet", new ParameterSetEntry(2, true));
			strs1.Add("ResourceUriSessionSet", new ParameterSetEntry(3));
			strs1.Add("ResourceUriComputerSet", new ParameterSetEntry(2));
			strs1.Add("ClassNameSessionSet", new ParameterSetEntry(3));
			strs1.Add("QueryComputerSet", new ParameterSetEntry(2));
			strs1.Add("QuerySessionSet", new ParameterSetEntry(3));
			strs1.Add("CimInstanceComputerSet", new ParameterSetEntry(2));
			strs1.Add("CimInstanceSessionSet", new ParameterSetEntry(3));
			strs1.Add("CimClassComputerSet", new ParameterSetEntry(2));
			strs1.Add("CimClassSessionSet", new ParameterSetEntry(3));
			InvokeCimMethodCommand.parameterSets = strs1;
		}

		public InvokeCimMethodCommand() : base(InvokeCimMethodCommand.parameters, InvokeCimMethodCommand.parameterSets)
		{
			DebugHelper.WriteLogEx();
		}

		protected override void BeginProcessing()
		{
			CimInvokeCimMethod operationAgent = this.GetOperationAgent();
			if (operationAgent == null)
			{
				operationAgent = this.CreateOperationAgent();
			}
			this.CmdletOperation = new CmdletOperationInvokeCimMethod(this, operationAgent);
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
				}
				else
				{
					return;
				}
			}
		}

		private CimInvokeCimMethod CreateOperationAgent()
		{
			CimInvokeCimMethod cimInvokeCimMethod = new CimInvokeCimMethod();
			base.AsyncOperation = cimInvokeCimMethod;
			return cimInvokeCimMethod;
		}

		protected override void EndProcessing()
		{
			CimInvokeCimMethod operationAgent = this.GetOperationAgent();
			if (operationAgent != null)
			{
				operationAgent.ProcessRemainActions(this.CmdletOperation);
			}
		}

		private CimInvokeCimMethod GetOperationAgent()
		{
			return base.AsyncOperation as CimInvokeCimMethod;
		}

		protected override void ProcessRecord()
		{
			base.CheckParameterSet();
			this.CheckArgument();
			CimInvokeCimMethod operationAgent = this.GetOperationAgent();
			operationAgent.InvokeCimMethod(this);
			operationAgent.ProcessActions(this.CmdletOperation);
		}
	}
}