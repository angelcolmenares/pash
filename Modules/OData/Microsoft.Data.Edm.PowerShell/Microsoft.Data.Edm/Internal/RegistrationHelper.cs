using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Library.Internal;
using System;
using System.Collections.Generic;

namespace Microsoft.Data.Edm.Internal
{
	internal static class RegistrationHelper
	{
		internal static void AddElement<T>(T element, string name, Dictionary<string, T> elementDictionary, Func<T, T, T> ambiguityCreator)
			where T : class, IEdmElement
		{
			T t = null;
			if (!elementDictionary.TryGetValue(name, out t))
			{
				elementDictionary[name] = element;
				return;
			}
			else
			{
				elementDictionary[name] = ambiguityCreator(t, element);
				return;
			}
		}

		internal static void AddFunction<T>(T function, string name, Dictionary<string, object> functionListDictionary)
			where T : class, IEdmFunctionBase
		{
			object obj = null;
			if (!functionListDictionary.TryGetValue(name, out obj))
			{
				functionListDictionary[name] = function;
				return;
			}
			else
			{
				List<T> ts = obj as List<T>;
				if (ts == null)
				{
					T t = (T)obj;
					ts = new List<T>();
					ts.Add(t);
					functionListDictionary[name] = ts;
				}
				ts.Add(function);
				return;
			}
		}

		internal static IEdmEntityContainer CreateAmbiguousEntityContainerBinding(IEdmEntityContainer first, IEdmEntityContainer second)
		{
			AmbiguousEntityContainerBinding ambiguousEntityContainerBinding = first as AmbiguousEntityContainerBinding;
			if (ambiguousEntityContainerBinding == null)
			{
				return new AmbiguousEntityContainerBinding(first, second);
			}
			else
			{
				ambiguousEntityContainerBinding.AddBinding(second);
				return ambiguousEntityContainerBinding;
			}
		}

		internal static IEdmEntitySet CreateAmbiguousEntitySetBinding(IEdmEntitySet first, IEdmEntitySet second)
		{
			AmbiguousEntitySetBinding ambiguousEntitySetBinding = first as AmbiguousEntitySetBinding;
			if (ambiguousEntitySetBinding == null)
			{
				return new AmbiguousEntitySetBinding(first, second);
			}
			else
			{
				ambiguousEntitySetBinding.AddBinding(second);
				return ambiguousEntitySetBinding;
			}
		}

		private static IEdmProperty CreateAmbiguousPropertyBinding(IEdmProperty first, IEdmProperty second)
		{
			AmbiguousPropertyBinding ambiguousPropertyBinding = first as AmbiguousPropertyBinding;
			if (ambiguousPropertyBinding == null)
			{
				return new AmbiguousPropertyBinding(first.DeclaringType, first, second);
			}
			else
			{
				ambiguousPropertyBinding.AddBinding(second);
				return ambiguousPropertyBinding;
			}
		}

		internal static IEdmSchemaType CreateAmbiguousTypeBinding(IEdmSchemaType first, IEdmSchemaType second)
		{
			AmbiguousTypeBinding ambiguousTypeBinding = first as AmbiguousTypeBinding;
			if (ambiguousTypeBinding == null)
			{
				return new AmbiguousTypeBinding(first, second);
			}
			else
			{
				ambiguousTypeBinding.AddBinding(second);
				return ambiguousTypeBinding;
			}
		}

		internal static IEdmValueTerm CreateAmbiguousValueTermBinding(IEdmValueTerm first, IEdmValueTerm second)
		{
			AmbiguousValueTermBinding ambiguousValueTermBinding = first as AmbiguousValueTermBinding;
			if (ambiguousValueTermBinding == null)
			{
				return new AmbiguousValueTermBinding(first, second);
			}
			else
			{
				ambiguousValueTermBinding.AddBinding(second);
				return ambiguousValueTermBinding;
			}
		}

		internal static void RegisterProperty(IEdmProperty element, string name, Dictionary<string, IEdmProperty> dictionary)
		{
			RegistrationHelper.AddElement<IEdmProperty>(element, name, dictionary, new Func<IEdmProperty, IEdmProperty, IEdmProperty>(RegistrationHelper.CreateAmbiguousPropertyBinding));
		}

		internal static void RegisterSchemaElement(IEdmSchemaElement element, Dictionary<string, IEdmSchemaType> schemaTypeDictionary, Dictionary<string, IEdmValueTerm> valueTermDictionary, Dictionary<string, object> functionGroupDictionary, Dictionary<string, IEdmEntityContainer> containerDictionary)
		{
			string str = element.FullName();
			EdmSchemaElementKind schemaElementKind = element.SchemaElementKind;
			switch (schemaElementKind)
			{
				case EdmSchemaElementKind.None:
				{
					throw new InvalidOperationException(Strings.EdmModel_CannotUseElementWithTypeNone);
				}
				case EdmSchemaElementKind.TypeDefinition:
				{
					RegistrationHelper.AddElement<IEdmSchemaType>((IEdmSchemaType)element, str, schemaTypeDictionary, new Func<IEdmSchemaType, IEdmSchemaType, IEdmSchemaType>(RegistrationHelper.CreateAmbiguousTypeBinding));
					return;
				}
				case EdmSchemaElementKind.Function:
				{
					RegistrationHelper.AddFunction<IEdmFunction>((IEdmFunction)element, str, functionGroupDictionary);
					return;
				}
				case EdmSchemaElementKind.ValueTerm:
				{
					RegistrationHelper.AddElement<IEdmValueTerm>((IEdmValueTerm)element, str, valueTermDictionary, new Func<IEdmValueTerm, IEdmValueTerm, IEdmValueTerm>(RegistrationHelper.CreateAmbiguousValueTermBinding));
					return;
				}
				case EdmSchemaElementKind.EntityContainer:
				{
					RegistrationHelper.AddElement<IEdmEntityContainer>((IEdmEntityContainer)element, str, containerDictionary, new Func<IEdmEntityContainer, IEdmEntityContainer, IEdmEntityContainer>(RegistrationHelper.CreateAmbiguousEntityContainerBinding));
					RegistrationHelper.AddElement<IEdmEntityContainer>((IEdmEntityContainer)element, element.Name, containerDictionary, new Func<IEdmEntityContainer, IEdmEntityContainer, IEdmEntityContainer>(RegistrationHelper.CreateAmbiguousEntityContainerBinding));
					return;
				}
			}
			throw new InvalidOperationException(Strings.UnknownEnumVal_SchemaElementKind(element.SchemaElementKind));
		}
	}
}