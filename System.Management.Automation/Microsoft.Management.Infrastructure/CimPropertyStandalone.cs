using Microsoft.Management.Infrastructure;
using Microsoft.Management.Infrastructure.Internal;
using Microsoft.Management.Infrastructure.Native;
using System;
using System.Collections;

namespace Microsoft.Management.Infrastructure.Internal.Data
{
	internal sealed class CimPropertyStandalone : CimProperty
	{
		private readonly string _name;

		private object _value;

		private readonly CimType _cimType;

		private CimFlags _flags;

		public override CimType CimType
		{
			get
			{
				return this._cimType;
			}
		}

		public override CimFlags Flags
		{
			get
			{
				return this._flags;
			}
		}

		public override bool IsValueModified
		{
			get
			{
				return base.IsValueModified;
			}
			set
			{
				bool flag = !value;
				if (!flag)
				{
					this._flags = this._flags & (CimFlags.Class | CimFlags.Method | CimFlags.Property | CimFlags.Parameter | CimFlags.Association | CimFlags.Indication | CimFlags.Reference | CimFlags.Any | CimFlags.EnableOverride | CimFlags.DisableOverride | CimFlags.Restricted | CimFlags.ToSubclass | CimFlags.Translatable | CimFlags.Key | CimFlags.In | CimFlags.Out | CimFlags.Required | CimFlags.Static | CimFlags.Abstract | CimFlags.Terminal | CimFlags.Expensive | CimFlags.Stream | CimFlags.ReadOnly | CimFlags.NullValue | CimFlags.Borrow | CimFlags.Adopt);
					return;
				}
				else
				{
					this._flags = this._flags | CimFlags.NotModified;
					return;
				}
			}
		}

		public override string Name
		{
			get
			{
				return this._name;
			}
		}

		public override object Value
		{
			get
			{
				return this._value;
			}
			set
			{
				if (value != null)
				{
					try
					{
						Helpers.ValidateNoNullElements(value as IList);
						//TODO: InstanceMethods.ThrowIfMismatchedType(this.CimType.ToMiType(), CimInstance.ConvertToNativeLayer(value, this.CimType));
					}
					catch (InvalidCastException invalidCastException1)
					{
						InvalidCastException invalidCastException = invalidCastException1;
						throw new ArgumentException(invalidCastException.Message, "value", invalidCastException);
					}
					catch (FormatException formatException1)
					{
						FormatException formatException = formatException1;
						throw new ArgumentException(formatException.Message, "value", formatException);
					}
					catch (ArgumentException argumentException1)
					{
						ArgumentException argumentException = argumentException1;
						throw new ArgumentException(argumentException.Message, "value", argumentException);
					}
				}
				this._value = value;
				this.IsValueModified = true;
			}
		}

		internal CimPropertyStandalone(string name, object value, CimType cimType, CimFlags flags)
		{
			this._name = name;
			this._cimType = cimType;
			this._flags = flags;
			this.Value = value;
		}
	}
}