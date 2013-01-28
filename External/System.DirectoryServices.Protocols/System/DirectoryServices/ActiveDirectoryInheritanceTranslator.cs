using System;
using System.ComponentModel;
using System.Security.AccessControl;

namespace System.DirectoryServices
{
	internal sealed class ActiveDirectoryInheritanceTranslator
	{
		internal static InheritanceFlags[] ITToIF;

		internal static PropagationFlags[] ITToPF;

		static ActiveDirectoryInheritanceTranslator()
		{
			InheritanceFlags[] inheritanceFlagsArray = new InheritanceFlags[5];
			inheritanceFlagsArray[1] = InheritanceFlags.ContainerInherit;
			inheritanceFlagsArray[2] = InheritanceFlags.ContainerInherit;
			inheritanceFlagsArray[3] = InheritanceFlags.ContainerInherit;
			inheritanceFlagsArray[4] = InheritanceFlags.ContainerInherit;
			ActiveDirectoryInheritanceTranslator.ITToIF = inheritanceFlagsArray;
			PropagationFlags[] propagationFlagsArray = new PropagationFlags[5];
			propagationFlagsArray[2] = PropagationFlags.InheritOnly;
			propagationFlagsArray[3] = PropagationFlags.NoPropagateInherit;
			propagationFlagsArray[4] = PropagationFlags.NoPropagateInherit | PropagationFlags.InheritOnly;
			ActiveDirectoryInheritanceTranslator.ITToPF = propagationFlagsArray;
		}

		public ActiveDirectoryInheritanceTranslator()
		{
		}

		internal static ActiveDirectorySecurityInheritance GetEffectiveInheritanceFlags(InheritanceFlags inheritanceFlags, PropagationFlags propagationFlags)
		{
			ActiveDirectorySecurityInheritance activeDirectorySecurityInheritance = ActiveDirectorySecurityInheritance.None;
			if ((inheritanceFlags & InheritanceFlags.ContainerInherit) != InheritanceFlags.None)
			{
				PropagationFlags propagationFlag = propagationFlags;
				if (propagationFlag == PropagationFlags.None)
				{
					activeDirectorySecurityInheritance = ActiveDirectorySecurityInheritance.All;
					return activeDirectorySecurityInheritance;
				}
				else if (propagationFlag == PropagationFlags.NoPropagateInherit)
				{
					activeDirectorySecurityInheritance = ActiveDirectorySecurityInheritance.SelfAndChildren;
					return activeDirectorySecurityInheritance;
				}
				else if (propagationFlag == PropagationFlags.InheritOnly)
				{
					activeDirectorySecurityInheritance = ActiveDirectorySecurityInheritance.Descendents;
					return activeDirectorySecurityInheritance;
				}
				else if (propagationFlag == (PropagationFlags.NoPropagateInherit | PropagationFlags.InheritOnly))
				{
					activeDirectorySecurityInheritance = ActiveDirectorySecurityInheritance.Children;
					return activeDirectorySecurityInheritance;
				}
				throw new ArgumentException("propagationFlags");
			}
			return activeDirectorySecurityInheritance;
		}

		internal static InheritanceFlags GetInheritanceFlags(ActiveDirectorySecurityInheritance inheritanceType)
		{
			if (inheritanceType < ActiveDirectorySecurityInheritance.None || inheritanceType > ActiveDirectorySecurityInheritance.Children)
			{
				throw new InvalidEnumArgumentException("inheritanceType", (int)inheritanceType, typeof(ActiveDirectorySecurityInheritance));
			}
			else
			{
				return ActiveDirectoryInheritanceTranslator.ITToIF[(int)inheritanceType];
			}
		}

		internal static PropagationFlags GetPropagationFlags(ActiveDirectorySecurityInheritance inheritanceType)
		{
			if (inheritanceType < ActiveDirectorySecurityInheritance.None || inheritanceType > ActiveDirectorySecurityInheritance.Children)
			{
				throw new InvalidEnumArgumentException("inheritanceType", (int)inheritanceType, typeof(ActiveDirectorySecurityInheritance));
			}
			else
			{
				return ActiveDirectoryInheritanceTranslator.ITToPF[(int)inheritanceType];
			}
		}
	}
}