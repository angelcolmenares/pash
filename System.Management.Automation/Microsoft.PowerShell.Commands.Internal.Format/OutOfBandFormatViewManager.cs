namespace Microsoft.PowerShell.Commands.Internal.Format
{
    using System;
    using System.Collections.Generic;
    using System.Management.Automation;
    using System.Management.Automation.Runspaces;
    using System.Runtime.InteropServices;

    internal static class OutOfBandFormatViewManager
    {
        internal static FormatEntryData GenerateOutOfBandData(TerminatingErrorContext errorContext, MshExpressionFactory expressionFactory, TypeInfoDataBase db, PSObject so, int enumerationLimit, bool useToStringFallback, out List<ErrorRecord> errors)
        {
            ViewGenerator generator;
            errors = null;
            ConsolidatedString internalTypeNames = so.InternalTypeNames;
            ViewDefinition view = DisplayDataQuery.GetOutOfBandView(expressionFactory, db, internalTypeNames);
            if (view != null)
            {
                if (view.mainControl is ComplexControlBody)
                {
                    generator = new ComplexViewGenerator();
                }
                else
                {
                    generator = new ListViewGenerator();
                }
                generator.Initialize(errorContext, expressionFactory, db, view, null);
            }
            else
            {
                if (DefaultScalarTypes.IsTypeInList(internalTypeNames) || IsPropertyLessObject(so))
                {
                    return GenerateOutOfBandObjectAsToString(so);
                }
                if (!useToStringFallback)
                {
                    return null;
                }
                if (new MshExpression("*").ResolveNames(so).Count <= 0)
                {
                    return null;
                }
                generator = new ListViewGenerator();
                generator.Initialize(errorContext, expressionFactory, so, db, null);
            }
            FormatEntryData data = generator.GeneratePayload(so, enumerationLimit);
            data.outOfBand = true;
            data.SetStreamTypeFromPSObject(so);
            errors = generator.ErrorManager.DrainFailedResultList();
            return data;
        }

        internal static FormatEntryData GenerateOutOfBandObjectAsToString(PSObject so)
        {
            FormatEntryData data = new FormatEntryData {
                outOfBand = true
            };
            RawTextFormatEntry entry = new RawTextFormatEntry {
                text = so.ToString()
            };
            data.formatEntryInfo = entry;
            return data;
        }

        internal static bool IsPropertyLessObject(PSObject so)
        {
            List<MshResolvedExpressionParameterAssociation> list = AssociationManager.ExpandAll(so);
            if (list.Count != 0)
            {
                if (list.Count == 3)
                {
                    foreach (MshResolvedExpressionParameterAssociation association in list)
                    {
                        if ((!association.ResolvedExpression.ToString().Equals(RemotingConstants.ComputerNameNoteProperty, StringComparison.OrdinalIgnoreCase) && !association.ResolvedExpression.ToString().Equals(RemotingConstants.ShowComputerNameNoteProperty, StringComparison.OrdinalIgnoreCase)) && !association.ResolvedExpression.ToString().Equals(RemotingConstants.RunspaceIdNoteProperty, StringComparison.OrdinalIgnoreCase))
                        {
                            return false;
                        }
                    }
                    return true;
                }
                if (list.Count == 4)
                {
                    foreach (MshResolvedExpressionParameterAssociation association2 in list)
                    {
                        if ((!association2.ResolvedExpression.ToString().Equals(RemotingConstants.ComputerNameNoteProperty, StringComparison.OrdinalIgnoreCase) && !association2.ResolvedExpression.ToString().Equals(RemotingConstants.ShowComputerNameNoteProperty, StringComparison.OrdinalIgnoreCase)) && (!association2.ResolvedExpression.ToString().Equals(RemotingConstants.RunspaceIdNoteProperty, StringComparison.OrdinalIgnoreCase) && !association2.ResolvedExpression.ToString().Equals(RemotingConstants.SourceJobInstanceId, StringComparison.OrdinalIgnoreCase)))
                        {
                            return false;
                        }
                    }
                    return true;
                }
                if (list.Count != 5)
                {
                    return false;
                }
                foreach (MshResolvedExpressionParameterAssociation association3 in list)
                {
                    if (((!association3.ResolvedExpression.ToString().Equals(RemotingConstants.ComputerNameNoteProperty, StringComparison.OrdinalIgnoreCase) && !association3.ResolvedExpression.ToString().Equals(RemotingConstants.ShowComputerNameNoteProperty, StringComparison.OrdinalIgnoreCase)) && (!association3.ResolvedExpression.ToString().Equals(RemotingConstants.RunspaceIdNoteProperty, StringComparison.OrdinalIgnoreCase) && !association3.ResolvedExpression.ToString().Equals(RemotingConstants.SourceJobInstanceId, StringComparison.OrdinalIgnoreCase))) && !association3.ResolvedExpression.ToString().Equals(RemotingConstants.SourceLength, StringComparison.OrdinalIgnoreCase))
                    {
                        return false;
                    }
                }
            }
            return true;
        }
    }
}

