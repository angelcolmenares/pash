namespace Microsoft.PowerShell.Commands
{
    using Microsoft.PowerShell.Commands.Internal.Format;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Management.Automation;
    using System.Management.Automation.Internal;
    using System.Management.Automation.Runspaces;

    internal class TableView
    {
        private FormatErrorManager errorManager;
        private MshExpressionFactory expressionFactory;
        private TypeInfoDataBase typeInfoDatabase;

        private void FilterActiveAssociationList(List<MshResolvedExpressionParameterAssociation> activeAssociationList)
        {
            int num = 30;
            if (activeAssociationList.Count > num)
            {
                List<MshResolvedExpressionParameterAssociation> list = new List<MshResolvedExpressionParameterAssociation>(activeAssociationList);
                activeAssociationList.Clear();
                for (int i = 0; i < num; i++)
                {
                    activeAssociationList.Add(list[i]);
                }
            }
        }

        internal HeaderInfo GenerateHeaderInfo(PSObject input, OutGridViewCommand parentCmdlet)
        {
            HeaderInfo info = new HeaderInfo();
            List<MshResolvedExpressionParameterAssociation> activeAssociationList = AssociationManager.ExpandDefaultPropertySet(input, this.expressionFactory);
            if (activeAssociationList.Count > 0)
            {
                if (PSObjectHelper.ShouldShowComputerNameProperty(input))
                {
                    activeAssociationList.Add(new MshResolvedExpressionParameterAssociation(null, new MshExpression(RemotingConstants.ComputerNameNoteProperty)));
                }
            }
            else
            {
                activeAssociationList = AssociationManager.ExpandAll(input);
                if (activeAssociationList.Count > 0)
                {
                    AssociationManager.HandleComputerNameProperties(input, activeAssociationList);
                    this.FilterActiveAssociationList(activeAssociationList);
                }
                else
                {
                    activeAssociationList = new List<MshResolvedExpressionParameterAssociation>();
                }
            }
            for (int i = 0; i < activeAssociationList.Count; i++)
            {
                string staleObjectPropertyName = null;
                MshResolvedExpressionParameterAssociation association = activeAssociationList[i];
                if (association.OriginatingParameter != null)
                {
                    object entry = association.OriginatingParameter.GetEntry("label");
                    if (entry != AutomationNull.Value)
                    {
                        staleObjectPropertyName = (string) entry;
                    }
                }
                if (staleObjectPropertyName == null)
                {
                    staleObjectPropertyName = association.ResolvedExpression.ToString();
                }
                Microsoft.PowerShell.Commands.ColumnInfo col = new OriginalColumnInfo(staleObjectPropertyName, staleObjectPropertyName, staleObjectPropertyName, parentCmdlet);
                info.AddColumn(col);
            }
            return info;
        }

        internal HeaderInfo GenerateHeaderInfo(PSObject input, TableControlBody tableBody, OutGridViewCommand parentCmdlet)
        {
            HeaderInfo info = new HeaderInfo();
            bool flag = typeof(FileSystemInfo).IsInstanceOfType(input.BaseObject);
            if (tableBody != null)
            {
                List<TableRowItemDefinition> activeTableRowDefinition = this.GetActiveTableRowDefinition(tableBody, input);
                int num = 0;
                foreach (TableRowItemDefinition definition in activeTableRowDefinition)
                {
                    Microsoft.PowerShell.Commands.ColumnInfo col = null;
                    string staleObjectPropertyName = null;
                    TableColumnHeaderDefinition definition2 = null;
                    if (tableBody.header.columnHeaderDefinitionList.Count >= (num - 1))
                    {
                        definition2 = tableBody.header.columnHeaderDefinitionList[num];
                    }
                    if ((definition2 != null) && (definition2.label != null))
                    {
                        staleObjectPropertyName = this.typeInfoDatabase.displayResourceManagerCache.GetTextTokenString(definition2.label);
                    }
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
                            if (staleObjectPropertyName == null)
                            {
                                staleObjectPropertyName = token2.expression.expressionValue;
                            }
                            if (token2.expression.isScriptBlock)
                            {
                                MshExpression expression = this.expressionFactory.CreateFromExpressionToken(token2.expression);
                                if (flag && staleObjectPropertyName.Equals("LastWriteTime", StringComparison.OrdinalIgnoreCase))
                                {
                                    col = new OriginalColumnInfo(staleObjectPropertyName, staleObjectPropertyName, "LastWriteTime", parentCmdlet);
                                }
                                else
                                {
                                    col = new ExpressionColumnInfo(staleObjectPropertyName, staleObjectPropertyName, expression);
                                }
                            }
                            else
                            {
                                col = new OriginalColumnInfo(token2.expression.expressionValue, staleObjectPropertyName, token2.expression.expressionValue, parentCmdlet);
                            }
                        }
                        else
                        {
                            TextToken tt = token as TextToken;
                            if (tt != null)
                            {
                                staleObjectPropertyName = this.typeInfoDatabase.displayResourceManagerCache.GetTextTokenString(tt);
                                col = new OriginalColumnInfo(tt.text, staleObjectPropertyName, tt.text, parentCmdlet);
                            }
                        }
                    }
                    if (col != null)
                    {
                        info.AddColumn(col);
                    }
                    num++;
                }
            }
            return info;
        }

        private List<TableRowItemDefinition> GetActiveTableRowDefinition(TableControlBody tableBody, PSObject so)
        {
            if (tableBody.optionalDefinitionList.Count == 0)
            {
                return tableBody.defaultDefinition.rowItemDefinitionList;
            }
            TableRowDefinition bestMatch = null;
            ConsolidatedString internalTypeNames = so.InternalTypeNames;
            TypeMatch match = new TypeMatch(this.expressionFactory, this.typeInfoDatabase, internalTypeNames);
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
                    match = new TypeMatch(this.expressionFactory, this.typeInfoDatabase, typeNames);
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

        internal void Initialize(MshExpressionFactory expressionFactory, TypeInfoDataBase db)
        {
            this.expressionFactory = expressionFactory;
            this.typeInfoDatabase = db;
            FormatErrorPolicy formatErrorPolicy = new FormatErrorPolicy {
                ShowErrorsAsMessages = this.typeInfoDatabase.defaultSettingsSection.formatErrorPolicy.ShowErrorsAsMessages,
                ShowErrorsInFormattedOutput = this.typeInfoDatabase.defaultSettingsSection.formatErrorPolicy.ShowErrorsInFormattedOutput
            };
            this.errorManager = new FormatErrorManager(formatErrorPolicy);
        }
    }
}

