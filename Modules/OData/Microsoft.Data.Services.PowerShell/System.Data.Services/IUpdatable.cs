namespace System.Data.Services
{
    using System;
    using System.Linq;

    internal interface IUpdatable
    {
        void AddReferenceToCollection(object targetResource, string propertyName, object resourceToBeAdded);
        void ClearChanges();
        object CreateResource(string containerName, string fullTypeName);
        void DeleteResource(object targetResource);
        object GetResource(IQueryable query, string fullTypeName);
        object GetValue(object targetResource, string propertyName);
        void RemoveReferenceFromCollection(object targetResource, string propertyName, object resourceToBeRemoved);
        object ResetResource(object resource);
        object ResolveResource(object resource);
        void SaveChanges();
        void SetReference(object targetResource, string propertyName, object propertyValue);
        void SetValue(object targetResource, string propertyName, object propertyValue);
    }
}

