using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Values;

namespace Microsoft.Data.Edm.Library.Values
{
	internal abstract class EdmValue : IEdmValue, IEdmElement, IEdmDelayedValue
	{
		private readonly IEdmTypeReference type;

		IEdmValue Microsoft.Data.Edm.Values.IEdmDelayedValue.Value
		{
			get
			{
				return this;
			}
		}

		public IEdmTypeReference Type
		{
			get
			{
				return this.type;
			}
		}

		public abstract EdmValueKind ValueKind
		{
			get;
		}

		protected EdmValue(IEdmTypeReference type)
		{
			this.type = type;
		}
	}
}