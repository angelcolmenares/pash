using Microsoft.Data.Edm.Csdl;
using System;
using System.Collections.Generic;

namespace Microsoft.Data.Edm.Csdl.Internal.Parsing.Ast
{
	internal class CsdlEntityContainer : CsdlNamedElement
	{
		private readonly string extends;

		private readonly List<CsdlEntitySet> entitySets;

		private readonly List<CsdlAssociationSet> associationSets;

		private readonly List<CsdlFunctionImport> functionImports;

		public IEnumerable<CsdlAssociationSet> AssociationSets
		{
			get
			{
				return this.associationSets;
			}
		}

		public IEnumerable<CsdlEntitySet> EntitySets
		{
			get
			{
				return this.entitySets;
			}
		}

		public string Extends
		{
			get
			{
				return this.extends;
			}
		}

		public IEnumerable<CsdlFunctionImport> FunctionImports
		{
			get
			{
				return this.functionImports;
			}
		}

		public CsdlEntityContainer(string name, string extends, IEnumerable<CsdlEntitySet> entitySets, IEnumerable<CsdlAssociationSet> associationSets, IEnumerable<CsdlFunctionImport> functionImports, CsdlDocumentation documentation, CsdlLocation location) : base(name, documentation, location)
		{
			this.extends = extends;
			this.entitySets = new List<CsdlEntitySet>(entitySets);
			this.associationSets = new List<CsdlAssociationSet>(associationSets);
			this.functionImports = new List<CsdlFunctionImport>(functionImports);
		}
	}
}