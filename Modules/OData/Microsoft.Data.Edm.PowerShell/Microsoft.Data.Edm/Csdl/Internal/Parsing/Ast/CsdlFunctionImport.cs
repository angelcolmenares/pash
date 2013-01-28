using Microsoft.Data.Edm.Csdl;
using System;
using System.Collections.Generic;

namespace Microsoft.Data.Edm.Csdl.Internal.Parsing.Ast
{
	internal class CsdlFunctionImport : CsdlFunctionBase
	{
		private readonly bool sideEffecting;

		private readonly bool composable;

		private readonly bool bindable;

		private readonly string entitySet;

		private readonly string entitySetPath;

		public bool Bindable
		{
			get
			{
				return this.bindable;
			}
		}

		public bool Composable
		{
			get
			{
				return this.composable;
			}
		}

		public string EntitySet
		{
			get
			{
				return this.entitySet;
			}
		}

		public string EntitySetPath
		{
			get
			{
				return this.entitySetPath;
			}
		}

		public bool SideEffecting
		{
			get
			{
				return this.sideEffecting;
			}
		}

		public CsdlFunctionImport(string name, bool sideEffecting, bool composable, bool bindable, string entitySet, string entitySetPath, IEnumerable<CsdlFunctionParameter> parameters, CsdlTypeReference returnType, CsdlDocumentation documentation, CsdlLocation location) : base(name, parameters, returnType, documentation, location)
		{
			this.sideEffecting = sideEffecting;
			this.composable = composable;
			this.bindable = bindable;
			this.entitySet = entitySet;
			this.entitySetPath = entitySetPath;
		}
	}
}