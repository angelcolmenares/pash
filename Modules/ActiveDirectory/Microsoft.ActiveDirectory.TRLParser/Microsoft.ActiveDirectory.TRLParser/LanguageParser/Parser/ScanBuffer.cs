using System;

namespace Microsoft.ActiveDirectory.TRLParser.LanguageParser.Parser
{
	internal abstract class ScanBuffer
	{
		public const int Eof = -1;

		public abstract int Position
		{
			get;
			set;
		}

		public abstract int ReadPosition
		{
			get;
		}

		protected ScanBuffer()
		{
		}

		public abstract string GetString(int beginPosition, int endPosition);

		public abstract int Peek();

		public abstract int Read();
	}
}