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
using System.Windows.Shapes;

using System.IO;
using System.IO.Compression;

namespace Archivy
{
	/// <summary>
	/// Логика взаимодействия для MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();
			Loaded += MainWindow_Loaded;
		}

		void MainWindow_Loaded(object sender, RoutedEventArgs e)
		{
			
			//fileList.ItemsSource = new[] {
			//	new { nameFile = "fileFirst.txt", text = "lol" },
			//	new { nameFile = "fileSecond.txt", text = "lol" },
			//	new { nameFile = "fileThrid.txt", text = "lol" }
			//};
		}
		private void Open_Archivy_Click(object sender, RoutedEventArgs e)
		{
			OpenFileDialog fileDialog = new OpenFileDialog();
			fileDialog.Filter = "Архивы (.zip)|*.zip" + "|Все файлы (*.*)|*.*";
			fileDialog.FilterIndex = 1;
			fileDialog.CheckFileExists = true;
			fileDialog.Multiselect = false;
			if (fileDialog.ShowDialog() == true)
			{
				using(ZipArchive archive = ZipFile.OpenRead(fileDialog.FileName))
				{
					Binding binding1 = new Binding();
					binding1.Source = archive.Entries;
					fileList.SetBinding(ListBox.ItemsSourceProperty, binding1);
					//foreach (ZipArchiveEntry entry in archive.Entries)
					//{
						
					//}
				}
			}
			else
			{
				
			}
		}
		private void Open_MunuItem_Click(object sender, RoutedEventArgs e)
		{
			fileList.ItemsSource = new[] {
				new { nameFile = "fileFirst.txt", text = "lol" },
				new { nameFile = "fileSecond.txt", text = "lol" },
				new { nameFile = "fileThrid.txt", text = "lol" }
			};
			//OpenFileDialog myDialog = new OpenFileDialog(); // создание окна выбора файла
			//myDialog.Filter = "Картинки (.jpg;.gif)|*.jpg;*.gif" + "|Все файлы (*.*)|*.*"; // фильтры выбора файла
			//myDialog.FilterIndex = 2; // какой фильтр ставить при отображение окна
			//myDialog.CheckFileExists = true; // проверка поля с именем файла
			//// myDialog.Multiselect = true; // выбор нескольких файлов
			//if (myDialog.ShowDialog() == true)
			//{
			//	//MessageBox.Show( myDialog.FileName); // высвечивает полный путь до выбраного файла
			//}
			//else
			//{
			//	// файл не выбран
			//}
		}
	}
}
