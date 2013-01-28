using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.DurableInstancing;
using System.Runtime.Serialization;
using System.Transactions;
using System.Windows.Markup;
using System.Xaml;
using System.Xml.Linq;
using System.Activities.Debugger;
using System.Activities.Expressions;
using System.Activities.Hosting;
using System.Activities.Statements;
using System.Activities.Tracking;
using System.Activities.Validation;

namespace System.Activities
{
	public sealed class RuntimeDelegateArgument
	{
		public RuntimeDelegateArgument (string name, Type type, ArgumentDirection direction, DelegateArgument boundArgument)
		{
			Name = name;
			Type = type;
			Direction = direction;
			BoundArgument = boundArgument;
		}

		public DelegateArgument BoundArgument { get; private set; }
		public ArgumentDirection Direction { get; private set; }
		public string Name { get; private set; }
		public Type Type { get; private set; }
	}
}
