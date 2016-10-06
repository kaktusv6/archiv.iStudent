using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.IO.Compression;

// Подключение библиотекаи Snappy
using Snappy;


namespace ArchivyConsole
{
    class ArchivyConsole
    {
	    static void Main(string[] args)
        {
			// Запоминаем имя файла которое передается как параметр при запуске программы
            string nameFile = args.Length > 0 ? @args[0] : "input.in";

			// Создание файлого потока для считывания данных
            FileStream fileRead = File.OpenRead(nameFile);
            StreamReader reader = new StreamReader(fileRead);

			// Создание файлого потока для записи
            FileStream fileWrite = File.OpenWrite(nameFile + ".sz");
			// Создаем SnappyStream для сжатия данных при взятие данных из потока
            SnappyStream compressor = new SnappyStream(fileWrite, CompressionMode.Compress);
			// Создаем поток который запишет сжатые файлы и сразу передаем данный из считываемого файла
            using (var writer = new StreamWriter(compressor))
			{
				writer.Write(reader.ReadToEnd());
			}

			// Console.ReadLine();
        }
    }
}
