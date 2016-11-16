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
        String pathToArchiv;
        public ArchiveSz(FileStream archive)
        {
            FileInfo info = new FileInfo(archive.Name);
            pathToArchiv = info.FullName;
        }
        public void Update(string PathToFile)
        {
            using (FileStream archFile = new FileStream(pathToArchiv, FileMode.Open))
            {                
                string fileNameSz = ArchivySnappy.Compress(PathToFile);
                FileInfo info = new FileInfo(fileNameSz);
                long SizeFileBefore = info.Length;

                info = new FileInfo(PathToFile);
                long SizeFileCode = info.Length;

                string allFiles = null;
                string header = "";

                using(FileStream szArch = new FileStream(fileNameSz, FileMode.Open)){
                    
                    string NameFile = info.Name;
                    DateTime data = info.LastWriteTime;

                    using (StreamReader readFile = new StreamReader(archFile))
                    {
                        FileInfo buff = new FileInfo(pathToArchiv);
                        if (buff.Length > 0)
                        {
                            allFiles = readFile.ReadLine();
                        }
                    

                    header = NameFile + " " +
                        Convert.ToString(SizeFileBefore) + " " +
                        Convert.ToString(SizeFileCode) + " " +
                        data.ToString();

                    byte[] headByte = new byte[1000];
                    Encoding uni = Encoding.Unicode;
                    headByte = uni.GetBytes(header);
                        if (allFiles == null)
                        {
                            using(StreamWriter write = new StreamWriter(archFile)){
                                write.WriteLine("1");
                                write.WriteLine(headByte);
                            }
                                using (FileStream Arch = new FileStream(pathToArchiv, FileMode.Append))
                                {
                                    szArch.CopyTo(Arch);
                                }
                        }
                        else
                        {
                            string newArchiv = pathToArchiv + "1";
                            using (StreamWriter newArchiveFile = new StreamWriter(newArchiv))
                            {
                                using (StreamReader FileArchiveRead = new StreamReader(archFile)){
                                    newArchiveFile.WriteLine(Convert.ToString(Convert.ToInt32(allFiles) + 1));
                                    for (int i = 0; i < Convert.ToInt32(allFiles); i++)
                                    {
                                        newArchiveFile.WriteLine(FileArchiveRead.ReadLine());//Считываем посточно: одна строка - один блок байтов(файл) в архиве
                                    }
                                    newArchiveFile.WriteLine(headByte);
                                    for (int i = 0; i < Convert.ToInt32(allFiles); i++)
                                    {
                                        newArchiveFile.Write(FileArchiveRead.Read());//Считываем посточно: одна строка - один блок байтов(файл) в архиве
                                    }
                                }   
                            }
                            using (FileStream Arch = new FileStream(pathToArchiv, FileMode.Append))
                            {
                                szArch.CopyTo(Arch);
                            }
                        }
                    } 
                }
            }
        }
    }
}
