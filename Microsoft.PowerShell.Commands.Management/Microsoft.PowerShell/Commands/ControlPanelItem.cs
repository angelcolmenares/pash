using System;

namespace Microsoft.PowerShell.Commands
{
	public sealed class ControlPanelItem
	{
		private string _name;

		private string _canonicalName;

		private string[] _category;

		private string _description;

		private string _path;

		public string CanonicalName
		{
			get
			{
				return this._canonicalName;
			}
		}

		public string[] Category
		{
			get
			{
				return this._category;
			}
		}

		public string Description
		{
			get
			{
				return this._description;
			}
		}

		public string Name
		{
			get
			{
				return this._name;
			}
		}

		internal string Path
		{
			get
			{
				return this._path;
			}
		}

		internal ControlPanelItem(string name, string canonicalName, string[] category, string description, string path)
		{
			this._name = name;
			this._path = path;
			this._canonicalName = canonicalName;
			this._category = category;
			this._description = description;
		}

		public override string ToString()
		{
			return this.Name;
		}
	}
}