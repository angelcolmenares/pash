namespace System.Data.Services.Providers
{
    using System.Linq;

    internal interface IProjectionProvider
    {
        IQueryable ApplyProjections(IQueryable source, RootProjectionNode rootProjectionNode);
    }
}

