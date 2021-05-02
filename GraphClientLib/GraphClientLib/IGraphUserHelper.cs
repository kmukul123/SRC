using Microsoft.Graph;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GraphClientLib
{
    public interface IGraphUserHelper
    {
        Task<IGraphServiceUsersCollectionPage> GetUsersPagedAsync();
    }
}