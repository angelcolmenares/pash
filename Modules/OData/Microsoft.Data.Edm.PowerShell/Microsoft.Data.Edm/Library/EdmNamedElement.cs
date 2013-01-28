using Microsoft.Data.Edm;
using System;

namespace Microsoft.Data.Edm.Library
{
	internal abstract class EdmNamedElement : EdmElement, IEdmNamedElement, IEdmElement
	{
		private readonly string name;

		public string Name
		{
			get
			{
				return this.name;
			}
		}

		protected EdmNamedElement(string name)
		{
			EdmUtil.CheckArgumentNull<string>(name, "name");
			this.name = name;
		}
	}
}