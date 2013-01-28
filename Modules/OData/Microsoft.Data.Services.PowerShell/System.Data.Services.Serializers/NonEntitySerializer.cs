namespace System.Data.Services.Serializers
{
    using Microsoft.Data.OData;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data.Services;
    using System.Data.Services.Providers;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Threading;

    internal sealed class NonEntitySerializer : Serializer
    {
        private ODataCollectionWriter collectionWriter;
        private readonly ODataMessageWriter writer;

        internal NonEntitySerializer(RequestDescription requestDescription, Uri absoluteServiceUri, IDataService service, ODataMessageWriter messageWriter) : base(requestDescription, absoluteServiceUri, service, null)
        {
            this.writer = messageWriter;
        }

        private string ComputeContainerName()
        {
            if (DataServiceActionProviderWrapper.IsServiceActionSegment(base.RequestDescription.LastSegmentInfo))
            {
                bool flag2;
                return base.Provider.GetNameFromContainerQualifiedName(base.RequestDescription.ContainerName, out flag2);
            }
            return base.RequestDescription.ContainerName;
        }

        internal override void Flush()
        {
            if (this.collectionWriter != null)
            {
                this.collectionWriter.Flush();
            }
        }

        private Uri GetEntityUri(object element)
        {
            ResourceType nonPrimitiveResourceType = WebUtil.GetNonPrimitiveResourceType(base.Provider, element);
            return Serializer.GetEditLink(element, nonPrimitiveResourceType, base.Provider, base.CurrentContainer, base.AbsoluteServiceUri);
        }

        private IEnumerable<ODataEntityReferenceLink> GetLinksCollection(IEnumerator elements, bool hasMoved, ODataEntityReferenceLinks linksCollection)
        {
            object lastObject = null;
            IExpandedResult skipTokenExpandedResult = null;
        Label_PostSwitchInIterator:;
            if (hasMoved)
            {
                object current = elements.Current;
                IExpandedResult skipToken = null;
                if (current != null)
                {
                    IExpandedResult expanded = current as IExpandedResult;
                    if (expanded != null)
                    {
                        current = Serializer.GetExpandedElement(expanded);
                        skipToken = this.GetSkipToken(expanded);
                    }
                }
                this.IncrementSegmentResultCount();
                ODataEntityReferenceLink iteratorVariable4 = new ODataEntityReferenceLink {
                    Url = this.GetEntityUri(current)
                };
                yield return iteratorVariable4;
                hasMoved = elements.MoveNext();
                lastObject = current;
                skipTokenExpandedResult = skipToken;
                goto Label_PostSwitchInIterator;
            }
            if (this.NeedNextPageLink(elements))
            {
                linksCollection.NextPageLink = this.GetNextLinkUri(lastObject, skipTokenExpandedResult, this.RequestDescription.ResultUri);
            }
        }

        private void WriteLink(object element)
        {
            base.IncrementSegmentResultCount();
            ODataEntityReferenceLink link = new ODataEntityReferenceLink {
                Url = this.GetEntityUri(element)
            };
            this.writer.WriteEntityReferenceLink(link);
        }

        private void WriteLinkCollection(IEnumerator elements, bool hasMoved)
        {
            ODataEntityReferenceLinks linksCollection = new ODataEntityReferenceLinks();
            if (base.RequestDescription.CountOption == RequestQueryCountOption.Inline)
            {
                linksCollection.Count = new long?(base.RequestDescription.CountValue);
            }
            linksCollection.Links = this.GetLinksCollection(elements, hasMoved, linksCollection);
            this.writer.WriteEntityReferenceLinks(linksCollection);
        }

        protected override void WriteTopLevelElement(IExpandedResult expandedResult, object element)
        {
            string propertyName = this.ComputeContainerName();
            if (base.RequestDescription.LinkUri)
            {
                bool needPop = base.PushSegmentForRoot();
                this.WriteLink(element);
                base.PopSegmentName(needPop);
            }
            else
            {
                ResourceType type;
                if (element == null)
                {
                    type = (base.RequestDescription.TargetKind == RequestTargetKind.OpenProperty) ? ResourceType.PrimitiveStringResourceType : base.RequestDescription.TargetResourceType;
                }
                else
                {
                    type = (base.RequestDescription.TargetKind == RequestTargetKind.Collection) ? base.RequestDescription.TargetResourceType : WebUtil.GetResourceType(base.Provider, element);
                }
                if (type == null)
                {
                    throw new InvalidOperationException(System.Data.Services.Strings.Serializer_UnsupportedTopLevelType(element.GetType()));
                }
                ODataProperty property = new ODataProperty {
                    Name = propertyName,
                    Value = base.GetPropertyValue(propertyName, type, element, false)
                };
                this.writer.WriteProperty(property);
            }
        }

        protected override void WriteTopLevelElements(IExpandedResult expanded, IEnumerator elements, bool hasMoved)
        {
            if (base.RequestDescription.LinkUri)
            {
                bool needPop = base.PushSegmentForRoot();
                this.WriteLinkCollection(elements, hasMoved);
                base.PopSegmentName(needPop);
            }
            else
            {
                this.collectionWriter = this.writer.CreateODataCollectionWriter();
                ODataCollectionStart collectionStart = new ODataCollectionStart {
                    Name = this.ComputeContainerName()
                };
                this.collectionWriter.WriteStart(collectionStart);
                while (hasMoved)
                {
                    object current = elements.Current;
                    ResourceType propertyResourceType = (current == null) ? base.RequestDescription.TargetResourceType : WebUtil.GetResourceType(base.Provider, current);
                    if (propertyResourceType == null)
                    {
                        throw new InvalidOperationException(System.Data.Services.Strings.Serializer_UnsupportedTopLevelType(current.GetType()));
                    }
                    this.collectionWriter.WriteItem(base.GetPropertyValue("element", propertyResourceType, current, false));
                    hasMoved = elements.MoveNext();
                }
                this.collectionWriter.WriteEnd();
                this.collectionWriter.Flush();
            }
        }

        
    }
}

