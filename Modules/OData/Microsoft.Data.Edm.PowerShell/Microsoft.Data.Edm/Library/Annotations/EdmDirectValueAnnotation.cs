using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Annotations;
using Microsoft.Data.Edm.Library;
using System;

namespace Microsoft.Data.Edm.Library.Annotations
{
	internal class EdmDirectValueAnnotation : EdmNamedElement, IEdmDirectValueAnnotation, IEdmNamedElement, IEdmElement
	{
		private readonly object @value;

		private readonly string namespaceUri;

		public string NamespaceUri
		{
			get
			{
				return this.namespaceUri;
			}
		}

		public object Value
		{
			get
			{
				return this.@value;
			}
		}

		public EdmDirectValueAnnotation(string namespaceUri, string name, object value) : this(namespaceUri, name)
		{
			EdmUtil.CheckArgumentNull<object>(value, "value");
			this.@value = value;
		}

		internal EdmDirectValueAnnotation(string namespaceUri, string name) : base(name)
		{
			EdmUtil.CheckArgumentNull<string>(namespaceUri, "namespaceUri");
			this.namespaceUri = namespaceUri;
		}
	}
}