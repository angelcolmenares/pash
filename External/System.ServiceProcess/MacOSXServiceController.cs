//
// System.ServiceProcess.MacOSXServiceController
//
// Author:
//	Gert Driesen (drieseng@users.sourceforge.net)
//
// Copyright (C) 2006 Novell, Inc (http://www.novell.com)
//
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Text;


namespace System.ServiceProcess
{
	internal class MacOSXServiceController : ServiceControllerImpl
	{
		private const string SystemDaemonsPath = "/System/Library/LaunchDaemons";
		private const string UserLandDaemonsPath = "/Library/LaunchDaemons";
		private const string SystemAgentsPath = "/System/Library/LaunchAgents";
		private const string UserLandAgentsPath = "/Library/LaunchAgents";
		private const string SystemStartupItemsPath = "/System/Library/StartupItems";
		private const string UserStartupItemsPath = "/Library/StartupItems";

		public MacOSXServiceController (ServiceController serviceController)
			: base (serviceController)
		{
		}

		public override bool CanPauseAndContinue {
			get {
				return false;
			}
		}

		public override bool CanShutdown {
			get {
				return true;
			}
		}

		public override bool CanStop {
			get {
				return true;
			}
		}

		public override ServiceController [] DependentServices {
			get {
				return new ServiceController[0];
			}
		}

		public override string DisplayName {
			get {
				return ServiceController.Name;
			}
		}

		public override string ServiceName {
			get {
				return ServiceController.Name;
			}
		}

		public override ServiceController [] ServicesDependedOn {
			get {
				return new ServiceController[0];
			}
		}

		public override ServiceType ServiceType {
			get {
				return System.ServiceProcess.ServiceType.InteractiveProcess;
			}
		}

		public override ServiceControllerStatus Status {
			get {
				var lines = statusSb.ToString ().Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
				var svc = ServiceName;
				var foundLine = lines.FirstOrDefault (x => IsServiceLine (x, svc));

				if (!string.IsNullOrEmpty (foundLine))
				{
					string[] data = foundLine.Split (new char[] { '\t' }, StringSplitOptions.RemoveEmptyEntries);
					if (data.Length >= 2)
					{
						string status = data[1].Trim ();
						if (status == "0") return ServiceControllerStatus.Stopped;
						return ServiceControllerStatus.Running;
					}
				}
				return ServiceControllerStatus.Stopped;
			}
		}

		private static bool IsServiceLine (string obj, string svc)
		{
			string[] data = obj.Split (new char[] { '\t' }, StringSplitOptions.RemoveEmptyEntries);
			if (data.Length >= 3) {
				return data[2].Trim ().Equals (svc);
			}
			return false;
		}

		public override void Close ()
		{

		}

		public override void Continue ()
		{

		}

		public override void Dispose (bool disposing)
		{

		}

		public override void ExecuteCommand (int command)
		{

		}

		public override ServiceController [] GetDevices ()
		{
			return GetServices ();
		}

		private static StringBuilder statusSb = new StringBuilder();

		public override ServiceController [] GetServices ()
		{
			statusSb = new StringBuilder();
			var list = new List<ServiceController> ();
			var files = new List<string> ();
			var result = Process.Start (new ProcessStartInfo("launchctl", "list") { UseShellExecute = false, CreateNoWindow = true, RedirectStandardOutput = true });
			result.OutputDataReceived += (object sender, DataReceivedEventArgs e) => {
				statusSb.AppendLine(e.Data);
			};
			result.BeginOutputReadLine ();
			result.WaitForExit ();
			result.Dispose ();

			files.AddRange (Directory.GetFiles (SystemDaemonsPath, "*.plist", SearchOption.AllDirectories));
			files.AddRange (Directory.GetFiles (UserLandDaemonsPath, "*.plist", SearchOption.AllDirectories));
			files.AddRange (Directory.GetFiles (SystemAgentsPath, "*.plist", SearchOption.AllDirectories));
			files.AddRange (Directory.GetFiles (UserLandAgentsPath, "*.plist", SearchOption.AllDirectories));
			files.AddRange (Directory.GetFiles (SystemStartupItemsPath, "*.plist", SearchOption.AllDirectories));
			files.AddRange (Directory.GetFiles (UserStartupItemsPath, "*.plist", SearchOption.AllDirectories));

			foreach (var file in files) {
				FileInfo fi = new FileInfo(file);
				var svc = new ServiceController(fi.Name.Replace (fi.Extension, ""), "localhost");
				list.Add(svc);
			}

			return list.ToArray();
		}

		public override void Pause ()
		{

		}

		public override void Refresh ()
		{

		}

		public override void Start (string [] args)
		{
			var process = Process.Start (new ProcessStartInfo("launchctl", "start " + ServiceName) { UseShellExecute = true, CreateNoWindow = true });
			process.WaitForExit ();
		}

		public override void Stop ()
		{
			var process = Process.Start (new ProcessStartInfo("launchctl", "stop " + ServiceName) { UseShellExecute = true, CreateNoWindow = true });
			process.WaitForExit ();
		}
	}
}
