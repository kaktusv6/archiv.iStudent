using Microsoft.Win32;
using System;

using System.Collections.Generic;

using System.Linq;

using System.Text;

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

namespace Archivy
{
	/// <summary>
	/// Логика взаимодействия для MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private string pathToArchive = "";
		private string nameTmpDirectory = @"\Archivy\";
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
		private void UpdateListBox()
		{
			using (ZipArchive archive = ZipFile.OpenRead(pathToArchive))
			{
				Binding binding1 = new Binding();
				List<ZipArchiveEntry> list = new List<ZipArchiveEntry>();
				foreach (ZipArchiveEntry entyre in archive.Entries)
				{
					if (Path.GetExtension(entyre.Name) != string.Empty)
					{
						list.Add(entyre);
					}
				}
				binding1.Source = list;
				fileList.SetBinding(ListBox.ItemsSourceProperty, binding1);
			}
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
			createArchivy.Filter = "ZIP Архив (.zip)|*.zip";

			Nullable<bool> result = createArchivy.ShowDialog();

			if (result == true)
			{
				DirectoryInfo dirInfo = new DirectoryInfo(Path.Combine(Path.GetTempPath(), @"\New_Folder_For_Zip"));
				dirInfo.Create();

				ZipFile.CreateFromDirectory(dirInfo.FullName, createArchivy.FileName);
				dirInfo.Delete();
				pathToArchive = createArchivy.FileName;
			}

			/* Create Archive */
			//string startPath = @"c:\example\start"; // путь до папки которую хотим архиваировать(архивируется и все содержимое)
			//string zipPath = @"c:\example\result.zip"; // путь до полученого в результате архива
			//string extractPath = @"c:\example\extract"; // путь до папки куда разархивировать содержимое

			//ZipFile.CreateFromDirectory(startPath, zipPath); // создание архива

			//ZipFile.ExtractToDirectory(zipPath, extractPath); // распаковка архива
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
				UpdateListBox();
			}
		}
		private void Decompress_Archivy_Click(object sender, RoutedEventArgs e)
		{
			if (pathToArchive == string.Empty)
			{
				MessageBox.Show("Откройте архив\nкоторый хотите распокавать");
				return;
			}

			System.Windows.Forms.FolderBrowserDialog folderDialog = new System.Windows.Forms.FolderBrowserDialog();
			System.Windows.Forms.DialogResult result = folderDialog.ShowDialog();
			if (result.ToString() == "OK")
			{
				ZipFile.ExtractToDirectory(pathToArchive, folderDialog.SelectedPath);
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
			fileDialog.Filter = "Все файлы (*.*)|*.*";
			fileDialog.FilterIndex = 1;
			fileDialog.CheckFileExists = false;
			fileDialog.Multiselect = true;

			Nullable<bool> result = fileDialog.ShowDialog();

			if (result == true && pathToArchive.Length != 0)
			{
				string[] files = fileDialog.FileNames;
				for (int i = 0; i < files.Length; i++)
				{
					FileInfo fileInfo = new FileInfo(files[i]);
					using (FileStream fileStream = fileInfo.OpenRead())
					{
						using (FileStream zipToOpen = new FileStream(pathToArchive, FileMode.Open))
						{
							using (ZipArchive archive = new ZipArchive(zipToOpen, ZipArchiveMode.Update))
							{
								foreach (ZipArchiveEntry entry in archive.Entries)
								{
									if (Path.GetFileName(files[i]) == Path.GetFileName(entry.FullName))
									{
										files[i] = Path.GetDirectoryName(files[i]) + "\\(Copy)" + Path.GetFileName(files[i]);
									}
								}

								ZipArchiveEntry newEntry = archive.CreateEntry(Path.GetFileName(files[i]));
								using (Stream writer = newEntry.Open())
								{
									fileStream.CopyTo(writer);
								}
							}
						}
					}
				}
				UpdateListBox();
			}
		}
		/* ----- Меню Правка ----- */
		private void Copy_Files_Click(object sender, RoutedEventArgs e)
		{	
			ResetTmpDirectory();
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
								string file = Path.Combine(Path.GetTempPath(), nameTmpDirectory, @entry.Name);
								
								archive.Entries[i].ExtractToFile(file);
								files.Add(file);
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
			UpdateListBox();
		}
		private void Past_Files_Click(object sender, RoutedEventArgs e)
		{
			System.Collections.Specialized.StringCollection files = Clipboard.GetFileDropList();

			for (int i = 0; i < files.Count; i++)
			{
				FileInfo fileInfo = new FileInfo(files[i]);
				using (FileStream fileStream = fileInfo.OpenRead())
				{
					using (FileStream zipToOpen = new FileStream(pathToArchive, FileMode.Open))
					{
						using (ZipArchive archive = new ZipArchive(zipToOpen, ZipArchiveMode.Update))
						{
							foreach (ZipArchiveEntry entry in archive.Entries)
							{
								if (Path.GetFileName(files[i]) == Path.GetFileName(entry.FullName))
								{
									files[i] = Path.GetDirectoryName(files[i]) + "\\(Copy)" + Path.GetFileName(files[i]);
								}
							}

							ZipArchiveEntry entryFile = archive.CreateEntry(Path.GetFileName(files[i]));
							using (Stream writer = entryFile.Open())
							{
								fileStream.CopyTo(writer);
							}
						}
					}
				}
			}
			UpdateListBox();
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
			UpdateListBox();
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
			WindowRename windowRename = new WindowRename(pathToArchive, file.Name);
			windowRename.ShowDialog();
			UpdateListBox();
		}
		/* ----- Меню Справка ----- */
		private void Help_Click(object sender, RoutedEventArgs e)
		{
			// Выводить отдельное окно с описанием как пользоваться
		}
		private void About_Click(object sender, RoutedEventArgs e)
		{
			// Выводить инфу о программе тоже отдельное окно
		}
	}
}
