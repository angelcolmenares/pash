using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace System.Management.Classes
{
	[Guid("f011c6a6-cc5d-45f3-9f79-f768f6f18a50")]
	internal class UNIX_ComputerSystem : CIM_ManagedSystemElement
	{
		public UNIX_ComputerSystem ()
		{

		}

		protected override void RegisterProperies()
		{
			base.RegisterProperies ();
			RegisterProperty ("ComputerName", CimType.String, 0);
			RegisterProperty ("Description", CimType.String, 0);
			RegisterProperty ("Domain", CimType.String, 0);
		}

		protected override QueryParser Parser { 
			get { return new QueryParser<UNIX_ComputerSystem> (); } 
		}

		public override string PathField {
			get { return "ComputerName"; }
		}

		protected override IUnixWbemClassHandler Build (object nativeObj)
		{
			return base.Build (nativeObj).WithProperty (PathField, System.Net.Dns.GetHostName ())
				.WithMethod ("Rename", new UnixCimMethodInfo { Name = "Rename" })
					.WithProperty ("Description", OSHelper.GetComputerDescription ().ToString ())
					.WithProperty ("Domain", GetDomain());
		}

		public override IUnixWbemClassHandler InvokeMethod (string methodName, IUnixWbemClassHandler obj)
		{
			return base.InvokeMethod (methodName, obj);
		}

		public virtual void Rename(string name, string username, string password)
		{

		}

		public string GetDomain ()
		{
			string domain = "local";
			try {
				var domainProcess = Process.Start (new ProcessStartInfo ("bash", "-c \"hostname\"") { UseShellExecute = false, CreateNoWindow = true, RedirectStandardOutput = true, RedirectStandardError = true, RedirectStandardInput = true }); 
				bool received = false;
				DataReceivedEventHandler receivedHandler = (object sender, DataReceivedEventArgs e) => {
					if (!received) domain += e.Data;
					received = true;
				};
				domainProcess.OutputDataReceived += receivedHandler;
				domainProcess.BeginOutputReadLine ();
				domainProcess.WaitForExit ();
				domainProcess.OutputDataReceived -= receivedHandler;
				string[] domainParts = domain.Split (new char[] { '.' });
				if (domainParts.Length > 1) {
					domain = string.Join (".", domainParts, 1, domainParts.Length - 1);
				}
				domainProcess.Dispose ();
			} catch (Exception) {
				domain = "local";
			}
			return domain;
		}

		public string ComputerName { get { return GetPropertyAs<string>(PathField); } }

		public string Description { get { return GetPropertyAs<string> ("Description"); } }

		public string Domain { get { return GetPropertyAs<string>("Domain"); } }

	}
}

