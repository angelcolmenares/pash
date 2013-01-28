using System;
using System.Reflection;
using System.Runtime;

namespace System.Management.Instrumentation
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field)]
	public class ManagedNameAttribute : Attribute
	{
		private string name;

		public string Name
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.name;
			}
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public ManagedNameAttribute(string name)
		{
			this.name = name;
		}

		internal static string GetBaseClassName(Type type)
		{
			InstrumentationClassAttribute attribute = InstrumentationClassAttribute.GetAttribute(type);
			string managedBaseClassName = attribute.ManagedBaseClassName;
			if (managedBaseClassName == null)
			{
				InstrumentationClassAttribute instrumentationClassAttribute = InstrumentationClassAttribute.GetAttribute(type.BaseType);
				if (instrumentationClassAttribute == null)
				{
					InstrumentationType instrumentationType = attribute.InstrumentationType;
					switch (instrumentationType)
					{
						case InstrumentationType.Instance:
						{
							return null;
						}
						case InstrumentationType.Event:
						{
							return "__ExtrinsicEvent";
						}
						case InstrumentationType.Abstract:
						{
							return null;
						}
					}
				}
				return ManagedNameAttribute.GetMemberName(type.BaseType);
			}
			else
			{
				return managedBaseClassName;
			}
		}

		internal static string GetMemberName(MemberInfo member)
		{
			object[] customAttributes = member.GetCustomAttributes(typeof(ManagedNameAttribute), false);
			if ((int)customAttributes.Length > 0)
			{
				ManagedNameAttribute managedNameAttribute = (ManagedNameAttribute)customAttributes[0];
				if (managedNameAttribute.name != null && managedNameAttribute.name.Length != 0)
				{
					return managedNameAttribute.name;
				}
			}
			return member.Name;
		}
	}
}