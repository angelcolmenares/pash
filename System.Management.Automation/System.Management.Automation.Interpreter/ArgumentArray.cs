namespace System.Management.Automation.Interpreter
{
    using System;
    using System.Dynamic;
    using System.Linq.Expressions;
    using System.Reflection;

    internal sealed class ArgumentArray
    {
        private readonly object[] _arguments;
        private readonly int _count;
        private readonly int _first;
        private static readonly MethodInfo _GetArgMethod = new Func<ArgumentArray, int, object>(ArgumentArray.GetArg).Method;

        internal ArgumentArray(object[] arguments, int first, int count)
        {
            this._arguments = arguments;
            this._first = first;
            this._count = count;
        }

        public static object GetArg(ArgumentArray array, int index)
        {
            return array._arguments[array._first + index];
        }

        public object GetArgument(int index)
        {
            return this._arguments[this._first + index];
        }

        public DynamicMetaObject GetMetaObject(Expression parameter, int index)
        {
            return DynamicMetaObject.Create(this.GetArgument(index), Expression.Call(_GetArgMethod, Utils.Convert(parameter, typeof(ArgumentArray)), Utils.Constant(index)));
        }

        public int Count
        {
            get
            {
                return this._count;
            }
        }
    }
}

