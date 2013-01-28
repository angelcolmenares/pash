using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Xaml;
using System.Xml;
using System.Activities;
using System.Activities.Debugger;
using System.Activities.Expressions;
using System.Activities.Hosting;
using System.Activities.Statements;
using System.Windows.Markup;

namespace System.Activities.XamlIntegration
{
	public class FuncDeferringLoader : XamlDeferringLoader
	{
		public override object Load (XamlReader xamlReader, IServiceProvider context)
		{
			throw new NotImplementedException ();
		}
		public override XamlReader Save (object value, IServiceProvider serviceProvider)
		{
			throw new NotImplementedException ();
		}
	}
}
