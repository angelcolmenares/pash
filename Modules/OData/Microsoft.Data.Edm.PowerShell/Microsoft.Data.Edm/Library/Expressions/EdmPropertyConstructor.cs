using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Expressions;
using Microsoft.Data.Edm.Library;
using System;

namespace Microsoft.Data.Edm.Library.Expressions
{
	internal class EdmPropertyConstructor : EdmElement, IEdmPropertyConstructor, IEdmElement
	{
		private readonly string name;

		private readonly IEdmExpression @value;

		public string Name
		{
			get
			{
				return this.name;
			}
		}

		public IEdmExpression Value
		{
			get
			{
				return this.@value;
			}
		}

		public EdmPropertyConstructor(string name, IEdmExpression value)
		{
			EdmUtil.CheckArgumentNull<string>(name, "name");
			EdmUtil.CheckArgumentNull<IEdmExpression>(value, "value");
			this.name = name;
			this.@value = value;
		}
	}
}