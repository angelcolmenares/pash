namespace Microsoft.PowerShell.Commands.Internal.Format
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Management.Automation;
    using System.Management.Automation.Internal;
    using System.Management.Automation.Runspaces;
    using System.Runtime.InteropServices;

    internal sealed class TableViewGenerator : ViewGenerator
    {
        private TableControlBody tableBody;

        private static int ComputeDefaultAlignment(PSObject so, MshExpression ex)
        {
            List<MshExpressionResult> values = ex.GetValues(so);
            if ((values.Count != 0) && (values[0].Exception == null))
            {
                object result = values[0].Result;
                if (result != null)
                {
                    ConsolidatedString internalTypeNames = PSObject.AsPSObject(result).InternalTypeNames;
                    if (string.Equals(PSObjectHelper.PSObjectIsOfExactType(internalTypeNames), "System.String", StringComparison.OrdinalIgnoreCase))
                    {
                        return 1;
                    }
                    if (DefaultScalarTypes.IsTypeInList(internalTypeNames))
                    {
                        return 3;
                    }
                }
            }
            return 1;
        }

        private void FilterActiveAssociationList()
        {
            int num = 10;
            if (base.activeAssociationList.Count > num)
            {
                List<MshResolvedExpressionParameterAssociation> activeAssociationList = base.activeAssociationList;
                base.activeAssociationList = new List<MshResolvedExpressionParameterAssociation>();
                for (int i = 0; i < num; i++)
                {
                    base.activeAssociationList.Add(activeAssociationList[i]);
                }
            }
        }

        internal override FormatEntryData GeneratePayload(PSObject so, int enumerationLimit)
        {
            TableRowEntry entry;
            FormatEntryData data = new FormatEntryData();
            if (base.dataBaseInfo.view != null)
            {
                entry = this.GenerateTableRowEntryFromDataBaseInfo(so, enumerationLimit);
            }
            else
            {
                entry = this.GenerateTableRowEntryFromFromProperties(so, enumerationLimit);
                entry.multiLine = base.dataBaseInfo.db.defaultSettingsSection.MultilineTables;
            }
            data.formatEntryInfo = entry;
            if ((base.parameters != null) && (base.parameters.shapeParameters != null))
            {
                TableSpecificParameters shapeParameters = (TableSpecificParameters) base.parameters.shapeParameters;
                if ((shapeParameters != null) && shapeParameters.multiLine.HasValue)
                {
                    entry.multiLine = shapeParameters.multiLine.Value;
                }
            }
            return data;
        }

        internal override FormatStartData GenerateStartData(PSObject so)
        {
            FormatStartData data = base.GenerateStartData(so);
            if (base.dataBaseInfo.view != null)
            {
                data.shapeInfo = this.GenerateTableHeaderInfoFromDataBaseInfo(so);
                return data;
            }
            data.shapeInfo = this.GenerateTableHeaderInfoFromProperties(so);
            return data;
        }

        private TableHeaderInfo GenerateTableHeaderInfoFromDataBaseInfo(PSObject so)
        {
            bool flag;
            TableHeaderInfo info = new TableHeaderInfo();
            List<TableRowItemDefinition> list = this.GetActiveTableRowDefinition(this.tableBody, so, out flag);
            info.hideHeader = this.HideHeaders;
            int num = 0;
            foreach (TableRowItemDefinition definition in list)
            {
                TableColumnInfo item = new TableColumnInfo();
                TableColumnHeaderDefinition definition2 = null;
                if (this.tableBody.header.columnHeaderDefinitionList.Count > 0)
                {
                    definition2 = this.tableBody.header.columnHeaderDefinitionList[num];
                }
                if (definition2 != null)
                {
                    item.width = definition2.width;
                    item.alignment = definition2.alignment;
                    if (definition2.label != null)
                    {
                        item.label = base.dataBaseInfo.db.displayResourceManagerCache.GetTextTokenString(definition2.label);
                    }
                }
                if (item.alignment == 0)
                {
                    item.alignment = definition.alignment;
                }
                if (item.label == null)
                {
                    FormatToken token = null;
                    if (definition.formatTokenList.Count > 0)
                    {
                        token = definition.formatTokenList[0];
                    }
                    if (token != null)
                    {
                        FieldPropertyToken token2 = token as FieldPropertyToken;
                        if (token2 != null)
                        {
                            item.label = token2.expression.expressionValue;
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
                    else
                    {
                        item.label = "";
                    }
                }
                info.tableColumnInfoList.Add(item);
                num++;
            }
            return info;
        }

        private TableHeaderInfo GenerateTableHeaderInfoFromProperties(PSObject so)
        {
            TableHeaderInfo info = new TableHeaderInfo {
                hideHeader = this.HideHeaders
            };
            for (int i = 0; i < base.activeAssociationList.Count; i++)
            {
                MshResolvedExpressionParameterAssociation association = base.activeAssociationList[i];
                TableColumnInfo item = new TableColumnInfo();
                if (association.OriginatingParameter != null)
                {
                    object entry = association.OriginatingParameter.GetEntry("label");
                    if (entry != AutomationNull.Value)
                    {
                        item.propertyName = (string) entry;
                    }
                }
                if (item.propertyName == null)
                {
                    item.propertyName = base.activeAssociationList[i].ResolvedExpression.ToString();
                }
                if (association.OriginatingParameter != null)
                {
                    object obj3 = association.OriginatingParameter.GetEntry("width");
                    if (obj3 != AutomationNull.Value)
                    {
                        item.width = (int) obj3;
                    }
                    else
                    {
                        item.width = 0;
                    }
                }
                else
                {
                    item.width = 0;
                }
                if (association.OriginatingParameter != null)
                {
                    object obj4 = association.OriginatingParameter.GetEntry("alignment");
                    if (obj4 != AutomationNull.Value)
                    {
                        item.alignment = (int) obj4;
                    }
                    else
                    {
                        item.alignment = ComputeDefaultAlignment(so, association.ResolvedExpression);
                    }
                }
                else
                {
                    item.alignment = ComputeDefaultAlignment(so, association.ResolvedExpression);
                }
                info.tableColumnInfoList.Add(item);
            }
            return info;
        }

        private TableRowEntry GenerateTableRowEntryFromDataBaseInfo(PSObject so, int enumerationLimit)
        {
            TableRowEntry entry = new TableRowEntry();
            foreach (TableRowItemDefinition definition in this.GetActiveTableRowDefinition(this.tableBody, so, out entry.multiLine))
            {
                FormatPropertyField item = base.GenerateFormatPropertyField(definition.formatTokenList, so, enumerationLimit);
                item.alignment = definition.alignment;
                entry.formatPropertyFieldList.Add(item);
            }
            return entry;
        }

        private TableRowEntry GenerateTableRowEntryFromFromProperties(PSObject so, int enumerationLimit)
        {
            TableRowEntry entry = new TableRowEntry();
            for (int i = 0; i < base.activeAssociationList.Count; i++)
            {
                FormatPropertyField item = new FormatPropertyField();
                FieldFormattingDirective directive = null;
                if (base.activeAssociationList[i].OriginatingParameter != null)
                {
                    directive = base.activeAssociationList[i].OriginatingParameter.GetEntry("formatString") as FieldFormattingDirective;
                }
                item.propertyValue = base.GetExpressionDisplayValue(so, enumerationLimit, base.activeAssociationList[i].ResolvedExpression, directive);
                entry.formatPropertyFieldList.Add(item);
            }
            return entry;
        }

        private List<TableRowItemDefinition> GetActiveTableRowDefinition(TableControlBody tableBody, PSObject so, out bool multiLine)
        {
            multiLine = tableBody.defaultDefinition.multiLine;
            if (tableBody.optionalDefinitionList.Count == 0)
            {
                return tableBody.defaultDefinition.rowItemDefinitionList;
            }
            TableRowDefinition bestMatch = null;
            ConsolidatedString internalTypeNames = so.InternalTypeNames;
            TypeMatch match = new TypeMatch(base.expressionFactory, base.dataBaseInfo.db, internalTypeNames);
            foreach (TableRowDefinition definition2 in tableBody.optionalDefinitionList)
            {
                if (match.PerfectMatch(new TypeMatchItem(definition2, definition2.appliesTo)))
                {
                    bestMatch = definition2;
                    break;
                }
            }
            if (bestMatch == null)
            {
                bestMatch = match.BestMatch as TableRowDefinition;
            }
            if (bestMatch == null)
            {
                Collection<string> typeNames = Deserializer.MaskDeserializationPrefix(internalTypeNames);
                if (typeNames != null)
                {
                    match = new TypeMatch(base.expressionFactory, base.dataBaseInfo.db, typeNames);
                    foreach (TableRowDefinition definition3 in tableBody.optionalDefinitionList)
                    {
                        if (match.PerfectMatch(new TypeMatchItem(definition3, definition3.appliesTo)))
                        {
                            bestMatch = definition3;
                            break;
                        }
                    }
                    if (bestMatch == null)
                    {
                        bestMatch = match.BestMatch as TableRowDefinition;
                    }
                }
            }
            if (bestMatch == null)
            {
                return tableBody.defaultDefinition.rowItemDefinitionList;
            }
            if (bestMatch.multiLine)
            {
                multiLine = bestMatch.multiLine;
            }
            List<TableRowItemDefinition> list = new List<TableRowItemDefinition>();
            int num = 0;
            foreach (TableRowItemDefinition definition4 in bestMatch.rowItemDefinitionList)
            {
                if (definition4.formatTokenList.Count == 0)
                {
                    list.Add(tableBody.defaultDefinition.rowItemDefinitionList[num]);
                }
                else
                {
                    list.Add(definition4);
                }
                num++;
            }
            return list;
        }

        internal override void Initialize(TerminatingErrorContext terminatingErrorContext, MshExpressionFactory mshExpressionFactory, TypeInfoDataBase db, ViewDefinition view, FormattingCommandLineParameters formatParameters)
        {
            base.Initialize(terminatingErrorContext, mshExpressionFactory, db, view, formatParameters);
            if ((base.dataBaseInfo != null) && (base.dataBaseInfo.view != null))
            {
                this.tableBody = (TableControlBody) base.dataBaseInfo.view.mainControl;
            }
        }

        internal override void Initialize(TerminatingErrorContext errorContext, MshExpressionFactory expressionFactory, PSObject so, TypeInfoDataBase db, FormattingCommandLineParameters parameters)
        {
            base.Initialize(errorContext, expressionFactory, so, db, parameters);
            if ((base.dataBaseInfo != null) && (base.dataBaseInfo.view != null))
            {
                this.tableBody = (TableControlBody) base.dataBaseInfo.view.mainControl;
            }
            List<MshParameter> mshParameterList = null;
            if (parameters != null)
            {
                mshParameterList = parameters.mshParameterList;
            }
            if ((mshParameterList != null) && (mshParameterList.Count > 0))
            {
                base.activeAssociationList = AssociationManager.ExpandTableParameters(mshParameterList, so);
            }
            else
            {
                base.activeAssociationList = AssociationManager.ExpandDefaultPropertySet(so, base.expressionFactory);
                if (base.activeAssociationList.Count > 0)
                {
                    if (PSObjectHelper.ShouldShowComputerNameProperty(so))
                    {
                        base.activeAssociationList.Add(new MshResolvedExpressionParameterAssociation(null, new MshExpression(RemotingConstants.ComputerNameNoteProperty)));
                    }
                }
                else
                {
                    base.activeAssociationList = AssociationManager.ExpandAll(so);
                    if (base.activeAssociationList.Count > 0)
                    {
                        AssociationManager.HandleComputerNameProperties(so, base.activeAssociationList);
                        this.FilterActiveAssociationList();
                    }
                    else
                    {
                        base.activeAssociationList = new List<MshResolvedExpressionParameterAssociation>();
                    }
                }
            }
        }

        internal override void PrepareForRemoteObjects(PSObject so)
        {
            PSPropertyInfo local1 = so.Properties[RemotingConstants.ComputerNameNoteProperty];
            if (((base.dataBaseInfo != null) && (base.dataBaseInfo.view != null)) && (base.dataBaseInfo.view.mainControl != null))
            {
                this.tableBody = (TableControlBody) base.dataBaseInfo.view.mainControl.Copy();
                TableRowItemDefinition item = new TableRowItemDefinition();
                PropertyTokenBase base2 = new FieldPropertyToken {
                    expression = new ExpressionToken(RemotingConstants.ComputerNameNoteProperty, false)
                };
                item.formatTokenList.Add(base2);
                this.tableBody.defaultDefinition.rowItemDefinitionList.Add(item);
                if (this.tableBody.header.columnHeaderDefinitionList.Count > 0)
                {
                    TableColumnHeaderDefinition definition2 = new TableColumnHeaderDefinition {
                        label = new TextToken()
                    };
                    definition2.label.text = RemotingConstants.ComputerNameNoteProperty;
                    this.tableBody.header.columnHeaderDefinitionList.Add(definition2);
                }
            }
        }

        private bool HideHeaders
        {
            get
            {
                if ((base.parameters != null) && (base.parameters.shapeParameters != null))
                {
                    TableSpecificParameters shapeParameters = (TableSpecificParameters) base.parameters.shapeParameters;
                    if ((shapeParameters != null) && shapeParameters.hideHeaders.HasValue)
                    {
                        return shapeParameters.hideHeaders.Value;
                    }
                }
                return ((base.dataBaseInfo.view != null) && this.tableBody.header.hideHeader);
            }
        }
    }
}

