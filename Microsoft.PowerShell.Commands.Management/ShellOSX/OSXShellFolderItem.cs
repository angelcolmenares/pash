using System;
using Microsoft.PowerShell.Commands.Management;
using System.Diagnostics;
using System.IO;

namespace Shell32
{
	public class OSXShellFolderItem : ShellFolderItem
	{
		private string _path;
		private string _name;
		public OSXShellFolderItem (string path)
		{
			_path = path;
			var fi = new DirectoryInfo(_path);
			_name = fi.Name.Replace(".prefPane", "");
		}

		#region FolderItem2 implementation

		public string Path {
			get { return "General/" + _name; }
		}

		public string Name {
			get { return _name; }
			set { _name = value; }
		}

		public void _VtblGap1_2 ()
		{

		}

		public void _VtblGap2_10 ()
		{

		}

		public FolderItemVerbs Verbs ()
		{
			return new OSXFolderItemVerbs();
		}

		public void InvokeVerb (object vVerb)
		{
			var flag = false;
			if (vVerb == System.Reflection.Missing.Value)
				flag = true;
			else {
				var verb = vVerb as OSXFolderItemVerb;
				if (verb != null && verb.Name == ControlPanelResources.VerbActionOpen) {
					flag = true;
				}
			}
			if (flag) {
				Process.Start (new ProcessStartInfo("open", _path) { UseShellExecute = false, CreateNoWindow = false, RedirectStandardOutput = true });
			}
		}

		public void _VtblGap3_1 ()
		{

		}

		public object ExtendedProperty (string bstrPropName)
		{
			if (bstrPropName == "System.ApplicationName") return _name + '\0';
			if (bstrPropName == "System.ControlPanel.Category") return new int[] { 0 };
			return '\0';
		}

		#endregion
	}
}

