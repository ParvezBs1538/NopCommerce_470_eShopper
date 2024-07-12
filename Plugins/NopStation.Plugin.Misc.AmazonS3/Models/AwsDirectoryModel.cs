using System.Collections.Generic;

namespace NopStation.Plugin.Misc.AmazonS3.Models
{
    public class AwsDirectoryModel
    {
        public AwsDirectoryModel()
        {
            ChildDirectories = new List<AwsDirectoryModel>();
        }

        public string Name { get; set; }

        public long Count { get; set; }

        public IList<AwsDirectoryModel> ChildDirectories { get; set; }
    }
}
