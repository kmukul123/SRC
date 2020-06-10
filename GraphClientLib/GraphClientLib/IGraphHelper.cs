using Microsoft.Graph;
using System.Threading.Tasks;

namespace GraphClientLib
{
    public interface IGraphHelper
    {
        Task<IGraphServiceUsersCollectionPage> GetUsersPagedAsync();
    }
}