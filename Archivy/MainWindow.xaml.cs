using Microsoft.Win32;
using System;

using System.Collections.Generic;

using System.Linq;

using System.Text;
using ConsoleApplicationArchivy;
using System.Threading.Tasks;

using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
//using System.Windows.Shapes;

using System.IO;
using System.IO.Compression;
using System.Collections;
using ArchivyFiles;

namespace Archivy
{
    public enum ExtensionArchive {ZIP, SZ, UNKNOWN};
	/// <summary>
	/// Логика взаимодействия для MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private string pathToArchive = "";
		private string nameTmpDirectory = @"\Archivy\";
        private string currentDirectory = "";
        private ExtensionArchive extensionArchive = ExtensionArchive.UNKNOWN;
		public MainWindow()
		{
			InitializeComponent();
			Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), nameTmpDirectory.Substring(0, nameTmpDirectory.Length - 1)));
			//Loaded += MainWindow_Loaded;
		}

        /* Метод который выполняется перед запуском программы */
        //void MainWindow_Loaded(object sender, RoutedEventArgs e)
        //{

        //}
        private void UpdateListBox(string subDirectory)
        {
            Binding binding = new Binding();

            if (Path.GetExtension(pathToArchive) == ".sz")
            {
                ArchiveSz archive = new ArchiveSz(pathToArchive);

                fileList.ItemsSource = archive.Entries;
                //binding.Source = archive.Entries;
                //fileList.SetBinding(ListBox.ItemsSourceProperty, binding);
                extensionArchive = ExtensionArchive.SZ;
                return;
            }

            using (ZipArchive archive = ZipFile.OpenRead(pathToArchive))
            {
                List<ZipArchiveEntry> entries = new List<ZipArchiveEntry>();
                foreach (ZipArchiveEntry entry in archive.Entries)
                {
                    //MessageBox.Show("Observing " + entry.FullName);
                    if (subDirectory.Length != 0 && entry.FullName.IndexOf(subDirectory) == 0 && entry.FullName != subDirectory)
                    {
                        int count = 0;
                        string entryName = entry.FullName.Remove(0, subDirectory.Length);
                        foreach (char c in entryName)
                        {
                            if (c == '/')
                                count++;
                        }
                        bool isFile = count == 0 && entryName == entry.Name;
                        bool isFolder = count == 1 && entryName.EndsWith("/");
                        if (isFile ^ isFolder)
                        {
                            entries.Add(entry);
                        }
                    }
                    else if (subDirectory.Length == 0)
                    {
                        int count = 0;
                        foreach (char c in entry.FullName)
                            if (c == '/') count++;
                        if (entry.FullName == entry.Name ^ (count == 1 && entry.FullName.EndsWith("/")))
                        {
                            entries.Add(entry);
                        }
                    }
                }
                binding = new Binding();
                binding.Source = entries;
                fileList.SetBinding(ListBox.ItemsSourceProperty, binding);
            }
            currentDirectory = subDirectory;
            extensionArchive = ExtensionArchive.ZIP;
        }
        private void ResetTmpDirectory()
		{
			Directory.Delete(
				Path.Combine(
					Path.GetTempPath(),
					nameTmpDirectory.Substring(0, nameTmpDirectory.Length - 1)
				),
				true
			);
			Directory.CreateDirectory(
				Path.Combine(
					Path.GetTempPath(),
					nameTmpDirectory.Substring(0, nameTmpDirectory.Length - 1)
				)
			);
		}

		/* ----- Меню Файл ----- */
		private void Create_Archivy_Click(object sender, RoutedEventArgs e)
		{
			SaveFileDialog createArchivy = new SaveFileDialog();
			createArchivy.FileName = "Archive";
			createArchivy.DefaultExt = ".zip";
			createArchivy.Filter = "ZIP Архив|*.zip" + "|SZ Архив|*.sz";

			Nullable<bool> result = createArchivy.ShowDialog();

			if (result == true)
			{
                switch (createArchivy.FilterIndex)
                {
                    case 1:
                    {
                        DirectoryInfo dirInfo = new DirectoryInfo(Path.Combine(Path.GetTempPath(), @"\New_Folder_For_Zip"));

                        dirInfo.Create();

                        ZipFile.CreateFromDirectory(dirInfo.FullName, createArchivy.FileName);

                        dirInfo.Delete();
                        pathToArchive = createArchivy.FileName;
                        extensionArchive = ExtensionArchive.ZIP;
                        break;
                    }
                    case 2:
                    {
                        pathToArchive = createArchivy.FileName;
                        File.Create(pathToArchive);
                        extensionArchive = ExtensionArchive.SZ;
                        break;
                    }
                    default:
                    {
                        MessageBox.Show("Error");
                        extensionArchive = ExtensionArchive.UNKNOWN;
                        break;
                    }
                }
			}
		}
		private void Open_Archivy_Click(object sender, RoutedEventArgs e)
		{
			OpenFileDialog fileDialog = new OpenFileDialog();
			fileDialog.Filter = "ZIP Архив|*.zip" + "|SZ Архив|*.sz" + "|Все файлы (*.*)|*.*";
			fileDialog.FilterIndex = 1;
			fileDialog.CheckFileExists = true;
			fileDialog.Multiselect = false;

			Nullable<bool> result = fileDialog.ShowDialog();

			if (result == true)
			{
				pathToArchive = fileDialog.FileName;
				UpdateListBox("");
			}
		}
        private void Decompress_Archivy_Click(object sender, RoutedEventArgs e)
        {
            if (pathToArchive == string.Empty)
            {
                MessageBox.Show("Откройте архив\nкоторый хотите распаковать");
                return;
            }

            System.Windows.Forms.FolderBrowserDialog folderDialog = new System.Windows.Forms.FolderBrowserDialog();
            System.Windows.Forms.DialogResult result = folderDialog.ShowDialog();
            if (result.ToString() == "OK")
            {
                switch(extensionArchive)
                {
                    case ExtensionArchive.ZIP:
                    {
                        ZipArchive archiv = ZipFile.OpenRead(pathToArchive);
                        foreach (ZipArchiveEntry entry in archiv.Entries)
                        {
                            // TODO: переписать распаковку всех файлов архива zip
                            String FullName = Path.Combine(folderDialog.SelectedPath, entry.FullName);
                            entry.ExtractToFile(FullName);
                            FileInfo entryInfo = new FileInfo(Path.GetFullPath(entry.FullName));

                            if (entryInfo.Extension == ".sz")
                            {
                                ArchivySnappy.Decompress(FullName);
                                File.Delete(FullName);
                            }

                        }
                        break;
                    }
                    case ExtensionArchive.SZ:
                    {
                        string pathToDirectory = folderDialog.SelectedPath;
                        ArchiveSz archive = new ArchiveSz(pathToArchive);

                        foreach(ArchiveSzEntry entry in archive.Entries)
                        {
                            archive.ExtractFile(entry.FullName, pathToDirectory);
                        }
                        break;
                    }
                    default: break;
                }
            }
        }
        private void Add_File_Click(object sender, RoutedEventArgs e)
        {
            if (pathToArchive.Length == 0)
            {
                MessageBox.Show("Выберите архив в который хотите добавить файлы");
                return;
            }

            OpenFileDialog fileDialog = new OpenFileDialog();
            
            string filter = "Все файлы (*.*)|*.*";
            switch(extensionArchive)
            {
                case ExtensionArchive.SZ:
                {
                    filter = "Документы|*.docx;*.doc;*.xlsx;*.xls;*.rtf;*.txt;*.html";
                    break;
                }
                default: break;
            }
            fileDialog.Filter = filter;
            fileDialog.FilterIndex = 1;
            fileDialog.CheckFileExists = false;
            fileDialog.Multiselect = true;

            Nullable<bool> result = fileDialog.ShowDialog();

            if (result == true && pathToArchive.Length != 0)
            {
                string[] files = fileDialog.FileNames;
                
                switch (extensionArchive)
                {
                    case ExtensionArchive.ZIP:
                    {
                        for (int i = 0; i < files.Length; i++)
                        {
                            FileInfo fileInfo = new FileInfo(files[i]);
                            string pathToFile = fileInfo.ToString();

                            using (FileStream fileStream = fileInfo.OpenRead())
                            {
                                using (FileStream zipToOpen = new FileStream(pathToArchive, FileMode.Open))
                                {
                                    using (ZipArchive archive = new ZipArchive(zipToOpen, ZipArchiveMode.Update))
                                    {
                                        foreach (ZipArchiveEntry entry in archive.Entries)
                                        {
                                            if (Path.GetFileName(pathToFile) == Path.GetFileName(entry.FullName))
                                            {
                                                pathToFile = Path.GetDirectoryName(pathToFile) + "\\(Copy)" + Path.GetFileName(pathToFile);
                                            }
                                        }

                                        ZipArchiveEntry newEntry = archive.CreateEntry(Path.GetFileName(pathToFile));

                                        using (Stream writer = newEntry.Open())
                                        {
                                            fileStream.CopyTo(writer);
                                        }
                                    }
                                }
                            }
                        }
                        break;
                    }
                    case ExtensionArchive.SZ:
                    {
                        for (int i = 0; i < files.Length; i++)
                        {
                            ArchiveSz archive = new ArchiveSz(pathToArchive);
                            archive.AddFile(files[i]);
                        }
                        break;
                    }
                    default: break;
                }
                UpdateListBox(currentDirectory);
            }
        }

		/* ----- Меню Правка ----- */
		private void Copy_Files_Click(object sender, RoutedEventArgs e)
		{	
			ResetTmpDirectory();
			
			IList selectedFiles = fileList.SelectedItems;
			System.Collections.Specialized.StringCollection files = new System.Collections.Specialized.StringCollection();
			
            switch(extensionArchive)
            {
                case ExtensionArchive.ZIP:
                {
                    string pathToTmpDirectory = Path.Combine(Path.GetTempPath(), nameTmpDirectory);
                    foreach (ZipArchiveEntry entry in selectedFiles)
                    {
                        using (FileStream zipToOpen = new FileStream(pathToArchive, FileMode.Open))
                        {
                            using (ZipArchive archive = new ZipArchive(zipToOpen, ZipArchiveMode.Update))
                            {
                                for (int i = 0; i < archive.Entries.Count; i++)
                                {
                                    if (entry.FullName == archive.Entries[i].FullName)
                                    {
                                        string file = Path.Combine(pathToTmpDirectory, @entry.Name);
                                        
                                        archive.Entries[i].ExtractToFile(file);
                                        FileInfo fileInf = new FileInfo(file);
                                        file = fileInf.FullName;

                                        files.Add(file);
                                    }
                                }
                            }
                        }
                    }
                    break;
                }
                case ExtensionArchive.SZ:
                {
                    ArchiveSz archive = new ArchiveSz(pathToArchive);
                    string pathToTmpDirectory = Path.Combine(Path.GetTempPath(), nameTmpDirectory);

                    foreach (ArchiveSzEntry entrySelected in selectedFiles)
                    {
                        foreach(ArchiveSzEntry entryArchive in archive.Entries)
                        {
                            if (entrySelected.FullName == entryArchive.FullName)
                            {
                                string file = Path.Combine(pathToTmpDirectory, @entrySelected.FullName);
                                FileInfo fi = new FileInfo(file);

                                archive.ExtractFile(entrySelected.FullName, pathToTmpDirectory);
                                file = fi.FullName;
                                files.Add(file);
                            }
                        }
                    }
                    break;
                }
                default: break;
            }

			Clipboard.SetFileDropList(files);
		}
		private void Cut_Files_Click(object sender, RoutedEventArgs e)
		{
			Copy_Files_Click(sender, e);
			IList selectedFiles = fileList.SelectedItems;

			switch(extensionArchive)
            {
                case ExtensionArchive.ZIP:
                {
                    foreach (ZipArchiveEntry entry in selectedFiles)
                    {
                        using (FileStream zipToOpen = new FileStream(pathToArchive, FileMode.Open))
                        {
                            using (ZipArchive archive = new ZipArchive(zipToOpen, ZipArchiveMode.Update))
                            {
                                for (int i = 0; i < archive.Entries.Count; i++)
                                {
                                    if (entry.FullName == archive.Entries[i].FullName)
                                    {
                                        archive.Entries[i].Delete();
                                    }
                                }
                            }
                        }
                    }
                    break;
                }
                case ExtensionArchive.SZ:
                {
                    ArchiveSz archive = new ArchiveSz(pathToArchive);
                    foreach(ArchiveSzEntry entrySelected in selectedFiles)
                    {
                        foreach(ArchiveSzEntry entryFile in archive.Entries)
                        {
                            if(entryFile.FullName == entrySelected.FullName)
                            {
                                archive.DeleteFile(entrySelected.FullName);
                            }
                        }
                    }
                    break;
                }
                default: break;
            }
			UpdateListBox(currentDirectory);
		}
		private void Past_Files_Click(object sender, RoutedEventArgs e)
		{
			System.Collections.Specialized.StringCollection files = Clipboard.GetFileDropList();

            switch(extensionArchive)
            {
                case ExtensionArchive.ZIP:
                {
                    for (int i = 0; i < files.Count; i++)
                    {
                        string fileInfoStr = files[i];
                        FileInfo fileInfo = new FileInfo(fileInfoStr);

                        using (FileStream fileStream = fileInfo.OpenRead())
                        {
                            using (FileStream zipToOpen = new FileStream(pathToArchive, FileMode.Open))
                            {
                                using (ZipArchive archive = new ZipArchive(zipToOpen, ZipArchiveMode.Update))
                                {
                                    foreach (ZipArchiveEntry entry in archive.Entries)
                                    {
                                        if (Path.GetFileName(fileInfoStr) == Path.GetFileName(entry.FullName))
                                        {
                                            fileInfoStr = Path.GetDirectoryName(fileInfoStr) + "\\(Copy)" + Path.GetFileName(fileInfoStr);
                                        }
                                    }

                                    ZipArchiveEntry entryFile = archive.CreateEntry(Path.GetFileName(fileInfoStr));
                                    using (Stream writer = entryFile.Open())
                                    {
                                        fileStream.CopyTo(writer);
                                    }
                                }
                            }
                        }
                    }
                    break;
                }

                case ExtensionArchive.SZ:
                {
                    ArchiveSz archive = new ArchiveSz(pathToArchive);
                    string extensionsDocuments = ".docx *.doc *.xlsx *.xls *.rtf *.txt *.html";
                    for(int i = 0; i < files.Count; i++)
                    {
                        bool isDocument = extensionsDocuments.IndexOf(Path.GetExtension(files[i])) == -1;
                        if (isDocument)
                        {
                            archive.AddFile(files[i]);
                        }
                    }
                    break;
                }
                default: break;
            }
			
			UpdateListBox(currentDirectory);
		}
		private void Delete_Files_Click(object sender, RoutedEventArgs e)
		{
			if (fileList.SelectedItems.Count == 0)
			{
				MessageBox.Show("Выберите файлы которые хотите удалить");
				return;
			}

			IList selectedFiles = fileList.SelectedItems;
			
            switch(extensionArchive)
            {
                case ExtensionArchive.ZIP:
                {
                    foreach (ZipArchiveEntry entry in selectedFiles)
                    {
                        using (FileStream zipToOpen = new FileStream(pathToArchive, FileMode.Open))
                        {
                            using (ZipArchive archive = new ZipArchive(zipToOpen, ZipArchiveMode.Update))
                            {
                                for (int i = 0; i < archive.Entries.Count; i++)
                                {
                                    if (entry.FullName == archive.Entries[i].FullName)
                                    {
                                        archive.Entries[i].Delete();
                                    }
                                }
                            }
                        }
                    }
                    break;
                }
                case ExtensionArchive.SZ:
                {
                    foreach(ArchiveSzEntry entrySelected in selectedFiles)
                    {
                        ArchiveSz archive = new ArchiveSz(pathToArchive);
                        foreach(ArchiveSzEntry entryArchive in archive.Entries)
                        {
                            if(entrySelected.FullName == entryArchive.FullName)
                            {
                                archive.DeleteFile(entrySelected.FullName);
                            }
                        }
                    }
                    break;
                }
                default: break;
            }
            
			UpdateListBox(currentDirectory);
		}
		private void Rename_File_Click(object sender, RoutedEventArgs e)
		{
			if (fileList.SelectedItems.Count == 0)
			{
				MessageBox.Show("Выберите файл\nкоторый хотите переименовать");
				return;
			}
			if (fileList.SelectedItems.Count > 1)
			{
				MessageBox.Show("Выберите один файл\nкоторый хотите переименовать");
				return;
			}
            string oldName = string.Empty;
            switch(extensionArchive)
            {
                case ExtensionArchive.ZIP:
                {
                    oldName = ((ZipArchiveEntry)fileList.SelectedItem).Name;
                    break;
                }
                case ExtensionArchive.SZ:
                {
                    oldName = ((ArchiveSzEntry)fileList.SelectedItem).FullName;
                    break;
                }
                default: break;
            }
			
			WindowRename windowRename = new WindowRename(pathToArchive, oldName, extensionArchive);
			
            windowRename.ShowDialog();
			UpdateListBox(currentDirectory);
		}

		/* ----- Меню Справка ----- */
		private void Help_Click(object sender, RoutedEventArgs e)
		{
			// System.Diagnostics.Process.Start(AppDomain.CurrentDomain.BaseDirectory + @"\Help\Archive.html"); // запуск файла стандартным приложением
			// AppDomain.CurrentDomain.BaseDirectory // путь до .exe
			// System.Windows.Forms.Help.ShowHelp(null, "Help/Archive.html");
		}
		private void About_Click(object sender, RoutedEventArgs e)
		{
			WindowAbout winAbout = new WindowAbout();
			winAbout.ShowDialog();
		}

        private void StoreFolder(DirectoryInfo folderName, string parentFolder)
        {
            MessageBox.Show("gonna store a folder");
            String extens = ".doc .docx .rtf .txt .html .xls .xlsx";
            bool altered;
            string newPath = parentFolder;
            using (FileStream zipToOpen = new FileStream(pathToArchive, FileMode.Open))
            {
                using (ZipArchive archive = new ZipArchive(zipToOpen, ZipArchiveMode.Update))
                {
                    ZipArchiveEntry readmeEntry;
                    readmeEntry = archive.CreateEntry(parentFolder + folderName.Name + "/");
                    FileInfo[] Files = folderName.GetFiles();
                    foreach (FileInfo file in Files)
                    {
                        //MessageBox.Show("adding " + parentFolder + folderName.Name + "/" + file.Name);
                        readmeEntry = archive.CreateEntryFromFile(folderName.FullName + "\\" + file.Name, parentFolder + folderName.Name + "/" + file.Name);
                    }
                }
            }
            DirectoryInfo[] directories = folderName.GetDirectories();
            foreach (DirectoryInfo directory in directories)
            {
                if (parentFolder.Length == 0)
                {
                    StoreFolder(directory, folderName.Name + "/");
                }
                else
                {
                    StoreFolder(directory, parentFolder + "/" + folderName.Name + "/");
                }
            }
            UpdateListBox(currentDirectory);
        }

        private void Drop_File(object sender, DragEventArgs e)
        {
            String extens = ".doc .docx .rtf .txt .html .xls .xlsx";
            if (pathToArchive.Length == 0)
            {
                MessageBox.Show("Выберите архив в который хотите добавить файлы");
                return;
            }
            
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                
                string[] files = e.Data.GetData(DataFormats.FileDrop, true) as string[];
                switch (extensionArchive)
                {
                    case ExtensionArchive.ZIP:
                {
                    foreach (string file in files)
                    {
                        string temp = file;
                        FileInfo fileInfo = new FileInfo(file);

                        FileAttributes attr = File.GetAttributes(file);
                        if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
                        {
                            DirectoryInfo dir = new DirectoryInfo(file);
                            StoreFolder(dir, currentDirectory);
                        }
                        else
                        {
                            using (FileStream fileStream = fileInfo.OpenRead())
                            {
                                using (FileStream zipToOpen = new FileStream(pathToArchive, FileMode.Open))
                                {
                                    using (ZipArchive archive = new ZipArchive(zipToOpen, ZipArchiveMode.Update))
                                    {
                                        foreach (ZipArchiveEntry entry in archive.Entries)
                                        {
                                            if (Path.GetFileName(fileInfo.Name) == Path.GetFileName(entry.FullName))
                                            {
                                                temp = Path.GetDirectoryName(fileInfo.ToString()) + "\\(Copy)" + Path.GetFileName(fileInfo.Name);
                                            }
                                        }

                                        ZipArchiveEntry newEntry = archive.CreateEntry(Path.GetFileName(temp));
                                        using (Stream writer = newEntry.Open())
                                        {
                                            fileStream.CopyTo(writer);
                                        }
                                    }
                                }
                            }
                        }
                    }
                            break;  
                }
                    case ExtensionArchive.SZ:
                        {
                            foreach(string file in files)
                            {
                                FileInfo checker = new FileInfo(file);
                                if (extens.IndexOf(checker.Extension) != -1)
                                {
                                    ArchiveSz archive = new ArchiveSz(pathToArchive);
                                    archive.AddFile(file); 
                                }
                            }
                            break;
                        }
                    default:break;
                }
                UpdateListBox(currentDirectory);
            }
        }
        private void Click_Element(object sender, MouseButtonEventArgs e)
        {
            if (sender.ToString().EndsWith("/"))
            {
                currentDirectory = sender.ToString().Replace("System.Windows.Controls.ListViewItem: ", "");
                UpdateListBox(currentDirectory);
            }
            else
            {
                MessageBox.Show("Not implemented yet");
            }
        }
        private void Back_Click(object sender, RoutedEventArgs e)
        {
            if (currentDirectory != "")
            {
                currentDirectory = currentDirectory.Remove(currentDirectory.Length - 1, 1);
                while (currentDirectory.Length != 0 && currentDirectory[currentDirectory.Length - 1] != '/')
                {
                    currentDirectory = currentDirectory.Remove(currentDirectory.Length - 1, 1);
                }
                UpdateListBox(currentDirectory);
            }
        }

		~MainWindow()
		{
			Directory.Delete(
				Path.Combine(
					Path.GetTempPath(),
					nameTmpDirectory.Substring(0, nameTmpDirectory.Length - 1)
				),
				true
			);
		}
    }
}
