using System;

namespace Microsoft.PowerShell.Workflow
{
	internal class Item<T>
	{
		private readonly Guid _instanceId;

		private readonly T _value;

		internal bool Busy
		{
			get;set;
		}

		internal bool Idle
		{
			get;set;
		}

		internal Guid InstanceId
		{
			get
			{
				return this._instanceId;
			}
		}

		internal T Value
		{
			get
			{
				return this._value;
			}
		}

		internal Item(T value, Guid instanceId)
		{
			this._value = value;
			this._instanceId = instanceId;
		}
	}
}