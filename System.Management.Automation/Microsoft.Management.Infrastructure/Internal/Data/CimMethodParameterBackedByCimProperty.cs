using Microsoft.Management.Infrastructure;
using System;

namespace Microsoft.Management.Infrastructure.Internal.Data
{
	internal class CimMethodParameterBackedByCimProperty : CimMethodParameter
	{
		private readonly CimProperty _backingProperty;

		private string _cimSessionComputerName;

		private Guid _cimSessionInstanceId;

		public override CimType CimType
		{
			get
			{
				return this._backingProperty.CimType;
			}
		}

		public override CimFlags Flags
		{
			get
			{
				return this._backingProperty.Flags;
			}
		}

		public override string Name
		{
			get
			{
				return this._backingProperty.Name;
			}
		}

		public override object Value
		{
			get
			{
				object value = this._backingProperty.Value;
				this.ProcessPropertyValue(value);
				return value;
			}
			set
			{
				this._backingProperty.Value = value;
			}
		}

		internal CimMethodParameterBackedByCimProperty(CimProperty backingProperty)
		{
			this._backingProperty = backingProperty;
			this.Initialize(null, Guid.Empty);
		}

		internal CimMethodParameterBackedByCimProperty(CimProperty backingProperty, string cimSessionComputerName, Guid cimSessionInstanceId)
		{
			this._backingProperty = backingProperty;
			this.Initialize(cimSessionComputerName, cimSessionInstanceId);
		}

		private void Initialize(string cimSessionComputerName, Guid cimSessionInstanceId)
		{
			this._cimSessionComputerName = cimSessionComputerName;
			this._cimSessionInstanceId = cimSessionInstanceId;
		}

		private void ProcessPropertyValue(object objectValue)
		{
			if (this._cimSessionComputerName != null || !this._cimSessionInstanceId.Equals(Guid.Empty))
			{
				CimInstance cimInstance = objectValue as CimInstance;
				if (cimInstance == null)
				{
					CimInstance[] cimInstanceArray = objectValue as CimInstance[];
					if (cimInstanceArray != null)
					{
						CimInstance[] cimInstanceArray1 = cimInstanceArray;
						for (int i = 0; i < (int)cimInstanceArray1.Length; i++)
						{
							CimInstance cimInstance1 = cimInstanceArray1[i];
							if (cimInstance1 != null)
							{
								cimInstance1.SetCimSessionComputerName(this._cimSessionComputerName);
								cimInstance1.SetCimSessionInstanceId(this._cimSessionInstanceId);
							}
						}
					}
				}
				else
				{
					cimInstance.SetCimSessionComputerName(this._cimSessionComputerName);
					cimInstance.SetCimSessionInstanceId(this._cimSessionInstanceId);
					return;
				}
			}
		}
	}
}