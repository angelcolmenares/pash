using System;

namespace Microsoft.ActiveDirectory.TRLParser.LanguageParser.Parser
{
	internal sealed class StringBuffer : ScanBuffer
	{
		private string str;

		private int bPos;

		private int sLen;

		public override int Position
		{
			get
			{
				return this.bPos;
			}
			set
			{
				this.bPos = value;
			}
		}

		public override int ReadPosition
		{
			get
			{
				return this.bPos - 1;
			}
		}

		public StringBuffer(string str)
		{
			this.str = str;
			this.sLen = str.Length;
		}

		public override string GetString(int beginPosition, int endPosition)
		{
			if (endPosition > this.sLen)
			{
				endPosition = this.sLen;
			}
			if (endPosition > beginPosition)
			{
				return this.str.Substring(beginPosition, endPosition - beginPosition);
			}
			else
			{
				return "";
			}
		}

		public override int Peek()
		{
			if (this.bPos >= this.sLen)
			{
				return 10;
			}
			else
			{
				return this.str[this.bPos];
			}
		}

		public override int Read()
		{
			if (this.bPos >= this.sLen)
			{
				if (this.bPos != this.sLen)
				{
					return -1;
				}
				else
				{
					StringBuffer stringBuffer = this;
					stringBuffer.bPos = stringBuffer.bPos + 1;
					return 10;
				}
			}
			else
			{
				StringBuffer stringBuffer1 = this;
				int num = stringBuffer1.bPos;
				int num1 = num;
				stringBuffer1.bPos = num + 1;
				return this.str[num1];
			}
		}
	}
}