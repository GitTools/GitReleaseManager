namespace GitReleaseManager.Core.Commands
{
    using System.Threading.Tasks;
    using GitReleaseManager.Core.Options;

    public interface ICommand<TOptions>
        where TOptions : BaseSubOptions
    {
        Task<int> Execute(TOptions options);
    }
}