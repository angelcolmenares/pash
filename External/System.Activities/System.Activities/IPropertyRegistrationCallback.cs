namespace System.Activities
{
	public interface IPropertyRegistrationCallback
	{
		void Register (RegistrationContext context);
		void Unregister (RegistrationContext context);
	}
}
