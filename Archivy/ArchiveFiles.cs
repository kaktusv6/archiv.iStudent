using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using ConsoleApplicationArchivy;

namespace ArchivyFiles
{
    
    class ArchiveSz
    {
        private string pathToArchive;

        public ArchiveSz(FileStream archive)
        {
            FileInfo info = new FileInfo(archive.Name);
            pathToArchive = info.FullName;
        }
        public void AddFile(string pathToFile)
        {
            bool ifFileEmpty = true;
            using (FileStream archFile = new FileStream(pathToArchive, FileMode.Open))
            {                
                string fileNameSz = ArchivySnappy.Compress(pathToFile);
                FileInfo info = new FileInfo(fileNameSz);
                long SizeFileBefore = info.Length;

                info = new FileInfo(pathToFile);
                long SizeFileCode = info.Length;

                string allFiles = string.Empty;
                string header = string.Empty;
                string sizeByteHead = string.Empty;

                using(FileStream szArch = new FileStream(fileNameSz, FileMode.Open)){
                    Encoding uni = Encoding.Unicode;
                    string NameFile = info.Name;
                    DateTime data = info.LastWriteTime;

                    using (BinaryReader readFile = new BinaryReader(archFile))
                    {
                        FileInfo infoArchive = new FileInfo(pathToArchive);
                        if (infoArchive.Length > 0)
                        {
                            char buff = readFile.ReadChar();
                            while (! char.IsLetter(buff)) // парсим количество файлов в архиве
                            {
                                if (char.IsDigit(buff)){
                                    allFiles += buff;
                                }
                                buff = readFile.ReadChar();
                            }
                        }
                    
                        header = NameFile + " " +
                            Convert.ToString(SizeFileBefore) + " " +
                            Convert.ToString(SizeFileCode) + " " +
                            data.ToString();
                        
                        byte[] headByte = uni.GetBytes(header);
                        string sv = uni.GetString(headByte);
                        if (allFiles == string.Empty)
                        {
                            ifFileEmpty = false;
                            using (BinaryWriter write = new BinaryWriter(archFile))// запись заголовка
                            {
                                write.Write("1");
                                write.Write('s' + Convert.ToString(headByte.Length) + 'd');
                                write.Write(headByte);
                            }
                            using (FileStream Arch = new FileStream(pathToArchive, FileMode.Append))//запись содержимого
                            {
                                szArch.CopyTo(Arch);
                            }
                        }
                        else
                        {
                            string newArchiv = pathToArchive + "1";
                            using (BinaryWriter newArchiveFile = new BinaryWriter(new FileStream(newArchiv, FileMode.Append)))
                            {
                                int allHead = Convert.ToInt32(allFiles);
                                int sizeHead = 0;
                                newArchiveFile.Write(Convert.ToString(allHead + 1));
                                int[] allFilesArr = new int[allHead];

                                for (int i = 0; i < allHead; i++)
                                {
                                    char buff = readFile.ReadChar();
                                    string buffStr = string.Empty;
                                    while (buff != 'd') // парсим длинну заголовка
                                    { 
                                        if (char.IsDigit(buff))
                                        {
                                            buffStr += buff;
                                        }
                                        buff = readFile.ReadChar();
                                    }
                                    if (buffStr != string.Empty)
                                    {
                                        sizeHead = Convert.ToInt32(buffStr);
                                    }
                                    newArchiveFile.Write('s' + Convert.ToString(sizeHead) + 'd');

                                    byte[] headBytes = readFile.ReadBytes(sizeHead);
                                    string headStr = uni.GetString(headBytes);
                                    string szSnappy = string.Empty;

                                    newArchiveFile.Write(headBytes);
                                                                        
                                    int j = 1;
                                    buff = headStr[0]; // Узнаем длинну закодированного Snappy файла
                                    while (buff != ' '){
                                        buff = headStr[j];
                                        j++;
                                    }
                                    buff = headStr[j];
                                    while(buff != ' '){
                                        szSnappy += headStr[j];
                                        j++;
                                        buff = headStr[j];
                                    }
                                    allFilesArr[i] = Convert.ToInt32(szSnappy);
                                }
                                newArchiveFile.Write('s');
                                newArchiveFile.Write(Convert.ToString(headByte.Length));
                                newArchiveFile.Write('d');
                                newArchiveFile.Write(headByte);

                                for (int j = 0; j < allFilesArr.Length; j++)
                                {
                                    newArchiveFile.Write(readFile.ReadBytes(allFilesArr[j]));
                                }
                            }
                            using (FileStream Arch = new FileStream(newArchiv, FileMode.Append))
                            {
                                szArch.CopyTo(Arch);
                            }
                        }
                    } 
                }
            }
            if (ifFileEmpty)
            {
                File.Replace(pathToArchive + 1, pathToArchive, "rezerv.txt");
            }
        }
        public void extractFile(string pathToFile)
        {

        }
    }
}
