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
        private string _content;

        public TestTaskItem(string content)
        {
            _content = content;
        }

        public IDictionary CloneCustomMetadata()
        {
            throw new NotImplementedException();
        }

        public void CopyMetadataTo(ITaskItem destinationItem)
        {
            throw new NotImplementedException();
        }

        public string GetMetadata(string metadataName)
        {
            throw new NotImplementedException();
        }

        public string ItemSpec
        {
            get
            {
                return _content;
            }
            set
            {
                _content = value;
            }
        }

        public int MetadataCount
        {
            get { throw new NotImplementedException(); }
        }

        public ICollection MetadataNames
        {
            get { throw new NotImplementedException(); }
        }

        public void RemoveMetadata(string metadataName)
        {
            throw new NotImplementedException();
        }

        public void SetMetadata(string metadataName, string metadataValue)
        {
            throw new NotImplementedException();
        }
    }
}
