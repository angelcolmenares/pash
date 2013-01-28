using System;
using System.IO;

namespace Microsoft.ActiveDirectory.TRLParser.LanguageParser.Parser
{
	internal sealed class StreamBuffer : ScanBuffer, IDisposable
	{
		private BufferedStream bufferedStrm;

		private int delta;

		public override int Position
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

		public override int ReadPosition
		{
			get
			{
				return (int)this.bufferedStrm.Position - this.delta;
			}
		}

		public StreamBuffer(Stream str)
		{
			this.delta = 1;
			this.bufferedStrm = new BufferedStream(str);
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

		public override string GetString(int beginPosition, int endPosition)
		{
			if (endPosition - beginPosition > 0)
			{
				long position = this.bufferedStrm.Position;
				char[] chrArray = new char[endPosition - beginPosition];
				this.bufferedStrm.Position = (long)beginPosition;
				for (int i = 0; i < endPosition - beginPosition; i++)
				{
					chrArray[i] = (char)((ushort)this.bufferedStrm.ReadByte());
				}
				this.bufferedStrm.Position = position;
				return new string(chrArray);
			}
			else
			{
				return "";
			}
		}

		public override int Peek()
		{
			int num = this.bufferedStrm.ReadByte();
			this.bufferedStrm.Seek((long)(-this.delta), SeekOrigin.Current);
			return num;
		}

		public override int Read()
		{
			return this.bufferedStrm.ReadByte();
		}
	}
}