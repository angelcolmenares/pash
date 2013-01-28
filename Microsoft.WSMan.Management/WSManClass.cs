using System.Runtime.InteropServices;

namespace Microsoft.WSMan.Management
{
	[ClassInterface(ClassInterfaceType.AutoDual)]
	[Guid("BCED617B-EC03-420b-8508-977DC7A686BD")]
	public class WSManClass : IWSManEx
	{
        public WSManClass()
        {

        }

		#region IWSManEx implementation

		public object CreateConnectionOptions ()
		{
			return new WSManConnectionOptions();
		}

		public object CreateResourceLocator (string strResourceLocator)
		{
			return new WSManResourceLocator();
		}

		public object CreateSession (string connection, int flags, object connectionOptions)
		{
			throw new System.NotImplementedException ();
		}

		public int EnumerationFlagAssociatedInstance ()
		{
			throw new System.NotImplementedException ();
		}

		public int EnumerationFlagAssociationInstance ()
		{
			throw new System.NotImplementedException ();
		}

		public int EnumerationFlagHierarchyDeep ()
		{
			throw new System.NotImplementedException ();
		}

		public int EnumerationFlagHierarchyDeepBasePropsOnly ()
		{
			throw new System.NotImplementedException ();
		}

		public int EnumerationFlagHierarchyShallow ()
		{
			throw new System.NotImplementedException ();
		}

		public int EnumerationFlagNonXmlText ()
		{
			throw new System.NotImplementedException ();
		}

		public int EnumerationFlagReturnEPR ()
		{
			throw new System.NotImplementedException ();
		}

		public int EnumerationFlagReturnObject ()
		{
			throw new System.NotImplementedException ();
		}

		public int EnumerationFlagReturnObjectAndEPR ()
		{
			throw new System.NotImplementedException ();
		}

		public string GetErrorMessage (uint errorNumber)
		{
			throw new System.NotImplementedException ();
		}

		public int SessionFlagCredUsernamePassword ()
		{
			throw new System.NotImplementedException ();
		}

		public int SessionFlagEnableSPNServerPort ()
		{
			throw new System.NotImplementedException ();
		}

		public int SessionFlagNoEncryption ()
		{
			throw new System.NotImplementedException ();
		}

		public int SessionFlagSkipCACheck ()
		{
			throw new System.NotImplementedException ();
		}

		public int SessionFlagSkipCNCheck ()
		{
			throw new System.NotImplementedException ();
		}

		public int SessionFlagUseBasic ()
		{
			throw new System.NotImplementedException ();
		}

		public int SessionFlagUseDigest ()
		{
			throw new System.NotImplementedException ();
		}

		public int SessionFlagUseKerberos ()
		{
			throw new System.NotImplementedException ();
		}

		public int SessionFlagUseNegotiate ()
		{
			throw new System.NotImplementedException ();
		}

		public int SessionFlagUseNoAuthentication ()
		{
			throw new System.NotImplementedException ();
		}

		public int SessionFlagUTF8 ()
		{
			throw new System.NotImplementedException ();
		}

		public string CommandLine {
			get;
			private set;
		}

		public string Error {
			get;
			private set;
		}

		#endregion
	}
}