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
	public class XamlDebuggerXmlReader : XamlReader, IXamlLineInfo
	{
		public XamlDebuggerXmlReader (XamlReader underlyingReader, TextReader textReader)
		{
			throw new NotImplementedException ();
		}
		public XamlDebuggerXmlReader (XamlReader underlyingReader, IXamlLineInfo xamlLineInfo, TextReader textReader)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static readonly AttachableMemberIdentifier EndColumnName = null;
		[MonoTODO]
		public static readonly AttachableMemberIdentifier EndLineName = null;
		[MonoTODO]
		public static readonly AttachableMemberIdentifier FileNameName = null;
		[MonoTODO]
		public static readonly AttachableMemberIdentifier StartColumnName = null;
		[MonoTODO]
		public static readonly AttachableMemberIdentifier StartLineName = null;

		public bool HasLineInfo { get { throw new NotImplementedException (); } }
		public override bool IsEof { get { throw new NotImplementedException (); } }
		public int LineNumber { get { throw new NotImplementedException (); } }
		public int LinePosition { get { throw new NotImplementedException (); } }
		public override XamlMember Member { get { throw new NotImplementedException (); } }
		public override NamespaceDeclaration Namespace { get { throw new NotImplementedException (); } }
		public override XamlNodeType NodeType { get { throw new NotImplementedException (); } }
		public override XamlSchemaContext SchemaContext { get { throw new NotImplementedException (); } }
		public override XamlType Type { get { throw new NotImplementedException (); } }
		public override object Value { get { throw new NotImplementedException (); } }

		public static void CopyAttachedSourceLocation (object source,object destination)
		{
			throw new NotImplementedException ();
		}
		public static object GetEndColumn (object instance)
		{
			throw new NotImplementedException ();
		}
		public static object GetEndLine (object instance)
		{
			throw new NotImplementedException ();
		}
		public static object GetFileName (object instance)
		{
			throw new NotImplementedException ();
		}
		public static object GetStartColumn (object instance)
		{
			throw new NotImplementedException ();
		}
		public static object GetStartLine (object instance)
		{
			throw new NotImplementedException ();
		}
		public override bool Read ()
		{
			throw new NotImplementedException ();
		}
		public static void SetEndColumn (object instance,object value)
		{
			throw new NotImplementedException ();
		}
		public static void SetEndLine (object instance,object value)
		{
			throw new NotImplementedException ();
		}
		public static void SetFileName (object instance,object value)
		{
			throw new NotImplementedException ();
		}
		public static void SetStartColumn (object instance,object value)
		{
			throw new NotImplementedException ();
		}
		public static void SetStartLine (object instance,object value)
		{
			throw new NotImplementedException ();
		}
	}
}
