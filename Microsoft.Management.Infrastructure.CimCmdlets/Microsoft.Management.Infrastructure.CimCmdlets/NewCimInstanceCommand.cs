using Microsoft.Management.Infrastructure;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Management.Automation;

namespace Microsoft.Management.Infrastructure.CimCmdlets
{
	[Cmdlet("New", "CimInstance", DefaultParameterSetName="ClassNameComputerSet", SupportsShouldProcess=true, HelpUri="http://go.microsoft.com/fwlink/?LinkId=227963")]
	[OutputType(new Type[] { typeof(CimInstance) })]
	public class NewCimInstanceCommand : CimBaseCommand
	{
		internal const string nameClassName = "ClassName";

		internal const string nameResourceUri = "ResourceUri";

		internal const string nameKey = "Key";

		internal const string nameCimClass = "CimClass";

		internal const string nameProperty = "Property";

		internal const string nameNamespace = "Namespace";

		internal const string nameCimSession = "CimSession";

		internal const string nameComputerName = "ComputerName";

		internal const string nameClientOnly = "ClientOnly";

		private string className;

		private Uri resourceUri;

		private string[] key;

		private CimClass cimClass;

		private IDictionary property;

		private string nameSpace;

		private uint operationTimeout;

		private CimSession[] cimSession;

		private string[] computerName;

		private SwitchParameter clientOnly;

		private static Dictionary<string, HashSet<ParameterDefinitionEntry>> parameters;

		private static Dictionary<string, ParameterSetEntry> parameterSets;

		[Parameter(Mandatory=true, Position=0, ValueFromPipeline=true, ParameterSetName="CimClassComputerSet")]
		[Parameter(Mandatory=true, Position=0, ValueFromPipeline=true, ParameterSetName="CimClassSessionSet")]
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

		[Parameter(Mandatory=true, ValueFromPipeline=true, ParameterSetName="ClassNameSessionSet")]
		[Parameter(Mandatory=true, ValueFromPipeline=true, ParameterSetName="ResourceUriSessionSet")]
		[Parameter(Mandatory=true, ValueFromPipeline=true, ParameterSetName="CimClassSessionSet")]
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

		[Alias(new string[] { "Local" })]
		[Parameter(ParameterSetName="ClassNameComputerSet")]
		[Parameter(ParameterSetName="CimClassComputerSet")]
		[Parameter(ParameterSetName="CimClassSessionSet")]
		[Parameter(ParameterSetName="ClassNameSessionSet")]
		public SwitchParameter ClientOnly
		{
			get
			{
				return this.clientOnly;
			}
			set
			{
				this.clientOnly = value;
				base.SetParameter(value, "ClientOnly");
			}
		}

		[Alias(new string[] { "CN", "ServerName" })]
		[Parameter(ValueFromPipelineByPropertyName=true, ParameterSetName="ResourceUriComputerSet")]
		[Parameter(ValueFromPipelineByPropertyName=true, ParameterSetName="CimClassComputerSet")]
		[Parameter(ValueFromPipelineByPropertyName=true, ParameterSetName="ClassNameComputerSet")]
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
		[Parameter(ValueFromPipelineByPropertyName=true, ParameterSetName="ClassNameComputerSet")]
		[Parameter(ValueFromPipelineByPropertyName=true, ParameterSetName="ResourceUriSessionSet")]
		[Parameter(ValueFromPipelineByPropertyName=true, ParameterSetName="ClassNameSessionSet")]
		[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
		public string[] Key
		{
			get
			{
				return this.key;
			}
			set
			{
				this.key = value;
				base.SetParameter(value, "Key");
			}
		}

		[Parameter(ValueFromPipelineByPropertyName=true, ParameterSetName="ClassNameSessionSet")]
		[Parameter(ValueFromPipelineByPropertyName=true, ParameterSetName="ClassNameComputerSet")]
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

		[Alias(new string[] { "Arguments" })]
		[Parameter(Position=1, ValueFromPipelineByPropertyName=true)]
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
			}
		}

		[Parameter(Mandatory=true, ValueFromPipelineByPropertyName=true, ParameterSetName="ResourceUriSessionSet")]
		[Parameter(Mandatory=true, ValueFromPipelineByPropertyName=true, ParameterSetName="ResourceUriComputerSet")]
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

		static NewCimInstanceCommand()
		{
			Dictionary<string, HashSet<ParameterDefinitionEntry>> strs = new Dictionary<string, HashSet<ParameterDefinitionEntry>>();
			HashSet<ParameterDefinitionEntry> parameterDefinitionEntries = new HashSet<ParameterDefinitionEntry>();
			parameterDefinitionEntries.Add(new ParameterDefinitionEntry("ClassNameSessionSet", true));
			parameterDefinitionEntries.Add(new ParameterDefinitionEntry("ClassNameComputerSet", true));
			strs.Add("ClassName", parameterDefinitionEntries);
			HashSet<ParameterDefinitionEntry> parameterDefinitionEntries1 = new HashSet<ParameterDefinitionEntry>();
			parameterDefinitionEntries1.Add(new ParameterDefinitionEntry("ResourceUriSessionSet", true));
			parameterDefinitionEntries1.Add(new ParameterDefinitionEntry("ResourceUriComputerSet", true));
			strs.Add("ResourceUri", parameterDefinitionEntries1);
			HashSet<ParameterDefinitionEntry> parameterDefinitionEntries2 = new HashSet<ParameterDefinitionEntry>();
			parameterDefinitionEntries2.Add(new ParameterDefinitionEntry("ClassNameSessionSet", false));
			parameterDefinitionEntries2.Add(new ParameterDefinitionEntry("ClassNameComputerSet", false));
			parameterDefinitionEntries2.Add(new ParameterDefinitionEntry("ResourceUriComputerSet", false));
			parameterDefinitionEntries2.Add(new ParameterDefinitionEntry("ResourceUriSessionSet", false));
			strs.Add("Key", parameterDefinitionEntries2);
			HashSet<ParameterDefinitionEntry> parameterDefinitionEntries3 = new HashSet<ParameterDefinitionEntry>();
			parameterDefinitionEntries3.Add(new ParameterDefinitionEntry("CimClassSessionSet", true));
			parameterDefinitionEntries3.Add(new ParameterDefinitionEntry("CimClassComputerSet", true));
			strs.Add("CimClass", parameterDefinitionEntries3);
			HashSet<ParameterDefinitionEntry> parameterDefinitionEntries4 = new HashSet<ParameterDefinitionEntry>();
			parameterDefinitionEntries4.Add(new ParameterDefinitionEntry("ClassNameSessionSet", false));
			parameterDefinitionEntries4.Add(new ParameterDefinitionEntry("ClassNameComputerSet", false));
			parameterDefinitionEntries4.Add(new ParameterDefinitionEntry("ResourceUriComputerSet", false));
			parameterDefinitionEntries4.Add(new ParameterDefinitionEntry("ResourceUriSessionSet", false));
			strs.Add("Namespace", parameterDefinitionEntries4);
			HashSet<ParameterDefinitionEntry> parameterDefinitionEntries5 = new HashSet<ParameterDefinitionEntry>();
			parameterDefinitionEntries5.Add(new ParameterDefinitionEntry("ClassNameSessionSet", true));
			parameterDefinitionEntries5.Add(new ParameterDefinitionEntry("ResourceUriSessionSet", true));
			parameterDefinitionEntries5.Add(new ParameterDefinitionEntry("CimClassSessionSet", true));
			strs.Add("CimSession", parameterDefinitionEntries5);
			HashSet<ParameterDefinitionEntry> parameterDefinitionEntries6 = new HashSet<ParameterDefinitionEntry>();
			parameterDefinitionEntries6.Add(new ParameterDefinitionEntry("ClassNameComputerSet", false));
			parameterDefinitionEntries6.Add(new ParameterDefinitionEntry("ResourceUriComputerSet", false));
			parameterDefinitionEntries6.Add(new ParameterDefinitionEntry("CimClassComputerSet", false));
			strs.Add("ComputerName", parameterDefinitionEntries6);
			HashSet<ParameterDefinitionEntry> parameterDefinitionEntries7 = new HashSet<ParameterDefinitionEntry>();
			parameterDefinitionEntries7.Add(new ParameterDefinitionEntry("ClassNameSessionSet", true));
			parameterDefinitionEntries7.Add(new ParameterDefinitionEntry("ClassNameComputerSet", true));
			parameterDefinitionEntries7.Add(new ParameterDefinitionEntry("CimClassSessionSet", true));
			parameterDefinitionEntries7.Add(new ParameterDefinitionEntry("CimClassComputerSet", true));
			strs.Add("ClientOnly", parameterDefinitionEntries7);
			NewCimInstanceCommand.parameters = strs;
			Dictionary<string, ParameterSetEntry> strs1 = new Dictionary<string, ParameterSetEntry>();
			strs1.Add("ClassNameSessionSet", new ParameterSetEntry(2));
			strs1.Add("ClassNameComputerSet", new ParameterSetEntry(1, true));
			strs1.Add("CimClassSessionSet", new ParameterSetEntry(2));
			strs1.Add("CimClassComputerSet", new ParameterSetEntry(1));
			strs1.Add("ResourceUriSessionSet", new ParameterSetEntry(2));
			strs1.Add("ResourceUriComputerSet", new ParameterSetEntry(1));
			NewCimInstanceCommand.parameterSets = strs1;
		}

		public NewCimInstanceCommand() : base(NewCimInstanceCommand.parameters, NewCimInstanceCommand.parameterSets)
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
				}
				else
				{
					return;
				}
			}
		}

		private CimNewCimInstance CreateOperationAgent()
		{
			CimNewCimInstance cimNewCimInstance = new CimNewCimInstance();
			base.AsyncOperation = cimNewCimInstance;
			return cimNewCimInstance;
		}

		protected override void EndProcessing()
		{
			CimNewCimInstance operationAgent = this.GetOperationAgent();
			if (operationAgent != null)
			{
				operationAgent.ProcessRemainActions(this.CmdletOperation);
			}
		}

		private CimNewCimInstance GetOperationAgent()
		{
			return base.AsyncOperation as CimNewCimInstance;
		}

		protected override void ProcessRecord()
		{
			base.CheckParameterSet();
			this.CheckArgument();
			if (this.ClientOnly)
			{
				string str = null;
				if (this.ComputerName == null)
				{
					if (this.CimSession != null)
					{
						str = "CimSession";
					}
				}
				else
				{
					str = "ComputerName";
				}
				if (str != null)
				{
					base.ThrowConflictParameterWasSet("New-CimInstance", str, "ClientOnly");
					return;
				}
			}
			CimNewCimInstance operationAgent = this.GetOperationAgent();
			if (operationAgent == null)
			{
				operationAgent = this.CreateOperationAgent();
			}
			operationAgent.NewCimInstance(this);
			operationAgent.ProcessActions(this.CmdletOperation);
		}
	}
}