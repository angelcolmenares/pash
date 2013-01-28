using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Values;
using System.Collections.Generic;

namespace Microsoft.Data.Edm.Library.Values
{
	internal class EdmCollectionValue : EdmValue, IEdmCollectionValue, IEdmValue, IEdmElement
	{
		private readonly IEnumerable<IEdmDelayedValue> elements;

		public IEnumerable<IEdmDelayedValue> Elements
		{
			get
			{
				return this.elements;
			}
		}

		public override EdmValueKind ValueKind
		{
			get
			{
				return EdmValueKind.Collection;
			}
		}

		public EdmCollectionValue(IEdmCollectionTypeReference type, IEnumerable<IEdmDelayedValue> elements) : base(type)
		{
			this.elements = elements;
		}
	}
}