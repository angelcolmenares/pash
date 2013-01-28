using Microsoft.Management.Odata.MofParser;

namespace Tokenizer
{
	internal sealed class TokenComment : TokenWhitespace
	{
		public override WhitespaceType WhitespaceType
		{
			get
			{
				return WhitespaceType.Comment;
			}
		}

		internal TokenComment(DocumentRange range) : base(range)
		{
		}
	}
}