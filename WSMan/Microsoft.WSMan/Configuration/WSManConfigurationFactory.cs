using System;
using Microsoft.WSMan.Transfer;
using System.Collections.Generic;
using System.Configuration;
using System.Reflection;
using Microsoft.WSMan.Management;
using Microsoft.WSMan.Enumeration;
using Microsoft.WSMan.Eventing;

namespace Microsoft.WSMan.Configuration
{
	public static class WSManConfigurationFactory
	{
		private static WSManConfigurationSection _section;
		private static readonly object _lock = new object();

		static void EnsureSectionCreated ()
		{
			if (_section == null) {
				lock (_lock) {
					_section = (WSManConfigurationSection)ConfigurationManager.GetSection (WSManConfigurationSection.SectionName);
				}
			}
		}

		public static void Bind (BindManagementDelegate bindManagement, BindEnumerationDelegate bindEnumeration, BindPullEventingDelegate bindPullEventing)
		{
			BindManagementHandlers(bindManagement);
			BindEnumerationHandlers (bindEnumeration);
			BindEventHandlers(bindPullEventing);
		}

		public static void BindManagementHandlers (BindManagementDelegate bindManagement)
		{
			EnsureSectionCreated ();
			if (_section.ManagementHandlers == null) return;
			foreach (ManagementConfigurationElement element in _section.ManagementHandlers) 
			{
				try 
				{
					Type type = Type.GetType (element.HandlerType);
					object instance = Activator.CreateInstance (type);
					bindManagement(new Uri(element.ResourceUri), (IManagementRequestHandler)instance);
				}
				finally
				{

				}
			}
		}

		public static void BindEventHandlers (BindPullEventingDelegate delegateEventing)
		{
			EnsureSectionCreated ();
			if (_section.EventHandlers == null) return;
			foreach (EventingConfigurationElement element in _section.EventHandlers) 
			{
				try 
				{
					Type type = Type.GetType (element.HandlerType);
					object instance = Activator.CreateInstance (type);
					string dialect = element.Dialect;
					Type filterType = Type.GetType (element.FilterType);
					Uri deliveryUri = new Uri(element.DeliveryUri);
					delegateEventing(new Uri(element.ResourceUri), dialect, filterType, (IEventingRequestHandler)instance, deliveryUri);
				}
				finally
				{
					
				}
			}
		}

		public static void BindEnumerationHandlers (BindEnumerationDelegate delegateEnumeration)
		{
			EnsureSectionCreated ();
			if (_section.EnumerationHandlers == null) return;
			foreach (EnumerationConfigurationElement element in _section.EnumerationHandlers) 
			{
				try 
				{
					Type type = Type.GetType (element.HandlerType);
					object instance = Activator.CreateInstance (type);
					string dialect = element.Dialect;
					Type filterType = Type.GetType (element.FilterType);
					delegateEnumeration(new Uri(element.ResourceUri), dialect, filterType, (IEnumerationRequestHandler)instance);
				}
				finally
				{
					
				}
			}
		}


	}
}

