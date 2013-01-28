using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Internal;
using Microsoft.Data.Edm.Library.Internal;
using Microsoft.Data.Edm.Validation;
using System;

namespace Microsoft.Data.Edm.Csdl.Internal.CsdlSemantics
{
	internal class UnresolvedParameter : BadElement, IEdmFunctionParameter, IEdmNamedElement, IEdmVocabularyAnnotatable, IEdmElement, IUnresolvedElement
	{
		private readonly string name;

		private readonly IEdmFunctionBase declaringFunction;

		private readonly Cache<UnresolvedParameter, IEdmTypeReference> type;

		private readonly static Func<UnresolvedParameter, IEdmTypeReference> ComputeTypeFunc;

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
				return EdmFunctionParameterMode.In;
			}
		}

		public string Name
		{
			get
			{
				return this.name;
			}
		}

		public IEdmTypeReference Type
		{
			get
			{
				return this.type.GetValue(this, UnresolvedParameter.ComputeTypeFunc, null);
			}
		}

		static UnresolvedParameter()
		{
			UnresolvedParameter.ComputeTypeFunc = (UnresolvedParameter me) => me.ComputeType();
		}

		public UnresolvedParameter(IEdmFunctionBase declaringFunction, string name, EdmLocation location)
			: base(new EdmError[] { new EdmError(location, EdmErrorCode.BadUnresolvedParameter, Strings.Bad_UnresolvedParameter(name)) })
		{
			this.type = new Cache<UnresolvedParameter, IEdmTypeReference>();
			string str = name;
			string empty = str;
			if (str == null)
			{
				empty = string.Empty;
			}
			this.name = empty;
			this.declaringFunction = declaringFunction;
		}

		private IEdmTypeReference ComputeType()
		{
			return new BadTypeReference(new BadType(base.Errors), true);
		}
	}
}