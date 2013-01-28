using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Internal;
using Microsoft.Data.Edm.Validation;
using Microsoft.Data.Edm.Values;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;

namespace Microsoft.Data.Edm.Validation.Internal
{
	internal static class ValidationHelper
	{
		internal static bool AddMemberNameToHashSet(IEdmNamedElement item, HashSetInternal<string> memberNameList, ValidationContext context, EdmErrorCode errorCode, string errorString, bool suppressError)
		{
			string name;
			IEdmSchemaElement edmSchemaElement = item as IEdmSchemaElement;
			if (edmSchemaElement != null)
			{
				name = edmSchemaElement.FullName();
			}
			else
			{
				name = item.Name;
			}
			string str = name;
			if (memberNameList.Add(str))
			{
				return true;
			}
			else
			{
				if (!suppressError)
				{
					context.AddError(item.Location(), errorCode, errorString);
				}
				return false;
			}
		}

		internal static bool AllPropertiesAreNullable(IEnumerable<IEdmStructuralProperty> properties)
		{
			IEnumerable<IEdmStructuralProperty> edmStructuralProperties = properties;
			return edmStructuralProperties.Where<IEdmStructuralProperty>((IEdmStructuralProperty p) => !p.Type.IsNullable).Count<IEdmStructuralProperty>() == 0;
		}

		internal static bool FunctionOrNameExistsInReferencedModel(this IEdmModel model, IEdmFunction function, string functionFullName, bool checkEntityContainer)
		{
			bool flag;
			Func<IEdmFunction, bool> func = null;
			IEnumerator<IEdmModel> enumerator = model.ReferencedModels.GetEnumerator();
			using (enumerator)
			{
				while (enumerator.MoveNext())
				{
					IEdmModel current = enumerator.Current;
					if (current.FindDeclaredType(functionFullName) != null || current.FindDeclaredValueTerm(functionFullName) != null || checkEntityContainer && current.FindDeclaredEntityContainer(functionFullName) != null)
					{
						flag = true;
						return flag;
					}
					else
					{
						IEnumerable<IEdmFunction> edmFunctions = current.FindDeclaredFunctions(functionFullName);
						IEnumerable<IEdmFunction> edmFunctions1 = edmFunctions;
						if (edmFunctions == null)
						{
							edmFunctions1 = Enumerable.Empty<IEdmFunction>();
						}
						IEnumerable<IEdmFunction> edmFunctions2 = edmFunctions1;
						IEnumerable<IEdmFunction> edmFunctions3 = edmFunctions2;
						if (func == null)
						{
							func = (IEdmFunction existingFunction) => function.IsFunctionSignatureEquivalentTo(existingFunction);
						}
						if (!edmFunctions3.Any<IEdmFunction>(func))
						{
							continue;
						}
						flag = true;
						return flag;
					}
				}
				return false;
			}
			return flag;
		}

		internal static bool HasNullableProperty(IEnumerable<IEdmStructuralProperty> properties)
		{
			IEnumerable<IEdmStructuralProperty> edmStructuralProperties = properties;
			return edmStructuralProperties.Where<IEdmStructuralProperty>((IEdmStructuralProperty p) => p.Type.IsNullable).Count<IEdmStructuralProperty>() > 0;
		}

		internal static bool IsEdmSystemNamespace(string namespaceName)
		{
			if (namespaceName == "Transient")
			{
				return true;
			}
			else
			{
				return namespaceName == "Edm";
			}
		}

		internal static bool IsInterfaceCritical(EdmError error)
		{
			if (error.ErrorCode < EdmErrorCode.InterfaceCriticalPropertyValueMustNotBeNull)
			{
				return false;
			}
			else
			{
				return error.ErrorCode <= EdmErrorCode.InterfaceCriticalCycleInTypeHierarchy;
			}
		}

		internal static bool ItemExistsInReferencedModel(this IEdmModel model, string fullName, bool checkEntityContainer)
		{
			bool flag;
			IEnumerable<IEdmFunction> edmFunctions;
			IEnumerator<IEdmModel> enumerator = model.ReferencedModels.GetEnumerator();
			using (enumerator)
			{
				do
				{
					if (enumerator.MoveNext())
					{
						IEdmModel current = enumerator.Current;
						if (current.FindDeclaredType(fullName) != null || current.FindDeclaredValueTerm(fullName) != null || checkEntityContainer && current.FindDeclaredEntityContainer(fullName) != null)
						{
							break;
						}
						IEnumerable<IEdmFunction> edmFunctions1 = current.FindDeclaredFunctions(fullName);
						edmFunctions = edmFunctions1;
						if (edmFunctions1 != null)
						{
							continue;
						}
						edmFunctions = Enumerable.Empty<IEdmFunction>();
					}
					else
					{
						return false;
					}
				}
				while (edmFunctions.FirstOrDefault<IEdmFunction>() == null);
				flag = true;
			}
			return flag;
		}

		internal static bool PropertySetIsSubset(IEnumerable<IEdmStructuralProperty> set, IEnumerable<IEdmStructuralProperty> subset)
		{
			return subset.Except<IEdmStructuralProperty>(set).Count<IEdmStructuralProperty>() <= 0;
		}

		internal static bool PropertySetsAreEquivalent(IEnumerable<IEdmStructuralProperty> set1, IEnumerable<IEdmStructuralProperty> set2)
		{
			bool flag;
			if (set1.Count<IEdmStructuralProperty>() == set2.Count<IEdmStructuralProperty>())
			{
				IEnumerator<IEdmStructuralProperty> enumerator = set2.GetEnumerator();
				IEnumerator<IEdmStructuralProperty> enumerator1 = set1.GetEnumerator();
				using (enumerator1)
				{
					while (enumerator1.MoveNext())
					{
						IEdmStructuralProperty current = enumerator1.Current;
						enumerator.MoveNext();
						if (current == enumerator.Current)
						{
							continue;
						}
						flag = false;
						return flag;
					}
					return true;
				}
				return flag;
			}
			else
			{
				return false;
			}
		}

		internal static bool TypeIndirectlyContainsTarget(IEdmEntityType source, IEdmEntityType target, HashSetInternal<IEdmEntityType> visited, IEdmModel context)
		{
			bool flag;
			if (visited.Add(source))
			{
				if (!source.IsOrInheritsFrom(target))
				{
					foreach (IEdmNavigationProperty edmNavigationProperty in source.NavigationProperties())
					{
						if (!edmNavigationProperty.ContainsTarget || !ValidationHelper.TypeIndirectlyContainsTarget(edmNavigationProperty.ToEntityType(), target, visited, context))
						{
							continue;
						}
						flag = true;
						return flag;
					}
					IEnumerator<IEdmStructuredType> enumerator = context.FindAllDerivedTypes(source).GetEnumerator();
					using (enumerator)
					{
						while (enumerator.MoveNext())
						{
							IEdmStructuredType current = enumerator.Current;
							IEdmEntityType edmEntityType = current as IEdmEntityType;
							if (edmEntityType == null || !ValidationHelper.TypeIndirectlyContainsTarget(edmEntityType, target, visited, context))
							{
								continue;
							}
							flag = true;
							return flag;
						}
						return false;
					}
					return flag;
				}
				else
				{
					return true;
				}
			}
			return false;
		}

		internal static bool ValidateValueCanBeWrittenAsXmlElementAnnotation(IEdmValue value, string annotationNamespace, string annotationName, out EdmError error)
		{
			bool flag;
			IEdmStringValue edmStringValue = value as IEdmStringValue;
			if (edmStringValue != null)
			{
				string str = edmStringValue.Value;
				XmlReader xmlReader = XmlReader.Create(new StringReader(str));
				try
				{
					if (xmlReader.NodeType != XmlNodeType.Element)
					{
						while (xmlReader.Read() && xmlReader.NodeType != XmlNodeType.Element)
						{
						}
					}
					if (!xmlReader.EOF)
					{
						string namespaceURI = xmlReader.NamespaceURI;
						string localName = xmlReader.LocalName;
						if (EdmUtil.IsNullOrWhiteSpaceInternal(namespaceURI) || EdmUtil.IsNullOrWhiteSpaceInternal(localName))
						{
							error = new EdmError(value.Location(), EdmErrorCode.InvalidElementAnnotation, Strings.EdmModel_Validator_Semantic_InvalidElementAnnotationNullNamespaceOrName);
							flag = false;
						}
						else
						{
							if ((annotationNamespace == null || namespaceURI == annotationNamespace) && (annotationName == null || localName == annotationName))
							{
								while (xmlReader.Read())
								{
								}
								error = null;
								flag = true;
							}
							else
							{
								error = new EdmError(value.Location(), EdmErrorCode.InvalidElementAnnotation, Strings.EdmModel_Validator_Semantic_InvalidElementAnnotationMismatchedTerm);
								flag = false;
							}
						}
					}
					else
					{
						error = new EdmError(value.Location(), EdmErrorCode.InvalidElementAnnotation, Strings.EdmModel_Validator_Semantic_InvalidElementAnnotationValueInvalidXml);
						flag = false;
					}
				}
				catch (Exception exception)
				{
					error = new EdmError(value.Location(), EdmErrorCode.InvalidElementAnnotation, Strings.EdmModel_Validator_Semantic_InvalidElementAnnotationValueInvalidXml);
					flag = false;
				}
				return flag;
			}
			else
			{
				error = new EdmError(value.Location(), EdmErrorCode.InvalidElementAnnotation, Strings.EdmModel_Validator_Semantic_InvalidElementAnnotationNotIEdmStringValue);
				return false;
			}
		}
	}
}