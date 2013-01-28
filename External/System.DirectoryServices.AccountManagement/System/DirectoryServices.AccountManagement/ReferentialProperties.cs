using System.Collections;

namespace System.DirectoryServices.AccountManagement
{
	internal class ReferentialProperties
	{
		internal readonly static Hashtable Properties;

		static ReferentialProperties()
		{
			ReferentialProperties.Properties = new Hashtable();
			ArrayList arrayLists = new ArrayList(1);
			arrayLists.Add("GroupPrincipal.Members");
			ReferentialProperties.Properties[typeof(GroupPrincipal)] = arrayLists;
		}

		private ReferentialProperties()
		{
		}
	}
}