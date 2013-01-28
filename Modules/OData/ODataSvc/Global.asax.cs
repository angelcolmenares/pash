
using System;
using System.Collections;
using System.ComponentModel;
using System.Web;
using System.Web.SessionState;
using System.Reflection;
using System.Web.Compilation;
using Microsoft.Management.Odata;
using System.Data.Services;
using System.Configuration;

namespace ODataSvc
{
	public class Global : System.Web.HttpApplication
	{
		/* <%@ ServiceHost Language="C#" Debug="true" Factory="System.Data.Services.DataServiceHostFactory, System.Data.Services, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" Service="Microsoft.Management.Odata.DataService, Microsoft.Management.Odata, Version=3.0.0.0, Culture=neutral, PublicKeyToken=0738eb9f132ed756" %> */


		public Global()
		{
			var root = ConfigurationManager.AppSettings["RootPath"];
			AppDomain.CurrentDomain.AssemblyResolve += (object sender, ResolveEventArgs args) => {
				var assemblyName = new System.Reflection.AssemblyName(args.Name);
				string path = System.IO.Path.Combine (root, assemblyName.Name + ".dll");
				if (System.IO.File.Exists (path))
				{
					return System.Reflection.Assembly.LoadFile (path);
				}
				return null;
			};
		}

		protected virtual void Application_Start (Object sender, EventArgs e)
		{

		}
		
		protected virtual void Session_Start (Object sender, EventArgs e)
		{
		}
		
		protected virtual void Application_BeginRequest (Object sender, EventArgs e)
		{
			HttpContext.Current.Response.DisableKernelCache ();
		}
		
		protected virtual void Application_EndRequest (Object sender, EventArgs e)
		{
		}
		
		protected virtual void Application_AuthenticateRequest (Object sender, EventArgs e)
		{
		}
		
		protected virtual void Application_Error (Object sender, EventArgs e)
		{
		}
		
		protected virtual void Session_End (Object sender, EventArgs e)
		{
		}
		
		protected virtual void Application_End (Object sender, EventArgs e)
		{
		}
	}
}

