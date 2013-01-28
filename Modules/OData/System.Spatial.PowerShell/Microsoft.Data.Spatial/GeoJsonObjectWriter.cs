namespace Microsoft.Data.Spatial
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    internal sealed class GeoJsonObjectWriter : GeoJsonWriterBase
    {
        private readonly Stack<object> containers = new Stack<object>();
        private string currentPropertyName;
        private object lastCompletedObject;

        protected override void AddPropertyName(string name)
        {
            this.currentPropertyName = name;
        }

        private void AddToScope(object jsonObject)
        {
            if (this.IsArray)
            {
                this.AsList().Add(jsonObject);
            }
            else
            {
                string andClearCurrentPropertyName = this.GetAndClearCurrentPropertyName();
                this.AsDictionary().Add(andClearCurrentPropertyName, jsonObject);
            }
        }

        protected override void AddValue(double value)
        {
            this.AddToScope(value);
        }

        protected override void AddValue(string value)
        {
            this.AddToScope(value);
        }

        private IDictionary<string, object> AsDictionary()
        {
            return (this.containers.Peek() as IDictionary<string, object>);
        }

        private IList AsList()
        {
            return (this.containers.Peek() as IList);
        }

        protected override void EndArrayScope()
        {
            this.containers.Pop();
        }

        protected override void EndObjectScope()
        {
            object obj2 = this.containers.Pop();
            if (this.containers.Count == 0)
            {
                this.lastCompletedObject = obj2;
            }
        }

        private string GetAndClearCurrentPropertyName()
        {
            string currentPropertyName = this.currentPropertyName;
            this.currentPropertyName = null;
            return currentPropertyName;
        }

        protected override void StartArrayScope()
        {
            object jsonObject = new List<object>();
            this.AddToScope(jsonObject);
            this.containers.Push(jsonObject);
        }

        protected override void StartObjectScope()
        {
            object jsonObject = new Dictionary<string, object>(StringComparer.Ordinal);
            if (this.containers.Count > 0)
            {
                this.AddToScope(jsonObject);
            }
            this.containers.Push(jsonObject);
        }

        private bool IsArray
        {
            get
            {
                return (this.containers.Peek() is IList);
            }
        }

        internal IDictionary<string, object> JsonObject
        {
            get
            {
                return (this.lastCompletedObject as IDictionary<string, object>);
            }
        }
    }
}

