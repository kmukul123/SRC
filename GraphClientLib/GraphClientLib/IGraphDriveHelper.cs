using Microsoft.Graph;
using System;
using System.IO;
using System.Threading.Tasks;

namespace GraphClientLib
{
    public interface IGraphDriveHelper
    {
        /// <summary>
        /// function to process all drive items one by one. 
        /// stores the token so that it can be called again 
        /// in that case would process the new changes from last time
        /// todo: not thread safes
        /// </summary>
        /// <returns></returns>
        Task<int> ProcessAllDriveItems();
        Func<string, Task> ProcessToken { get; set; }
        Func<DriveItem, Task> ProcessDriveChange { get; set; }

        Task<DriveItem> UploadSmallFile(DriveItem parent, Stream stream, string fileName);
    }
}