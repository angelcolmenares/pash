using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.Serialization;
using System.Activities;
using System.Activities.Debugger;
using System.Activities.Hosting;
using System.Activities.Statements;
using System.Activities.XamlIntegration;
using System.Windows.Markup;

namespace System.Activities.Expressions
{
	public sealed class OrElse : Activity<bool>
	{
		[RequiredArgumentAttribute]
		public Activity<bool> Left { get; set; }
		[RequiredArgumentAttribute]
		public Activity<bool> Right { get; set; }
	}
}
