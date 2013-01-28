namespace Microsoft.PowerShell.Commands.Internal.Format
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Management.Automation;
    using System.Management.Automation.Runspaces;

    internal sealed class ComplexControlGenerator
    {
        private List<ControlDefinition> controlDefinitionList;
        private TypeInfoDataBase db;
        private int enumerationLimit;
        private TerminatingErrorContext errorContext;
        private FormatErrorManager errorManager;
        private MshExpressionFactory expressionFactory;
        private DatabaseLoadingInfo loadingInfo;

        internal ComplexControlGenerator(TypeInfoDataBase dataBase, DatabaseLoadingInfo loadingInfo, MshExpressionFactory expressionFactory, List<ControlDefinition> controlDefinitionList, FormatErrorManager resultErrorManager, int enumerationLimit, TerminatingErrorContext errorContext)
        {
            this.db = dataBase;
            this.loadingInfo = loadingInfo;
            this.expressionFactory = expressionFactory;
            this.controlDefinitionList = controlDefinitionList;
            this.errorManager = resultErrorManager;
            this.enumerationLimit = enumerationLimit;
            this.errorContext = errorContext;
        }

        private bool EvaluateDisplayCondition(PSObject so, ExpressionToken conditionToken)
        {
            MshExpressionResult result;
            if (conditionToken == null)
            {
                return true;
            }
            MshExpression ex = this.expressionFactory.CreateFromExpressionToken(conditionToken, this.loadingInfo);
            bool flag = DisplayCondition.Evaluate(so, ex, out result);
            if ((result != null) && (result.Exception != null))
            {
                this.errorManager.LogMshExpressionFailedResult(result, so);
            }
            return flag;
        }

        private bool ExecuteFormatControl(TraversalInfo level, ControlBase control, PSObject so, List<FormatValue> formatValueList)
        {
            ComplexControlBody complexBody = null;
            ControlReference controlReference = control as ControlReference;
            if ((controlReference != null) && (controlReference.controlType == typeof(ComplexControlBody)))
            {
                complexBody = DisplayDataQuery.ResolveControlReference(this.db, this.controlDefinitionList, controlReference) as ComplexControlBody;
            }
            else
            {
                complexBody = control as ComplexControlBody;
            }
            if (complexBody != null)
            {
                this.ExecuteFormatControlBody(level, so, complexBody, formatValueList);
                return true;
            }
            return false;
        }

        private void ExecuteFormatControlBody(TraversalInfo level, PSObject so, ComplexControlBody complexBody, List<FormatValue> formatValueList)
        {
            ComplexControlEntryDefinition activeComplexControlEntryDefinition = this.GetActiveComplexControlEntryDefinition(complexBody, so);
            this.ExecuteFormatTokenList(level, so, activeComplexControlEntryDefinition.itemDefinition.formatTokenList, formatValueList);
        }

        private void ExecuteFormatTokenList(TraversalInfo level, PSObject so, List<FormatToken> formatTokenList, List<FormatValue> formatValueList)
        {
            if (so == null)
            {
                throw PSTraceSource.NewArgumentNullException("so");
            }
            if (level.Level != level.MaxDepth)
            {
                FormatEntry item = new FormatEntry();
                formatValueList.Add(item);
                foreach (FormatToken token in formatTokenList)
                {
                    TextToken tt = token as TextToken;
                    if (tt != null)
                    {
                        FormatTextField field = new FormatTextField {
                            text = this.db.displayResourceManagerCache.GetTextTokenString(tt)
                        };
                        item.formatValueList.Add(field);
                    }
                    else if (token is NewLineToken)
                    {
                        item.formatValueList.Add(new FormatNewLine());
                    }
                    else
                    {
                        FrameToken token3 = token as FrameToken;
                        if (token3 != null)
                        {
                            FormatEntry entry2 = new FormatEntry {
                                frameInfo = new FrameInfo()
                            };
                            entry2.frameInfo.firstLine = token3.frameInfoDefinition.firstLine;
                            entry2.frameInfo.leftIndentation = token3.frameInfoDefinition.leftIndentation;
                            entry2.frameInfo.rightIndentation = token3.frameInfoDefinition.rightIndentation;
                            this.ExecuteFormatTokenList(level, so, token3.itemDefinition.formatTokenList, entry2.formatValueList);
                            item.formatValueList.Add(entry2);
                        }
                        else
                        {
                            CompoundPropertyToken token4 = token as CompoundPropertyToken;
                            if ((token4 != null) && this.EvaluateDisplayCondition(so, token4.conditionToken))
                            {
                                object result = null;
                                if ((token4.expression == null) || string.IsNullOrEmpty(token4.expression.expressionValue))
                                {
                                    result = so;
                                }
                                else
                                {
                                    List<MshExpressionResult> values = this.expressionFactory.CreateFromExpressionToken(token4.expression, this.loadingInfo).GetValues(so);
                                    if (values.Count > 0)
                                    {
                                        result = values[0].Result;
                                        if (values[0].Exception != null)
                                        {
                                            this.errorManager.LogMshExpressionFailedResult(values[0], so);
                                        }
                                    }
                                }
                                if ((token4.control == null) || (token4.control is FieldControlBody))
                                {
                                    if (result == null)
                                    {
                                        result = "";
                                    }
                                    FieldFormattingDirective fieldFormattingDirective = null;
                                    StringFormatError formatErrorObject = null;
                                    if (token4.control != null)
                                    {
                                        fieldFormattingDirective = ((FieldControlBody) token4.control).fieldFormattingDirective;
                                        if ((fieldFormattingDirective != null) && this.errorManager.DisplayFormatErrorString)
                                        {
                                            formatErrorObject = new StringFormatError();
                                        }
                                    }
                                    IEnumerable enumerable = PSObjectHelper.GetEnumerable(result);
                                    FormatPropertyField field2 = new FormatPropertyField();
                                    if (token4.enumerateCollection && (enumerable != null))
                                    {
                                        foreach (object obj3 in enumerable)
                                        {
                                            if (obj3 != null)
                                            {
                                                field2 = new FormatPropertyField {
                                                    propertyValue = PSObjectHelper.FormatField(fieldFormattingDirective, obj3, this.enumerationLimit, formatErrorObject, this.expressionFactory)
                                                };
                                                item.formatValueList.Add(field2);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        field2 = new FormatPropertyField {
                                            propertyValue = PSObjectHelper.FormatField(fieldFormattingDirective, result, this.enumerationLimit, formatErrorObject, this.expressionFactory)
                                        };
                                        item.formatValueList.Add(field2);
                                    }
                                    if ((formatErrorObject != null) && (formatErrorObject.exception != null))
                                    {
                                        this.errorManager.LogStringFormatError(formatErrorObject);
                                        field2.propertyValue = this.errorManager.FormatErrorString;
                                    }
                                }
                                else if (result != null)
                                {
                                    IEnumerable enumerable2 = PSObjectHelper.GetEnumerable(result);
                                    if (token4.enumerateCollection && (enumerable2 != null))
                                    {
                                        foreach (object obj4 in enumerable2)
                                        {
                                            if (obj4 != null)
                                            {
                                                this.ExecuteFormatControl(level.NextLevel, token4.control, PSObject.AsPSObject(obj4), item.formatValueList);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        this.ExecuteFormatControl(level.NextLevel, token4.control, PSObjectHelper.AsPSObject(result), item.formatValueList);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        internal void GenerateFormatEntries(int maxTreeDepth, ControlBase control, PSObject so, List<FormatValue> formatValueList)
        {
            if (control == null)
            {
                throw PSTraceSource.NewArgumentNullException("control");
            }
            this.ExecuteFormatControl(new TraversalInfo(0, maxTreeDepth), control, so, formatValueList);
        }

        private ComplexControlEntryDefinition GetActiveComplexControlEntryDefinition(ComplexControlBody complexBody, PSObject so)
        {
            ConsolidatedString internalTypeNames = so.InternalTypeNames;
            TypeMatch match = new TypeMatch(this.expressionFactory, this.db, internalTypeNames);
            foreach (ComplexControlEntryDefinition definition in complexBody.optionalEntryList)
            {
                if (match.PerfectMatch(new TypeMatchItem(definition, definition.appliesTo)))
                {
                    return definition;
                }
            }
            if (match.BestMatch != null)
            {
                return (match.BestMatch as ComplexControlEntryDefinition);
            }
            Collection<string> typeNames = Deserializer.MaskDeserializationPrefix(internalTypeNames);
            if (typeNames != null)
            {
                match = new TypeMatch(this.expressionFactory, this.db, typeNames);
                foreach (ComplexControlEntryDefinition definition2 in complexBody.optionalEntryList)
                {
                    if (match.PerfectMatch(new TypeMatchItem(definition2, definition2.appliesTo)))
                    {
                        return definition2;
                    }
                }
                if (match.BestMatch != null)
                {
                    return (match.BestMatch as ComplexControlEntryDefinition);
                }
            }
            return complexBody.defaultEntry;
        }
    }
}

