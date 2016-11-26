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

using Archivy;
using ArchivyFiles;

//using System.Windows.Shapes;

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
		private string oldName = string.Empty;
        private ExtensionArchive extentionArchive = ExtensionArchive.UNKNOWN;
		public WindowRename()
		{
			InitializeComponent();
		}
		public WindowRename(string path, string _oldName, ExtensionArchive extention)
		{
			InitializeComponent();

			pathToArchive = path;
			oldName = _oldName;
            extentionArchive = extention;
			textBox.Text = Path.GetFileName(_oldName);
		}

		private void Rename_Button_Click(object sender, RoutedEventArgs e)
		{
            switch(extentionArchive)
            {
                case ExtensionArchive.ZIP:
                {
                    using (FileStream zipToOpen = new FileStream(pathToArchive, FileMode.Open))
                    {
                        using (ZipArchive archive = new ZipArchive(zipToOpen, ZipArchiveMode.Update))
                        {
                            ZipArchiveEntry oldEntry = archive.GetEntry(oldName);
                            ZipArchiveEntry newEntry = archive.CreateEntry(textBox.GetLineText(0));

                            using (Stream oldStream = oldEntry.Open())
                            using (Stream newStream = newEntry.Open())
                            {
                                oldStream.CopyTo(newStream);
                            }

                            oldEntry.Delete();
                        }
                    }
                    break;
                }

                case ExtensionArchive.SZ:
                {
                    ArchiveSz archive = new ArchiveSz(pathToArchive);
                    string newName = textBox.GetLineText(0);
                    archive.ExtractFile(oldName, Path.GetTempPath());
                    archive.DeleteFile(oldName);

                    string pathToNewFile = Path.Combine(Path.GetTempPath(), newName);
                    string pathToOldFile = Path.Combine(Path.GetTempPath(), oldName);
                    
                    using (FileStream oldFile = File.Open(pathToOldFile, FileMode.Open))
                    using(FileStream newFile = File.Create(pathToNewFile))
                    {
                        oldFile.CopyTo(newFile);
                    }
                    archive.AddFile(pathToNewFile);
                    break;
                }

                default: break;
            }

			Close();
		}
	}
}
