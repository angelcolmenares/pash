using System;
using System.Runtime;

namespace System.DirectoryServices.Protocols
{
	public class VlvResponseControl : DirectoryControl
	{
		private int position;

		private int count;

		private byte[] context;

		private ResultCode result;

		public int ContentCount
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.count;
			}
		}

		public byte[] ContextId
		{
			get
			{
				if (this.context != null)
				{
					byte[] numArray = new byte[(int)this.context.Length];
					for (int i = 0; i < (int)numArray.Length; i++)
					{
						numArray[i] = this.context[i];
					}
					return numArray;
				}
				else
				{
					return new byte[0];
				}
			}
		}

		public ResultCode Result
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.result;
			}
		}

		public int TargetPosition
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.position;
			}
		}

		internal VlvResponseControl(int targetPosition, int count, byte[] context, ResultCode result, bool criticality, byte[] value) : base("2.16.840.1.113730.3.4.10", value, criticality, true)
		{
			this.position = targetPosition;
			this.count = count;
			this.context = context;
			this.result = result;
		}
	}
}