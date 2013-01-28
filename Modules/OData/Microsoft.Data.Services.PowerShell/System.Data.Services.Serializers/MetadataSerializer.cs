namespace System.Data.Services.Serializers
{
    using Microsoft.Data.Edm;
    using Microsoft.Data.Edm.Csdl;
    using Microsoft.Data.Edm.Validation;
    using Microsoft.Data.OData;
    using System;
    using System.Collections.Generic;
    using System.Data.Services;
    using System.Data.Services.Providers;
    using System.Linq;
    using System.Text;

    internal sealed class MetadataSerializer
    {
        private static readonly ValidationRule[] additionalSchemaValidationRules = new ValidationRule[] { MetadataProviderUtils.PropertyNameIncludesReservedODataCharacters };
        private static readonly Version edmxVersion = new Version(1, 0);
        private static readonly ValidationRule[] excludedSchemaValidationRules = new ValidationRule[] { ValidationRules.NamedElementNameIsNotAllowed, ValidationRules.NavigationPropertyAssociationEndNameIsValid, ValidationRules.NavigationPropertyAssociationNameIsValid, ValidationRules.EntitySetAssociationSetNameMustBeValid, ValidationRules.EntityTypeEntityKeyMustNotBeBinaryBeforeV2, ValidationRules.TypeAnnotationInaccessibleTerm, ValidationRules.ValueAnnotationInaccessibleTerm };
        private readonly ODataMessageWriter messageWriter;

        internal MetadataSerializer(ODataMessageWriter messageWriter)
        {
            this.messageWriter = messageWriter;
        }

        internal static void ValidateModel(IEdmModel model, Version edmSchemaVersion)
        {
            IEnumerable<EdmError> enumerable;
            ValidationRuleSet ruleSet = new ValidationRuleSet(ValidationRuleSet.GetEdmModelRuleSet(edmSchemaVersion).Except<ValidationRule>(excludedSchemaValidationRules).Concat<ValidationRule>(additionalSchemaValidationRules));
            model.Validate(ruleSet, out enumerable);
            if ((enumerable != null) && enumerable.Any<EdmError>())
            {
                StringBuilder builder = new StringBuilder();
                foreach (EdmError error in enumerable)
                {
                    builder.AppendLine(error.ToString());
                }
                throw new DataServiceException(500, System.Data.Services.Strings.MetadataSerializer_ModelValidationErrors(builder.ToString()));
            }
        }

        internal void WriteMetadataDocument(IDataService service)
        {
            IEdmModel metadataModel = service.Provider.GetMetadataModel(service.OperationContext);
            metadataModel.SetDataServiceVersion(service.Provider.GetMetadataVersion(service.OperationContext));
            metadataModel.SetMaxDataServiceVersion(service.Configuration.DataServiceBehavior.MaxProtocolVersion.ToVersion());
            Version version = service.Provider.GetMetadataEdmSchemaVersion(service.OperationContext).ToVersion();
            metadataModel.SetEdmVersion(version);
            metadataModel.SetEdmxVersion(edmxVersion);
            if (!service.Configuration.DisableValidationOnMetadataWrite)
            {
                ValidateModel(metadataModel, version);
            }
            this.messageWriter.WriteMetadataDocument();
        }
    }
}

