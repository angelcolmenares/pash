namespace System.Activities
{
	public delegate void FaultCallback (NativeActivityFaultContext faultContext, Exception propagatedException, ActivityInstance propagatedFrom);
}
