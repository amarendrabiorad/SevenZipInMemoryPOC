using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SevenZip;
using System.IO;

namespace SevenZipPOC
{
    public class CompressionHelper : SevenZipOperation
    {
        SevenZipCompressor compressor;
        public CompressionHelper()
        {
            compressor = new SevenZipCompressor();
        }
        internal void ZipFolder(string folder,
                              string zipFileName,
                              string fileID,
                              bool noCompression = false)
        {
            if (!noCompression)
            {
                compressor.ArchiveFormat = OutArchiveFormat.SevenZip;
                compressor.CompressionMethod = CompressionMethod.Lzma;
                compressor.CompressionLevel = CompressionLevel.Fast;
                compressor.EncryptHeaders = false;
                compressor.ZipEncryptionMethod = ZipEncryptionMethod.Aes256;
            }
            else
            {
                compressor.ArchiveFormat = OutArchiveFormat.SevenZip;
                compressor.CompressionMethod = CompressionMethod.Copy;
                compressor.CompressionLevel = CompressionLevel.None;
            }
            compressor.CompressDirectory(folder, zipFileName, fileID);
        }

        internal void DeleteFiles(string zipFileName, string filePattern, string fileID)
        {
            using (var ext = new SevenZipExtractor(zipFileName, fileID))
            {
                Dictionary<int, string> toDelete = new Dictionary<int, string>();
                foreach (var file in ext.ArchiveFileData)
                {
                    if (new FileInfo(file.FileName).Extension == filePattern)
                        toDelete.Add(file.Index, null);
                }
                compressor.ModifyArchive(zipFileName, toDelete, fileID);
            }
        }

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

        /// <summary>
        /// Function to Update file path or Delete a file from archive file.
        /// </summary>
        /// <param name="zipFileName"></param>
        /// <param name="fileName"></param>
        /// <param name="newFileName"></param>
        /// <param name="fileID"></param>
        internal void ModifyDeleteFile(string zipFileName, string fileName, string newFileName, string fileID)
        {
            using (var ext = new SevenZipExtractor(zipFileName, fileID))
            {
                Dictionary<int, string> toModify = new Dictionary<int, string>();
                foreach (var file in ext.ArchiveFileData)
                {
                    if (file.FileName == fileName)
                        toModify.Add(file.Index, !string.IsNullOrEmpty(newFileName) ? newFileName : null);
                }
                if (toModify.Count > 0)
                    compressor.ModifyArchive(zipFileName, toModify, fileID);
            }
        }

        internal void UpdateZip(string zipFileName,
                              string fileToBeUpdated,
                              string dataRootDirectory,
                              string fileID)
        {
            AddFileToZip(zipFileName,
                         fileToBeUpdated,
                         dataRootDirectory,
                         fileID);
        }
        internal void UpdateZipWithFolder(string zipFileName, string sourceDirectory, string dataRootDirectory, string fileID)
        {
            // Create Folder in Zip file.
            string dirName = new DirectoryInfo(sourceDirectory).Name;
            AddFileToZip(zipFileName, null, Path.Combine(dataRootDirectory, dirName), fileID);

            // Add files to the folder.
            foreach (string file in Directory.GetFiles(sourceDirectory))
            {
                AddFileToZip(zipFileName, file, Path.Combine(dataRootDirectory, dirName), fileID);
            }
        }
    }
}
