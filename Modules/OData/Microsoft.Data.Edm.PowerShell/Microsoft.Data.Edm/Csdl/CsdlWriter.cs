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
	internal static class CsdlWriter
	{
		public static bool TryWriteCsdl(this IEdmModel model, XmlWriter writer, out IEnumerable<EdmError> errors)
		{
			return CsdlWriter.TryWriteCsdl(model, (string x) => writer, true, out errors);
		}

		public static bool TryWriteCsdl(this IEdmModel model, Func<string, XmlWriter> writerProvider, out IEnumerable<EdmError> errors)
		{
			return CsdlWriter.TryWriteCsdl(model, writerProvider, false, out errors);
		}

		internal static bool TryWriteCsdl(IEdmModel model, Func<string, XmlWriter> writerProvider, bool singleFileExpected, out IEnumerable<EdmError> errors)
		{
			EdmUtil.CheckArgumentNull<IEdmModel>(model, "model");
			EdmUtil.CheckArgumentNull<Func<string, XmlWriter>>(writerProvider, "writerProvider");
			errors = model.GetSerializationErrors();
			if (errors.FirstOrDefault<EdmError>() == null)
			{
				IEnumerable<EdmSchema> schemas = (new EdmModelSchemaSeparationSerializationVisitor(model)).GetSchemas();
				if (schemas.Count<EdmSchema>() <= 1 || !singleFileExpected)
				{
					if (schemas.Count<EdmSchema>() != 0)
					{
						CsdlWriter.WriteSchemas(model, schemas, writerProvider);
						errors = Enumerable.Empty<EdmError>();
						return true;
					}
					else
					{
						EdmError[] edmError = new EdmError[1];
						edmError[0] = new EdmError(new CsdlLocation(0, 0), EdmErrorCode.NoSchemasProduced, Strings.Serializer_NoSchemasProduced);
						errors = edmError;
						return false;
					}
				}
				else
				{
					EdmError[] edmErrorArray = new EdmError[1];
					edmErrorArray[0] = new EdmError(new CsdlLocation(0, 0), EdmErrorCode.SingleFileExpected, Strings.Serializer_SingleFileExpected);
					errors = edmErrorArray;
					return false;
				}
			}
			else
			{
				return false;
			}
		}

		internal static void WriteSchemas(IEdmModel model, IEnumerable<EdmSchema> schemas, Func<string, XmlWriter> writerProvider)
		{
			Version edmVersion = model.GetEdmVersion();
			Version edmVersionLatest = edmVersion;
			if (edmVersion == null)
			{
				edmVersionLatest = EdmConstants.EdmVersionLatest;
			}
			Version version = edmVersionLatest;
			foreach (EdmSchema schema in schemas)
			{
				XmlWriter @namespace = writerProvider(schema.Namespace);
				if (@namespace == null)
				{
					continue;
				}
				EdmModelCsdlSerializationVisitor edmModelCsdlSerializationVisitor = new EdmModelCsdlSerializationVisitor(model, @namespace, version);
				edmModelCsdlSerializationVisitor.VisitEdmSchema(schema, model.GetNamespacePrefixMappings());
			}
		}
	}
}