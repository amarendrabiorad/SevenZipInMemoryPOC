using SevenZip;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SevenZipPOC
{
    public class InMemoryFileOperations : SevenZipOperation
    {
        SevenZipCompressor compressor;
        public InMemoryFileOperations()
        {
            compressor = new SevenZipCompressor();
        }

        /// <summary>
        /// Add a file to in memory zip archieve
        /// </summary>
        /// <param name="zipFileName"></param>
        /// <param name="fileToBeAdded"></param>
        /// <param name="dataRootDirectory"></param>
        /// <param name="fileID"></param>
        internal void AddFileToZip(string zipFileName,
                              string fileToBeAdded,
                              string dataRootDirectory,
                              string fileID)
        {
            FileInfo fs;
            Dictionary<string, string> fileDict = new Dictionary<string, string>();
            compressor.CompressionMode = CompressionMode.Append;
            if (!string.IsNullOrEmpty(fileToBeAdded))
            {
                fs = new FileInfo(fileToBeAdded);
                fileDict.Add(Path.Combine(dataRootDirectory, fs.Name), fileToBeAdded);
            }
            else
                fileDict.Add(dataRootDirectory, fileToBeAdded);
            compressor.CompressFileDictionary(fileDict, zipFileName, fileID);
        }

        //TO DO : For Now, Only Direct file from zip can be deleted, File within Folder inside zip cannot be deleted
        /// <summary>
        /// Deletes files from in memory zip archieve.
        /// </summary>
        /// <param name="zipFileName"></param>
        /// <param name="fileNametoDelete"></param>
        /// <param name="dataRootDirectory"></param>
        /// <param name="fileID"></param>
        internal void DeleteFileFromZip(string zipFileName, string fileNametoDelete, string dataRootDirectory, string fileID)
        {          
            using (var ext = new SevenZipExtractor(zipFileName, fileID))
            {
                Dictionary<int, string> toDelete = new Dictionary<int, string>();
                foreach (var file in ext.ArchiveFileData)
                {
                    if (file.FileName == fileNametoDelete)
                        toDelete.Add(file.Index, null);
                }
                compressor.ModifyArchive(zipFileName, toDelete, fileID);
            }
        }

        /// <summary>
        /// ReplaceFileContents from in memory zip archieve.
        /// </summary>
        /// <param name="zipFileName"></param>
        /// <param name="fileNameToReplace"></param>
        /// <param name="fileID"></param>
        internal void ReplaceFileContent(string zipFileName, string fileName, string fileNameToReplace, string fileID)
        {
            Dictionary<string, Stream> toAdd = new Dictionary<string, Stream>();
            Dictionary<int, string> toRemove = new Dictionary<int, string>();
            int index;
            byte[] content = System.IO.File.ReadAllBytes(fileNameToReplace);
            using (var ext = new SevenZipExtractor(zipFileName, fileID))
            {
                index = ext.ArchiveFileData.FirstOrDefault(x => x.FileName == fileName).Index;
            }
            using (MemoryStream outStream = new MemoryStream())
            {
                outStream.Write(content, 0, content.Length);
                outStream.Seek(0, SeekOrigin.Begin);
                toAdd.Add(fileName, outStream);
                toRemove.Add(index, null);
                compressor.ModifyArchive(zipFileName, toRemove, fileID);
                compressor.CompressionMode = CompressionMode.Append;
                compressor.CompressStreamDictionary(toAdd, zipFileName, fileID);
            }
        }

        /// <summary>
        /// AppendFileContent to in memory zip archieve.
        /// </summary>
        /// <param name="zipFileName"></param>
        /// <param name="fileNameToAppendContent"></param>
        /// <param name="dataRootDirectory"></param>
        /// <param name="fileID"></param>
        internal void AppendFileContent(string zipFileName, string fileName, string contentAppend,string fileID)
        {
            Dictionary<string, Stream> toAdd = new Dictionary<string, Stream>();
            Dictionary<int, string> toRemove = new Dictionary<int, string>();
            int index;
            byte[] content = Encoding.ASCII.GetBytes(contentAppend);
            using (MemoryStream outStream = new MemoryStream())
            {
                using (var ext = new SevenZipExtractor(zipFileName, fileID))
                {
                    index = ext.ArchiveFileData.FirstOrDefault(x => x.FileName == fileName).Index;
                    ext.ExtractFile(index, outStream);
                }
                outStream.Seek(0, SeekOrigin.Begin);
                using (MemoryStream outStream1 = new MemoryStream())
                {
                    outStream1.Write(outStream.ToArray(), 0, outStream.ToArray().Length);
                    outStream1.Seek(0, SeekOrigin.End); //move to end of the stream for appending
                    outStream1.Write(content, 0, content.Length);
                    outStream1.Seek(0, SeekOrigin.Begin);
                    toAdd.Add(fileName, outStream1);
                    toRemove.Add(index, null);
                    compressor.ModifyArchive(zipFileName, toRemove, fileID);
                    compressor.CompressionMode = CompressionMode.Append;
                    compressor.CompressStreamDictionary(toAdd, zipFileName, fileID);
                }
            }
        }

        /// <summary>
        /// RenameFile to in memory zip archieve.
        /// </summary>
        /// <param name="zipFileName"></param>
        /// <param name="currentFileName"></param>
        /// <param name="newFileName"></param>
        /// <param name="fileID"></param>
        internal void RenameFile(string zipFileName, string currentFileName, string newFileName, string fileID)
        {
            Dictionary<int, string> filesToRename = new Dictionary<int, string>();
            using (SevenZipExtractor ext = new SevenZipExtractor(zipFileName, fileID))
            {
                var fileData = ext.ArchiveFileData.FirstOrDefault(a=>a.FileName == currentFileName);
                filesToRename.Add(fileData.Index, newFileName);
            }
            compressor.ModifyArchive(zipFileName, filesToRename, fileID);
        }

        internal List<string> GetZipItems(string zipFileName, string password)
        {
            List<string> items= new List<string>();
            using (var ext = new SevenZipExtractor(zipFileName, password))
            {
                Dictionary<int, string> toModify = new Dictionary<int, string>();
                foreach (var file in ext.ArchiveFileData)
                {
                    items.Add(file.FileName);
                }
            }
            return items;
        }

        internal void DeleteFile(string zipFileName, string fileNameToDelete, string fileID)
        {
            using (var ext = new SevenZipExtractor(zipFileName, fileID))
            {
                Dictionary<int, string> toDelete = new Dictionary<int, string>();
                foreach (var file in ext.ArchiveFileData)
                {
                    if (file.FileName== fileNameToDelete)
                        toDelete.Add(file.Index, null);
                }
                compressor.ModifyArchive(zipFileName, toDelete, fileID);
            }
        }
    }
}