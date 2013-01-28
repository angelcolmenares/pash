using Microsoft.Management.Odata;
using Microsoft.Management.Odata.Common;
using Microsoft.Management.Odata.Core;
using Microsoft.Management.Odata.GenericInvoke;
using Microsoft.Management.Odata.MofParser;
using Microsoft.Management.Odata.MofParser.Parsers;
using Microsoft.Management.Odata.MofParser.ParseTree;
using Microsoft.Management.Odata.PS;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.Services.Providers;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Data.Entity.Core;

namespace Microsoft.Management.Odata.Schema
{
	internal class SchemaLoader
	{
		public const string PswsNamespaceName = "http://schemas.microsoft.com/powershell-web-services/2010/09";

		public const string ComplexTypeQualifier = "ComplexType";

		public const string EtagQualifier = "Etag";

		public const string AssociationClassQualifier = "AssociationClass";

		public const string ToEndQualifier = "ToEnd";

		public const string RequiredQualifier = "Required";

		public const string KeyQualifier = "Key";

		public const string EmbeddedInstanceQualifier = "EmbeddedInstance";

		public const string AssociationQualifier = "Association";

		public const string ConcurrencyModeFacetName = "ConcurrencyMode";

		private readonly HashSet<string> supportedQualifiers;

		private string basePath;

		public SchemaLoader()
		{
			this.basePath = Utils.GetBaseContentDirectory();
			this.supportedQualifiers = new HashSet<string>();
			this.supportedQualifiers.Add("Key");
			this.supportedQualifiers.Add("Required");
			this.supportedQualifiers.Add("EmbeddedInstance");
			this.supportedQualifiers.Add("Etag");
			this.supportedQualifiers.Add("ComplexType");
			this.supportedQualifiers.Add("Association");
			this.supportedQualifiers.Add("AssociationClass");
			this.supportedQualifiers.Add("ToEnd");
		}

		private static void AddReferenceProperties(Schema schema, List<ResourceType> entityResources, ClassDeclaration classDeclaration, HashSet<MofProduction> mof)
		{
			ResourceType resourceType = schema.ResourceTypes[SchemaLoader.TransformCimNameToCsdl(classDeclaration.Name.FullName, true)];
			foreach (PropertyDeclaration property in classDeclaration.Properties)
			{
				Func<ResourceProperty, bool> func = null;
				Func<MofProduction, bool> func1 = null;
				Qualifier qualifier = property.GetQualifier("AssociationClass");
				Qualifier qualifier1 = property.GetQualifier("ToEnd");
				if (property.IsReferenceType())
				{
					if (qualifier == null || qualifier1 == null)
					{
						SchemaLoader.ThrowPropertyMetadataException(classDeclaration.Name.FullName, property.Name, Resources.RefWithoutAssociationQual);
					}
					if (qualifier.Parameter == null || qualifier.Parameter as string == null)
					{
						SchemaLoader.ThrowPropertyMetadataException(classDeclaration.Name.FullName, property.Name, Resources.AssociationQualWithoutParm);
					}
					string parameter = qualifier.Parameter as string;
					Schema.AssociationType associationType = null;
					if (!schema.AssociationTypes.TryGetValue(SchemaLoader.TransformCimNameToCsdl(parameter, true), out associationType))
					{
						object[] objArray = new object[1];
						objArray[0] = qualifier.Parameter as string;
						string str = string.Format(CultureInfo.CurrentCulture, Resources.MissingAssociationType, objArray);
						SchemaLoader.ThrowPropertyMetadataException(classDeclaration.Name.FullName, property.Name, str);
					}
					ResourceType referencedResourceType = SchemaLoader.GetReferencedResourceType(property, classDeclaration.Name.FullName, entityResources);
					Schema.AssociationEnd associationEnd = associationType.Ends.FirstOrDefault<Schema.AssociationEnd>((Schema.AssociationEnd it) => string.Equals(it.Name, qualifier.Parameter as string, StringComparison.Ordinal));
					if (associationEnd == null)
					{
						SchemaLoader.ThrowPropertyMetadataException(classDeclaration.Name.FullName, property.Name, Resources.MissingAssociationEnd);
					}
					if (associationEnd.Type != referencedResourceType)
					{
						object[] cim = new object[2];
						cim[0] = SchemaLoader.TransformCsdlNameToCim(referencedResourceType.FullName);
						cim[1] = SchemaLoader.TransformCsdlNameToCim(associationEnd.Type.FullName);
						string str1 = string.Format(CultureInfo.CurrentCulture, Resources.WrongAssocEndDataType, cim);
						SchemaLoader.ThrowPropertyMetadataException(classDeclaration.Name.FullName, property.Name, str1);
					}
					if (property.DataType.Type != DataTypeType.ObjectReference)
					{
						schema.AddResourceSetReferenceProperty(resourceType, property.Name, referencedResourceType, associationType);
					}
					else
					{
						HashSet<MofProduction> mofProductions = mof;
						if (func1 == null)
						{
							func1 = (MofProduction item) => item.GetFullClassName() == SchemaLoader.TransformCsdlNameToCim(classDeclaration.Name.FullName); /* TODO: not sure of classDeclaration here */
						}
						ClassDeclaration classDeclaration1 = mofProductions.First<MofProduction>(func1) as ClassDeclaration;
						object explicitDefaultValue = null;
						if (referencedResourceType.KeyProperties.Count == 1)
						{
							string name = referencedResourceType.KeyProperties.First<ResourceProperty>().Name;
							PropertyDeclaration propertyDeclaration = classDeclaration1.GetProperty(name, mof);
							explicitDefaultValue = TypeSystem.GetExplicitDefaultValue(SchemaLoader.GetClrType(propertyDeclaration));
						}
						schema.AddResourceReferenceProperty(resourceType, property.Name, referencedResourceType, associationType, explicitDefaultValue);
					}
					Schema.AssociationEnd associationEnd1 = associationType.Ends.First<Schema.AssociationEnd>((Schema.AssociationEnd it) => !string.Equals(it.Name, qualifier1.Parameter as string));
					Schema.AssociationEnd associationEnd2 = associationEnd1;
					ReadOnlyCollection<ResourceProperty> properties = resourceType.Properties;
					if (func == null)
					{
						func = (ResourceProperty it) => string.Equals(it.Name, property.Name);
					}
					associationEnd2.Property = properties.First<ResourceProperty>(func);
				}
				else
				{
					if (qualifier == null && qualifier1 == null)
					{
						continue;
					}
					SchemaLoader.ThrowPropertyMetadataException(classDeclaration.Name.FullName, property.Name, Resources.AssociationQualWithoutRef);
				}
			}
		}

		internal void AddToSchema(Schema schema, MofSpecificationSet mof, XElement resources)
		{
			this.VerifyQualifierNames(mof);
			this.VerifyClassNames(mof);
			HashSet<MofProduction> mofProductions = new HashSet<MofProduction>();
			foreach (XElement xElement in resources.Elements())
			{
				mof.GetClosureOfClass(mofProductions, xElement.GetUniqueElement("http://schemas.microsoft.com/powershell-web-services/2010/09", "Class").Value);
			}
			List<ResourceType> resourceTypes = new List<ResourceType>();
			List<ResourceType> resourceTypes1 = new List<ResourceType>();
			HashSet<MofProduction> mofProductions1 = mofProductions;
			foreach (MofProduction mofProduction in mofProductions1.Where<MofProduction>((MofProduction item) => item is ClassDeclaration))
			{
				SchemaLoader.CreateResourceType(mofProduction as ClassDeclaration, mofProductions, schema, resourceTypes, resourceTypes1);
			}
			foreach (MofProduction mofProduction1 in mofProductions)
			{
				ClassDeclaration classDeclaration = mofProduction1 as ClassDeclaration;
				if (classDeclaration == null)
				{
					continue;
				}
				SchemaLoader.ClassCategory category = classDeclaration.GetCategory();
				switch (category)
				{
					case SchemaLoader.ClassCategory.Complex:
					{
						SchemaLoader.PopulateComplexType(schema, resourceTypes, classDeclaration);
						continue;
					}
					case SchemaLoader.ClassCategory.Entity:
					{
						SchemaLoader.PopulateEntityType(schema, resourceTypes, resourceTypes1, classDeclaration);
						continue;
					}
					case SchemaLoader.ClassCategory.Association:
					{
						SchemaLoader.PopulateAssociationType(schema, resourceTypes1, classDeclaration);
						continue;
					}
				}
				throw new NotImplementedException(string.Concat("class category ", classDeclaration.GetCategory()));
			}
			foreach (XElement xElement1 in resources.Elements())
			{
				string value = xElement1.GetUniqueElement("http://schemas.microsoft.com/powershell-web-services/2010/09", "RelativeUrl").Value;
				string str = xElement1.GetUniqueElement("http://schemas.microsoft.com/powershell-web-services/2010/09", "Class").Value;
				ResourceType resourceType = resourceTypes1.Find((ResourceType item) => item.FullName == SchemaLoader.TransformCimNameToCsdl(str, true));
				if (resourceType != null)
				{
					schema.AddResourceSet(value, resourceType);
				}
				else
				{
					object[] csdl = new object[1];
					csdl[0] = SchemaLoader.TransformCimNameToCsdl(str, true);
					throw new ArgumentException(ExceptionHelpers.GetExceptionMessage(Resources.EntityTypeResourceNotFound, csdl));
				}
			}
			foreach (MofProduction mofProduction2 in mofProductions)
			{
				if (mofProduction2 as ClassDeclaration == null || (mofProduction2 as ClassDeclaration).GetCategory () != SchemaLoader.ClassCategory.Entity)
				{
					continue;
				}
				SchemaLoader.AddReferenceProperties(schema, resourceTypes1, mofProduction2 as ClassDeclaration, mofProductions);
			}
			foreach (Schema.AssociationType associationType in schema.AssociationTypes.Values)
			{
				associationType.CreateWcfType(schema.ResourceSets);
			}
		}

		private static Type CheckForPrimitiveEmbeddedType(Qualifier embeddedObject, bool nullable)
		{
			Type type = null;
			if (embeddedObject == null)
			{
				return null;
			}
			else
			{
				if (!string.Equals(embeddedObject.Parameter as string, "EDM_GUID", StringComparison.OrdinalIgnoreCase))
				{
					if (!string.Equals(embeddedObject.Parameter as string, "EDM_Binary", StringComparison.OrdinalIgnoreCase))
					{
						return null;
					}
					else
					{
						type = typeof(byte[]);
					}
				}
				else
				{
					type = typeof(Guid);
				}
				if (!nullable || !(type != typeof(string)) || !(type != typeof(byte[])))
				{
					return type;
				}
				else
				{
					Type[] typeArray = new Type[1];
					typeArray[0] = type;
					return typeof(Nullable<>).MakeGenericType(typeArray);
				}
			}
		}

		internal void CheckForUnrecognizedQualifiers(NodeList<Qualifier> qualifierList)
		{
			foreach (Qualifier qualifier in qualifierList)
			{
				Func<string, bool> func = null;
				HashSet<string> strs = this.supportedQualifiers;
				if (func == null)
				{
					func = (string item) => string.Equals(item, qualifier.Name, StringComparison.OrdinalIgnoreCase);
				}
				if (strs.Any<string>(func))
				{
					continue;
				}
				object[] name = new object[2];
				name[0] = qualifier.Name;
				DocumentRange location = qualifier.Location;
				name[1] = location.ToString();
				throw new MetadataException(ExceptionHelpers.GetExceptionMessage(Resources.UnknownQualifier, name));
			}
		}

		private static Schema.AssociationEnd CreateAssociationEnd(string className, PropertyDeclaration property, List<ResourceType> entityResources)
		{
			if (!property.IsReferenceType())
			{
				SchemaLoader.ThrowPropertyMetadataException(className, property.Name, Resources.AssociationEndType);
			}
			if (property.DataType.Type == DataTypeType.ObjectReferenceArray)
			{
				SchemaLoader.ThrowPropertyMetadataException(className, property.Name, Resources.NeedSingularReference);
			}
			ResourceType referencedResourceType = SchemaLoader.GetReferencedResourceType(property, className, entityResources);
			return new Schema.AssociationEnd(property.Name, referencedResourceType);
		}

		private static PSReferenceSetCmdletInfo CreateGetRefSetCmdlet(ResourceType type, XElement element)
		{
			XElement uniqueElement = element.GetUniqueElement("http://schemas.microsoft.com/powershell-web-services/2010/09", "Cmdlet");
			XElement xElement = element.GetUniqueElement("http://schemas.microsoft.com/powershell-web-services/2010/09", "ParameterForThisObject");
			PSReferenceSetCmdletInfo pSReferenceSetCmdletInfo = new PSReferenceSetCmdletInfo(uniqueElement.Value);
			PSParameterSet pSParameterSet = new PSParameterSet("Default");
			foreach (XElement xElement1 in xElement.Nodes())
			{
				XElement uniqueElement1 = xElement1.GetUniqueElement("http://schemas.microsoft.com/powershell-web-services/2010/09", "ParameterName");
				pSParameterSet.Parameters.Add(uniqueElement1.Value);
			}
			pSReferenceSetCmdletInfo.ParameterSets.Add(pSParameterSet);
			SchemaLoader.PopulateReferringFieldParameterMapping(type, "ParameterForThisObject", pSReferenceSetCmdletInfo, xElement);
			XElement xElement2 = element.TryGetUniqueElement("http://schemas.microsoft.com/powershell-web-services/2010/09", "Options");
			if (xElement2 != null)
			{
				foreach (XElement xElement3 in xElement2.Nodes())
				{
					pSParameterSet.Parameters.Add(xElement3.Value);
				}
				SchemaLoader.PopulateCmdletUrlOptions(type, pSReferenceSetCmdletInfo, xElement2);
			}
			return pSReferenceSetCmdletInfo;
		}

		private static PSReferenceSetCmdletInfo CreateRefSetCmdlet(ResourceType otherResourceType, ResourceType type, XElement element)
		{
			XElement uniqueElement = element.GetUniqueElement("http://schemas.microsoft.com/powershell-web-services/2010/09", "Cmdlet");
			XElement xElement = element.GetUniqueElement("http://schemas.microsoft.com/powershell-web-services/2010/09", "ParameterForThisObject");
			XElement uniqueElement1 = element.GetUniqueElement("http://schemas.microsoft.com/powershell-web-services/2010/09", "ParameterForReferredObject");
			PSReferenceSetCmdletInfo pSReferenceSetCmdletInfo = new PSReferenceSetCmdletInfo(uniqueElement.Value);
			PSParameterSet pSParameterSet = new PSParameterSet("Default");
			foreach (XElement xElement1 in xElement.Nodes())
			{
				XElement uniqueElement2 = xElement1.GetUniqueElement("http://schemas.microsoft.com/powershell-web-services/2010/09", "ParameterName");
				pSParameterSet.Parameters.Add(uniqueElement2.Value);
			}
			foreach (XElement xElement2 in uniqueElement1.Nodes())
			{
				XElement uniqueElement3 = xElement2.GetUniqueElement("http://schemas.microsoft.com/powershell-web-services/2010/09", "ParameterName");
				pSParameterSet.Parameters.Add(uniqueElement3.Value);
			}
			pSReferenceSetCmdletInfo.ParameterSets.Add(pSParameterSet);
			SchemaLoader.PopulateReferringFieldParameterMapping(type, "ParameterForThisObject", pSReferenceSetCmdletInfo, xElement);
			SchemaLoader.PopulateReferredFieldParameterMapping(otherResourceType, "ParameterForReferredObject", pSReferenceSetCmdletInfo, uniqueElement1);
			XElement xElement3 = element.TryGetUniqueElement("http://schemas.microsoft.com/powershell-web-services/2010/09", "Options");
			if (xElement3 != null)
			{
				foreach (XElement xElement4 in xElement3.Nodes())
				{
					pSParameterSet.Parameters.Add(xElement4.Value);
				}
				SchemaLoader.PopulateCmdletUrlOptions(type, pSReferenceSetCmdletInfo, xElement3);
			}
			return pSReferenceSetCmdletInfo;
		}

		private static void CreateResourceType(ClassDeclaration c, HashSet<MofProduction> mof, Schema schema, List<ResourceType> complexTypeResources, List<ResourceType> entityResources)
		{
			Func<MofProduction, bool> func = null;
			if (!SchemaLoader.IsTypeDefined(c, complexTypeResources, entityResources, schema.AssociationTypes))
			{
				if (!c.IsClassAndSuperclassesContainsLoop(mof))
				{
					SchemaLoader.ClassCategory category = c.GetCategory();
					ResourceType resourceType = null;
					if (c.SuperclassName != null)
					{
						HashSet<MofProduction> mofProductions = mof;
						if (func == null)
						{
							func = (MofProduction item) => string.Equals(c.SuperclassName.FullName, item.GetFullClassName(), StringComparison.Ordinal);
						}
						ClassDeclaration classDeclaration = mofProductions.FirstOrDefault<MofProduction>(func) as ClassDeclaration;
						if (classDeclaration != null)
						{
							SchemaLoader.ClassCategory classCategory = classDeclaration.GetCategory();
							if (category == classCategory)
							{
								if (!SchemaLoader.IsTypeDefined(classDeclaration, complexTypeResources, entityResources, schema.AssociationTypes))
								{
									SchemaLoader.CreateResourceType(classDeclaration, mof, schema, complexTypeResources, entityResources);
								}
								if (!schema.ResourceTypes.TryGetValue(SchemaLoader.TransformCimNameToCsdl(classDeclaration.Name.FullName, true), out resourceType))
								{
									object[] fullName = new object[2];
									fullName[0] = classDeclaration.Name.FullName;
									fullName[1] = c.Name.FullName;
									throw new MetadataException(ExceptionHelpers.GetExceptionMessage(Resources.BaseClassNotFound, fullName));
								}
							}
							else
							{
								object[] superclassName = new object[2];
								superclassName[0] = c.Name.FullName;
								superclassName[1] = c.SuperclassName;
								throw new MetadataException(ExceptionHelpers.GetExceptionMessage(Resources.CrossCategoryDerivation, superclassName));
							}
						}
						else
						{
							object[] objArray = new object[2];
							objArray[0] = c.Name.FullName;
							objArray[1] = c.SuperclassName;
							throw new MetadataException(ExceptionHelpers.GetExceptionMessage(Resources.MissingBaseClass, objArray));
						}
					}
					SchemaLoader.ClassCategory category1 = c.GetCategory();
					switch (category1)
					{
						case SchemaLoader.ClassCategory.Complex:
						{
							complexTypeResources.Add(schema.AddResourceType(c.Name.Identifier, ResourceTypeKind.ComplexType, SchemaLoader.TransformCimNameToCsdl(c.Name.Schema, false), resourceType, null));
							return;
						}
						case SchemaLoader.ClassCategory.Entity:
						{
							entityResources.Add(schema.AddResourceType(c.Name.Identifier, ResourceTypeKind.EntityType, SchemaLoader.TransformCimNameToCsdl(c.Name.Schema, false), resourceType, null));
							return;
						}
						case SchemaLoader.ClassCategory.Association:
						{
							schema.AddAssociationType(SchemaLoader.TransformCimNameToCsdl(c.Name.Identifier, false), SchemaLoader.TransformCimNameToCsdl(c.Name.Schema, false));
							return;
						}
					}
					throw new NotImplementedException(string.Concat("class category ", c.GetCategory()));
				}
				else
				{
					object[] identifier = new object[1];
					identifier[0] = c.Name.Identifier;
					throw new MetadataException(ExceptionHelpers.GetExceptionMessage(Resources.BaseClassesNotPresentOrRecursive, identifier));
				}
			}
			else
			{
				return;
			}
		}

		private static Type GetClrType(PropertyDeclaration property)
		{
			Type clrType = property.DataType.Type.GetClrType();
			if (!SchemaLoader.IsNullableProperty(property) || !(clrType != typeof(string)))
			{
				return clrType;
			}
			else
			{
				Type[] typeArray = new Type[1];
				typeArray[0] = clrType;
				return typeof(Nullable<>).MakeGenericType(typeArray);
			}
		}

		private static ResourceType GetReferencedResourceType(PropertyDeclaration property, string className, List<ResourceType> entityResources)
		{
			ObjectReference elementType = null;
			if (property.DataType.Type != DataTypeType.ObjectReference)
			{
				if (property.DataType.Type != DataTypeType.ObjectReferenceArray)
				{
					throw new ArgumentException("the property must be an object reference or an array thereof", "property");
				}
				else
				{
					ArrayType dataType = property.DataType as ArrayType;
					elementType = dataType.ElementType as ObjectReference;
				}
			}
			else
			{
				elementType = property.DataType as ObjectReference;
			}
			string csdl = null;
			try
			{
				csdl = SchemaLoader.TransformCimNameToCsdl(elementType.Name.FullName, true);
			}
			catch (MetadataException metadataException1)
			{
				MetadataException metadataException = metadataException1;
				SchemaLoader.ThrowPropertyMetadataException(className, property.Name, metadataException.Message);
			}
			ResourceType resourceType = entityResources.FirstOrDefault<ResourceType>((ResourceType item) => string.Equals(item.FullName, csdl, StringComparison.Ordinal));
			if (resourceType == null)
			{
				object[] fullName = new object[1];
				fullName[0] = elementType.Name.FullName;
				string str = string.Format(CultureInfo.CurrentCulture, Resources.MissingResourceType, fullName);
				SchemaLoader.ThrowPropertyMetadataException(className, property.Name, str);
			}
			return resourceType;
		}

		internal Schema InterpretSchema(string mofFileName, string mofData, string mappingFileName, string mappingData, bool allowInvoke = false)
		{
			ValidationEventHandler validationEventHandler = null;
			MofSpecification mofSpecification = null;
			try
			{
				mofSpecification = MofFileParser.ParseMofFile(mofData, mofFileName);
			}
			catch (ParseFailureException parseFailureException1)
			{
				ParseFailureException parseFailureException = parseFailureException1;
				TraceHelper.Current.SchemaFileNotValidCsdl(mofFileName);
				throw new InvalidSchemaException(mofFileName, parseFailureException.Message, parseFailureException);
			}
			XDocument xDocument = null;
			try
			{
				XmlReader xmlReader = XmlReader.Create(new StringReader(Encoding.ASCII.GetString(Resources.pswsSchema)));
				XmlSchemaSet xmlSchemaSet = new XmlSchemaSet();
				xmlSchemaSet.Add("http://schemas.microsoft.com/powershell-web-services/2010/09", xmlReader);
				XmlReaderSettings xmlReaderSetting = new XmlReaderSettings();
				xmlReaderSetting.IgnoreComments = true;
				xmlReaderSetting.IgnoreProcessingInstructions = true;
				xmlReaderSetting.IgnoreWhitespace = true;
				xmlReaderSetting.XmlResolver = null;
				xmlReaderSetting.ValidationType = ValidationType.Schema;
				xmlReaderSetting.Schemas.Add(xmlSchemaSet);
				XmlReaderSettings xmlReaderSetting1 = xmlReaderSetting;
				if (validationEventHandler == null)
				{
					validationEventHandler = (object sender, ValidationEventArgs args) => {
						object[] message = new object[2];
						message[0] = mappingFileName;
						message[1] = args.Exception.Message;
						throw new MetadataException(ExceptionHelpers.GetExceptionMessage(args.Exception, Resources.InvalidResourceMappingFile, message), args.Exception);
					}
					;
				}
				xmlReaderSetting1.ValidationEventHandler += validationEventHandler;
				XmlReader xmlReader1 = XmlReader.Create(new StringReader(mappingData), xmlReaderSetting);
				xDocument = XDocument.Load(xmlReader1);
			}
			catch (XmlException xmlException1)
			{
				XmlException xmlException = xmlException1;
				TraceHelper.Current.SchemaFileNotValidCsdl(mappingFileName);
				throw new InvalidSchemaException(mappingFileName, xmlException.Message, xmlException);
			}
			catch (MetadataException metadataException1)
			{
				MetadataException metadataException = metadataException1;
				TraceHelper.Current.SchemaFileNotValidCsdl(mappingFileName);
				throw new InvalidSchemaException(mappingFileName, metadataException.Message, metadataException);
			}
			string str = Encoding.ASCII.GetString(Resources.CsdlForWellKnownTypes);
			MofSpecification mofSpecification1 = MofFileParser.ParseMofFile(str.AsEnumerable<char>(), "(built-in types)");
			List<MofSpecification> mofSpecifications = new List<MofSpecification>();
			mofSpecifications.Add(mofSpecification1);
			mofSpecifications.Add(mofSpecification);
			return this.LoadSchema(mofSpecifications, xDocument, allowInvoke);
		}

		private static bool IsNullableProperty(PropertyDeclaration property)
		{
			Qualifier qualifier = property.GetQualifier("Key");
			Qualifier qualifier1 = property.GetQualifier("Required");
			if (qualifier == null)
			{
				if (qualifier1 != null)
				{
					if (qualifier1.Parameter != null)
					{
						string parameter = qualifier1.Parameter as string;
						if (parameter != null)
						{
							if (!string.Equals(parameter, "true", StringComparison.OrdinalIgnoreCase))
							{
								if (!string.Equals(parameter, "false", StringComparison.OrdinalIgnoreCase))
								{
									throw new MetadataException(string.Concat("incorrect qualifier value", property.ToString()));
								}
								else
								{
									return true;
								}
							}
							else
							{
								return false;
							}
						}
						else
						{
							throw new MetadataException(string.Concat("incorrect qualifier value", property.ToString()));
						}
					}
					else
					{
						return false;
					}
				}
				else
				{
					return true;
				}
			}
			else
			{
				return false;
			}
		}

		private static bool IsTypeDefined(ClassDeclaration c, List<ResourceType> complexTypeResources, List<ResourceType> entityResources, Dictionary<string, Schema.AssociationType> associationResources)
		{
			Func<ResourceType, bool> func = null;
			Func<ResourceType, bool> func1 = null;
			SchemaLoader.ClassCategory category = c.GetCategory();
			switch (category)
			{
				case SchemaLoader.ClassCategory.Complex:
				{
					object obj = null;
					List<ResourceType> resourceTypes = complexTypeResources;
					if (func1 == null)
					{
						func1 = (ResourceType it) => string.Equals(it.FullName, SchemaLoader.TransformCimNameToCsdl(c.Name.FullName, true));
					}
					return obj != resourceTypes.FirstOrDefault<ResourceType>(func1);
				}
				case SchemaLoader.ClassCategory.Entity:
				{
					object obj1 = null;
					List<ResourceType> resourceTypes1 = entityResources;
					if (func == null)
					{
						func = (ResourceType it) => string.Equals(it.FullName, SchemaLoader.TransformCimNameToCsdl(c.Name.FullName, true));
					}
					return obj1 != resourceTypes1.FirstOrDefault<ResourceType>(func);
				}
				case SchemaLoader.ClassCategory.Association:
				{
					return associationResources.Keys.Contains<string>(SchemaLoader.TransformCimNameToCsdl(c.Name.FullName, true));
				}
			}
			throw new NotImplementedException(string.Concat("class category ", c.GetCategory()));
		}

		internal Schema LoadSchema(List<MofSpecification> mofs, XDocument dispatch, bool allowInvoke = false)
		{
			string value = dispatch.GetUniqueElement("http://schemas.microsoft.com/powershell-web-services/2010/09", "SchemaNamespace").Value;
			string str = dispatch.GetUniqueElement("http://schemas.microsoft.com/powershell-web-services/2010/09", "ContainerName").Value;
			Schema schema = new Schema(str, value);
			if (allowInvoke)
			{
				string str1 = Encoding.ASCII.GetString(Resources.genericInvoke);
				MofSpecification mofSpecification = MofFileParser.ParseMofFile(str1.AsEnumerable<char>(), "(command invocation types)");
				mofs.Add(mofSpecification);
				XDocument xDocument = this.XDocumentFromString(Encoding.ASCII.GetString(Resources.GenericInvokeMapping));
				dispatch.InsertResourceMapping(xDocument);
			}
			XElement uniqueElement = dispatch.GetUniqueElement("http://schemas.microsoft.com/powershell-web-services/2010/09", "Resources");
			this.AddToSchema(schema, new MofSpecificationSet(mofs.ToArray()), uniqueElement);
			schema.FreezeSchema();
			try
			{
				foreach (ResourceType property in schema.ResourceTypes.Values)
				{
					IEnumerator<ResourceProperty> enumerator = property.Properties.GetEnumerator();
					using (enumerator)
					{
						while (enumerator.MoveNext())
						{
							//property;
						}
					}
				}
			}
			catch (InvalidOperationException invalidOperationException1)
			{
				InvalidOperationException invalidOperationException = invalidOperationException1;
				throw new MetadataException(ExceptionHelpers.GetExceptionMessage(invalidOperationException.Message, new object[0]), invalidOperationException);
			}
			if (allowInvoke)
			{
				SchemaLoader.PopulateInvocationEntityMetadata(schema);
			}
			SchemaLoader.PopulateResourceMetadata(schema, dispatch);
			schema.Trace("New schema loaded in memory ");
			return schema;
		}

		internal Schema LoadSchemaFiles(string mofFileName, string mappingFileName, bool allowInvoke = true)
		{
			if (!Path.IsPathRooted(mofFileName))
			{
				mofFileName = Path.Combine(this.basePath, mofFileName);
			}
			if (!Path.IsPathRooted(mappingFileName))
			{
				mappingFileName = Path.Combine(this.basePath, mappingFileName);
			}
			string str = null;
			try
			{
				str = File.ReadAllText(mofFileName);
			}
			catch (FileNotFoundException fileNotFoundException)
			{
				TraceHelper.Current.SchemaFileNotFound(mofFileName);
				throw;
			}
			string str1 = null;
			try
			{
				str1 = File.ReadAllText(mappingFileName);
			}
			catch (FileNotFoundException fileNotFoundException1)
			{
				TraceHelper.Current.SchemaFileNotFound(mappingFileName);
				throw;
			}
			return this.InterpretSchema(mofFileName, str, mappingFileName, str1, allowInvoke);
		}

		private static void PopulateAssociationType(Schema schema, List<ResourceType> entityResources, ClassDeclaration classDeclaration)
		{
			Schema.AssociationType item = schema.AssociationTypes[SchemaLoader.TransformCimNameToCsdl(classDeclaration.Name.FullName, true)];
			if (classDeclaration.Properties.Count == 2)
			{
				Schema.AssociationEnd associationEnd = SchemaLoader.CreateAssociationEnd(classDeclaration.Name.FullName, classDeclaration.Properties.ElementAt<PropertyDeclaration>(0), entityResources);
				Schema.AssociationEnd associationEnd1 = SchemaLoader.CreateAssociationEnd(classDeclaration.Name.FullName, classDeclaration.Properties.ElementAt<PropertyDeclaration>(1), entityResources);
				item.AddEnds(associationEnd, associationEnd1);
				return;
			}
			else
			{
				object[] fullName = new object[1];
				fullName[0] = classDeclaration.Name.FullName;
				string str = string.Format(CultureInfo.CurrentCulture, Resources.AssociationEndCount, fullName);
				throw new MetadataException(str);
			}
		}

		private static void PopulateCmdletFieldParameterMapping(ResourceType type, string xmlNodeName, PSCmdletInfo cmdletInfo, XElement fieldParameterMappingRoot)
		{
			XElement firstNode = (XElement)fieldParameterMappingRoot.FirstNode;
			while (firstNode != null)
			{
				string value = firstNode.GetUniqueElement("http://schemas.microsoft.com/powershell-web-services/2010/09", "FieldName").Value;
				string str = firstNode.GetUniqueElement("http://schemas.microsoft.com/powershell-web-services/2010/09", "ParameterName").Value;
				if (type.Properties.FirstOrDefault<ResourceProperty>((ResourceProperty it) => it.Name == value) != null)
				{
					cmdletInfo.FieldParameterMapping.Add(value, str);
					firstNode = (XElement)firstNode.NextNode;
				}
				else
				{
					object[] name = new object[3];
					name[0] = type.Name;
					name[1] = xmlNodeName;
					name[2] = value;
					throw new MetadataException(ExceptionHelpers.GetExceptionMessage(Resources.InvalidFieldInParameterMapping, name));
				}
			}
		}

		private static void PopulateCmdletInformation(ResourceType type, Microsoft.Management.Odata.Core.CommandType commandType, XElement entityXml, string xmlNodeName, PSEntityMetadata entityMetadata)
		{
			XNamespace xNamespace = "http://schemas.microsoft.com/powershell-web-services/2010/09";
			XElement xElement = entityXml.Elements(xNamespace + xmlNodeName).FirstOrDefault<XElement>();
			if (xElement != null)
			{
				entityMetadata.AddCmdlet(commandType, xElement.GetUniqueElement("http://schemas.microsoft.com/powershell-web-services/2010/09", "Cmdlet").Value);
				SchemaLoader.PopulateCmdletFieldParameterMapping(type, xmlNodeName, entityMetadata.Cmdlets[commandType], xElement.GetUniqueElement("http://schemas.microsoft.com/powershell-web-services/2010/09", "FieldParameterMap"));
				XElement xElement1 = xElement.TryGetUniqueElement("http://schemas.microsoft.com/powershell-web-services/2010/09", "Options");
				if (xElement1 != null)
				{
					SchemaLoader.PopulateCmdletUrlOptions(type, entityMetadata.Cmdlets[commandType], xElement1);
				}
				XElement xElement2 = xElement.TryGetUniqueElement("http://schemas.microsoft.com/powershell-web-services/2010/09", "ImmutableParameters");
				if (xElement2 != null)
				{
					SchemaLoader.PopulateCmdletMandatoryParameters(type, entityMetadata.Cmdlets[commandType], xElement2);
				}
				SchemaLoader.PopulateParameterSets(type, entityMetadata.Cmdlets[commandType], xElement.GetUniqueElement("http://schemas.microsoft.com/powershell-web-services/2010/09", "ParameterSets"));
				return;
			}
			else
			{
				return;
			}
		}

		private static void PopulateCmdletMandatoryParameters(ResourceType resourceType, PSCmdletInfo cmdletInfo, XElement mandatoryParamsRoot)
		{
			XElement firstNode = (XElement)mandatoryParamsRoot.FirstNode;
			while (firstNode != null)
			{
				if (string.Equals(firstNode.Name.LocalName, "ParameterValue", StringComparison.OrdinalIgnoreCase))
				{
					string value = firstNode.GetUniqueElement("http://schemas.microsoft.com/powershell-web-services/2010/09", "ParameterName").Value;
					string str = firstNode.GetUniqueElement("http://schemas.microsoft.com/powershell-web-services/2010/09", "Value").Value;
					cmdletInfo.ImmutableParameters.Add(value, str);
					firstNode = (XElement)firstNode.NextNode;
				}
				else
				{
					object[] localName = new object[2];
					localName[0] = "ParameterValue";
					localName[1] = firstNode.Name.LocalName;
					throw new MetadataException(ExceptionHelpers.GetExceptionMessage(Resources.InvalidXmlTag, localName));
				}
			}
		}

		private static void PopulateCmdletReferenceSetInfo(ResourceType type, XElement entityXml, PSEntityMetadata entityMetadata)
		{
			ResourceType resourceType;
			XNamespace xNamespace = "http://schemas.microsoft.com/powershell-web-services/2010/09";
			XElement xElement = entityXml.Elements(xNamespace + "Associations").FirstOrDefault<XElement>();
			if (xElement != null)
			{
				foreach (XElement xElement1 in xElement.Elements())
				{
					string value = xElement1.GetUniqueElement("http://schemas.microsoft.com/powershell-web-services/2010/09", "Name").Value;
					ResourceProperty resourceProperty = type.Properties.FirstOrDefault<ResourceProperty>((ResourceProperty it) => string.Equals(it.Name, value, StringComparison.Ordinal));
					if (resourceProperty != null)
					{
						ReferenceCustomState customState = resourceProperty.CustomState as ReferenceCustomState;
						if (customState != null)
						{
							PSReferenceSetCmdletInfo pSReferenceSetCmdletInfo = null;
							PSReferenceSetCmdletInfo pSReferenceSetCmdletInfo1 = null;
							PSReferenceSetCmdletInfo pSReferenceSetCmdletInfo2 = null;
							PSEntityMetadata.ReferenceSetCmdlets.ReferencePropertyType referencePropertyType = PSEntityMetadata.ReferenceSetCmdlets.ReferencePropertyType.Instance;
							bool flag = false;
							XElement xElement2 = xElement1.TryGetUniqueElement("http://schemas.microsoft.com/powershell-web-services/2010/09", "PropertyType");
							if (xElement2 != null)
							{
								referencePropertyType = (PSEntityMetadata.ReferenceSetCmdlets.ReferencePropertyType)Enum.Parse(typeof(PSEntityMetadata.ReferenceSetCmdlets.ReferencePropertyType), xElement2.Value.ToString());
								flag = true;
							}
							if (customState.AssociationType.Ends[0].Type != type)
							{
								resourceType = customState.AssociationType.Ends[0].Type;
							}
							else
							{
								resourceType = customState.AssociationType.Ends[1].Type;
							}
							ResourceType resourceType1 = resourceType;
							if (resourceType1.KeyProperties.Count <= 1 || referencePropertyType != PSEntityMetadata.ReferenceSetCmdlets.ReferencePropertyType.KeyOnly)
							{
								xElement2 = xElement1.TryGetUniqueElement("http://schemas.microsoft.com/powershell-web-services/2010/09", "AddReference");
								if (xElement2 != null)
								{
									pSReferenceSetCmdletInfo1 = SchemaLoader.CreateRefSetCmdlet(resourceType1, type, xElement2);
								}
								xElement2 = xElement1.TryGetUniqueElement("http://schemas.microsoft.com/powershell-web-services/2010/09", "RemoveReference");
								if (xElement2 != null)
								{
									pSReferenceSetCmdletInfo2 = SchemaLoader.CreateRefSetCmdlet(resourceType1, type, xElement2);
								}
								xElement2 = xElement1.TryGetUniqueElement("http://schemas.microsoft.com/powershell-web-services/2010/09", "GetReference");
								if (xElement2 != null)
								{
									if (!flag)
									{
										pSReferenceSetCmdletInfo = SchemaLoader.CreateGetRefSetCmdlet(type, xElement2);
									}
									else
									{
										object[] name = new object[2];
										name[0] = value;
										name[1] = type.Name;
										string str = string.Format(CultureInfo.CurrentCulture, Resources.ContainsBothPropertyTypeAndGetReference, name);
										throw new MetadataException(str);
									}
								}
								if (customState.IsSet || pSReferenceSetCmdletInfo1 == null && pSReferenceSetCmdletInfo2 == null && pSReferenceSetCmdletInfo == null)
								{
									entityMetadata.AddCmdletsForReference(resourceProperty.Name, referencePropertyType, pSReferenceSetCmdletInfo1, pSReferenceSetCmdletInfo2, pSReferenceSetCmdletInfo);
								}
								else
								{
									object[] objArray = new object[2];
									objArray[0] = type.Name;
									objArray[1] = value;
									string str1 = string.Format(CultureInfo.CurrentCulture, Resources.RefSetCmdletsOnNonRefSet, objArray);
									throw new MetadataException(str1);
								}
							}
							else
							{
								object[] name1 = new object[3];
								name1[0] = resourceType1.Name;
								name1[1] = value;
								name1[2] = type.Name;
								string str2 = string.Format(CultureInfo.CurrentCulture, Resources.CompoundForeignKeysCannotEmbeddedAsKeys, name1);
								throw new MetadataException(str2);
							}
						}
						else
						{
							object[] objArray1 = new object[2];
							objArray1[0] = type.Name;
							objArray1[1] = value;
							string str3 = string.Format(CultureInfo.CurrentCulture, Resources.RefSetCmdletsOnNonRefSet, objArray1);
							throw new MetadataException(str3);
						}
					}
					else
					{
						object[] name2 = new object[3];
						name2[0] = type.Name;
						name2[1] = "Associations";
						name2[2] = value;
						string str4 = string.Format(CultureInfo.CurrentCulture, Resources.InvalidFieldInParameterMapping, name2);
						throw new MetadataException(str4);
					}
				}
				return;
			}
			else
			{
				return;
			}
		}

		private static void PopulateCmdletUrlOptions(ResourceType resourceType, PSCmdletInfo cmdletInfo, XElement urlOptionsRoot)
		{
			XElement firstNode = (XElement)urlOptionsRoot.FirstNode;
			while (firstNode != null)
			{
				if (string.Equals(firstNode.Name.LocalName, "ParameterName", StringComparison.OrdinalIgnoreCase))
				{
					cmdletInfo.Options.Add(firstNode.Value);
					firstNode = (XElement)firstNode.NextNode;
				}
				else
				{
					object[] localName = new object[2];
					localName[0] = "ParameterName";
					localName[1] = firstNode.Name.LocalName;
					throw new MetadataException(ExceptionHelpers.GetExceptionMessage(Resources.InvalidXmlTag, localName));
				}
			}
		}

		private static void PopulateComplexType(Schema schema, List<ResourceType> complexTypeResources, ClassDeclaration classDeclaration)
		{
			ResourceType resourceType = complexTypeResources.Find((ResourceType item) => item.FullName == SchemaLoader.TransformCimNameToCsdl(classDeclaration.Name.FullName, true));
			if (resourceType != null)
			{
				foreach (PropertyDeclaration property in classDeclaration.Properties)
				{
					bool qualifier = property.GetQualifier("Key") != null;
					bool flag = property.GetQualifier("Etag") != null;
					bool flag1 = SchemaLoader.IsNullableProperty(property);
					Qualifier qualifier1 = property.GetQualifier("EmbeddedInstance");
					if (qualifier)
					{
						SchemaLoader.ThrowPropertyMetadataException(classDeclaration.Name.FullName, property.Name, Resources.ComplexTypeSpecifiesKeyProperty);
					}
					if (flag)
					{
						SchemaLoader.ThrowPropertyMetadataException(classDeclaration.Name.FullName, property.Name, Resources.ComplexTypeSpecifiesEtagProperty);
					}
					Type clrType = SchemaLoader.CheckForPrimitiveEmbeddedType(qualifier1, flag1);
					if (qualifier1 == null || clrType != null)
					{
						bool flag2 = property.DataType.IsArray();
						if (clrType == null)
						{
							clrType = SchemaLoader.GetClrType(property);
						}
						object defaultValue = TypeSystem.GetDefaultValue(clrType);
						if (flag2)
						{
							schema.AddPrimitiveCollectionProperty(resourceType, property.Name, clrType, defaultValue);
						}
						else
						{
							schema.AddPrimitiveProperty(resourceType, property.Name, clrType, flag, defaultValue);
						}
					}
					else
					{
						bool flag3 = property.DataType.IsArray();
						string csdl = SchemaLoader.TransformCimNameToCsdl(qualifier1.Parameter as string, true);
						ResourceType resourceType1 = complexTypeResources.Find((ResourceType item) => item.FullName == csdl);
						if (resourceType1 != null)
						{
							if (flag3)
							{
								schema.AddComplexCollectionProperty(resourceType, property.Name, resourceType1);
							}
							else
							{
								schema.AddComplexProperty(resourceType, property.Name, resourceType1);
							}
						}
						else
						{
							object[] objArray = new object[1];
							objArray[0] = csdl;
							throw new ArgumentException(ExceptionHelpers.GetExceptionMessage(Resources.ComplexTypeResourceNotFound, objArray));
						}
					}
				}
				return;
			}
			else
			{
				object[] csdl1 = new object[1];
				csdl1[0] = SchemaLoader.TransformCimNameToCsdl(classDeclaration.Name.FullName, true);
				throw new ArgumentException(ExceptionHelpers.GetExceptionMessage(Resources.ComplexTypeResourceNotFound, csdl1));
			}
		}

		private static void PopulateEntityType(Schema schema, List<ResourceType> complexTypeResources, List<ResourceType> entityResources, ClassDeclaration classDeclaration)
		{
			ResourceType resourceType = schema.ResourceTypes[SchemaLoader.TransformCimNameToCsdl(classDeclaration.Name.FullName, true)];
			foreach (PropertyDeclaration property in classDeclaration.Properties)
			{
				if (property.IsReferenceType())
				{
					continue;
				}
				bool qualifier = property.GetQualifier("Key") != null;
				bool flag = property.GetQualifier("Etag") != null;
				bool flag1 = SchemaLoader.IsNullableProperty(property);
				Qualifier qualifier1 = property.GetQualifier("EmbeddedInstance");
				if (qualifier && flag)
				{
					SchemaLoader.ThrowPropertyMetadataException(classDeclaration.Name.FullName, property.Name, Resources.PropertySpecifiesKeyWithEtag);
				}
				Type clrType = SchemaLoader.CheckForPrimitiveEmbeddedType(qualifier1, flag1);
				if (qualifier1 == null || clrType != null)
				{
					bool flag2 = property.DataType.IsArray();
					if (clrType == null)
					{
						clrType = SchemaLoader.GetClrType(property);
					}
					object defaultValue = TypeSystem.GetDefaultValue(clrType);
					if (flag2)
					{
						schema.AddPrimitiveCollectionProperty(resourceType, property.Name, clrType, defaultValue);
					}
					else
					{
						if (!qualifier)
						{
							schema.AddPrimitiveProperty(resourceType, property.Name, clrType, flag, defaultValue);
						}
						else
						{
							defaultValue = TypeSystem.GetExplicitDefaultValue(clrType);
							schema.AddKeyProperty(resourceType, property.Name, clrType, defaultValue);
						}
					}
				}
				else
				{
					bool flag3 = property.DataType.IsArray();
					if (flag)
					{
						SchemaLoader.ThrowPropertyMetadataException(classDeclaration.Name.FullName, property.Name, Resources.ComplexPropertySpecifiesEtag);
					}
					string csdl = SchemaLoader.TransformCimNameToCsdl(qualifier1.Parameter as string, true);
					ResourceType resourceType1 = complexTypeResources.Find((ResourceType item) => item.FullName == csdl);
					if (resourceType1 != null)
					{
						if (flag3)
						{
							schema.AddComplexCollectionProperty(resourceType, property.Name, resourceType1);
						}
						else
						{
							schema.AddComplexProperty(resourceType, property.Name, resourceType1);
						}
					}
					else
					{
						object[] objArray = new object[1];
						objArray[0] = csdl;
						throw new ArgumentException(ExceptionHelpers.GetExceptionMessage(Resources.ComplexTypeResourceNotFound, objArray));
					}
				}
			}
		}

		private static void PopulateInvocationEntityMetadata(Schema schema)
		{
			GIEntityMetadata gIEntityMetadatum = new GIEntityMetadata();
			schema.EntityMetadataDictionary.Add(SchemaLoader.TransformCimNameToCsdl("PowerShell_CommandInvocation", true), gIEntityMetadatum);
		}

		private static void PopulateParameterSets(ResourceType resourceType, PSCmdletInfo cmdletInfo, XElement parameterSetsNodeRoot)
		{
			XNamespace xNamespace = "http://schemas.microsoft.com/powershell-web-services/2010/09";
			XElement firstNode = (XElement)parameterSetsNodeRoot.FirstNode;
			while (firstNode != null)
			{
				PSParameterSet pSParameterSet = new PSParameterSet(firstNode.GetUniqueElement("http://schemas.microsoft.com/powershell-web-services/2010/09", "Name").Value);
				foreach (XElement xElement in firstNode.Elements(xNamespace + "Parameter"))
				{
					string value = xElement.GetUniqueElement("http://schemas.microsoft.com/powershell-web-services/2010/09", "Name").Value;
					XElement xElement1 = xElement.TryGetUniqueElement("http://schemas.microsoft.com/powershell-web-services/2010/09", "IsSwitch");
					bool flag = false;
					if (xElement1 != null)
					{
						try
						{
							flag = (bool)TypeConverter.ConvertTo(xElement1.Value, typeof(bool));
						}
						catch (InvalidCastException invalidCastException1)
						{
							InvalidCastException invalidCastException = invalidCastException1;
							object[] objArray = new object[1];
							objArray[0] = xElement1.Value;
							throw new MetadataException(ExceptionHelpers.GetExceptionMessage(invalidCastException, Resources.InvalidSwitchParameterValue, objArray), invalidCastException);
						}
					}
					XElement xElement2 = xElement.TryGetUniqueElement("http://schemas.microsoft.com/powershell-web-services/2010/09", "IsMandatory");
					bool flag1 = false;
					if (xElement2 != null)
					{
						try
						{
							flag1 = (bool)TypeConverter.ConvertTo(xElement2.Value, typeof(bool));
						}
						catch (InvalidCastException invalidCastException3)
						{
							InvalidCastException invalidCastException2 = invalidCastException3;
							object[] value1 = new object[1];
							value1[0] = xElement2.Value;
							throw new MetadataException(ExceptionHelpers.GetExceptionMessage(invalidCastException2, Resources.InvalidSwitchParameterValue, value1), invalidCastException2);
						}
					}
					string str = null;
					XElement xElement3 = xElement.TryGetUniqueElement("http://schemas.microsoft.com/powershell-web-services/2010/09", "Type");
					if (xElement3 != null)
					{
						str = xElement3.Value;
					}
					pSParameterSet.Parameters.Add(value, new PSParameterInfo(flag, flag1, str));
				}
				firstNode = (XElement)firstNode.NextNode;
				cmdletInfo.ParameterSets.Add(pSParameterSet);
			}
			try
			{
				cmdletInfo.ThrowIfInvalidState(resourceType);
			}
			catch (InvalidOperationException invalidOperationException1)
			{
				InvalidOperationException invalidOperationException = invalidOperationException1;
				object[] cmdletName = new object[2];
				cmdletName[0] = cmdletInfo.CmdletName;
				cmdletName[1] = invalidOperationException.Message;
				throw new MetadataException(ExceptionHelpers.GetExceptionMessage(invalidOperationException, Resources.FieldOptionOrMandatoryParameterNotInParamset, cmdletName), invalidOperationException);
			}
		}

		private static void PopulateReferredFieldParameterMapping(ResourceType otherResourceType, string xmlNodeName, PSReferenceSetCmdletInfo cmdletInfo, XElement fieldParameterMappingRoot)
		{
			XElement firstNode = (XElement)fieldParameterMappingRoot.FirstNode;
			while (firstNode != null)
			{
				string value = firstNode.GetUniqueElement("http://schemas.microsoft.com/powershell-web-services/2010/09", "FieldName").Value;
				string str = firstNode.GetUniqueElement("http://schemas.microsoft.com/powershell-web-services/2010/09", "ParameterName").Value;
				if (otherResourceType.KeyProperties.FirstOrDefault<ResourceProperty>((ResourceProperty it) => it.Name == value) != null)
				{
					cmdletInfo.ReferredObjectParameterMapping.Add(value, str);
					firstNode = (XElement)firstNode.NextNode;
				}
				else
				{
					object[] name = new object[3];
					name[0] = otherResourceType.Name;
					name[1] = xmlNodeName;
					name[2] = value;
					throw new MetadataException(ExceptionHelpers.GetExceptionMessage(Resources.InvalidFieldInParameterMapping, name));
				}
			}
		}

		private static void PopulateReferringFieldParameterMapping(ResourceType type, string xmlNodeName, PSReferenceSetCmdletInfo cmdletInfo, XElement fieldParameterMappingRoot)
		{
			XElement firstNode = (XElement)fieldParameterMappingRoot.FirstNode;
			while (firstNode != null)
			{
				string value = firstNode.GetUniqueElement("http://schemas.microsoft.com/powershell-web-services/2010/09", "FieldName").Value;
				string str = firstNode.GetUniqueElement("http://schemas.microsoft.com/powershell-web-services/2010/09", "ParameterName").Value;
				if (type.KeyProperties.FirstOrDefault<ResourceProperty>((ResourceProperty it) => it.Name == value) != null)
				{
					cmdletInfo.ReferringObjectParameterMapping.Add(value, str);
					firstNode = (XElement)firstNode.NextNode;
				}
				else
				{
					object[] name = new object[3];
					name[0] = type.Name;
					name[1] = xmlNodeName;
					name[2] = value;
					throw new MetadataException(ExceptionHelpers.GetExceptionMessage(Resources.InvalidFieldInParameterMapping, name));
				}
			}
		}

		private static void PopulateResourceMetadata(Schema schema, XDocument dispatchXml)
		{
			XElement uniqueElement = dispatchXml.GetUniqueElement("http://schemas.microsoft.com/powershell-web-services/2010/09", "ClassImplementations");
			foreach (XElement xElement in uniqueElement.Elements())
			{
				string value = xElement.GetUniqueElement("http://schemas.microsoft.com/powershell-web-services/2010/09", "Name").Value;
				ResourceType resourceType = null;
				if (schema.ResourceTypes.TryGetValue(SchemaLoader.TransformCimNameToCsdl(value, true), out resourceType))
				{
					XElement xElement1 = xElement.TryGetUniqueElement("http://schemas.microsoft.com/powershell-web-services/2010/09", "ClrType");
					if (xElement1 != null)
					{
						resourceType.SetClrTypeStr(xElement1.Value);
					}
					XElement xElement2 = xElement.TryGetUniqueElement("http://schemas.microsoft.com/powershell-web-services/2010/09", "RenamedFields");
					if (xElement2 != null)
					{
						foreach (XElement xElement3 in xElement2.Elements())
						{
							string str = xElement3.GetUniqueElement("http://schemas.microsoft.com/powershell-web-services/2010/09", "SchemaProperty").Value;
							string value1 = xElement3.GetUniqueElement("http://schemas.microsoft.com/powershell-web-services/2010/09", "PowerShellProperty").Value;
							ResourceProperty resourceProperty = resourceType.Properties.FirstOrDefault<ResourceProperty>((ResourceProperty it) => string.Equals(str, it.Name, StringComparison.Ordinal));
							if (resourceProperty != null)
							{
								resourceProperty.GetCustomState().PsProperty = value1;
							}
							else
							{
								object[] objArray = new object[2];
								objArray[0] = value;
								objArray[1] = str;
								throw new MetadataException(ExceptionHelpers.GetExceptionMessage(Resources.InvalidRenamedProperty, objArray));
							}
						}
					}
					schema.ResourceTypes.Values.Where<ResourceType>((ResourceType item) => item.BaseType == resourceType).ToList<ResourceType>().ForEach((ResourceType item) => resourceType.AddDerivedType(item));
					if (resourceType.ResourceTypeKind != ResourceTypeKind.EntityType)
					{
						continue;
					}
					PSEntityMetadata pSEntityMetadatum = new PSEntityMetadata();
					XElement uniqueElement1 = xElement.GetUniqueElement("http://schemas.microsoft.com/powershell-web-services/2010/09", "CmdletImplementation");
					SchemaLoader.PopulateCmdletInformation(resourceType, Microsoft.Management.Odata.Core.CommandType.Read, uniqueElement1, "Query", pSEntityMetadatum);
					SchemaLoader.PopulateCmdletInformation(resourceType, Microsoft.Management.Odata.Core.CommandType.Create, uniqueElement1, "Create", pSEntityMetadatum);
					SchemaLoader.PopulateCmdletInformation(resourceType, Microsoft.Management.Odata.Core.CommandType.Update, uniqueElement1, "Update", pSEntityMetadatum);
					SchemaLoader.PopulateCmdletInformation(resourceType, Microsoft.Management.Odata.Core.CommandType.Delete, uniqueElement1, "Delete", pSEntityMetadatum);
					SchemaLoader.PopulateCmdletReferenceSetInfo(resourceType, uniqueElement1, pSEntityMetadatum);
					schema.EntityMetadataDictionary.Add(SchemaLoader.TransformCimNameToCsdl(value, true), pSEntityMetadatum);
				}
				else
				{
					object[] objArray1 = new object[1];
					objArray1[0] = value;
					throw new MetadataException(ExceptionHelpers.GetExceptionMessage(Resources.MappingClassNotFound, objArray1));
				}
			}
			foreach (ResourceType resourceType1 in schema.ResourceTypes.Values)
			{
				if (resourceType1.ResourceTypeKind != ResourceTypeKind.EntityType)
				{
					continue;
				}
				if (!schema.EntityMetadataDictionary.ContainsKey(resourceType1.FullName))
				{
					schema.EntityMetadataDictionary.Add(resourceType1.FullName, new PSEntityMetadata());
				}
				if (schema.EntityMetadataDictionary[resourceType1.FullName] as PSEntityMetadata == null)
				{
					continue;
				}
				SchemaLoader.UpdateEntityPropertyMetadata(schema.EntityMetadataDictionary[resourceType1.FullName] as PSEntityMetadata, resourceType1);
			}
		}

		private static void ThrowPropertyMetadataException(string className, string propertyName, string flaw)
		{
			object[] objArray = new object[3];
			objArray[0] = propertyName;
			objArray[1] = className;
			objArray[2] = flaw;
			throw new MetadataException(ExceptionHelpers.GetExceptionMessage(Resources.PropertyDeclarationProblem, objArray));
		}

		private static string TransformCimNameToCsdl(string cimName, bool isFullName)
		{
			int num;
			string str;
			string cimFullNameInImproperFormat;
			if (isFullName)
			{
				char[] chrArray = new char[1];
				chrArray[0] = '\u005F';
				string[] strArrays = cimName.Split(chrArray);
				if (isFullName)
				{
					num = 2;
				}
				else
				{
					num = 1;
				}
				int num1 = num;
				if (strArrays.Count<string>() >= num1)
				{
					string str1 = strArrays[0];
					for (int i = 1; i < strArrays.Count<string>(); i++)
					{
						string str2 = str1;
						if (i == 1)
						{
							str = ".";
						}
						else
						{
							str = "_";
						}
						str1 = string.Concat(str2, str, strArrays[i]);
					}
					return str1;
				}
				else
				{
					CultureInfo currentCulture = CultureInfo.CurrentCulture;
					if (isFullName)
					{
						cimFullNameInImproperFormat = Resources.CimFullNameInImproperFormat;
					}
					else
					{
						cimFullNameInImproperFormat = Resources.CimSchemaNameInImproperFormat;
					}
					object[] objArray = new object[1];
					objArray[0] = cimName;
					string str3 = string.Format(currentCulture, cimFullNameInImproperFormat, objArray);
					throw new MetadataException(str3);
				}
			}
			else
			{
				return cimName;
			}
		}

		private static string TransformCsdlNameToCim(string csdlName)
		{
			return csdlName.Replace('.', '\u005F');
		}

		private static void UpdateEntityPropertyMetadata(PSEntityMetadata metadata, ResourceType resourceType)
		{
			foreach (ResourceProperty property in resourceType.Properties)
			{
				bool flag = false;
				if (property.Kind != ResourcePropertyKind.ResourceSetReference)
				{
					if ((property.Kind & ResourcePropertyKind.Key) != ResourcePropertyKind.Key && metadata.Cmdlets.ContainsKey(Microsoft.Management.Odata.Core.CommandType.Update))
					{
						if (metadata.Cmdlets[Microsoft.Management.Odata.Core.CommandType.Update].FieldParameterMapping.ContainsKey(property.Name))
						{
							flag = true;
						}
						if (!flag)
						{
							foreach (PSParameterSet parameterSet in metadata.Cmdlets[Microsoft.Management.Odata.Core.CommandType.Update].ParameterSets)
							{
								if (!parameterSet.Parameters.ContainsKey(property.Name))
								{
									continue;
								}
								flag = true;
								break;
							}
						}
					}
				}
				else
				{
					if (metadata.CmdletsForReferenceSets.Count > 0)
					{
						flag = true;
					}
				}
				property.GetCustomState().IsUpdatable = flag;
			}
		}

		internal void VerifyClassNames(MofSpecificationSet mof)
		{
			foreach (ClassDeclaration value in mof.ClassDeclarations.Values)
			{
				if (!value.Name.Identifier.Contains<char>('\u005F') && !string.IsNullOrEmpty(value.Name.Schema))
				{
					continue;
				}
				object[] fullName = new object[1];
				fullName[0] = value.Name.FullName;
				throw new MetadataException(ExceptionHelpers.GetExceptionMessage(Resources.UseCimClassName, fullName));
			}
		}

		internal void VerifyQualifierNames(MofSpecificationSet mof)
		{
			foreach (ClassDeclaration property in mof.ClassDeclarations.Values)
			{
				this.CheckForUnrecognizedQualifiers(property.Qualifiers);
				IEnumerator<PropertyDeclaration> enumerator = property.Properties.GetEnumerator();
				using (enumerator)
				{
					while (enumerator.MoveNext())
					{
						PropertyDeclaration propertyDeclaration = enumerator.Current;
						this.CheckForUnrecognizedQualifiers(propertyDeclaration.Qualifiers);
					}
				}
			}
		}

		internal XDocument XDocumentFromString(string content)
		{
			XmlReaderSettings xmlReaderSetting = new XmlReaderSettings();
			xmlReaderSetting.IgnoreComments = true;
			xmlReaderSetting.IgnoreProcessingInstructions = true;
			xmlReaderSetting.IgnoreWhitespace = true;
			xmlReaderSetting.XmlResolver = null;
			XmlReader xmlReader = XmlReader.Create(new StringReader(content), xmlReaderSetting);
			return XDocument.Load(xmlReader);
		}

		internal enum ClassCategory
		{
			Complex,
			Entity,
			Association
		}
	}
}