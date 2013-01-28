namespace System.Data.Services.Client
{
    using System;
    using System.ComponentModel;
    using System.Threading;

    internal sealed class DataServiceStreamLink : INotifyPropertyChanged
    {
        private string contentType;
        private Uri editLink;
        private string etag;
        private readonly string name;
        private Uri selfLink;

        public event PropertyChangedEventHandler PropertyChanged;

        internal DataServiceStreamLink(string name)
        {
            this.name = name;
        }

        private void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public string ContentType
        {
            get
            {
                return this.contentType;
            }
            internal set
            {
                this.contentType = value;
                this.OnPropertyChanged("ContentType");
            }
        }

        public Uri EditLink
        {
            get
            {
                return this.editLink;
            }
            internal set
            {
                this.editLink = value;
                this.OnPropertyChanged("EditLink");
            }
        }

        public string ETag
        {
            get
            {
                return this.etag;
            }
            internal set
            {
                this.etag = value;
                this.OnPropertyChanged("ETag");
            }
        }

        public string Name
        {
            get
            {
                return this.name;
            }
        }

        public Uri SelfLink
        {
            get
            {
                return this.selfLink;
            }
            internal set
            {
                this.selfLink = value;
                this.OnPropertyChanged("SelfLink");
            }
        }
    }
}

