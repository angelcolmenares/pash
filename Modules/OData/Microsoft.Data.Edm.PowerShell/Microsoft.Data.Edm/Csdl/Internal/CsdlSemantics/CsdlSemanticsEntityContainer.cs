using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Annotations;
using Microsoft.Data.Edm.Csdl.Internal.Parsing.Ast;
using Microsoft.Data.Edm.Internal;
using Microsoft.Data.Edm.Library.Internal;
using Microsoft.Data.Edm.Validation;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Data.Edm.Csdl.Internal.CsdlSemantics
{
	internal class CsdlSemanticsEntityContainer : CsdlSemanticsElement, IEdmEntityContainer, IEdmSchemaElement, IEdmNamedElement, IEdmVocabularyAnnotatable, IEdmElement, IEdmCheckable
	{
		private readonly CsdlEntityContainer entityContainer;

		private readonly CsdlSemanticsSchema context;

		private readonly Cache<CsdlSemanticsEntityContainer, IEnumerable<IEdmEntityContainerElement>> elementsCache;

		private readonly static Func<CsdlSemanticsEntityContainer, IEnumerable<IEdmEntityContainerElement>> ComputeElementsFunc;

		private readonly Cache<CsdlSemanticsEntityContainer, IEnumerable<CsdlSemanticsAssociationSet>> associationSetsCache;

		private readonly static Func<CsdlSemanticsEntityContainer, IEnumerable<CsdlSemanticsAssociationSet>> ComputeAssociationSetsFunc;

		private readonly Cache<CsdlSemanticsEntityContainer, Dictionary<IEdmAssociation, IEnumerable<CsdlSemanticsAssociationSet>>> associationSetMappingsCache;

		private readonly static Func<CsdlSemanticsEntityContainer, Dictionary<IEdmAssociation, IEnumerable<CsdlSemanticsAssociationSet>>> ComputeAssociationSetMappingsFunc;

		private readonly Cache<CsdlSemanticsEntityContainer, Dictionary<string, IEdmEntitySet>> entitySetDictionaryCache;

		private readonly static Func<CsdlSemanticsEntityContainer, Dictionary<string, IEdmEntitySet>> ComputeEntitySetDictionaryFunc;

		private readonly Cache<CsdlSemanticsEntityContainer, Dictionary<string, object>> functionImportsDictionaryCache;

		private readonly static Func<CsdlSemanticsEntityContainer, Dictionary<string, object>> ComputeFunctionImportsDictionaryFunc;

		private readonly Cache<CsdlSemanticsEntityContainer, IEnumerable<EdmError>> errorsCache;

		private readonly static Func<CsdlSemanticsEntityContainer, IEnumerable<EdmError>> ComputeErrorsFunc;

		private readonly Cache<CsdlSemanticsEntityContainer, IEdmEntityContainer> extendsCache;

		private readonly static Func<CsdlSemanticsEntityContainer, IEdmEntityContainer> ComputeExtendsFunc;

		private readonly static Func<CsdlSemanticsEntityContainer, IEdmEntityContainer> OnCycleExtendsFunc;

		private Dictionary<IEdmAssociation, IEnumerable<CsdlSemanticsAssociationSet>> AssociationSetMappings
		{
			get
			{
				return this.associationSetMappingsCache.GetValue(this, CsdlSemanticsEntityContainer.ComputeAssociationSetMappingsFunc, null);
			}
		}

		public IEnumerable<CsdlSemanticsAssociationSet> AssociationSets
		{
			get
			{
				return this.associationSetsCache.GetValue(this, CsdlSemanticsEntityContainer.ComputeAssociationSetsFunc, null);
			}
		}

		internal CsdlSemanticsSchema Context
		{
			get
			{
				return this.context;
			}
		}

		public override CsdlElement Element
		{
			get
			{
				return this.entityContainer;
			}
		}

		public IEnumerable<IEdmEntityContainerElement> Elements
		{
			get
			{
				return this.elementsCache.GetValue(this, CsdlSemanticsEntityContainer.ComputeElementsFunc, null);
			}
		}

		private Dictionary<string, IEdmEntitySet> EntitySetDictionary
		{
			get
			{
				return this.entitySetDictionaryCache.GetValue(this, CsdlSemanticsEntityContainer.ComputeEntitySetDictionaryFunc, null);
			}
		}

		public IEnumerable<EdmError> Errors
		{
			get
			{
				return this.errorsCache.GetValue(this, CsdlSemanticsEntityContainer.ComputeErrorsFunc, null);
			}
		}

		private IEdmEntityContainer Extends
		{
			get
			{
				return this.extendsCache.GetValue(this, CsdlSemanticsEntityContainer.ComputeExtendsFunc, CsdlSemanticsEntityContainer.OnCycleExtendsFunc);
			}
		}

		private Dictionary<string, object> FunctionImportsDictionary
		{
			get
			{
				return this.functionImportsDictionaryCache.GetValue(this, CsdlSemanticsEntityContainer.ComputeFunctionImportsDictionaryFunc, null);
			}
		}

		public override CsdlSemanticsModel Model
		{
			get
			{
				return this.context.Model;
			}
		}

		public string Name
		{
			get
			{
				return this.entityContainer.Name;
			}
		}

		public string Namespace
		{
			get
			{
				return this.context.Namespace;
			}
		}

		public EdmSchemaElementKind SchemaElementKind
		{
			get
			{
				return EdmSchemaElementKind.EntityContainer;
			}
		}

		static CsdlSemanticsEntityContainer()
		{
			CsdlSemanticsEntityContainer.ComputeElementsFunc = (CsdlSemanticsEntityContainer me) => me.ComputeElements();
			CsdlSemanticsEntityContainer.ComputeAssociationSetsFunc = (CsdlSemanticsEntityContainer me) => me.ComputeAssociationSets();
			CsdlSemanticsEntityContainer.ComputeAssociationSetMappingsFunc = (CsdlSemanticsEntityContainer me) => me.ComputeAssociationSetMappings();
			CsdlSemanticsEntityContainer.ComputeEntitySetDictionaryFunc = (CsdlSemanticsEntityContainer me) => me.ComputeEntitySetDictionary();
			CsdlSemanticsEntityContainer.ComputeFunctionImportsDictionaryFunc = (CsdlSemanticsEntityContainer me) => me.ComputeFunctionImportsDictionary();
			CsdlSemanticsEntityContainer.ComputeErrorsFunc = (CsdlSemanticsEntityContainer me) => me.ComputeErrors();
			CsdlSemanticsEntityContainer.ComputeExtendsFunc = (CsdlSemanticsEntityContainer me) => me.ComputeExtends();
			CsdlSemanticsEntityContainer.OnCycleExtendsFunc = (CsdlSemanticsEntityContainer me) => new CyclicEntityContainer(me.entityContainer.Extends, me.Location);
		}

		public CsdlSemanticsEntityContainer(CsdlSemanticsSchema context, CsdlEntityContainer entityContainer) : base(entityContainer)
		{
			this.elementsCache = new Cache<CsdlSemanticsEntityContainer, IEnumerable<IEdmEntityContainerElement>>();
			this.associationSetsCache = new Cache<CsdlSemanticsEntityContainer, IEnumerable<CsdlSemanticsAssociationSet>>();
			this.associationSetMappingsCache = new Cache<CsdlSemanticsEntityContainer, Dictionary<IEdmAssociation, IEnumerable<CsdlSemanticsAssociationSet>>>();
			this.entitySetDictionaryCache = new Cache<CsdlSemanticsEntityContainer, Dictionary<string, IEdmEntitySet>>();
			this.functionImportsDictionaryCache = new Cache<CsdlSemanticsEntityContainer, Dictionary<string, object>>();
			this.errorsCache = new Cache<CsdlSemanticsEntityContainer, IEnumerable<EdmError>>();
			this.extendsCache = new Cache<CsdlSemanticsEntityContainer, IEdmEntityContainer>();
			this.context = context;
			this.entityContainer = entityContainer;
		}

		private Dictionary<IEdmAssociation, IEnumerable<CsdlSemanticsAssociationSet>> ComputeAssociationSetMappings()
		{
			IEnumerable<CsdlSemanticsAssociationSet> csdlSemanticsAssociationSets = null;
			Dictionary<IEdmAssociation, IEnumerable<CsdlSemanticsAssociationSet>> edmAssociations = new Dictionary<IEdmAssociation, IEnumerable<CsdlSemanticsAssociationSet>>();
			if (this.entityContainer.Extends != null)
			{
				CsdlSemanticsEntityContainer extends = this.Extends as CsdlSemanticsEntityContainer;
				if (extends != null)
				{
					foreach (KeyValuePair<IEdmAssociation, IEnumerable<CsdlSemanticsAssociationSet>> associationSetMapping in extends.AssociationSetMappings)
					{
						edmAssociations[associationSetMapping.Key] = new List<CsdlSemanticsAssociationSet>(associationSetMapping.Value);
					}
				}
			}
			foreach (CsdlSemanticsAssociationSet associationSet in this.AssociationSets)
			{
				CsdlSemanticsAssociation association = associationSet.Association as CsdlSemanticsAssociation;
				if (association == null)
				{
					continue;
				}
				if (!edmAssociations.TryGetValue(association, out csdlSemanticsAssociationSets))
				{
					csdlSemanticsAssociationSets = new List<CsdlSemanticsAssociationSet>();
					edmAssociations[association] = csdlSemanticsAssociationSets;
				}
				((List<CsdlSemanticsAssociationSet>)csdlSemanticsAssociationSets).Add(associationSet);
			}
			return edmAssociations;
		}

		private IEnumerable<CsdlSemanticsAssociationSet> ComputeAssociationSets()
		{
			List<CsdlSemanticsAssociationSet> csdlSemanticsAssociationSets = new List<CsdlSemanticsAssociationSet>();
			if (this.entityContainer.Extends != null)
			{
				CsdlSemanticsEntityContainer extends = this.Extends as CsdlSemanticsEntityContainer;
				if (extends != null)
				{
					foreach (CsdlAssociationSet associationSet in extends.entityContainer.AssociationSets)
					{
						CsdlSemanticsAssociationSet csdlSemanticsAssociationSet = new CsdlSemanticsAssociationSet(this, associationSet);
						csdlSemanticsAssociationSets.Add(csdlSemanticsAssociationSet);
					}
				}
			}
			foreach (CsdlAssociationSet csdlAssociationSet in this.entityContainer.AssociationSets)
			{
				CsdlSemanticsAssociationSet csdlSemanticsAssociationSet1 = new CsdlSemanticsAssociationSet(this, csdlAssociationSet);
				csdlSemanticsAssociationSets.Add(csdlSemanticsAssociationSet1);
			}
			return csdlSemanticsAssociationSets;
		}

		private IEnumerable<IEdmEntityContainerElement> ComputeElements()
		{
			List<IEdmEntityContainerElement> edmEntityContainerElements = new List<IEdmEntityContainerElement>();
			CsdlSemanticsEntityContainer extends = this.Extends as CsdlSemanticsEntityContainer;
			if (extends != null)
			{
				foreach (CsdlEntitySet entitySet in extends.entityContainer.EntitySets)
				{
					CsdlSemanticsEntitySet csdlSemanticsEntitySet = new CsdlSemanticsEntitySet(this, entitySet);
					edmEntityContainerElements.Add(csdlSemanticsEntitySet);
				}
				foreach (CsdlFunctionImport functionImport in extends.entityContainer.FunctionImports)
				{
					CsdlSemanticsFunctionImport csdlSemanticsFunctionImport = new CsdlSemanticsFunctionImport(this, functionImport);
					edmEntityContainerElements.Add(csdlSemanticsFunctionImport);
				}
			}
			foreach (CsdlEntitySet csdlEntitySet in this.entityContainer.EntitySets)
			{
				CsdlSemanticsEntitySet csdlSemanticsEntitySet1 = new CsdlSemanticsEntitySet(this, csdlEntitySet);
				edmEntityContainerElements.Add(csdlSemanticsEntitySet1);
			}
			foreach (CsdlFunctionImport csdlFunctionImport in this.entityContainer.FunctionImports)
			{
				CsdlSemanticsFunctionImport csdlSemanticsFunctionImport1 = new CsdlSemanticsFunctionImport(this, csdlFunctionImport);
				edmEntityContainerElements.Add(csdlSemanticsFunctionImport1);
			}
			return edmEntityContainerElements;
		}

		private Dictionary<string, IEdmEntitySet> ComputeEntitySetDictionary()
		{
			Dictionary<string, IEdmEntitySet> strs = new Dictionary<string, IEdmEntitySet>();
			foreach (IEdmEntitySet edmEntitySet in this.Elements.OfType<IEdmEntitySet>())
			{
				RegistrationHelper.AddElement<IEdmEntitySet>(edmEntitySet, edmEntitySet.Name, strs, new Func<IEdmEntitySet, IEdmEntitySet, IEdmEntitySet>(RegistrationHelper.CreateAmbiguousEntitySetBinding));
			}
			return strs;
		}

		private IEnumerable<EdmError> ComputeErrors()
		{
			List<EdmError> edmErrors = new List<EdmError>();
			if (this.Extends != null && this.Extends.IsBad())
			{
				edmErrors.AddRange(((IEdmCheckable)this.Extends).Errors);
			}
			foreach (CsdlSemanticsAssociationSet associationSet in this.AssociationSets)
			{
				int count = edmErrors.Count;
				edmErrors.AddRange(associationSet.Errors());
				if (edmErrors.Count != count)
				{
					continue;
				}
				edmErrors.AddRange(associationSet.End1.Errors());
				edmErrors.AddRange(associationSet.End2.Errors());
			}
			return edmErrors;
		}

		private IEdmEntityContainer ComputeExtends()
		{
			if (this.entityContainer.Extends == null)
			{
				return null;
			}
			else
			{
				CsdlSemanticsEntityContainer csdlSemanticsEntityContainer = this.Model.FindDeclaredEntityContainer(this.entityContainer.Extends) as CsdlSemanticsEntityContainer;
				if (csdlSemanticsEntityContainer == null)
				{
					return new UnresolvedEntityContainer(this.entityContainer.Extends, base.Location);
				}
				else
				{
					var extends = csdlSemanticsEntityContainer.Extends;
					if (extends != null) { }
					return csdlSemanticsEntityContainer;
				}
			}
		}

		private Dictionary<string, object> ComputeFunctionImportsDictionary()
		{
			Dictionary<string, object> strs = new Dictionary<string, object>();
			foreach (IEdmFunctionImport edmFunctionImport in this.Elements.OfType<IEdmFunctionImport>())
			{
				RegistrationHelper.AddFunction<IEdmFunctionImport>(edmFunctionImport, edmFunctionImport.Name, strs);
			}
			return strs;
		}

		protected override IEnumerable<IEdmVocabularyAnnotation> ComputeInlineVocabularyAnnotations()
		{
			return this.Model.WrapInlineVocabularyAnnotations(this, this.Context);
		}

		public IEnumerable<CsdlSemanticsAssociationSet> FindAssociationSets(IEdmAssociation association)
		{
			IEnumerable<CsdlSemanticsAssociationSet> csdlSemanticsAssociationSets = null;
			if (this.AssociationSetMappings.TryGetValue(association, out csdlSemanticsAssociationSets))
			{
				return csdlSemanticsAssociationSets;
			}
			else
			{
				return null;
			}
		}

		public IEdmEntitySet FindEntitySet(string name)
		{
			IEdmEntitySet edmEntitySet = null;
			if (this.EntitySetDictionary.TryGetValue(name, out edmEntitySet))
			{
				return edmEntitySet;
			}
			else
			{
				return null;
			}
		}

		public IEnumerable<IEdmFunctionImport> FindFunctionImports(string name)
		{
			object obj = null;
			if (!this.FunctionImportsDictionary.TryGetValue(name, out obj))
			{
				return Enumerable.Empty<IEdmFunctionImport>();
			}
			else
			{
				List<IEdmFunctionImport> edmFunctionImports = obj as List<IEdmFunctionImport>;
				if (edmFunctionImports == null)
				{
					IEdmFunctionImport[] edmFunctionImportArray = new IEdmFunctionImport[1];
					edmFunctionImportArray[0] = (IEdmFunctionImport)obj;
					return edmFunctionImportArray;
				}
				else
				{
					return edmFunctionImports;
				}
			}
		}
	}
}