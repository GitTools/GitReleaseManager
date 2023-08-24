using System.Threading.Tasks;
using GitReleaseManager.Core.Options;

namespace GitReleaseManager.Core.Commands
{
    public interface ICommand<TOptions>
        where TOptions : BaseSubOptions
    {
        Task<int> ExecuteAsync(TOptions options);
    }
}