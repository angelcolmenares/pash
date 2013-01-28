using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Csdl.Internal.Parsing.Ast;
using Microsoft.Data.Edm.Internal;
using Microsoft.Data.Edm.Library.Internal;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Data.Edm.Csdl.Internal.CsdlSemantics
{
	internal class CsdlSemanticsEntityTypeDefinition : CsdlSemanticsStructuredTypeDefinition, IEdmEntityType, IEdmStructuredType, IEdmSchemaType, IEdmType, IEdmTerm, IEdmSchemaElement, IEdmNamedElement, IEdmVocabularyAnnotatable, IEdmElement
	{
		private readonly CsdlEntityType entity;

		private readonly Cache<CsdlSemanticsEntityTypeDefinition, IEdmEntityType> baseTypeCache;

		private readonly static Func<CsdlSemanticsEntityTypeDefinition, IEdmEntityType> ComputeBaseTypeFunc;

		private readonly static Func<CsdlSemanticsEntityTypeDefinition, IEdmEntityType> OnCycleBaseTypeFunc;

		private readonly Cache<CsdlSemanticsEntityTypeDefinition, IEnumerable<IEdmStructuralProperty>> declaredKeyCache;

		private readonly static Func<CsdlSemanticsEntityTypeDefinition, IEnumerable<IEdmStructuralProperty>> ComputeDeclaredKeyFunc;

		public override IEdmStructuredType BaseType
		{
			get
			{
				return this.baseTypeCache.GetValue(this, CsdlSemanticsEntityTypeDefinition.ComputeBaseTypeFunc, CsdlSemanticsEntityTypeDefinition.OnCycleBaseTypeFunc);
			}
		}

		public IEnumerable<IEdmStructuralProperty> DeclaredKey
		{
			get
			{
				return this.declaredKeyCache.GetValue(this, CsdlSemanticsEntityTypeDefinition.ComputeDeclaredKeyFunc, null);
			}
		}

		public override bool IsAbstract
		{
			get
			{
				return this.entity.IsAbstract;
			}
		}

		public override bool IsOpen
		{
			get
			{
				return this.entity.IsOpen;
			}
		}

		protected override CsdlStructuredType MyStructured
		{
			get
			{
				return this.entity;
			}
		}

		public string Name
		{
			get
			{
				return this.entity.Name;
			}
		}

		public EdmTermKind TermKind
		{
			get
			{
				return EdmTermKind.Type;
			}
		}

		public override EdmTypeKind TypeKind
		{
			get
			{
				return EdmTypeKind.Entity;
			}
		}

		static CsdlSemanticsEntityTypeDefinition()
		{
			CsdlSemanticsEntityTypeDefinition.ComputeBaseTypeFunc = (CsdlSemanticsEntityTypeDefinition me) => me.ComputeBaseType();
			CsdlSemanticsEntityTypeDefinition.OnCycleBaseTypeFunc = (CsdlSemanticsEntityTypeDefinition me) => new CyclicEntityType(me.GetCyclicBaseTypeName(me.entity.BaseTypeName), me.Location);
			CsdlSemanticsEntityTypeDefinition.ComputeDeclaredKeyFunc = (CsdlSemanticsEntityTypeDefinition me) => me.ComputeDeclaredKey();
		}

		public CsdlSemanticsEntityTypeDefinition(CsdlSemanticsSchema context, CsdlEntityType entity) : base(context, entity)
		{
			this.baseTypeCache = new Cache<CsdlSemanticsEntityTypeDefinition, IEdmEntityType>();
			this.declaredKeyCache = new Cache<CsdlSemanticsEntityTypeDefinition, IEnumerable<IEdmStructuralProperty>>();
			this.entity = entity;
		}

		private IEdmEntityType ComputeBaseType()
		{
			if (this.entity.BaseTypeName == null)
			{
				return null;
			}
			else
			{
				IEdmEntityType edmEntityType = base.Context.FindType(this.entity.BaseTypeName) as IEdmEntityType;
				if (edmEntityType != null)
				{
					var baseType = edmEntityType.BaseType;
					if (baseType == null) { }
				}
				IEdmEntityType edmEntityType1 = edmEntityType;
				IEdmEntityType unresolvedEntityType = edmEntityType1;
				if (edmEntityType1 == null)
				{
					unresolvedEntityType = new UnresolvedEntityType(base.Context.UnresolvedName(this.entity.BaseTypeName), base.Location);
				}
				return unresolvedEntityType;
			}
		}

		private IEnumerable<IEdmStructuralProperty> ComputeDeclaredKey()
		{
			if (this.entity.Key == null)
			{
				return null;
			}
			else
			{
				List<IEdmStructuralProperty> edmStructuralProperties = new List<IEdmStructuralProperty>();
				IEnumerator<CsdlPropertyReference> enumerator = this.entity.Key.Properties.GetEnumerator();
				using (enumerator)
				{
					Func<IEdmProperty, bool> func = null;
					while (enumerator.MoveNext())
					{
						CsdlPropertyReference current = enumerator.Current;
						IEdmStructuralProperty edmStructuralProperty = base.FindProperty(current.PropertyName) as IEdmStructuralProperty;
						if (edmStructuralProperty == null)
						{
							IEnumerable<IEdmProperty> declaredProperties = base.DeclaredProperties;
							if (func == null)
							{
								func = (IEdmProperty p) => p.Name == current.PropertyName;
							}
							edmStructuralProperty = declaredProperties.SingleOrDefault<IEdmProperty>(func) as IEdmStructuralProperty;
							if (edmStructuralProperty == null)
							{
								edmStructuralProperties.Add(new UnresolvedProperty(this, current.PropertyName, base.Location));
							}
							else
							{
								edmStructuralProperties.Add(edmStructuralProperty);
							}
						}
						else
						{
							edmStructuralProperties.Add(edmStructuralProperty);
						}
					}
				}
				return edmStructuralProperties;
			}
		}

		protected override List<IEdmProperty> ComputeDeclaredProperties()
		{
			List<IEdmProperty> edmProperties = base.ComputeDeclaredProperties();
			foreach (CsdlNavigationProperty navigationProperty in this.entity.NavigationProperties)
			{
				edmProperties.Add(new CsdlSemanticsNavigationProperty(this, navigationProperty));
			}
			return edmProperties;
		}
	}
}