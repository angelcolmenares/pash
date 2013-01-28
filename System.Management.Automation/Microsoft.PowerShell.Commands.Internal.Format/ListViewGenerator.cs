namespace Microsoft.PowerShell.Commands.Internal.Format
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Management.Automation;
    using System.Management.Automation.Internal;
    using System.Management.Automation.Runspaces;

    internal sealed class ListViewGenerator : ViewGenerator
    {
        private ListControlBody listBody;

        private ListViewEntry GenerateListViewEntryFromDataBaseInfo(PSObject so, int enumerationLimit)
        {
            ListViewEntry entry = new ListViewEntry();
            foreach (ListControlItemDefinition definition2 in this.GetActiveListControlEntryDefinition(this.listBody, so).itemDefinitionList)
            {
                if (base.EvaluateDisplayCondition(so, definition2.conditionToken))
                {
                    MshExpressionResult result;
                    ListViewField item = new ListViewField {
                        formatPropertyField = base.GenerateFormatPropertyField(definition2.formatTokenList, so, enumerationLimit, out result)
                    };
                    if (definition2.label != null)
                    {
                        item.label = base.dataBaseInfo.db.displayResourceManagerCache.GetTextTokenString(definition2.label);
                    }
                    else if (result != null)
                    {
                        item.label = result.ResolvedExpression.ToString();
                    }
                    else
                    {
                        FormatToken token = definition2.formatTokenList[0];
                        FieldPropertyToken token2 = token as FieldPropertyToken;
                        if (token2 != null)
                        {
                            item.label = base.expressionFactory.CreateFromExpressionToken(token2.expression, base.dataBaseInfo.view.loadingInfo).ToString();
                        }
                        else
                        {
                            TextToken tt = token as TextToken;
                            if (tt != null)
                            {
                                item.label = base.dataBaseInfo.db.displayResourceManagerCache.GetTextTokenString(tt);
                            }
                        }
                    }
                    entry.listViewFieldList.Add(item);
                }
            }
            return entry;
        }

        private ListViewEntry GenerateListViewEntryFromProperties(PSObject so, int enumerationLimit)
        {
            if (base.activeAssociationList == null)
            {
                this.SetUpActiveProperties(so);
            }
            ListViewEntry entry = new ListViewEntry();
            for (int i = 0; i < base.activeAssociationList.Count; i++)
            {
                MshResolvedExpressionParameterAssociation association = base.activeAssociationList[i];
                ListViewField item = new ListViewField();
                if (association.OriginatingParameter != null)
                {
                    object obj2 = association.OriginatingParameter.GetEntry("label");
                    if (obj2 != AutomationNull.Value)
                    {
                        item.propertyName = (string) obj2;
                    }
                    else
                    {
                        item.propertyName = association.ResolvedExpression.ToString();
                    }
                }
                else
                {
                    item.propertyName = association.ResolvedExpression.ToString();
                }
                FieldFormattingDirective directive = null;
                if (association.OriginatingParameter != null)
                {
                    directive = association.OriginatingParameter.GetEntry("formatString") as FieldFormattingDirective;
                }
                item.formatPropertyField.propertyValue = base.GetExpressionDisplayValue(so, enumerationLimit, association.ResolvedExpression, directive);
                entry.listViewFieldList.Add(item);
            }
            base.activeAssociationList = null;
            return entry;
        }

        internal override FormatEntryData GeneratePayload(PSObject so, int enumerationLimit)
        {
            FormatEntryData data = new FormatEntryData();
            if (base.dataBaseInfo.view != null)
            {
                data.formatEntryInfo = this.GenerateListViewEntryFromDataBaseInfo(so, enumerationLimit);
                return data;
            }
            data.formatEntryInfo = this.GenerateListViewEntryFromProperties(so, enumerationLimit);
            return data;
        }

        internal override FormatStartData GenerateStartData(PSObject so)
        {
            FormatStartData data = base.GenerateStartData(so);
            data.shapeInfo = new ListViewHeaderInfo();
            return data;
        }

        private ListControlEntryDefinition GetActiveListControlEntryDefinition(ListControlBody listBody, PSObject so)
        {
            ConsolidatedString internalTypeNames = so.InternalTypeNames;
            TypeMatch match = new TypeMatch(base.expressionFactory, base.dataBaseInfo.db, internalTypeNames);
            foreach (ListControlEntryDefinition definition in listBody.optionalEntryList)
            {
                if (match.PerfectMatch(new TypeMatchItem(definition, definition.appliesTo, so)))
                {
                    return definition;
                }
            }
            if (match.BestMatch != null)
            {
                return (match.BestMatch as ListControlEntryDefinition);
            }
            Collection<string> typeNames = Deserializer.MaskDeserializationPrefix(internalTypeNames);
            if (typeNames != null)
            {
                match = new TypeMatch(base.expressionFactory, base.dataBaseInfo.db, typeNames);
                foreach (ListControlEntryDefinition definition2 in listBody.optionalEntryList)
                {
                    if (match.PerfectMatch(new TypeMatchItem(definition2, definition2.appliesTo)))
                    {
                        return definition2;
                    }
                }
                if (match.BestMatch != null)
                {
                    return (match.BestMatch as ListControlEntryDefinition);
                }
            }
            return listBody.defaultEntryDefinition;
        }

        internal override void Initialize(TerminatingErrorContext terminatingErrorContext, MshExpressionFactory mshExpressionFactory, TypeInfoDataBase db, ViewDefinition view, FormattingCommandLineParameters formatParameters)
        {
            base.Initialize(terminatingErrorContext, mshExpressionFactory, db, view, formatParameters);
            if ((base.dataBaseInfo != null) && (base.dataBaseInfo.view != null))
            {
                this.listBody = (ListControlBody) base.dataBaseInfo.view.mainControl;
            }
        }

        internal override void Initialize(TerminatingErrorContext errorContext, MshExpressionFactory expressionFactory, PSObject so, TypeInfoDataBase db, FormattingCommandLineParameters parameters)
        {
            base.Initialize(errorContext, expressionFactory, so, db, parameters);
            if ((base.dataBaseInfo != null) && (base.dataBaseInfo.view != null))
            {
                this.listBody = (ListControlBody) base.dataBaseInfo.view.mainControl;
            }
            base.inputParameters = parameters;
            this.SetUpActiveProperties(so);
        }

        internal override void PrepareForRemoteObjects(PSObject so)
        {
            PSPropertyInfo local1 = so.Properties[RemotingConstants.ComputerNameNoteProperty];
            if (((base.dataBaseInfo != null) && (base.dataBaseInfo.view != null)) && (base.dataBaseInfo.view.mainControl != null))
            {
                this.listBody = (ListControlBody) base.dataBaseInfo.view.mainControl.Copy();
                ListControlItemDefinition item = new ListControlItemDefinition {
                    label = new TextToken()
                };
                item.label.text = RemotingConstants.ComputerNameNoteProperty;
                FieldPropertyToken token = new FieldPropertyToken {
                    expression = new ExpressionToken(RemotingConstants.ComputerNameNoteProperty, false)
                };
                item.formatTokenList.Add(token);
                this.listBody.defaultEntryDefinition.itemDefinitionList.Add(item);
            }
        }

        private void SetUpActiveProperties(PSObject so)
        {
            List<MshParameter> rawMshParameterList = null;
            if (base.inputParameters != null)
            {
                rawMshParameterList = base.inputParameters.mshParameterList;
            }
            base.activeAssociationList = AssociationManager.SetupActiveProperties(rawMshParameterList, so, base.expressionFactory);
        }
    }
}

