using Microsoft.Data.Edm;
using System;

namespace Microsoft.Data.Edm.Library
{
	internal class EdmFunctionParameter : EdmNamedElement, IEdmFunctionParameter, IEdmNamedElement, IEdmVocabularyAnnotatable, IEdmElement
	{
		private readonly IEdmTypeReference type;

		private readonly EdmFunctionParameterMode mode;

		private readonly IEdmFunctionBase declaringFunction;

		public IEdmFunctionBase DeclaringFunction
		{
			get
			{
				return this.declaringFunction;
			}
		}

		public EdmFunctionParameterMode Mode
		{
			get
			{
				return this.mode;
			}
		}

		public IEdmTypeReference Type
		{
			get
			{
				return this.type;
			}
		}

		public EdmFunctionParameter(IEdmFunctionBase declaringFunction, string name, IEdmTypeReference type) : this(declaringFunction, name, type, (EdmFunctionParameterMode)1)
		{
		}

		public EdmFunctionParameter(IEdmFunctionBase declaringFunction, string name, IEdmTypeReference type, EdmFunctionParameterMode mode) : base(name)
		{
			EdmUtil.CheckArgumentNull<IEdmFunctionBase>(declaringFunction, "declaringFunction");
			EdmUtil.CheckArgumentNull<string>(name, "name");
			EdmUtil.CheckArgumentNull<IEdmTypeReference>(type, "type");
			this.type = type;
			this.mode = mode;
			this.declaringFunction = declaringFunction;
		}
	}
}