using System;

namespace Microsoft.Management.PowerShellWebAccess.Primitives
{
	public class PromptFieldDescription
	{
		public string HelpMessage
		{
			get;
			set;
		}

		public string Label
		{
			get;
			set;
		}

		public string Name
		{
			get;
			set;
		}

		public PromptFieldType PromptFieldType
		{
			get;
			set;
		}

		public bool PromptFieldTypeIsList
		{
			get;
			set;
		}

		public PromptFieldDescription()
		{
		}
	}
}