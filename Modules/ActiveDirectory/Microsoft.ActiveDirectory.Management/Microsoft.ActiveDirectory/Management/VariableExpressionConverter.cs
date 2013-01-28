using Microsoft.ActiveDirectory;
using System;
using System.Globalization;
using System.Management.Automation;
using System.Reflection;

namespace Microsoft.ActiveDirectory.Management
{
	internal class VariableExpressionConverter
	{
		private EvaluateVariableDelegate _variableConverter;

		internal VariableExpressionConverter(EvaluateVariableDelegate variableConverterDelegate)
		{
			this._variableConverter = variableConverterDelegate;
		}

		private object GetPropertyValue(object targetObject, string propertyName)
		{
			if (targetObject != null)
			{
				ADEntity aDEntity = targetObject as ADEntity;
				if (aDEntity == null)
				{
					PropertyInfo property = targetObject.GetType().GetProperty(propertyName);
					if (property != null)
					{
						return property.GetValue(targetObject, null);
					}
					else
					{
						object[] str = new object[2];
						str[0] = propertyName;
						str[1] = targetObject.GetType().ToString();
						throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, StringResources.ADFilterPropertyNotFoundInObject, str));
					}
				}
				else
				{
					return ADEntityAdapter.GetPropertyValue(aDEntity, propertyName);
				}
			}
			else
			{
				throw new ArgumentNullException("targetObject");
			}
		}

		public object GetVariableExpressionValue(string variableExpression)
		{
			object baseObject;
			int num = 0;
			string str;
			string str1;
			int num1 = variableExpression.IndexOf(".");
			if (num1 >= 0)
			{
				str = variableExpression.Substring(0, num1);
			}
			else
			{
				str = variableExpression;
			}
			string str2 = str;
			object obj = this._variableConverter(str2);
			if (obj != null)
			{
				PSObject pSObject = obj as PSObject;
				if (pSObject == null)
				{
					baseObject = obj;
				}
				else
				{
					baseObject = pSObject.BaseObject;
				}
				for (int i = num1; i >= 0; i = num)
				{
					num = variableExpression.IndexOf(".", i + 1);
					if (num >= 0)
					{
						str1 = variableExpression.Substring(i + 1, num - i - 1);
					}
					else
					{
						str1 = variableExpression.Substring(i + 1);
					}
					string str3 = str1;
					baseObject = this.GetPropertyValue(baseObject, str3);
				}
				return baseObject;
			}
			else
			{
				object[] objArray = new object[2];
				objArray[0] = str2;
				objArray[1] = variableExpression;
				throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, StringResources.ADFilterVariableNotDefinedMessage, objArray));
			}
		}
	}
}