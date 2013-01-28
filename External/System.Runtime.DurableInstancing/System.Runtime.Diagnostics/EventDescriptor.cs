using System;
using System.Runtime;
using System.Runtime.InteropServices;
using System.Security.Permissions;

namespace System.Runtime.Diagnostics
{
	[HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort=true)]
	[StructLayout(LayoutKind.Explicit)]
	internal struct EventDescriptor
	{
		[FieldOffset(0)]
		private ushort m_id;

		[FieldOffset(2)]
		private byte m_version;

		[FieldOffset(3)]
		private byte m_channel;

		[FieldOffset(4)]
		private byte m_level;

		[FieldOffset(5)]
		private byte m_opcode;

		[FieldOffset(6)]
		private ushort m_task;

		[FieldOffset(8)]
		private long m_keywords;

		public byte Channel
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.m_channel;
			}
		}

		public int EventId
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.m_id;
			}
		}

		public long Keywords
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.m_keywords;
			}
		}

		public byte Level
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.m_level;
			}
		}

		public byte Opcode
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.m_opcode;
			}
		}

		public int Task
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.m_task;
			}
		}

		public byte Version
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.m_version;
			}
		}

		public EventDescriptor(int id, byte version, byte channel, byte level, byte opcode, int task, long keywords)
		{
			if (id >= 0)
			{
				if (id <= 0xffff)
				{
					this.m_id = (ushort)id;
					this.m_version = version;
					this.m_channel = channel;
					this.m_level = level;
					this.m_opcode = opcode;
					this.m_keywords = keywords;
					if (task >= 0)
					{
						if (task <= 0xffff)
						{
							this.m_task = (ushort)task;
							return;
						}
						else
						{
							throw Fx.Exception.ArgumentOutOfRange("task", task, string.Empty);
						}
					}
					else
					{
						throw Fx.Exception.ArgumentOutOfRange("task", task, InternalSR.ValueMustBeNonNegative);
					}
				}
				else
				{
					throw Fx.Exception.ArgumentOutOfRange("id", id, string.Empty);
				}
			}
			else
			{
				throw Fx.Exception.ArgumentOutOfRange("id", id, InternalSR.ValueMustBeNonNegative);
			}
		}

		public override bool Equals(object obj)
		{
			if (obj as EventDescriptor != null)
			{
				return this.Equals((EventDescriptor)obj);
			}
			else
			{
				return false;
			}
		}

		public bool Equals(EventDescriptor other)
		{
			if (this.m_id != other.m_id || this.m_version != other.m_version || this.m_channel != other.m_channel || this.m_level != other.m_level || this.m_opcode != other.m_opcode || this.m_task != other.m_task || this.m_keywords != other.m_keywords)
			{
				return false;
			}
			else
			{
				return true;
			}
		}

		public override int GetHashCode()
		{
			return this.m_id ^ this.m_version ^ this.m_channel ^ this.m_level ^ this.m_opcode ^ this.m_task ^ (int)this.m_keywords;
		}

		public static bool operator ==(EventDescriptor event1, EventDescriptor event2)
		{
			return event1.Equals(event2);
		}

		public static bool operator !=(EventDescriptor event1, EventDescriptor event2)
		{
			return !event1.Equals(event2);
		}
	}
}