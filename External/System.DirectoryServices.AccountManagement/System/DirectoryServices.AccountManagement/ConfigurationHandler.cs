using System;
using System.Collections;
using System.Configuration;
using System.Globalization;
using System.Xml;

namespace System.DirectoryServices.AccountManagement
{
	internal class ConfigurationHandler : IConfigurationSectionHandler
	{
		public ConfigurationHandler()
		{
		}

		public virtual object Create(object parent, object configContext, XmlNode section)
		{
			ConfigSettings configSetting;
			bool flag = false;
			Enum @enum = DebugLevel.None;
			string str = null;
			IEnumerator enumerator = section.ChildNodes.GetEnumerator();
			try
			{
				if (enumerator.MoveNext())
				{
					XmlNode current = (XmlNode)enumerator.Current;
					object[] name = new object[1];
					name[0] = current.Name;
					throw new ConfigurationErrorsException(string.Format(CultureInfo.CurrentCulture, StringResources.ConfigHandlerUnknownConfigSection, name));
				}
			}
			finally
			{
				IDisposable disposable = enumerator as IDisposable;
				if (disposable != null)
				{
					disposable.Dispose();
				}
			}
			if (!flag)
			{
				configSetting = new ConfigSettings();
			}
			else
			{
				configSetting = new ConfigSettings((DebugLevel)@enum, str);
			}
			return configSetting;
		}
	}
}