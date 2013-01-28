using System;
using System.Data;
using System.Data.Entity.Core;

namespace Microsoft.Management.Odata.Common
{
	internal class TypeWrapper
	{
		private Type @value;

		private string name;

		public Type Value
		{
			get
			{
				if (this.@value == null && !string.IsNullOrEmpty(this.name))
				{
					this.@value = Type.GetType(this.name);
					if (this.@value == null)
					{
						object[] objArray = new object[1];
						objArray[0] = this.name;
						throw new MetadataException(ExceptionHelpers.GetExceptionMessage(Resources.ClrTypeNameIsInvalid, objArray));
					}
				}
				return this.@value;
			}
		}

		public TypeWrapper(string name)
		{
			this.name = name;
		}

		public TypeWrapper(Type type)
		{
			this.@value = type;
		}
	}
}