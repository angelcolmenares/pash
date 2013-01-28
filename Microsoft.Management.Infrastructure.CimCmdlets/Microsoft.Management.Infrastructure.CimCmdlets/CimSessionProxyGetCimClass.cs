using Microsoft.Management.Infrastructure;
using System;
using System.Management.Automation;
using Microsoft.Management.Infrastructure.Options;

namespace Microsoft.Management.Infrastructure.CimCmdlets
{
	internal class CimSessionProxyGetCimClass : CimSessionProxy
	{
		public CimSessionProxyGetCimClass(string computerName) : base(computerName)
		{

		}

		public CimSessionProxyGetCimClass(string computerName, CimSessionOptions options) : base(computerName, options)
		{

		}

		public CimSessionProxyGetCimClass(CimSession session) : base(session)
		{

		}

		protected override bool PreNewActionEvent(CmdletActionEventArgs args)
		{
			WildcardPattern wildcardPattern;
			DebugHelper.WriteLogEx();
			if (args.Action as CimWriteResultObject != null)
			{
				CimWriteResultObject action = args.Action as CimWriteResultObject;
				CimClass result = action.Result as CimClass;
				if (result != null)
				{
					object[] className = new object[1];
					className[0] = result.CimSystemProperties.ClassName;
					DebugHelper.WriteLog("class name = {0}", 1, className);
					CimGetCimClassContext contextObject = base.ContextObject as CimGetCimClassContext;
					if (WildcardPattern.ContainsWildcardCharacters(contextObject.ClassName))
					{
						wildcardPattern = new WildcardPattern(contextObject.ClassName, WildcardOptions.IgnoreCase);
						if (!wildcardPattern.IsMatch(result.CimSystemProperties.ClassName))
						{
							return false;
						}
					}
					if (contextObject.PropertyName != null)
					{
						wildcardPattern = new WildcardPattern(contextObject.PropertyName, WildcardOptions.IgnoreCase);
						bool flag = false;
						if (result.CimClassProperties != null)
						{
							foreach (CimPropertyDeclaration cimClassProperty in result.CimClassProperties)
							{
								object[] name = new object[1];
								name[0] = cimClassProperty.Name;
								DebugHelper.WriteLog("--- property name : {0}", 1, name);
								if (!wildcardPattern.IsMatch(cimClassProperty.Name))
								{
									continue;
								}
								flag = true;
								break;
							}
						}
						if (!flag)
						{
							object[] propertyName = new object[1];
							propertyName[0] = contextObject.PropertyName;
							DebugHelper.WriteLog("Property name does not match: {0}", 1, propertyName);
							return flag;
						}
					}
					if (contextObject.MethodName != null)
					{
						wildcardPattern = new WildcardPattern(contextObject.MethodName, WildcardOptions.IgnoreCase);
						bool flag1 = false;
						if (result.CimClassMethods != null)
						{
							foreach (CimMethodDeclaration cimClassMethod in result.CimClassMethods)
							{
								object[] objArray = new object[1];
								objArray[0] = cimClassMethod.Name;
								DebugHelper.WriteLog("--- method name : {0}", 1, objArray);
								if (!wildcardPattern.IsMatch(cimClassMethod.Name))
								{
									continue;
								}
								flag1 = true;
								break;
							}
						}
						if (!flag1)
						{
							object[] methodName = new object[1];
							methodName[0] = contextObject.MethodName;
							DebugHelper.WriteLog("Method name does not match: {0}", 1, methodName);
							return flag1;
						}
					}
					if (contextObject.QualifierName != null)
					{
						wildcardPattern = new WildcardPattern(contextObject.QualifierName, WildcardOptions.IgnoreCase);
						bool flag2 = false;
						if (result.CimClassQualifiers != null)
						{
							foreach (CimQualifier cimClassQualifier in result.CimClassQualifiers)
							{
								object[] name1 = new object[1];
								name1[0] = cimClassQualifier.Name;
								DebugHelper.WriteLog("--- qualifier name : {0}", 1, name1);
								if (!wildcardPattern.IsMatch(cimClassQualifier.Name))
								{
									continue;
								}
								flag2 = true;
								break;
							}
						}
						if (!flag2)
						{
							object[] qualifierName = new object[1];
							qualifierName[0] = contextObject.QualifierName;
							DebugHelper.WriteLog("Qualifer name does not match: {0}", 1, qualifierName);
							return flag2;
						}
					}
					object[] className1 = new object[1];
					className1[0] = result.CimSystemProperties.ClassName;
					DebugHelper.WriteLog("CimClass '{0}' is qulified.", 1, className1);
					return true;
				}
				else
				{
					return true;
				}
			}
			else
			{
				return true;
			}
		}
	}
}