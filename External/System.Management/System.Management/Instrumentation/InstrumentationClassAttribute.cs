using System;
using System.Runtime;

namespace System.Management.Instrumentation
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
	public class InstrumentationClassAttribute : Attribute
	{
		private InstrumentationType instrumentationType;

		private string managedBaseClassName;

		public InstrumentationType InstrumentationType
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.instrumentationType;
			}
		}

		public string ManagedBaseClassName
		{
			get
			{
				if (this.managedBaseClassName == null || this.managedBaseClassName.Length == 0)
				{
					return null;
				}
				else
				{
					return this.managedBaseClassName;
				}
			}
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public InstrumentationClassAttribute(InstrumentationType instrumentationType)
		{
			this.instrumentationType = instrumentationType;
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public InstrumentationClassAttribute(InstrumentationType instrumentationType, string managedBaseClassName)
		{
			this.instrumentationType = instrumentationType;
			this.managedBaseClassName = managedBaseClassName;
		}

		internal static InstrumentationClassAttribute GetAttribute(Type type)
		{
			if (type == typeof(BaseEvent) || type == typeof(Instance))
			{
				return null;
			}
			else
			{
				object[] customAttributes = type.GetCustomAttributes(typeof(InstrumentationClassAttribute), true);
				if ((int)customAttributes.Length <= 0)
				{
					return null;
				}
				else
				{
					return (InstrumentationClassAttribute)customAttributes[0];
				}
			}
		}

		internal static Type GetBaseInstrumentationType(Type type)
		{
			if (InstrumentationClassAttribute.GetAttribute(type.BaseType) == null)
			{
				return null;
			}
			else
			{
				return type.BaseType;
			}
		}
	}
}