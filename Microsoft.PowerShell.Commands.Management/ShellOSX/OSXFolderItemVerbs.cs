using System;
using System.Collections.Generic;
using Microsoft.PowerShell.Commands.Management;

namespace Shell32
{
	public class OSXFolderItemVerbs : FolderItemVerbs
	{
		private IEnumerable<OSXFolderItemVerb> _verbs = new List<OSXFolderItemVerb>(new OSXFolderItemVerb[] { new OSXFolderItemVerb(ControlPanelResources.VerbActionOpen) });

		public OSXFolderItemVerbs ()
		{

		}

		#region FolderItemVerbs implementation

		public void _VtblGap1_4 ()
		{

		}

		public System.Collections.IEnumerator GetEnumerator ()
		{
			return _verbs.GetEnumerator ();
		}

		#endregion

		#region IEnumerable implementation

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator ()
		{
			return _verbs.GetEnumerator ();
		}

		#endregion
	}
}

