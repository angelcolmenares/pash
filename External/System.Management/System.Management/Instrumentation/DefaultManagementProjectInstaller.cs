using System.Configuration.Install;

namespace System.Management.Instrumentation
{
	public class DefaultManagementProjectInstaller : Installer
	{
		public DefaultManagementProjectInstaller()
		{
			ManagementInstaller managementInstaller = new ManagementInstaller();
			base.Installers.Add(managementInstaller);
		}
	}
}