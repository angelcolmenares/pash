using System;

namespace FacebookPSModule
{
	public class FacebookOAuthManager
	{
		public FacebookOAuthManager ()
		{
			var browser = Mono.WebBrowser.Manager.GetNewInstance (Mono.WebBrowser.Platform.Gtk);
			browser.NavigationRequested += HandleNavigationRequested;
		}

		void HandleNavigationRequested (object sender, Mono.WebBrowser.NavigationRequestedEventArgs e)
		{

		}
	}
}

