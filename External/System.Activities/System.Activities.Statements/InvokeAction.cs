using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.Serialization;
using System.Transactions;
using System.Activities;
using System.Activities.Debugger;
using System.Activities.Expressions;
using System.Activities.Hosting;
using System.Windows.Markup;

namespace System.Activities.Statements
{
	[ContentProperty ("Action")]
	public sealed class InvokeAction : NativeActivity
	{
		public ActivityAction Action { get; set; }

		protected override void Execute (NativeActivityContext context)
		{
			throw new NotImplementedException ();
		}
	}
	
	[ContentProperty ("Action")]
	public sealed class InvokeAction<T> : NativeActivity
	{
		public ActivityAction<T> Action { get; set; }
		[RequiredArgumentAttribute]
		public InArgument<T> Argument { get; set; }

		protected override void Execute (NativeActivityContext context)
		{
			throw new NotImplementedException ();
		}
	}

	[ContentProperty ("Action")]
	public sealed class InvokeAction<T1, T2> : NativeActivity
	{
		[RequiredArgumentAttribute]
		public InArgument<T1> Argument1 { get; set; }
		[RequiredArgumentAttribute]
		public InArgument<T2> Argument2 { get; set; }

		protected override void Execute (NativeActivityContext context)
		{
			throw new NotImplementedException ();
		}
	}

	[ContentProperty ("Action")]
	public sealed class InvokeAction<T1, T2, T3> : NativeActivity
	{
		[RequiredArgumentAttribute]
		public InArgument<T1> Argument1 { get; set; }
		[RequiredArgumentAttribute]
		public InArgument<T2> Argument2 { get; set; }
		[RequiredArgumentAttribute]
		public InArgument<T3> Argument3 { get; set; }

		protected override void Execute (NativeActivityContext context)
		{
			throw new NotImplementedException ();
		}
	}

	[ContentProperty ("Action")]
	public sealed class InvokeAction<T1, T2, T3, T4> : NativeActivity
	{
		[RequiredArgumentAttribute]
		public InArgument<T1> Argument1 { get; set; }
		[RequiredArgumentAttribute]
		public InArgument<T2> Argument2 { get; set; }
		[RequiredArgumentAttribute]
		public InArgument<T3> Argument3 { get; set; }
		[RequiredArgumentAttribute]
		public InArgument<T4> Argument4 { get; set; }

		protected override void Execute (NativeActivityContext context)
		{
			throw new NotImplementedException ();
		}
	}

	[ContentProperty ("Action")]
	public sealed class InvokeAction<T1, T2, T3, T4, T5> : NativeActivity
	{
		[RequiredArgumentAttribute]
		public InArgument<T1> Argument1 { get; set; }
		[RequiredArgumentAttribute]
		public InArgument<T2> Argument2 { get; set; }
		[RequiredArgumentAttribute]
		public InArgument<T3> Argument3 { get; set; }
		[RequiredArgumentAttribute]
		public InArgument<T4> Argument4 { get; set; }
		[RequiredArgumentAttribute]
		public InArgument<T5> Argument5 { get; set; }

		protected override void Execute (NativeActivityContext context)
		{
			throw new NotImplementedException ();
		}
	}

	[ContentProperty ("Action")]
	public sealed class InvokeAction<T1, T2, T3, T4, T5, T6> : NativeActivity
	{
		[RequiredArgumentAttribute]
		public InArgument<T1> Argument1 { get; set; }
		[RequiredArgumentAttribute]
		public InArgument<T2> Argument2 { get; set; }
		[RequiredArgumentAttribute]
		public InArgument<T3> Argument3 { get; set; }
		[RequiredArgumentAttribute]
		public InArgument<T4> Argument4 { get; set; }
		[RequiredArgumentAttribute]
		public InArgument<T5> Argument5 { get; set; }
		[RequiredArgumentAttribute]
		public InArgument<T6> Argument6 { get; set; }

		protected override void Execute (NativeActivityContext context)
		{
			throw new NotImplementedException ();
		}
	}

	[ContentProperty ("Action")]
	public sealed class InvokeAction<T1, T2, T3, T4, T5, T6, T7> : NativeActivity
	{
		[RequiredArgumentAttribute]
		public InArgument<T1> Argument1 { get; set; }
		[RequiredArgumentAttribute]
		public InArgument<T2> Argument2 { get; set; }
		[RequiredArgumentAttribute]
		public InArgument<T3> Argument3 { get; set; }
		[RequiredArgumentAttribute]
		public InArgument<T4> Argument4 { get; set; }
		[RequiredArgumentAttribute]
		public InArgument<T5> Argument5 { get; set; }
		[RequiredArgumentAttribute]
		public InArgument<T6> Argument6 { get; set; }
		[RequiredArgumentAttribute]
		public InArgument<T7> Argument7 { get; set; }

		protected override void Execute (NativeActivityContext context)
		{
			throw new NotImplementedException ();
		}
	}

	[ContentProperty ("Action")]
	public sealed class InvokeAction<T1, T2, T3, T4, T5, T6, T7, T8> : NativeActivity
	{
		[RequiredArgumentAttribute]
		public InArgument<T1> Argument1 { get; set; }
		[RequiredArgumentAttribute]
		public InArgument<T2> Argument2 { get; set; }
		[RequiredArgumentAttribute]
		public InArgument<T3> Argument3 { get; set; }
		[RequiredArgumentAttribute]
		public InArgument<T4> Argument4 { get; set; }
		[RequiredArgumentAttribute]
		public InArgument<T5> Argument5 { get; set; }
		[RequiredArgumentAttribute]
		public InArgument<T6> Argument6 { get; set; }
		[RequiredArgumentAttribute]
		public InArgument<T7> Argument7 { get; set; }
		[RequiredArgumentAttribute]
		public InArgument<T8> Argument8 { get; set; }

		protected override void Execute (NativeActivityContext context)
		{
			throw new NotImplementedException ();
		}
	}

	[ContentProperty ("Action")]
	public sealed class InvokeAction<T1, T2, T3, T4, T5, T6, T7, T8, T9> : NativeActivity
	{
		[RequiredArgumentAttribute]
		public InArgument<T1> Argument1 { get; set; }
		[RequiredArgumentAttribute]
		public InArgument<T2> Argument2 { get; set; }
		[RequiredArgumentAttribute]
		public InArgument<T3> Argument3 { get; set; }
		[RequiredArgumentAttribute]
		public InArgument<T4> Argument4 { get; set; }
		[RequiredArgumentAttribute]
		public InArgument<T5> Argument5 { get; set; }
		[RequiredArgumentAttribute]
		public InArgument<T6> Argument6 { get; set; }
		[RequiredArgumentAttribute]
		public InArgument<T7> Argument7 { get; set; }
		[RequiredArgumentAttribute]
		public InArgument<T8> Argument8 { get; set; }
		[RequiredArgumentAttribute]
		public InArgument<T9> Argument9 { get; set; }

		protected override void Execute (NativeActivityContext context)
		{
			throw new NotImplementedException ();
		}
	}

	[ContentProperty ("Action")]
	public sealed class InvokeAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> : NativeActivity
	{
		[RequiredArgumentAttribute]
		public InArgument<T1> Argument1 { get; set; }
		[RequiredArgumentAttribute]
		public InArgument<T2> Argument2 { get; set; }
		[RequiredArgumentAttribute]
		public InArgument<T3> Argument3 { get; set; }
		[RequiredArgumentAttribute]
		public InArgument<T4> Argument4 { get; set; }
		[RequiredArgumentAttribute]
		public InArgument<T5> Argument5 { get; set; }
		[RequiredArgumentAttribute]
		public InArgument<T6> Argument6 { get; set; }
		[RequiredArgumentAttribute]
		public InArgument<T7> Argument7 { get; set; }
		[RequiredArgumentAttribute]
		public InArgument<T8> Argument8 { get; set; }
		[RequiredArgumentAttribute]
		public InArgument<T9> Argument9 { get; set; }
		[RequiredArgumentAttribute]
		public InArgument<T10> Argument10 { get; set; }

		protected override void Execute (NativeActivityContext context)
		{
			throw new NotImplementedException ();
		}
	}

	[ContentProperty ("Action")]
	public sealed class InvokeAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> : NativeActivity
	{
		[RequiredArgumentAttribute]
		public InArgument<T1> Argument1 { get; set; }
		[RequiredArgumentAttribute]
		public InArgument<T2> Argument2 { get; set; }
		[RequiredArgumentAttribute]
		public InArgument<T3> Argument3 { get; set; }
		[RequiredArgumentAttribute]
		public InArgument<T4> Argument4 { get; set; }
		[RequiredArgumentAttribute]
		public InArgument<T5> Argument5 { get; set; }
		[RequiredArgumentAttribute]
		public InArgument<T6> Argument6 { get; set; }
		[RequiredArgumentAttribute]
		public InArgument<T7> Argument7 { get; set; }
		[RequiredArgumentAttribute]
		public InArgument<T8> Argument8 { get; set; }
		[RequiredArgumentAttribute]
		public InArgument<T9> Argument9 { get; set; }
		[RequiredArgumentAttribute]
		public InArgument<T10> Argument10 { get; set; }
		[RequiredArgumentAttribute]
		public InArgument<T11> Argument11 { get; set; }

		protected override void Execute (NativeActivityContext context)
		{
			throw new NotImplementedException ();
		}
	}

	[ContentProperty ("Action")]
	public sealed class InvokeAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> : NativeActivity
	{
		[RequiredArgumentAttribute]
		public InArgument<T1> Argument1 { get; set; }
		[RequiredArgumentAttribute]
		public InArgument<T2> Argument2 { get; set; }
		[RequiredArgumentAttribute]
		public InArgument<T3> Argument3 { get; set; }
		[RequiredArgumentAttribute]
		public InArgument<T4> Argument4 { get; set; }
		[RequiredArgumentAttribute]
		public InArgument<T5> Argument5 { get; set; }
		[RequiredArgumentAttribute]
		public InArgument<T6> Argument6 { get; set; }
		[RequiredArgumentAttribute]
		public InArgument<T7> Argument7 { get; set; }
		[RequiredArgumentAttribute]
		public InArgument<T8> Argument8 { get; set; }
		[RequiredArgumentAttribute]
		public InArgument<T9> Argument9 { get; set; }
		[RequiredArgumentAttribute]
		public InArgument<T10> Argument10 { get; set; }
		[RequiredArgumentAttribute]
		public InArgument<T11> Argument11 { get; set; }
		[RequiredArgumentAttribute]
		public InArgument<T12> Argument12 { get; set; }

		protected override void Execute (NativeActivityContext context)
		{
			throw new NotImplementedException ();
		}
	}

	[ContentProperty ("Action")]
	public sealed class InvokeAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> : NativeActivity
	{
		[RequiredArgumentAttribute]
		public InArgument<T1> Argument1 { get; set; }
		[RequiredArgumentAttribute]
		public InArgument<T2> Argument2 { get; set; }
		[RequiredArgumentAttribute]
		public InArgument<T3> Argument3 { get; set; }
		[RequiredArgumentAttribute]
		public InArgument<T4> Argument4 { get; set; }
		[RequiredArgumentAttribute]
		public InArgument<T5> Argument5 { get; set; }
		[RequiredArgumentAttribute]
		public InArgument<T6> Argument6 { get; set; }
		[RequiredArgumentAttribute]
		public InArgument<T7> Argument7 { get; set; }
		[RequiredArgumentAttribute]
		public InArgument<T8> Argument8 { get; set; }
		[RequiredArgumentAttribute]
		public InArgument<T9> Argument9 { get; set; }
		[RequiredArgumentAttribute]
		public InArgument<T10> Argument10 { get; set; }
		[RequiredArgumentAttribute]
		public InArgument<T11> Argument11 { get; set; }
		[RequiredArgumentAttribute]
		public InArgument<T12> Argument12 { get; set; }
		[RequiredArgumentAttribute]
		public InArgument<T13> Argument13 { get; set; }

		protected override void Execute (NativeActivityContext context)
		{
			throw new NotImplementedException ();
		}
	}

	[ContentProperty ("Action")]
	public sealed class InvokeAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> : NativeActivity
	{
		[RequiredArgumentAttribute]
		public InArgument<T1> Argument1 { get; set; }
		[RequiredArgumentAttribute]
		public InArgument<T2> Argument2 { get; set; }
		[RequiredArgumentAttribute]
		public InArgument<T3> Argument3 { get; set; }
		[RequiredArgumentAttribute]
		public InArgument<T4> Argument4 { get; set; }
		[RequiredArgumentAttribute]
		public InArgument<T5> Argument5 { get; set; }
		[RequiredArgumentAttribute]
		public InArgument<T6> Argument6 { get; set; }
		[RequiredArgumentAttribute]
		public InArgument<T7> Argument7 { get; set; }
		[RequiredArgumentAttribute]
		public InArgument<T8> Argument8 { get; set; }
		[RequiredArgumentAttribute]
		public InArgument<T9> Argument9 { get; set; }
		[RequiredArgumentAttribute]
		public InArgument<T10> Argument10 { get; set; }
		[RequiredArgumentAttribute]
		public InArgument<T11> Argument11 { get; set; }
		[RequiredArgumentAttribute]
		public InArgument<T12> Argument12 { get; set; }
		[RequiredArgumentAttribute]
		public InArgument<T13> Argument13 { get; set; }
		[RequiredArgumentAttribute]
		public InArgument<T14> Argument14 { get; set; }

		protected override void Execute (NativeActivityContext context)
		{
			throw new NotImplementedException ();
		}
	}

	[ContentProperty ("Action")]
	public sealed class InvokeAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> : NativeActivity
	{
		[RequiredArgumentAttribute]
		public InArgument<T1> Argument1 { get; set; }
		[RequiredArgumentAttribute]
		public InArgument<T2> Argument2 { get; set; }
		[RequiredArgumentAttribute]
		public InArgument<T3> Argument3 { get; set; }
		[RequiredArgumentAttribute]
		public InArgument<T4> Argument4 { get; set; }
		[RequiredArgumentAttribute]
		public InArgument<T5> Argument5 { get; set; }
		[RequiredArgumentAttribute]
		public InArgument<T6> Argument6 { get; set; }
		[RequiredArgumentAttribute]
		public InArgument<T7> Argument7 { get; set; }
		[RequiredArgumentAttribute]
		public InArgument<T8> Argument8 { get; set; }
		[RequiredArgumentAttribute]
		public InArgument<T9> Argument9 { get; set; }
		[RequiredArgumentAttribute]
		public InArgument<T10> Argument10 { get; set; }
		[RequiredArgumentAttribute]
		public InArgument<T11> Argument11 { get; set; }
		[RequiredArgumentAttribute]
		public InArgument<T12> Argument12 { get; set; }
		[RequiredArgumentAttribute]
		public InArgument<T13> Argument13 { get; set; }
		[RequiredArgumentAttribute]
		public InArgument<T14> Argument14 { get; set; }
		[RequiredArgumentAttribute]
		public InArgument<T15> Argument15 { get; set; }

		protected override void Execute (NativeActivityContext context)
		{
			throw new NotImplementedException ();
		}
	}

	[ContentProperty ("Action")]
	public sealed class InvokeAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> : NativeActivity
	{
		[RequiredArgumentAttribute]
		public InArgument<T1> Argument1 { get; set; }
		[RequiredArgumentAttribute]
		public InArgument<T2> Argument2 { get; set; }
		[RequiredArgumentAttribute]
		public InArgument<T3> Argument3 { get; set; }
		[RequiredArgumentAttribute]
		public InArgument<T4> Argument4 { get; set; }
		[RequiredArgumentAttribute]
		public InArgument<T5> Argument5 { get; set; }
		[RequiredArgumentAttribute]
		public InArgument<T6> Argument6 { get; set; }
		[RequiredArgumentAttribute]
		public InArgument<T7> Argument7 { get; set; }
		[RequiredArgumentAttribute]
		public InArgument<T8> Argument8 { get; set; }
		[RequiredArgumentAttribute]
		public InArgument<T9> Argument9 { get; set; }
		[RequiredArgumentAttribute]
		public InArgument<T10> Argument10 { get; set; }
		[RequiredArgumentAttribute]
		public InArgument<T11> Argument11 { get; set; }
		[RequiredArgumentAttribute]
		public InArgument<T12> Argument12 { get; set; }
		[RequiredArgumentAttribute]
		public InArgument<T13> Argument13 { get; set; }
		[RequiredArgumentAttribute]
		public InArgument<T14> Argument14 { get; set; }
		[RequiredArgumentAttribute]
		public InArgument<T15> Argument15 { get; set; }
		[RequiredArgumentAttribute]
		public InArgument<T16> Argument16 { get; set; }

		protected override void Execute (NativeActivityContext context)
		{
			throw new NotImplementedException ();
		}
	}
}
