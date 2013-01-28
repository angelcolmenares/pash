using System;

namespace System.Management
{
	internal class SecuredIEnumWbemClassObjectHandler
	{
		private IEnumWbemClassObject pEnumWbemClassObjectsecurityHelper;

		private ManagementScope scope;

		internal SecuredIEnumWbemClassObjectHandler(ManagementScope theScope, IEnumWbemClassObject pEnumWbemClassObject)
		{
			this.scope = theScope;
			this.pEnumWbemClassObjectsecurityHelper = pEnumWbemClassObject;
		}

		internal int Clone_(ref IEnumWbemClassObject ppEnum)
		{
			int num = -2147217407;
			if (this.scope != null)
			{
				num = WmiNetUtilsHelper.CloneEnumWbemClassObject_f(out ppEnum, (int)this.scope.Options.Authentication, (int)this.scope.Options.Impersonation, this.pEnumWbemClassObjectsecurityHelper, this.scope.Options.Username, this.scope.Options.GetPassword(), this.scope.Options.Authority);
			}
			return num;
		}

		internal int Next_(int lTimeout, int uCount, IWbemClassObject_DoNotMarshal[] ppOutParams, ref uint puReturned)
		{
			int num = this.pEnumWbemClassObjectsecurityHelper.Next_(lTimeout, uCount, ppOutParams, out puReturned);
			return num;
		}

		internal int NextAsync_(uint uCount, IWbemObjectSink pSink)
		{
			int num = this.pEnumWbemClassObjectsecurityHelper.NextAsync_(uCount, pSink);
			return num;
		}

		internal int Reset_()
		{
			int num = this.pEnumWbemClassObjectsecurityHelper.Reset_();
			return num;
		}

		internal int Skip_(int lTimeout, uint nCount)
		{
			int num = this.pEnumWbemClassObjectsecurityHelper.Skip_(lTimeout, nCount);
			return num;
		}
	}
}