using Microsoft.Management.Odata.MofParser;
using System;

namespace Microsoft.Management.Odata.MofParser.ParseTree
{
	internal sealed class PragmaInclude : CompilerDirective
	{
		private readonly string m_filename;

		public string Filename
		{
			get
			{
				return this.m_filename;
			}
		}

		internal PragmaInclude(DocumentRange range, string filename) : base(range, "Include")
		{
			this.m_filename = filename;
		}

		public override string ToString()
		{
			return string.Concat("#pragma include(", MofDataType.QuoteAndEscapeString(this.Filename), ")");
		}
	}
}