using System;
using System.IO;

namespace Microsoft.ActiveDirectory.TRLParser.LanguageParser.Parser
{
	internal sealed class BigEndTextBuffer : TextBuffer
	{
		internal BigEndTextBuffer(Stream str) : base(str)
		{
		}

		public override int Read()
		{
			int num = this.bufferedStrm.ReadByte();
			int num1 = this.bufferedStrm.ReadByte();
			if (num != -1 || num1 != -1)
			{
				this.delta = 2;
				return (num << 8) + num1;
			}
			else
			{
				return -1;
			}
		}
	}
}