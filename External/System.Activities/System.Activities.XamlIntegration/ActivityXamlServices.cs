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
	public static class ActivityXamlServices
	{
		public static XamlReader CreateBuilderReader (XamlReader innerReader)
		{
			throw new NotImplementedException ();
		}
		public static XamlReader CreateBuilderReader (XamlReader innerReader,XamlSchemaContext schemaContext)
		{
			throw new NotImplementedException ();
		}
		public static XamlWriter CreateBuilderWriter (XamlWriter innerWriter)
		{
			throw new NotImplementedException ();
		}
		public static XamlReader CreateReader (Stream stream)
		{
			throw new NotImplementedException ();
		}
		public static XamlReader CreateReader (XamlReader innerReader)
		{
			throw new NotImplementedException ();
		}
		public static XamlReader CreateReader (XamlReader innerReader,XamlSchemaContext schemaContext)
		{
			throw new NotImplementedException ();
		}
		public static Activity Load (Stream stream)
		{
			throw new NotImplementedException ();
		}
		public static Activity Load (string fileName)
		{
			throw new NotImplementedException ();
		}
		public static Activity Load (TextReader textReader)
		{
			throw new NotImplementedException ();
		}
		public static Activity Load (XamlReader xamlReader)
		{
			throw new NotImplementedException ();
		}
		public static Activity Load (XmlReader xmlReader)
		{
			throw new NotImplementedException ();
		}
	}
}
