using System;
using System.Collections;
using System.Globalization;
using System.IO;

namespace System.Management.Instrumentation
{
	internal class CodeWriter
	{
		private int depth;

		private ArrayList children;

		public CodeWriter()
		{
			this.children = new ArrayList();
		}

		public CodeWriter AddChild(string name)
		{
			this.Line(name);
			this.Line("{");
			CodeWriter codeWriter = new CodeWriter();
			codeWriter.depth = this.depth + 1;
			this.children.Add(codeWriter);
			this.Line("}");
			return codeWriter;
		}

		public CodeWriter AddChild(string[] parts)
		{
			return this.AddChild(string.Concat(parts));
		}

		public CodeWriter AddChild(CodeWriter snippet)
		{
			snippet.depth = this.depth;
			this.children.Add(snippet);
			return snippet;
		}

		public CodeWriter AddChildNoIndent(string name)
		{
			this.Line(name);
			CodeWriter codeWriter = new CodeWriter();
			codeWriter.depth = this.depth + 1;
			this.children.Add(codeWriter);
			return codeWriter;
		}

		public void Line(string line)
		{
			this.children.Add(line);
		}

		public void Line(string[] parts)
		{
			this.Line(string.Concat(parts));
		}

		public void Line()
		{
			this.children.Add(null);
		}

		public static explicit operator String(CodeWriter writer)
		{
			return writer.ToString();
		}

		public override string ToString()
		{
			StringWriter stringWriter = new StringWriter(CultureInfo.InvariantCulture);
			this.WriteCode(stringWriter);
			string str = stringWriter.ToString();
			stringWriter.Close();
			return str;
		}

		private void WriteCode(TextWriter writer)
		{
			string str = new string(' ', this.depth * 4);
			foreach (object child in this.children)
			{
				if (child != null)
				{
					if (child as string == null)
					{
						((CodeWriter)child).WriteCode(writer);
					}
					else
					{
						writer.Write(str);
						writer.WriteLine(child);
					}
				}
				else
				{
					writer.WriteLine();
				}
			}
		}
	}
}