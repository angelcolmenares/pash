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
	public class LocalsItemDescription
	{
		public LocalsItemDescription (string name, Type type)
		{
			throw new NotImplementedException ();
		}

		public string Name { get; private set; }
		public Type Type { get; private set; }

		public override string ToString ()
		{
			throw new NotImplementedException ();
		}
	}
}
