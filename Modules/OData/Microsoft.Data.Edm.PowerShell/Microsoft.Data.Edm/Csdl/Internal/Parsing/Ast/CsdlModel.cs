using System;
using System.Collections.Generic;

namespace Microsoft.Data.Edm.Csdl.Internal.Parsing.Ast
{
	internal class CsdlModel
	{
		private readonly List<CsdlSchema> schemata;

		public IEnumerable<CsdlSchema> Schemata
		{
			get
			{
				return this.schemata;
			}
		}

		public CsdlModel()
		{
			this.schemata = new List<CsdlSchema>();
		}

		public void AddSchema(CsdlSchema schema)
		{
			this.schemata.Add(schema);
		}
	}
}