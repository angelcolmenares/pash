using System;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	public class ADDCCloningExcludedApplication
	{
		private const bool ignoreCase = true;

		private const string typeTag = "Type";

		private const string nameTag = "Name";

		public string Name;

		public string Type;

		public ADDCCloningExcludedApplication()
		{
		}

		public void Add(string attrName, string attrValue)
		{
			if (string.Compare(attrName, "Type", true) != 0)
			{
				if (string.Compare(attrName, "Name", true) == 0)
				{
					this.Name = attrValue;
				}
				return;
			}
			else
			{
				this.Type = attrValue;
				return;
			}
		}
	}
}