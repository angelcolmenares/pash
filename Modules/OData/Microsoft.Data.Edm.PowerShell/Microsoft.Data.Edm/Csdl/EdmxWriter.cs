using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Csdl.Internal.Serialization;
using Microsoft.Data.Edm.Library;
using Microsoft.Data.Edm.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace Microsoft.Data.Edm.Csdl
{
	internal class EdmxWriter
	{
		private readonly IEdmModel model;

		private readonly IEnumerable<EdmSchema> schemas;

		private readonly XmlWriter writer;

		private readonly Version edmxVersion;

		private readonly string edmxNamespace;

		private readonly EdmxTarget target;

		private EdmxWriter(IEdmModel model, IEnumerable<EdmSchema> schemas, XmlWriter writer, Version edmxVersion, EdmxTarget target)
		{
			this.model = model;
			this.schemas = schemas;
			this.writer = writer;
			this.edmxVersion = edmxVersion;
			this.target = target;
			this.edmxNamespace = CsdlConstants.SupportedEdmxVersions[edmxVersion];
		}

		private void EndElement()
		{
			this.writer.WriteEndElement();
		}

		public static bool TryWriteEdmx(IEdmModel model, XmlWriter writer, EdmxTarget target, out IEnumerable<EdmError> errors)
		{
			EdmUtil.CheckArgumentNull<IEdmModel>(model, "model");
			EdmUtil.CheckArgumentNull<XmlWriter>(writer, "writer");
			errors = model.GetSerializationErrors();
			if (errors.FirstOrDefault<EdmError>() == null)
			{
				Version edmxVersion = model.GetEdmxVersion();
				if (edmxVersion == null)
				{
					Dictionary<Version, Version> edmToEdmxVersions = CsdlConstants.EdmToEdmxVersions;
					Version edmVersion = model.GetEdmVersion();
					Version edmVersionLatest = edmVersion;
					if (edmVersion == null)
					{
						edmVersionLatest = EdmConstants.EdmVersionLatest;
					}
					if (!edmToEdmxVersions.TryGetValue(edmVersionLatest, out edmxVersion))
					{
						EdmError[] edmError = new EdmError[1];
						edmError[0] = new EdmError(new CsdlLocation(0, 0), EdmErrorCode.UnknownEdmVersion, Strings.Serializer_UnknownEdmVersion);
						errors = edmError;
						return false;
					}
				}
				else
				{
					if (!CsdlConstants.SupportedEdmxVersions.ContainsKey(edmxVersion))
					{
						EdmError[] edmErrorArray = new EdmError[1];
						edmErrorArray[0] = new EdmError(new CsdlLocation(0, 0), EdmErrorCode.UnknownEdmxVersion, Strings.Serializer_UnknownEdmxVersion);
						errors = edmErrorArray;
						return false;
					}
				}
				IEnumerable<EdmSchema> schemas = (new EdmModelSchemaSeparationSerializationVisitor(model)).GetSchemas();
				EdmxWriter edmxWriter = new EdmxWriter(model, schemas, writer, edmxVersion, target);
				edmxWriter.WriteEdmx();
				errors = Enumerable.Empty<EdmError>();
				return true;
			}
			else
			{
				return false;
			}
		}

		private void WriteConceptualModelsElement()
		{
			this.writer.WriteStartElement("edmx", "ConceptualModels", this.edmxNamespace);
		}

		private void WriteDataServicesElement()
		{
			this.writer.WriteStartElement("edmx", "DataServices", this.edmxNamespace);
			Version dataServiceVersion = this.model.GetDataServiceVersion();
			if (dataServiceVersion != null)
			{
				this.writer.WriteAttributeString("m", "DataServiceVersion", "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata", dataServiceVersion.ToString());
			}
			Version maxDataServiceVersion = this.model.GetMaxDataServiceVersion();
			if (maxDataServiceVersion != null)
			{
				this.writer.WriteAttributeString("m", "MaxDataServiceVersion", "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata", maxDataServiceVersion.ToString());
			}
		}

		private void WriteEdmx()
		{
			EdmxTarget edmxTarget = this.target;
			switch (edmxTarget)
			{
				case EdmxTarget.EntityFramework:
				{
					this.WriteEFEdmx();
					return;
				}
				case EdmxTarget.OData:
				{
					this.WriteODataEdmx();
					return;
				}
			}
			throw new InvalidOperationException(Strings.UnknownEnumVal_EdmxTarget(this.target.ToString()));
		}

		private void WriteEdmxElement()
		{
			this.writer.WriteStartElement("edmx", "Edmx", this.edmxNamespace);
			this.writer.WriteAttributeString("Version", this.edmxVersion.ToString());
		}

		private void WriteEFEdmx()
		{
			this.WriteEdmxElement();
			this.WriteRuntimeElement();
			this.WriteConceptualModelsElement();
			this.WriteSchemas();
			this.EndElement();
			this.EndElement();
			this.EndElement();
		}

		private void WriteODataEdmx()
		{
			this.WriteEdmxElement();
			this.WriteDataServicesElement();
			this.WriteSchemas();
			this.EndElement();
			this.EndElement();
		}

		private void WriteRuntimeElement()
		{
			this.writer.WriteStartElement("edmx", "Runtime", this.edmxNamespace);
		}

		private void WriteSchemas()
		{
			Version edmVersion = this.model.GetEdmVersion();
			Version edmVersionLatest = edmVersion;
			if (edmVersion == null)
			{
				edmVersionLatest = EdmConstants.EdmVersionLatest;
			}
			Version version = edmVersionLatest;
			foreach (EdmSchema schema in this.schemas)
			{
				EdmModelCsdlSerializationVisitor edmModelCsdlSerializationVisitor = new EdmModelCsdlSerializationVisitor(this.model, this.writer, version);
				edmModelCsdlSerializationVisitor.VisitEdmSchema(schema, this.model.GetNamespacePrefixMappings());
			}
		}
	}
}