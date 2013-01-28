using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Activities;
using System.Activities.Debugger;
using System.Activities.Hosting;
using System.Activities.Statements;
using System.Activities.XamlIntegration;
using System.Windows.Markup;

namespace System.Activities.ExpressionParser
{
	[SerializableAttribute]
	public class SourceExpressionException : Exception, ISerializable
	{
		public SourceExpressionException () : this ("Source expression error") {}
		public SourceExpressionException (string msg) : base (msg) {}
		public SourceExpressionException (string msg, Exception inner) : base (msg, inner) {}
		protected SourceExpressionException (SerializationInfo info, StreamingContext context) :
			base (info, context) {}

		public SourceExpressionException (string msg, CompilerErrorCollection errors)
		{
			throw new NotImplementedException ();
		}

		public IEnumerable<CompilerError> Errors { get { throw new NotImplementedException (); } }

		public override void GetObjectData (SerializationInfo info, StreamingContext context)
		{
			throw new NotImplementedException ();
		}
	}
}
