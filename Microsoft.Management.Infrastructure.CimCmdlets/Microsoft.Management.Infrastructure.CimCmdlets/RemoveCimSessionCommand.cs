using Microsoft.Management.Infrastructure;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Management.Automation;

namespace Microsoft.Management.Infrastructure.CimCmdlets
{
	[Cmdlet("Remove", "CimSession", SupportsShouldProcess=true, DefaultParameterSetName="CimSessionSet", HelpUri="http://go.microsoft.com/fwlink/?LinkId=227968")]
	public sealed class RemoveCimSessionCommand : CimBaseCommand
	{
		internal const string nameCimSession = "CimSession";

		internal const string nameComputerName = "ComputerName";

		internal const string nameId = "Id";

		internal const string nameInstanceId = "InstanceId";

		internal const string nameName = "Name";

		private CimSession[] cimsession;

		private string[] computername;

		private uint[] id;

		private Guid[] instanceid;

		private string[] name;

		private CimRemoveSession cimRemoveSession;

		private static Dictionary<string, HashSet<ParameterDefinitionEntry>> parameters;

		private static Dictionary<string, ParameterSetEntry> parameterSets;

		[Parameter(Mandatory=true, Position=0, ValueFromPipeline=true, ValueFromPipelineByPropertyName=true, ParameterSetName="CimSessionSet")]
		[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
		public CimSession[] CimSession
		{
			get
			{
				return this.cimsession;
			}
			set
			{
				this.cimsession = value;
				base.SetParameter(value, "CimSession");
			}
		}

		[Alias(new string[] { "CN", "ServerName" })]
		[Parameter(Mandatory=true, Position=0, ValueFromPipelineByPropertyName=true, ParameterSetName="ComputerNameSet")]
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

		[Parameter(Mandatory=true, Position=0, ValueFromPipelineByPropertyName=true, ParameterSetName="SessionIdSet")]
		[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
		public uint[] Id
		{
			get
			{
				return this.id;
			}
			set
			{
				this.id = value;
				base.SetParameter(value, "Id");
			}
		}

		[Parameter(Mandatory=true, ValueFromPipelineByPropertyName=true, ParameterSetName="InstanceIdSet")]
		[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
		public Guid[] InstanceId
		{
			get
			{
				return this.instanceid;
			}
			set
			{
				this.instanceid = value;
				base.SetParameter(value, "InstanceId");
			}
		}

		[Parameter(Mandatory=true, ValueFromPipelineByPropertyName=true, ParameterSetName="NameSet")]
		[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
		public string[] Name
		{
			get
			{
				return this.name;
			}
			set
			{
				this.name = value;
				base.SetParameter(value, "Name");
			}
		}

		static RemoveCimSessionCommand()
		{
			Dictionary<string, HashSet<ParameterDefinitionEntry>> strs = new Dictionary<string, HashSet<ParameterDefinitionEntry>>();
			HashSet<ParameterDefinitionEntry> parameterDefinitionEntries = new HashSet<ParameterDefinitionEntry>();
			parameterDefinitionEntries.Add(new ParameterDefinitionEntry("CimSessionSet", true));
			strs.Add("CimSession", parameterDefinitionEntries);
			HashSet<ParameterDefinitionEntry> parameterDefinitionEntries1 = new HashSet<ParameterDefinitionEntry>();
			parameterDefinitionEntries1.Add(new ParameterDefinitionEntry("ComputerNameSet", true));
			strs.Add("ComputerName", parameterDefinitionEntries1);
			HashSet<ParameterDefinitionEntry> parameterDefinitionEntries2 = new HashSet<ParameterDefinitionEntry>();
			parameterDefinitionEntries2.Add(new ParameterDefinitionEntry("SessionIdSet", true));
			strs.Add("Id", parameterDefinitionEntries2);
			HashSet<ParameterDefinitionEntry> parameterDefinitionEntries3 = new HashSet<ParameterDefinitionEntry>();
			parameterDefinitionEntries3.Add(new ParameterDefinitionEntry("InstanceIdSet", true));
			strs.Add("InstanceId", parameterDefinitionEntries3);
			HashSet<ParameterDefinitionEntry> parameterDefinitionEntries4 = new HashSet<ParameterDefinitionEntry>();
			parameterDefinitionEntries4.Add(new ParameterDefinitionEntry("NameSet", true));
			strs.Add("Name", parameterDefinitionEntries4);
			RemoveCimSessionCommand.parameters = strs;
			Dictionary<string, ParameterSetEntry> strs1 = new Dictionary<string, ParameterSetEntry>();
			strs1.Add("CimSessionSet", new ParameterSetEntry(1, true));
			strs1.Add("ComputerNameSet", new ParameterSetEntry(1));
			strs1.Add("SessionIdSet", new ParameterSetEntry(1));
			strs1.Add("InstanceIdSet", new ParameterSetEntry(1));
			strs1.Add("NameSet", new ParameterSetEntry(1));
			RemoveCimSessionCommand.parameterSets = strs1;
		}

		public RemoveCimSessionCommand() : base(RemoveCimSessionCommand.parameters, RemoveCimSessionCommand.parameterSets)
		{
		}

		protected override void BeginProcessing()
		{
			this.cimRemoveSession = new CimRemoveSession();
			base.AtBeginProcess = false;
		}

		protected override void ProcessRecord()
		{
			base.CheckParameterSet();
			this.cimRemoveSession.RemoveCimSession(this);
		}
	}
}