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
	public class VirtualStackFrame
	{
		public VirtualStackFrame (State state)
		{
			throw new NotImplementedException ();
		}
		public VirtualStackFrame (State state, IDictionary<string, Object> locals)
		{
			throw new NotImplementedException ();
		}

		public IDictionary<string, Object> Locals { get { throw new NotImplementedException (); } }
		public State State { get { throw new NotImplementedException (); } }

		public override string ToString ()
		{
			throw new NotImplementedException ();
		}
	}
}
