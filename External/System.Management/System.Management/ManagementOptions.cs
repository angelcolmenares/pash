using System;
using System.ComponentModel;
using System.Runtime;

namespace System.Management
{
	[TypeConverter(typeof(ExpandableObjectConverter))]
	public abstract class ManagementOptions : ICloneable
	{
		public readonly static TimeSpan InfiniteTimeout;

		internal int flags;

		internal ManagementNamedValueCollection context;

		internal TimeSpan timeout;

		public ManagementNamedValueCollection Context
		{
			get
			{
				if (this.context != null)
				{
					return this.context;
				}
				else
				{
					ManagementNamedValueCollection managementNamedValueCollection = new ManagementNamedValueCollection();
					ManagementNamedValueCollection managementNamedValueCollection1 = managementNamedValueCollection;
					this.context = managementNamedValueCollection;
					return managementNamedValueCollection1;
				}
			}
			set
			{
				ManagementNamedValueCollection managementNamedValueCollection = this.context;
				if (value == null)
				{
					this.context = new ManagementNamedValueCollection();
				}
				else
				{
					this.context = value.Clone();
				}
				if (managementNamedValueCollection != null)
				{
					managementNamedValueCollection.IdentifierChanged -= new IdentifierChangedEventHandler(this.HandleIdentifierChange);
				}
				this.context.IdentifierChanged += new IdentifierChangedEventHandler(this.HandleIdentifierChange);
				this.HandleIdentifierChange(this, null);
			}
		}

		internal int Flags
		{
			get
			{
				return this.flags;
			}
			set
			{
				this.flags = value;
			}
		}

		internal bool SendStatus
		{
			get
			{
				if ((this.Flags & 128) != 0)
				{
					return true;
				}
				else
				{
					return false;
				}
			}
			set
			{
				int flags;
				ManagementOptions managementOption = this;
				if (!value)
				{
					flags = this.Flags & -129;
				}
				else
				{
					flags = this.Flags | 128;
				}
				managementOption.Flags = flags;
			}
		}

		public TimeSpan Timeout
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.timeout;
			}
			set
			{
				if (value.Ticks >= (long)0)
				{
					this.timeout = value;
					this.FireIdentifierChanged();
					return;
				}
				else
				{
					throw new ArgumentOutOfRangeException("value");
				}
			}
		}

		static ManagementOptions()
		{
			ManagementOptions.InfiniteTimeout = TimeSpan.MaxValue;
		}

		internal ManagementOptions() : this(null, ManagementOptions.InfiniteTimeout)
		{
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		internal ManagementOptions(ManagementNamedValueCollection context, TimeSpan timeout) : this(context, timeout, 0)
		{
		}

		internal ManagementOptions(ManagementNamedValueCollection context, TimeSpan timeout, int flags)
		{
			this.flags = flags;
			if (context == null)
			{
				this.context = null;
			}
			else
			{
				this.Context = context;
			}
			this.Timeout = timeout;
		}

		public abstract object Clone();

		internal void FireIdentifierChanged()
		{
			if (this.IdentifierChanged != null)
			{
				this.IdentifierChanged(this, null);
			}
		}

		internal IWbemContext GetContext()
		{
			if (this.context == null)
			{
				return null;
			}
			else
			{
				return this.context.GetContext();
			}
		}

		internal void HandleIdentifierChange(object sender, IdentifierChangedEventArgs args)
		{
			this.FireIdentifierChanged();
		}

		internal event IdentifierChangedEventHandler IdentifierChanged;
	}
}