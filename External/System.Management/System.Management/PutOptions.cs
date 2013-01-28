using System;

namespace System.Management
{
	public class PutOptions : ManagementOptions
	{
		public PutType Type
		{
			get
			{
				if ((base.Flags & 1) != 0)
				{
					return PutType.UpdateOnly;
				}
				else
				{
					if ((base.Flags & 2) != 0)
					{
						return PutType.CreateOnly;
					}
					else
					{
						return PutType.UpdateOrCreate;
					}
				}
			}
			set
			{
				PutType putType = value;
				switch (putType)
				{
					case PutType.UpdateOnly:
					{
						PutOptions flags = this;
						flags.Flags = flags.Flags | 1;
						return;
					}
					case PutType.CreateOnly:
					{
						PutOptions putOption = this;
						putOption.Flags = putOption.Flags | 2;
						return;
					}
					case PutType.UpdateOrCreate:
					{
						PutOptions flags1 = this;
						flags1.Flags = flags1.Flags;
						return;
					}
				}
				throw new ArgumentException(null, "Type");
			}
		}

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
				PutOptions putOption = this;
				if (value)
				{
					flags = base.Flags | 0x20000;
				}
				else
				{
					flags = base.Flags & -131073;
				}
				putOption.Flags = flags;
			}
		}

		public PutOptions() : this((ManagementNamedValueCollection)null, ManagementOptions.InfiniteTimeout, false, (PutType)3)
		{
		}

		public PutOptions(ManagementNamedValueCollection context) : this(context, ManagementOptions.InfiniteTimeout, false, (PutType)3)
		{
		}

		public PutOptions(ManagementNamedValueCollection context, TimeSpan timeout, bool useAmendedQualifiers, PutType putType) : base(context, timeout)
		{
			this.UseAmendedQualifiers = useAmendedQualifiers;
			this.Type = putType;
		}

		public override object Clone()
		{
			ManagementNamedValueCollection managementNamedValueCollection = null;
			if (base.Context != null)
			{
				managementNamedValueCollection = base.Context.Clone();
			}
			return new PutOptions(managementNamedValueCollection, base.Timeout, this.UseAmendedQualifiers, this.Type);
		}
	}
}