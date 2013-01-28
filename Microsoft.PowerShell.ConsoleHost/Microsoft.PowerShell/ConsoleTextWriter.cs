using System;
using System.IO;
using System.Text;
using System.Threading;

namespace Microsoft.PowerShell
{
	internal class ConsoleTextWriter : TextWriter
	{
		private ConsoleHostUserInterface ui;

		public override Encoding Encoding
		{
			get
			{
				return null;
			}
		}

		internal ConsoleTextWriter(ConsoleHostUserInterface ui) : base(Thread.CurrentThread.CurrentCulture)
		{
			this.ui = ui;
		}

		public override void Write(string value)
		{
			this.ui.WriteToConsole(value, true);
		}

		public override void Write(bool b)
		{
			this.Write(b.ToString());
		}

		public override void Write(char c)
		{
			this.Write(new string(c, 1));
		}

		public override void Write(char[] a)
		{
			this.Write(new string(a));
		}

		public override void WriteLine(string value)
		{
			this.Write(string.Concat(value, "\r\n"));
		}
	}
}