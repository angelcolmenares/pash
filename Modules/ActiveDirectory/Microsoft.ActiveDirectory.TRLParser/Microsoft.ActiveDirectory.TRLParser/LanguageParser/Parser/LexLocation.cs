using System;

namespace Microsoft.ActiveDirectory.TRLParser.LanguageParser.Parser
{
	internal class LexLocation : IMerge<LexLocation>
	{
		public int sLin;

		public int sCol;

		public int eLin;

		public int eCol;

		public LexLocation()
		{
		}

		public LexLocation(int sl, int sc, int el, int ec)
		{
			this.sLin = sl;
			this.sCol = sc;
			this.eLin = el;
			this.eCol = ec;
		}

		public LexLocation Merge(LexLocation last)
		{
			return new LexLocation(this.sLin, this.sCol, last.eLin, last.eCol);
		}
	}
}