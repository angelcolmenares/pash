using Microsoft.Management.Odata.MofParser;
using System;

namespace Microsoft.Management.Odata.MofParser.ParseTree
{
	internal sealed class PragmaInstanceLocale : CompilerDirective
	{
		private readonly string m_locale;

		public string Locale
		{
			get
			{
				return this.m_locale;
			}
		}

		internal PragmaInstanceLocale(DocumentRange range, string locale) : base(range, "Locale")
		{
			this.m_locale = locale;
		}

		public override string ToString()
		{
			return string.Concat("#pragma instancelocale(", MofDataType.QuoteAndEscapeString(this.Locale), ")");
		}
	}
}