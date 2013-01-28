namespace Microsoft.PowerShell.Commands.Internal.Format
{
    using Microsoft.PowerShell.Commands;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Management.Automation;
    using System.Management.Automation.Host;
    using System.Management.Automation.Internal;
    using System.Management.Automation.Security;
    using System.Runtime.InteropServices;
    using System.Xml;

    internal sealed class TypeInfoDataBaseLoader : XmlLoaderBase
    {
        private const string resBaseName = "TypeInfoDataBaseLoaderStrings";
        private bool suppressValidation;
        [TraceSource("TypeInfoDataBaseLoader", "TypeInfoDataBaseLoader")]
        private static PSTraceSource tracer = PSTraceSource.GetTracer("TypeInfoDataBaseLoader", "TypeInfoDataBaseLoader");

        private bool LoadAlignmentValue(System.Xml.XmlNode n, out int alignmentValue)
        {
            alignmentValue = 0;
            string mandatoryInnerText = base.GetMandatoryInnerText(n);
            if (mandatoryInnerText != null)
            {
                if (string.Equals(n.InnerText, "left", StringComparison.OrdinalIgnoreCase))
                {
                    alignmentValue = 1;
                    goto Label_0088;
                }
                if (string.Equals(n.InnerText, "right", StringComparison.OrdinalIgnoreCase))
                {
                    alignmentValue = 3;
                    goto Label_0088;
                }
                if (string.Equals(n.InnerText, "center", StringComparison.OrdinalIgnoreCase))
                {
                    alignmentValue = 2;
                    goto Label_0088;
                }
                base.ReportError(StringUtil.Format(FormatAndOutXmlLoadingStrings.InvalidAlignmentValue, new object[] { base.ComputeCurrentXPath(), base.FilePath, mandatoryInnerText }));
            }
            return false;
        Label_0088:
            return true;
        }

        private AppliesTo LoadAppliesToSection(System.Xml.XmlNode appliesToNode, bool allowSelectionCondition)
        {
            using (base.StackFrame(appliesToNode))
            {
                AppliesTo to = new AppliesTo();
                foreach (System.Xml.XmlNode node in appliesToNode.ChildNodes)
                {
                    using (base.StackFrame(node))
                    {
                        if (base.MatchNodeName(node, "SelectionSetName"))
                        {
                            TypeGroupReference item = this.LoadTypeGroupReference(node);
                            if (item == null)
                            {
                                return null;
                            }
                            to.referenceList.Add(item);
                        }
                        else if (base.MatchNodeName(node, "TypeName"))
                        {
                            TypeReference reference2 = this.LoadTypeReference(node);
                            if (reference2 == null)
                            {
                                return null;
                            }
                            to.referenceList.Add(reference2);
                        }
                        else if (allowSelectionCondition && base.MatchNodeName(node, "SelectionCondition"))
                        {
                            TypeOrGroupReference reference3 = this.LoadSelectionConditionNode(node);
                            if (reference3 == null)
                            {
                                return null;
                            }
                            to.referenceList.Add(reference3);
                        }
                        else
                        {
                            base.ProcessUnknownNode(node);
                        }
                    }
                }
                if (to.referenceList.Count == 0)
                {
                    base.ReportError(StringUtil.Format(FormatAndOutXmlLoadingStrings.EmptyAppliesTo, base.ComputeCurrentXPath(), base.FilePath));
                    return null;
                }
                return to;
            }
        }

        private AppliesTo LoadAppliesToSectionFromObjectModel(List<string> selectedBy)
        {
            AppliesTo to = new AppliesTo();
            foreach (string str in selectedBy)
            {
                if (string.IsNullOrEmpty(str))
                {
                    return null;
                }
                TypeReference item = new TypeReference {
                    name = str
                };
                to.referenceList.Add(item);
            }
            return to;
        }

        private void LoadColumnEntries(System.Xml.XmlNode columnEntriesNode, TableRowDefinition trd)
        {
            using (base.StackFrame(columnEntriesNode))
            {
                int num = 0;
                foreach (System.Xml.XmlNode node in columnEntriesNode.ChildNodes)
                {
                    if (base.MatchNodeName(node, "TableColumnItem"))
                    {
                        TableRowItemDefinition item = this.LoadColumnEntry(node, num++);
                        if (item != null)
                        {
                            trd.rowItemDefinitionList.Add(item);
                            continue;
                        }
                        trd.rowItemDefinitionList = null;
                        return;
                    }
                    base.ProcessUnknownNode(node);
                }
            }
        }

        private void LoadColumnEntriesFromObjectModel(TableRowDefinition trd, List<TableControlColumn> columns, int viewIndex, string typeName)
        {
            foreach (TableControlColumn column in columns)
            {
                TableRowItemDefinition item = new TableRowItemDefinition();
                if (column.DisplayEntry != null)
                {
                    ExpressionToken token = this.LoadExpressionFromObjectModel(column.DisplayEntry, viewIndex, typeName);
                    if (token == null)
                    {
                        trd.rowItemDefinitionList = null;
                        break;
                    }
                    FieldPropertyToken token2 = new FieldPropertyToken {
                        expression = token
                    };
                    item.formatTokenList.Add(token2);
                }
                item.alignment = (int) column.Alignment;
                trd.rowItemDefinitionList.Add(item);
            }
        }

        private TableRowItemDefinition LoadColumnEntry(System.Xml.XmlNode columnEntryNode, int index)
        {
            using (base.StackFrame(columnEntryNode, index))
            {
                ViewEntryNodeMatch match = new ViewEntryNodeMatch(this);
                List<System.Xml.XmlNode> unprocessedNodes = new List<System.Xml.XmlNode>();
                if (!match.ProcessExpressionDirectives(columnEntryNode, unprocessedNodes))
                {
                    return null;
                }
                TableRowItemDefinition definition = new TableRowItemDefinition();
                bool flag = false;
                foreach (System.Xml.XmlNode node in unprocessedNodes)
                {
                    if (base.MatchNodeName(node, "Alignment"))
                    {
                        if (flag)
                        {
                            base.ProcessDuplicateNode(node);
                            return null;
                        }
                        flag = true;
                        if (!this.LoadAlignmentValue(node, out definition.alignment))
                        {
                            return null;
                        }
                    }
                    else
                    {
                        base.ProcessUnknownNode(node);
                    }
                }
                if (match.TextToken != null)
                {
                    definition.formatTokenList.Add(match.TextToken);
                }
                else if (match.Expression != null)
                {
                    FieldPropertyToken item = new FieldPropertyToken {
                        expression = match.Expression
                    };
                    item.fieldFormattingDirective.formatString = match.FormatString;
                    definition.formatTokenList.Add(item);
                }
                return definition;
            }
        }

        private TableColumnHeaderDefinition LoadColumnHeaderDefinition(System.Xml.XmlNode columnHeaderNode, int index)
        {
            using (base.StackFrame(columnHeaderNode, index))
            {
                TableColumnHeaderDefinition definition = new TableColumnHeaderDefinition();
                bool flag = false;
                bool flag2 = false;
                bool flag3 = false;
                foreach (System.Xml.XmlNode node in columnHeaderNode.ChildNodes)
                {
                    if (base.MatchNodeNameWithAttributes(node, "Label"))
                    {
                        if (flag)
                        {
                            base.ProcessDuplicateNode(node);
                            return null;
                        }
                        flag = true;
                        definition.label = this.LoadLabel(node);
                        if (definition.label == null)
                        {
                            return null;
                        }
                    }
                    else if (base.MatchNodeName(node, "Width"))
                    {
                        int num;
                        if (flag2)
                        {
                            base.ProcessDuplicateNode(node);
                            return null;
                        }
                        flag2 = true;
                        if (!this.ReadPositiveIntegerValue(node, out num))
                        {
                            base.ReportError(StringUtil.Format(FormatAndOutXmlLoadingStrings.InvalidNodeValue, new object[] { base.ComputeCurrentXPath(), base.FilePath, "Width" }));
                            return null;
                        }
                        definition.width = num;
                    }
                    else if (base.MatchNodeName(node, "Alignment"))
                    {
                        if (flag3)
                        {
                            base.ProcessDuplicateNode(node);
                            return null;
                        }
                        flag3 = true;
                        if (!this.LoadAlignmentValue(node, out definition.alignment))
                        {
                            return null;
                        }
                    }
                    else
                    {
                        base.ProcessUnknownNode(node);
                    }
                }
                return definition;
            }
        }

        private bool LoadCommonViewData(System.Xml.XmlNode viewNode, ViewDefinition view, List<System.Xml.XmlNode> unprocessedNodes)
        {
            if (viewNode == null)
            {
                throw PSTraceSource.NewArgumentNullException("viewNode");
            }
            if (view == null)
            {
                throw PSTraceSource.NewArgumentNullException("view");
            }
            view.loadingInfo = base.LoadingInfo;
            view.loadingInfo.xPath = base.ComputeCurrentXPath();
            bool flag = false;
            bool flag2 = false;
            bool flag3 = false;
            foreach (System.Xml.XmlNode node in viewNode.ChildNodes)
            {
                if (base.MatchNodeName(node, "Name"))
                {
                    if (flag)
                    {
                        base.ProcessDuplicateNode(node);
                        return false;
                    }
                    flag = true;
                    view.name = base.GetMandatoryInnerText(node);
                    if (view.name == null)
                    {
                        return false;
                    }
                }
                else if (base.MatchNodeName(node, "ViewSelectedBy"))
                {
                    if (flag2)
                    {
                        base.ProcessDuplicateNode(node);
                        return false;
                    }
                    flag2 = true;
                    view.appliesTo = this.LoadAppliesToSection(node, false);
                    if (view.appliesTo == null)
                    {
                        return false;
                    }
                }
                else if (base.MatchNodeName(node, "GroupBy"))
                {
                    if (flag3)
                    {
                        base.ProcessDuplicateNode(node);
                        return false;
                    }
                    flag3 = true;
                    view.groupBy = this.LoadGroupBySection(node);
                    if (view.groupBy == null)
                    {
                        return false;
                    }
                }
                else
                {
                    unprocessedNodes.Add(node);
                }
            }
            if (!flag)
            {
                base.ReportMissingNode("Name");
                return false;
            }
            if (!flag2)
            {
                base.ReportMissingNode("ViewSelectedBy");
                return false;
            }
            return true;
        }

        private ComplexControlBody LoadComplexControl(System.Xml.XmlNode controlNode)
        {
            using (base.StackFrame(controlNode))
            {
                ComplexControlBody complexBody = new ComplexControlBody();
                bool flag = false;
                foreach (System.Xml.XmlNode node in controlNode.ChildNodes)
                {
                    if (base.MatchNodeName(node, "CustomEntries"))
                    {
                        if (!flag)
                        {
                            flag = true;
                            this.LoadComplexControlEntries(node, complexBody);
                            if (complexBody.defaultEntry == null)
                            {
                                return null;
                            }
                        }
                        else
                        {
                            base.ProcessDuplicateNode(node);
                        }
                    }
                    else
                    {
                        base.ProcessUnknownNode(node);
                    }
                }
                if (!flag)
                {
                    base.ReportMissingNode("CustomEntries");
                    return null;
                }
                return complexBody;
            }
        }

        private void LoadComplexControlEntries(System.Xml.XmlNode complexControlEntriesNode, ComplexControlBody complexBody)
        {
            using (base.StackFrame(complexControlEntriesNode))
            {
                int num = 0;
                foreach (System.Xml.XmlNode node in complexControlEntriesNode.ChildNodes)
                {
                    if (base.MatchNodeName(node, "CustomEntry"))
                    {
                        ComplexControlEntryDefinition item = this.LoadComplexControlEntryDefinition(node, num++);
                        if (item == null)
                        {
                            base.ReportError(StringUtil.Format(FormatAndOutXmlLoadingStrings.LoadTagFailed, new object[] { base.ComputeCurrentXPath(), base.FilePath, "CustomEntry" }));
                            complexBody.defaultEntry = null;
                            return;
                        }
                        if (item.appliesTo == null)
                        {
                            if (complexBody.defaultEntry == null)
                            {
                                complexBody.defaultEntry = item;
                                continue;
                            }
                            base.ReportError(StringUtil.Format(FormatAndOutXmlLoadingStrings.TooManyDefaultShapeEntry, new object[] { base.ComputeCurrentXPath(), base.FilePath, "CustomEntry" }));
                            complexBody.defaultEntry = null;
                            return;
                        }
                        complexBody.optionalEntryList.Add(item);
                    }
                    else
                    {
                        base.ProcessUnknownNode(node);
                    }
                }
                if (complexBody.defaultEntry == null)
                {
                    base.ReportError(StringUtil.Format(FormatAndOutXmlLoadingStrings.NoDefaultShapeEntry, new object[] { base.ComputeCurrentXPath(), base.FilePath, "CustomEntry" }));
                }
            }
        }

        private ComplexControlEntryDefinition LoadComplexControlEntryDefinition(System.Xml.XmlNode complexControlEntryNode, int index)
        {
            using (base.StackFrame(complexControlEntryNode, index))
            {
                bool flag = false;
                bool flag2 = false;
                ComplexControlEntryDefinition definition = new ComplexControlEntryDefinition();
                foreach (System.Xml.XmlNode node in complexControlEntryNode.ChildNodes)
                {
                    if (base.MatchNodeName(node, "EntrySelectedBy"))
                    {
                        if (flag)
                        {
                            base.ProcessDuplicateNode(node);
                            return null;
                        }
                        flag = true;
                        definition.appliesTo = this.LoadAppliesToSection(node, true);
                    }
                    else if (base.MatchNodeName(node, "CustomItem"))
                    {
                        if (flag2)
                        {
                            base.ProcessDuplicateNode(node);
                            return null;
                        }
                        flag2 = true;
                        definition.itemDefinition.formatTokenList = this.LoadComplexControlTokenListDefinitions(node);
                    }
                    else
                    {
                        base.ProcessUnknownNode(node);
                    }
                }
                if (definition.itemDefinition.formatTokenList == null)
                {
                    base.ReportError(StringUtil.Format(FormatAndOutXmlLoadingStrings.MissingNode, new object[] { base.ComputeCurrentXPath(), base.FilePath, "CustomItem" }));
                    return null;
                }
                return definition;
            }
        }

        private List<FormatToken> LoadComplexControlTokenListDefinitions(System.Xml.XmlNode bodyNode)
        {
            using (base.StackFrame(bodyNode))
            {
                List<FormatToken> list = new List<FormatToken>();
                int num = 0;
                int num2 = 0;
                int num3 = 0;
                int num4 = 0;
                foreach (System.Xml.XmlNode node in bodyNode.ChildNodes)
                {
                    if (base.MatchNodeName(node, "ExpressionBinding"))
                    {
                        CompoundPropertyToken item = this.LoadCompoundProperty(node, num++);
                        if (item == null)
                        {
                            base.ReportError(StringUtil.Format(FormatAndOutXmlLoadingStrings.LoadTagFailed, new object[] { base.ComputeCurrentXPath(), base.FilePath, "ExpressionBinding" }));
                            return null;
                        }
                        list.Add(item);
                    }
                    else if (base.MatchNodeName(node, "NewLine"))
                    {
                        NewLineToken token2 = this.LoadNewLine(node, num2++);
                        if (token2 == null)
                        {
                            base.ReportError(StringUtil.Format(FormatAndOutXmlLoadingStrings.LoadTagFailed, new object[] { base.ComputeCurrentXPath(), base.FilePath, "NewLine" }));
                            return null;
                        }
                        list.Add(token2);
                    }
                    else if (base.MatchNodeNameWithAttributes(node, "Text"))
                    {
                        TextToken token3 = this.LoadText(node, num3++);
                        if (token3 == null)
                        {
                            base.ReportError(StringUtil.Format(FormatAndOutXmlLoadingStrings.LoadTagFailed, new object[] { base.ComputeCurrentXPath(), base.FilePath, "Text" }));
                            return null;
                        }
                        list.Add(token3);
                    }
                    else if (base.MatchNodeName(node, "Frame"))
                    {
                        FrameToken token4 = this.LoadFrameDefinition(node, num4++);
                        if (token4 == null)
                        {
                            base.ReportError(StringUtil.Format(FormatAndOutXmlLoadingStrings.LoadTagFailed, new object[] { base.ComputeCurrentXPath(), base.FilePath, "Frame" }));
                            return null;
                        }
                        list.Add(token4);
                    }
                    else
                    {
                        base.ProcessUnknownNode(node);
                    }
                }
                if (list.Count == 0)
                {
                    base.ReportError(StringUtil.Format(FormatAndOutXmlLoadingStrings.EmptyCustomControlList, base.ComputeCurrentXPath(), base.FilePath));
                    return null;
                }
                return list;
            }
        }

        private CompoundPropertyToken LoadCompoundProperty(System.Xml.XmlNode compoundPropertyNode, int index)
        {
            using (base.StackFrame(compoundPropertyNode, index))
            {
                CompoundPropertyToken ptb = new CompoundPropertyToken();
                List<System.Xml.XmlNode> unprocessedNodes = new List<System.Xml.XmlNode>();
                if (!this.LoadPropertyBaseHelper(compoundPropertyNode, ptb, unprocessedNodes))
                {
                    return null;
                }
                ptb.control = null;
                bool flag2 = false;
                bool flag3 = false;
                ComplexControlMatch match = new ComplexControlMatch(this);
                FieldControlBody body = null;
                foreach (System.Xml.XmlNode node in unprocessedNodes)
                {
                    if (match.MatchNode(node))
                    {
                        if (flag2)
                        {
                            base.ProcessDuplicateAlternateNode(node, "CustomControl", "CustomControlName");
                            return null;
                        }
                        flag2 = true;
                        if (!match.ProcessNode(node))
                        {
                            return null;
                        }
                    }
                    else if (base.MatchNodeName(node, "FieldControl"))
                    {
                        if (flag3)
                        {
                            base.ProcessDuplicateAlternateNode(node, "CustomControl", "CustomControlName");
                            return null;
                        }
                        flag3 = true;
                        body = new FieldControlBody {
                            fieldFormattingDirective = { formatString = base.GetMandatoryInnerText(node) }
                        };
                        if (body.fieldFormattingDirective.formatString == null)
                        {
                            return null;
                        }
                    }
                    else
                    {
                        base.ProcessUnknownNode(node);
                    }
                }
                if (flag3 && flag2)
                {
                    base.ProcessDuplicateAlternateNode("CustomControl", "CustomControlName");
                    return null;
                }
                if (flag3)
                {
                    ptb.control = body;
                }
                else
                {
                    ptb.control = match.Control;
                }
                return ptb;
            }
        }

        private ControlDefinition LoadControlDefinition(System.Xml.XmlNode controlDefinitionNode, int index)
        {
            using (base.StackFrame(controlDefinitionNode, index))
            {
                bool flag = false;
                bool flag2 = false;
                ControlDefinition definition = new ControlDefinition();
                foreach (System.Xml.XmlNode node in controlDefinitionNode.ChildNodes)
                {
                    if (base.MatchNodeName(node, "Name"))
                    {
                        if (!flag)
                        {
                            flag = true;
                            definition.name = base.GetMandatoryInnerText(node);
                            if (definition.name == null)
                            {
                                base.ReportError(StringUtil.Format(FormatAndOutXmlLoadingStrings.NullControlName, base.ComputeCurrentXPath(), base.FilePath));
                                return null;
                            }
                        }
                        else
                        {
                            base.ProcessDuplicateNode(node);
                        }
                    }
                    else if (base.MatchNodeName(node, "CustomControl"))
                    {
                        if (flag2)
                        {
                            base.ProcessDuplicateNode(node);
                            return null;
                        }
                        flag2 = true;
                        definition.controlBody = this.LoadComplexControl(node);
                        if (definition.controlBody == null)
                        {
                            return null;
                        }
                    }
                    else
                    {
                        base.ProcessUnknownNode(node);
                    }
                }
                if (definition.name == null)
                {
                    base.ReportMissingNode("Name");
                    return null;
                }
                if (definition.controlBody == null)
                {
                    base.ReportMissingNode("CustomControl");
                    return null;
                }
                return definition;
            }
        }

        private void LoadControlDefinitions(System.Xml.XmlNode definitionsNode, List<ControlDefinition> controlDefinitionList)
        {
            using (base.StackFrame(definitionsNode))
            {
                int num = 0;
                foreach (System.Xml.XmlNode node in definitionsNode.ChildNodes)
                {
                    if (base.MatchNodeName(node, "Control"))
                    {
                        ControlDefinition item = this.LoadControlDefinition(node, num++);
                        if (item != null)
                        {
                            controlDefinitionList.Add(item);
                        }
                    }
                    else
                    {
                        base.ProcessUnknownNode(node);
                    }
                }
            }
        }

        private void LoadData(ExtendedTypeDefinition typeDefinition, TypeInfoDataBase db)
        {
            if (typeDefinition == null)
            {
                throw PSTraceSource.NewArgumentNullException("viewDefinition");
            }
            if (db == null)
            {
                throw PSTraceSource.NewArgumentNullException("db");
            }
            int num = 0;
            foreach (FormatViewDefinition definition in typeDefinition.FormatViewDefinition)
            {
                ViewDefinition item = this.LoadViewFromObjectModle(typeDefinition.TypeName, definition, num++);
                if (item != null)
                {
                    base.ReportTrace(string.Format(CultureInfo.InvariantCulture, "{0} view {1} is loaded from the 'FormatViewDefinition' at index {2} in 'ExtendedTypeDefinition' with type name {3}", new object[] { ControlBase.GetControlShapeName(item.mainControl), item.name, num - 1, typeDefinition.TypeName }));
                    db.viewDefinitionsSection.viewDefinitionList.Add(item);
                }
            }
        }

        private void LoadData(XmlDocument doc, TypeInfoDataBase db)
        {
            if (doc == null)
            {
                throw PSTraceSource.NewArgumentNullException("doc");
            }
            if (db == null)
            {
                throw PSTraceSource.NewArgumentNullException("db");
            }
            XmlElement documentElement = doc.DocumentElement;
            bool flag = false;
            bool flag2 = false;
            bool flag3 = false;
            bool flag4 = false;
            if (base.MatchNodeName(documentElement, "Configuration"))
            {
                using (base.StackFrame(documentElement))
                {
                    foreach (System.Xml.XmlNode node in documentElement.ChildNodes)
                    {
                        if (base.MatchNodeName(node, "DefaultSettings"))
                        {
                            if (flag)
                            {
                                base.ProcessDuplicateNode(node);
                            }
                            flag = true;
                            this.LoadDefaultSettings(db, node);
                        }
                        else if (base.MatchNodeName(node, "SelectionSets"))
                        {
                            if (flag2)
                            {
                                base.ProcessDuplicateNode(node);
                            }
                            flag2 = true;
                            this.LoadTypeGroups(db, node);
                        }
                        else if (base.MatchNodeName(node, "ViewDefinitions"))
                        {
                            if (flag3)
                            {
                                base.ProcessDuplicateNode(node);
                            }
                            flag3 = true;
                            this.LoadViewDefinitions(db, node);
                        }
                        else if (base.MatchNodeName(node, "Controls"))
                        {
                            if (flag4)
                            {
                                base.ProcessDuplicateNode(node);
                            }
                            flag4 = true;
                            this.LoadControlDefinitions(node, db.formatControlDefinitionHolder.controlDefinitionList);
                        }
                        else
                        {
                            base.ProcessUnknownNode(node);
                        }
                    }
                    return;
                }
            }
            base.ProcessUnknownNode(documentElement);
        }

        private void LoadDefaultSettings(TypeInfoDataBase db, System.Xml.XmlNode defaultSettingsNode)
        {
            bool flag = false;
            bool flag2 = false;
            bool flag3 = false;
            bool flag4 = false;
            bool flag5 = false;
            using (base.StackFrame(defaultSettingsNode))
            {
                foreach (System.Xml.XmlNode node in defaultSettingsNode.ChildNodes)
                {
                    bool flag6;
                    if (base.MatchNodeName(node, "ShowError"))
                    {
                        if (flag2)
                        {
                            base.ProcessDuplicateNode(node);
                        }
                        flag2 = true;
                        if (this.ReadBooleanNode(node, out flag6))
                        {
                            db.defaultSettingsSection.formatErrorPolicy.ShowErrorsAsMessages = flag6;
                        }
                    }
                    else if (base.MatchNodeName(node, "DisplayError"))
                    {
                        if (flag3)
                        {
                            base.ProcessDuplicateNode(node);
                        }
                        flag3 = true;
                        if (this.ReadBooleanNode(node, out flag6))
                        {
                            db.defaultSettingsSection.formatErrorPolicy.ShowErrorsInFormattedOutput = flag6;
                        }
                    }
                    else if (base.MatchNodeName(node, "PropertyCountForTable"))
                    {
                        int num;
                        if (flag)
                        {
                            base.ProcessDuplicateNode(node);
                        }
                        flag = true;
                        if (this.ReadPositiveIntegerValue(node, out num))
                        {
                            db.defaultSettingsSection.shapeSelectionDirectives.PropertyCountForTable = num;
                        }
                        else
                        {
                            base.ReportError(StringUtil.Format(FormatAndOutXmlLoadingStrings.InvalidNodeValue, new object[] { base.ComputeCurrentXPath(), base.FilePath, "PropertyCountForTable" }));
                        }
                    }
                    else if (base.MatchNodeName(node, "WrapTables"))
                    {
                        if (flag5)
                        {
                            base.ProcessDuplicateNode(node);
                        }
                        flag5 = true;
                        if (this.ReadBooleanNode(node, out flag6))
                        {
                            db.defaultSettingsSection.MultilineTables = flag6;
                        }
                        else
                        {
                            base.ReportError(StringUtil.Format(FormatAndOutXmlLoadingStrings.InvalidNodeValue, new object[] { base.ComputeCurrentXPath(), base.FilePath, "WrapTables" }));
                        }
                    }
                    else if (base.MatchNodeName(node, "EnumerableExpansions"))
                    {
                        if (flag4)
                        {
                            base.ProcessDuplicateNode(node);
                        }
                        flag4 = true;
                        db.defaultSettingsSection.enumerableExpansionDirectiveList = this.LoadEnumerableExpansionDirectiveList(node);
                    }
                    else
                    {
                        base.ProcessUnknownNode(node);
                    }
                }
            }
        }

        private EnumerableExpansionDirective LoadEnumerableExpansionDirective(System.Xml.XmlNode directive, int index)
        {
            using (base.StackFrame(directive, index))
            {
                EnumerableExpansionDirective directive2 = new EnumerableExpansionDirective();
                bool flag = false;
                bool flag2 = false;
                foreach (System.Xml.XmlNode node in directive.ChildNodes)
                {
                    if (base.MatchNodeName(node, "EntrySelectedBy"))
                    {
                        if (flag)
                        {
                            base.ProcessDuplicateNode(node);
                            return null;
                        }
                        flag = true;
                        directive2.appliesTo = this.LoadAppliesToSection(node, true);
                    }
                    else if (base.MatchNodeName(node, "Expand"))
                    {
                        if (flag2)
                        {
                            base.ProcessDuplicateNode(node);
                            return null;
                        }
                        flag2 = true;
                        string mandatoryInnerText = base.GetMandatoryInnerText(node);
                        if (mandatoryInnerText == null)
                        {
                            return null;
                        }
                        if (!EnumerableExpansionConversion.Convert(mandatoryInnerText, out directive2.enumerableExpansion))
                        {
                            base.ReportError(StringUtil.Format(FormatAndOutXmlLoadingStrings.InvalidNodeValue, new object[] { base.ComputeCurrentXPath(), base.FilePath, "Expand" }));
                            return null;
                        }
                    }
                    else
                    {
                        base.ProcessUnknownNode(node);
                    }
                }
                return directive2;
            }
        }

        private List<EnumerableExpansionDirective> LoadEnumerableExpansionDirectiveList(System.Xml.XmlNode expansionListNode)
        {
            List<EnumerableExpansionDirective> list = new List<EnumerableExpansionDirective>();
            using (base.StackFrame(expansionListNode))
            {
                int num = 0;
                foreach (System.Xml.XmlNode node in expansionListNode.ChildNodes)
                {
                    if (base.MatchNodeName(node, "EnumerableExpansion"))
                    {
                        EnumerableExpansionDirective item = this.LoadEnumerableExpansionDirective(node, num++);
                        if (item == null)
                        {
                            base.ReportError(StringUtil.Format(FormatAndOutXmlLoadingStrings.LoadTagFailed, new object[] { base.ComputeCurrentXPath(), base.FilePath, "EnumerableExpansion" }));
                            return null;
                        }
                        list.Add(item);
                    }
                    else
                    {
                        base.ProcessUnknownNode(node);
                    }
                }
            }
            return list;
        }

        private ExpressionToken LoadExpressionFromObjectModel(DisplayEntry displayEntry, int viewIndex, string typeName)
        {
            ExpressionToken token = new ExpressionToken();
            if (displayEntry.ValueType.Equals(DisplayEntryValueType.Property))
            {
                token.expressionValue = displayEntry.Value;
                return token;
            }
            if (displayEntry.ValueType.Equals(DisplayEntryValueType.ScriptBlock))
            {
                token.isScriptBlock = true;
                token.expressionValue = displayEntry.Value;
                try
                {
                    base.expressionFactory.VerifyScriptBlockText(token.expressionValue);
                }
                catch (ParseException exception)
                {
                    base.ReportErrorForLoadingFromObjectModel(StringUtil.Format(FormatAndOutXmlLoadingStrings.InvalidScriptBlockInFormattingData, new object[] { typeName, viewIndex, exception.Message }), typeName);
                    return null;
                }
                catch (Exception)
                {
                    throw;
                }
                return token;
            }
            PSTraceSource.NewInvalidOperationException();
            return null;
        }

        internal bool LoadFormattingData(ExtendedTypeDefinition typeDefinition, TypeInfoDataBase db, MshExpressionFactory expressionFactory)
        {
            if (typeDefinition == null)
            {
                throw PSTraceSource.NewArgumentNullException("typeDefinition");
            }
            if (typeDefinition.TypeName == null)
            {
                throw PSTraceSource.NewArgumentNullException("typeDefinition.TypeName");
            }
            if (db == null)
            {
                throw PSTraceSource.NewArgumentNullException("db");
            }
            if (expressionFactory == null)
            {
                throw PSTraceSource.NewArgumentNullException("expressionFactory");
            }
            base.expressionFactory = expressionFactory;
            base.ReportTrace("loading ExtendedTypeDefinition started");
            try
            {
                this.LoadData(typeDefinition, db);
            }
            catch (TooManyErrorsException)
            {
                return false;
            }
            catch (Exception exception)
            {
                base.ReportErrorForLoadingFromObjectModel(StringUtil.Format(FormatAndOutXmlLoadingStrings.ErrorInFormattingData, typeDefinition.TypeName, exception.Message), typeDefinition.TypeName);
                throw;
            }
            if (base.HasErrors)
            {
                return false;
            }
            base.ReportTrace("ExtendedTypeDefinition loaded with no errors");
            return true;
        }

        private FrameToken LoadFrameDefinition(System.Xml.XmlNode frameNode, int index)
        {
            using (base.StackFrame(frameNode, index))
            {
                bool flag = false;
                bool flag2 = false;
                bool flag3 = false;
                bool flag4 = false;
                bool flag5 = false;
                FrameToken token = new FrameToken();
                foreach (System.Xml.XmlNode node in frameNode.ChildNodes)
                {
                    bool flag6;
                    if (base.MatchNodeName(node, "LeftIndent"))
                    {
                        if (flag2)
                        {
                            base.ProcessDuplicateNode(node);
                            return null;
                        }
                        flag2 = true;
                        token.frameInfoDefinition.leftIndentation = this.LoadPositiveOrZeroIntegerValue(node, out flag6);
                        if (!flag6)
                        {
                            return null;
                        }
                    }
                    else if (base.MatchNodeName(node, "RightIndent"))
                    {
                        if (flag3)
                        {
                            base.ProcessDuplicateNode(node);
                            return null;
                        }
                        flag3 = true;
                        token.frameInfoDefinition.rightIndentation = this.LoadPositiveOrZeroIntegerValue(node, out flag6);
                        if (!flag6)
                        {
                            return null;
                        }
                    }
                    else if (base.MatchNodeName(node, "FirstLineIndent"))
                    {
                        if (flag4)
                        {
                            base.ProcessDuplicateAlternateNode(node, "FirstLineIndent", "FirstLineHanging");
                            return null;
                        }
                        flag4 = true;
                        token.frameInfoDefinition.firstLine = this.LoadPositiveOrZeroIntegerValue(node, out flag6);
                        if (!flag6)
                        {
                            return null;
                        }
                    }
                    else if (base.MatchNodeName(node, "FirstLineHanging"))
                    {
                        if (flag5)
                        {
                            base.ProcessDuplicateAlternateNode(node, "FirstLineIndent", "FirstLineHanging");
                            return null;
                        }
                        flag5 = true;
                        token.frameInfoDefinition.firstLine = this.LoadPositiveOrZeroIntegerValue(node, out flag6);
                        if (!flag6)
                        {
                            return null;
                        }
                        token.frameInfoDefinition.firstLine = -token.frameInfoDefinition.firstLine;
                    }
                    else if (base.MatchNodeName(node, "CustomItem"))
                    {
                        if (flag)
                        {
                            base.ProcessDuplicateNode(node);
                            return null;
                        }
                        flag = true;
                        token.itemDefinition.formatTokenList = this.LoadComplexControlTokenListDefinitions(node);
                    }
                    else
                    {
                        base.ProcessUnknownNode(node);
                    }
                }
                if (flag5 && flag4)
                {
                    base.ProcessDuplicateAlternateNode("FirstLineIndent", "FirstLineHanging");
                    return null;
                }
                if (token.itemDefinition.formatTokenList == null)
                {
                    base.ReportError(StringUtil.Format(FormatAndOutXmlLoadingStrings.MissingNode, new object[] { base.ComputeCurrentXPath(), base.FilePath, "CustomItem" }));
                    return null;
                }
                return token;
            }
        }

        private GroupBy LoadGroupBySection(System.Xml.XmlNode groupByNode)
        {
            using (base.StackFrame(groupByNode))
            {
                ExpressionNodeMatch match = new ExpressionNodeMatch(this);
                ComplexControlMatch match2 = new ComplexControlMatch(this);
                bool flag = false;
                bool flag2 = false;
                bool flag3 = false;
                GroupBy by = new GroupBy();
                TextToken token = null;
                foreach (System.Xml.XmlNode node in groupByNode)
                {
                    if (match.MatchNode(node))
                    {
                        if (flag)
                        {
                            base.ProcessDuplicateNode(node);
                            return null;
                        }
                        flag = true;
                        if (!match.ProcessNode(node))
                        {
                            return null;
                        }
                    }
                    else if (match2.MatchNode(node))
                    {
                        if (flag2)
                        {
                            base.ProcessDuplicateAlternateNode(node, "CustomControl", "CustomControlName");
                            return null;
                        }
                        flag2 = true;
                        if (!match2.ProcessNode(node))
                        {
                            return null;
                        }
                    }
                    else if (base.MatchNodeNameWithAttributes(node, "Label"))
                    {
                        if (flag3)
                        {
                            base.ProcessDuplicateAlternateNode(node, "CustomControl", "CustomControlName");
                            return null;
                        }
                        flag3 = true;
                        token = this.LoadLabel(node);
                        if (token == null)
                        {
                            return null;
                        }
                    }
                    else
                    {
                        base.ProcessUnknownNode(node);
                    }
                }
                if (flag2 && flag3)
                {
                    base.ReportError(StringUtil.Format(FormatAndOutXmlLoadingStrings.ControlAndLabel, base.ComputeCurrentXPath(), base.FilePath));
                    return null;
                }
                if (flag2 || flag3)
                {
                    if (!flag)
                    {
                        base.ReportError(StringUtil.Format(FormatAndOutXmlLoadingStrings.ControlLabelWithoutExpression, base.ComputeCurrentXPath(), base.FilePath));
                        return null;
                    }
                    if (flag2)
                    {
                        by.startGroup.control = match2.Control;
                    }
                    else if (flag3)
                    {
                        by.startGroup.labelTextToken = token;
                    }
                }
                if (flag)
                {
                    ExpressionToken token2 = match.GenerateExpressionToken();
                    if (token2 == null)
                    {
                        return null;
                    }
                    by.startGroup.expression = token2;
                    return by;
                }
                base.ReportError(StringUtil.Format(FormatAndOutXmlLoadingStrings.ExpectExpression, base.ComputeCurrentXPath(), base.FilePath));
                return null;
            }
        }

        private void LoadHeadersSection(TableControlBody tableBody, System.Xml.XmlNode headersNode)
        {
            using (base.StackFrame(headersNode))
            {
                int num = 0;
                foreach (System.Xml.XmlNode node in headersNode.ChildNodes)
                {
                    if (base.MatchNodeName(node, "TableColumnHeader"))
                    {
                        TableColumnHeaderDefinition item = this.LoadColumnHeaderDefinition(node, num++);
                        if (item != null)
                        {
                            tableBody.header.columnHeaderDefinitionList.Add(item);
                            continue;
                        }
                        base.ReportError(StringUtil.Format(FormatAndOutXmlLoadingStrings.InvalidColumnHeader, base.ComputeCurrentXPath(), base.FilePath));
                        tableBody.header.columnHeaderDefinitionList = null;
                        return;
                    }
                    base.ProcessUnknownNode(node);
                }
            }
        }

        private void LoadHeadersSectionFromObjectModel(TableControlBody tableBody, List<TableControlColumnHeader> headers)
        {
            foreach (TableControlColumnHeader header in headers)
            {
                TableColumnHeaderDefinition item = new TableColumnHeaderDefinition();
                if (!string.IsNullOrEmpty(header.Label))
                {
                    TextToken token = new TextToken {
                        text = header.Label
                    };
                    item.label = token;
                }
                item.width = header.Width;
                item.alignment = (int) header.Alignment;
                tableBody.header.columnHeaderDefinitionList.Add(item);
            }
        }

        private int LoadIntegerValue(System.Xml.XmlNode node, out bool success)
        {
            using (base.StackFrame(node))
            {
                success = false;
                int result = 0;
                if (base.VerifyNodeHasNoChildren(node))
                {
                    string mandatoryInnerText = base.GetMandatoryInnerText(node);
                    if (mandatoryInnerText == null)
                    {
                        base.ReportError(StringUtil.Format(FormatAndOutXmlLoadingStrings.MissingInnerText, base.ComputeCurrentXPath(), base.FilePath));
                        return result;
                    }
                    if (!int.TryParse(mandatoryInnerText, out result))
                    {
                        base.ReportError(StringUtil.Format(FormatAndOutXmlLoadingStrings.ExpectInteger, base.ComputeCurrentXPath(), base.FilePath));
                        return result;
                    }
                    success = true;
                }
                return result;
            }
        }

        private ExpressionToken LoadItemSelectionCondition(System.Xml.XmlNode itemNode)
        {
            using (base.StackFrame(itemNode))
            {
                bool flag = false;
                ExpressionNodeMatch match = new ExpressionNodeMatch(this);
                foreach (System.Xml.XmlNode node in itemNode.ChildNodes)
                {
                    if (match.MatchNode(node))
                    {
                        if (flag)
                        {
                            base.ProcessDuplicateNode(node);
                            return null;
                        }
                        flag = true;
                        if (!match.ProcessNode(node))
                        {
                            return null;
                        }
                    }
                    else
                    {
                        base.ProcessUnknownNode(node);
                    }
                }
                return match.GenerateExpressionToken();
            }
        }

        private TextToken LoadLabel(System.Xml.XmlNode textNode)
        {
            using (base.StackFrame(textNode))
            {
                return this.LoadTextToken(textNode);
            }
        }

        private ListControlBody LoadListControl(System.Xml.XmlNode controlNode)
        {
            using (base.StackFrame(controlNode))
            {
                ListControlBody listBody = new ListControlBody();
                bool flag = false;
                foreach (System.Xml.XmlNode node in controlNode.ChildNodes)
                {
                    if (base.MatchNodeName(node, "ListEntries"))
                    {
                        if (!flag)
                        {
                            flag = true;
                            this.LoadListControlEntries(node, listBody);
                            if (listBody.defaultEntryDefinition == null)
                            {
                                return null;
                            }
                        }
                        else
                        {
                            base.ProcessDuplicateNode(node);
                        }
                    }
                    else
                    {
                        base.ProcessUnknownNode(node);
                    }
                }
                if (!flag)
                {
                    base.ReportMissingNode("ListEntries");
                    return null;
                }
                return listBody;
            }
        }

        private void LoadListControlEntries(System.Xml.XmlNode listViewEntriesNode, ListControlBody listBody)
        {
            using (base.StackFrame(listViewEntriesNode))
            {
                int num = 0;
                foreach (System.Xml.XmlNode node in listViewEntriesNode.ChildNodes)
                {
                    if (base.MatchNodeName(node, "ListEntry"))
                    {
                        ListControlEntryDefinition item = this.LoadListControlEntryDefinition(node, num++);
                        if (item == null)
                        {
                            base.ReportError(StringUtil.Format(FormatAndOutXmlLoadingStrings.LoadTagFailed, new object[] { base.ComputeCurrentXPath(), base.FilePath, "ListEntry" }));
                            listBody.defaultEntryDefinition = null;
                            return;
                        }
                        if (item.appliesTo == null)
                        {
                            if (listBody.defaultEntryDefinition == null)
                            {
                                listBody.defaultEntryDefinition = item;
                                continue;
                            }
                            base.ReportError(StringUtil.Format(FormatAndOutXmlLoadingStrings.TooManyDefaultShapeEntry, new object[] { base.ComputeCurrentXPath(), base.FilePath, "ListEntry" }));
                            listBody.defaultEntryDefinition = null;
                            return;
                        }
                        listBody.optionalEntryList.Add(item);
                    }
                    else
                    {
                        base.ProcessUnknownNode(node);
                    }
                }
                if (listBody.optionalEntryList == null)
                {
                    base.ReportError(StringUtil.Format(FormatAndOutXmlLoadingStrings.NoDefaultShapeEntry, new object[] { base.ComputeCurrentXPath(), base.FilePath, "ListEntry" }));
                }
            }
        }

        private void LoadListControlEntriesFromObjectModel(ListControlBody listBody, List<ListControlEntry> entries, int viewIndex, string typeName)
        {
            foreach (ListControlEntry entry in entries)
            {
                ListControlEntryDefinition item = this.LoadListControlEntryDefinitionFromObjectModel(entry, viewIndex, typeName);
                if (item == null)
                {
                    base.ReportErrorForLoadingFromObjectModel(StringUtil.Format(FormatAndOutXmlLoadingStrings.LoadTagFailedInFormattingData, new object[] { typeName, viewIndex, "ListEntry" }), typeName);
                    listBody.defaultEntryDefinition = null;
                    return;
                }
                if (item.appliesTo == null)
                {
                    if (listBody.defaultEntryDefinition == null)
                    {
                        listBody.defaultEntryDefinition = item;
                        continue;
                    }
                    base.ReportErrorForLoadingFromObjectModel(StringUtil.Format(FormatAndOutXmlLoadingStrings.TooManyDefaultShapeEntryInFormattingData, new object[] { typeName, viewIndex, "ListEntry" }), typeName);
                    listBody.defaultEntryDefinition = null;
                    return;
                }
                listBody.optionalEntryList.Add(item);
            }
            if (listBody.defaultEntryDefinition == null)
            {
                base.ReportErrorForLoadingFromObjectModel(StringUtil.Format(FormatAndOutXmlLoadingStrings.NoDefaultShapeEntryInFormattingData, new object[] { typeName, viewIndex, "ListEntry" }), typeName);
            }
        }

        private ListControlEntryDefinition LoadListControlEntryDefinition(System.Xml.XmlNode listViewEntryNode, int index)
        {
            using (base.StackFrame(listViewEntryNode, index))
            {
                bool flag = false;
                bool flag2 = false;
                ListControlEntryDefinition lved = new ListControlEntryDefinition();
                foreach (System.Xml.XmlNode node in listViewEntryNode.ChildNodes)
                {
                    if (base.MatchNodeName(node, "EntrySelectedBy"))
                    {
                        if (flag)
                        {
                            base.ProcessDuplicateNode(node);
                            return null;
                        }
                        flag = true;
                        lved.appliesTo = this.LoadAppliesToSection(node, true);
                    }
                    else if (base.MatchNodeName(node, "ListItems"))
                    {
                        if (flag2)
                        {
                            base.ProcessDuplicateNode(node);
                            return null;
                        }
                        flag2 = true;
                        this.LoadListControlItemDefinitions(lved, node);
                    }
                    else
                    {
                        base.ProcessUnknownNode(node);
                    }
                }
                if (lved.itemDefinitionList == null)
                {
                    base.ReportError(StringUtil.Format(FormatAndOutXmlLoadingStrings.NoDefinitionList, base.ComputeCurrentXPath(), base.FilePath));
                    return null;
                }
                return lved;
            }
        }

        private ListControlEntryDefinition LoadListControlEntryDefinitionFromObjectModel(ListControlEntry listEntry, int viewIndex, string typeName)
        {
            ListControlEntryDefinition lved = new ListControlEntryDefinition();
            if (listEntry.SelectedBy.Count > 0)
            {
                lved.appliesTo = this.LoadAppliesToSectionFromObjectModel(listEntry.SelectedBy);
            }
            this.LoadListControlItemDefinitionsFromObjectModel(lved, listEntry.Items, viewIndex, typeName);
            if (lved.itemDefinitionList == null)
            {
                return null;
            }
            return lved;
        }

        private ListControlBody LoadListControlFromObjectModel(ListControl list, int viewIndex, string typeName)
        {
            ListControlBody listBody = new ListControlBody();
            this.LoadListControlEntriesFromObjectModel(listBody, list.Entries, viewIndex, typeName);
            if (listBody.defaultEntryDefinition == null)
            {
                return null;
            }
            return listBody;
        }

        private ListControlItemDefinition LoadListControlItemDefinition(System.Xml.XmlNode propertyEntryNode)
        {
            using (base.StackFrame(propertyEntryNode))
            {
                ViewEntryNodeMatch match = new ViewEntryNodeMatch(this);
                List<System.Xml.XmlNode> unprocessedNodes = new List<System.Xml.XmlNode>();
                if (!match.ProcessExpressionDirectives(propertyEntryNode, unprocessedNodes))
                {
                    return null;
                }
                TextToken token = null;
                ExpressionToken token2 = null;
                bool flag = false;
                bool flag2 = false;
                foreach (System.Xml.XmlNode node in unprocessedNodes)
                {
                    if (base.MatchNodeName(node, "ItemSelectionCondition"))
                    {
                        if (flag2)
                        {
                            base.ProcessDuplicateNode(node);
                            return null;
                        }
                        flag2 = true;
                        token2 = this.LoadItemSelectionCondition(node);
                        if (token2 == null)
                        {
                            return null;
                        }
                    }
                    else if (base.MatchNodeNameWithAttributes(node, "Label"))
                    {
                        if (flag)
                        {
                            base.ProcessDuplicateNode(node);
                            return null;
                        }
                        flag = true;
                        token = this.LoadLabel(node);
                        if (token == null)
                        {
                            return null;
                        }
                    }
                    else
                    {
                        base.ProcessUnknownNode(node);
                    }
                }
                ListControlItemDefinition definition = new ListControlItemDefinition {
                    label = token,
                    conditionToken = token2
                };
                if (match.TextToken != null)
                {
                    definition.formatTokenList.Add(match.TextToken);
                }
                else
                {
                    FieldPropertyToken item = new FieldPropertyToken {
                        expression = match.Expression
                    };
                    item.fieldFormattingDirective.formatString = match.FormatString;
                    definition.formatTokenList.Add(item);
                }
                return definition;
            }
        }

        private void LoadListControlItemDefinitions(ListControlEntryDefinition lved, System.Xml.XmlNode bodyNode)
        {
            using (base.StackFrame(bodyNode))
            {
                int num = 0;
                foreach (System.Xml.XmlNode node in bodyNode.ChildNodes)
                {
                    if (base.MatchNodeName(node, "ListItem"))
                    {
                        num++;
                        ListControlItemDefinition item = this.LoadListControlItemDefinition(node);
                        if (item == null)
                        {
                            base.ReportError(StringUtil.Format(FormatAndOutXmlLoadingStrings.InvalidPropertyEntry, base.ComputeCurrentXPath(), base.FilePath));
                            lved.itemDefinitionList = null;
                            return;
                        }
                        lved.itemDefinitionList.Add(item);
                    }
                    else
                    {
                        base.ProcessUnknownNode(node);
                    }
                }
                if (lved.itemDefinitionList.Count == 0)
                {
                    base.ReportError(StringUtil.Format(FormatAndOutXmlLoadingStrings.NoListViewItem, base.ComputeCurrentXPath(), base.FilePath));
                    lved.itemDefinitionList = null;
                }
            }
        }

        private void LoadListControlItemDefinitionsFromObjectModel(ListControlEntryDefinition lved, List<ListControlEntryItem> listItems, int viewIndex, string typeName)
        {
            foreach (ListControlEntryItem item in listItems)
            {
                ListControlItemDefinition definition = new ListControlItemDefinition();
                if (item.DisplayEntry != null)
                {
                    ExpressionToken token = this.LoadExpressionFromObjectModel(item.DisplayEntry, viewIndex, typeName);
                    if (token == null)
                    {
                        lved.itemDefinitionList = null;
                        return;
                    }
                    FieldPropertyToken token2 = new FieldPropertyToken {
                        expression = token
                    };
                    definition.formatTokenList.Add(token2);
                }
                if (!string.IsNullOrEmpty(item.Label))
                {
                    TextToken token3 = new TextToken {
                        text = item.Label
                    };
                    definition.label = token3;
                }
                lved.itemDefinitionList.Add(definition);
            }
            if (lved.itemDefinitionList.Count == 0)
            {
                base.ReportErrorForLoadingFromObjectModel(StringUtil.Format(FormatAndOutXmlLoadingStrings.NoListViewItemInFormattingData, typeName, viewIndex), typeName);
                lved.itemDefinitionList = null;
            }
        }

        private bool LoadMainControlDependentData(List<System.Xml.XmlNode> unprocessedNodes, ViewDefinition view)
        {
            foreach (System.Xml.XmlNode node in unprocessedNodes)
            {
                bool flag = false;
                bool flag2 = false;
                if (base.MatchNodeName(node, "OutOfBand"))
                {
                    if (flag)
                    {
                        base.ProcessDuplicateNode(node);
                        return false;
                    }
                    flag = true;
                    if (!this.ReadBooleanNode(node, out view.outOfBand))
                    {
                        return false;
                    }
                    if (!(view.mainControl is ComplexControlBody) && !(view.mainControl is ListControlBody))
                    {
                        base.ReportError(StringUtil.Format(FormatAndOutXmlLoadingStrings.InvalidControlForOutOfBandView, base.ComputeCurrentXPath(), base.FilePath));
                        return false;
                    }
                }
                else if (base.MatchNodeName(node, "Controls"))
                {
                    if (flag2)
                    {
                        base.ProcessDuplicateNode(node);
                        return false;
                    }
                    flag2 = true;
                    this.LoadControlDefinitions(node, view.formatControlDefinitionHolder.controlDefinitionList);
                }
                else
                {
                    base.ProcessUnknownNode(node);
                }
            }
            return true;
        }

        private NewLineToken LoadNewLine(System.Xml.XmlNode newLineNode, int index)
        {
            using (base.StackFrame(newLineNode, index))
            {
                if (!base.VerifyNodeHasNoChildren(newLineNode))
                {
                    return null;
                }
                return new NewLineToken();
            }
        }

        private int LoadPositiveOrZeroIntegerValue(System.Xml.XmlNode node, out bool success)
        {
            int num = this.LoadIntegerValue(node, out success);
            if (!success)
            {
                return num;
            }
            using (base.StackFrame(node))
            {
                if (num < 0)
                {
                    base.ReportError(StringUtil.Format(FormatAndOutXmlLoadingStrings.ExpectNaturalNumber, base.ComputeCurrentXPath(), base.FilePath));
                    success = false;
                }
                return num;
            }
        }

        private bool LoadPropertyBaseHelper(System.Xml.XmlNode propertyBaseNode, PropertyTokenBase ptb, List<System.Xml.XmlNode> unprocessedNodes)
        {
            ExpressionNodeMatch match = new ExpressionNodeMatch(this);
            bool flag = false;
            bool flag2 = false;
            bool flag3 = false;
            ExpressionToken token = null;
            foreach (System.Xml.XmlNode node in propertyBaseNode.ChildNodes)
            {
                if (match.MatchNode(node))
                {
                    if (flag)
                    {
                        base.ProcessDuplicateNode(node);
                        return false;
                    }
                    flag = true;
                    if (!match.ProcessNode(node))
                    {
                        return false;
                    }
                }
                else if (base.MatchNodeName(node, "EnumerateCollection"))
                {
                    if (flag2)
                    {
                        base.ProcessDuplicateNode(node);
                        return false;
                    }
                    flag2 = true;
                    if (!this.ReadBooleanNode(node, out ptb.enumerateCollection))
                    {
                        return false;
                    }
                }
                else if (base.MatchNodeName(node, "ItemSelectionCondition"))
                {
                    if (flag3)
                    {
                        base.ProcessDuplicateNode(node);
                        return false;
                    }
                    flag3 = true;
                    token = this.LoadItemSelectionCondition(node);
                    if (token == null)
                    {
                        return false;
                    }
                }
                else if (!XmlLoaderBase.IsFilteredOutNode(node))
                {
                    unprocessedNodes.Add(node);
                }
            }
            if (flag)
            {
                ExpressionToken token2 = match.GenerateExpressionToken();
                if (token2 == null)
                {
                    return false;
                }
                ptb.expression = token2;
                ptb.conditionToken = token;
            }
            return true;
        }

        private List<FormatToken> LoadPropertyEntry(System.Xml.XmlNode propertyEntryNode)
        {
            using (base.StackFrame(propertyEntryNode))
            {
                ViewEntryNodeMatch match = new ViewEntryNodeMatch(this);
                List<System.Xml.XmlNode> unprocessedNodes = new List<System.Xml.XmlNode>();
                if (!match.ProcessExpressionDirectives(propertyEntryNode, unprocessedNodes))
                {
                    return null;
                }
                foreach (System.Xml.XmlNode node in unprocessedNodes)
                {
                    base.ProcessUnknownNode(node);
                }
                List<FormatToken> list2 = new List<FormatToken>();
                if (match.TextToken != null)
                {
                    list2.Add(match.TextToken);
                }
                else
                {
                    FieldPropertyToken item = new FieldPropertyToken {
                        expression = match.Expression
                    };
                    item.fieldFormattingDirective.formatString = match.FormatString;
                    list2.Add(item);
                }
                return list2;
            }
        }

        private StringResourceReference LoadResourceAttributes(XmlAttributeCollection attributes)
        {
            StringResourceReference resourceReference = new StringResourceReference();
            foreach (System.Xml.XmlAttribute attribute in attributes)
            {
                if (base.MatchAttributeName(attribute, "AssemblyName"))
                {
                    resourceReference.assemblyName = base.GetMandatoryAttributeValue(attribute);
                    if (resourceReference.assemblyName == null)
                    {
                        return null;
                    }
                }
                else if (base.MatchAttributeName(attribute, "BaseName"))
                {
                    resourceReference.baseName = base.GetMandatoryAttributeValue(attribute);
                    if (resourceReference.baseName == null)
                    {
                        return null;
                    }
                }
                else if (base.MatchAttributeName(attribute, "ResourceId"))
                {
                    resourceReference.resourceId = base.GetMandatoryAttributeValue(attribute);
                    if (resourceReference.resourceId == null)
                    {
                        return null;
                    }
                }
                else
                {
                    base.ProcessUnknownAttribute(attribute);
                    return null;
                }
            }
            if (resourceReference.assemblyName == null)
            {
                base.ReportMissingAttribute("AssemblyName");
                return null;
            }
            if (resourceReference.baseName == null)
            {
                base.ReportMissingAttribute("BaseName");
                return null;
            }
            if (resourceReference.resourceId == null)
            {
                base.ReportMissingAttribute("ResourceId");
                return null;
            }
            resourceReference.loadingInfo = base.LoadingInfo;
            if (base.VerifyStringResources)
            {
                DisplayResourceManagerCache.LoadingResult result;
                DisplayResourceManagerCache.AssemblyBindingStatus status;
                base.displayResourceManagerCache.VerifyResource(resourceReference, out result, out status);
                if (result != DisplayResourceManagerCache.LoadingResult.NoError)
                {
                    this.ReportStringResourceFailure(resourceReference, result, status);
                    return null;
                }
            }
            return resourceReference;
        }

        private void LoadRowEntriesSection(TableControlBody tableBody, System.Xml.XmlNode rowEntriesNode)
        {
            using (base.StackFrame(rowEntriesNode))
            {
                int num = 0;
                foreach (System.Xml.XmlNode node in rowEntriesNode.ChildNodes)
                {
                    if (base.MatchNodeName(node, "TableRowEntry"))
                    {
                        TableRowDefinition item = this.LoadRowEntryDefinition(node, num++);
                        if (item == null)
                        {
                            base.ReportError(StringUtil.Format(FormatAndOutXmlLoadingStrings.LoadTagFailed, new object[] { base.ComputeCurrentXPath(), base.FilePath, "TableRowEntry" }));
                            tableBody.defaultDefinition = null;
                            return;
                        }
                        if (item.appliesTo == null)
                        {
                            if (tableBody.defaultDefinition == null)
                            {
                                tableBody.defaultDefinition = item;
                                continue;
                            }
                            base.ReportError(StringUtil.Format(FormatAndOutXmlLoadingStrings.TooManyDefaultShapeEntry, new object[] { base.ComputeCurrentXPath(), base.FilePath, "TableRowEntry" }));
                            tableBody.defaultDefinition = null;
                            return;
                        }
                        tableBody.optionalDefinitionList.Add(item);
                    }
                    else
                    {
                        base.ProcessUnknownNode(node);
                    }
                }
                if (tableBody.defaultDefinition == null)
                {
                    base.ReportError(StringUtil.Format(FormatAndOutXmlLoadingStrings.NoDefaultShapeEntry, new object[] { base.ComputeCurrentXPath(), base.FilePath, "TableRowEntry" }));
                }
            }
        }

        private void LoadRowEntriesSectionFromObjectModel(TableControlBody tableBody, List<TableControlRow> rowEntries, int viewIndex, string typeName)
        {
            foreach (TableControlRow row in rowEntries)
            {
                TableRowDefinition trd = new TableRowDefinition();
                if (row.Columns.Count > 0)
                {
                    this.LoadColumnEntriesFromObjectModel(trd, row.Columns, viewIndex, typeName);
                    if (trd.rowItemDefinitionList == null)
                    {
                        tableBody.defaultDefinition = null;
                        return;
                    }
                }
                tableBody.defaultDefinition = trd;
            }
            if (tableBody.defaultDefinition == null)
            {
                base.ReportErrorForLoadingFromObjectModel(StringUtil.Format(FormatAndOutXmlLoadingStrings.NoDefaultShapeEntryInFormattingData, new object[] { typeName, viewIndex, "TableRowEntry" }), typeName);
            }
        }

        private TableRowDefinition LoadRowEntryDefinition(System.Xml.XmlNode rowEntryNode, int index)
        {
            using (base.StackFrame(rowEntryNode, index))
            {
                bool flag = false;
                bool flag2 = false;
                bool flag3 = false;
                TableRowDefinition trd = new TableRowDefinition();
                foreach (System.Xml.XmlNode node in rowEntryNode.ChildNodes)
                {
                    if (base.MatchNodeName(node, "EntrySelectedBy"))
                    {
                        if (flag)
                        {
                            base.ProcessDuplicateNode(node);
                            return null;
                        }
                        flag = true;
                        trd.appliesTo = this.LoadAppliesToSection(node, true);
                    }
                    else if (base.MatchNodeName(node, "TableColumnItems"))
                    {
                        if (flag2)
                        {
                            base.ProcessDuplicateNode(node);
                            return null;
                        }
                        this.LoadColumnEntries(node, trd);
                        if (trd.rowItemDefinitionList == null)
                        {
                            return null;
                        }
                    }
                    else if (base.MatchNodeName(node, "Wrap"))
                    {
                        if (flag3)
                        {
                            base.ProcessDuplicateNode(node);
                            return null;
                        }
                        flag3 = true;
                        if (!this.ReadBooleanNode(node, out trd.multiLine))
                        {
                            return null;
                        }
                    }
                    else
                    {
                        base.ProcessUnknownNode(node);
                    }
                }
                return trd;
            }
        }

        private TypeOrGroupReference LoadSelectionConditionNode(System.Xml.XmlNode selectionConditionNode)
        {
            using (base.StackFrame(selectionConditionNode))
            {
                TypeOrGroupReference reference = null;
                bool flag = false;
                bool flag2 = false;
                bool flag3 = false;
                ExpressionNodeMatch match = new ExpressionNodeMatch(this);
                foreach (System.Xml.XmlNode node in selectionConditionNode.ChildNodes)
                {
                    if (base.MatchNodeName(node, "SelectionSetName"))
                    {
                        if (flag3)
                        {
                            base.ProcessDuplicateAlternateNode(node, "SelectionSetName", "TypeName");
                            return null;
                        }
                        flag3 = true;
                        TypeGroupReference reference2 = this.LoadTypeGroupReference(node);
                        if (reference2 == null)
                        {
                            return null;
                        }
                        reference = reference2;
                    }
                    else if (base.MatchNodeName(node, "TypeName"))
                    {
                        if (flag2)
                        {
                            base.ProcessDuplicateAlternateNode(node, "SelectionSetName", "TypeName");
                            return null;
                        }
                        flag2 = true;
                        TypeReference reference3 = this.LoadTypeReference(node);
                        if (reference3 == null)
                        {
                            return null;
                        }
                        reference = reference3;
                    }
                    else if (match.MatchNode(node))
                    {
                        if (flag)
                        {
                            base.ProcessDuplicateNode(node);
                            return null;
                        }
                        flag = true;
                        if (!match.ProcessNode(node))
                        {
                            return null;
                        }
                    }
                    else
                    {
                        base.ProcessUnknownNode(node);
                    }
                }
                if (flag2 && flag3)
                {
                    base.ReportError(StringUtil.Format(FormatAndOutXmlLoadingStrings.SelectionSetNameAndTypeName, base.ComputeCurrentXPath(), base.FilePath));
                    return null;
                }
                if (reference == null)
                {
                    base.ReportMissingNodes(new string[] { "SelectionSetName", "TypeName" });
                    return null;
                }
                if (flag)
                {
                    reference.conditionToken = match.GenerateExpressionToken();
                    if (reference.conditionToken == null)
                    {
                        return null;
                    }
                    return reference;
                }
                base.ReportError(StringUtil.Format(FormatAndOutXmlLoadingStrings.ExpectExpression, base.ComputeCurrentXPath(), base.FilePath));
                return null;
            }
        }

        private bool LoadStringResourceReference(System.Xml.XmlNode n, out StringResourceReference resource)
        {
            resource = null;
            XmlElement element = n as XmlElement;
            if (element == null)
            {
                base.ReportError(StringUtil.Format(FormatAndOutXmlLoadingStrings.NonXmlElementNode, base.ComputeCurrentXPath(), base.FilePath));
                return false;
            }
            if (element.Attributes.Count <= 0)
            {
                return true;
            }
            resource = this.LoadResourceAttributes(element.Attributes);
            return (resource != null);
        }

        private ControlBase LoadTableControl(System.Xml.XmlNode controlNode)
        {
            using (base.StackFrame(controlNode))
            {
                TableControlBody tableBody = new TableControlBody();
                bool flag = false;
                bool flag2 = false;
                bool flag3 = false;
                bool flag4 = false;
                foreach (System.Xml.XmlNode node in controlNode.ChildNodes)
                {
                    if (base.MatchNodeName(node, "HideTableHeaders"))
                    {
                        if (flag3)
                        {
                            base.ProcessDuplicateNode(node);
                            return null;
                        }
                        flag3 = true;
                        if (!this.ReadBooleanNode(node, out tableBody.header.hideHeader))
                        {
                            return null;
                        }
                    }
                    else if (base.MatchNodeName(node, "AutoSize"))
                    {
                        bool flag5;
                        if (flag4)
                        {
                            base.ProcessDuplicateNode(node);
                            return null;
                        }
                        flag4 = true;
                        if (!this.ReadBooleanNode(node, out flag5))
                        {
                            return null;
                        }
                        tableBody.autosize = new bool?(flag5);
                    }
                    else if (base.MatchNodeName(node, "TableHeaders"))
                    {
                        if (flag)
                        {
                            base.ProcessDuplicateNode(node);
                            return null;
                        }
                        flag = true;
                        this.LoadHeadersSection(tableBody, node);
                        if (tableBody.header.columnHeaderDefinitionList == null)
                        {
                            return null;
                        }
                    }
                    else if (base.MatchNodeName(node, "TableRowEntries"))
                    {
                        if (flag2)
                        {
                            base.ProcessDuplicateNode(node);
                            return null;
                        }
                        flag2 = true;
                        this.LoadRowEntriesSection(tableBody, node);
                        if (tableBody.defaultDefinition == null)
                        {
                            return null;
                        }
                    }
                    else
                    {
                        base.ProcessUnknownNode(node);
                    }
                }
                if (!flag2)
                {
                    base.ReportMissingNode("TableRowEntries");
                    return null;
                }
                if ((tableBody.header.columnHeaderDefinitionList.Count != 0) && (tableBody.header.columnHeaderDefinitionList.Count != tableBody.defaultDefinition.rowItemDefinitionList.Count))
                {
                    base.ReportError(StringUtil.Format(FormatAndOutXmlLoadingStrings.IncorrectHeaderItemCount, new object[] { base.ComputeCurrentXPath(), base.FilePath, tableBody.header.columnHeaderDefinitionList.Count, tableBody.defaultDefinition.rowItemDefinitionList.Count }));
                    return null;
                }
                if (tableBody.optionalDefinitionList.Count != 0)
                {
                    int num = 0;
                    foreach (TableRowDefinition definition in tableBody.optionalDefinitionList)
                    {
                        if (definition.rowItemDefinitionList.Count != tableBody.defaultDefinition.rowItemDefinitionList.Count)
                        {
                            base.ReportError(StringUtil.Format(FormatAndOutXmlLoadingStrings.IncorrectRowItemCount, new object[] { base.ComputeCurrentXPath(), base.FilePath, definition.rowItemDefinitionList.Count, tableBody.defaultDefinition.rowItemDefinitionList.Count, num + 1 }));
                            return null;
                        }
                        num++;
                    }
                }
                return tableBody;
            }
        }

        private ControlBase LoadTableControlFromObjectModel(TableControl table, int viewIndex, string typeName)
        {
            TableControlBody tableBody = new TableControlBody();
            this.LoadHeadersSectionFromObjectModel(tableBody, table.Headers);
            if (table.Rows.Count > 1)
            {
                base.ReportErrorForLoadingFromObjectModel(StringUtil.Format(FormatAndOutXmlLoadingStrings.MultipleRowEntriesFoundInFormattingData, new object[] { typeName, viewIndex, "TableRowEntry" }), typeName);
                return null;
            }
            this.LoadRowEntriesSectionFromObjectModel(tableBody, table.Rows, viewIndex, typeName);
            if (tableBody.defaultDefinition == null)
            {
                return null;
            }
            if ((tableBody.header.columnHeaderDefinitionList.Count != 0) && (tableBody.header.columnHeaderDefinitionList.Count != tableBody.defaultDefinition.rowItemDefinitionList.Count))
            {
                base.ReportErrorForLoadingFromObjectModel(StringUtil.Format(FormatAndOutXmlLoadingStrings.IncorrectHeaderItemCountInFormattingData, new object[] { typeName, viewIndex, tableBody.header.columnHeaderDefinitionList.Count, tableBody.defaultDefinition.rowItemDefinitionList.Count }), typeName);
                return null;
            }
            return tableBody;
        }

        internal TextToken LoadText(System.Xml.XmlNode textNode)
        {
            using (base.StackFrame(textNode))
            {
                return this.LoadTextToken(textNode);
            }
        }

        private TextToken LoadText(System.Xml.XmlNode textNode, int index)
        {
            using (base.StackFrame(textNode, index))
            {
                return this.LoadTextToken(textNode);
            }
        }

        private TextToken LoadTextToken(System.Xml.XmlNode n)
        {
            TextToken token = new TextToken();
            if (!this.LoadStringResourceReference(n, out token.resource))
            {
                return null;
            }
            if (token.resource != null)
            {
                token.text = n.InnerText;
                return token;
            }
            token.text = base.GetMandatoryInnerText(n);
            if (token.text == null)
            {
                return null;
            }
            return token;
        }

        private void LoadTypeGroup(TypeInfoDataBase db, System.Xml.XmlNode typeGroupNode, int index)
        {
            using (base.StackFrame(typeGroupNode, index))
            {
                TypeGroupDefinition typeGroupDefinition = new TypeGroupDefinition();
                bool flag = false;
                foreach (System.Xml.XmlNode node in typeGroupNode.ChildNodes)
                {
                    if (base.MatchNodeName(node, "Name"))
                    {
                        if (flag)
                        {
                            base.ProcessDuplicateNode(node);
                        }
                        else
                        {
                            flag = true;
                            typeGroupDefinition.name = base.GetMandatoryInnerText(node);
                        }
                    }
                    else if (base.MatchNodeName(node, "Types"))
                    {
                        this.LoadTypeGroupTypeRefs(node, typeGroupDefinition);
                    }
                    else
                    {
                        base.ProcessUnknownNode(node);
                    }
                }
                if (!flag)
                {
                    base.ReportMissingNode("Name");
                }
                db.typeGroupSection.typeGroupDefinitionList.Add(typeGroupDefinition);
            }
        }

        private TypeGroupReference LoadTypeGroupReference(System.Xml.XmlNode n)
        {
            string mandatoryInnerText = base.GetMandatoryInnerText(n);
            if (mandatoryInnerText != null)
            {
                return new TypeGroupReference { name = mandatoryInnerText };
            }
            return null;
        }

        private void LoadTypeGroups(TypeInfoDataBase db, System.Xml.XmlNode typeGroupsNode)
        {
            using (base.StackFrame(typeGroupsNode))
            {
                int num = 0;
                foreach (System.Xml.XmlNode node in typeGroupsNode.ChildNodes)
                {
                    if (base.MatchNodeName(node, "SelectionSet"))
                    {
                        this.LoadTypeGroup(db, node, num++);
                    }
                    else
                    {
                        base.ProcessUnknownNode(node);
                    }
                }
            }
        }

        private void LoadTypeGroupTypeRefs(System.Xml.XmlNode typesNode, TypeGroupDefinition typeGroupDefinition)
        {
            using (base.StackFrame(typesNode))
            {
                int num = 0;
                foreach (System.Xml.XmlNode node in typesNode.ChildNodes)
                {
                    if (base.MatchNodeName(node, "TypeName"))
                    {
                        using (base.StackFrame(node, num++))
                        {
                            TypeReference item = new TypeReference {
                                name = base.GetMandatoryInnerText(node)
                            };
                            typeGroupDefinition.typeReferenceList.Add(item);
                            continue;
                        }
                    }
                    base.ProcessUnknownNode(node);
                }
            }
        }

        private TypeReference LoadTypeReference(System.Xml.XmlNode n)
        {
            string mandatoryInnerText = base.GetMandatoryInnerText(n);
            if (mandatoryInnerText != null)
            {
                return new TypeReference { name = mandatoryInnerText };
            }
            return null;
        }

        private ViewDefinition LoadView(System.Xml.XmlNode viewNode, int index)
        {
            using (base.StackFrame(viewNode, index))
            {
                ViewDefinition view = new ViewDefinition();
                List<System.Xml.XmlNode> unprocessedNodes = new List<System.Xml.XmlNode>();
                if (!this.LoadCommonViewData(viewNode, view, unprocessedNodes))
                {
                    base.ReportError(StringUtil.Format(FormatAndOutXmlLoadingStrings.ViewNotLoaded, base.ComputeCurrentXPath(), base.FilePath));
                    return null;
                }
                string[] names = new string[] { "TableControl", "ListControl", "WideControl", "CustomControl" };
                List<System.Xml.XmlNode> list2 = new List<System.Xml.XmlNode>();
                bool flag2 = false;
                foreach (System.Xml.XmlNode node in unprocessedNodes)
                {
                    if (base.MatchNodeName(node, "TableControl"))
                    {
                        if (flag2)
                        {
                            base.ProcessDuplicateNode(node);
                            return null;
                        }
                        flag2 = true;
                        view.mainControl = this.LoadTableControl(node);
                    }
                    else if (base.MatchNodeName(node, "ListControl"))
                    {
                        if (flag2)
                        {
                            base.ProcessDuplicateNode(node);
                            return null;
                        }
                        flag2 = true;
                        view.mainControl = this.LoadListControl(node);
                    }
                    else if (base.MatchNodeName(node, "WideControl"))
                    {
                        if (flag2)
                        {
                            base.ProcessDuplicateNode(node);
                            return null;
                        }
                        flag2 = true;
                        view.mainControl = this.LoadWideControl(node);
                    }
                    else if (base.MatchNodeName(node, "CustomControl"))
                    {
                        if (flag2)
                        {
                            base.ProcessDuplicateNode(node);
                            return null;
                        }
                        flag2 = true;
                        view.mainControl = this.LoadComplexControl(node);
                    }
                    else
                    {
                        list2.Add(node);
                    }
                }
                if (view.mainControl == null)
                {
                    base.ReportMissingNodes(names);
                    return null;
                }
                if (!this.LoadMainControlDependentData(list2, view))
                {
                    return null;
                }
                if (view.outOfBand && (view.groupBy != null))
                {
                    base.ReportError(StringUtil.Format(FormatAndOutXmlLoadingStrings.OutOfBandGroupByConflict, base.ComputeCurrentXPath(), base.FilePath));
                    return null;
                }
                return view;
            }
        }

        private void LoadViewDefinitions(TypeInfoDataBase db, System.Xml.XmlNode viewDefinitionsNode)
        {
            using (base.StackFrame(viewDefinitionsNode))
            {
                int num = 0;
                foreach (System.Xml.XmlNode node in viewDefinitionsNode.ChildNodes)
                {
                    if (base.MatchNodeName(node, "View"))
                    {
                        ViewDefinition item = this.LoadView(node, num++);
                        if (item != null)
                        {
                            base.ReportTrace(string.Format(CultureInfo.InvariantCulture, "{0} view {1} is loaded from file {2}", new object[] { ControlBase.GetControlShapeName(item.mainControl), item.name, item.loadingInfo.filePath }));
                            db.viewDefinitionsSection.viewDefinitionList.Add(item);
                        }
                    }
                    else
                    {
                        base.ProcessUnknownNode(node);
                    }
                }
            }
        }

        private ViewDefinition LoadViewFromObjectModle(string typeName, FormatViewDefinition formatView, int viewIndex)
        {
            TypeReference reference = new TypeReference {
                name = typeName
            };
            AppliesTo to = new AppliesTo {
                referenceList = { reference }
            };
            ViewDefinition definition = new ViewDefinition {
                appliesTo = to,
                name = formatView.Name
            };
            PSControl control = formatView.Control;
            if (control is TableControl)
            {
                TableControl table = control as TableControl;
                definition.mainControl = this.LoadTableControlFromObjectModel(table, viewIndex, typeName);
            }
            else if (control is ListControl)
            {
                ListControl list = control as ListControl;
                definition.mainControl = this.LoadListControlFromObjectModel(list, viewIndex, typeName);
            }
            else if (control is WideControl)
            {
                WideControl wide = control as WideControl;
                definition.mainControl = this.LoadWideControlFromObjectModel(wide, viewIndex, typeName);
            }
            if (definition.mainControl == null)
            {
                return null;
            }
            return definition;
        }

        private WideControlBody LoadWideControl(System.Xml.XmlNode controlNode)
        {
            using (base.StackFrame(controlNode))
            {
                WideControlBody wideBody = new WideControlBody();
                bool flag = false;
                bool flag2 = false;
                bool flag3 = false;
                foreach (System.Xml.XmlNode node in controlNode.ChildNodes)
                {
                    if (base.MatchNodeName(node, "AutoSize"))
                    {
                        bool flag4;
                        if (flag2)
                        {
                            base.ProcessDuplicateAlternateNode(node, "AutoSize", "ColumnNumber");
                            return null;
                        }
                        flag2 = true;
                        if (!this.ReadBooleanNode(node, out flag4))
                        {
                            return null;
                        }
                        wideBody.autosize = new bool?(flag4);
                    }
                    else if (base.MatchNodeName(node, "ColumnNumber"))
                    {
                        if (flag3)
                        {
                            base.ProcessDuplicateAlternateNode(node, "AutoSize", "ColumnNumber");
                            return null;
                        }
                        flag3 = true;
                        if (!this.ReadPositiveIntegerValue(node, out wideBody.columns))
                        {
                            return null;
                        }
                    }
                    else if (base.MatchNodeName(node, "WideEntries"))
                    {
                        if (!flag)
                        {
                            flag = true;
                            this.LoadWideControlEntries(node, wideBody);
                            if (wideBody.defaultEntryDefinition == null)
                            {
                                return null;
                            }
                        }
                        else
                        {
                            base.ProcessDuplicateNode(node);
                        }
                    }
                    else
                    {
                        base.ProcessUnknownNode(node);
                    }
                }
                if (flag2 && flag3)
                {
                    base.ProcessDuplicateAlternateNode("AutoSize", "ColumnNumber");
                    return null;
                }
                if (!flag)
                {
                    base.ReportMissingNode("WideEntries");
                    return null;
                }
                return wideBody;
            }
        }

        private void LoadWideControlEntries(System.Xml.XmlNode wideControlEntriesNode, WideControlBody wideBody)
        {
            using (base.StackFrame(wideControlEntriesNode))
            {
                int num = 0;
                foreach (System.Xml.XmlNode node in wideControlEntriesNode.ChildNodes)
                {
                    if (base.MatchNodeName(node, "WideEntry"))
                    {
                        WideControlEntryDefinition item = this.LoadWideControlEntry(node, num++);
                        if (item == null)
                        {
                            base.ReportError(StringUtil.Format(FormatAndOutXmlLoadingStrings.InvalidNode, new object[] { base.ComputeCurrentXPath(), base.FilePath, "WideEntry" }));
                            return;
                        }
                        if (item.appliesTo == null)
                        {
                            if (wideBody.defaultEntryDefinition == null)
                            {
                                wideBody.defaultEntryDefinition = item;
                                continue;
                            }
                            base.ReportError(StringUtil.Format(FormatAndOutXmlLoadingStrings.TooManyDefaultShapeEntry, new object[] { base.ComputeCurrentXPath(), base.FilePath, "WideEntry" }));
                            wideBody.defaultEntryDefinition = null;
                            return;
                        }
                        wideBody.optionalEntryList.Add(item);
                    }
                    else
                    {
                        base.ProcessUnknownNode(node);
                    }
                }
                if (wideBody.defaultEntryDefinition == null)
                {
                    base.ReportError(StringUtil.Format(FormatAndOutXmlLoadingStrings.NoDefaultShapeEntry, new object[] { base.ComputeCurrentXPath(), base.FilePath, "WideEntry" }));
                }
            }
        }

        private void LoadWideControlEntriesFromObjectModel(WideControlBody wideBody, List<WideControlEntryItem> wideEntries, int viewIndex, string typeName)
        {
            foreach (WideControlEntryItem item in wideEntries)
            {
                WideControlEntryDefinition definition = this.LoadWideControlEntryFromObjectModel(item, viewIndex, typeName);
                if (definition == null)
                {
                    base.ReportErrorForLoadingFromObjectModel(StringUtil.Format(FormatAndOutXmlLoadingStrings.InvalidFormattingData, new object[] { typeName, viewIndex, "WideEntry" }), typeName);
                    wideBody.defaultEntryDefinition = null;
                    return;
                }
                if (definition.appliesTo == null)
                {
                    if (wideBody.defaultEntryDefinition == null)
                    {
                        wideBody.defaultEntryDefinition = definition;
                        continue;
                    }
                    base.ReportErrorForLoadingFromObjectModel(StringUtil.Format(FormatAndOutXmlLoadingStrings.TooManyDefaultShapeEntryInFormattingData, new object[] { typeName, viewIndex, "WideEntry" }), typeName);
                    wideBody.defaultEntryDefinition = null;
                    return;
                }
                wideBody.optionalEntryList.Add(definition);
            }
            if (wideBody.defaultEntryDefinition == null)
            {
                base.ReportErrorForLoadingFromObjectModel(StringUtil.Format(FormatAndOutXmlLoadingStrings.NoDefaultShapeEntryInFormattingData, new object[] { typeName, viewIndex, "WideEntry" }), typeName);
            }
        }

        private WideControlEntryDefinition LoadWideControlEntry(System.Xml.XmlNode wideControlEntryNode, int index)
        {
            using (base.StackFrame(wideControlEntryNode, index))
            {
                bool flag = false;
                bool flag2 = false;
                WideControlEntryDefinition definition = new WideControlEntryDefinition();
                foreach (System.Xml.XmlNode node in wideControlEntryNode.ChildNodes)
                {
                    if (base.MatchNodeName(node, "EntrySelectedBy"))
                    {
                        if (flag)
                        {
                            base.ProcessDuplicateNode(node);
                            return null;
                        }
                        flag = true;
                        definition.appliesTo = this.LoadAppliesToSection(node, true);
                    }
                    else if (base.MatchNodeName(node, "WideItem"))
                    {
                        if (flag2)
                        {
                            base.ProcessDuplicateNode(node);
                            return null;
                        }
                        flag2 = true;
                        definition.formatTokenList = this.LoadPropertyEntry(node);
                        if (definition.formatTokenList == null)
                        {
                            base.ReportError(StringUtil.Format(FormatAndOutXmlLoadingStrings.InvalidNode, new object[] { base.ComputeCurrentXPath(), base.FilePath, "WideItem" }));
                            return null;
                        }
                    }
                    else
                    {
                        base.ProcessUnknownNode(node);
                    }
                }
                if (definition.formatTokenList.Count == 0)
                {
                    base.ReportMissingNode("WideItem");
                    return null;
                }
                return definition;
            }
        }

        private WideControlEntryDefinition LoadWideControlEntryFromObjectModel(WideControlEntryItem wideItem, int viewIndex, string typeName)
        {
            WideControlEntryDefinition definition = new WideControlEntryDefinition();
            if (wideItem.SelectedBy.Count > 0)
            {
                definition.appliesTo = this.LoadAppliesToSectionFromObjectModel(wideItem.SelectedBy);
            }
            ExpressionToken token = this.LoadExpressionFromObjectModel(wideItem.DisplayEntry, viewIndex, typeName);
            if (token == null)
            {
                return null;
            }
            FieldPropertyToken item = new FieldPropertyToken {
                expression = token
            };
            definition.formatTokenList.Add(item);
            return definition;
        }

        private WideControlBody LoadWideControlFromObjectModel(WideControl wide, int viewIndex, string typeName)
        {
            WideControlBody wideBody = new WideControlBody {
                columns = (int) wide.Columns
            };
            this.LoadWideControlEntriesFromObjectModel(wideBody, wide.Entries, viewIndex, typeName);
            if (wideBody.defaultEntryDefinition == null)
            {
                return null;
            }
            return wideBody;
        }

        internal bool LoadXmlFile(XmlFileLoadInfo info, TypeInfoDataBase db, MshExpressionFactory expressionFactory, AuthorizationManager authorizationManager, PSHost host, bool preValidated)
        {
            if (info == null)
            {
                throw PSTraceSource.NewArgumentNullException("info");
            }
            if (info.filePath == null)
            {
                throw PSTraceSource.NewArgumentNullException("info.filePath");
            }
            if (db == null)
            {
                throw PSTraceSource.NewArgumentNullException("db");
            }
            if (expressionFactory == null)
            {
                throw PSTraceSource.NewArgumentNullException("expressionFactory");
            }
            base.displayResourceManagerCache = db.displayResourceManagerCache;
            base.expressionFactory = expressionFactory;
            base.SetDatabaseLoadingInfo(info);
            base.ReportTrace("loading file started");
            XmlDocument doc = null;
            bool isFullyTrusted = false;
            doc = base.LoadXmlDocumentFromFileLoadingInfo(authorizationManager, host, out isFullyTrusted);
            if (SystemPolicy.GetSystemLockdownPolicy() == SystemEnforcementMode.Enforce)
            {
                base.SetLoadingInfoIsFullyTrusted(isFullyTrusted);
            }
            if (doc == null)
            {
                return false;
            }
            bool suppressValidation = this.suppressValidation;
            try
            {
                this.suppressValidation = preValidated;
                try
                {
                    this.LoadData(doc, db);
                }
                catch (TooManyErrorsException)
                {
                    return false;
                }
                catch (Exception exception)
                {
                    base.ReportError(StringUtil.Format(FormatAndOutXmlLoadingStrings.ErrorInFile, base.FilePath, exception.Message));
                    throw;
                }
                if (base.HasErrors)
                {
                    return false;
                }
            }
            finally
            {
                this.suppressValidation = suppressValidation;
            }
            base.ReportTrace("file loaded with no errors");
            return true;
        }

        private bool ReadBooleanNode(System.Xml.XmlNode collectionElement, out bool val)
        {
            val = false;
            if (base.VerifyNodeHasNoChildren(collectionElement))
            {
                string innerText = collectionElement.InnerText;
                if (string.IsNullOrEmpty(innerText))
                {
                    val = true;
                    return true;
                }
                if (string.Equals(innerText, "FALSE", StringComparison.OrdinalIgnoreCase))
                {
                    val = false;
                    return true;
                }
                if (string.Equals(innerText, "TRUE", StringComparison.OrdinalIgnoreCase))
                {
                    val = true;
                    return true;
                }
                base.ReportError(StringUtil.Format(FormatAndOutXmlLoadingStrings.ExpectBoolean, base.ComputeCurrentXPath(), base.FilePath));
            }
            return false;
        }

        private bool ReadPositiveIntegerValue(System.Xml.XmlNode n, out int val)
        {
            val = -1;
            string mandatoryInnerText = base.GetMandatoryInnerText(n);
            if (mandatoryInnerText != null)
            {
                if (int.TryParse(mandatoryInnerText, out val) && (val > 0))
                {
                    return true;
                }
                base.ReportError(StringUtil.Format(FormatAndOutXmlLoadingStrings.ExpectPositiveInteger, base.ComputeCurrentXPath(), base.FilePath));
            }
            return false;
        }

        private void ReportStringResourceFailure(StringResourceReference resource, DisplayResourceManagerCache.LoadingResult result, DisplayResourceManagerCache.AssemblyBindingStatus bindingStatus)
        {
            string assemblyName;
            switch (bindingStatus)
            {
                case DisplayResourceManagerCache.AssemblyBindingStatus.FoundInGac:
                    assemblyName = StringUtil.Format(FormatAndOutXmlLoadingStrings.AssemblyInGAC, resource.assemblyName);
                    break;

                case DisplayResourceManagerCache.AssemblyBindingStatus.FoundInPath:
                    assemblyName = Path.Combine(resource.loadingInfo.fileDirectory, resource.assemblyName);
                    break;

                default:
                    assemblyName = resource.assemblyName;
                    break;
            }
            string message = null;
            switch (result)
            {
                case DisplayResourceManagerCache.LoadingResult.AssemblyNotFound:
                    message = StringUtil.Format(FormatAndOutXmlLoadingStrings.AssemblyNotFound, new object[] { base.ComputeCurrentXPath(), base.FilePath, assemblyName });
                    break;

                case DisplayResourceManagerCache.LoadingResult.ResourceNotFound:
                    message = StringUtil.Format(FormatAndOutXmlLoadingStrings.ResourceNotFound, new object[] { base.ComputeCurrentXPath(), base.FilePath, resource.baseName, assemblyName });
                    break;

                case DisplayResourceManagerCache.LoadingResult.StringNotFound:
                    message = StringUtil.Format(FormatAndOutXmlLoadingStrings.StringResourceNotFound, new object[] { base.ComputeCurrentXPath(), base.FilePath, resource.resourceId, resource.baseName, assemblyName });
                    break;
            }
            base.ReportError(message);
        }

        internal bool VerifyScriptBlock(string scriptBlockText)
        {
            try
            {
                base.expressionFactory.VerifyScriptBlockText(scriptBlockText);
            }
            catch (ParseException exception)
            {
                base.ReportError(StringUtil.Format(FormatAndOutXmlLoadingStrings.InvalidScriptBlock, new object[] { base.ComputeCurrentXPath(), base.FilePath, exception.Message }));
                return false;
            }
            catch (Exception)
            {
                throw;
            }
            return true;
        }

        private sealed class ComplexControlMatch
        {
            private ControlBase _control;
            private TypeInfoDataBaseLoader _loader;

            internal ComplexControlMatch(TypeInfoDataBaseLoader loader)
            {
                this._loader = loader;
            }

            internal bool MatchNode(System.Xml.XmlNode n)
            {
                if (!this._loader.MatchNodeName(n, "CustomControl"))
                {
                    return this._loader.MatchNodeName(n, "CustomControlName");
                }
                return true;
            }

            internal bool ProcessNode(System.Xml.XmlNode n)
            {
                if (this._loader.MatchNodeName(n, "CustomControl"))
                {
                    this._control = this._loader.LoadComplexControl(n);
                    return true;
                }
                if (this._loader.MatchNodeName(n, "CustomControlName"))
                {
                    string mandatoryInnerText = this._loader.GetMandatoryInnerText(n);
                    if (mandatoryInnerText == null)
                    {
                        return false;
                    }
                    ControlReference reference = new ControlReference {
                        name = mandatoryInnerText,
                        controlType = typeof(ComplexControlBody)
                    };
                    this._control = reference;
                    return true;
                }
                PSTraceSource.NewInvalidOperationException();
                return false;
            }

            internal ControlBase Control
            {
                get
                {
                    return this._control;
                }
            }
        }

        private sealed class ExpressionNodeMatch
        {
            private bool _fatalError;
            private TypeInfoDataBaseLoader _loader;
            private ExpressionToken _token;

            internal ExpressionNodeMatch(TypeInfoDataBaseLoader loader)
            {
                this._loader = loader;
            }

            internal ExpressionToken GenerateExpressionToken()
            {
                if (this._fatalError)
                {
                    return null;
                }
                if (this._token == null)
                {
                    this._loader.ReportMissingNodes(new string[] { "PropertyName", "ScriptBlock" });
                    return null;
                }
                return this._token;
            }

            internal bool MatchNode(System.Xml.XmlNode n)
            {
                if (!this._loader.MatchNodeName(n, "PropertyName"))
                {
                    return this._loader.MatchNodeName(n, "ScriptBlock");
                }
                return true;
            }

            internal bool ProcessNode(System.Xml.XmlNode n)
            {
                if (this._loader.MatchNodeName(n, "PropertyName"))
                {
                    if (this._token != null)
                    {
                        if (this._token.isScriptBlock)
                        {
                            this._loader.ProcessDuplicateAlternateNode(n, "PropertyName", "ScriptBlock");
                        }
                        else
                        {
                            this._loader.ProcessDuplicateNode(n);
                        }
                        return false;
                    }
                    this._token = new ExpressionToken();
                    this._token.expressionValue = this._loader.GetMandatoryInnerText(n);
                    if (this._token.expressionValue == null)
                    {
                        this._loader.ReportError(StringUtil.Format(FormatAndOutXmlLoadingStrings.NoProperty, this._loader.ComputeCurrentXPath(), this._loader.FilePath));
                        this._fatalError = true;
                        return false;
                    }
                    return true;
                }
                if (this._loader.MatchNodeName(n, "ScriptBlock"))
                {
                    if (this._token != null)
                    {
                        if (!this._token.isScriptBlock)
                        {
                            this._loader.ProcessDuplicateAlternateNode(n, "PropertyName", "ScriptBlock");
                        }
                        else
                        {
                            this._loader.ProcessDuplicateNode(n);
                        }
                        return false;
                    }
                    this._token = new ExpressionToken();
                    this._token.isScriptBlock = true;
                    this._token.expressionValue = this._loader.GetMandatoryInnerText(n);
                    if (this._token.expressionValue == null)
                    {
                        this._loader.ReportError(StringUtil.Format(FormatAndOutXmlLoadingStrings.NoScriptBlockText, this._loader.ComputeCurrentXPath(), this._loader.FilePath));
                        this._fatalError = true;
                        return false;
                    }
                    if (!this._loader.suppressValidation && !this._loader.VerifyScriptBlock(this._token.expressionValue))
                    {
                        this._fatalError = true;
                        return false;
                    }
                    return true;
                }
                PSTraceSource.NewInvalidOperationException();
                return false;
            }
        }

        private sealed class ViewEntryNodeMatch
        {
            private ExpressionToken _expression;
            private string _formatString;
            private TypeInfoDataBaseLoader _loader;
            private Microsoft.PowerShell.Commands.Internal.Format.TextToken _textToken;

            internal ViewEntryNodeMatch(TypeInfoDataBaseLoader loader)
            {
                this._loader = loader;
            }

            internal bool ProcessExpressionDirectives(System.Xml.XmlNode containerNode, List<System.Xml.XmlNode> unprocessedNodes)
            {
                if (containerNode == null)
                {
                    throw PSTraceSource.NewArgumentNullException("containerNode");
                }
                string mandatoryInnerText = null;
                Microsoft.PowerShell.Commands.Internal.Format.TextToken token = null;
                TypeInfoDataBaseLoader.ExpressionNodeMatch match = new TypeInfoDataBaseLoader.ExpressionNodeMatch(this._loader);
                bool flag = false;
                bool flag2 = false;
                bool flag3 = false;
                foreach (System.Xml.XmlNode node in containerNode.ChildNodes)
                {
                    if (match.MatchNode(node))
                    {
                        if (flag2)
                        {
                            this._loader.ProcessDuplicateNode(node);
                            return false;
                        }
                        flag2 = true;
                        if (!match.ProcessNode(node))
                        {
                            return false;
                        }
                    }
                    else if (this._loader.MatchNodeName(node, "FormatString"))
                    {
                        if (flag)
                        {
                            this._loader.ProcessDuplicateNode(node);
                            return false;
                        }
                        flag = true;
                        mandatoryInnerText = this._loader.GetMandatoryInnerText(node);
                        if (mandatoryInnerText == null)
                        {
                            this._loader.ReportError(StringUtil.Format(FormatAndOutXmlLoadingStrings.NoFormatString, this._loader.ComputeCurrentXPath(), this._loader.FilePath));
                            return false;
                        }
                    }
                    else if (this._loader.MatchNodeNameWithAttributes(node, "Text"))
                    {
                        if (flag3)
                        {
                            this._loader.ProcessDuplicateNode(node);
                            return false;
                        }
                        flag3 = true;
                        token = this._loader.LoadText(node);
                        if (token == null)
                        {
                            this._loader.ReportError(StringUtil.Format(FormatAndOutXmlLoadingStrings.InvalidNode, new object[] { this._loader.ComputeCurrentXPath(), this._loader.FilePath, "Text" }));
                            return false;
                        }
                    }
                    else
                    {
                        unprocessedNodes.Add(node);
                    }
                }
                if (flag2)
                {
                    if (flag3)
                    {
                        this._loader.ReportError(StringUtil.Format(FormatAndOutXmlLoadingStrings.NodeWithExpression, new object[] { this._loader.ComputeCurrentXPath(), this._loader.FilePath, "Text" }));
                        return false;
                    }
                    ExpressionToken token2 = match.GenerateExpressionToken();
                    if (token2 == null)
                    {
                        return false;
                    }
                    if (!string.IsNullOrEmpty(mandatoryInnerText))
                    {
                        this._formatString = mandatoryInnerText;
                    }
                    this._expression = token2;
                }
                else
                {
                    if (flag)
                    {
                        this._loader.ReportError(StringUtil.Format(FormatAndOutXmlLoadingStrings.NodeWithoutExpression, new object[] { this._loader.ComputeCurrentXPath(), this._loader.FilePath, "FormatString" }));
                        return false;
                    }
                    if (flag3)
                    {
                        this._textToken = token;
                    }
                }
                return true;
            }

            internal ExpressionToken Expression
            {
                get
                {
                    return this._expression;
                }
            }

            internal string FormatString
            {
                get
                {
                    return this._formatString;
                }
            }

            internal Microsoft.PowerShell.Commands.Internal.Format.TextToken TextToken
            {
                get
                {
                    return this._textToken;
                }
            }
        }

        private static class XMLStringValues
        {
            internal const string AligmentCenter = "center";
            internal const string AligmentLeft = "left";
            internal const string AligmentRight = "right";
            internal const string False = "FALSE";
            internal const string True = "TRUE";
        }

        private static class XmlTags
        {
            internal const string AlignmentNode = "Alignment";
            internal const string AssemblyNameAttribute = "AssemblyName";
            internal const string AutoSizeNode = "AutoSize";
            internal const string BaseNameAttribute = "BaseName";
            internal const string ColumnNumberNode = "ColumnNumber";
            internal const string ComplexControlNameNode = "CustomControlName";
            internal const string ComplexControlNode = "CustomControl";
            internal const string ComplexEntriesNode = "CustomEntries";
            internal const string ComplexEntryNode = "CustomEntry";
            internal const string ComplexItemNode = "CustomItem";
            internal const string ConfigurationNode = "Configuration";
            internal const string ControlNode = "Control";
            internal const string ControlsNode = "Controls";
            internal const string DefaultSettingsNode = "DefaultSettings";
            internal const string EntrySelectedByNode = "EntrySelectedBy";
            internal const string EnumerableExpansionNode = "EnumerableExpansion";
            internal const string EnumerableExpansionsNode = "EnumerableExpansions";
            internal const string EnumerateCollectionNode = "EnumerateCollection";
            internal const string ExpandNode = "Expand";
            internal const string ExpressionBindingNode = "ExpressionBinding";
            internal const string FieldControlNode = "FieldControl";
            internal const string FirstLineHangingNode = "FirstLineHanging";
            internal const string FirstLineIndentNode = "FirstLineIndent";
            internal const string FormatStringNode = "FormatString";
            internal const string FrameNode = "Frame";
            internal const string GroupByNode = "GroupBy";
            internal const string HideTableHeadersNode = "HideTableHeaders";
            internal const string ItemSelectionConditionNode = "ItemSelectionCondition";
            internal const string LabelNode = "Label";
            internal const string LeftIndentNode = "LeftIndent";
            internal const string ListControlNode = "ListControl";
            internal const string ListEntriesNode = "ListEntries";
            internal const string ListEntryNode = "ListEntry";
            internal const string ListItemNode = "ListItem";
            internal const string ListItemsNode = "ListItems";
            internal const string MultiLineNode = "Wrap";
            internal const string MultilineTablesNode = "WrapTables";
            internal const string NameNode = "Name";
            internal const string NewLineNode = "NewLine";
            internal const string OutOfBandNode = "OutOfBand";
            internal const string PropertyCountForTableNode = "PropertyCountForTable";
            internal const string PropertyNameNode = "PropertyName";
            internal const string ResourceIdAttribute = "ResourceId";
            internal const string RightIndentNode = "RightIndent";
            internal const string ScriptBlockNode = "ScriptBlock";
            internal const string SelectionConditionNode = "SelectionCondition";
            internal const string SelectionSetNameNode = "SelectionSetName";
            internal const string SelectionSetNode = "SelectionSet";
            internal const string SelectionSetsNode = "SelectionSets";
            internal const string ShowErrorsAsMessagesNode = "ShowError";
            internal const string ShowErrorsInFormattedOutputNode = "DisplayError";
            internal const string TableColumnHeaderNode = "TableColumnHeader";
            internal const string TableColumnItemNode = "TableColumnItem";
            internal const string TableColumnItemsNode = "TableColumnItems";
            internal const string TableControlNode = "TableControl";
            internal const string TableHeadersNode = "TableHeaders";
            internal const string TableRowEntriesNode = "TableRowEntries";
            internal const string TableRowEntryNode = "TableRowEntry";
            internal const string TextNode = "Text";
            internal const string TypeNameNode = "TypeName";
            internal const string TypesNode = "Types";
            internal const string ViewDefinitionsNode = "ViewDefinitions";
            internal const string ViewNode = "View";
            internal const string ViewSelectedByNode = "ViewSelectedBy";
            internal const string WideControlNode = "WideControl";
            internal const string WideEntriesNode = "WideEntries";
            internal const string WideEntryNode = "WideEntry";
            internal const string WideItemNode = "WideItem";
            internal const string WidthNode = "Width";
        }
    }
}

