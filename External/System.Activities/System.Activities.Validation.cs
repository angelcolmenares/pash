using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Activities.Debugger;
using System.Activities.Expressions;
using System.Activities.Hosting;
using System.Activities.Statements;
using System.Windows.Markup;

namespace System.Activities.Validation
{
	public static class ActivityValidationServices
	{
	}
	
	public sealed class AddValidationError : NativeActivity
	{
		protected override void Execute (NativeActivityContext context)
		{
			throw new NotImplementedException ();
		}
	}
	
	public sealed class AssertValidation : NativeActivity
	{
		protected override void Execute (NativeActivityContext context)
		{
			throw new NotImplementedException ();
		}

		public bool IsWarning
		{
			get;set;
		}

		public string Message
		{
			get;set;
		}

		public InArgument<bool> Assertion
		{
			get;set;
		}
	}
	
	public abstract class Constraint : NativeActivity
	{
		protected override void Execute (NativeActivityContext context)
		{
			throw new NotImplementedException ();
		}
	}
	
	[ContentProperty ("Body")]
	public sealed class Constraint<T> : Constraint
	{
		public ActivityAction<T, ValidationContext> Body
		{
			get;set;
		}
	}
	
	public sealed class GetChildSubtree : CodeActivity<IEnumerable<Activity>>
	{
		protected override IEnumerable<Activity> Execute (CodeActivityContext context)
		{
			throw new NotImplementedException ();
		}
	}
	
	public sealed class GetParentChain : CodeActivity<IEnumerable<Activity>>
	{
		protected override IEnumerable<Activity> Execute (CodeActivityContext context)
		{
			throw new NotImplementedException ();
		}
	}
	
	public sealed class GetWorkflowTree : CodeActivity<IEnumerable<Activity>>
	{
		protected override IEnumerable<Activity> Execute (CodeActivityContext context)
		{
			throw new NotImplementedException ();
		}
	}
	
	public sealed class ValidationContext
	{
	}
	
	public class ValidationError
	{
	}
	
	public class ValidationResults
	{
	}
	
	public class ValidationSettings
	{
	}
}
