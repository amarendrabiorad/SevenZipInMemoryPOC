using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SevenZip;
using System.IO;

namespace SevenZipPOC
{
    class ExtractionHelper : SevenZipOperation
    {
        // Extract entire content from zip file to a folder.
        internal bool ExtractZip(string zipFileName, string destFolder, string fileID)
        {
            using (var ext = new SevenZipExtractor(zipFileName, fileID))
            {
                try
                {
                    ext.ExtractArchive(destFolder);
                    return true;
                }

                catch (Exception)
                {
                    return false;
                }
            }
        }

        private bool ExtractZipExt(string archivePath, string destFolder, List<string> fileIDs)
        {
            foreach (var fileID in fileIDs)
            {
                if (ExtractZip(archivePath, destFolder, fileID))
                {
                    return true;
                }
            }
            return false;
        }

        private bool ExtractZipFile(string archivePath, string filePath, string destFolder, string fileID)
        {
            using (var ext = new SevenZipExtractor(archivePath, fileID))
            {
                try
                {
                    ext.ExtractFiles(destFolder, filePath);
                    return true;
                }

                catch (Exception)
                {
                    return false;
                }
            }
        }
        internal bool ExtractZipFiles(string zipFileName, string destFolder, string fileID, string filePattern)
        {
            using (var ext = new SevenZipExtractor(zipFileName, fileID))
            {
                foreach (var file in ext.ArchiveFileData)
                {
                    if (new FileInfo(file.FileName).Extension == filePattern)
                        ExtractZipFile(zipFileName, file.FileName, destFolder, fileID);
                }
                return true;
            }
        }
    }
}
