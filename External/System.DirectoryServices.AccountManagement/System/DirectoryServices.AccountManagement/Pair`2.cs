using System.Runtime;

namespace System.DirectoryServices.AccountManagement
{
	internal class Pair<J, K>
	{
		private J left;

		private K right;

		internal J Left
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.left;
			}
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			set
			{
				this.left = value;
			}
		}

		internal K Right
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.right;
			}
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			set
			{
				this.right = value;
			}
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		internal Pair(J left, K right)
		{
			this.left = left;
			this.right = right;
		}
	}
}