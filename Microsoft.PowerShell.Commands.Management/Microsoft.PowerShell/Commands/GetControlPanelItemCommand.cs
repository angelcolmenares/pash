using Shell32;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Management.Automation;

namespace Microsoft.PowerShell.Commands
{
	[Cmdlet("Get", "ControlPanelItem", DefaultParameterSetName="RegularName", HelpUri="http://go.microsoft.com/fwlink/?LinkID=219982")]
	[OutputType(new Type[] { typeof(ControlPanelItem) })]
	public sealed class GetControlPanelItemCommand : ControlPanelItemBaseCommand
	{
		private const string RegularNameParameterSet = "RegularName";

		private const string CanonicalNameParameterSet = "CanonicalName";

		private bool _nameSpecified;

		private bool _canonicalNameSpecified;

		private bool _categorySpecified;

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
				this._canonicalNameSpecified = true;
			}
		}

		[Parameter]
		[ValidateNotNullOrEmpty]
		public string[] Category
		{
			get
			{
				return this.CategoryNames;
			}
			set
			{
				this.CategoryNames = value;
				this._categorySpecified = true;
			}
		}

		[Parameter(Position=0, ParameterSetName="RegularName", ValueFromPipeline=true, ValueFromPipelineByPropertyName=true)]
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
				this._nameSpecified = true;
			}
		}

		public GetControlPanelItemCommand()
		{
		}

		private static int CompareControlPanelItems(ControlPanelItem x, ControlPanelItem y)
		{
			if (x.CanonicalName != null || y.CanonicalName != null)
			{
				if (x.CanonicalName != null)
				{
					if (y.CanonicalName != null)
					{
						return string.Compare(x.CanonicalName, y.CanonicalName, StringComparison.OrdinalIgnoreCase);
					}
					else
					{
						return -1;
					}
				}
				else
				{
					return 1;
				}
			}
			else
			{
				return 0;
			}
		}

		protected override void ProcessRecord()
		{
			string str;
			base.GetCategoryMap();
			List<ShellFolderItem> controlPanelItemByCategory = base.GetControlPanelItemByCategory(base.AllControlPanelItems);
			if (!this._nameSpecified)
			{
				if (this._canonicalNameSpecified)
				{
					controlPanelItemByCategory = base.GetControlPanelItemByCanonicalName(controlPanelItemByCategory, this._categorySpecified);
				}
			}
			else
			{
				controlPanelItemByCategory = base.GetControlPanelItemByName(controlPanelItemByCategory, this._categorySpecified);
			}
			List<ControlPanelItem> controlPanelItems = new List<ControlPanelItem>();
			foreach (ShellFolderItem variable in controlPanelItemByCategory)
			{
				string name = variable.Name;
				string path = variable.Path;
				string str1 = (string)((dynamic)variable.ExtendedProperty("InfoTip"));
				string str2 = (string)((dynamic)variable.ExtendedProperty("System.ApplicationName"));
				if (str2 != null)
				{
					str = str2.Substring(0, str2.IndexOf("\0", StringComparison.OrdinalIgnoreCase));
				}
				else
				{
					str = null;
				}
				str2 = str;
				int[] numArray = (int[])((dynamic)variable.ExtendedProperty("System.ControlPanel.Category"));
				string[] item = new string[(int)numArray.Length];
				for (int i = 0; i < (int)numArray.Length; i++)
				{
					string str3 = (string)LanguagePrimitives.ConvertTo(numArray[i], typeof(string), CultureInfo.InvariantCulture);
					item[i] = this.CategoryMap[str3];
				}
				ControlPanelItem controlPanelItem = new ControlPanelItem(name, str2, item, str1, path);
				controlPanelItems.Add(controlPanelItem);
			}
			controlPanelItems.Sort(new Comparison<ControlPanelItem>(GetControlPanelItemCommand.CompareControlPanelItems));
			foreach (ControlPanelItem controlPanelItem1 in controlPanelItems)
			{
				base.WriteObject(controlPanelItem1);
			}
		}
	}
}