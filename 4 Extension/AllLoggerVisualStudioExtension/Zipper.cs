using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ICSharpCode.SharpZipLib.Checksum;
using ICSharpCode.SharpZipLib.Zip;

namespace AllLoggerVisualStudioExtension {
    /// <summary>
    /// 压缩工具。
    /// </summary>
    internal class Zipper {
        /// <summary>
        /// 压缩文件。
        /// </summary>
        /// <param name="sourceFilePath">待压缩的路径，可能包含子文件夹。</param>
        /// <param name="destinationZipFilePath">要存放zip文件的目录。</param>
        public void Zip(string sourceFilePath, string destinationZipFilePath) {
            string rootDir = sourceFilePath + "\\";

            if (sourceFilePath[sourceFilePath.Length - 1] !=
                Path.DirectorySeparatorChar) {
                sourceFilePath += Path.DirectorySeparatorChar;
            }

            ZipOutputStream zipStream =
                new ZipOutputStream(File.Create(destinationZipFilePath));
            zipStream.SetLevel(6);

            ZipFiles(sourceFilePath, zipStream, rootDir);

            zipStream.Finish();
            zipStream.Close();
        }

        /// <summary>
        /// 递归压缩文件。
        /// </summary>
        /// <param name="sourceFilePath">待压缩的路径，可能包含子文件夹，每次递归时不一样。</param>
        /// <param name="zipStream">待压缩的zip流。</param>
        /// <param name="rootDir">总的根路径，就是某个项目文件夹。</param>
        private void ZipFiles(string sourceFilePath, ZipOutputStream zipStream,
            string rootDir) {
            Crc32 crc = new Crc32();
            string[] filesArray =
                Directory.GetFileSystemEntries(sourceFilePath);
            foreach (string file in filesArray) {
                if (Directory.Exists(file)) {
                    string currentFolder = new DirectoryInfo(file).Name;
                    if (currentFolder == "obj" || currentFolder == "bin") {
                        continue;
                    }

                    ZipFiles(file, zipStream, rootDir);
                } else {
                    if (!(file.EndsWith(".cs") || file.EndsWith(".xaml"))) {
                        continue;
                    }

                    FileStream fileStream = File.OpenRead(file);
                    byte[] buffer = new byte[fileStream.Length];
                    fileStream.Read(buffer, 0, buffer.Length);
                    string tempFile = file.Replace(rootDir, "");
                    ZipEntry entry = new ZipEntry(tempFile);
                    entry.DateTime = DateTime.Now;
                    entry.Size = fileStream.Length;
                    fileStream.Close();
                    crc.Reset();
                    crc.Update(buffer);
                    entry.Crc = crc.Value;
                    zipStream.PutNextEntry(entry);
                    zipStream.Write(buffer, 0, buffer.Length);
                }
            }
        }
    }
}