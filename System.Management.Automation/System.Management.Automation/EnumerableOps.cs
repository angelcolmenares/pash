namespace System.Management.Automation
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Management.Automation.Internal;
    using System.Management.Automation.Language;
    using System.Management.Automation.Runspaces;
    using System.Reflection;
    using System.Runtime.CompilerServices;

    internal static class EnumerableOps
    {
        internal static object AddEnumerable(ExecutionContext context, IEnumerator lhs, IEnumerator rhs)
        {
            NonEnumerableObjectEnumerator fakeEnumerator = lhs as NonEnumerableObjectEnumerator;
            if (fakeEnumerator != null)
            {
                return AddFakeEnumerable(fakeEnumerator, rhs);
            }
            ArrayList list = new ArrayList();
            while (MoveNext(context, lhs))
            {
                list.Add(Current(lhs));
            }
            while (MoveNext(context, rhs))
            {
                list.Add(Current(rhs));
            }
            return list.ToArray();
        }

        internal static object AddFakeEnumerable(NonEnumerableObjectEnumerator fakeEnumerator, object rhs)
        {
            NonEnumerableObjectEnumerator enumerator = rhs as NonEnumerableObjectEnumerator;
            return ParserOps.ImplicitOp(fakeEnumerator.GetNonEnumerableObject(), (enumerator != null) ? enumerator.GetNonEnumerableObject() : rhs, "op_Addition", null, "+");
        }

        internal static object AddObject(ExecutionContext context, IEnumerator lhs, object rhs)
        {
            NonEnumerableObjectEnumerator fakeEnumerator = lhs as NonEnumerableObjectEnumerator;
            if (fakeEnumerator != null)
            {
                return AddFakeEnumerable(fakeEnumerator, rhs);
            }
            ArrayList list = new ArrayList();
            while (MoveNext(context, lhs))
            {
                list.Add(Current(lhs));
            }
            list.Add(rhs);
            return list.ToArray();
        }

        internal static object Compare(IEnumerator enumerator, object valueToCompareTo, Func<object, object, bool> compareDelegate)
        {
            NonEnumerableObjectEnumerator enumerator2 = enumerator as NonEnumerableObjectEnumerator;
            if (enumerator2 != null)
            {
                if (!compareDelegate(enumerator2.GetNonEnumerableObject(), valueToCompareTo))
                {
                    return Boxed.False;
                }
                return Boxed.True;
            }
            ArrayList list = new ArrayList();
            while (MoveNext(null, enumerator))
            {
                object obj2 = Current(enumerator);
                if (compareDelegate(obj2, valueToCompareTo))
                {
                    list.Add(obj2);
                }
            }
            return list.ToArray();
        }

        internal static object Current(IEnumerator enumerator)
        {
            object current;
            try
            {
                current = enumerator.Current;
            }
            catch (RuntimeException)
            {
                throw;
            }
            catch (ScriptCallDepthException)
            {
                throw;
            }
            catch (FlowControlException)
            {
                throw;
            }
            catch (Exception exception)
            {
                CommandProcessorBase.CheckForSevereException(exception);
                throw InterpreterError.NewInterpreterExceptionWithInnerException(enumerator, typeof(RuntimeException), null, "BadEnumeration", ParserStrings.BadEnumeration, exception, new object[] { exception.Message });
            }
            return current;
        }

        private static void FlattenResults(object o, ArrayList result)
        {
            IEnumerator enumerator = LanguagePrimitives.GetEnumerator(o);
            if (enumerator != null)
            {
                while (enumerator.MoveNext())
                {
                    o = enumerator.Current;
                    if (o != AutomationNull.Value)
                    {
                        result.Add(o);
                    }
                }
            }
            else
            {
                result.Add(o);
            }
        }

        internal static IEnumerator GetCOMEnumerator(object obj)
        {
            try
            {
                IEnumerable enumerable = obj as IEnumerable;
                if (enumerable != null)
                {
                    IEnumerator enumerator = enumerable.GetEnumerator();
                    if (enumerator != null)
                    {
                        return enumerator;
                    }
                }
            }
            catch (Exception exception)
            {
                CommandProcessorBase.CheckForSevereException(exception);
            }
            return ((obj as IEnumerator) ?? NonEnumerableObjectEnumerator.Create(obj));
        }

        internal static IEnumerator GetEnumerator(IEnumerable enumerable)
        {
            IEnumerator enumerator;
            try
            {
                enumerator = enumerable.GetEnumerator();
            }
            catch (RuntimeException)
            {
                throw;
            }
            catch (Exception exception)
            {
                CommandProcessorBase.CheckForSevereException(exception);
                throw new ExtendedTypeSystemException("ExceptionInGetEnumerator", exception, ExtendedTypeSystem.EnumerationException, new object[] { exception.Message });
            }
            return enumerator;
        }

        internal static IEnumerator GetGenericEnumerator<T>(IEnumerable<T> enumerable)
        {
            IEnumerator enumerator;
            try
            {
                enumerator = enumerable.GetEnumerator();
            }
            catch (RuntimeException)
            {
                throw;
            }
            catch (Exception exception)
            {
                CommandProcessorBase.CheckForSevereException(exception);
                throw new ExtendedTypeSystemException("ExceptionInGetEnumerator", exception, ExtendedTypeSystem.EnumerationException, new object[] { exception.Message });
            }
            return enumerator;
        }

        internal static object[] GetSlice(IList list, int startIndex)
        {
            int num = list.Count - startIndex;
            object[] objArray = new object[num];
            int num2 = startIndex;
            int num3 = 0;
            while (num3 < num)
            {
                objArray[num3++] = list[num2++];
            }
            return objArray;
        }

        internal static object MethodInvoker(PSInvokeMemberBinder binder, Type delegateType, IEnumerator enumerator, object[] args, Type typeForMessage)
        {
            CallSite invokeMemberSite = CallSite.Create(delegateType, binder);
            ArrayList result = new ArrayList();
            ExecutionContext executionContextFromTLS = LocalPipeline.GetExecutionContextFromTLS();
            bool foundMethod = false;
            MethodInvokerWorker(invokeMemberSite, enumerator, args, executionContextFromTLS, result, ref foundMethod);
            if (result.Count == 1)
            {
                return result[0];
            }
            if (!foundMethod)
            {
                throw InterpreterError.NewInterpreterException(null, typeof(RuntimeException), null, "MethodNotFound", ParserStrings.MethodNotFound, new object[] { typeForMessage.FullName, binder.Name });
            }
            if (result.Count == 0)
            {
                return AutomationNull.Value;
            }
            return result.ToArray();
        }

        private static void MethodInvokerWorker(CallSite invokeMemberSite, IEnumerator enumerator, object[] args, ExecutionContext context, ArrayList result, ref bool foundMethod)
        {
            RuntimeHelpers.EnsureSufficientExecutionStack();
            while (MoveNext(context, enumerator))
            {
                object element = Current(enumerator);
                try
                {
                    dynamic obj3 = invokeMemberSite;
                    object o = obj3.Target.DynamicInvoke(args.Prepend<object>(element).Prepend<object>(invokeMemberSite).ToArray<object>());
                    foundMethod = true;
                    if (o != AutomationNull.Value)
                    {
                        FlattenResults(o, result);
                    }
                    continue;
                }
                catch (TargetInvocationException exception)
                {
                    RuntimeException innerException = exception.InnerException as RuntimeException;
                    if ((innerException != null) && innerException.ErrorRecord.FullyQualifiedErrorId.Equals("MethodNotFound", StringComparison.Ordinal))
                    {
                        IEnumerator enumerator2 = LanguagePrimitives.GetEnumerator(element);
                        if (enumerator2 != null)
                        {
                            MethodInvokerWorker(invokeMemberSite, enumerator2, args, context, result, ref foundMethod);
                            continue;
                        }
                    }
                    throw exception.InnerException;
                }
            }
        }

        internal static bool MoveNext(ExecutionContext context, IEnumerator enumerator)
        {
            bool flag;
            try
            {
                if ((context != null) && context.CurrentPipelineStopping)
                {
                    throw new PipelineStoppedException();
                }
                flag = enumerator.MoveNext();
            }
            catch (RuntimeException)
            {
                throw;
            }
            catch (FlowControlException)
            {
                throw;
            }
            catch (ScriptCallDepthException)
            {
                throw;
            }
            catch (Exception exception)
            {
                CommandProcessorBase.CheckForSevereException(exception);
                throw InterpreterError.NewInterpreterExceptionWithInnerException(enumerator, typeof(RuntimeException), null, "BadEnumeration", ParserStrings.BadEnumeration, exception, new object[] { exception.Message });
            }
            return flag;
        }

        internal static object Multiply(IEnumerator enumerator, int times)
        {
            NonEnumerableObjectEnumerator enumerator2 = enumerator as NonEnumerableObjectEnumerator;
            if (enumerator2 != null)
            {
                return ParserOps.ImplicitOp(enumerator2.GetNonEnumerableObject(), times, "op_Multiply", null, "*");
            }
            ArrayList list = new ArrayList();
            while (MoveNext(null, enumerator))
            {
                list.Add(Current(enumerator));
            }
            if (list.Count == 0)
            {
                return new object[0];
            }
            return ArrayOps.Multiply<object>(list.ToArray(), times);
        }

        internal static object PropertyGetter(PSGetMemberBinder binder, IEnumerator enumerator)
        {
            CallSite<Func<CallSite, object, object>> getMemberBinderSite = CallSite<Func<CallSite, object, object>>.Create(binder);
            ArrayList result = new ArrayList();
            ExecutionContext executionContextFromTLS = LocalPipeline.GetExecutionContextFromTLS();
            PropertyGetterWorker(getMemberBinderSite, enumerator, executionContextFromTLS, result);
            if (result.Count == 1)
            {
                return result[0];
            }
            if (result.Count != 0)
            {
                return result.ToArray();
            }
            if (executionContextFromTLS.IsStrictVersion(2))
            {
                throw InterpreterError.NewInterpreterException(null, typeof(RuntimeException), null, "PropertyNotFoundStrict", ParserStrings.PropertyNotFoundStrict, new object[] { binder.Name });
            }
            return null;
        }

        private static void PropertyGetterWorker(CallSite<Func<CallSite, object, object>> getMemberBinderSite, IEnumerator enumerator, ExecutionContext context, ArrayList result)
        {
            RuntimeHelpers.EnsureSufficientExecutionStack();
            while (MoveNext(context, enumerator))
            {
                object obj2 = Current(enumerator);
                object o = getMemberBinderSite.Target(getMemberBinderSite, obj2);
                if (o != AutomationNull.Value)
                {
                    FlattenResults(o, result);
                }
                else
                {
                    IEnumerator enumerator2 = LanguagePrimitives.GetEnumerator(obj2);
                    if (enumerator2 != null)
                    {
                        PropertyGetterWorker(getMemberBinderSite, enumerator2, context, result);
                    }
                }
            }
        }

        internal static object SlicingIndex(object target, IEnumerator indexes, Func<object, object, object> indexer)
        {
            NonEnumerableObjectEnumerator enumerator = indexes as NonEnumerableObjectEnumerator;
            if (enumerator != null)
            {
                return indexer(target, enumerator.GetNonEnumerableObject());
            }
            ArrayList list = new ArrayList();
            while (MoveNext(null, indexes))
            {
                object obj2 = indexer(target, Current(indexes));
                if (obj2 != AutomationNull.Value)
                {
                    list.Add(obj2);
                }
            }
            return list.ToArray();
        }

        internal static object[] ToArray(IEnumerator enumerator)
        {
            ArrayList list = new ArrayList();
            while (MoveNext(null, enumerator))
            {
                list.Add(Current(enumerator));
            }
            return list.ToArray();
        }

        internal static void WriteEnumerableToPipe(IEnumerator enumerator, Pipe pipe, ExecutionContext context, bool dispose)
        {
            try
            {
                while (MoveNext(context, enumerator))
                {
                    pipe.Add(Current(enumerator));
                }
            }
            finally
            {
                if (dispose)
                {
                    IDisposable disposable = enumerator as IDisposable;
                    if (disposable != null)
                    {
                        disposable.Dispose();
                    }
                }
            }
        }

        internal class NonEnumerableObjectEnumerator : IEnumerator
        {
            private object obj;
            private IEnumerator realEnumerator;

            internal static IEnumerator Create(object obj)
            {
                return new EnumerableOps.NonEnumerableObjectEnumerator { obj = obj, realEnumerator = new object[] { obj }.GetEnumerator() };
            }

            internal object GetNonEnumerableObject()
            {
                return this.obj;
            }

            bool IEnumerator.MoveNext()
            {
                return this.realEnumerator.MoveNext();
            }

            void IEnumerator.Reset()
            {
                this.realEnumerator.Reset();
            }

            object IEnumerator.Current
            {
                get
                {
                    return this.realEnumerator.Current;
                }
            }
        }
    }
}

