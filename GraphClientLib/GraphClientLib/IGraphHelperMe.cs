using Microsoft.Graph;
using System.Threading.Tasks;

namespace GraphClientLib.GraphTutorial
{
    public interface IGraphHelperMe
    {
        Task<User> GetMeAsync();
    }
}