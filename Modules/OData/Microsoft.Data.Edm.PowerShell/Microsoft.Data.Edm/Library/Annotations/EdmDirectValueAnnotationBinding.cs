using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Annotations;
using System;

namespace Microsoft.Data.Edm.Library.Annotations
{
	internal class EdmDirectValueAnnotationBinding : IEdmDirectValueAnnotationBinding
	{
		private readonly IEdmElement element;

		private readonly string namespaceUri;

		private readonly string name;

		private readonly object @value;

		public IEdmElement Element
		{
			get
			{
				return this.element;
			}
		}

		public string Name
		{
			get
			{
				return this.name;
			}
		}

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

		public EdmDirectValueAnnotationBinding(IEdmElement element, string namespaceUri, string name, object value)
		{
			EdmUtil.CheckArgumentNull<IEdmElement>(element, "element");
			EdmUtil.CheckArgumentNull<string>(namespaceUri, "namespaceUri");
			EdmUtil.CheckArgumentNull<string>(name, "name");
			this.element = element;
			this.namespaceUri = namespaceUri;
			this.name = name;
			this.@value = value;
		}

		public EdmDirectValueAnnotationBinding(IEdmElement element, string namespaceUri, string name)
		{
			EdmUtil.CheckArgumentNull<IEdmElement>(element, "element");
			EdmUtil.CheckArgumentNull<string>(namespaceUri, "namespaceUri");
			EdmUtil.CheckArgumentNull<string>(name, "name");
			this.element = element;
			this.namespaceUri = namespaceUri;
			this.name = name;
		}
	}
}