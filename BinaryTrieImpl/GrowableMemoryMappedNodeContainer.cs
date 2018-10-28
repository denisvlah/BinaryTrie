using System;
using System.Linq;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace BinaryTrieImpl
{
    public class GrowableMemoryMappedNodeContainer<T> : BaseGrowableNodeContainer<T, MemoryMappedNodeContainer<T>> where T: struct
    {
        private readonly string _basePath;
        private readonly string _fileTemplate;
        public GrowableMemoryMappedNodeContainer(string basePath = null, string fileTemplate = null, int size = 100000)
        {
            basePath = basePath ?? ".";
            fileTemplate = fileTemplate ?? "data_{0}.bin";

            if (string.IsNullOrWhiteSpace(basePath))
            {
                throw new ArgumentException(nameof(basePath));
            }

            if (string.IsNullOrWhiteSpace(fileTemplate))
            {
                throw new ArgumentException(nameof(fileTemplate));
            }

            if (fileTemplate.Contains("{0}") == false)
            {
                throw new ArgumentException(nameof(fileTemplate));
            }

            _basePath = basePath;
            _fileTemplate = fileTemplate;

            var dirInfo = Directory.CreateDirectory(_basePath);

            var allFileNames = dirInfo.EnumerateFiles().ToList();

            var pattern = "^" + _fileTemplate.Replace("{0}", "(//d+)") + "$";
            var parsedFileNames = new HashSet<string>(allFileNames.Select(f=>f.FullName));                                    
                                    
            
            if (parsedFileNames.Count > 0)
            {
                var sortedFileNames = new List<string>(allFileNames.Count);
                for (var i=0; i< parsedFileNames.Count; i++)
                {
                    var expectedFileName = string.Format(_fileTemplate, i);

                    if (parsedFileNames.Contains(expectedFileName))
                    {
                        throw new ArgumentException($"Missing file ${expectedFileName}");
                    }

                    sortedFileNames.Add(expectedFileName);
                }

                foreach(var fileName in sortedFileNames)
                {
                    var nodeContainer = new MemoryMappedNodeContainer<T>(fileName, _size);
                    _data.Add(nodeContainer);
                }
            }
            else
            {
                var fileName = string.Format(_fileTemplate, 0);
                var filePath = Path.Combine(_basePath, fileName);

                var nodeContainer = new MemoryMappedNodeContainer<T>(filePath, _size);
                _data.Add(nodeContainer);
            }
        }

        protected override MemoryMappedNodeContainer<T> CreateNodeContainer()
        {
            var fileName = string.Format(_fileTemplate, _data.Count);
            var filePath = Path.Combine(_basePath, fileName);

            return new MemoryMappedNodeContainer<T>(filePath, _size);
        }
    }
}