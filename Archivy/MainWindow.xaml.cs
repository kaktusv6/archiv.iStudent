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

		public MainWindow()
		{
			InitializeComponent();
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
				binding1.Source = archive.Entries;
				fileList.SetBinding(ListBox.ItemsSourceProperty, binding1);
			}
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
				using(ZipArchive archive = ZipFile.OpenRead(fileDialog.FileName))
				{
					Binding binding1 = new Binding();
					binding1.Source = archive.Entries;
					fileList.SetBinding(ListBox.ItemsSourceProperty, binding1);
				}
				pathToArchive = fileDialog.FileName;
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
			IList selectedItems = fileList.SelectedItems;
			string files = string.Empty;
			foreach(object file in selectedItems)
			{
				files += file + "\n";
			}
			MessageBox.Show(files);
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
