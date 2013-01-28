using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Internal;
using System;
using System.Collections.Generic;

namespace Microsoft.Data.Edm.Library
{
	internal abstract class EdmStructuredType : EdmType, IEdmStructuredType, IEdmType, IEdmElement
	{
		private readonly IEdmStructuredType baseStructuredType;

		private readonly List<IEdmProperty> declaredProperties;

		private readonly bool isAbstract;

		private readonly bool isOpen;

		private readonly Cache<EdmStructuredType, IDictionary<string, IEdmProperty>> propertiesDictionary;

		private readonly static Func<EdmStructuredType, IDictionary<string, IEdmProperty>> ComputePropertiesDictionaryFunc;

		public IEdmStructuredType BaseType
		{
			get
			{
				return this.baseStructuredType;
			}
		}

		public virtual IEnumerable<IEdmProperty> DeclaredProperties
		{
			get
			{
				return this.declaredProperties;
			}
		}

		public bool IsAbstract
		{
			get
			{
				return this.isAbstract;
			}
		}

		public bool IsOpen
		{
			get
			{
				return this.isOpen;
			}
		}

		protected IDictionary<string, IEdmProperty> PropertiesDictionary
		{
			get
			{
				return this.propertiesDictionary.GetValue(this, EdmStructuredType.ComputePropertiesDictionaryFunc, null);
			}
		}

		static EdmStructuredType()
		{
			EdmStructuredType.ComputePropertiesDictionaryFunc = (EdmStructuredType me) => me.ComputePropertiesDictionary();
		}

		protected EdmStructuredType(bool isAbstract, bool isOpen, IEdmStructuredType baseStructuredType)
		{
			this.declaredProperties = new List<IEdmProperty>();
			this.propertiesDictionary = new Cache<EdmStructuredType, IDictionary<string, IEdmProperty>>();
			this.isAbstract = isAbstract;
			this.isOpen = isOpen;
			this.baseStructuredType = baseStructuredType;
		}

		public void AddProperty(IEdmProperty property)
		{
			EdmUtil.CheckArgumentNull<IEdmProperty>(property, "property");
			if (object.ReferenceEquals(this, property.DeclaringType))
			{
				this.declaredProperties.Add(property);
				this.propertiesDictionary.Clear(null);
				return;
			}
			else
			{
				throw new InvalidOperationException(Strings.EdmModel_Validator_Semantic_DeclaringTypeMustBeCorrect(property.Name));
			}
		}

		public EdmStructuralProperty AddStructuralProperty(string name, EdmPrimitiveTypeKind type)
		{
			EdmStructuralProperty edmStructuralProperty = new EdmStructuralProperty(this, name, EdmCoreModel.Instance.GetPrimitive(type, true));
			this.AddProperty(edmStructuralProperty);
			return edmStructuralProperty;
		}

		public EdmStructuralProperty AddStructuralProperty(string name, EdmPrimitiveTypeKind type, bool isNullable)
		{
			EdmStructuralProperty edmStructuralProperty = new EdmStructuralProperty(this, name, EdmCoreModel.Instance.GetPrimitive(type, isNullable));
			this.AddProperty(edmStructuralProperty);
			return edmStructuralProperty;
		}

		public EdmStructuralProperty AddStructuralProperty(string name, IEdmTypeReference type)
		{
			EdmStructuralProperty edmStructuralProperty = new EdmStructuralProperty(this, name, type);
			this.AddProperty(edmStructuralProperty);
			return edmStructuralProperty;
		}

		public EdmStructuralProperty AddStructuralProperty(string name, IEdmTypeReference type, string defaultValue, EdmConcurrencyMode concurrencyMode)
		{
			EdmStructuralProperty edmStructuralProperty = new EdmStructuralProperty(this, name, type, defaultValue, concurrencyMode);
			this.AddProperty(edmStructuralProperty);
			return edmStructuralProperty;
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
			if (this.PropertiesDictionary.TryGetValue(name, out edmProperty))
			{
				return edmProperty;
			}
			else
			{
				return null;
			}
		}
	}
}