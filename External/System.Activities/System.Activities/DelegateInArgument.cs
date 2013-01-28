namespace System.Activities
{
	public abstract class DelegateInArgument : DelegateArgument
	{
		internal DelegateInArgument ()
		{
		}
	}

	public sealed class DelegateInArgument<T> : DelegateInArgument
	{
		public DelegateInArgument (string name)
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
