using System;
using System.Collections;

namespace System.DirectoryServices.Protocols
{
	internal class ADSubstringFilter
	{
		public string Name;

		public ADValue Initial;

		public ADValue Final;

		public ArrayList Any;

		public ADSubstringFilter()
		{
			this.Initial = null;
			this.Final = null;
			this.Any = new ArrayList();
		}
	}
}