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
using System.Windows.Shapes;

using System.IO;
using System.IO.Compression;
using System.Collections;

namespace Archivy
{
	/// <summary>
	/// Логика взаимодействия для Window1.xaml
	/// </summary>
	public partial class WindowRename : Window
	{
		private string pathToArchive = string.Empty;
		private string oldName;
		public WindowRename()
		{
			InitializeComponent();
		}
		public WindowRename(string path, string _oldName)
		{
			pathToArchive = path;
			oldName = _oldName;
		}

		private void Rename_Button_Click(object sender, RoutedEventArgs e)
		{
			using (FileStream zipToOpen = new FileStream(pathToArchive, FileMode.Open))
			{
				using (ZipArchive archive = new ZipArchive(zipToOpen, ZipArchiveMode.Update))
				{
					ZipArchiveEntry oldEntry = archive.GetEntry(oldName);
					ZipArchiveEntry newEntry = archive.CreateEntry(textBox.GetLineText(0));

					using(Stream oldStream = oldEntry.Open())
					using(Stream newStream = newEntry.Open())
					{
						oldStream.CopyTo(newStream);
					}

					oldEntry.Delete();
				}
			}

			Close();
		}
	}
}
