using System;
using System.Runtime;

namespace System.DirectoryServices.Protocols
{
	public class SortKey
	{
		private string name;

		private string rule;

		private bool order;

		public string AttributeName
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.name;
			}
			set
			{
				if (value != null)
				{
					this.name = value;
					return;
				}
				else
				{
					throw new ArgumentNullException("value");
				}
			}
		}

		public string MatchingRule
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.rule;
			}
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			set
			{
				this.rule = value;
			}
		}

		public bool ReverseOrder
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.order;
			}
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			set
			{
				this.order = value;
			}
		}

		public SortKey()
		{
			Utility.CheckOSVersion();
		}

		public SortKey(string attributeName, string matchingRule, bool reverseOrder)
		{
			Utility.CheckOSVersion();
			this.AttributeName = attributeName;
			this.rule = matchingRule;
			this.order = reverseOrder;
		}
	}
}