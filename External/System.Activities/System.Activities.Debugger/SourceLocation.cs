using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Activities;
using System.Activities.Expressions;
using System.Activities.Hosting;
using System.Activities.Statements;
using System.Xaml;

namespace System.Activities.Debugger
{
	[Serializable]
	public class SourceLocation
	{
		public SourceLocation (string fileName, int line)
		{
			throw new NotImplementedException ();
		}

		public SourceLocation (string fileName, int startLine, int startColumn, int endLine, int endColumn)
		{
			throw new NotImplementedException ();
		}

		public int EndColumn { get { throw new NotImplementedException (); } }
		public int EndLine { get { throw new NotImplementedException (); } }
		public string FileName { get { throw new NotImplementedException (); } }
		public bool IsSingleWholeLine { get { throw new NotImplementedException (); } }
		public int StartColumn { get { throw new NotImplementedException (); } }
		public int StartLine { get { throw new NotImplementedException (); } }
	}
}
