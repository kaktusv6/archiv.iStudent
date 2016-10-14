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
			if (args.Length == 0 || args.Length == 1)
			{
				Console.WriteLine("Введите два аргумента:");
				Console.WriteLine("1. <mode>	Ражим архиватора:	--com для сжатия файла");
				Console.WriteLine("									--uncom для декодирования файла");
				Console.WriteLine("2. [files, ] Имена файлов которые вы хотите сжать или декодировать");
			}
			else
			{
				string mode = args[0];
				if (mode == "--com")
				{
                    string nameFile = "";
                    for (int i = 1; i < args.Length; i++)
                    {
                        // Запоминаем имя файла которое передается как параметр при запуске программы
                        nameFile = args[i];
                        byte[] data = Encoding.UTF8.GetBytes(args[i]);

                        // Создание файлого потока для считывания данных
                        var fileRead = File.OpenRead(nameFile);
                        var reader = new StreamReader(fileRead);
                        //Path.ChangeExtension(nameFile,  ".snz");
                        

                        // Создание файлого потока для записи
                        FileStream fileWrite = File.OpenWrite(Path.ChangeExtension(nameFile, ".snz"));

                        // Создаем SnappyStream для сжатия данных при взятие данных из потока
                        
                        //var writ = SnappyCodec.Compress(data);
                        SnappyStream compressor = new SnappyStream(fileWrite, CompressionMode.Compress);
                        //fileWrite.Write(writ, 0, writ.Length);

                        // Создаем поток который запишет сжатые файлы и сразу передаем данные из считываемого файла
                        using (var writer = new StreamWriter(compressor))
                        {
                           writer.Write(reader.ReadToEnd());
                            writer.Close();
                        }
                        //File.WriteAllBytes(nameFile, writ);

                        fileWrite.Close();
                        fileRead.Close();
                        reader.Close();
                    }
				}
				else if (mode == "--uncom")
				{
					for (int i = 1; i < args.Length; i++)
					{
						// Запоминаем имя файла которое передается как параметр при запуске программы
						string nameFile = args[i];

						// Создание файлого потока для считывания данных
						FileStream fileRead = File.OpenRead(nameFile);

						// Создаем SnappyStream для декодирования данных при взятие данных из потока
                        SnappyStream decompressor = new SnappyStream(fileRead, CompressionMode.Decompress);
						// Создаем поток который считает сжатые файлы и сразу передаем данные из считываемого файла
						StreamReader reader = new StreamReader(decompressor);
						// Создание файлого потока для записи
						FileStream fileWrite = File.OpenWrite(nameFile.Substring(0, nameFile.Length - 3));
						using (var writer = new StreamWriter(fileWrite))
						{
							writer.Write(reader.ReadToEnd());
							writer.Close();
						}
						reader.Close();
						fileRead.Close();
						fileWrite.Close();
					}
				}
			}
        }
    }
}
