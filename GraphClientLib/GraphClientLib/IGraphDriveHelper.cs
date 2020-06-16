using Microsoft.Graph;
using System;
using System.Threading.Tasks;

namespace GraphClientLib
{
    internal interface IGraphDriveHelper
    {
        Task<bool> ProcessAllDriveItems();
        Func<string, Task> ProcessToken { get; set; }
        Func<DriveItem, Task> ProcessDriveChange { get; set; }

    }
}