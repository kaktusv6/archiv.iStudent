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
    enum ExtensionArchive {ZIP, SZ, UNKNOWN};
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
            //ArchiveSz per;
            //using (FileStream stream = new FileStream("Arch.sz", FileMode.Append))
            //{
            //    per = new ArchiveSz(stream);
            //}
            //per.AddFile("test1.txt");
            //per.AddFile("test2.txt");
            //per.AddFile("test2.txt");
            //per.AddFile("test1.txt");
            //per.AddFile("test3.txt");


			InitializeComponent();
			Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), nameTmpDirectory.Substring(0, nameTmpDirectory.Length - 1)));
			//Loaded += MainWindow_Loaded;
		}

		/* Метод который выполняется перед запуском программы */
		//void MainWindow_Loaded(object sender, RoutedEventArgs e)
		//{
			
		//}
        private void WhatExtensionArchive()
        {
            string _extensionArchive = Path.GetExtension(pathToArchive);
            if (_extensionArchive == "zip")
            {
                extensionArchive = ExtensionArchive.ZIP;
            }
            if (_extensionArchive == "sz")
            {
                extensionArchive = ExtensionArchive.SZ;
            }

            extensionArchive = ExtensionArchive.UNKNOWN;
        }
		private void UpdateListBox(string subDirectory)
		{
            if (Path.GetExtension(pathToArchive) == "sz")
            {
                ArchiveSz archive = new ArchiveSz(pathToArchive);
                Binding binding1 = new Binding();
                //ZipArchiveEntry back;
                binding1.Source = archive.archiveSzEntry;
                fileList.SetBinding(ListBox.ItemsSourceProperty, binding1);
                extensionArchive = ExtensionArchive.SZ;
                return;
            }
            
			using (ZipArchive archive = ZipFile.OpenRead(pathToArchive))
			{
                List<ZipArchiveEntry> entries = new List<ZipArchiveEntry>();
                foreach (ZipArchiveEntry entry in archive.Entries)
                {
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
                    else if(subDirectory.Length == 0)
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
				Binding binding1 = new Binding();
                //ZipArchiveEntry back;
				binding1.Source = entries;
				fileList.SetBinding(ListBox.ItemsSourceProperty, binding1);
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
			createArchivy.Filter = "ZIP Архив (.zip)|*.zip" + "|SZ Архив (.sz)|*.sz";

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
                // создание SZ архива
			}
		}
		private void Open_Archivy_Click(object sender, RoutedEventArgs e)
		{
			OpenFileDialog fileDialog = new OpenFileDialog();
			fileDialog.Filter = "Архивы (.zip)|*.zip" + "|Все файлы (*.*)|*.*";
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
                ZipArchive archiv = ZipFile.OpenRead(pathToArchive);
                foreach (ZipArchiveEntry entry in archiv.Entries)
                {
                    String fullName = Path.Combine(folderDialog.SelectedPath, entry.FullName);
                    entry.ExtractToFile(fullName);
                    FileInfo entryInfo = new FileInfo(Path.GetFullPath(entry.FullName));

                    if (entryInfo.Extension == ".sz")
                    {
                        ArchivySnappy.Decompress(fullName);
                        File.Delete(fullName);
                    }

                }
            }
        }
        private void Add_File_Click(object sender, RoutedEventArgs e)
        {
            String extens = ".doc .docx .rtf .txt .html . xls . xlsx";
            if (pathToArchive.Length == 0)
            {
                MessageBox.Show("Выберите архив в который хотите добавить файлы");
                return;
            }

            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Filter = "Все файлы (*.*)|*.*";
            fileDialog.FilterIndex = 1;
            fileDialog.CheckFileExists = false;
            fileDialog.Multiselect = true;


            Nullable<bool> result = fileDialog.ShowDialog();

            if (result == true && pathToArchive.Length != 0)
            {

                string[] files;

                files = fileDialog.FileNames;


                for (int i = 0; i < files.Length; i++)
                {
                    FileInfo fileInfo = new FileInfo(files[i]);
                    string FileS = fileInfo.ToString();

                    if (extens.IndexOf(fileInfo.Extension) != -1)
                    {
                        FileS = ArchivySnappy.Compress(FileS);
                        fileInfo = new FileInfo(FileS);
                    }

                    using (FileStream fileStream = fileInfo.OpenRead())
                    {
                        using (FileStream zipToOpen = new FileStream(pathToArchive, FileMode.Open))
                        {
                            using (ZipArchive archive = new ZipArchive(zipToOpen, ZipArchiveMode.Update))
                            {
                                foreach (ZipArchiveEntry entry in archive.Entries)
                                {
                                    if (Path.GetFileName(FileS) == Path.GetFileName(entry.FullName))
                                    {
                                        FileS = Path.GetDirectoryName(FileS) + "\\(Copy)" + Path.GetFileName(FileS);
                                    }
                                }

                                ZipArchiveEntry newEntry = archive.CreateEntry(Path.GetFileName(FileS));
                                using (Stream writer = newEntry.Open())
                                {
                                    fileStream.CopyTo(writer);
                                }
                            }
                        }
                    }
                    if (fileInfo.Extension == ".sz")
                    {
                        fileInfo.Delete();
                    }
                }
                UpdateListBox("");
            }
        }

		/* ----- Меню Правка ----- */
		private void Copy_Files_Click(object sender, RoutedEventArgs e)
		{	
			ResetTmpDirectory();

			String extens = ".doc .docx .rtf .txt .html .xls .xlsx";
			IList selectedFiles = fileList.SelectedItems;
			System.Collections.Specialized.StringCollection files = new System.Collections.Specialized.StringCollection();
			
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
								bool isDocument = (Path.GetExtension(entry.Name)) == ".sz";
								
								string file = Path.Combine(Path.GetTempPath(), nameTmpDirectory, @entry.Name);
								if (isDocument)
								{
									file = Path.Combine(Path.GetTempPath(), nameTmpDirectory, @entry.Name);
								}

								archive.Entries[i].ExtractToFile(file);
                                FileInfo fileInf = new FileInfo(file);
                                file = fileInf.FullName;

								if (isDocument)
								{
									FileInfo f = new FileInfo(file);

									ArchivySnappy.Decompress(file);
                                    f.Delete();
                                    
									files.Add(file.Remove(file.Length - 3));
								}
								else
								{
									files.Add(file);
								}
								
							}
						}
					}
				}
			}

			Clipboard.SetFileDropList(files);
		}
		private void Cut_Files_Click(object sender, RoutedEventArgs e)
		{
			Copy_Files_Click(sender, e);
			IList selectedFiles = fileList.SelectedItems;
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
			UpdateListBox("");
		}
		private void Past_Files_Click(object sender, RoutedEventArgs e)
		{
			String extens = ".doc .docx .rtf .txt .html .xls .xlsx";
			System.Collections.Specialized.StringCollection files = Clipboard.GetFileDropList();

			for (int i = 0; i < files.Count; i++)
			{
				bool isDocument = extens.IndexOf(Path.GetExtension(files[i])) != -1;

				string fileInfoStr = files[i];

				if (isDocument)
				{
					fileInfoStr = ArchivySnappy.Compress(files[i]);
				}

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

				if(isDocument)
				{
					fileInfo.Delete();
				}

			}
			UpdateListBox("");
		}
		private void Delete_Files_Click(object sender, RoutedEventArgs e)
		{
			if (fileList.SelectedItems.Count == 0)
			{
				MessageBox.Show("Выберите файлы которые хотите удалить");
				return;
			}

			IList selectedFiles = fileList.SelectedItems;
			foreach(ZipArchiveEntry entry in selectedFiles)
			{
				using (FileStream zipToOpen = new FileStream(pathToArchive, FileMode.Open))
				{
					using (ZipArchive archive = new ZipArchive(zipToOpen, ZipArchiveMode.Update))
					{
						for(int i = 0; i < archive.Entries.Count; i++)
						{
							if (entry.FullName == archive.Entries[i].FullName)
							{
								archive.Entries[i].Delete();
							}
						}
					}
				}
			}
			UpdateListBox("");
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

			ZipArchiveEntry file = (ZipArchiveEntry)fileList.SelectedItem;
			
			// выводить окно ввода имени файла
			//WindowRename windowRename = new WindowRename();
			WindowRename windowRename = new WindowRename(pathToArchive, file.FullName);
			windowRename.ShowDialog();
			UpdateListBox("");
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
			// Выводить инфу о программе тоже отдельное окно
		}

        private void storeFolder(string foldername, string parentfolder)
        {
            String extens = ".doc .docx .rtf .txt .html .xls .xlsx";
            string[] filecontent = Directory.GetFiles(foldername);
            string[] directories = Directory.GetDirectories(foldername);
            foreach (string file in filecontent)
            {
                string temp = file;
                FileInfo fileInfo = new FileInfo(file);
                if (extens.IndexOf(fileInfo.Extension) != -1 )
                    {
                        temp = ArchivySnappy.Compress(file);
                        fileInfo = new FileInfo(temp);
                    }

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

                            ZipArchiveEntry newEntry = archive.CreateEntry(parentfolder+ "\\" + foldername + temp);
                            using (Stream writer = newEntry.Open())
                            {
                                fileStream.CopyTo(writer);
                            }
                        }
                    }
                }
                if (fileInfo.Extension == ".sz")
                {
                    fileInfo.Delete();
                }
            }
            UpdateListBox("");
        }

        
        private void dropfile(object sender, DragEventArgs e)
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
                foreach (string file in files)
                {
                    string temp = file;
                    FileInfo fileInfo = new FileInfo(file);

                    FileAttributes attr = File.GetAttributes(file);
                    if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
                    {
                        DirectoryInfo dir = new DirectoryInfo(file);
                        storeFolder(dir.FullName, "");
                    }
                    else
                    {

                        if (extens.IndexOf(fileInfo.Extension) != -1 )
                        {
                            temp = ArchivySnappy.Compress(file);
                            fileInfo = new FileInfo(temp);
                        }

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
                        if (fileInfo.Extension == ".sz")
                        {
                            fileInfo.Delete();
                        }
                    }
                }
                UpdateListBox("");
            }
        }

        private void ClickElement(object sender, MouseButtonEventArgs e)
        {
            //MessageBox.Show(sender.ToString().Replace("System.Windows.Controls.ListViewItem: ", ""));
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

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            //MessageBox.Show(currentDirectory);
            if (currentDirectory != "")
            {
                currentDirectory = currentDirectory.Remove(currentDirectory.Length - 1, 1);
                while (currentDirectory.Length != 0 && currentDirectory[currentDirectory.Length - 1] != '/')
                {
                    currentDirectory = currentDirectory.Remove(currentDirectory.Length - 1, 1);
                }
                //MessageBox.Show(currentDirectory);
                UpdateListBox(currentDirectory);
            }
        }
    }
}
