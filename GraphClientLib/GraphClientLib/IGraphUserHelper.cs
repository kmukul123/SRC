using Microsoft.Graph;
using System.Threading.Tasks;

namespace GraphClientLib
{
    public interface IGraphUserHelper
    {
        Task<IGraphServiceUsersCollectionPage> GetUsersPagedAsync();
    }
}