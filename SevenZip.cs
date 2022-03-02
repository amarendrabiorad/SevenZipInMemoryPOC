using SevenZip;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SevenZipPOC
{
    public partial class SevenZip : Form
    {
        private InMemoryFileOperations compressionExtendedHelper;
        public SevenZip()
        {
            InitializeComponent();            
            Initialize();
        }

        private void Initialize()
        {
            lblStatus.Visible = false;
            rdAddFile.Checked=true;
            rdAddFolder.Checked = true;
            chkEncryption.Checked=true;
            lblStatusExtact.Visible=false;
            lblStatusCompress.Visible = false;
            txtPasswordFile.Text= "1b53402e-503a-4303-bf86-71af1f3178dd";
            txtPwdCompress.Text = "1b53402e-503a-4303-bf86-71af1f3178dd";
            txtPwdExtract.Text = "1b53402e-503a-4303-bf86-71af1f3178dd";
            txtPasswordFolder.Text = "1b53402e-503a-4303-bf86-71af1f3178dd";
            SetAddFileInformation();
        }

        #region "File Events - Zip Operations"
        private void rd_CheckedChanged(object sender, EventArgs e)
        {
            txtInformation.Text = string.Empty;
            txtFileName.Text = string.Empty;
            txtDirectoryPath.Text = string.Empty;
            txtInformation.Text=SetFileInformation();
            lblFileName.Text= SetFileName();
            if (rdRenameFile.Checked)
            {
                lblNewFileName.Text = "New File Name:";
                btnFileName.Visible = false;
            }
            else if (rdAppendContent.Checked)
            {
                lblNewFileName.Text = "Content To Append:";
                btnFileName.Visible = false;
            }
            else if (rdReplaceContent.Checked)
            {
                btnFileName.Visible = true;
                lblNewFileName.Text = "File To be Replaced :";
            }
            else
            {
                lblNewFileName.Text = "Directory Name:";
                btnFileName.Visible = true;
            }
        }
        private void chkEncryption_CheckedChanged(object sender, EventArgs e)
        {
            if(chkEncryption.Checked)
                txtPasswordFile.Enabled=true;
            else
                txtPasswordFile.Enabled = false;
                txtPasswordFile.Text=string.Empty;
        }  
        private void btnBrowseZipFile_Click(object sender, EventArgs e)
        {
            var openDialog = BrowseFile();
            if (openDialog.ShowDialog() == DialogResult.OK)
            {
                txtZipFileNameFile.Text = openDialog.FileName;
                lstZipItems.DataSource = FillZipItems(txtZipFileNameFile.Text,txtPasswordFile.Text);
            }
        }     
        private void btnFileName_Click(object sender, EventArgs e)
        {
            var openDialog = BrowseFile();
            if (openDialog.ShowDialog() == DialogResult.OK)
            {
                txtFileName.Text = openDialog.FileName;
            }
        }
        private void btnOk_Click(object sender, EventArgs e)
        {
           var result= ValidateFileOperationUI();
            if(result.Item2)
            { 
                ExecuteFileOperation();
                lstZipItems.DataSource = FillZipItems(txtZipFileNameFile.Text, txtPasswordFile.Text);  
             }
            else
            {
                MessageBox.Show(result.Item1, "Seven Zip Operations", MessageBoxButtons.OK);
            }
        }

        #endregion

        #region "Private Methods - File Zip Operations"
        private string SetFileName()
        {
            btnFileName.Visible = true;
            txtDirectoryPath.Enabled = false;
            if (rdAddFile.Checked)
            {
                txtDirectoryPath.Enabled = true;
                return "File Name to Add :";
            }
            else if (rdDeleteFile.Checked)
            {
                btnFileName.Visible = false;
                return "File Name to Delete :";
            }
            if (rdRenameFile.Checked)
            {
                txtDirectoryPath.Enabled = true;
                return "File Name :";
            }
            else if (rdReplaceContent.Checked)
            {
                txtDirectoryPath.Enabled = true;
                return "File Name(Reffer Content):";
            }
            else if (rdAppendContent.Checked)
            {
                txtDirectoryPath.Enabled = true;
                return "File Name :";
            }
            else
            {
                btnFileName.Visible = true;
                return "File Name :";
            }

        }
        private string SetFileInformation()
        {
            if (rdAddFile.Checked)
                return SetAddFileInformation();
            else if (rdDeleteFile.Checked)
                return SetDeleteFileInformation();
            else if (rdAppendContent.Checked)
                return SetAppendContentFileInformation();
            else if (rdReplaceContent.Checked)
                return SetReplaceFileContentsInformation();
            else if(rdRenameFile.Checked)
                return SetRenameFileInformation();
            else
                return string.Empty;
        }
        private string SetAddFileInformation()
        {
            string strInfo = "Adds a file (provided file name) to selected zip archieve.@* Provide Zip file name in which file needs to be added.@* Provide File Name to be added.@* (Optional) Provide Directory Name in which file needs to be added. (If Provided file will be added in Directory)";
            strInfo = strInfo.Replace("@", System.Environment.NewLine);
            return strInfo;
        }
        private string SetDeleteFileInformation()
        {
            string strInfo = "Deletes a file (provided file name) from selected zip archieve.@* Provide Zip file name from which file needs to be deleted.@* Provide File Name to be deleted. (If File which needs to be deleted is in folder, please provide folder name including file name i.e FolderName\\FileNameToDelete.";
            strInfo = strInfo.Replace("@", System.Environment.NewLine);
            return strInfo;
        }
        private string SetAppendContentFileInformation()
        {
            string strInfo = "Appends contents of the provided file into the selected zip archieve.@* Provide Zip file name from which contents need to be appended.@* Provide File Name to be updated.@* Provide content to be appended in the file.";
            strInfo = strInfo.Replace("@", System.Environment.NewLine);
            return strInfo;
        }
        private string SetReplaceFileContentsInformation()
        {
            string strInfo = "Replace the file content completely from selected zip folder.@* Provide File Name to be reffered from which contents need to be updated.@* Provide Zip file name from which file needs to be replaced.";
            strInfo = strInfo.Replace("@", System.Environment.NewLine);
            return strInfo;
        }
        private string SetRenameFileInformation()
        {
            string strInfo = "Renames a file from selected zip archieve.@* Provide Zip file name in which file needs to be renames.@* Provide File Name to be renamed.@* Provide New File Name.";
            strInfo = strInfo.Replace("@", System.Environment.NewLine);
            return strInfo;
        }
        private List<string> FillZipItems(string zipFileName, string password)
        {
            compressionExtendedHelper = new InMemoryFileOperations();
           return  compressionExtendedHelper.GetZipItems(zipFileName, password);
        }
        private void ExecuteFileOperation()
        {           
            if (rdAddFile.Checked)
                compressionExtendedHelper.AddFileToZip(txtZipFileNameFile.Text, txtFileName.Text, txtDirectoryPath.Text, txtPasswordFile.Text);
            else if (rdDeleteFile.Checked)
                compressionExtendedHelper.DeleteFileFromZip(txtZipFileNameFile.Text, txtFileName.Text, "", txtPasswordFile.Text);
            else if (rdReplaceContent.Checked)
                compressionExtendedHelper.ReplaceFileContent(txtZipFileNameFile.Text, txtDirectoryPath.Text, txtFileName.Text, txtPasswordFile.Text);
            else if (rdAppendContent.Checked)
                compressionExtendedHelper.AppendFileContent(txtZipFileNameFile.Text, txtFileName.Text,txtDirectoryPath.Text, txtPasswordFile.Text);
            else if (rdRenameFile.Checked)
                compressionExtendedHelper.RenameFile(txtZipFileNameFile.Text, txtFileName.Text, txtDirectoryPath.Text, txtPasswordFile.Text);
            lblStatus.Visible=true;
        }
        private (string,bool) ValidateFileOperationUI()
        {            
            if(txtZipFileNameFile.Text==string.Empty)
            {                
                return ("Please select Zip File name.",false);
            }
            if (txtFileName.Text == string.Empty)
            {
                return ("Please select File name.", false);
            }
            if (rdRenameFile.Checked && txtDirectoryPath.Text == string.Empty)
            {
                return ("Please enter File name to Rename.", false);
            }
            return (string.Empty,true);
        }

        private OpenFileDialog BrowseFile()
        {
            return new OpenFileDialog
            {
                InitialDirectory = @"C:\",
                Title = "Select Zip File Name",
                CheckFileExists = true,
                CheckPathExists = true,
                ReadOnlyChecked = true,
                ShowReadOnly = true
            };
        }
        private FolderBrowserDialog BrowseFolder()
        {
            return new FolderBrowserDialog()
            {
                ShowNewFolderButton = true,
            };
        }
        #endregion

        #region "Events - Compress/Extract"
        private void btnCompress_Click(object sender, EventArgs e)
        {
            var compressor = new CompressionHelper();            
            compressor.ZipFolder(txtFolderNameCompress.Text, txtZipFileNameCompress.Text, 
                txtPwdCompress.Text, chkCompression.Checked);
            lblStatusCompress.Visible=true;
        }

        private void btnExtract_Click(object sender, EventArgs e)
        {
            var extractionHelper = new ExtractionHelper();    
            if (string.IsNullOrEmpty(txtPatternExtract.Text))
                extractionHelper.ExtractZip(txtZipFileNameExtract.Text, txtFolderNameExtract.Text, txtPwdExtract.Text);
            else
                extractionHelper.ExtractZipFiles(txtZipFileNameExtract.Text, txtFolderNameExtract.Text, txtPwdExtract.Text, txtPatternExtract.Text);
            lblStatusExtact.Visible=true;
        }

        private void btnBrowseFolderCompress_Click(object sender, EventArgs e)
        {
            lblStatusCompress.Visible = false;
            var selectFolder = BrowseFolder();
            if (selectFolder.ShowDialog() == DialogResult.OK)
            {
                txtFolderNameCompress.Text = selectFolder.SelectedPath;
            }
        }

        private void btnBrowseZipFileNameCompress_Click(object sender, EventArgs e)
        {
            lblStatusCompress.Visible = false;
            var openDialog = BrowseFile();
            if (openDialog.ShowDialog() == DialogResult.OK)
            {
                txtZipFileNameCompress.Text = openDialog.FileName;
            }
        }

        private void btnBrowseZipFileNameExtract_Click(object sender, EventArgs e)
        {
            lblStatusExtact.Visible = false;
            var openDialog = BrowseFile();
            if (openDialog.ShowDialog() == DialogResult.OK)
            {
                txtZipFileNameExtract.Text = openDialog.FileName;
            }
        }

        private void btnBrowseFolderNameExtract_Click(object sender, EventArgs e)
        {
            lblStatusExtact.Visible = false;
            var selectFolder = BrowseFolder();
            if (selectFolder.ShowDialog() == DialogResult.OK)
            {
                txtFolderNameExtract.Text = selectFolder.SelectedPath;
            }
        }

        #endregion

        #region "Folder Events - Zip Operations"
        private void btnBrowseZipFileFolder_Click(object sender, EventArgs e)
        {
            var openDialog = BrowseFile();
            if (openDialog.ShowDialog() == DialogResult.OK)
            {
                txtZipFileNameFolder.Text = openDialog.FileName;
                lstZipItemsFolder.DataSource = FillZipItems(txtZipFileNameFolder.Text, txtPasswordFolder.Text);

            }
        }

        private void btnOKFolder_Click(object sender, EventArgs e)
        {
            var result = ValidateFolderOperationUI();
            if (result.Item2)
            {
                ExecuteFolderOperation();
                lstZipItemsFolder.DataSource = FillZipItems(txtZipFileNameFolder.Text, txtPasswordFolder.Text);
            }
            else
            {
                MessageBox.Show(result.Item1, "Seven Zip Operations", MessageBoxButtons.OK);
            }
        }       
        private void rdFolder_CheckedChanged(object sender, EventArgs e)
        {
            EnableDisableControlsForFolderOption();
            txtFolderInformation.Text = SetFolderInformation();
        }

        private void chkEncryptionFolder_CheckedChanged(object sender, EventArgs e)
        {
            if (chkEncryption.Checked)
                txtPasswordFolder.Enabled = true;
            else
                txtPasswordFolder.Enabled = false;
            txtPasswordFolder.Text = string.Empty;
        }

        #endregion

        #region "Private Methods - Folder Zip Operations"
        private void ExecuteFolderOperation()
        {
            if (rdAddFolder.Checked)
                AddFolder();
            if (rdDeleteFolder.Checked)
                DeleteFolder();
            if (rdRenameFolder.Checked)
                RenameFolder();
            lblStatusFolder.Visible = true;
        }
        private void TransverseFolder()
        {
            var folderFileIO = new InMemoryFolderOperations();
            List<ArchiveFileInfo> files = folderFileIO.TraverseFolderinArchive(txtZipFileNameFolder.Text, txtPasswordFolder.Text);
            StringBuilder s = new StringBuilder();
            s.AppendLine("Folders");
            foreach (var file in files)
            {
                if (file.IsDirectory)
                {
                    s.AppendLine(file.FileName + Environment.NewLine);
                }
            }
            s.AppendLine("Files");
            foreach (var file in files)
            {
                if (!file.IsDirectory)
                {
                    s.AppendLine(file.FileName + Environment.NewLine);
                }
            }
            //lstTraverseFiles.DataSource = files;
        }
        private void DeleteFolder()
        {
            var folderName = txtFolderName.Text;
            var folderFileIO = new InMemoryFolderOperations();
            string[] foldersToDelete = folderName.Split(',');
            folderFileIO.DeleteFolder(txtZipFileNameFolder.Text, foldersToDelete, txtPasswordFolder.Text);

        }
        private void AddFolder()
        {
            var folderFileIO = new InMemoryFolderOperations();
            folderFileIO.AddFolderToArchive(txtZipFileNameFolder.Text, txtFolderName.Text, txtNewFolderName.Text, txtPasswordFolder.Text);

        }
        private void RenameFolder()
        {
            var folderFileIO = new InMemoryFolderOperations();
            string[] currentFolderNames = txtFolderName.Text.Split(',');
            string[] newFolderNames = txtNewFolderName.Text.Split(',');
            folderFileIO.RenameFolder(txtZipFileNameFolder.Text, currentFolderNames, newFolderNames, txtPasswordFolder.Text);

        }
        private void EnableDisableControlsForFolderOption()
        {
            txtNewFolderName.Enabled = false;
            lblStatusFolder.Visible = false;
            lblFolderName.Text = "Folder Name:";
            txtFolderName.Enabled = true;
            if (rdDeleteFolder.Checked)
            {
                lblFolderName.Text = "Folder Name to Delete:";
                txtNewFolderName.Enabled = false;
                btnFolderOperation.Visible = false;
            }
            if (rdAddFolder.Checked)
            {
                lblFolderName.Text = "Folder Name to Add:";
                label11.Text = "Data Root Directory";
                txtNewFolderName.Enabled = true;
            }
            if (rdRenameFolder.Checked)
            {
                txtNewFolderName.Enabled = true;
                label11.Text = "New Folder Names";
            }
        }
        private (string, bool) ValidateFolderOperationUI()
        {
            if (txtZipFileNameFolder.Text == string.Empty)
            {
                return ("Please select Zip File name.", false);
            }
            if (rdDeleteFolder.Checked && txtFolderName.Text == string.Empty)
            {
                return ("Please select Folder name to Delete.", false);
            }
            if (rdAddFolder.Checked && txtFolderName.Text == string.Empty)
            {
                return ("Please select Folder Name to Add.", false);
            }
            if (rdRenameFolder.Checked && (txtFolderName.Text == string.Empty || txtNewFolderName.Text == string.Empty))
            {
                return ("Please select Folder Name to Rename and New Folder name.", false);
            }
            return (string.Empty, true);
        }
        private string SetFolderInformation()
        {
            if (rdAddFolder.Checked)
                return SetAddFolderInformation();
            else if (rdDeleteFolder.Checked)
                return SetDeleteFolderInformation();
            //else if (rdTraverseFolder.Checked)
            //    return SetTraverseInformation();
            else if (rdRenameFolder.Checked)
                return SetRenameFolderInformation();
            else
                return string.Empty;
        }

        private string SetAddFolderInformation()
        {
            string strInfo = "Adds a folder (provided folder name) to selected zip archieve.@* Provide Zip file name in which folder needs to be added.@* Provide Folder Name to be added.@* (Optional) Provide Directory Name in which file needs to be added. (If Provided folder will be added in Directory)";
            strInfo = strInfo.Replace("@", System.Environment.NewLine);
            return strInfo;
        }
        private string SetDeleteFolderInformation()
        {
            string strInfo = "Deletes a folder (provided folder name) from selected zip archieve.@* Provide Zip file name from which folder needs to be deleted.@* Provide multiple Folder Names (Comma separated) to be deleted.";
            strInfo = strInfo.Replace("@", System.Environment.NewLine);
            return strInfo;
        }
        private string SetTraverseInformation()
        {
            string strInfo = "Traverse selected zip archieve information.@* Provide Zip file name.";
            strInfo = strInfo.Replace("@", System.Environment.NewLine);
            return strInfo;
        }
        private string SetRenameFolderInformation()
        {
            string strInfo = "Rename folder from selected zip folder.@* Provide Zip file name from which folder needs to be renames.@* Provide Folder Name to be renamed. @* Provide New Folder Name(s)";
            strInfo = strInfo.Replace("@", System.Environment.NewLine);
            return strInfo;
        }

        #endregion

        private void btnFolderOperation_Click(object sender, EventArgs e)
        {
            lblStatusExtact.Visible = false;
            var selectFolder = BrowseFolder();
            if (selectFolder.ShowDialog() == DialogResult.OK)
            {
                txtFolderName.Text = selectFolder.SelectedPath;
            }
        }
    }
}
