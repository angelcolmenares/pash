namespace Microsoft.PowerShell.Commands.Internal.Format
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Management.Automation;
    using System.Management.Automation.Internal;
    using System.Management.Automation.Runspaces;

    internal sealed class ComplexViewObjectBrowser
    {
        private ComplexSpecificParameters complexSpecificParameters;
        private int enumerationLimit;
        private FormatErrorManager errorManager;
        private MshExpressionFactory expressionFactory;
        private int indentationStep = 2;

        internal ComplexViewObjectBrowser(FormatErrorManager resultErrorManager, MshExpressionFactory mshExpressionFactory, int enumerationLimit)
        {
            this.errorManager = resultErrorManager;
            this.expressionFactory = mshExpressionFactory;
            this.enumerationLimit = enumerationLimit;
        }

        private static void AddEpilogue(List<FormatValue> formatValueList, string closeTag)
        {
            FormatTextField item = new FormatTextField {
                text = closeTag
            };
            formatValueList.Add(item);
            formatValueList.Add(new FormatNewLine());
        }

        private List<FormatValue> AddIndentationLevel(List<FormatValue> formatValueList)
        {
            FormatEntry item = new FormatEntry {
                frameInfo = new FrameInfo()
            };
            item.frameInfo.firstLine = 0;
            item.frameInfo.leftIndentation = this.indentationStep;
            item.frameInfo.rightIndentation = 0;
            formatValueList.Add(item);
            return item.formatValueList;
        }

        private static void AddPrologue(List<FormatValue> formatValueList, string openTag, string label)
        {
            if (label != null)
            {
                FormatTextField field = new FormatTextField {
                    text = label
                };
                formatValueList.Add(field);
                formatValueList.Add(new FormatNewLine());
            }
            FormatTextField item = new FormatTextField {
                text = openTag
            };
            formatValueList.Add(item);
            formatValueList.Add(new FormatNewLine());
        }

        private void DisplayEnumeration(IEnumerable e, TraversalInfo level, List<FormatValue> formatValueList)
        {
            AddPrologue(formatValueList, "[", null);
            this.DisplayEnumerationInner(e, level, this.AddIndentationLevel(formatValueList));
            AddEpilogue(formatValueList, "]");
            formatValueList.Add(new FormatNewLine());
        }

        private void DisplayEnumerationInner(IEnumerable e, TraversalInfo level, List<FormatValue> formatValueList)
        {
            int num = 0;
            foreach (object obj2 in e)
            {
                if (LocalPipeline.GetExecutionContextFromTLS().CurrentPipelineStopping)
                {
                    throw new PipelineStoppedException();
                }
                if (this.enumerationLimit >= 0)
                {
                    if (this.enumerationLimit == num)
                    {
                        this.DisplayLeaf("...", formatValueList);
                        break;
                    }
                    num++;
                }
                if (TreatAsLeafNode(obj2, level))
                {
                    this.DisplayLeaf(obj2, formatValueList);
                }
                else
                {
                    IEnumerable enumerable = PSObjectHelper.GetEnumerable(obj2);
                    if (enumerable != null)
                    {
                        formatValueList.Add(new FormatNewLine());
                        this.DisplayEnumeration(enumerable, level.NextLevel, this.AddIndentationLevel(formatValueList));
                    }
                    else
                    {
                        this.DisplayObject(PSObjectHelper.AsPSObject(obj2), level.NextLevel, null, formatValueList);
                    }
                }
            }
        }

        private void DisplayLeaf(object val, List<FormatValue> formatValueList)
        {
            FormatPropertyField item = new FormatPropertyField {
                propertyValue = PSObjectHelper.FormatField(null, PSObjectHelper.AsPSObject(val), this.enumerationLimit, null, this.expressionFactory)
            };
            formatValueList.Add(item);
            formatValueList.Add(new FormatNewLine());
        }

        private void DisplayObject(PSObject so, TraversalInfo currentLevel, List<MshParameter> parameterList, List<FormatValue> formatValueList)
        {
            List<MshResolvedExpressionParameterAssociation> activeAssociationList = AssociationManager.SetupActiveProperties(parameterList, so, this.expressionFactory);
            FormatEntry item = new FormatEntry();
            formatValueList.Add(item);
            string objectDisplayName = this.GetObjectDisplayName(so);
            if (objectDisplayName != null)
            {
                objectDisplayName = "class " + objectDisplayName;
            }
            AddPrologue(item.formatValueList, "{", objectDisplayName);
            this.ProcessActiveAssociationList(so, currentLevel, activeAssociationList, this.AddIndentationLevel(item.formatValueList));
            AddEpilogue(item.formatValueList, "}");
        }

        private void DisplayRawObject(PSObject so, List<FormatValue> formatValueList)
        {
            FormatPropertyField item = new FormatPropertyField();
            StringFormatError formatErrorObject = null;
            if (this.errorManager.DisplayFormatErrorString)
            {
                formatErrorObject = new StringFormatError();
            }
            item.propertyValue = PSObjectHelper.SmartToString(so, this.expressionFactory, this.enumerationLimit, formatErrorObject);
            if ((formatErrorObject != null) && (formatErrorObject.exception != null))
            {
                this.errorManager.LogStringFormatError(formatErrorObject);
                if (this.errorManager.DisplayFormatErrorString)
                {
                    item.propertyValue = this.errorManager.FormatErrorString;
                }
            }
            formatValueList.Add(item);
            formatValueList.Add(new FormatNewLine());
        }

        internal ComplexViewEntry GenerateView(PSObject so, FormattingCommandLineParameters inputParameters)
        {
            this.complexSpecificParameters = (ComplexSpecificParameters) inputParameters.shapeParameters;
            int maxDepth = this.complexSpecificParameters.maxDepth;
            TraversalInfo level = new TraversalInfo(0, maxDepth);
            List<MshParameter> parameterList = null;
            if (inputParameters != null)
            {
                parameterList = inputParameters.mshParameterList;
            }
            ComplexViewEntry entry = new ComplexViewEntry();
            if (TreatAsScalarType(so.InternalTypeNames))
            {
                FormatEntry item = new FormatEntry();
                entry.formatValueList.Add(item);
                this.DisplayRawObject(so, item.formatValueList);
                return entry;
            }
            IEnumerable e = PSObjectHelper.GetEnumerable(so);
            if (e != null)
            {
                FormatEntry entry3 = new FormatEntry();
                entry.formatValueList.Add(entry3);
                this.DisplayEnumeration(e, level, entry3.formatValueList);
                return entry;
            }
            this.DisplayObject(so, level, parameterList, entry.formatValueList);
            return entry;
        }

        private string GetObjectDisplayName(PSObject so)
        {
            if (this.complexSpecificParameters.classDisplay == ComplexSpecificParameters.ClassInfoDisplay.none)
            {
                return null;
            }
            ConsolidatedString internalTypeNames = so.InternalTypeNames;
            if (internalTypeNames.Count == 0)
            {
                return "PSObject";
            }
            if (this.complexSpecificParameters.classDisplay == ComplexSpecificParameters.ClassInfoDisplay.shortName)
            {
                string[] strArray = internalTypeNames[0].Split(new char[] { '.' });
                if (strArray.Length > 0)
                {
                    return strArray[strArray.Length - 1];
                }
            }
            return internalTypeNames[0];
        }

        private void ProcessActiveAssociationList(PSObject so, TraversalInfo currentLevel, List<MshResolvedExpressionParameterAssociation> activeAssociationList, List<FormatValue> formatValueList)
        {
            foreach (MshResolvedExpressionParameterAssociation association in activeAssociationList)
            {
                FormatTextField item = new FormatTextField {
                    text = association.ResolvedExpression.ToString() + " = "
                };
                formatValueList.Add(item);
                List<MshExpressionResult> values = association.ResolvedExpression.GetValues(so);
                object errorString = null;
                if (values.Count >= 1)
                {
                    MshExpressionResult result = values[0];
                    if (result.Exception != null)
                    {
                        this.errorManager.LogMshExpressionFailedResult(result, so);
                        if (this.errorManager.DisplayErrorStrings)
                        {
                            errorString = this.errorManager.ErrorString;
                        }
                        else
                        {
                            errorString = "";
                        }
                    }
                    else
                    {
                        errorString = result.Result;
                    }
                }
                TraversalInfo level = currentLevel;
                if (association.OriginatingParameter != null)
                {
                    object entry = association.OriginatingParameter.GetEntry("depth");
                    if (entry != AutomationNull.Value)
                    {
                        int maxDepth = (int) entry;
                        level = new TraversalInfo(currentLevel.Level, maxDepth);
                    }
                }
                IEnumerable e = null;
                if ((errorString != null) || (level.Level >= level.MaxDepth))
                {
                    e = PSObjectHelper.GetEnumerable(errorString);
                }
                if (e != null)
                {
                    formatValueList.Add(new FormatNewLine());
                    this.DisplayEnumeration(e, level.NextLevel, this.AddIndentationLevel(formatValueList));
                }
                else if ((errorString == null) || TreatAsLeafNode(errorString, level))
                {
                    this.DisplayLeaf(errorString, formatValueList);
                }
                else
                {
                    formatValueList.Add(new FormatNewLine());
                    this.DisplayObject(PSObject.AsPSObject(errorString), level.NextLevel, null, this.AddIndentationLevel(formatValueList));
                }
            }
        }

        private static bool TreatAsLeafNode(object val, TraversalInfo level)
        {
            if ((level.Level < level.MaxDepth) && (val != null))
            {
                return TreatAsScalarType(PSObject.GetTypeNames(val));
            }
            return true;
        }

        private static bool TreatAsScalarType(Collection<string> typeNames)
        {
            return DefaultScalarTypes.IsTypeInList(typeNames);
        }
    }
}

