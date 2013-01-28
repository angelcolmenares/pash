namespace System.Management.Automation.Interpreter
{
    using System;
    using System.Linq.Expressions;
    using System.Runtime.CompilerServices;

    internal sealed class LightDelegateCreator
    {
        private Delegate _compiled;
        private Type _compiledDelegateType;
        private readonly object _compileLock = new object();
        private readonly System.Management.Automation.Interpreter.Interpreter _interpreter;
        private readonly Expression _lambda;

        internal LightDelegateCreator(System.Management.Automation.Interpreter.Interpreter interpreter, LambdaExpression lambda)
        {
            this._interpreter = interpreter;
            this._lambda = lambda;
        }

        internal void Compile(object state)
        {
            if (this._compiled == null)
            {
                lock (this._compileLock)
                {
                    if (this._compiled == null)
                    {
                        LambdaExpression lambda = this._lambda as LambdaExpression;
                        if (this._interpreter != null)
                        {
                            this._compiledDelegateType = GetFuncOrAction(lambda);
                            lambda = Expression.Lambda(this._compiledDelegateType, lambda.Body, lambda.Name, lambda.Parameters);
                        }
                        if (this.HasClosure)
                        {
                            this._compiled = LightLambdaClosureVisitor.BindLambda(lambda, this._interpreter.ClosureVariables);
                        }
                        else
                        {
                            this._compiled = lambda.Compile();
                        }
                    }
                }
            }
        }

        internal Delegate CreateCompiledDelegate(StrongBox<object>[] closure)
        {
            if (this.HasClosure)
            {
                Func<StrongBox<object>[], Delegate> func = (Func<StrongBox<object>[], Delegate>) this._compiled;
                return func(closure);
            }
            return this._compiled;
        }

        public Delegate CreateDelegate()
        {
            return this.CreateDelegate(null);
        }

        internal Delegate CreateDelegate(StrongBox<object>[] closure)
        {
            if ((this._compiled != null) && this.SameDelegateType)
            {
                return this.CreateCompiledDelegate(closure);
            }
            if (this._interpreter == null)
            {
                this.Compile(null);
                return this.CreateCompiledDelegate(closure);
            }
            return new LightLambda(this, closure, this._interpreter._compilationThreshold).MakeDelegate(this.DelegateType);
        }

        private static Type GetFuncOrAction(LambdaExpression lambda)
        {
            Type type;
            bool flag = lambda.ReturnType == typeof(void);
            Type[] typeArgs = lambda.Parameters.Map<ParameterExpression, Type>(delegate (ParameterExpression p) {
                if (!p.IsByRef)
                {
                    return p.Type;
                }
                return p.Type.MakeByRefType();
            });
            if (flag)
            {
                if (Expression.TryGetActionType(typeArgs, out type))
                {
                    return type;
                }
            }
            else if (Expression.TryGetFuncType(typeArgs.AddLast<Type>(lambda.ReturnType), out type))
            {
                return type;
            }
            return lambda.Type;
        }

        private Type DelegateType
        {
            get
            {
                LambdaExpression expression = this._lambda as LambdaExpression;
                if (expression != null)
                {
                    return expression.Type;
                }
                return null;
            }
        }

        private bool HasClosure
        {
            get
            {
                return ((this._interpreter != null) && (this._interpreter.ClosureSize > 0));
            }
        }

        internal bool HasCompiled
        {
            get
            {
                return (this._compiled != null);
            }
        }

        internal System.Management.Automation.Interpreter.Interpreter Interpreter
        {
            get
            {
                return this._interpreter;
            }
        }

        internal bool SameDelegateType
        {
            get
            {
                return (this._compiledDelegateType == this.DelegateType);
            }
        }
    }
}

