using Microsoft.Build.Framework;
using System;
using System.Collections;

namespace Codartis.NsDepCop.MsBuildTask.Test
{
    /// <summary>
    /// An implementation of the MSBuild TaskItem used for unit testing.
    /// </summary>
    internal class TestTaskItem : ITaskItem
    {
        public TestTaskItem(string content)
        {
            ItemSpec = content;
        }

        public string ItemSpec { get; set; }

        public IDictionary CloneCustomMetadata() => throw new NotImplementedException();
        public void CopyMetadataTo(ITaskItem destinationItem) => throw new NotImplementedException();
        public string GetMetadata(string metadataName) => throw new NotImplementedException();
        public int MetadataCount => throw new NotImplementedException();
        public ICollection MetadataNames => throw new NotImplementedException();
        public void RemoveMetadata(string metadataName) => throw new NotImplementedException();
        public void SetMetadata(string metadataName, string metadataValue) => throw new NotImplementedException();
    }
}
