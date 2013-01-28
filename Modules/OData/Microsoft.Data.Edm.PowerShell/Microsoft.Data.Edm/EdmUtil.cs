using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Microsoft.Data.Edm
{
	internal static class EdmUtil
	{
		private const string StartCharacterExp = "[\\p{Ll}\\p{Lu}\\p{Lt}\\p{Lo}\\p{Lm}\\p{Nl}]";

		private const string OtherCharacterExp = "[\\p{Ll}\\p{Lu}\\p{Lt}\\p{Lo}\\p{Lm}\\p{Nl}\\p{Mn}\\p{Mc}\\p{Nd}\\p{Pc}\\p{Cf}]";

		private const string NameExp = "[\\p{Ll}\\p{Lu}\\p{Lt}\\p{Lo}\\p{Lm}\\p{Nl}][\\p{Ll}\\p{Lu}\\p{Lt}\\p{Lo}\\p{Lm}\\p{Nl}\\p{Mn}\\p{Mc}\\p{Nd}\\p{Pc}\\p{Cf}]{0,}";

		private static Regex UndottedNameValidator;

		static EdmUtil()
		{
			EdmUtil.UndottedNameValidator = new Regex("^[\\p{Ll}\\p{Lu}\\p{Lt}\\p{Lo}\\p{Lm}\\p{Nl}][\\p{Ll}\\p{Lu}\\p{Lt}\\p{Lo}\\p{Lm}\\p{Nl}\\p{Mn}\\p{Mc}\\p{Nd}\\p{Pc}\\p{Cf}]{0,}$", RegexOptions.Compiled | RegexOptions.Singleline);
		}

		public static T CheckArgumentNull<T>(T value, string parameterName)
		where T : class
		{
			if (value != null)
			{
				return value;
			}
			else
			{
				throw new ArgumentNullException(parameterName);
			}
		}

		public static bool EqualsOrdinal(this string string1, string string2)
		{
			return string.Equals(string1, string2, StringComparison.Ordinal);
		}

		public static bool EqualsOrdinalIgnoreCase(this string string1, string string2)
		{
			return string.Equals(string1, string2, StringComparison.OrdinalIgnoreCase);
		}

		public static string FullyQualifiedName(IEdmVocabularyAnnotatable element)
		{
			IEdmSchemaElement edmSchemaElement = element as IEdmSchemaElement;
			if (edmSchemaElement == null)
			{
				IEdmEntityContainerElement edmEntityContainerElement = element as IEdmEntityContainerElement;
				if (edmEntityContainerElement == null)
				{
					IEdmProperty edmProperty = element as IEdmProperty;
					if (edmProperty == null)
					{
						IEdmFunctionParameter edmFunctionParameter = element as IEdmFunctionParameter;
						if (edmFunctionParameter != null)
						{
							string str = EdmUtil.FullyQualifiedName(edmFunctionParameter.DeclaringFunction);
							if (str != null)
							{
								return string.Concat(str, "/", edmFunctionParameter.Name);
							}
						}
					}
					else
					{
						IEdmSchemaType declaringType = edmProperty.DeclaringType as IEdmSchemaType;
						if (declaringType != null)
						{
							string str1 = EdmUtil.FullyQualifiedName(declaringType);
							if (str1 != null)
							{
								return string.Concat(str1, "/", edmProperty.Name);
							}
						}
					}
					return null;
				}
				else
				{
					IEdmFunctionImport edmFunctionImport = edmEntityContainerElement as IEdmFunctionImport;
					if (edmFunctionImport == null)
					{
						return string.Concat(edmEntityContainerElement.Container.FullName(), "/", edmEntityContainerElement.Name);
					}
					else
					{
						return string.Concat(edmFunctionImport.Container.FullName(), "/", EdmUtil.ParameterizedName(edmFunctionImport));
					}
				}
			}
			else
			{
				IEdmFunction edmFunction = edmSchemaElement as IEdmFunction;
				if (edmFunction == null)
				{
					return edmSchemaElement.FullName();
				}
				else
				{
					return EdmUtil.ParameterizedName(edmFunction);
				}
			}
		}

		public static bool IsNullOrWhiteSpaceInternal(string value)
		{
			if (value == null)
			{
				return true;
			}
			else
			{
				return value.All<char>(new Func<char, bool>(char.IsWhiteSpace));
			}
		}

		public static bool IsQualifiedName(string name)
		{
			char[] chrArray = new char[1];
			chrArray[0] = '.';
			string[] strArrays = name.Split(chrArray);
			if (strArrays.Count<string>() >= 2)
			{
				string[] strArrays1 = strArrays;
				int num = 0;
				while (num < (int)strArrays1.Length)
				{
					string str = strArrays1[num];
					if (!EdmUtil.IsNullOrWhiteSpaceInternal(str))
					{
						num++;
					}
					else
					{
						bool flag = false;
						return flag;
					}
				}
				return true;
			}
			else
			{
				return false;
			}
		}

		public static bool IsValidDottedName(string name)
		{
			char[] chrArray = new char[1];
			chrArray[0] = '.';
			return name.Split(chrArray).All<string>(new Func<string, bool>(EdmUtil.IsValidUndottedName));
		}

		public static bool IsValidUndottedName(string name)
		{
			if (string.IsNullOrEmpty(name))
			{
				return false;
			}
			else
			{
				return EdmUtil.UndottedNameValidator.IsMatch(name);
			}
		}

		public static string JoinInternal<T>(string separator, IEnumerable<T> values)
		{
			string str;
			if (values != null)
			{
				if (separator == null)
				{
					separator = string.Empty;
				}
				IEnumerator<T> enumerator = values.GetEnumerator();
				using (enumerator)
				{
					if (enumerator.MoveNext())
					{
						StringBuilder stringBuilder = new StringBuilder();
						if (enumerator.Current != null)
						{
							T current = enumerator.Current;
							string str1 = current.ToString();
							if (str1 != null)
							{
								stringBuilder.Append(str1);
							}
						}
						while (enumerator.MoveNext())
						{
							stringBuilder.Append(separator);
							if (enumerator.Current == null)
							{
								continue;
							}
							T t = enumerator.Current;
							string str2 = t.ToString();
							if (str2 == null)
							{
								continue;
							}
							stringBuilder.Append(str2);
						}
						str = stringBuilder.ToString();
					}
					else
					{
						str = string.Empty;
					}
				}
				return str;
			}
			else
			{
				throw new ArgumentNullException("values");
			}
		}

		public static string ParameterizedName(IEdmFunctionBase function)
		{
			string str;
			int num = 0;
			int num1 = function.Parameters.Count<IEdmFunctionParameter>();
			StringBuilder stringBuilder = new StringBuilder();
			IEdmSchemaElement edmSchemaElement = function as IEdmSchemaElement;
			if (edmSchemaElement != null)
			{
				stringBuilder.Append(edmSchemaElement.Namespace);
				stringBuilder.Append(".");
			}
			stringBuilder.Append(function.Name);
			stringBuilder.Append("(");
			foreach (IEdmFunctionParameter parameter in function.Parameters)
			{
				if (!parameter.Type.IsCollection())
				{
					if (!parameter.Type.IsEntityReference())
					{
						str = parameter.Type.FullName();
					}
					else
					{
						str = string.Concat("Ref(", parameter.Type.AsEntityReference().EntityType().FullName(), ")");
					}
				}
				else
				{
					str = string.Concat("Collection(", parameter.Type.AsCollection().ElementType().FullName(), ")");
				}
				stringBuilder.Append(str);
				num++;
				if (num >= num1)
				{
					continue;
				}
				stringBuilder.Append(", ");
			}
			stringBuilder.Append(")");
			return stringBuilder.ToString();
		}

		public static bool TryGetNamespaceNameFromQualifiedName(string qualifiedName, out string namespaceName, out string name)
		{
			int num = qualifiedName.LastIndexOf('.');
			if (num >= 0)
			{
				namespaceName = qualifiedName.Substring(0, num);
				name = qualifiedName.Substring(num + 1);
				return true;
			}
			else
			{
				namespaceName = string.Empty;
				name = qualifiedName;
				return false;
			}
		}

		private sealed class ValidatedNotNullAttribute : Attribute
		{
			public ValidatedNotNullAttribute()
			{
			}
		}
	}
}