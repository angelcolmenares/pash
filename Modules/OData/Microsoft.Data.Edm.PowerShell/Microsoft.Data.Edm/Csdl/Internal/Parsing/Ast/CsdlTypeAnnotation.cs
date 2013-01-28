using Microsoft.Data.Edm.Csdl;
using System;
using System.Collections.Generic;

namespace Microsoft.Data.Edm.Csdl.Internal.Parsing.Ast
{
	internal class CsdlTypeAnnotation : CsdlVocabularyAnnotationBase
	{
		private readonly List<CsdlPropertyValue> properties;

		public IEnumerable<CsdlPropertyValue> Properties
		{
			get
			{
				return this.properties;
			}
		}

		public CsdlTypeAnnotation(string term, string qualifier, IEnumerable<CsdlPropertyValue> properties, CsdlLocation location) : base(term, qualifier, location)
		{
			this.properties = new List<CsdlPropertyValue>(properties);
		}
	}
}