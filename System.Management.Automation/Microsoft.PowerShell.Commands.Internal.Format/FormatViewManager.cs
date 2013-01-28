namespace Microsoft.PowerShell.Commands.Internal.Format
{
    using System;
    using System.Collections.Generic;
    using System.Management.Automation;
    using System.Management.Automation.Internal;
    using System.Management.Automation.Runspaces;
    using System.Text;

    internal sealed class FormatViewManager
    {
        [TraceSource("FormatViewBinding", "Format view binding")]
        private static PSTraceSource formatViewBindingTracer = PSTraceSource.GetTracer("FormatViewBinding", "Format view binding", false);
        private Microsoft.PowerShell.Commands.Internal.Format.ViewGenerator viewGenerator;

        internal void Initialize(TerminatingErrorContext errorContext, MshExpressionFactory expressionFactory, TypeInfoDataBase db, PSObject so, FormatShape shape, FormattingCommandLineParameters parameters)
        {
            ViewDefinition view = null;
            try
            {
                DisplayDataQuery.SetTracer(formatViewBindingTracer);
                ConsolidatedString internalTypeNames = so.InternalTypeNames;
                if (shape == FormatShape.Undefined)
                {
                    using (formatViewBindingTracer.TraceScope("FINDING VIEW  TYPE: {0}", new object[] { PSObjectTypeName(so) }))
                    {
                        view = DisplayDataQuery.GetViewByShapeAndType(expressionFactory, db, shape, internalTypeNames, null);
                    }
                    if (view != null)
                    {
                        this.viewGenerator = SelectViewGeneratorFromViewDefinition(errorContext, expressionFactory, db, view, parameters);
                        formatViewBindingTracer.WriteLine("An applicable view has been found", new object[0]);
                        PrepareViewForRemoteObjects(this.ViewGenerator, so);
                    }
                    else
                    {
                        formatViewBindingTracer.WriteLine("No applicable view has been found", new object[0]);
                        this.viewGenerator = SelectViewGeneratorFromProperties(shape, so, errorContext, expressionFactory, db, null);
                        PrepareViewForRemoteObjects(this.ViewGenerator, so);
                    }
                }
                else if ((parameters != null) && (parameters.mshParameterList.Count > 0))
                {
                    this.viewGenerator = SelectViewGeneratorFromProperties(shape, so, errorContext, expressionFactory, db, parameters);
                }
                else
                {
                    if ((parameters != null) && !string.IsNullOrEmpty(parameters.viewName))
                    {
                        using (formatViewBindingTracer.TraceScope("FINDING VIEW NAME: {0}  TYPE: {1}", new object[] { parameters.viewName, PSObjectTypeName(so) }))
                        {
                            view = DisplayDataQuery.GetViewByShapeAndType(expressionFactory, db, shape, internalTypeNames, parameters.viewName);
                        }
                        if (view != null)
                        {
                            this.viewGenerator = SelectViewGeneratorFromViewDefinition(errorContext, expressionFactory, db, view, parameters);
                            formatViewBindingTracer.WriteLine("An applicable view has been found", new object[0]);
                            return;
                        }
                        formatViewBindingTracer.WriteLine("No applicable view has been found", new object[0]);
                        ProcessUnknownViewName(errorContext, parameters.viewName, so, db, shape);
                    }
                    using (formatViewBindingTracer.TraceScope("FINDING VIEW {0} TYPE: {1}", new object[] { shape, PSObjectTypeName(so) }))
                    {
                        view = DisplayDataQuery.GetViewByShapeAndType(expressionFactory, db, shape, internalTypeNames, null);
                    }
                    if (view != null)
                    {
                        this.viewGenerator = SelectViewGeneratorFromViewDefinition(errorContext, expressionFactory, db, view, parameters);
                        formatViewBindingTracer.WriteLine("An applicable view has been found", new object[0]);
                        PrepareViewForRemoteObjects(this.ViewGenerator, so);
                    }
                    else
                    {
                        formatViewBindingTracer.WriteLine("No applicable view has been found", new object[0]);
                        this.viewGenerator = SelectViewGeneratorFromProperties(shape, so, errorContext, expressionFactory, db, parameters);
                        PrepareViewForRemoteObjects(this.ViewGenerator, so);
                    }
                }
            }
            finally
            {
                DisplayDataQuery.ResetTracer();
            }
        }

        private static void PrepareViewForRemoteObjects(Microsoft.PowerShell.Commands.Internal.Format.ViewGenerator viewGenerator, PSObject so)
        {
            if (PSObjectHelper.ShouldShowComputerNameProperty(so))
            {
                viewGenerator.PrepareForRemoteObjects(so);
            }
        }

        private static void ProcessUnknownViewName(TerminatingErrorContext errorContext, string viewName, PSObject so, TypeInfoDataBase db, FormatShape formatShape)
        {
            string message = null;
            bool flag = false;
            string str2 = null;
            string str3 = ", ";
            StringBuilder builder = new StringBuilder();
            if ((((so != null) && (so.BaseObject != null)) && ((db != null) && (db.viewDefinitionsSection != null))) && ((db.viewDefinitionsSection.viewDefinitionList != null) && (db.viewDefinitionsSection.viewDefinitionList.Count > 0)))
            {
                StringBuilder builder2 = new StringBuilder();
                string a = so.BaseObject.GetType().ToString();
                Type type = null;
                if (formatShape == FormatShape.Table)
                {
                    type = typeof(TableControlBody);
                    str2 = "Table";
                }
                else if (formatShape == FormatShape.List)
                {
                    type = typeof(ListControlBody);
                    str2 = "List";
                }
                else if (formatShape == FormatShape.Wide)
                {
                    type = typeof(WideControlBody);
                    str2 = "Wide";
                }
                else if (formatShape == FormatShape.Complex)
                {
                    type = typeof(ComplexControlBody);
                    str2 = "Custom";
                }
                if (type != null)
                {
                    foreach (ViewDefinition definition in db.viewDefinitionsSection.viewDefinitionList)
                    {
                        if (definition.mainControl != null)
                        {
                            foreach (TypeOrGroupReference reference in definition.appliesTo.referenceList)
                            {
                                if (!string.IsNullOrEmpty(reference.name) && string.Equals(a, reference.name, StringComparison.OrdinalIgnoreCase))
                                {
                                    if (definition.mainControl.GetType() == type)
                                    {
                                        builder2.Append(definition.name);
                                        builder2.Append(str3);
                                    }
                                    else if (string.Equals(viewName, definition.name, StringComparison.OrdinalIgnoreCase))
                                    {
                                        string str5 = null;
                                        if (definition.mainControl.GetType() == typeof(TableControlBody))
                                        {
                                            str5 = "Format-Table";
                                        }
                                        else if (definition.mainControl.GetType() == typeof(ListControlBody))
                                        {
                                            str5 = "Format-List";
                                        }
                                        else if (definition.mainControl.GetType() == typeof(WideControlBody))
                                        {
                                            str5 = "Format-Wide";
                                        }
                                        else if (definition.mainControl.GetType() == typeof(ComplexControlBody))
                                        {
                                            str5 = "Format-Custom";
                                        }
                                        if (builder.Length == 0)
                                        {
                                            string str6 = StringUtil.Format(FormatAndOut_format_xxx.SuggestValidViewNamePrefix, new object[0]);
                                            builder.Append(str6);
                                        }
                                        else
                                        {
                                            builder.Append(", ");
                                        }
                                        builder.Append(str5);
                                    }
                                }
                            }
                        }
                    }
                }
                if (builder2.Length > 0)
                {
                    builder2.Remove(builder2.Length - str3.Length, str3.Length);
                    message = StringUtil.Format(FormatAndOut_format_xxx.InvalidViewNameError, new object[] { viewName, str2, builder2.ToString() });
                    flag = true;
                }
            }
            if (!flag)
            {
                StringBuilder builder3 = new StringBuilder();
                if (builder.Length > 0)
                {
                    builder3.Append(StringUtil.Format(FormatAndOut_format_xxx.UnknownViewNameErrorSuffix, viewName, str2));
                    builder3.Append(builder.ToString());
                }
                else
                {
                    builder3.Append(StringUtil.Format(FormatAndOut_format_xxx.UnknownViewNameError, viewName));
                    builder3.Append(StringUtil.Format(FormatAndOut_format_xxx.NonExistingViewNameError, str2, so.BaseObject.GetType()));
                }
                message = builder3.ToString();
            }
            ErrorRecord errorRecord = new ErrorRecord(new PipelineStoppedException(), "FormatViewNotFound", ErrorCategory.ObjectNotFound, viewName) {
                ErrorDetails = new ErrorDetails(message)
            };
            errorContext.ThrowTerminatingError(errorRecord);
        }

        private static string PSObjectTypeName(PSObject so)
        {
            if (so != null)
            {
                ConsolidatedString internalTypeNames = so.InternalTypeNames;
                if (internalTypeNames.Count > 0)
                {
                    return internalTypeNames[0];
                }
            }
            return "";
        }

        private static Microsoft.PowerShell.Commands.Internal.Format.ViewGenerator SelectViewGeneratorFromProperties(FormatShape shape, PSObject so, TerminatingErrorContext errorContext, MshExpressionFactory expressionFactory, TypeInfoDataBase db, FormattingCommandLineParameters parameters)
        {
            if ((shape == FormatShape.Undefined) && (parameters == null))
            {
                ConsolidatedString internalTypeNames = so.InternalTypeNames;
                shape = DisplayDataQuery.GetShapeFromType(expressionFactory, db, internalTypeNames);
                if (shape == FormatShape.Undefined)
                {
                    List<MshExpression> defaultPropertySet = PSObjectHelper.GetDefaultPropertySet(so);
                    if (defaultPropertySet.Count == 0)
                    {
                        foreach (MshResolvedExpressionParameterAssociation association in AssociationManager.ExpandAll(so))
                        {
                            defaultPropertySet.Add(association.ResolvedExpression);
                        }
                    }
                    shape = DisplayDataQuery.GetShapeFromPropertyCount(db, defaultPropertySet.Count);
                }
            }
            Microsoft.PowerShell.Commands.Internal.Format.ViewGenerator generator = null;
            if (shape == FormatShape.Table)
            {
                generator = new TableViewGenerator();
            }
            else if (shape == FormatShape.List)
            {
                generator = new ListViewGenerator();
            }
            else if (shape == FormatShape.Wide)
            {
                generator = new WideViewGenerator();
            }
            else if (shape == FormatShape.Complex)
            {
                generator = new ComplexViewGenerator();
            }
            generator.Initialize(errorContext, expressionFactory, so, db, parameters);
            return generator;
        }

        private static Microsoft.PowerShell.Commands.Internal.Format.ViewGenerator SelectViewGeneratorFromViewDefinition(TerminatingErrorContext errorContext, MshExpressionFactory expressionFactory, TypeInfoDataBase db, ViewDefinition view, FormattingCommandLineParameters parameters)
        {
            Microsoft.PowerShell.Commands.Internal.Format.ViewGenerator generator = null;
            if (view.mainControl is TableControlBody)
            {
                generator = new TableViewGenerator();
            }
            else if (view.mainControl is ListControlBody)
            {
                generator = new ListViewGenerator();
            }
            else if (view.mainControl is WideControlBody)
            {
                generator = new WideViewGenerator();
            }
            else if (view.mainControl is ComplexControlBody)
            {
                generator = new ComplexViewGenerator();
            }
            generator.Initialize(errorContext, expressionFactory, db, view, parameters);
            return generator;
        }

        internal Microsoft.PowerShell.Commands.Internal.Format.ViewGenerator ViewGenerator
        {
            get
            {
                return this.viewGenerator;
            }
        }
    }
}

