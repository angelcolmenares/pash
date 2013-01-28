using System;

namespace Shell32
{
	public class OSXShell : Shell
	{
		public OSXShell ()
		{
		}

		#region IShellDispatch4 implementation

		public void _VtblGap1_2 ()
		{

		}

		public Folder NameSpace (object vDir)
		{
			string str = vDir as string;
			if (string.IsNullOrEmpty (str)) return new OSXFolder();
			if (str == "shell:::{26EE0668-A00A-44D7-9371-BEB064C98683}\\0") return new OSXFolder();
			if (str == "shell:::{26EE0668-A00A-44D7-9371-BEB064C98683}") return new OSXCategoryFolder();
			return new OSXFolder();
		}

		#endregion
	}
}

