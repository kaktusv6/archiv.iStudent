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
	/// Логика взаимодействия для WindowRename.xaml
	/// </summary>
	public partial class WindowRename : Window
	{
		string pathToArchive = string.Empty;
		ZipArchiveEntry entyre;

		public WindowRename()
		{
			InitializeComponent();
		}
		public WindowRename(string path, ZipArchiveEntry e)
		{
			pathToArchive = path;
			entyre = e;
		}
	}
}
