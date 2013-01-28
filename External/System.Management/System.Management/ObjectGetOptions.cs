using System;

namespace System.Management
{
	public class ObjectGetOptions : ManagementOptions
	{
		public bool UseAmendedQualifiers
		{
			get
			{
				if ((base.Flags & 0x20000) != 0)
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
				ObjectGetOptions objectGetOption = this;
				if (value)
				{
					flags = base.Flags | 0x20000;
				}
				else
				{
					flags = base.Flags & -131073;
				}
				objectGetOption.Flags = flags;
				base.FireIdentifierChanged();
			}
		}

		public ObjectGetOptions() : this(null, ManagementOptions.InfiniteTimeout, false)
		{
		}

		public ObjectGetOptions(ManagementNamedValueCollection context) : this(context, ManagementOptions.InfiniteTimeout, false)
		{
		}

		public ObjectGetOptions(ManagementNamedValueCollection context, TimeSpan timeout, bool useAmendedQualifiers) : base(context, timeout)
		{
			this.UseAmendedQualifiers = useAmendedQualifiers;
		}

		internal static ObjectGetOptions _Clone(ObjectGetOptions options)
		{
			return ObjectGetOptions._Clone(options, null);
		}

		internal static ObjectGetOptions _Clone(ObjectGetOptions options, IdentifierChangedEventHandler handler)
		{
			ObjectGetOptions objectGetOption;
			if (options == null)
			{
				objectGetOption = new ObjectGetOptions();
			}
			else
			{
				objectGetOption = new ObjectGetOptions(options.context, options.timeout, options.UseAmendedQualifiers);
			}
			if (handler == null)
			{
				if (options != null)
				{
					objectGetOption.IdentifierChanged += new IdentifierChangedEventHandler(options.HandleIdentifierChange);
				}
			}
			else
			{
				objectGetOption.IdentifierChanged += handler;
			}
			return objectGetOption;
		}

		public override object Clone()
		{
			ManagementNamedValueCollection managementNamedValueCollection = null;
			if (base.Context != null)
			{
				managementNamedValueCollection = base.Context.Clone();
			}
			return new ObjectGetOptions(managementNamedValueCollection, base.Timeout, this.UseAmendedQualifiers);
		}
	}
}