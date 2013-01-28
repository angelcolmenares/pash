using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Annotations;
using Microsoft.Data.Edm.Library;
using System;

namespace Microsoft.Data.Edm.Library.Annotations
{
	internal class EdmTypedDirectValueAnnotationBinding<T> : EdmNamedElement, IEdmDirectValueAnnotationBinding
	{
		private readonly IEdmElement element;

		private readonly T @value;

		public IEdmElement Element
		{
			get
			{
				return this.element;
			}
		}

		public string NamespaceUri
		{
			get
			{
				return "http://schemas.microsoft.com/ado/2011/04/edm/internal";
			}
		}

		public object Value
		{
			get
			{
				return this.@value;
			}
		}

		public EdmTypedDirectValueAnnotationBinding(IEdmElement element, T value) : base(ExtensionMethods.TypeName<T>.LocalName)
		{
			this.element = element;
			this.@value = value;
		}
	}
}