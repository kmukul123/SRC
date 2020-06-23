using Microsoft.Graph;
using System;
using System.Threading.Tasks;

namespace GraphClientLib
{
    public interface IGraphDriveHelper
    {
        Task<bool> ProcessAllDriveItems();
        Func<string, Task> ProcessToken { get; set; }
        Func<DriveItem, Task> ProcessDriveChange { get; set; }

    }
}