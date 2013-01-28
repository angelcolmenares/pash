using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Expressions;
using Microsoft.Data.Edm.Internal;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Data.Edm.Library
{
	internal class EdmEntityContainer : EdmElement, IEdmEntityContainer, IEdmSchemaElement, IEdmNamedElement, IEdmVocabularyAnnotatable, IEdmElement
	{
		private readonly string namespaceName;

		private readonly string name;

		private readonly List<IEdmEntityContainerElement> containerElements;

		private readonly Dictionary<string, IEdmEntitySet> entitySetDictionary;

		private readonly Dictionary<string, object> functionImportDictionary;

		public IEnumerable<IEdmEntityContainerElement> Elements
		{
			get
			{
				return this.containerElements;
			}
		}

		public string Name
		{
			get
			{
				return this.name;
			}
		}

		public string Namespace
		{
			get
			{
				return this.namespaceName;
			}
		}

		public EdmSchemaElementKind SchemaElementKind
		{
			get
			{
				return EdmSchemaElementKind.EntityContainer;
			}
		}

		public EdmEntityContainer(string namespaceName, string name)
		{
			this.containerElements = new List<IEdmEntityContainerElement>();
			this.entitySetDictionary = new Dictionary<string, IEdmEntitySet>();
			this.functionImportDictionary = new Dictionary<string, object>();
			EdmUtil.CheckArgumentNull<string>(namespaceName, "namespaceName");
			EdmUtil.CheckArgumentNull<string>(name, "name");
			this.namespaceName = namespaceName;
			this.name = name;
		}

		public void AddElement(IEdmEntityContainerElement element)
		{
			EdmUtil.CheckArgumentNull<IEdmEntityContainerElement>(element, "element");
			this.containerElements.Add(element);
			EdmContainerElementKind containerElementKind = element.ContainerElementKind;
			switch (containerElementKind)
			{
				case EdmContainerElementKind.None:
				{
					throw new InvalidOperationException(Strings.EdmEntityContainer_CannotUseElementWithTypeNone);
				}
				case EdmContainerElementKind.EntitySet:
				{
					RegistrationHelper.AddElement<IEdmEntitySet>((IEdmEntitySet)element, element.Name, this.entitySetDictionary, new Func<IEdmEntitySet, IEdmEntitySet, IEdmEntitySet>(RegistrationHelper.CreateAmbiguousEntitySetBinding));
					return;
				}
				case EdmContainerElementKind.FunctionImport:
				{
					RegistrationHelper.AddFunction<IEdmFunctionImport>((IEdmFunctionImport)element, element.Name, this.functionImportDictionary);
					return;
				}
			}
			throw new InvalidOperationException(Strings.UnknownEnumVal_ContainerElementKind(element.ContainerElementKind));
		}

		public virtual EdmEntitySet AddEntitySet(string name, IEdmEntityType elementType)
		{
			EdmEntitySet edmEntitySet = new EdmEntitySet(this, name, elementType);
			this.AddElement(edmEntitySet);
			return edmEntitySet;
		}

		public virtual EdmFunctionImport AddFunctionImport(string name, IEdmTypeReference returnType)
		{
			EdmFunctionImport edmFunctionImport = new EdmFunctionImport(this, name, returnType);
			this.AddElement(edmFunctionImport);
			return edmFunctionImport;
		}

		public virtual EdmFunctionImport AddFunctionImport(string name, IEdmTypeReference returnType, IEdmExpression entitySet)
		{
			EdmFunctionImport edmFunctionImport = new EdmFunctionImport(this, name, returnType, entitySet);
			this.AddElement(edmFunctionImport);
			return edmFunctionImport;
		}

		public virtual EdmFunctionImport AddFunctionImport(string name, IEdmTypeReference returnType, IEdmExpression entitySet, bool sideEffecting, bool composable, bool bindable)
		{
			EdmFunctionImport edmFunctionImport = new EdmFunctionImport(this, name, returnType, entitySet, sideEffecting, composable, bindable);
			this.AddElement(edmFunctionImport);
			return edmFunctionImport;
		}

		public virtual IEdmEntitySet FindEntitySet(string setName)
		{
			IEdmEntitySet edmEntitySet = null;
			if (this.entitySetDictionary.TryGetValue(setName, out edmEntitySet))
			{
				return edmEntitySet;
			}
			else
			{
				return null;
			}
		}

		public IEnumerable<IEdmFunctionImport> FindFunctionImports(string functionName)
		{
			object obj = null;
			if (!this.functionImportDictionary.TryGetValue(functionName, out obj))
			{
				return Enumerable.Empty<IEdmFunctionImport>();
			}
			else
			{
				List<IEdmFunctionImport> elements = this.Elements as List<IEdmFunctionImport>;
				if (elements == null)
				{
					IEdmFunctionImport[] edmFunctionImportArray = new IEdmFunctionImport[1];
					edmFunctionImportArray[0] = (IEdmFunctionImport)obj;
					return edmFunctionImportArray;
				}
				else
				{
					return elements;
				}
			}
		}
	}
}