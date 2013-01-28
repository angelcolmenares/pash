namespace Microsoft.PowerShell.Cmdletization
{
    using System;
    using System.Collections;

    public abstract class QueryBuilder
    {
        protected QueryBuilder()
        {
        }

        public virtual void AddQueryOption(string optionName, object optionValue)
        {
            throw new NotImplementedException();
        }

        public virtual void ExcludeByProperty(string propertyName, IEnumerable excludedPropertyValues, bool wildcardsEnabled, BehaviorOnNoMatch behaviorOnNoMatch)
        {
            throw new NotImplementedException();
        }

        public virtual void FilterByAssociatedInstance(object associatedInstance, string associationName, string sourceRole, string resultRole, BehaviorOnNoMatch behaviorOnNoMatch)
        {
            throw new NotImplementedException();
        }

        public virtual void FilterByMaxPropertyValue(string propertyName, object maxPropertyValue, BehaviorOnNoMatch behaviorOnNoMatch)
        {
            throw new NotImplementedException();
        }

        public virtual void FilterByMinPropertyValue(string propertyName, object minPropertyValue, BehaviorOnNoMatch behaviorOnNoMatch)
        {
            throw new NotImplementedException();
        }

        public virtual void FilterByProperty(string propertyName, IEnumerable allowedPropertyValues, bool wildcardsEnabled, BehaviorOnNoMatch behaviorOnNoMatch)
        {
            throw new NotImplementedException();
        }
    }
}

