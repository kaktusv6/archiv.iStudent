using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using ConsoleApplicationArchivy;

namespace ArchivyFiles
{

    class ArchiveSzEntry
    {
        public string FullName;
        public string LastWriteTime;
        public string Length;
        public string CompressedLength;

        public ArchiveSzEntry(string header)
        {
            int i = 0;
            while (header[i] != '|')
            {
                FullName += header[i++];
            }

            while (header[i] != ' '){
                Length += header[++i];
            }
            i++;
            while (header[i] != ' ')
            {
                CompressedLength += header[i++];
            }
            i++;
            while (i <= header.Length-1)
            {
                LastWriteTime += header[i++];
            }
        }
    }
    
    class ArchiveSz
    {
        public ArchiveSzEntry[] Entries;
        private string pathToArchive;

        public ArchiveSz(FileStream archive)
        {
            pathToArchive = archive.Name;

            if (archive.Length == 0)
            {
                Entries = new ArchiveSzEntry[0];
                return;
            }
            
            using (BinaryReader readFile = new BinaryReader(archive))
            {
                char buff = readFile.ReadChar();
                string allFilesStr = string.Empty;
                while (!char.IsLetter(buff)) // парсим количество файлов в архиве
                {
                    if (char.IsDigit(buff))
                    {
                        allFilesStr += buff;
                    }
                    buff = readFile.ReadChar();
                }
                int allHead = Convert.ToInt32(allFilesStr);
                Entries = new ArchiveSzEntry[allHead];
                for (int i = 0; i < allHead; i++)
                {

                    string sizeHeadStr = string.Empty;
                    while (buff != 'd') // парсим длинну заголовка
                    {
                        if (char.IsDigit(buff))
                        {
                            sizeHeadStr += buff;
                        }
                        buff = readFile.ReadChar();
                    }
                    Encoding uni = Encoding.Unicode;
                    int sizeHead = Convert.ToInt32(sizeHeadStr);
                    string header = uni.GetString(readFile.ReadBytes(sizeHead));
                    Entries[i] = new ArchiveSzEntry(header);
                    buff = readFile.ReadChar();

                }
            }
            archive.Close();
        }
        public ArchiveSz(string _pathToArchive)
        {
            using(FileStream archive = new FileStream(_pathToArchive, FileMode.Open))
            {
                pathToArchive = archive.Name;
            
                if (archive.Length == 0)
                {
                    Entries = new ArchiveSzEntry[0];
                    return;
                }
            
                using (BinaryReader readFile = new BinaryReader(archive))
                {
                    char buff = readFile.ReadChar();
                    string allFilesStr = string.Empty;
                
                    while (!char.IsLetter(buff)) // парсим количество файлов в архиве
                    {
                        if (char.IsDigit(buff))
                        {
                            allFilesStr += buff;
                        }
                        buff = readFile.ReadChar();
                    }
                
                    int allHead = Convert.ToInt32(allFilesStr);
                
                    Entries = new ArchiveSzEntry[allHead];

                    for (int i = 0; i < allHead; i++)
                    {
                        string sizeHeadStr = string.Empty;
                
                        while (buff != 'd') // парсим длинну заголовка
                        {
                            if (char.IsDigit(buff))
                            {
                                sizeHeadStr += buff;
                            }
                            buff = readFile.ReadChar();
                        }
                
                        Encoding uni = Encoding.Unicode;
                        int sizeHead = Convert.ToInt32(sizeHeadStr);
                        string header = uni.GetString(readFile.ReadBytes(sizeHead));
                
                        Entries[i] = new ArchiveSzEntry(header);
                        buff = readFile.ReadChar();
                    }
                }
            }
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

                using(FileStream szArch = new FileStream(fileNameSz, FileMode.Open)) {
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
                    
                        header = NameFile + '|' + 
                            Convert.ToString(SizeFileBefore) + " " + 
                            Convert.ToString(SizeFileCode) + " " +
                            data.ToString();


                        byte[] headByte = uni.GetBytes(header);
                        if (allFiles == string.Empty)
                        {
                            Entries = new ArchiveSzEntry[1];
                            ifFileEmpty = false;
                            using (BinaryWriter write = new BinaryWriter(archFile))// запись заголовка
                            {
                                write.Write("1");
                                write.Write('s' + Convert.ToString(headByte.Length) + 'd');
                                write.Write(headByte);
                                Entries[0] = new ArchiveSzEntry(header); 
                            }
                            using (FileStream Arch = new FileStream(pathToArchive, FileMode.Append))//запись содержимого
                            {
                                szArch.CopyTo(Arch);
                            }
                        }
                        else
                        {
                            Entries = new ArchiveSzEntry[Convert.ToInt32(allFiles)];
                            string newArchiv = pathToArchive + "1";
                            using (BinaryWriter newArchiveFile = new BinaryWriter(new FileStream(newArchiv, FileMode.Append)))
                            {
                                int allHead = Convert.ToInt32(allFiles);
                                Entries = new ArchiveSzEntry[allHead+1];
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
                                    Entries[i] = new ArchiveSzEntry(headStr);
                                    string szSnappy = string.Empty;

                                    newArchiveFile.Write(headBytes);
                                                                        
                                    int j = 1;
                                    buff = headStr[0]; // Узнаем длинну закодированного Snappy файла
                                    while (buff != '|'){
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
                                Entries[Entries.Length-1] =new ArchiveSzEntry(header);
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
                File.Delete(fileNameSz);
            }
            if (ifFileEmpty)
            {
                File.Replace(pathToArchive + 1, pathToArchive, "rezerv.txt");
            }
        }

        public void AddFile(FileInfo fileInfo)
        {
            this.AddFile(fileInfo.ToString());
        }

        public void ExtractFile(string pathToFile, string pathFileDirectory)
        {
            using (BinaryReader readFile = new BinaryReader(new FileStream(pathToArchive, FileMode.Open)))
            {
                    char sym = readFile.ReadChar();
                    string allFilesStr = string.Empty;
                    Encoding uni = Encoding.Unicode;

                    while (!char.IsLetter(sym)) // парсим количество файлов в архиве
                    {
                        if (char.IsDigit(sym))
                        {
                            allFilesStr += sym;
                        }
                        sym = readFile.ReadChar();
                    }
                    int before = 0;
                    int toSnappyFile = 0;
                    int sizeAllSnappyCode = 0;
                   
                    int allFiles = Convert.ToInt32(allFilesStr);

                    int k = 0;
                    for (int i = 0; i < allFiles; i++) //парсим голову файлов
                    {
                        int sizeHeadInt = 0;
                        int sizeSnappy = 0;
                        string nameFile = string.Empty;
                        string sizeHeadStr = string.Empty;
                        string header = string.Empty;
                        string sizeSnappyStr = string.Empty;
                        sym = readFile.ReadChar();

                        while (sym != 'd') {
                            if (char.IsDigit(sym)){
                                sizeHeadStr += sym;
                            }
                            sym = readFile.ReadChar();
                        }
                        sizeHeadInt = Convert.ToInt32(sizeHeadStr);
                        header = uni.GetString(readFile.ReadBytes(sizeHeadInt));//считал заголовочный файл

                        int j = 0;
                        
                        while(header[j] != '|'){
                            nameFile += header[j];
                            j++;
                        }
                        j++;
                        while (header[j] != ' ')
                        {
                            sizeSnappyStr += header[j];
                            j++;
                        }

                        sizeSnappy = Convert.ToInt32(sizeSnappyStr);

                        if  (nameFile.Equals(pathToFile)){
                            toSnappyFile = sizeSnappy;
                            before = sizeAllSnappyCode;
                        }
                        else
                        {
                            Entries[k++] = new ArchiveSzEntry(header);
                            sizeAllSnappyCode += sizeSnappy;
                        }
                    }

                    string fullPath = pathFileDirectory + "\\" + pathToFile + ".sz";
                    using (BinaryWriter SnappyFile = new BinaryWriter(new FileStream(fullPath, FileMode.Append)))
                    {
                        readFile.ReadBytes(before);
                        SnappyFile.Write(readFile.ReadBytes(toSnappyFile));
                    }
                    ArchivySnappy.Decompress(fullPath);
                    File.Delete(fullPath);
            }
        }

        //public void ExtractFile(string pathToFile, string pathFileDirectory)
        //{
        //    using (BinaryReader readFile = new BinaryReader(new FileStream(pathToArchive, FileMode.Open)))
        //    {
        //        using (BinaryWriter newFile = new BinaryWriter(new FileStream(pathToArchive + 1, FileMode.Append)))
        //        {
        //            char sym = readFile.ReadChar();
        //            string allFilesStr = string.Empty;
        //            Encoding uni = Encoding.Unicode;
        //            Entries = new ArchiveSzEntry[Entries.Length];

        //            while (!char.IsLetter(sym)) // парсим количество файлов в архиве
        //            {
        //                if (char.IsDigit(sym))
        //                {
        //                    allFilesStr += sym;
        //                }
        //                sym = readFile.ReadChar();
        //            }
        //            int before = 0;
        //            int toSnappyFile = 0;
        //            int sizeAllSnappyCode = 0;
                    
        //            int allFiles = Convert.ToInt32(allFilesStr);
        //            if ((allFiles - 1) != 0)
        //            {
        //                newFile.Write(Convert.ToString(allFiles));
        //            }
        //            int k = 0;
        //            for (int i = 0; i < allFiles; i++) //парсим голову файлов
        //            {
        //                int sizeHeadInt = 0;
        //                int sizeSnappy = 0;
        //                string nameFile = string.Empty;
        //                string sizeHeadStr = string.Empty;
        //                string header = string.Empty;
        //                string sizeSnappyStr = string.Empty;
        //                sym = readFile.ReadChar();

        //                while (sym != 'd') {
        //                    if (char.IsDigit(sym)){
        //                        sizeHeadStr += sym;
        //                    }
        //                    sym = readFile.ReadChar();
        //                }
        //                sizeHeadInt = Convert.ToInt32(sizeHeadStr);
        //                header = uni.GetString(readFile.ReadBytes(sizeHeadInt));//считал заголовочный файл

        //                int j = 0;
                        
        //                while(header[j] != '|'){
        //                    nameFile += header[j];
        //                    j++;
        //                }
        //                j++;
        //                while (header[j] != ' ')
        //                {
        //                    sizeSnappyStr += header[j];
        //                    j++;
        //                }

        //                sizeSnappy = Convert.ToInt32(sizeSnappyStr);
        //                Entries[k] = new ArchiveSzEntry(header);

        //                if  (nameFile.Equals(pathToFile)){
        //                    toSnappyFile = sizeSnappy;
        //                    before = sizeAllSnappyCode;
        //                }
        //                else
        //                {
        //                    newFile.Write('s' + sizeHeadStr + 'd');
        //                    newFile.Write(uni.GetBytes(header));
        //                    sizeAllSnappyCode += sizeSnappy;
        //                    k++;
        //                }
        //            }
        //            newFile.Write(readFile.ReadBytes(before));
        //            string fullPath = pathFileDirectory + "\\" + pathToFile + ".sz";
        //            using (BinaryWriter SnappyFile = new BinaryWriter(new FileStream(fullPath, FileMode.Append)))
        //            {
        //                SnappyFile.Write(readFile.ReadBytes(toSnappyFile));
        //            }
        //            newFile.Write(readFile.ReadBytes(sizeAllSnappyCode - before));
        //            ArchivySnappy.Decompress(fullPath);
        //            File.Delete(fullPath);
        //        }
        //    }
        //    File.Replace(pathToArchive + 1, pathToArchive, "rezerv.txt");
        //}

        public void DeleteFile(string pathToFile)
        {
            using (BinaryReader readFile = new BinaryReader(new FileStream(pathToArchive, FileMode.Open)))
            {
                using (BinaryWriter newFile = new BinaryWriter(new FileStream(pathToArchive + 1, FileMode.Append)))
                {
                    char sym = readFile.ReadChar();
                    string allFilesStr = string.Empty;
                    Encoding uni = Encoding.Unicode;
                    Entries = new ArchiveSzEntry[Entries.Length - 1];

                    while (!char.IsLetter(sym)) // парсим количество файлов в архиве
                    {
                        if (char.IsDigit(sym))
                        {
                            allFilesStr += sym;
                        }
                        sym = readFile.ReadChar();
                    }
                    int before = 0;
                    int toDeletedFile = 0;
                    int sizeAllCode = 0;

                    int allFiles = Convert.ToInt32(allFilesStr);
                    if ((allFiles - 1) != 0)
                    {
                        newFile.Write(Convert.ToString(allFiles - 1));
                    }
                    int k = 0;
                    for (int i = 0; i < allFiles; i++) //парсим голову файлов
                    {
                        int sizeHeadInt = 0;
                        int sizeSnappy = 0;
                        string nameFile = string.Empty;
                        string sizeHeadStr = string.Empty;
                        string header = string.Empty;
                        string sizeSnappyStr = string.Empty;
                        sym = readFile.ReadChar();

                        while (sym != 'd')
                        {
                            if (char.IsDigit(sym))
                            {
                                sizeHeadStr += sym;
                            }
                            sym = readFile.ReadChar();
                        }
                        sizeHeadInt = Convert.ToInt32(sizeHeadStr);
                        header = uni.GetString(readFile.ReadBytes(sizeHeadInt));//считал заголовочный файл

                        int j = 0;

                        while (header[j] != '|')
                        {
                            nameFile += header[j];
                            j++;
                        }
                        j++;
                        while (header[j] != ' ')
                        {
                            sizeSnappyStr += header[j];
                            j++;
                        }

                        sizeSnappy = Convert.ToInt32(sizeSnappyStr);

                        if (nameFile.Equals(pathToFile))
                        {
                            toDeletedFile = sizeSnappy;
                            before = sizeAllCode;
                        }
                        else
                        {
                            Entries[k] = new ArchiveSzEntry(header);
                            newFile.Write('s' + sizeHeadStr + 'd');
                            newFile.Write(uni.GetBytes(header));
                            sizeAllCode += sizeSnappy;
                            k++;
                        }
                    }
                    newFile.Write(readFile.ReadBytes(before));

                    newFile.Write(readFile.ReadBytes(sizeAllCode - before));
                }
            }
            File.Replace(pathToArchive + 1, pathToArchive, "rezerv.txt");
        }
    }
}
