namespace System.Activities
{
	public abstract class DelegateOutArgument : DelegateArgument
	{
		internal DelegateOutArgument ()
		{
		}
	}
	
	public sealed class DelegateOutArgument<T> : DelegateArgument
	{
		public DelegateOutArgument (string name)
		{
			throw new NotImplementedException ();
		}

		protected override Type TypeCore {
			get { throw new NotImplementedException (); }
		}

		public new T Get (ActivityContext context)
		{
			throw new NotImplementedException ();
		}
		public void Set (ActivityContext context, T value)
		{
			throw new NotImplementedException ();
		}
	}
}
