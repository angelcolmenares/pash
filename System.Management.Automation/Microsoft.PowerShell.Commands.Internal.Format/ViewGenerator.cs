namespace Microsoft.PowerShell.Commands.Internal.Format
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Management.Automation;
    using System.Management.Automation.Internal;
    using System.Runtime.InteropServices;

    internal abstract class ViewGenerator
    {
        protected List<MshResolvedExpressionParameterAssociation> activeAssociationList;
        private bool autosize;
        protected DataBaseInfo dataBaseInfo = new DataBaseInfo();
        protected TerminatingErrorContext errorContext;
        private FormatErrorManager errorManager;
        protected MshExpressionFactory expressionFactory;
        private GroupingInfoManager groupingManager;
        protected FormattingCommandLineParameters inputParameters;
        protected FormattingCommandLineParameters parameters;

        protected ViewGenerator()
        {
        }

        protected bool EvaluateDisplayCondition(PSObject so, ExpressionToken conditionToken)
        {
            MshExpressionResult result;
            if (conditionToken == null)
            {
                return true;
            }
            MshExpression ex = this.expressionFactory.CreateFromExpressionToken(conditionToken, this.dataBaseInfo.view.loadingInfo);
            bool flag = DisplayCondition.Evaluate(so, ex, out result);
            if ((result != null) && (result.Exception != null))
            {
                this.errorManager.LogMshExpressionFailedResult(result, so);
            }
            return flag;
        }

        protected FormatPropertyField GenerateFormatPropertyField(List<FormatToken> formatTokenList, PSObject so, int enumerationLimit)
        {
            MshExpressionResult result;
            return this.GenerateFormatPropertyField(formatTokenList, so, enumerationLimit, out result);
        }

        protected FormatPropertyField GenerateFormatPropertyField(List<FormatToken> formatTokenList, PSObject so, int enumerationLimit, out MshExpressionResult result)
        {
            result = null;
            FormatPropertyField field = new FormatPropertyField();
            if (formatTokenList.Count != 0)
            {
                FormatToken token = formatTokenList[0];
                FieldPropertyToken token2 = token as FieldPropertyToken;
                if (token2 != null)
                {
                    MshExpression ex = this.expressionFactory.CreateFromExpressionToken(token2.expression, this.dataBaseInfo.view.loadingInfo);
                    field.propertyValue = this.GetExpressionDisplayValue(so, enumerationLimit, ex, token2.fieldFormattingDirective, out result);
                    return field;
                }
                TextToken tt = token as TextToken;
                if (tt != null)
                {
                    field.propertyValue = this.dataBaseInfo.db.displayResourceManagerCache.GetTextTokenString(tt);
                }
                return field;
            }
            field.propertyValue = "";
            return field;
        }

        internal GroupEndData GenerateGroupEndData()
        {
            return new GroupEndData();
        }

        internal GroupStartData GenerateGroupStartData(PSObject firstObjectInGroup, int enumerationLimit)
        {
            GroupStartData data = new GroupStartData();
            if (this.groupingManager != null)
            {
                object currentGroupingKeyPropertyValue = this.groupingManager.CurrentGroupingKeyPropertyValue;
                if (currentGroupingKeyPropertyValue == AutomationNull.Value)
                {
                    return data;
                }
                PSObject so = PSObjectHelper.AsPSObject(currentGroupingKeyPropertyValue);
                ControlBase control = null;
                TextToken tt = null;
                if (((this.dataBaseInfo.view != null) && (this.dataBaseInfo.view.groupBy != null)) && (this.dataBaseInfo.view.groupBy.startGroup != null))
                {
                    control = this.dataBaseInfo.view.groupBy.startGroup.control;
                    tt = this.dataBaseInfo.view.groupBy.startGroup.labelTextToken;
                }
                data.groupingEntry = new GroupingEntry();
                if (control == null)
                {
                    string textTokenString;
                    StringFormatError formatErrorObject = null;
                    if (this.errorManager.DisplayFormatErrorString)
                    {
                        formatErrorObject = new StringFormatError();
                    }
                    string formatErrorString = PSObjectHelper.SmartToString(so, this.expressionFactory, enumerationLimit, formatErrorObject);
                    if ((formatErrorObject != null) && (formatErrorObject.exception != null))
                    {
                        this.errorManager.LogStringFormatError(formatErrorObject);
                        if (this.errorManager.DisplayFormatErrorString)
                        {
                            formatErrorString = this.errorManager.FormatErrorString;
                        }
                    }
                    FormatEntry item = new FormatEntry();
                    data.groupingEntry.formatValueList.Add(item);
                    FormatTextField field = new FormatTextField();
                    if (tt != null)
                    {
                        textTokenString = this.dataBaseInfo.db.displayResourceManagerCache.GetTextTokenString(tt);
                    }
                    else
                    {
                        textTokenString = this.groupingManager.GroupingKeyDisplayName;
                    }
                    field.text = StringUtil.Format(FormatAndOut_format_xxx.GroupStartDataIndentedAutoGeneratedLabel, textTokenString);
                    item.formatValueList.Add(field);
                    FormatPropertyField field2 = new FormatPropertyField {
                        propertyValue = formatErrorString
                    };
                    item.formatValueList.Add(field2);
                    return data;
                }
                new ComplexControlGenerator(this.dataBaseInfo.db, this.dataBaseInfo.view.loadingInfo, this.expressionFactory, this.dataBaseInfo.view.formatControlDefinitionHolder.controlDefinitionList, this.ErrorManager, enumerationLimit, this.errorContext).GenerateFormatEntries(50, control, firstObjectInGroup, data.groupingEntry.formatValueList);
            }
            return data;
        }

        internal abstract FormatEntryData GeneratePayload(PSObject so, int enumerationLimit);
        internal virtual FormatStartData GenerateStartData(PSObject so)
        {
            FormatStartData data = new FormatStartData();
            if (this.autosize)
            {
                data.autosizeInfo = new AutosizeInfo();
            }
            return data;
        }

        protected string GetExpressionDisplayValue(PSObject so, int enumerationLimit, MshExpression ex, FieldFormattingDirective directive)
        {
            MshExpressionResult result;
            return this.GetExpressionDisplayValue(so, enumerationLimit, ex, directive, out result);
        }

        protected string GetExpressionDisplayValue(PSObject so, int enumerationLimit, MshExpression ex, FieldFormattingDirective directive, out MshExpressionResult expressionResult)
        {
            StringFormatError formatErrorObject = null;
            if (this.errorManager.DisplayFormatErrorString)
            {
                formatErrorObject = new StringFormatError();
            }
            string errorString = PSObjectHelper.GetExpressionDisplayValue(so, enumerationLimit, ex, directive, formatErrorObject, this.expressionFactory, out expressionResult);
            if (expressionResult != null)
            {
                if (expressionResult.Exception != null)
                {
                    this.errorManager.LogMshExpressionFailedResult(expressionResult, so);
                    if (this.errorManager.DisplayErrorStrings)
                    {
                        errorString = this.errorManager.ErrorString;
                    }
                    return errorString;
                }
                if ((formatErrorObject != null) && (formatErrorObject.exception != null))
                {
                    this.errorManager.LogStringFormatError(formatErrorObject);
                    if (this.errorManager.DisplayErrorStrings)
                    {
                        errorString = this.errorManager.FormatErrorString;
                    }
                }
            }
            return errorString;
        }

        internal virtual void Initialize(TerminatingErrorContext terminatingErrorContext, MshExpressionFactory mshExpressionFactory, TypeInfoDataBase db, ViewDefinition view, FormattingCommandLineParameters formatParameters)
        {
            this.errorContext = terminatingErrorContext;
            this.expressionFactory = mshExpressionFactory;
            this.parameters = formatParameters;
            this.dataBaseInfo.db = db;
            this.dataBaseInfo.view = view;
            this.dataBaseInfo.applicableTypes = DisplayDataQuery.GetAllApplicableTypes(db, view.appliesTo);
            this.InitializeHelper();
        }

        internal virtual void Initialize(TerminatingErrorContext terminatingErrorContext, MshExpressionFactory mshExpressionFactory, PSObject so, TypeInfoDataBase db, FormattingCommandLineParameters formatParameters)
        {
            this.errorContext = terminatingErrorContext;
            this.expressionFactory = mshExpressionFactory;
            this.parameters = formatParameters;
            this.dataBaseInfo.db = db;
            this.InitializeHelper();
        }

        private void InitializeAutoSize()
        {
            if ((this.parameters != null) && this.parameters.autosize.HasValue)
            {
                this.autosize = this.parameters.autosize.Value;
            }
            else if ((this.dataBaseInfo.view != null) && (this.dataBaseInfo.view.mainControl != null))
            {
                ControlBody mainControl = this.dataBaseInfo.view.mainControl as ControlBody;
                if ((mainControl != null) && mainControl.autosize.HasValue)
                {
                    this.autosize = mainControl.autosize.Value;
                }
            }
        }

        private void InitializeFormatErrorManager()
        {
            FormatErrorPolicy formatErrorPolicy = new FormatErrorPolicy();
            if ((this.parameters != null) && this.parameters.showErrorsAsMessages.HasValue)
            {
                formatErrorPolicy.ShowErrorsAsMessages = this.parameters.showErrorsAsMessages.Value;
            }
            else
            {
                formatErrorPolicy.ShowErrorsAsMessages = this.dataBaseInfo.db.defaultSettingsSection.formatErrorPolicy.ShowErrorsAsMessages;
            }
            if ((this.parameters != null) && this.parameters.showErrorsInFormattedOutput.HasValue)
            {
                formatErrorPolicy.ShowErrorsInFormattedOutput = this.parameters.showErrorsInFormattedOutput.Value;
            }
            else
            {
                formatErrorPolicy.ShowErrorsInFormattedOutput = this.dataBaseInfo.db.defaultSettingsSection.formatErrorPolicy.ShowErrorsInFormattedOutput;
            }
            this.errorManager = new FormatErrorManager(formatErrorPolicy);
        }

        private void InitializeGroupBy()
        {
            if ((this.parameters != null) && (this.parameters.groupByParameter != null))
            {
                MshExpression entry = this.parameters.groupByParameter.GetEntry("expression") as MshExpression;
                string displayLabel = null;
                object obj2 = this.parameters.groupByParameter.GetEntry("label");
                if (obj2 != AutomationNull.Value)
                {
                    displayLabel = obj2 as string;
                }
                this.groupingManager = new GroupingInfoManager();
                this.groupingManager.Initialize(entry, displayLabel);
            }
            else if (this.dataBaseInfo.view != null)
            {
                GroupBy groupBy = this.dataBaseInfo.view.groupBy;
                if ((groupBy != null) && ((groupBy.startGroup != null) && (groupBy.startGroup.expression != null)))
                {
                    MshExpression groupingExpression = this.expressionFactory.CreateFromExpressionToken(groupBy.startGroup.expression, this.dataBaseInfo.view.loadingInfo);
                    this.groupingManager = new GroupingInfoManager();
                    this.groupingManager.Initialize(groupingExpression, null);
                }
            }
        }

        private void InitializeHelper()
        {
            this.InitializeFormatErrorManager();
            this.InitializeGroupBy();
            this.InitializeAutoSize();
        }

        internal bool IsObjectApplicable(Collection<string> typeNames)
        {
            if (this.dataBaseInfo.view == null)
            {
                return true;
            }
            if (typeNames.Count == 0)
            {
                return false;
            }
            TypeMatch match = new TypeMatch(this.expressionFactory, this.dataBaseInfo.db, typeNames);
            if (match.PerfectMatch(new TypeMatchItem(this, this.dataBaseInfo.applicableTypes)))
            {
                return true;
            }
            bool flag = match.BestMatch != null;
            if (!flag)
            {
                Collection<string> collection = Deserializer.MaskDeserializationPrefix(typeNames);
                if (collection != null)
                {
                    flag = this.IsObjectApplicable(collection);
                }
            }
            return flag;
        }

        internal virtual void PrepareForRemoteObjects(PSObject so)
        {
        }

        internal bool UpdateGroupingKeyValue(PSObject so)
        {
            if (this.groupingManager == null)
            {
                return false;
            }
            return this.groupingManager.UpdateGroupingKeyValue(so);
        }

        protected bool AutoSize
        {
            get
            {
                return this.autosize;
            }
        }

        internal FormatErrorManager ErrorManager
        {
            get
            {
                return this.errorManager;
            }
        }

        protected class DataBaseInfo
        {
            internal AppliesTo applicableTypes;
            internal TypeInfoDataBase db;
            internal ViewDefinition view;
        }
    }
}

