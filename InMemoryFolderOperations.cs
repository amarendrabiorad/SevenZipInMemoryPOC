using SevenZip;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
//using FileOps;


namespace SevenZipPOC
{
    internal class InMemoryFolderOperations
    {
        CompressionHelper compress;
        SevenZipCompressor compressor;
        public InMemoryFolderOperations()
        {
            compress = new CompressionHelper();
            compressor = new SevenZipCompressor();
        }

        /// <summary>
        /// Method to add a new folder to an existing archive file.
        /// </summary>
        /// <param name="zipFileName">Path of archive file on disk</param>
        /// <param name="folderToAdd">Path of folder on disk</param>
        /// <param name="dataRootDirectory">Relative path inside archive</param>
        /// <param name="fileID">Password</param>
        public void AddFolderToArchive(string zipFileName,
                              string folderToAdd,
                              string dataRootDirectory,
                              string fileID)
        {
            // Create Folder in Zip file.
            var fileIO = new InMemoryFileOperations();
            string dirName = new DirectoryInfo(folderToAdd).Name;
            fileIO.AddFileToZip(zipFileName, null, Path.Combine(dataRootDirectory, dirName), fileID);

            // Add files to the folder.
            foreach (string file in Directory.GetFiles(folderToAdd))
            {
                fileIO.AddFileToZip(zipFileName, file, Path.Combine(dataRootDirectory, dirName), fileID);
            }
        }

        /// <summary>
        /// Function to traverse through folder and files.
        /// </summary>
        /// <param name="zipFileName"></param>
        /// <param name="fileName"></param>
        /// <param name="newFileName"></param>
        /// <param name="fileID"></param>
        internal List<ArchiveFileInfo> TraverseFolderinArchive(string zipFileName, string fileID)
        {
            using (var files = new SevenZipExtractor(zipFileName, fileID))
            {
                return files.ArchiveFileData.ToList();
            }
        }

        internal void RenameFolder(string zipFileName, string[] currentFolderNames, string[] newFolderNames, string fileID)
        {
            Dictionary<string, string> filesToModify = new Dictionary<string, string>();
            SevenZipExtractor ext;
            Stream fileStream = new MemoryStream();
            string OutputDirectory = @"D:\Logs"; //TODO
            for (int i = 0; i < currentFolderNames.Length; i++)
            {
                using (ext = new SevenZipExtractor(zipFileName, fileID))
                {
                    foreach (var file in ext.ArchiveFileData.Where(s => s.FileName.Contains(currentFolderNames[i] + "\\")))
                    {
                        FileInfo fs = new FileInfo(file.FileName);
                        using (fileStream = new FileStream(Path.Combine(OutputDirectory, fs.Name), FileMode.Create))
                        {
                            ext.ExtractFile(file.Index, fileStream);
                            filesToModify.Add(Path.Combine(newFolderNames[i], fs.Name), Path.Combine(OutputDirectory, fs.Name));
                        }
                    }
                }
                compressor.CompressionMode = CompressionMode.Append;
                compressor.CompressFileDictionary(filesToModify, zipFileName, fileID);
                DeleteFolder(zipFileName, new string[] { currentFolderNames[i] }, fileID);
            }
        }


        internal void DeleteFolder(string zipFileName, string[] folderNames, string fileID)
        {
            Dictionary<int, string> filesToDelete = new Dictionary<int, string>();
            Dictionary<int, string> foldersToDelete = new Dictionary<int, string>();
            SevenZipExtractor ext;
            using (ext = new SevenZipExtractor(zipFileName, fileID))
            {
                foreach (string folder in folderNames)
                {
                    GetFilesToDelete(folder);
                    foldersToDelete.Add(ext.ArchiveFileData.FirstOrDefault(a => a.FileName == folder).Index, null);
                    compressor.ModifyArchive(zipFileName, filesToDelete, fileID);
                    compressor.ModifyArchive(zipFileName, foldersToDelete, fileID);
                }
            }

            void GetFilesToDelete(string folderName)
            {
                foreach (var file in ext.ArchiveFileData)
                {
                    if (!file.IsDirectory && file.FileName.Contains(folderName + "\\"))
                        filesToDelete.Add(file.Index, null);

                    else if (file.IsDirectory && file.FileName.Contains(folderName + "\\"))
                        foldersToDelete.Add(file.Index, null);
                }
            }
        }

        internal void AppendToFile(string zipFileName, string fileName, string content, string fileID)
        {
            MemoryStream fileStream = new MemoryStream();
            Dictionary<string, Stream> toAdd = new Dictionary<string, Stream>();
            Dictionary<int, string> toRemove = new Dictionary<int, string>();
            int index;
            using (MemoryStream outStream = new MemoryStream())
            {
                using (var ext = new SevenZipExtractor(zipFileName, fileID))
                {
                    index = ext.ArchiveFileData.FirstOrDefault(x => x.FileName == fileName).Index;
                    ext.ExtractFile(index, outStream);
                }
                //SevenZipExtractor.DecompressStream(outStream, outStream1, 40, null);
                outStream.Seek(0, SeekOrigin.Begin);
                using (MemoryStream outStream1 = new MemoryStream())
                {
                    outStream1.Write(outStream.ToArray(), 0, outStream.ToArray().Length);
                    outStream1.Seek(0, SeekOrigin.End); //move to end of the stream for appending
                    outStream1.Write(Encoding.ASCII.GetBytes(content), 0, Encoding.ASCII.GetBytes(content).Length);
                    outStream1.Seek(0, SeekOrigin.Begin);
                    toAdd.Add(fileName, outStream1);
                    toRemove.Add(index, null);
                    compressor.ModifyArchive(zipFileName, toRemove, fileID);
                    compressor.CompressionMode = CompressionMode.Append;
                    compressor.CompressStreamDictionary(toAdd, zipFileName, fileID);
                }
            }
        }

        MemoryStream WriteBytes(byte[] newBytesToWrite, byte[] currentFileData, long position)
        {
            MemoryStream s2 = new MemoryStream();
            s2.Write(currentFileData, 0, currentFileData.Length);
            s2.Seek(0, SeekOrigin.End); //move to end of the stream for appending
            s2.Write(newBytesToWrite, 0, newBytesToWrite.Length);
            return s2;
        }
    }
}
