using Microsoft.ActiveDirectory.TRLParser;
using System;
using System.IO;

namespace Microsoft.ActiveDirectory.TRLParser.LanguageParser.Parser
{
	internal class TextBuffer : ScanBuffer, IDisposable
	{
		protected BufferedStream bufferedStrm;

		protected int delta;

		public sealed override int Position
		{
			get
			{
				return (int)this.bufferedStrm.Position;
			}
			set
			{
				this.bufferedStrm.Position = (long)value;
			}
		}

		public sealed override int ReadPosition
		{
			get
			{
				return (int)this.bufferedStrm.Position - this.delta;
			}
		}

		protected TextBuffer(Stream str)
		{
			this.delta = 1;
			this.bufferedStrm = new BufferedStream(str);
		}

		private static Exception BadUTF8()
		{
			return new InvalidOperationException(SR.GetString("POLICY0033", new object[0]));
		}

		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		private void Dispose(bool disposing)
		{
			if (disposing && this.bufferedStrm != null)
			{
				this.bufferedStrm.Dispose();
			}
		}

		public sealed override string GetString(int beginPosition, int endPosition)
		{
			if (endPosition - beginPosition > 0)
			{
				long position = this.bufferedStrm.Position;
				char[] chrArray = new char[endPosition - beginPosition];
				this.bufferedStrm.Position = (long)beginPosition;
				int num = 0;
				while (this.bufferedStrm.Position < (long)endPosition)
				{
					chrArray[num] = (char)((ushort)this.Read());
					num++;
				}
				this.bufferedStrm.Position = position;
				return new string(chrArray, 0, num);
			}
			else
			{
				return "";
			}
		}

		public static TextBuffer NewTextBuff(Stream strm)
		{
			int num = strm.ReadByte();
			int num1 = strm.ReadByte();
			if (num != 254 || num1 != 0xff)
			{
				if (num != 0xff || num1 != 254)
				{
					int num2 = strm.ReadByte();
					if (num != 239 || num1 != 187 || num2 != 191)
					{
						strm.Seek((long)0, SeekOrigin.Begin);
						return new TextBuffer(strm);
					}
					else
					{
						return new TextBuffer(strm);
					}
				}
				else
				{
					return new LittleEndTextBuffer(strm);
				}
			}
			else
			{
				return new BigEndTextBuffer(strm);
			}
		}

		public sealed override int Peek()
		{
			int num = this.Read();
			this.bufferedStrm.Seek((long)(-this.delta), SeekOrigin.Current);
			return num;
		}

		public override int Read()
		{
			int num;
			int num1;
			int num2 = this.bufferedStrm.ReadByte();
			if (num2 >= 127)
			{
				if ((num2 & 224) != 192)
				{
					if ((num2 & 240) != 224)
					{
						throw TextBuffer.BadUTF8();
					}
					else
					{
						this.delta = 3;
						num = this.bufferedStrm.ReadByte();
						int num3 = this.bufferedStrm.ReadByte();
						if ((num & num3 & 192) != 128)
						{
							throw TextBuffer.BadUTF8();
						}
						else
						{
							return ((num2 & 15) << 12) + ((num & 63) << 6) + (num3 & 63);
						}
					}
				}
				else
				{
					this.delta = 2;
					num = this.bufferedStrm.ReadByte();
					if ((num & 192) != 128)
					{
						throw TextBuffer.BadUTF8();
					}
					else
					{
						return ((num2 & 31) << 6) + (num & 63);
					}
				}
			}
			else
			{
				TextBuffer textBuffer = this;
				if (num2 == -1)
				{
					num1 = 0;
				}
				else
				{
					num1 = 1;
				}
				textBuffer.delta = num1;
				return num2;
			}
		}
	}
}