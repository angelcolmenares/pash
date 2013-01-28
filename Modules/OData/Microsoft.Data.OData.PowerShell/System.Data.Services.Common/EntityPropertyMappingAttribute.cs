namespace System.Data.Services.Common
{
    using Microsoft.Data.OData;
    using Microsoft.Data.OData.Metadata;
    using System;

    [AttributeUsage(AttributeTargets.Class, AllowMultiple=true, Inherited=true)]
    internal sealed class EntityPropertyMappingAttribute : Attribute
    {
        private readonly bool keepInContent;
        private readonly string sourcePath;
        private readonly string targetNamespacePrefix;
        private readonly string targetNamespaceUri;
        private readonly string targetPath;
        private readonly SyndicationItemProperty targetSyndicationItem;
        private readonly SyndicationTextContentKind targetTextContentKind;

        public EntityPropertyMappingAttribute(string sourcePath, SyndicationItemProperty targetSyndicationItem, SyndicationTextContentKind targetTextContentKind, bool keepInContent)
        {
            if (string.IsNullOrEmpty(sourcePath))
            {
                throw new ArgumentException(Strings.EntityPropertyMapping_EpmAttribute("sourcePath"));
            }
            this.sourcePath = sourcePath;
            this.targetPath = targetSyndicationItem.ToTargetPath();
            this.targetSyndicationItem = targetSyndicationItem;
            this.targetTextContentKind = targetTextContentKind;
            this.targetNamespacePrefix = "atom";
            this.targetNamespaceUri = "http://www.w3.org/2005/Atom";
            this.keepInContent = keepInContent;
        }

        public EntityPropertyMappingAttribute(string sourcePath, string targetPath, string targetNamespacePrefix, string targetNamespaceUri, bool keepInContent)
        {
            Uri uri;
            if (string.IsNullOrEmpty(sourcePath))
            {
                throw new ArgumentException(Strings.EntityPropertyMapping_EpmAttribute("sourcePath"));
            }
            this.sourcePath = sourcePath;
            if (string.IsNullOrEmpty(targetPath))
            {
                throw new ArgumentException(Strings.EntityPropertyMapping_EpmAttribute("targetPath"));
            }
            if (targetPath[0] == '@')
            {
                throw new ArgumentException(Strings.EntityPropertyMapping_InvalidTargetPath(targetPath));
            }
            this.targetPath = targetPath;
            this.targetSyndicationItem = SyndicationItemProperty.CustomProperty;
            this.targetTextContentKind = SyndicationTextContentKind.Plaintext;
            this.targetNamespacePrefix = targetNamespacePrefix;
            if (string.IsNullOrEmpty(targetNamespaceUri))
            {
                throw new ArgumentException(Strings.EntityPropertyMapping_EpmAttribute("targetNamespaceUri"));
            }
            this.targetNamespaceUri = targetNamespaceUri;
            if (!Uri.TryCreate(targetNamespaceUri, UriKind.Absolute, out uri))
            {
                throw new ArgumentException(Strings.EntityPropertyMapping_TargetNamespaceUriNotValid(targetNamespaceUri));
            }
            this.keepInContent = keepInContent;
        }

        public bool KeepInContent
        {
            get
            {
                return this.keepInContent;
            }
        }

        public string SourcePath
        {
            get
            {
                return this.sourcePath;
            }
        }

        public string TargetNamespacePrefix
        {
            get
            {
                return this.targetNamespacePrefix;
            }
        }

        public string TargetNamespaceUri
        {
            get
            {
                return this.targetNamespaceUri;
            }
        }

        public string TargetPath
        {
            get
            {
                return this.targetPath;
            }
        }

        public SyndicationItemProperty TargetSyndicationItem
        {
            get
            {
                return this.targetSyndicationItem;
            }
        }

        public SyndicationTextContentKind TargetTextContentKind
        {
            get
            {
                return this.targetTextContentKind;
            }
        }
    }
}

