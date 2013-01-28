using Microsoft.Management.Infrastructure;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Management.Automation;

namespace Microsoft.Management.Infrastructure.CimCmdlets
{
	[Cmdlet("Get", "CimSession", DefaultParameterSetName="ComputerNameSet", HelpUri="http://go.microsoft.com/fwlink/?LinkId=227966")]
	[OutputType(new Type[] { typeof(CimSession) })]
	public sealed class GetCimSessionCommand : CimBaseCommand
	{
		internal const string nameComputerName = "ComputerName";

		internal const string nameId = "Id";

		internal const string nameInstanceId = "InstanceId";

		internal const string nameName = "Name";

		private string[] computername;

		private uint[] id;

		private Guid[] instanceid;

		private string[] name;

		private CimGetSession cimGetSession;

		private static Dictionary<string, HashSet<ParameterDefinitionEntry>> parameters;

		private static Dictionary<string, ParameterSetEntry> parameterSets;

		[Alias(new string[] { "CN", "ServerName" })]
		[Parameter(Position=0, ValueFromPipelineByPropertyName=true, ParameterSetName="ComputerNameSet")]
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

		static GetCimSessionCommand()
		{
			Dictionary<string, HashSet<ParameterDefinitionEntry>> strs = new Dictionary<string, HashSet<ParameterDefinitionEntry>>();
			HashSet<ParameterDefinitionEntry> parameterDefinitionEntries = new HashSet<ParameterDefinitionEntry>();
			parameterDefinitionEntries.Add(new ParameterDefinitionEntry("ComputerNameSet", false));
			strs.Add("ComputerName", parameterDefinitionEntries);
			HashSet<ParameterDefinitionEntry> parameterDefinitionEntries1 = new HashSet<ParameterDefinitionEntry>();
			parameterDefinitionEntries1.Add(new ParameterDefinitionEntry("SessionIdSet", true));
			strs.Add("Id", parameterDefinitionEntries1);
			HashSet<ParameterDefinitionEntry> parameterDefinitionEntries2 = new HashSet<ParameterDefinitionEntry>();
			parameterDefinitionEntries2.Add(new ParameterDefinitionEntry("InstanceIdSet", true));
			strs.Add("InstanceId", parameterDefinitionEntries2);
			HashSet<ParameterDefinitionEntry> parameterDefinitionEntries3 = new HashSet<ParameterDefinitionEntry>();
			parameterDefinitionEntries3.Add(new ParameterDefinitionEntry("NameSet", true));
			strs.Add("Name", parameterDefinitionEntries3);
			GetCimSessionCommand.parameters = strs;
			Dictionary<string, ParameterSetEntry> strs1 = new Dictionary<string, ParameterSetEntry>();
			strs1.Add("ComputerNameSet", new ParameterSetEntry(0, true));
			strs1.Add("SessionIdSet", new ParameterSetEntry(1));
			strs1.Add("InstanceIdSet", new ParameterSetEntry(1));
			strs1.Add("NameSet", new ParameterSetEntry(1));
			GetCimSessionCommand.parameterSets = strs1;
		}

		public GetCimSessionCommand() : base(GetCimSessionCommand.parameters, GetCimSessionCommand.parameterSets)
		{
			DebugHelper.WriteLogEx();
		}

		protected override void BeginProcessing()
		{
			this.cimGetSession = new CimGetSession();
			base.AtBeginProcess = false;
		}

		protected override void ProcessRecord()
		{
			base.CheckParameterSet();
			this.cimGetSession.GetCimSession(this);
		}
	}
}