namespace Microsoft.PowerShell.Commands.Internal.Format
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.Management.Automation;
    using System.Text;

    internal static class DisplayDataQuery
    {
        private static PSTraceSource activeTracer = null;
        [TraceSource("DisplayDataQuery", "DisplayDataQuery")]
        private static readonly PSTraceSource classTracer = PSTraceSource.GetTracer("DisplayDataQuery", "DisplayDataQuery");

        internal static TypeGroupDefinition FindGroupDefinition(TypeInfoDataBase db, string groupName)
        {
            foreach (TypeGroupDefinition definition in db.typeGroupSection.typeGroupDefinitionList)
            {
                if (string.Equals(definition.name, groupName, StringComparison.OrdinalIgnoreCase))
                {
                    return definition;
                }
            }
            return null;
        }

        internal static AppliesTo GetAllApplicableTypes(TypeInfoDataBase db, AppliesTo appliesTo)
        {
            Hashtable hashtable = new Hashtable(StringComparer.OrdinalIgnoreCase);
            foreach (TypeOrGroupReference reference in appliesTo.referenceList)
            {
                TypeReference reference2 = reference as TypeReference;
                if (reference2 != null)
                {
                    if (!hashtable.ContainsKey(reference2.name))
                    {
                        hashtable.Add(reference2.name, null);
                    }
                }
                else
                {
                    TypeGroupReference reference3 = reference as TypeGroupReference;
                    if (reference3 != null)
                    {
                        TypeGroupDefinition definition = FindGroupDefinition(db, reference3.name);
                        if (definition != null)
                        {
                            foreach (TypeReference reference4 in definition.typeReferenceList)
                            {
                                if (!hashtable.ContainsKey(reference4.name))
                                {
                                    hashtable.Add(reference4.name, null);
                                }
                            }
                        }
                    }
                }
            }
            AppliesTo to = new AppliesTo();
            foreach (DictionaryEntry entry in hashtable)
            {
                to.AddAppliesToType(entry.Key as string);
            }
            return to;
        }

        private static ViewDefinition GetBestMatch(TypeMatch match)
        {
            ViewDefinition bestMatch = match.BestMatch as ViewDefinition;
            if (bestMatch != null)
            {
                TraceHelper(bestMatch, true);
            }
            return bestMatch;
        }

        private static ViewDefinition GetDefaultView(MshExpressionFactory expressionFactory, TypeInfoDataBase db, Collection<string> typeNames)
        {
            TypeMatch match = new TypeMatch(expressionFactory, db, typeNames);
            foreach (ViewDefinition definition in db.viewDefinitionsSection.viewDefinitionList)
            {
                if (definition != null)
                {
                    if (IsOutOfBandView(definition))
                    {
                        ActiveTracer.WriteLine("NOT MATCH OutOfBand {0}  NAME: {1}", new object[] { ControlBase.GetControlShapeName(definition.mainControl), definition.name });
                    }
                    else if (definition.appliesTo == null)
                    {
                        ActiveTracer.WriteLine("NOT MATCH {0}  NAME: {1}  No applicable types", new object[] { ControlBase.GetControlShapeName(definition.mainControl), definition.name });
                    }
                    else
                    {
                        try
                        {
                            TypeMatch.SetTracer(ActiveTracer);
                            if (match.PerfectMatch(new TypeMatchItem(definition, definition.appliesTo)))
                            {
                                TraceHelper(definition, true);
                                return definition;
                            }
                        }
                        finally
                        {
                            TypeMatch.ResetTracer();
                        }
                        TraceHelper(definition, false);
                    }
                }
            }
            ViewDefinition bestMatch = GetBestMatch(match);
            if (bestMatch == null)
            {
                Collection<string> collection = Deserializer.MaskDeserializationPrefix(typeNames);
                if (collection != null)
                {
                    bestMatch = GetDefaultView(expressionFactory, db, collection);
                }
            }
            return bestMatch;
        }

        internal static EnumerableExpansion GetEnumerableExpansionFromType(MshExpressionFactory expressionFactory, TypeInfoDataBase db, Collection<string> typeNames)
        {
            TypeMatch match = new TypeMatch(expressionFactory, db, typeNames);
            foreach (EnumerableExpansionDirective directive in db.defaultSettingsSection.enumerableExpansionDirectiveList)
            {
                if (match.PerfectMatch(new TypeMatchItem(directive, directive.appliesTo)))
                {
                    return directive.enumerableExpansion;
                }
            }
            if (match.BestMatch != null)
            {
                return ((EnumerableExpansionDirective) match.BestMatch).enumerableExpansion;
            }
            Collection<string> collection = Deserializer.MaskDeserializationPrefix(typeNames);
            if (collection != null)
            {
                return GetEnumerableExpansionFromType(expressionFactory, db, collection);
            }
            return EnumerableExpansion.EnumOnly;
        }

        internal static ViewDefinition GetOutOfBandView(MshExpressionFactory expressionFactory, TypeInfoDataBase db, Collection<string> typeNames)
        {
            TypeMatch match = new TypeMatch(expressionFactory, db, typeNames);
            foreach (ViewDefinition definition in db.viewDefinitionsSection.viewDefinitionList)
            {
                if (IsOutOfBandView(definition) && match.PerfectMatch(new TypeMatchItem(definition, definition.appliesTo)))
                {
                    return definition;
                }
            }
            ViewDefinition bestMatch = match.BestMatch as ViewDefinition;
            if (bestMatch == null)
            {
                Collection<string> collection = Deserializer.MaskDeserializationPrefix(typeNames);
                if (collection != null)
                {
                    bestMatch = GetOutOfBandView(expressionFactory, db, collection);
                }
            }
            return bestMatch;
        }

        internal static FormatShape GetShapeFromPropertyCount(TypeInfoDataBase db, int propertyCount)
        {
            if (propertyCount <= db.defaultSettingsSection.shapeSelectionDirectives.PropertyCountForTable)
            {
                return FormatShape.Table;
            }
            return FormatShape.List;
        }

        internal static FormatShape GetShapeFromType(MshExpressionFactory expressionFactory, TypeInfoDataBase db, Collection<string> typeNames)
        {
            ShapeSelectionDirectives shapeSelectionDirectives = db.defaultSettingsSection.shapeSelectionDirectives;
            TypeMatch match = new TypeMatch(expressionFactory, db, typeNames);
            foreach (FormatShapeSelectionOnType type in shapeSelectionDirectives.formatShapeSelectionOnTypeList)
            {
                if (match.PerfectMatch(new TypeMatchItem(type, type.appliesTo)))
                {
                    return type.formatShape;
                }
            }
            if (match.BestMatch != null)
            {
                return ((FormatShapeSelectionOnType) match.BestMatch).formatShape;
            }
            Collection<string> collection = Deserializer.MaskDeserializationPrefix(typeNames);
            if (collection != null)
            {
                return GetShapeFromType(expressionFactory, db, collection);
            }
            return FormatShape.Undefined;
        }

        private static ViewDefinition GetView(MshExpressionFactory expressionFactory, TypeInfoDataBase db, Type mainControlType, Collection<string> typeNames, string viewName)
        {
            TypeMatch match = new TypeMatch(expressionFactory, db, typeNames);
            foreach (ViewDefinition definition in db.viewDefinitionsSection.viewDefinitionList)
            {
                if ((definition == null) || (mainControlType != definition.mainControl.GetType()))
                {
                    ActiveTracer.WriteLine("NOT MATCH {0}  NAME: {1}", new object[] { ControlBase.GetControlShapeName(definition.mainControl), (definition != null) ? definition.name : string.Empty });
                }
                else if (IsOutOfBandView(definition))
                {
                    ActiveTracer.WriteLine("NOT MATCH OutOfBand {0}  NAME: {1}", new object[] { ControlBase.GetControlShapeName(definition.mainControl), definition.name });
                }
                else if (definition.appliesTo == null)
                {
                    ActiveTracer.WriteLine("NOT MATCH {0}  NAME: {1}  No applicable types", new object[] { ControlBase.GetControlShapeName(definition.mainControl), definition.name });
                }
                else if ((viewName != null) && !string.Equals(definition.name, viewName, StringComparison.OrdinalIgnoreCase))
                {
                    ActiveTracer.WriteLine("NOT MATCH {0}  NAME: {1}", new object[] { ControlBase.GetControlShapeName(definition.mainControl), definition.name });
                }
                else
                {
                    try
                    {
                        TypeMatch.SetTracer(ActiveTracer);
                        if (match.PerfectMatch(new TypeMatchItem(definition, definition.appliesTo)))
                        {
                            TraceHelper(definition, true);
                            return definition;
                        }
                    }
                    finally
                    {
                        TypeMatch.ResetTracer();
                    }
                    TraceHelper(definition, false);
                }
            }
            ViewDefinition bestMatch = GetBestMatch(match);
            if (bestMatch == null)
            {
                Collection<string> collection = Deserializer.MaskDeserializationPrefix(typeNames);
                if (collection != null)
                {
                    bestMatch = GetView(expressionFactory, db, mainControlType, collection, viewName);
                }
            }
            return bestMatch;
        }

        internal static ViewDefinition GetViewByShapeAndType(MshExpressionFactory expressionFactory, TypeInfoDataBase db, FormatShape shape, Collection<string> typeNames, string viewName)
        {
            if (shape == FormatShape.Undefined)
            {
                return GetDefaultView(expressionFactory, db, typeNames);
            }
            Type mainControlType = null;
            if (shape == FormatShape.Table)
            {
                mainControlType = typeof(TableControlBody);
            }
            else if (shape == FormatShape.List)
            {
                mainControlType = typeof(ListControlBody);
            }
            else if (shape == FormatShape.Wide)
            {
                mainControlType = typeof(WideControlBody);
            }
            else if (shape == FormatShape.Complex)
            {
                mainControlType = typeof(ComplexControlBody);
            }
            else
            {
                return null;
            }
            return GetView(expressionFactory, db, mainControlType, typeNames, viewName);
        }

        private static bool IsOutOfBandView(ViewDefinition vd)
        {
            return (((vd.mainControl is ComplexControlBody) || (vd.mainControl is ListControlBody)) && vd.outOfBand);
        }

        internal static void ResetTracer()
        {
            activeTracer = classTracer;
        }

        internal static ControlBody ResolveControlReference(TypeInfoDataBase db, List<ControlDefinition> viewControlDefinitionList, ControlReference controlReference)
        {
            ControlBody body = ResolveControlReferenceInList(controlReference, viewControlDefinitionList);
            if (body != null)
            {
                return body;
            }
            return ResolveControlReferenceInList(controlReference, db.formatControlDefinitionHolder.controlDefinitionList);
        }

        private static ControlBody ResolveControlReferenceInList(ControlReference controlReference, List<ControlDefinition> controlDefinitionList)
        {
            foreach (ControlDefinition definition in controlDefinitionList)
            {
                if (!(definition.controlBody.GetType() != controlReference.controlType) && (string.Compare(controlReference.name, definition.name, StringComparison.OrdinalIgnoreCase) == 0))
                {
                    return definition.controlBody;
                }
            }
            return null;
        }

        internal static void SetTracer(PSTraceSource t)
        {
            activeTracer = t;
        }

        private static void TraceHelper(ViewDefinition vd, bool isMatched)
        {
            if ((ActiveTracer.Options & PSTraceSourceOptions.WriteLine) != PSTraceSourceOptions.None)
            {
                foreach (TypeOrGroupReference reference in vd.appliesTo.referenceList)
                {
                    StringBuilder builder = new StringBuilder();
                    TypeReference reference2 = reference as TypeReference;
                    builder.Append(isMatched ? "MATCH FOUND" : "NOT MATCH");
                    if (reference2 != null)
                    {
                        builder.AppendFormat(CultureInfo.InvariantCulture, " {0} NAME: {1}  TYPE: {2}", new object[] { ControlBase.GetControlShapeName(vd.mainControl), vd.name, reference2.name });
                    }
                    else
                    {
                        TypeGroupReference reference3 = reference as TypeGroupReference;
                        builder.AppendFormat(CultureInfo.InvariantCulture, " {0} NAME: {1}  GROUP: {2}", new object[] { ControlBase.GetControlShapeName(vd.mainControl), vd.name, reference3.name });
                    }
                    ActiveTracer.WriteLine(builder.ToString(), new object[0]);
                }
            }
        }

        private static PSTraceSource ActiveTracer
        {
            get
            {
                return (activeTracer ?? classTracer);
            }
        }
    }
}

