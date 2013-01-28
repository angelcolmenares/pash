namespace Microsoft.PowerShell.Commands.Internal.Format
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Management.Automation;
    using System.Management.Automation.Runspaces;

    internal sealed class WideViewGenerator : ViewGenerator
    {
        internal override FormatEntryData GeneratePayload(PSObject so, int enumerationLimit)
        {
            FormatEntryData data = new FormatEntryData();
            if (base.dataBaseInfo.view != null)
            {
                data.formatEntryInfo = this.GenerateWideViewEntryFromDataBaseInfo(so, enumerationLimit);
                return data;
            }
            data.formatEntryInfo = this.GenerateWideViewEntryFromProperties(so, enumerationLimit);
            return data;
        }

        internal override FormatStartData GenerateStartData(PSObject so)
        {
            FormatStartData data = base.GenerateStartData(so);
            WideViewHeaderInfo info = new WideViewHeaderInfo();
            data.shapeInfo = info;
            if (!base.AutoSize)
            {
                info.columns = this.Columns;
                return data;
            }
            info.columns = 0;
            return data;
        }

        private WideViewEntry GenerateWideViewEntryFromDataBaseInfo(PSObject so, int enumerationLimit)
        {
            WideControlBody mainControl = (WideControlBody) base.dataBaseInfo.view.mainControl;
            WideControlEntryDefinition activeWideControlEntryDefinition = this.GetActiveWideControlEntryDefinition(mainControl, so);
            return new WideViewEntry { formatPropertyField = base.GenerateFormatPropertyField(activeWideControlEntryDefinition.formatTokenList, so, enumerationLimit) };
        }

        private WideViewEntry GenerateWideViewEntryFromProperties(PSObject so, int enumerationLimit)
        {
            if (base.activeAssociationList == null)
            {
                this.SetUpActiveProperty(so);
            }
            WideViewEntry entry = new WideViewEntry();
            FormatPropertyField field = new FormatPropertyField();
            entry.formatPropertyField = field;
            if (base.activeAssociationList.Count > 0)
            {
                MshResolvedExpressionParameterAssociation association = base.activeAssociationList[0];
                FieldFormattingDirective directive = null;
                if (association.OriginatingParameter != null)
                {
                    directive = association.OriginatingParameter.GetEntry("formatString") as FieldFormattingDirective;
                }
                field.propertyValue = base.GetExpressionDisplayValue(so, enumerationLimit, association.ResolvedExpression, directive);
            }
            base.activeAssociationList = null;
            return entry;
        }

        private WideControlEntryDefinition GetActiveWideControlEntryDefinition(WideControlBody wideBody, PSObject so)
        {
            ConsolidatedString internalTypeNames = so.InternalTypeNames;
            TypeMatch match = new TypeMatch(base.expressionFactory, base.dataBaseInfo.db, internalTypeNames);
            foreach (WideControlEntryDefinition definition in wideBody.optionalEntryList)
            {
                if (match.PerfectMatch(new TypeMatchItem(definition, definition.appliesTo)))
                {
                    return definition;
                }
            }
            if (match.BestMatch != null)
            {
                return (match.BestMatch as WideControlEntryDefinition);
            }
            Collection<string> typeNames = Deserializer.MaskDeserializationPrefix(internalTypeNames);
            if (typeNames != null)
            {
                match = new TypeMatch(base.expressionFactory, base.dataBaseInfo.db, typeNames);
                foreach (WideControlEntryDefinition definition2 in wideBody.optionalEntryList)
                {
                    if (match.PerfectMatch(new TypeMatchItem(definition2, definition2.appliesTo)))
                    {
                        return definition2;
                    }
                }
                if (match.BestMatch != null)
                {
                    return (match.BestMatch as WideControlEntryDefinition);
                }
            }
            return wideBody.defaultEntryDefinition;
        }

        internal override void Initialize(TerminatingErrorContext errorContext, MshExpressionFactory expressionFactory, PSObject so, TypeInfoDataBase db, FormattingCommandLineParameters parameters)
        {
            base.Initialize(errorContext, expressionFactory, so, db, parameters);
            base.inputParameters = parameters;
        }

        private void SetUpActiveProperty(PSObject so)
        {
            List<MshParameter> parameters = null;
            if (base.inputParameters != null)
            {
                parameters = base.inputParameters.mshParameterList;
            }
            if ((parameters != null) && (parameters.Count > 0))
            {
                base.activeAssociationList = AssociationManager.ExpandParameters(parameters, so);
            }
            else
            {
                MshExpression displayNameExpression = PSObjectHelper.GetDisplayNameExpression(so, base.expressionFactory);
                if (displayNameExpression != null)
                {
                    base.activeAssociationList = new List<MshResolvedExpressionParameterAssociation>();
                    base.activeAssociationList.Add(new MshResolvedExpressionParameterAssociation(null, displayNameExpression));
                }
                else
                {
                    base.activeAssociationList = AssociationManager.ExpandDefaultPropertySet(so, base.expressionFactory);
                    if (base.activeAssociationList.Count <= 0)
                    {
                        base.activeAssociationList = AssociationManager.ExpandAll(so);
                    }
                }
            }
        }

        private int Columns
        {
            get
            {
                if ((base.parameters != null) && (base.parameters.shapeParameters != null))
                {
                    WideSpecificParameters shapeParameters = (WideSpecificParameters) base.parameters.shapeParameters;
                    if (shapeParameters.columns.HasValue)
                    {
                        return shapeParameters.columns.Value;
                    }
                }
                if ((base.dataBaseInfo.view != null) && (base.dataBaseInfo.view.mainControl != null))
                {
                    WideControlBody mainControl = (WideControlBody) base.dataBaseInfo.view.mainControl;
                    return mainControl.columns;
                }
                return 0;
            }
        }
    }
}

