namespace Microsoft.Data.OData
{
    using System;

    internal abstract class ODataAnnotatable
    {
        private object annotations;

        protected ODataAnnotatable()
        {
        }

        private void AddOrReplaceAnnotation<T>(T annotation) where T: class
        {
            if (this.annotations == null)
            {
                this.annotations = annotation;
            }
            else
            {
                object[] annotations = this.annotations as object[];
                if (annotations == null)
                {
                    if (IsOfType(this.annotations, typeof(T)))
                    {
                        this.annotations = annotation;
                    }
                    else
                    {
                        this.annotations = new object[] { this.annotations, annotation };
                    }
                }
                else
                {
                    int index = 0;
                    while (index < annotations.Length)
                    {
                        object instance = annotations[index];
                        if ((instance == null) || IsOfType(instance, typeof(T)))
                        {
                            annotations[index] = annotation;
                            break;
                        }
                        index++;
                    }
                    if (index == annotations.Length)
                    {
                        Array.Resize<object>(ref annotations, index * 2);
                        this.annotations = annotations;
                        annotations[index] = annotation;
                    }
                }
            }
        }

        public T GetAnnotation<T>() where T: class
        {
            if (this.annotations != null)
            {
                object[] annotations = this.annotations as object[];
                if (annotations == null)
                {
                    return (this.annotations as T);
                }
                for (int i = 0; i < annotations.Length; i++)
                {
                    object obj2 = annotations[i];
                    if (obj2 == null)
                    {
                        break;
                    }
                    T local = obj2 as T;
                    if (local != null)
                    {
                        return local;
                    }
                }
            }
            return default(T);
        }

        private static bool IsOfType(object instance, Type type)
        {
            return (instance.GetType() == type);
        }

        private void RemoveAnnotation<T>() where T: class
        {
            if (this.annotations != null)
            {
                object[] annotations = this.annotations as object[];
                if (annotations == null)
                {
                    if (IsOfType(this.annotations, typeof(T)))
                    {
                        this.annotations = null;
                    }
                }
                else
                {
                    int index = 0;
                    int num2 = -1;
                    int length = annotations.Length;
                    while (index < length)
                    {
                        object instance = annotations[index];
                        if (instance == null)
                        {
                            break;
                        }
                        if (IsOfType(instance, typeof(T)))
                        {
                            num2 = index;
                            break;
                        }
                        index++;
                    }
                    if (num2 >= 0)
                    {
                        for (int i = num2; i < (length - 1); i++)
                        {
                            annotations[i] = annotations[i + 1];
                        }
                        annotations[length - 1] = null;
                    }
                }
            }
        }

        public void SetAnnotation<T>(T annotation) where T: class
        {
            if (annotation == null)
            {
                this.RemoveAnnotation<T>();
            }
            else
            {
                this.AddOrReplaceAnnotation<T>(annotation);
            }
        }
    }
}

