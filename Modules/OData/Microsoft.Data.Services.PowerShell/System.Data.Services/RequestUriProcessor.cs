namespace System.Data.Services
{
    using System;
    using System.Collections.Generic;
    using System.Data.Services.Parsing;
    using System.Data.Services.Providers;
    using System.Data.Services.Serializers;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    internal static class RequestUriProcessor
    {
        private const int RecursionLimit = 100;

        internal static Uri AppendEscapedSegment(Uri uri, string segmentIdentifier)
        {
            string str = CommonUtil.UriToString(uri);
            if (!str.EndsWith("/", StringComparison.Ordinal))
            {
                str = str + "/";
            }
            return new Uri(str + segmentIdentifier, UriKind.RelativeOrAbsolute);
        }

        internal static Uri AppendUnescapedSegment(Uri uri, string text)
        {
            return AppendEscapedSegment(uri, Uri.EscapeDataString(text));
        }

        private static void ApplyKeyPredicates(System.Data.Services.SegmentInfo segment)
        {
            if (!segment.Key.AreValuesNamed && (segment.Key.ValueCount > 1))
            {
                throw DataServiceException.CreateBadRequestError(System.Data.Services.Strings.RequestUriProcessor_KeysMustBeNamed);
            }
            segment.RequestExpression = SelectResourceByKey(segment.RequestExpression, segment.TargetResourceType, segment.Key);
        }

        private static void CheckSegmentIsComposable(System.Data.Services.SegmentInfo previous)
        {
            OperationWrapper operation = previous.Operation;
            if ((operation != null) && (((operation.ResultKind == ServiceOperationResultKind.Enumeration) || (operation.ResultKind == ServiceOperationResultKind.DirectValue)) || ((operation.ResultKind == ServiceOperationResultKind.QueryWithSingleResult) && (operation.ResultType.ResourceTypeKind != ResourceTypeKind.EntityType))))
            {
                throw DataServiceException.ResourceNotFoundError(System.Data.Services.Strings.RequestUriProcessor_IEnumerableServiceOperationsCannotBeFurtherComposed(previous.Identifier));
            }
        }

        private static void CheckSegmentRights(System.Data.Services.SegmentInfo segment)
        {
            if (((segment.Operation != null) && (segment.Operation.Kind == OperationKind.ServiceOperation)) && ((segment.Operation.ServiceOperationRights & ServiceOperationRights.OverrideEntitySetRights) != ServiceOperationRights.None))
            {
                DataServiceConfiguration.CheckServiceOperationRights(segment.Operation, segment.SingleResult);
            }
            else if (segment.TargetKind == RequestTargetKind.Resource)
            {
                DataServiceConfiguration.CheckResourceRightsForRead(segment.TargetContainer, segment.SingleResult);
            }
        }

        private static void CheckSingleResult(bool isSingleResult, string identifier)
        {
            if (!isSingleResult)
            {
                throw DataServiceException.CreateBadRequestError(System.Data.Services.Strings.RequestUriProcessor_CannotQueryCollections(identifier));
            }
        }

        private static void ComposeExpressionForEntitySet(System.Data.Services.SegmentInfo segment, IDataService service, bool isLastSegment, bool checkRights)
        {
            bool hasKeyValues = (segment.Key != null) && !segment.Key.IsEmpty;
            if (ShouldRequestQuery(service, isLastSegment, false, hasKeyValues))
            {
                segment.RequestExpression = service.Provider.GetQueryRootForResourceSet(segment.TargetContainer, service.OperationContext);
            }
            if (hasKeyValues)
            {
                ApplyKeyPredicates(segment);
            }
            if (checkRights)
            {
                DataServiceConfiguration.CheckResourceRightsForRead(segment.TargetContainer, segment.SingleResult);
            }
            if (segment.RequestExpression != null)
            {
                segment.RequestExpression = DataServiceConfiguration.ComposeResourceContainer(service, segment.TargetContainer, segment.RequestExpression);
            }
        }

        private static void ComposeExpressionForProperty(System.Data.Services.SegmentInfo segment, System.Data.Services.SegmentInfo previous, IDataService service, bool lastSegment, bool checkRights)
        {
            if (segment.ProjectedProperty.CanReflectOnInstanceTypeProperty)
            {
                segment.RequestExpression = (segment.ProjectedProperty.Kind == ResourcePropertyKind.ResourceSetReference) ? SelectMultiple(previous.RequestExpression, segment.ProjectedProperty) : SelectElement(previous.RequestExpression, segment.ProjectedProperty);
            }
            else
            {
                segment.RequestExpression = (segment.ProjectedProperty.Kind == ResourcePropertyKind.ResourceSetReference) ? SelectLateBoundPropertyMultiple(previous.RequestExpression, segment.ProjectedProperty) : SelectLateBoundProperty(previous.RequestExpression, segment.ProjectedProperty);
            }
            bool hasKeyValues = (segment.Key != null) && !segment.Key.IsEmpty;
            if (hasKeyValues)
            {
                ApplyKeyPredicates(segment);
            }
            if (segment.TargetContainer != null)
            {
                if (checkRights)
                {
                    DataServiceConfiguration.CheckResourceRightsForRead(segment.TargetContainer, segment.SingleResult);
                }
                if (ShouldRequestQuery(service, lastSegment, previous.TargetKind == RequestTargetKind.Link, hasKeyValues))
                {
                    segment.RequestExpression = DataServiceConfiguration.ComposeResourceContainer(service, segment.TargetContainer, segment.RequestExpression);
                }
            }
        }

        private static void ComposeExpressionForSegments(System.Data.Services.SegmentInfo[] segments, IDataService service)
        {
            bool flag = Deserializer.IsCrossReferencedSegment(segments[0], service);
            int num = -1;
            System.Data.Services.SegmentInfo lastSegment = segments.Last<System.Data.Services.SegmentInfo>();
            if ((lastSegment.Operation != null) && (lastSegment.Operation.Kind == OperationKind.Action))
            {
                for (int j = segments.Length - 2; j > -1; j--)
                {
                    if (!segments[j].IsTypeIdentifierSegment)
                    {
                        num = j;
                        break;
                    }
                }
            }
            System.Data.Services.SegmentInfo segment = null;
            System.Data.Services.SegmentInfo previous = null;
            for (int i = 0; i < segments.Length; i++)
            {
                bool isLastSegment = i == (segments.Length - 1);
                bool checkRights = !isLastSegment && (i != num);
                bool flag4 = i != num;
                previous = segment;
                segment = segments[i];
                if (!flag && (previous != null))
                {
                    segment.RequestExpression = previous.RequestExpression;
                }
                if ((!flag && (segment.TargetKind != RequestTargetKind.Link)) && (segment.Identifier != "$count"))
                {
                    if (segment.IsTypeIdentifierSegment)
                    {
                        ComposeExpressionForTypeNameSegment(segment, previous);
                    }
                    else if (segment.TargetSource == RequestTargetSource.EntitySet)
                    {
                        ComposeExpressionForEntitySet(segment, service, isLastSegment, checkRights);
                    }
                    else if (segment.TargetSource == RequestTargetSource.ServiceOperation)
                    {
                        if (DataServiceActionProviderWrapper.IsServiceActionSegment(segment))
                        {
                            ComposeExpressionForServiceAction(segment, previous, service);
                        }
                        else
                        {
                            ComposeExpressionForServiceOperation(segment, service, flag4, lastSegment);
                        }
                    }
                    else if ((segment.TargetSource == RequestTargetSource.Property) && (segment.Identifier != "$value"))
                    {
                        if ((segment.ProjectedProperty != null) && !segment.ProjectedProperty.IsOfKind(ResourcePropertyKind.Stream))
                        {
                            ComposeExpressionForProperty(segment, previous, service, isLastSegment, checkRights);
                        }
                        else if (segment.TargetKind == RequestTargetKind.OpenProperty)
                        {
                            segment.RequestExpression = SelectOpenProperty(previous.RequestExpression, segment.Identifier);
                        }
                    }
                }
            }
        }

        private static void ComposeExpressionForServiceAction(System.Data.Services.SegmentInfo segment, System.Data.Services.SegmentInfo previousSegment, IDataService service)
        {
            Expression[] parameterTokens = ValidateBindingParameterAndReadPayloadParametersForAction(service, segment, previousSegment);
            segment.RequestExpression = service.ActionProvider.CreateInvokable(service.OperationContext, segment.Operation, parameterTokens);
        }

        private static System.Data.Services.SegmentInfo ComposeExpressionForServiceOperation(System.Data.Services.SegmentInfo segment, IDataService service, bool checkRights, System.Data.Services.SegmentInfo lastSegment)
        {
            bool flag = false;
            if (checkRights)
            {
                CheckSegmentRights(segment);
            }
            if (segment != lastSegment)
            {
                flag = DataServiceActionProviderWrapper.IsServiceActionSegment(lastSegment);
                if ((segment.Operation.Method == "POST") && flag)
                {
                    throw DataServiceException.CreateBadRequestError(System.Data.Services.Strings.RequestUriProcessor_ActionComposedWithWebInvokeServiceOperationNotAllowed);
                }
            }
            if (!flag && (service.OperationContext.RequestMethod != segment.Operation.Method))
            {
                throw DataServiceException.CreateMethodNotAllowed(System.Data.Services.Strings.RequestUriProcessor_MethodNotAllowed, segment.Operation.Method);
            }
            object[] parameters = ReadOperationParameters(service.OperationContext.Host, segment.Operation);
            ConstantExpression expression = null;
            switch (segment.Operation.ResultKind)
            {
                case ServiceOperationResultKind.DirectValue:
                case ServiceOperationResultKind.Enumeration:
                    expression = service.Provider.InvokeServiceOperation(segment.Operation, parameters);
                    WebUtil.CheckResourceExists(segment.SingleResult || (expression.Value != null), segment.Identifier);
                    break;

                case ServiceOperationResultKind.QueryWithMultipleResults:
                case ServiceOperationResultKind.QueryWithSingleResult:
                    expression = service.Provider.InvokeServiceOperation(segment.Operation, parameters);
                    WebUtil.CheckResourceExists(expression.Value != null, segment.Identifier);
                    break;

                default:
                    service.Provider.InvokeServiceOperation(segment.Operation, parameters);
                    break;
            }
            segment.RequestExpression = expression;
            if (((segment.RequestExpression != null) && (segment.Key != null)) && !segment.Key.IsEmpty)
            {
                ApplyKeyPredicates(segment);
            }
            return segment;
        }

        private static void ComposeExpressionForTypeNameSegment(System.Data.Services.SegmentInfo segment, System.Data.Services.SegmentInfo previous)
        {
            segment.RequestExpression = SelectDerivedResourceType(previous.RequestExpression, segment.TargetResourceType);
            if ((segment.Key != null) && !segment.Key.IsEmpty)
            {
                ApplyKeyPredicates(segment);
            }
        }

        private static System.Data.Services.SegmentInfo CreateCountSegment(System.Data.Services.SegmentInfo previous, IDataService service)
        {
            CheckSegmentIsComposable(previous);
            if (previous.TargetKind != RequestTargetKind.Resource)
            {
                throw DataServiceException.CreateResourceNotFound(System.Data.Services.Strings.RequestUriProcessor_CountNotSupported(previous.Identifier));
            }
            if (previous.SingleResult)
            {
                throw DataServiceException.CreateResourceNotFound(System.Data.Services.Strings.RequestUriProcessor_CannotQuerySingletons(previous.Identifier, "$count"));
            }
            if (service.OperationContext.Host.HttpVerb != HttpVerbs.GET)
            {
                throw DataServiceException.CreateBadRequestError(System.Data.Services.Strings.RequestQueryProcessor_RequestVerbCannotCountError);
            }
            return new System.Data.Services.SegmentInfo { Identifier = "$count", TargetSource = RequestTargetSource.Property, RequestExpression = previous.RequestExpression, SingleResult = true, TargetKind = RequestTargetKind.PrimitiveValue, TargetResourceType = previous.TargetResourceType, TargetContainer = previous.TargetContainer };
        }

        private static System.Data.Services.SegmentInfo CreateFirstSegment(IDataService service, string identifier, string queryPortion, out bool crossReferencingUrl)
        {
            bool flag;
            crossReferencingUrl = false;
            System.Data.Services.SegmentInfo segment = new System.Data.Services.SegmentInfo {
                Identifier = identifier
            };
            if (segment.Identifier == "$metadata")
            {
                WebUtil.CheckSyntaxValid(queryPortion == null);
                segment.TargetKind = RequestTargetKind.Metadata;
                return segment;
            }
            if (segment.Identifier == "$batch")
            {
                WebUtil.CheckSyntaxValid(queryPortion == null);
                segment.TargetKind = RequestTargetKind.Batch;
                return segment;
            }
            if (segment.Identifier == "$count")
            {
                throw DataServiceException.CreateResourceNotFound(System.Data.Services.Strings.RequestUriProcessor_CountOnRoot);
            }
            segment.Operation = service.Provider.TryResolveServiceOperation(segment.Identifier);
            if (segment.Operation != null)
            {
                return CreateSegmentForServiceOperation(service, queryPortion, segment);
            }
            System.Data.Services.SegmentInfo segmentForContentId = service.GetSegmentForContentId(segment.Identifier);
            if (segmentForContentId != null)
            {
                segmentForContentId.Identifier = segment.Identifier;
                crossReferencingUrl = true;
                return segmentForContentId;
            }
            ResourceSetWrapper wrapper = service.Provider.TryResolveResourceSet(segment.Identifier);
            if (wrapper != null)
            {
                segment.TargetContainer = wrapper;
                segment.TargetResourceType = wrapper.ResourceType;
                segment.TargetSource = RequestTargetSource.EntitySet;
                segment.TargetKind = RequestTargetKind.Resource;
                segment.SingleResult = false;
                if (queryPortion != null)
                {
                    ExtractKeyPredicates(queryPortion, segment);
                }
                return segment;
            }
            string nameFromContainerQualifiedName = service.Provider.GetNameFromContainerQualifiedName(identifier, out flag);
            OperationWrapper serviceAction = service.ActionProvider.TryResolveServiceAction(service.OperationContext, nameFromContainerQualifiedName);
            WebUtil.CheckResourceExists(serviceAction != null, identifier);
            return CreateSegmentForServiceAction(null, serviceAction, service, identifier, queryPortion);
        }

        private static System.Data.Services.SegmentInfo CreateNamedStreamSegment(System.Data.Services.SegmentInfo previous, ResourceProperty streamProperty, IDataService service)
        {
            System.Data.Services.SegmentInfo info = new System.Data.Services.SegmentInfo {
                Identifier = streamProperty.Name
            };
            info.TargetKind = RequestTargetKind.MediaResource;
            info.RequestExpression = previous.RequestExpression;
            info.SingleResult = true;
            info.TargetResourceType = previous.TargetResourceType;
            info.TargetSource = RequestTargetSource.Property;
            RequestQueryProcessor.CheckEmptyQueryArguments(service, false);
            return info;
        }

        private static System.Data.Services.SegmentInfo CreateOpenPropertySegment(System.Data.Services.SegmentInfo previous, IDataService service, string identifier, bool hasQuery)
        {
            System.Data.Services.SegmentInfo info = new System.Data.Services.SegmentInfo {
                Identifier = identifier
            };
            if (previous.TargetResourceType != null)
            {
                WebUtil.CheckResourceExists(previous.TargetResourceType.IsOpenType, info.Identifier);
            }
            if (((previous.TargetKind == RequestTargetKind.Link) || hasQuery) || (service.OperationContext.Host.HttpVerb == HttpVerbs.POST))
            {
                throw DataServiceException.CreateBadRequestError(System.Data.Services.Strings.OpenNavigationPropertiesNotSupportedOnOpenTypes(info.Identifier));
            }
            info.TargetSource = RequestTargetSource.Property;
            info.TargetResourceType = null;
            info.TargetKind = RequestTargetKind.OpenProperty;
            info.SingleResult = true;
            return info;
        }

        private static System.Data.Services.SegmentInfo CreatePropertySegment(System.Data.Services.SegmentInfo previous, ResourceProperty property, string queryPortion, IDataService service, bool crossReferencingUri)
        {
            System.Data.Services.SegmentInfo segment = new System.Data.Services.SegmentInfo {
                Identifier = property.Name,
                ProjectedProperty = property
            };
            segment.TargetResourceType = property.ResourceType;
            ResourcePropertyKind kind = property.Kind;
            segment.SingleResult = kind != ResourcePropertyKind.ResourceSetReference;
            segment.TargetSource = RequestTargetSource.Property;
            if ((previous.TargetKind == RequestTargetKind.Link) && (property.TypeKind != ResourceTypeKind.EntityType))
            {
                throw DataServiceException.CreateBadRequestError(System.Data.Services.Strings.RequestUriProcessor_LinkSegmentMustBeFollowedByEntitySegment(segment.Identifier, "$links"));
            }
            switch (kind)
            {
                case ResourcePropertyKind.ResourceSetReference:
                case ResourcePropertyKind.ResourceReference:
                    segment.TargetKind = RequestTargetKind.Resource;
                    segment.TargetContainer = service.Provider.GetContainer(previous.TargetContainer, previous.TargetResourceType, property);
                    if (segment.TargetContainer == null)
                    {
                        throw DataServiceException.CreateResourceNotFound(property.Name);
                    }
                    break;

                case ResourcePropertyKind.Collection:
                    segment.TargetKind = RequestTargetKind.Collection;
                    break;

                case ResourcePropertyKind.ComplexType:
                    segment.TargetKind = RequestTargetKind.ComplexObject;
                    break;

                default:
                    segment.TargetKind = RequestTargetKind.Primitive;
                    break;
            }
            if (queryPortion != null)
            {
                WebUtil.CheckSyntaxValid(!segment.SingleResult);
                if (crossReferencingUri)
                {
                    throw DataServiceException.CreateBadRequestError(System.Data.Services.Strings.BadRequest_ResourceCanBeCrossReferencedOnlyForBindOperation);
                }
                ExtractKeyPredicates(queryPortion, segment);
            }
            return segment;
        }

        private static System.Data.Services.SegmentInfo CreateSegmentForServiceAction(System.Data.Services.SegmentInfo previousSegment, OperationWrapper serviceAction, IDataService service, string identifier, string queryPortion)
        {
            System.Data.Services.SegmentInfo info = new System.Data.Services.SegmentInfo {
                Identifier = identifier,
                Operation = serviceAction
            };
            if ((previousSegment != null) && (previousSegment.TargetKind == RequestTargetKind.Link))
            {
                throw DataServiceException.CreateBadRequestError(System.Data.Services.Strings.RequestUriProcessor_LinkSegmentMustBeFollowedByEntitySegment(identifier, "$links"));
            }
            if ((previousSegment != null) && Deserializer.IsCrossReferencedSegment(previousSegment, service))
            {
                throw DataServiceException.CreateBadRequestError(System.Data.Services.Strings.RequestUriProcessor_BatchedActionOnEntityCreatedInSameChangeset(identifier));
            }
            info.TargetSource = RequestTargetSource.ServiceOperation;
            if (service.OperationContext.RequestMethod != serviceAction.Method)
            {
                throw DataServiceException.CreateMethodNotAllowed(System.Data.Services.Strings.RequestUriProcessor_MethodNotAllowed, serviceAction.Method);
            }
            if ((queryPortion != null) && !string.IsNullOrEmpty(RemoveFilterParens(queryPortion)))
            {
                throw DataServiceException.CreateBadRequestError(System.Data.Services.Strings.RequestUriProcessor_SegmentDoesNotSupportKeyPredicates(identifier));
            }
            if (serviceAction.ResultKind != ServiceOperationResultKind.Void)
            {
                info.TargetContainer = serviceAction.GetResultSet(service.Provider, (previousSegment == null) ? null : previousSegment.TargetContainer);
                info.TargetResourceType = serviceAction.ReturnType;
                info.TargetKind = TargetKindFromType(info.TargetResourceType);
                if ((info.TargetKind == RequestTargetKind.Resource) && (info.TargetContainer == null))
                {
                    throw DataServiceException.CreateForbidden();
                }
                info.SingleResult = serviceAction.ResultKind == ServiceOperationResultKind.DirectValue;
            }
            else
            {
                info.TargetContainer = null;
                info.TargetResourceType = null;
                info.TargetKind = RequestTargetKind.VoidOperation;
            }
            RequestQueryProcessor.CheckEmptyQueryArguments(service, false);
            return info;
        }

        private static System.Data.Services.SegmentInfo CreateSegmentForServiceOperation(IDataService service, string queryPortion, System.Data.Services.SegmentInfo segment)
        {
            segment.TargetSource = RequestTargetSource.ServiceOperation;
            segment.TargetContainer = segment.Operation.ResourceSet;
            if (segment.Operation.ResultKind != ServiceOperationResultKind.Void)
            {
                segment.TargetResourceType = segment.Operation.ResultType;
                segment.TargetKind = TargetKindFromType(segment.TargetResourceType);
                segment.SingleResult = (segment.Operation.ResultKind == ServiceOperationResultKind.QueryWithSingleResult) || (segment.Operation.ResultKind == ServiceOperationResultKind.DirectValue);
                if ((segment.Operation.ResultKind != ServiceOperationResultKind.QueryWithMultipleResults) && (queryPortion != null))
                {
                    throw DataServiceException.CreateBadRequestError(System.Data.Services.Strings.RequestUriProcessor_SegmentDoesNotSupportQueryPortion(segment.Identifier));
                }
                if ((segment.Operation.ResultKind == ServiceOperationResultKind.DirectValue) || (segment.Operation.ResultKind == ServiceOperationResultKind.Enumeration))
                {
                    RequestQueryProcessor.CheckEmptyQueryArguments(service, false);
                }
                if (queryPortion != null)
                {
                    ExtractKeyPredicates(queryPortion, segment);
                }
                return segment;
            }
            segment.TargetResourceType = null;
            segment.TargetKind = RequestTargetKind.VoidOperation;
            return segment;
        }

        private static System.Data.Services.SegmentInfo[] CreateSegments(string[] segments, IDataService service)
        {
            System.Data.Services.SegmentInfo segment = null;
            System.Data.Services.SegmentInfo[] infoArray = new System.Data.Services.SegmentInfo[segments.Length];
            bool crossReferencingUrl = false;
            bool flag2 = false;
            ResourceType targetResourceType = null;
            for (int i = 0; i < segments.Length; i++)
            {
                string str2;
                System.Data.Services.SegmentInfo info2;
                string str = segments[i];
                bool hasQuery = ExtractSegmentIdentifier(str, out str2);
                string queryPortion = hasQuery ? str.Substring(str2.Length) : null;
                if (str2.Length == 0)
                {
                    throw DataServiceException.ResourceNotFoundError(System.Data.Services.Strings.RequestUriProcessor_EmptySegmentInRequestUrl);
                }
                if (segment == null)
                {
                    info2 = CreateFirstSegment(service, str2, queryPortion, out crossReferencingUrl);
                }
                else
                {
                    if (DataServiceActionProviderWrapper.IsServiceActionSegment(segment))
                    {
                        throw DataServiceException.CreateBadRequestError(System.Data.Services.Strings.RequestUriProcessor_MustBeLeafSegment(segment.Identifier));
                    }
                    if ((((segment.TargetKind == RequestTargetKind.Batch) || (segment.TargetKind == RequestTargetKind.Metadata)) || ((segment.TargetKind == RequestTargetKind.PrimitiveValue) || (segment.TargetKind == RequestTargetKind.VoidOperation))) || (((segment.TargetKind == RequestTargetKind.OpenPropertyValue) || (segment.TargetKind == RequestTargetKind.MediaResource)) || (segment.TargetKind == RequestTargetKind.Collection)))
                    {
                        throw DataServiceException.ResourceNotFoundError(System.Data.Services.Strings.RequestUriProcessor_MustBeLeafSegment(segment.Identifier));
                    }
                    if (flag2 && (str2 != "$count"))
                    {
                        throw DataServiceException.ResourceNotFoundError(System.Data.Services.Strings.RequestUriProcessor_CannotSpecifyAfterPostLinkSegment(str2, "$links"));
                    }
                    if (str2 == "$value")
                    {
                        info2 = CreateValueSegment(segment, service, str2, hasQuery);
                    }
                    else
                    {
                        if (segment.TargetKind == RequestTargetKind.Primitive)
                        {
                            throw DataServiceException.ResourceNotFoundError(System.Data.Services.Strings.RequestUriProcessor_ValueSegmentAfterScalarPropertySegment(segment.Identifier, str2));
                        }
                        if (((segment.TargetKind == RequestTargetKind.Resource) && segment.SingleResult) && (str2 == "$links"))
                        {
                            info2 = new System.Data.Services.SegmentInfo(segment) {
                                Identifier = str2,
                                TargetKind = RequestTargetKind.Link
                            };
                        }
                        else if (str2 == "$count")
                        {
                            info2 = CreateCountSegment(segment, service);
                        }
                        else
                        {
                            ResourceProperty property;
                            flag2 = segment.TargetKind == RequestTargetKind.Link;
                            CheckSegmentIsComposable(segment);
                            if (segment.TargetResourceType == null)
                            {
                                property = null;
                                targetResourceType = null;
                            }
                            else
                            {
                                property = segment.TargetResourceType.TryResolvePropertyName(str2);
                            }
                            if (property != null)
                            {
                                CheckSingleResult(segment.SingleResult, segment.Identifier);
                                targetResourceType = null;
                                if (property.IsOfKind(ResourcePropertyKind.Stream))
                                {
                                    info2 = CreateNamedStreamSegment(segment, property, service);
                                }
                                else
                                {
                                    info2 = CreatePropertySegment(segment, property, queryPortion, service, crossReferencingUrl);
                                }
                            }
                            else
                            {
                                if (segment.TargetContainer != null)
                                {
                                    targetResourceType = WebUtil.ResolveTypeIdentifier(service.Provider, str2, segment.TargetResourceType, targetResourceType != null);
                                }
                                else
                                {
                                    targetResourceType = null;
                                }
                                if (targetResourceType != null)
                                {
                                    info2 = CreateTypeNameSegment(segment, targetResourceType, service, str2, queryPortion);
                                }
                                else
                                {
                                    bool flag4;
                                    OperationWrapper serviceAction = null;
                                    string nameFromContainerQualifiedName = service.Provider.GetNameFromContainerQualifiedName(str2, out flag4);
                                    if ((segment.TargetResourceType != null) && (!segment.TargetResourceType.IsOpenType || flag4))
                                    {
                                        serviceAction = service.ActionProvider.TryResolveServiceAction(service.OperationContext, nameFromContainerQualifiedName);
                                    }
                                    if (serviceAction != null)
                                    {
                                        info2 = CreateSegmentForServiceAction(segment, serviceAction, service, str2, queryPortion);
                                    }
                                    else
                                    {
                                        CheckSingleResult(segment.SingleResult, segment.Identifier);
                                        info2 = CreateOpenPropertySegment(segment, service, str2, queryPortion != null);
                                    }
                                }
                            }
                        }
                    }
                }
                infoArray[i] = info2;
                segment = infoArray[i];
            }
            if ((segments.Length != 0) && (segment.TargetKind == RequestTargetKind.Link))
            {
                throw DataServiceException.CreateBadRequestError(System.Data.Services.Strings.RequestUriProcessor_MissingSegmentAfterLink("$links"));
            }
            if (infoArray.Length > 0)
            {
                if ((segment.Operation != null) && (segment.Operation.ResultKind == ServiceOperationResultKind.QueryWithSingleResult))
                {
                    RequestQueryProcessor.CheckEmptySetQueryArguments(service);
                }
                ComposeExpressionForSegments(infoArray, service);
            }
            return infoArray;
        }

        private static System.Data.Services.SegmentInfo CreateTypeNameSegment(System.Data.Services.SegmentInfo previous, ResourceType targetResourceType, IDataService service, string identifier, string queryPortion)
        {
            WebUtil.CheckMaxProtocolVersion(RequestDescription.Version3Dot0, service.Configuration.DataServiceBehavior.MaxProtocolVersion.ToVersion());
            System.Data.Services.SegmentInfo segment = new System.Data.Services.SegmentInfo {
                Identifier = identifier
            };
            segment.Operation = previous.Operation;
            segment.TargetKind = previous.TargetKind;
            segment.TargetSource = previous.TargetSource;
            segment.TargetResourceType = targetResourceType;
            segment.SingleResult = previous.SingleResult;
            segment.TargetContainer = previous.TargetContainer;
            segment.ProjectedProperty = previous.ProjectedProperty;
            segment.IsTypeIdentifierSegment = true;
            if (queryPortion != null)
            {
                WebUtil.CheckSyntaxValid(!segment.SingleResult);
                ExtractKeyPredicates(queryPortion, segment);
            }
            return segment;
        }

        private static System.Data.Services.SegmentInfo CreateValueSegment(System.Data.Services.SegmentInfo previous, IDataService service, string identifier, bool hasQuery)
        {
            System.Data.Services.SegmentInfo info;
            if (previous.TargetKind == RequestTargetKind.Primitive)
            {
                info = new System.Data.Services.SegmentInfo(previous);
            }
            else
            {
                CheckSegmentIsComposable(previous);
                info = new System.Data.Services.SegmentInfo {
                    TargetSource = RequestTargetSource.Property,
                    TargetResourceType = previous.TargetResourceType
                };
            }
            info.Identifier = identifier;
            WebUtil.CheckSyntaxValid(!hasQuery);
            CheckSingleResult(previous.SingleResult, previous.Identifier);
            info.RequestExpression = previous.RequestExpression;
            info.SingleResult = true;
            if (previous.TargetKind == RequestTargetKind.Primitive)
            {
                info.TargetKind = RequestTargetKind.PrimitiveValue;
                if (previous.TargetResourceType.InstanceType.IsSpatial())
                {
                    throw DataServiceException.CreateBadRequestError(System.Data.Services.Strings.BadRequest_ValuesCannotBeReturnedForSpatialTypes);
                }
                return info;
            }
            if (previous.TargetKind == RequestTargetKind.OpenProperty)
            {
                info.TargetKind = RequestTargetKind.OpenPropertyValue;
                return info;
            }
            info.TargetKind = RequestTargetKind.MediaResource;
            RequestQueryProcessor.CheckEmptyQueryArguments(service, false);
            return info;
        }

        internal static string[] EnumerateSegments(Uri absoluteRequestUri, Uri baseUri)
        {
            string[] strArray3;
            if (!UriUtil.UriInvariantInsensitiveIsBaseOf(baseUri, absoluteRequestUri))
            {
                throw DataServiceException.CreateBadRequestError(System.Data.Services.Strings.BadRequest_RequestUriDoesNotHaveTheRightBaseUri(absoluteRequestUri, baseUri));
            }
            try
            {
                Uri uri = absoluteRequestUri;
                int length = baseUri.Segments.Length;
                string[] segments = uri.Segments;
                int num2 = 0;
                for (int i = length; i < segments.Length; i++)
                {
                    string str = segments[i];
                    if ((str.Length != 0) && (str != "/"))
                    {
                        num2++;
                    }
                }
                string[] strArray2 = new string[num2];
                int num4 = 0;
                for (int j = length; j < segments.Length; j++)
                {
                    string str2 = UriUtil.ReadSegmentValue(segments[j]);
                    if (str2 != null)
                    {
                        strArray2[num4++] = str2;
                    }
                }
                strArray3 = strArray2;
            }
            catch (UriFormatException)
            {
                throw DataServiceException.CreateSyntaxError();
            }
            return strArray3;
        }

        private static void ExtractKeyPredicates(string filter, System.Data.Services.SegmentInfo segment)
        {
            ResourceType targetResourceType = segment.TargetResourceType;
            segment.Key = ExtractKeyValues(targetResourceType, filter);
            segment.SingleResult = !segment.Key.IsEmpty;
        }

        private static KeyInstance ExtractKeyValues(ResourceType resourceType, string filter)
        {
            KeyInstance instance;
            filter = RemoveFilterParens(filter);
            WebUtil.CheckSyntaxValid(KeyInstance.TryParseKeysFromUri(filter, out instance));
            if (!instance.IsEmpty)
            {
                if (resourceType.KeyProperties.Count != instance.ValueCount)
                {
                    throw DataServiceException.CreateBadRequestError(System.Data.Services.Strings.BadRequest_KeyCountMismatch(resourceType.FullName));
                }
                WebUtil.CheckSyntaxValid(instance.TryConvertValues(resourceType));
            }
            return instance;
        }

        private static bool ExtractSegmentIdentifier(string segment, out string identifier)
        {
            int length = 0;
            while ((length < segment.Length) && (segment[length] != '('))
            {
                length++;
            }
            identifier = segment.Substring(0, length);
            return (length < segment.Length);
        }

        internal static Uri GetAbsoluteUriFromReference(string reference, DataServiceOperationContext operationContext)
        {
            return GetAbsoluteUriFromReference(reference, operationContext.AbsoluteServiceUri, operationContext.Host.RequestVersion);
        }

        internal static Uri GetAbsoluteUriFromReference(string reference, Uri absoluteServiceUri, Version dataServiceVersion)
        {
            Uri referenceAsUri = new Uri(reference, UriKind.RelativeOrAbsolute);
            return GetAbsoluteUriFromReference(referenceAsUri, absoluteServiceUri, dataServiceVersion);
        }

        internal static Uri GetAbsoluteUriFromReference(Uri referenceAsUri, Uri absoluteServiceUri, Version dataServiceVersion)
        {
            if (!referenceAsUri.IsAbsoluteUri)
            {
                string relativeUri = CommonUtil.UriToString(referenceAsUri);
                string str2 = CommonUtil.UriToString(absoluteServiceUri);
                if (dataServiceVersion == RequestDescription.Version3Dot0)
                {
                    if (!Uri.TryCreate(absoluteServiceUri, relativeUri, out referenceAsUri))
                    {
                        throw DataServiceException.CreateBadRequestError(System.Data.Services.Strings.BadRequest_RequestUriCannotBeBasedOnBaseUri(relativeUri, absoluteServiceUri));
                    }
                }
                else
                {
                    string str4 = string.Empty;
                    if (str2.EndsWith("/", StringComparison.Ordinal))
                    {
                        if (relativeUri.StartsWith("/", StringComparison.Ordinal))
                        {
                            relativeUri = relativeUri.Substring(1, relativeUri.Length - 1);
                        }
                    }
                    else if (!relativeUri.StartsWith("/", StringComparison.Ordinal))
                    {
                        str4 = "/";
                    }
                    referenceAsUri = new Uri(str2 + str4 + relativeUri);
                }
            }
            if (!UriUtil.UriInvariantInsensitiveIsBaseOf(absoluteServiceUri, referenceAsUri))
            {
                throw DataServiceException.CreateBadRequestError(System.Data.Services.Strings.BadRequest_RequestUriDoesNotHaveTheRightBaseUri(referenceAsUri, absoluteServiceUri));
            }
            return referenceAsUri;
        }

        private static ResourceType GetItemTypeFromResourceType(ResourceType type, out bool isCollection)
        {
            ResourceType itemType = type;
            isCollection = false;
            if (type.ResourceTypeKind == ResourceTypeKind.EntityCollection)
            {
                itemType = ((EntityCollectionResourceType) type).ItemType;
                isCollection = true;
                return itemType;
            }
            if (type.ResourceTypeKind == ResourceTypeKind.Collection)
            {
                itemType = ((CollectionResourceType) type).ItemType;
                isCollection = true;
            }
            return itemType;
        }

        internal static Uri GetResultUri(DataServiceOperationContext operationContext)
        {
            UriBuilder builder = new UriBuilder(operationContext.AbsoluteRequestUri) {
                Query = null
            };
            if (builder.Path.EndsWith("()", StringComparison.Ordinal))
            {
                builder.Path = builder.Path.Substring(0, builder.Path.Length - 2);
            }
            return builder.Uri;
        }

        private static void InvokeRequestExpression(RequestDescription description, IDataService service)
        {
            HttpVerbs httpVerb = service.OperationContext.Host.HttpVerb;
            bool flag = (httpVerb == HttpVerbs.POST) && (description.TargetSource == RequestTargetSource.ServiceOperation);
            if ((httpVerb == HttpVerbs.GET) || flag)
            {
                System.Data.Services.SegmentInfo lastSegmentInfo = description.LastSegmentInfo;
                if ((httpVerb == HttpVerbs.GET) && (description.TargetSource == RequestTargetSource.Property))
                {
                    lastSegmentInfo = description.SegmentInfos[description.GetIndexOfTargetEntityResource()];
                }
                if ((lastSegmentInfo.RequestExpression != null) && (lastSegmentInfo.RequestEnumerable == null))
                {
                    lastSegmentInfo.RequestEnumerable = service.ExecutionProvider.GetResultEnumerableFromRequest(lastSegmentInfo);
                }
            }
        }

        internal static RequestDescription ProcessRequestUri(Uri absoluteRequestUri, IDataService service, bool internalQuery)
        {
            RequestDescription description;
            string[] segments = EnumerateSegments(absoluteRequestUri, service.OperationContext.AbsoluteServiceUri);
            if (segments.Length > 100)
            {
                throw DataServiceException.CreateBadRequestError(System.Data.Services.Strings.RequestUriProcessor_TooManySegments);
            }
            System.Data.Services.SegmentInfo[] segmentInfos = CreateSegments(segments, service);
            System.Data.Services.SegmentInfo info = (segmentInfos.Length == 0) ? null : segmentInfos[segmentInfos.Length - 1];
            RequestTargetKind targetKind = (info == null) ? RequestTargetKind.ServiceDirectory : info.TargetKind;
            Uri resultUri = GetResultUri(service.OperationContext);
            switch (targetKind)
            {
                case RequestTargetKind.Metadata:
                case RequestTargetKind.Batch:
                case RequestTargetKind.ServiceDirectory:
                    description = new RequestDescription(targetKind, RequestTargetSource.None, resultUri);
                    break;

                default:
                {
                    RequestQueryCountOption option = (info.Identifier == "$count") ? RequestQueryCountOption.ValueOnly : RequestQueryCountOption.None;
                    if ((option != RequestQueryCountOption.None) && !service.Configuration.DataServiceBehavior.AcceptCountRequests)
                    {
                        throw DataServiceException.CreateBadRequestError(System.Data.Services.Strings.DataServiceConfiguration_CountNotAccepted);
                    }
                    description = new RequestDescription(segmentInfos, resultUri) {
                        CountOption = option
                    };
                    if (!internalQuery)
                    {
                        description.UpdateAndCheckEpmFeatureVersion(service);
                        description.UpdateVersions(service.OperationContext.Host.RequestAccept, service);
                    }
                    if (((description.TargetKind == RequestTargetKind.MediaResource) && RequestDescription.IsNamedStream(description)) && ((service.OperationContext.Host.HttpVerb != HttpVerbs.GET) && (service.OperationContext.Host.HttpVerb != HttpVerbs.PUT)))
                    {
                        throw DataServiceException.CreateMethodNotAllowed(System.Data.Services.Strings.RequestUriProcessor_InvalidHttpMethodForNamedStream(CommonUtil.UriToString(service.OperationContext.AbsoluteRequestUri), service.OperationContext.RequestMethod), DataServiceConfiguration.GetAllowedMethods(service.Configuration, description));
                    }
                    break;
                }
            }
            description = RequestQueryProcessor.ProcessQuery(service, description);
            if (!internalQuery)
            {
                description.ApplyRequestMinVersion(service);
                description.AnalyzeClientPreference(service);
                if (description.PreferenceApplied != PreferenceApplied.None)
                {
                    description.VerifyAndRaiseResponseVersion(RequestDescription.Version3Dot0, service);
                }
            }
            InvokeRequestExpression(description, service);
            return description;
        }

        private static object[] ReadOperationParameters(DataServiceHostWrapper host, OperationWrapper operation)
        {
            object[] objArray = new object[operation.Parameters.Count];
            for (int i = 0; i < operation.Parameters.Count; i++)
            {
                Type instanceType = operation.Parameters[i].ParameterType.InstanceType;
                string queryStringItem = host.GetQueryStringItem(operation.Parameters[i].Name);
                Type underlyingType = Nullable.GetUnderlyingType(instanceType);
                if (string.IsNullOrEmpty(queryStringItem))
                {
                    WebUtil.CheckSyntaxValid(instanceType.IsClass || (underlyingType != null));
                    objArray[i] = null;
                }
                else
                {
                    queryStringItem = queryStringItem.Trim();
                    Type type = underlyingType ?? instanceType;
                    if (WebConvert.IsKeyTypeQuoted(type))
                    {
                        WebUtil.CheckSyntaxValid(WebConvert.IsKeyValueQuoted(queryStringItem));
                    }
                    WebUtil.CheckSyntaxValid(WebConvert.TryKeyStringToPrimitive(queryStringItem, type, out objArray[i]));
                }
            }
            return objArray;
        }

        private static string RemoveFilterParens(string filter)
        {
            WebUtil.CheckSyntaxValid(((filter.Length > 0) && (filter[0] == '(')) && (filter[filter.Length - 1] == ')'));
            return filter.Substring(1, filter.Length - 2);
        }

        private static Expression SelectDerivedResourceType(Expression queryExpression, ResourceType resourceType)
        {
            if (resourceType.CanReflectOnInstanceType)
            {
                return queryExpression.QueryableOfType(resourceType.InstanceType);
            }
            Type type = queryExpression.ElementType();
            ConstantExpression expression = Expression.Constant(resourceType, typeof(ResourceType));
            return Expression.Call(null, DataServiceExecutionProviderMethods.OfTypeMethodInfo.MakeGenericMethod(new Type[] { type, resourceType.InstanceType }), queryExpression, expression);
        }

        [MethodImpl(MethodImplOptions.NoOptimization | MethodImplOptions.NoInlining)]
        private static Expression SelectElement(Expression queryExpression, ResourceProperty property)
        {
            ParameterExpression expression = Expression.Parameter(queryExpression.ElementType(), "element");
            Expression expression2 = Expression.Property(expression, property.Name);
            if ((property.TypeKind == ResourceTypeKind.Collection) && (expression2.Type != property.Type))
            {
                expression2 = Expression.Convert(expression2, property.Type);
            }
            LambdaExpression selector = Expression.Lambda(expression2, new ParameterExpression[] { expression });
            return queryExpression.QueryableSelect(selector);
        }

        [MethodImpl(MethodImplOptions.NoOptimization | MethodImplOptions.NoInlining)]
        private static Expression SelectLateBoundProperty(Expression queryExpression, ResourceProperty property)
        {
            ParameterExpression expression = Expression.Parameter(queryExpression.ElementType(), "element");
            LambdaExpression selector = Expression.Lambda(Expression.Convert(Expression.Call(null, DataServiceProviderMethods.GetValueMethodInfo, expression, Expression.Constant(property)), property.Type), new ParameterExpression[] { expression });
            return queryExpression.QueryableSelect(selector);
        }

        [MethodImpl(MethodImplOptions.NoOptimization | MethodImplOptions.NoInlining)]
        private static Expression SelectLateBoundPropertyMultiple(Expression queryExpression, ResourceProperty property)
        {
            Type iEnumerableElement = BaseServiceProvider.GetIEnumerableElement(property.Type);
            ParameterExpression expression = Expression.Parameter(queryExpression.ElementType(), "element");
            MethodInfo method = DataServiceProviderMethods.GetSequenceValueMethodInfo.MakeGenericMethod(new Type[] { iEnumerableElement });
            LambdaExpression selector = Expression.Lambda(Expression.Call(null, method, expression, Expression.Constant(property)), new ParameterExpression[] { expression });
            return queryExpression.QueryableSelectMany(selector);
        }

        [MethodImpl(MethodImplOptions.NoOptimization | MethodImplOptions.NoInlining)]
        private static Expression SelectMultiple(Expression queryExpression, ResourceProperty property)
        {
            Type iEnumerableElement = BaseServiceProvider.GetIEnumerableElement(property.Type);
            ParameterExpression expression = Expression.Parameter(queryExpression.ElementType(), "element");
            LambdaExpression selector = Expression.Lambda(Expression.ConvertChecked(Expression.Property(expression, property.Name), typeof(IEnumerable<>).MakeGenericType(new Type[] { iEnumerableElement })), new ParameterExpression[] { expression });
            return queryExpression.QueryableSelectMany(selector);
        }

        [MethodImpl(MethodImplOptions.NoOptimization | MethodImplOptions.NoInlining)]
        private static Expression SelectOpenProperty(Expression queryExpression, string propertyName)
        {
            ParameterExpression expression = Expression.Parameter(queryExpression.ElementType(), "element");
            LambdaExpression selector = Expression.Lambda(Expression.Call(null, OpenTypeMethods.GetValueOpenPropertyMethodInfo, expression, Expression.Constant(propertyName)), new ParameterExpression[] { expression });
            return queryExpression.QueryableSelect(selector);
        }

        private static Expression SelectResourceByKey(Expression queryExpression, ResourceType resourceType, KeyInstance key)
        {
            for (int i = 0; i < resourceType.KeyProperties.Count; i++)
            {
                Expression expression2;
                ResourceProperty property = resourceType.KeyProperties[i];
                object obj2 = key.AreValuesNamed ? key.NamedValues[property.Name] : key.PositionalValues[i];
                ParameterExpression expression = Expression.Parameter(queryExpression.ElementType(), "element");
                if (property.CanReflectOnInstanceTypeProperty)
                {
                    expression2 = Expression.Property(expression, property.Name);
                }
                else
                {
                    expression2 = Expression.Convert(Expression.Call(null, DataServiceProviderMethods.GetValueMethodInfo, expression, Expression.Constant(property)), property.Type);
                }
                LambdaExpression predicate = Expression.Lambda(Expression.Equal(expression2, Expression.Constant(obj2)), new ParameterExpression[] { expression });
                queryExpression = queryExpression.QueryableWhere(predicate);
            }
            return queryExpression;
        }

        private static bool ShouldRequestQuery(IDataService service, bool isLastSegment, bool isAfterLink, bool hasKeyValues)
        {
            if (service.Provider.IsV1Provider)
            {
                return true;
            }
            HttpVerbs httpVerb = service.OperationContext.Host.HttpVerb;
            bool flag = (isLastSegment && (httpVerb == HttpVerbs.POST)) && !hasKeyValues;
            bool flag2 = isAfterLink && (((httpVerb == HttpVerbs.PUT) || (httpVerb == HttpVerbs.MERGE)) || (httpVerb == HttpVerbs.PATCH));
            return (!flag && !flag2);
        }

        private static RequestTargetKind TargetKindFromType(ResourceType type)
        {
            switch (type.ResourceTypeKind)
            {
                case ResourceTypeKind.EntityType:
                case ResourceTypeKind.EntityCollection:
                    return RequestTargetKind.Resource;

                case ResourceTypeKind.ComplexType:
                    return RequestTargetKind.ComplexObject;

                case ResourceTypeKind.Collection:
                    return RequestTargetKind.Collection;
            }
            return RequestTargetKind.Primitive;
        }

        private static Expression[] ValidateBindingParameterAndReadPayloadParametersForAction(IDataService dataService, System.Data.Services.SegmentInfo actionSegment, System.Data.Services.SegmentInfo previousSegment)
        {
            OperationWrapper operation = actionSegment.Operation;
            int index = 0;
            int count = operation.Parameters.Count;
            Expression[] expressionArray = new Expression[count];
            if (previousSegment != null)
            {
                bool flag;
                bool flag2;
                if (operation.BindingParameter == null)
                {
                    throw DataServiceException.CreateBadRequestError(System.Data.Services.Strings.RequestUriProcessor_UnbindableOperationsMustBeCalledAtRootLevel(operation.Name));
                }
                ResourceType targetResourceType = previousSegment.TargetResourceType;
                ResourceType itemTypeFromResourceType = GetItemTypeFromResourceType(operation.BindingParameter.ParameterType, out flag);
                ResourceType subType = GetItemTypeFromResourceType(targetResourceType, out flag2);
                flag2 = flag2 || !previousSegment.SingleResult;
                if ((flag != flag2) || !itemTypeFromResourceType.IsAssignableFrom(subType))
                {
                    throw DataServiceException.CreateBadRequestError(System.Data.Services.Strings.RequestUriProcessor_BindingParameterNotAssignableFromPreviousSegment(operation.Name, previousSegment.Identifier));
                }
                expressionArray[index++] = previousSegment.RequestExpression;
            }
            else if (operation.BindingParameter != null)
            {
                throw DataServiceException.CreateBadRequestError(System.Data.Services.Strings.RequestUriProcessor_MissingBindingParameter(operation.Name));
            }
            if ((index < count) || !string.IsNullOrEmpty(dataService.OperationContext.Host.RequestContentType))
            {
                Dictionary<string, object> dictionary = Deserializer.ReadPayloadParameters(actionSegment, dataService);
                while (index < count)
                {
                    object obj2;
                    string name = operation.Parameters[index].Name;
                    dictionary.TryGetValue(name, out obj2);
                    expressionArray[index] = Expression.Constant(obj2, typeof(object));
                    index++;
                }
            }
            return expressionArray;
        }
    }
}

