using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Annotations;
using Microsoft.Data.Edm.Csdl.Internal.Parsing.Ast;
using System;

namespace Microsoft.Data.Edm.Csdl.Internal.CsdlSemantics
{
	internal class CsdlSemanticsDocumentation : CsdlSemanticsElement, IEdmDocumentation, IEdmDirectValueAnnotation, IEdmNamedElement, IEdmElement
	{
		private readonly CsdlDocumentation documentation;

		private readonly CsdlSemanticsModel model;

		public string Description
		{
			get
			{
				return this.documentation.LongDescription;
			}
		}

		public override CsdlElement Element
		{
			get
			{
				return this.documentation;
			}
		}

		object Microsoft.Data.Edm.Annotations.IEdmDirectValueAnnotation.Value
		{
			get
			{
				return this;
			}
		}

		public override CsdlSemanticsModel Model
		{
			get
			{
				return this.model;
			}
		}

		public string Name
		{
			get
			{
				return "Documentation";
			}
		}

		public string NamespaceUri
		{
			get
			{
				return "http://schemas.microsoft.com/ado/2011/04/edm/documentation";
			}
		}

		public string Summary
		{
			get
			{
				return this.documentation.Summary;
			}
		}

		public CsdlSemanticsDocumentation(CsdlDocumentation documentation, CsdlSemanticsModel model) : base(documentation)
		{
			this.documentation = documentation;
			this.model = model;
		}
	}
}