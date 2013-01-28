using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Annotations;
using Microsoft.Data.Edm.Csdl.Internal.Parsing.Ast;
using Microsoft.Data.Edm.Internal;
using System;
using System.Collections.Generic;

namespace Microsoft.Data.Edm.Csdl.Internal.CsdlSemantics
{
	internal abstract class CsdlSemanticsStructuredTypeDefinition : CsdlSemanticsTypeDefinition, IEdmStructuredType, IEdmType, IEdmElement
	{
		private readonly CsdlSemanticsSchema context;

		private readonly Cache<CsdlSemanticsStructuredTypeDefinition, List<IEdmProperty>> declaredPropertiesCache;

		private readonly static Func<CsdlSemanticsStructuredTypeDefinition, List<IEdmProperty>> ComputeDeclaredPropertiesFunc;

		private readonly Cache<CsdlSemanticsStructuredTypeDefinition, IDictionary<string, IEdmProperty>> propertiesDictionaryCache;

		private readonly static Func<CsdlSemanticsStructuredTypeDefinition, IDictionary<string, IEdmProperty>> ComputePropertiesDictionaryFunc;

		public abstract IEdmStructuredType BaseType
		{
			get;
		}

		public CsdlSemanticsSchema Context
		{
			get
			{
				return this.context;
			}
		}

		public IEnumerable<IEdmProperty> DeclaredProperties
		{
			get
			{
				return this.declaredPropertiesCache.GetValue(this, CsdlSemanticsStructuredTypeDefinition.ComputeDeclaredPropertiesFunc, null);
			}
		}

		public override CsdlElement Element
		{
			get
			{
				return this.MyStructured;
			}
		}

		public virtual bool IsAbstract
		{
			get
			{
				return false;
			}
		}

		public virtual bool IsOpen
		{
			get
			{
				return false;
			}
		}

		public override CsdlSemanticsModel Model
		{
			get
			{
				return this.context.Model;
			}
		}

		protected abstract CsdlStructuredType MyStructured
		{
			get;
		}

		public string Namespace
		{
			get
			{
				return this.context.Namespace;
			}
		}

		private IDictionary<string, IEdmProperty> PropertiesDictionary
		{
			get
			{
				return this.propertiesDictionaryCache.GetValue(this, CsdlSemanticsStructuredTypeDefinition.ComputePropertiesDictionaryFunc, null);
			}
		}

		static CsdlSemanticsStructuredTypeDefinition()
		{
			CsdlSemanticsStructuredTypeDefinition.ComputeDeclaredPropertiesFunc = (CsdlSemanticsStructuredTypeDefinition me) => me.ComputeDeclaredProperties();
			CsdlSemanticsStructuredTypeDefinition.ComputePropertiesDictionaryFunc = (CsdlSemanticsStructuredTypeDefinition me) => me.ComputePropertiesDictionary();
		}

		protected CsdlSemanticsStructuredTypeDefinition(CsdlSemanticsSchema context, CsdlStructuredType type) : base(type)
		{
			this.declaredPropertiesCache = new Cache<CsdlSemanticsStructuredTypeDefinition, List<IEdmProperty>>();
			this.propertiesDictionaryCache = new Cache<CsdlSemanticsStructuredTypeDefinition, IDictionary<string, IEdmProperty>>();
			this.context = context;
		}

		protected virtual List<IEdmProperty> ComputeDeclaredProperties()
		{
			List<IEdmProperty> edmProperties = new List<IEdmProperty>();
			foreach (CsdlProperty property in this.MyStructured.Properties)
			{
				edmProperties.Add(new CsdlSemanticsProperty(this, property));
			}
			return edmProperties;
		}

		protected override IEnumerable<IEdmVocabularyAnnotation> ComputeInlineVocabularyAnnotations()
		{
			return this.Model.WrapInlineVocabularyAnnotations(this, this.context);
		}

		private IDictionary<string, IEdmProperty> ComputePropertiesDictionary()
		{
			Dictionary<string, IEdmProperty> strs = new Dictionary<string, IEdmProperty>();
			foreach (IEdmProperty edmProperty in this.Properties())
			{
				RegistrationHelper.RegisterProperty(edmProperty, edmProperty.Name, strs);
			}
			return strs;
		}

		public IEdmProperty FindProperty(string name)
		{
			IEdmProperty edmProperty = null;
			this.PropertiesDictionary.TryGetValue(name, out edmProperty);
			return edmProperty;
		}

		protected string GetCyclicBaseTypeName(string baseTypeName)
		{
			IEdmSchemaType edmSchemaType = this.context.FindType(baseTypeName);
			if (edmSchemaType != null)
			{
				return edmSchemaType.FullName();
			}
			else
			{
				return baseTypeName;
			}
		}
	}
}