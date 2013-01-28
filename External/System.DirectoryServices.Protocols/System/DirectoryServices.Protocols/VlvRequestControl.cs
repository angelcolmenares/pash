using System;
using System.Collections;
using System.Runtime;
using System.Text;

namespace System.DirectoryServices.Protocols
{
	public class VlvRequestControl : DirectoryControl
	{
		private int before;

		private int after;

		private int offset;

		private int estimateCount;

		private byte[] target;

		private byte[] context;

		public int AfterCount
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.after;
			}
			set
			{
				if (value >= 0)
				{
					this.after = value;
					return;
				}
				else
				{
					throw new ArgumentException(Res.GetString("ValidValue"), "value");
				}
			}
		}

		public int BeforeCount
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.before;
			}
			set
			{
				if (value >= 0)
				{
					this.before = value;
					return;
				}
				else
				{
					throw new ArgumentException(Res.GetString("ValidValue"), "value");
				}
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
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			set
			{
				this.context = value;
			}
		}

		public int EstimateCount
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.estimateCount;
			}
			set
			{
				if (value >= 0)
				{
					this.estimateCount = value;
					return;
				}
				else
				{
					throw new ArgumentException(Res.GetString("ValidValue"), "value");
				}
			}
		}

		public int Offset
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.offset;
			}
			set
			{
				if (value >= 0)
				{
					this.offset = value;
					return;
				}
				else
				{
					throw new ArgumentException(Res.GetString("ValidValue"), "value");
				}
			}
		}

		public byte[] Target
		{
			get
			{
				if (this.target != null)
				{
					byte[] numArray = new byte[(int)this.target.Length];
					for (int i = 0; i < (int)numArray.Length; i++)
					{
						numArray[i] = this.target[i];
					}
					return numArray;
				}
				else
				{
					return new byte[0];
				}
			}
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			set
			{
				this.target = value;
			}
		}

		public VlvRequestControl() : base("2.16.840.1.113730.3.4.9", null, true, true)
		{
		}

		public VlvRequestControl(int beforeCount, int afterCount, int offset) : this()
		{
			this.BeforeCount = beforeCount;
			this.AfterCount = afterCount;
			this.Offset = offset;
		}

		public VlvRequestControl(int beforeCount, int afterCount, string target) : this()
		{
			this.BeforeCount = beforeCount;
			this.AfterCount = afterCount;
			if (target != null)
			{
				UTF8Encoding uTF8Encoding = new UTF8Encoding();
				byte[] bytes = uTF8Encoding.GetBytes(target);
				this.target = bytes;
			}
		}

		public VlvRequestControl(int beforeCount, int afterCount, byte[] target) : this()
		{
			this.BeforeCount = beforeCount;
			this.AfterCount = afterCount;
			this.Target = target;
		}

		public override byte[] GetValue()
		{
			StringBuilder stringBuilder = new StringBuilder(10);
			ArrayList arrayLists = new ArrayList();
			stringBuilder.Append("{ii");
			arrayLists.Add(this.BeforeCount);
			arrayLists.Add(this.AfterCount);
			if ((int)this.Target.Length == 0)
			{
				stringBuilder.Append("t{");
				arrayLists.Add(160);
				stringBuilder.Append("ii");
				arrayLists.Add(this.Offset);
				arrayLists.Add(this.EstimateCount);
				stringBuilder.Append("}");
			}
			else
			{
				stringBuilder.Append("t");
				arrayLists.Add(129);
				stringBuilder.Append("o");
				arrayLists.Add(this.Target);
			}
			if ((int)this.ContextId.Length != 0)
			{
				stringBuilder.Append("o");
				arrayLists.Add(this.ContextId);
			}
			stringBuilder.Append("}");
			object[] item = new object[arrayLists.Count];
			for (int i = 0; i < arrayLists.Count; i++)
			{
				item[i] = arrayLists[i];
			}
			this.directoryControlValue = BerConverter.Encode(stringBuilder.ToString(), item);
			return base.GetValue();
		}
	}
}