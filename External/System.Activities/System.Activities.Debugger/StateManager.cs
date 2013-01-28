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
	public sealed class StateManager : IDisposable
	{
		internal StateManager ()
		{
		}

		public void Dispose ()
		{
			throw new NotImplementedException ();
		}
		
		public void Exit (int threadIndex)
		{
			throw new NotImplementedException ();
		}
	}
}
