using Microsoft.Data.Edm.Csdl.Internal.Parsing.Ast;

namespace Microsoft.Data.Edm.Csdl.Internal.CsdlSemantics
{
	internal class CsdlSemanticsAnnotations
	{
		private readonly CsdlAnnotations annotations;

		private readonly CsdlSemanticsSchema context;

		public CsdlAnnotations Annotations
		{
			get
			{
				return this.annotations;
			}
		}

		public CsdlSemanticsSchema Context
		{
			get
			{
				return this.context;
			}
		}

		public CsdlSemanticsAnnotations(CsdlSemanticsSchema context, CsdlAnnotations annotations)
		{
			this.context = context;
			this.annotations = annotations;
		}
	}
}