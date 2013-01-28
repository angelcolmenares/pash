using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Values;
using System;

namespace Microsoft.Data.Edm.Library.Values
{
	internal class EdmPropertyValue : IEdmPropertyValue, IEdmDelayedValue
	{
		private readonly string name;

		private IEdmValue @value;

		public string Name
		{
			get
			{
				return this.name;
			}
		}

		public IEdmValue JustDecompileGenerated_get_Value()
		{
			return this.@value;
		}

		public void JustDecompileGenerated_set_Value(IEdmValue value)
		{
			EdmUtil.CheckArgumentNull<IEdmValue>(value, "value");
			if (this.@value == null)
			{
				this.@value = value;
				return;
			}
			else
			{
				throw new InvalidOperationException(Strings.ValueHasAlreadyBeenSet);
			}
		}

		public IEdmValue Value
		{
			get
			{
				return JustDecompileGenerated_get_Value();
			}
			set
			{
				JustDecompileGenerated_set_Value(value);
			}
		}

		public EdmPropertyValue(string name)
		{
			EdmUtil.CheckArgumentNull<string>(name, "name");
			this.name = name;
		}

		public EdmPropertyValue(string name, IEdmValue value)
		{
			EdmUtil.CheckArgumentNull<string>(name, "name");
			EdmUtil.CheckArgumentNull<IEdmValue>(value, "value");
			this.name = name;
			this.@value = value;
		}
	}
}