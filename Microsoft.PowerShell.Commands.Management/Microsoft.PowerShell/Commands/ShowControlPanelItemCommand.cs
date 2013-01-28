using Shell32;
using System;
using System.Collections.Generic;
using System.Management.Automation;
using System.Reflection;

namespace Microsoft.PowerShell.Commands
{
	[Cmdlet("Show", "ControlPanelItem", DefaultParameterSetName="RegularName", HelpUri="http://go.microsoft.com/fwlink/?LinkID=219983")]
	public sealed class ShowControlPanelItemCommand : ControlPanelItemBaseCommand
	{
		private const string RegularNameParameterSet = "RegularName";

		private const string CanonicalNameParameterSet = "CanonicalName";

		private const string ControlPanelItemParameterSet = "ControlPanelItem";

		[AllowNull]
		[Parameter(Mandatory=true, ParameterSetName="CanonicalName")]
		public string[] CanonicalName
		{
			get
			{
				return this.CanonicalNames;
			}
			set
			{
				this.CanonicalNames = value;
			}
		}

		[Parameter(Position=0, ParameterSetName="ControlPanelItem", ValueFromPipeline=true)]
		[ValidateNotNullOrEmpty]
		public ControlPanelItem[] InputObject
		{
			get
			{
				return this.ControlPanelItems;
			}
			set
			{
				this.ControlPanelItems = value;
			}
		}

		[Parameter(Position=0, Mandatory=true, ParameterSetName="RegularName", ValueFromPipeline=true, ValueFromPipelineByPropertyName=true)]
		[ValidateNotNullOrEmpty]
		public string[] Name
		{
			get
			{
				return this.RegularNames;
			}
			set
			{
				this.RegularNames = value;
			}
		}

		public ShowControlPanelItemCommand()
		{
		}

		protected override void ProcessRecord()
		{
			List<ShellFolderItem> controlPanelItemsByInstance;
			if (base.ParameterSetName != "RegularName")
			{
				if (base.ParameterSetName != "CanonicalName")
				{
					controlPanelItemsByInstance = base.GetControlPanelItemsByInstance(base.AllControlPanelItems);
				}
				else
				{
					controlPanelItemsByInstance = base.GetControlPanelItemByCanonicalName(base.AllControlPanelItems, false);
				}
			}
			else
			{
				controlPanelItemsByInstance = base.GetControlPanelItemByName(base.AllControlPanelItems, false);
			}
			foreach (ShellFolderItem variable in controlPanelItemsByInstance)
			{
				variable.InvokeVerb(Missing.Value);
			}
		}
	}
}