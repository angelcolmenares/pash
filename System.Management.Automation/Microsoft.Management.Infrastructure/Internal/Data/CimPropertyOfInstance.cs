using Microsoft.Management.Infrastructure;
using Microsoft.Management.Infrastructure.Internal;
using Microsoft.Management.Infrastructure.Native;
using Microsoft.Management.Infrastructure.Options.Internal;
using System;
using System.Collections;

namespace Microsoft.Management.Infrastructure.Internal.Data
{
	internal sealed class CimPropertyOfInstance : CimProperty
	{
		private readonly SharedInstanceHandle _instanceHandle;

		private readonly CimInstance _instance;

		private readonly int _index;

		public override CimType CimType
		{
			get
			{
				MiType miType = MiType.Boolean;
				MiResult elementAtGetType = InstanceMethods.GetElementAt_GetType(this._instanceHandle.Handle, this._index, out miType);
				CimException.ThrowIfMiResultFailure(elementAtGetType);
				return miType.ToCimType();
			}
		}

		public override CimFlags Flags
		{
			get
			{
				MiFlags miFlag = 0;
				MiResult elementAtGetFlags = InstanceMethods.GetElementAt_GetFlags(this._instanceHandle.Handle, this._index, out miFlag);
				CimException.ThrowIfMiResultFailure(elementAtGetFlags);
				return miFlag.ToCimFlags();
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
				MiResult miResult = InstanceMethods.SetElementAt_SetNotModifiedFlag(this._instanceHandle.Handle, this._index, flag);
				CimException.ThrowIfMiResultFailure(miResult);
			}
		}

		public override string Name
		{
			get
			{
				string str = null;
				MiResult elementAtGetName = InstanceMethods.GetElementAt_GetName(this._instanceHandle.Handle, this._index, out str);
				CimException.ThrowIfMiResultFailure(elementAtGetName);
				return str;
			}
		}

		public override object Value
		{
			get
			{
				object obj = null;
				object obj1;
				try
				{
					this._instanceHandle.AddRef();
					MiResult elementAtGetValue = InstanceMethods.GetElementAt_GetValue(this._instanceHandle.Handle, this._index, out obj);
					CimException.ThrowIfMiResultFailure(elementAtGetValue);
					obj1 = CimInstance.ConvertFromNativeLayer(obj, this._instanceHandle, this._instance, false);
				}
				finally
				{
					this._instanceHandle.Release();
				}
				return obj1;
			}
			set
			{
				MiResult miResult;
				if (value != null)
				{
					try
					{
						Helpers.ValidateNoNullElements(value as IList);
						miResult = InstanceMethods.SetElementAt_SetValue(this._instanceHandle.Handle, this._index, CimInstance.ConvertToNativeLayer(value, this.CimType));
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
				else
				{
					miResult = InstanceMethods.ClearElementAt(this._instanceHandle.Handle, this._index);
				}
				CimException.ThrowIfMiResultFailure(miResult);
			}
		}

		internal CimPropertyOfInstance (SharedInstanceHandle instanceHandle, CimInstance instance, int index)
		{
			this._instanceHandle = instanceHandle;
			this._instance = instance;
			this._index = index;
		}
	}
}