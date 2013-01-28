using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Annotations;
using Microsoft.Data.Edm.Internal;
using Microsoft.Data.Edm.Library.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Data.Edm.Library
{
	internal abstract class EdmModelBase : EdmElement, IEdmModel, IEdmElement
	{
		private readonly EdmDirectValueAnnotationsManager annotationsManager;

		private readonly Dictionary<string, IEdmEntityContainer> containersDictionary;

		private readonly Dictionary<string, IEdmSchemaType> schemaTypeDictionary;

		private readonly Dictionary<string, IEdmValueTerm> valueTermDictionary;

		private readonly Dictionary<string, object> functionDictionary;

		private readonly List<IEdmModel> referencedModels;

		public IEdmDirectValueAnnotationsManager DirectValueAnnotationsManager
		{
			get
			{
				return this.annotationsManager;
			}
		}

		public IEnumerable<IEdmModel> ReferencedModels
		{
			get
			{
				return this.referencedModels;
			}
		}

		public abstract IEnumerable<IEdmSchemaElement> SchemaElements
		{
			get;
		}

		public virtual IEnumerable<IEdmVocabularyAnnotation> VocabularyAnnotations
		{
			get
			{
				return Enumerable.Empty<IEdmVocabularyAnnotation>();
			}
		}

		protected EdmModelBase(IEnumerable<IEdmModel> referencedModels, EdmDirectValueAnnotationsManager annotationsManager)
		{
			this.containersDictionary = new Dictionary<string, IEdmEntityContainer>();
			this.schemaTypeDictionary = new Dictionary<string, IEdmSchemaType>();
			this.valueTermDictionary = new Dictionary<string, IEdmValueTerm>();
			this.functionDictionary = new Dictionary<string, object>();
			EdmUtil.CheckArgumentNull<IEnumerable<IEdmModel>>(referencedModels, "referencedModels");
			EdmUtil.CheckArgumentNull<EdmDirectValueAnnotationsManager>(annotationsManager, "annotationsManager");
			this.referencedModels = new List<IEdmModel>(referencedModels);
			this.referencedModels.Add(EdmCoreModel.Instance);
			this.annotationsManager = annotationsManager;
		}

		protected void AddReferencedModel(IEdmModel model)
		{
			EdmUtil.CheckArgumentNull<IEdmModel>(model, "model");
			this.referencedModels.Add(model);
		}

		public IEdmEntityContainer FindDeclaredEntityContainer(string name)
		{
			IEdmEntityContainer edmEntityContainer = null;
			if (this.containersDictionary.TryGetValue(name, out edmEntityContainer))
			{
				return edmEntityContainer;
			}
			else
			{
				return null;
			}
		}

		public IEnumerable<IEdmFunction> FindDeclaredFunctions(string qualifiedName)
		{
			object obj = null;
			if (!this.functionDictionary.TryGetValue(qualifiedName, out obj))
			{
				return Enumerable.Empty<IEdmFunction>();
			}
			else
			{
				List<IEdmFunction> edmFunctions = obj as List<IEdmFunction>;
				if (edmFunctions == null)
				{
					IEdmFunction[] edmFunctionArray = new IEdmFunction[1];
					edmFunctionArray[0] = (IEdmFunction)obj;
					return edmFunctionArray;
				}
				else
				{
					return edmFunctions;
				}
			}
		}

		public IEdmSchemaType FindDeclaredType(string qualifiedName)
		{
			IEdmSchemaType edmSchemaType = null;
			this.schemaTypeDictionary.TryGetValue(qualifiedName, out edmSchemaType);
			return edmSchemaType;
		}

		public IEdmValueTerm FindDeclaredValueTerm(string qualifiedName)
		{
			IEdmValueTerm edmValueTerm = null;
			this.valueTermDictionary.TryGetValue(qualifiedName, out edmValueTerm);
			return edmValueTerm;
		}

		public virtual IEnumerable<IEdmVocabularyAnnotation> FindDeclaredVocabularyAnnotations(IEdmVocabularyAnnotatable element)
		{
			return Enumerable.Empty<IEdmVocabularyAnnotation>();
		}

		public abstract IEnumerable<IEdmStructuredType> FindDirectlyDerivedTypes(IEdmStructuredType baseType);

		protected void RegisterElement(IEdmSchemaElement element)
		{
			EdmUtil.CheckArgumentNull<IEdmSchemaElement>(element, "element");
			RegistrationHelper.RegisterSchemaElement(element, this.schemaTypeDictionary, this.valueTermDictionary, this.functionDictionary, this.containersDictionary);
		}
	}
}