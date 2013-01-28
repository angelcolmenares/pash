using System;
using Microsoft.PowerShell.Commands.Management;
using System.Diagnostics;
using System.IO;

namespace Shell32
{
	public class OSXCategoryFolderItem : ShellFolderItem
	{
		private string _name;
		public OSXCategoryFolderItem (string name)
		{
			_name = name;
		}
		
		#region FolderItem2 implementation
		
		public string Path {
			get { return _name; }
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

