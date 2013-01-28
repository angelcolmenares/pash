namespace Microsoft.PowerShell.Commands.Internal.Format
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Management.Automation;

    internal static class AssociationManager
    {
        internal static List<MshResolvedExpressionParameterAssociation> ExpandAll(PSObject target)
        {
            List<string> propertyNamesFromView = GetPropertyNamesFromView(target, PSMemberViewTypes.Adapted);
            List<string> list2 = GetPropertyNamesFromView(target, PSMemberViewTypes.Base);
            List<string> collection = GetPropertyNamesFromView(target, PSMemberViewTypes.Extended);
            List<string> list4 = new List<string>();
            if (propertyNamesFromView.Count != 0)
            {
                list4 = propertyNamesFromView;
            }
            else
            {
                list4 = list2;
            }
            list4.AddRange(collection);
            Dictionary<string, object> dictionary = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            List<MshResolvedExpressionParameterAssociation> list5 = new List<MshResolvedExpressionParameterAssociation>();
            foreach (string str in list4)
            {
                if (!dictionary.ContainsKey(str))
                {
                    dictionary.Add(str, null);
                    MshExpression expression = new MshExpression(str, true);
                    list5.Add(new MshResolvedExpressionParameterAssociation(null, expression));
                }
            }
            return list5;
        }

        internal static List<MshResolvedExpressionParameterAssociation> ExpandDefaultPropertySet(PSObject target, MshExpressionFactory expressionFactory)
        {
            List<MshResolvedExpressionParameterAssociation> list = new List<MshResolvedExpressionParameterAssociation>();
            foreach (MshExpression expression in PSObjectHelper.GetDefaultPropertySet(target))
            {
                list.Add(new MshResolvedExpressionParameterAssociation(null, expression));
            }
            return list;
        }

        internal static List<MshResolvedExpressionParameterAssociation> ExpandParameters(List<MshParameter> parameters, PSObject target)
        {
            List<MshResolvedExpressionParameterAssociation> list = new List<MshResolvedExpressionParameterAssociation>();
            foreach (MshParameter parameter in parameters)
            {
                MshExpression entry = parameter.GetEntry("expression") as MshExpression;
                foreach (MshExpression expression2 in entry.ResolveNames(target))
                {
                    list.Add(new MshResolvedExpressionParameterAssociation(parameter, expression2));
                }
            }
            return list;
        }

        internal static List<MshResolvedExpressionParameterAssociation> ExpandTableParameters(List<MshParameter> parameters, PSObject target)
        {
            List<MshResolvedExpressionParameterAssociation> list = new List<MshResolvedExpressionParameterAssociation>();
            foreach (MshParameter parameter in parameters)
            {
                MshExpression entry = parameter.GetEntry("expression") as MshExpression;
                List<MshExpression> list2 = entry.ResolveNames(target);
                if (!entry.HasWildCardCharacters && (list2.Count == 0))
                {
                    list.Add(new MshResolvedExpressionParameterAssociation(parameter, entry));
                }
                foreach (MshExpression expression2 in list2)
                {
                    list.Add(new MshResolvedExpressionParameterAssociation(parameter, expression2));
                }
            }
            return list;
        }

        private static List<string> GetPropertyNamesFromView(PSObject source, PSMemberViewTypes viewType)
        {
            Collection<CollectionEntry<PSMemberInfo>> memberCollection = PSObject.GetMemberCollection(viewType);
            ReadOnlyPSMemberInfoCollection<PSMemberInfo> infos = new PSMemberInfoIntegratingCollection<PSMemberInfo>(source, memberCollection).Match("*", PSMemberTypes.Properties);
            List<string> list = new List<string>();
            foreach (PSMemberInfo info in infos)
            {
                list.Add(info.Name);
            }
            return list;
        }

        internal static void HandleComputerNameProperties(PSObject so, List<MshResolvedExpressionParameterAssociation> activeAssociationList)
        {
            if (so.Properties[RemotingConstants.ShowComputerNameNoteProperty] != null)
            {
                Collection<MshResolvedExpressionParameterAssociation> collection = new Collection<MshResolvedExpressionParameterAssociation>();
                foreach (MshResolvedExpressionParameterAssociation association in activeAssociationList)
                {
                    if (association.ResolvedExpression.ToString().Equals(RemotingConstants.ShowComputerNameNoteProperty, StringComparison.OrdinalIgnoreCase))
                    {
                        collection.Add(association);
                        break;
                    }
                }
                if ((so.Properties[RemotingConstants.ComputerNameNoteProperty] != null) && !PSObjectHelper.ShouldShowComputerNameProperty(so))
                {
                    foreach (MshResolvedExpressionParameterAssociation association2 in activeAssociationList)
                    {
                        if (association2.ResolvedExpression.ToString().Equals(RemotingConstants.ComputerNameNoteProperty, StringComparison.OrdinalIgnoreCase))
                        {
                            collection.Add(association2);
                            break;
                        }
                    }
                }
                if (collection.Count > 0)
                {
                    foreach (MshResolvedExpressionParameterAssociation association3 in collection)
                    {
                        activeAssociationList.Remove(association3);
                    }
                }
            }
        }

        internal static List<MshResolvedExpressionParameterAssociation> SetupActiveProperties(List<MshParameter> rawMshParameterList, PSObject target, MshExpressionFactory expressionFactory)
        {
            if ((rawMshParameterList != null) && (rawMshParameterList.Count > 0))
            {
                return ExpandParameters(rawMshParameterList, target);
            }
            List<MshResolvedExpressionParameterAssociation> activeAssociationList = ExpandDefaultPropertySet(target, expressionFactory);
            if (activeAssociationList.Count > 0)
            {
                if (PSObjectHelper.ShouldShowComputerNameProperty(target))
                {
                    activeAssociationList.Add(new MshResolvedExpressionParameterAssociation(null, new MshExpression(RemotingConstants.ComputerNameNoteProperty)));
                }
                return activeAssociationList;
            }
            activeAssociationList = ExpandAll(target);
            HandleComputerNameProperties(target, activeAssociationList);
            return activeAssociationList;
        }
    }
}

