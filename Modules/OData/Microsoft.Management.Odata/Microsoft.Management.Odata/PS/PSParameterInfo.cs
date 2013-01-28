using System;

namespace Microsoft.Management.Odata.PS
{
	internal class PSParameterInfo
	{
		public bool IsMandatory
		{
			get;
			private set;
		}

		public bool IsSwitch
		{
			get;
			private set;
		}

		public string Type
		{
			get;
			private set;
		}

		public PSParameterInfo() : this(false, false, null)
		{
		}

		public PSParameterInfo(bool isSwitch, bool isMandatory, string type)
		{
			this.Type = type;
			this.IsSwitch = isSwitch;
			this.IsMandatory = isMandatory;
		}
	}
}