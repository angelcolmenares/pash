using Microsoft.Data.Edm;
using System;
using System.Collections.Generic;

namespace Microsoft.Data.Edm.Library
{
	internal abstract class EdmFunctionBase : EdmNamedElement, IEdmFunctionBase, IEdmNamedElement, IEdmVocabularyAnnotatable, IEdmElement
	{
		private readonly List<IEdmFunctionParameter> parameters;

		private IEdmTypeReference returnType;

		public IEnumerable<IEdmFunctionParameter> Parameters
		{
			get
			{
				return this.parameters;
			}
		}

		public IEdmTypeReference ReturnType
		{
			get
			{
				return this.returnType;
			}
		}

		protected EdmFunctionBase(string name, IEdmTypeReference returnType) : base(name)
		{
			this.parameters = new List<IEdmFunctionParameter>();
			this.returnType = returnType;
		}

		public EdmFunctionParameter AddParameter(string name, IEdmTypeReference type)
		{
			EdmFunctionParameter edmFunctionParameter = new EdmFunctionParameter(this, name, type);
			this.parameters.Add(edmFunctionParameter);
			return edmFunctionParameter;
		}

		public EdmFunctionParameter AddParameter(string name, IEdmTypeReference type, EdmFunctionParameterMode mode)
		{
			EdmFunctionParameter edmFunctionParameter = new EdmFunctionParameter(this, name, type, mode);
			this.parameters.Add(edmFunctionParameter);
			return edmFunctionParameter;
		}

		public void AddParameter(IEdmFunctionParameter parameter)
		{
			EdmUtil.CheckArgumentNull<IEdmFunctionParameter>(parameter, "parameter");
			this.parameters.Add(parameter);
		}

		public IEdmFunctionParameter FindParameter(string name)
		{
			IEdmFunctionParameter edmFunctionParameter;
			List<IEdmFunctionParameter>.Enumerator enumerator = this.parameters.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					IEdmFunctionParameter current = enumerator.Current;
					if (current.Name != name)
					{
						continue;
					}
					edmFunctionParameter = current;
					return edmFunctionParameter;
				}
				return null;
			}
			finally
			{
				enumerator.Dispose();
			}
			return edmFunctionParameter;
		}
	}
}