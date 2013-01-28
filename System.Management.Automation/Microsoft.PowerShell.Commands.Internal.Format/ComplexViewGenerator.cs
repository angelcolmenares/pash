namespace Microsoft.PowerShell.Commands.Internal.Format
{
    using System;
    using System.Management.Automation;

    internal sealed class ComplexViewGenerator : ViewGenerator
    {
        private ComplexViewEntry GenerateComplexViewEntryFromDataBaseInfo(PSObject so, int enumerationLimit)
        {
            ComplexViewEntry entry = new ComplexViewEntry();
            new ComplexControlGenerator(base.dataBaseInfo.db, base.dataBaseInfo.view.loadingInfo, base.expressionFactory, base.dataBaseInfo.view.formatControlDefinitionHolder.controlDefinitionList, base.ErrorManager, enumerationLimit, base.errorContext).GenerateFormatEntries(50, base.dataBaseInfo.view.mainControl, so, entry.formatValueList);
            return entry;
        }

        private ComplexViewEntry GenerateComplexViewEntryFromProperties(PSObject so, int enumerationLimit)
        {
            ComplexViewObjectBrowser browser = new ComplexViewObjectBrowser(base.ErrorManager, base.expressionFactory, enumerationLimit);
            return browser.GenerateView(so, base.inputParameters);
        }

        internal override FormatEntryData GeneratePayload(PSObject so, int enumerationLimit)
        {
            FormatEntryData data = new FormatEntryData();
            if (base.dataBaseInfo.view != null)
            {
                data.formatEntryInfo = this.GenerateComplexViewEntryFromDataBaseInfo(so, enumerationLimit);
                return data;
            }
            data.formatEntryInfo = this.GenerateComplexViewEntryFromProperties(so, enumerationLimit);
            return data;
        }

        internal override FormatStartData GenerateStartData(PSObject so)
        {
            FormatStartData data = base.GenerateStartData(so);
            data.shapeInfo = new ComplexViewHeaderInfo();
            return data;
        }

        internal override void Initialize(TerminatingErrorContext errorContext, MshExpressionFactory expressionFactory, PSObject so, TypeInfoDataBase db, FormattingCommandLineParameters parameters)
        {
            base.Initialize(errorContext, expressionFactory, so, db, parameters);
            base.inputParameters = parameters;
        }
    }
}

