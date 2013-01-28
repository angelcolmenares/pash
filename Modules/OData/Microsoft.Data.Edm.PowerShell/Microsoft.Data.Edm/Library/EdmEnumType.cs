using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Values;
using System;
using System.Collections.Generic;

namespace Microsoft.Data.Edm.Library
{
	internal class EdmEnumType : EdmType, IEdmEnumType, IEdmSchemaType, IEdmSchemaElement, IEdmNamedElement, IEdmVocabularyAnnotatable, IEdmType, IEdmElement
	{
		private readonly IEdmPrimitiveType underlyingType;

		private readonly string namespaceName;

		private readonly string name;

		private readonly bool isFlags;

		private readonly List<IEdmEnumMember> members;

		public bool IsFlags
		{
			get
			{
				return this.isFlags;
			}
		}

		public IEnumerable<IEdmEnumMember> Members
		{
			get
			{
				return this.members;
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
				return EdmSchemaElementKind.TypeDefinition;
			}
		}

		public override EdmTypeKind TypeKind
		{
			get
			{
				return EdmTypeKind.Enum;
			}
		}

		public IEdmPrimitiveType UnderlyingType
		{
			get
			{
				return this.underlyingType;
			}
		}

		public EdmEnumType(string namespaceName, string name) : this(namespaceName, name, false)
		{
		}

		public EdmEnumType(string namespaceName, string name, bool isFlags) : this(namespaceName, name, (EdmPrimitiveTypeKind)10, isFlags)
		{
		}

		public EdmEnumType(string namespaceName, string name, EdmPrimitiveTypeKind underlyingType, bool isFlags) : this(namespaceName, name, EdmCoreModel.Instance.GetPrimitiveType(underlyingType), isFlags)
		{
		}

		public EdmEnumType(string namespaceName, string name, IEdmPrimitiveType underlyingType, bool isFlags)
		{
			this.members = new List<IEdmEnumMember>();
			EdmUtil.CheckArgumentNull<IEdmPrimitiveType>(underlyingType, "underlyingType");
			EdmUtil.CheckArgumentNull<string>(namespaceName, "namespaceName");
			EdmUtil.CheckArgumentNull<string>(name, "name");
			this.underlyingType = underlyingType;
			this.name = name;
			this.namespaceName = namespaceName;
			this.isFlags = isFlags;
		}

		public void AddMember(IEdmEnumMember member)
		{
			this.members.Add(member);
		}

		public EdmEnumMember AddMember(string name, IEdmPrimitiveValue value)
		{
			EdmEnumMember edmEnumMember = new EdmEnumMember(this, name, value);
			this.AddMember(edmEnumMember);
			return edmEnumMember;
		}
	}
}