using System;

namespace Shell32
{
	public class OSXFolderItemVerb : FolderItemVerb
	{
		private string _name;

		public OSXFolderItemVerb (string name)
		{
			_name = name;
		}

		#region FolderItemVerb implementation

		public void _VtblGap1_2 ()
		{

		}

		public string Name {
			get { return _name; }
		}

		#endregion
	}
}

