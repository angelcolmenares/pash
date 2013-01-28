namespace System.Data.Services.Parsing
{
    using System;

    internal static class OperationSignatures
    {
        internal interface IAddSignatures : OperationSignatures.IArithmeticSignatures
        {
        }

        internal interface IArithmeticSignatures
        {
            void F(decimal? x, decimal? y);
            void F(double? x, double? y);
            void F(int? x, int? y);
            void F(long? x, long? y);
            void F(float? x, float? y);
            void F(decimal x, decimal y);
            void F(double x, double y);
            void F(int x, int y);
            void F(long x, long y);
            void F(object x, object y);
            void F(float x, float y);
        }

        internal interface IEnumerableSignatures
        {
            void All(bool predicate);
            void Any();
            void Any(bool predicate);
            void Average(decimal? selector);
            void Average(double? selector);
            void Average(decimal selector);
            void Average(int? selector);
            void Average(long? selector);
            void Average(float? selector);
            void Average(double selector);
            void Average(int selector);
            void Average(long selector);
            void Average(float selector);
            void Count();
            void Count(bool predicate);
            void Max(object selector);
            void Min(object selector);
            void Sum(decimal? selector);
            void Sum(decimal selector);
            void Sum(double selector);
            void Sum(int selector);
            void Sum(long selector);
            void Sum(double? selector);
            void Sum(int? selector);
            void Sum(long? selector);
            void Sum(float? selector);
            void Sum(float selector);
            void Where(bool predicate);
        }

        internal interface IEqualitySignatures : OperationSignatures.IRelationalSignatures, OperationSignatures.IArithmeticSignatures
        {
        }

        internal interface ILogicalSignatures
        {
            void F(bool x, bool y);
            void F(bool? x, bool? y);
            void F(object x, object y);
        }

        internal interface INegationSignatures
        {
            void F(decimal? x);
            void F(double? x);
            void F(int? x);
            void F(long? x);
            void F(float? x);
            void F(decimal x);
            void F(double x);
            void F(int x);
            void F(long x);
            void F(object x);
            void F(float x);
        }

        internal interface INotSignatures
        {
            void F(bool x);
            void F(bool? x);
            void F(object x);
        }

        internal interface IRelationalSignatures : OperationSignatures.IArithmeticSignatures
        {
            void F(bool? x, bool? y);
            void F(char? x, char? y);
            void F(DateTime? x, DateTime? y);
            void F(DateTimeOffset? x, DateTimeOffset? y);
            void F(Guid? x, Guid? y);
            void F(TimeSpan? x, TimeSpan? y);
            void F(bool x, bool y);
            void F(char x, char y);
            void F(DateTime x, DateTime y);
            void F(DateTimeOffset x, DateTimeOffset y);
            void F(Guid x, Guid y);
            void F(string x, string y);
            void F(TimeSpan x, TimeSpan y);
        }

        internal interface ISubtractSignatures : OperationSignatures.IAddSignatures, OperationSignatures.IArithmeticSignatures
        {
        }
    }
}

