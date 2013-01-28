namespace System.Data.Services.Providers
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data.Entity.Core.Objects;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading;

    internal static class DbContextHelper
    {
        private static readonly ReaderWriterLockSlim cacheLock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
        private static readonly Dictionary<Type, DbContextAccessor> contextAccessorCache = new Dictionary<Type, DbContextAccessor>(EqualityComparer<Type>.Default);

        private static DbContextAccessor CreateDbContextAccessor(Type type)
        {
            DbContextAccessor accessor = null;
            if (IsTypeOf(type, "System.Data.Entity.DbContext"))
            {
                Type type2 = type.GetInterface("System.Data.Entity.Infrastructure.IObjectContextAdapter", false);
                if (type2 != null)
                {
                    PropertyInfo property = type2.GetProperty("ObjectContext", BindingFlags.Public | BindingFlags.Instance);
                    if ((property != null) && (property.GetGetMethod() != null))
                    {
                        MethodInfo method = type.GetMethod("SaveChanges", BindingFlags.Public | BindingFlags.Instance, null, Type.EmptyTypes, null);
                        if (method != null)
                        {
                            accessor = new DbContextAccessor();
                            ParameterExpression expression = Expression.Parameter(typeof(object));
                            accessor.GetContext = Expression.Lambda<Func<object, ObjectContext>>(Expression.Property(Expression.Convert(expression, type2), property.GetGetMethod()), new ParameterExpression[] { expression }).Compile();
                            accessor.SaveChanges = Expression.Lambda<Func<object, int>>(Expression.Call(Expression.Convert(expression, type), method), new ParameterExpression[] { expression }).Compile();
                        }
                    }
                }
            }
            return accessor;
        }

        private static DbContextAccessor GetDbContextAccessor(Type type)
        {
            DbContextAccessor accessor;
            cacheLock.EnterUpgradeableReadLock();
            try
            {
                if (contextAccessorCache.TryGetValue(type, out accessor))
                {
                    return accessor;
                }
                cacheLock.EnterWriteLock();
                try
                {
                    if (!contextAccessorCache.TryGetValue(type, out accessor))
                    {
                        contextAccessorCache.Add(type, accessor = CreateDbContextAccessor(type));
                    }
                }
                finally
                {
                    cacheLock.ExitWriteLock();
                }
            }
            finally
            {
                cacheLock.ExitUpgradeableReadLock();
            }
            return accessor;
        }

        public static ObjectContext GetObjectContext(object o)
        {
            ObjectContext context = o as ObjectContext;
            if (context == null)
            {
                DbContextAccessor dbContextAccessor = GetDbContextAccessor(o.GetType());
                if (dbContextAccessor != null)
                {
                    context = dbContextAccessor.GetContext(o);
                }
            }
            return context;
        }

        public static void GetObjectContext(object o, out ObjectContext objectContext, out Func<int> saveChangesMethod)
        {
            objectContext = o as ObjectContext;
            saveChangesMethod = null;
            if (objectContext == null)
            {
                Func<int> func = null;
                DbContextAccessor accessor = GetDbContextAccessor(o.GetType());
                if (accessor != null)
                {
                    objectContext = accessor.GetContext(o);
                    if (func == null)
                    {
                        func = () => accessor.SaveChanges(o);
                    }
                    saveChangesMethod = func;
                }
            }
            else
            {
                saveChangesMethod = new Func<int>(objectContext.SaveChanges);
            }
        }

        public static bool IsDbContextType(Type type)
        {
            return (GetDbContextAccessor(type) != null);
        }

        public static bool IsDbEntityValidationException(Exception e)
        {
            return DbEntityValidationExceptionAccessor.IsDbEntityValidationException(e);
        }

        private static bool IsTypeOf(Type type, string fromTypeName)
        {
            bool flag = false;
            for (Type type2 = type; (!flag && (type2 != typeof(object))) && (type2 != null); type2 = type2.BaseType)
            {
                flag = string.Equals(type2.FullName, fromTypeName, StringComparison.Ordinal);
            }
            return flag;
        }

        public static Exception WrapDbEntityValidationException(Exception e)
        {
            DbEntityValidationExceptionAccessor accessor = new DbEntityValidationExceptionAccessor(e);
            return new InvalidOperationException(accessor.CreateVerboseMessage(), e);
        }

        private class DbContextAccessor
        {
            public Func<object, ObjectContext> GetContext { get; set; }

            public Func<object, int> SaveChanges { get; set; }
        }

        private class DbEntityValidationExceptionAccessor
        {
            private const string DbEntityValidationExceptionTypeName = "System.Data.Entity.Validation.DbEntityValidationException";
            private readonly Exception instance;

            public DbEntityValidationExceptionAccessor(Exception instance)
            {
                this.instance = instance;
            }

            public string CreateVerboseMessage()
            {
                PropertyInfo property = this.instance.GetType().GetProperty("EntityValidationErrors");
                StringBuilder builder = new StringBuilder();
                PropertyInfo info2 = null;
                PropertyInfo info3 = null;
                bool flag = true;
                foreach (object obj2 in (IEnumerable) property.GetValue(this.instance, null))
                {
                    if (info2 == null)
                    {
                        info2 = obj2.GetType().GetProperty("ValidationErrors");
                    }
                    foreach (object obj3 in (IEnumerable) info2.GetValue(obj2, null))
                    {
                        if (info3 == null)
                        {
                            info3 = obj3.GetType().GetProperty("ErrorMessage");
                        }
                        if (flag)
                        {
                            flag = false;
                        }
                        else
                        {
                            builder.AppendLine();
                        }
                        builder.Append((string) info3.GetValue(obj3, null));
                    }
                }
                return builder.ToString();
            }

            public static bool IsDbEntityValidationException(Exception e)
            {
                return DbContextHelper.IsTypeOf(e.GetType(), "System.Data.Entity.Validation.DbEntityValidationException");
            }
        }
    }
}

