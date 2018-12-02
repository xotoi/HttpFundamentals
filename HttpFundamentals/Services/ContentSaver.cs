using System;
using System.IO;
using System.Linq;

namespace HttpFundamentals.Services
{
    public interface IContentSaver
    {
        void SaveHtmlDocument(Uri uri, string name, Stream documentStream);
        void SaveFile(Uri uri, Stream fileStream);
    }

    public class ContentSaver : IContentSaver
    {
        private readonly DirectoryInfo _rootDirectory;

        public ContentSaver(DirectoryInfo rootDirectory)
        {
            _rootDirectory = rootDirectory;
        }

        public void SaveHtmlDocument(Uri uri, string name, Stream documentStream)
        {
            var directoryPath = CombineLocations(_rootDirectory, uri);
            Directory.CreateDirectory(directoryPath);
            name = RemoveInvalidSymbols(name);
            var fileFullPath = Path.Combine(directoryPath, name);

            SaveToFile(documentStream, fileFullPath);
            documentStream.Close();
        }

        public void SaveFile(Uri uri, Stream fileStream)
        {
            var fileFullPath = CombineLocations(_rootDirectory, uri);
            var directoryPath = Path.GetDirectoryName(fileFullPath);
            Directory.CreateDirectory(directoryPath);
            if (Directory.Exists(fileFullPath)) // if file name cannot be obtained from uri
            {
                fileFullPath = Path.Combine(fileFullPath, Guid.NewGuid().ToString());
            }

            SaveToFile(fileStream, fileFullPath);
            fileStream.Close();
        }

        private static void SaveToFile(Stream stream, string fileFullPath)
        {
            var createdFileStream = File.Create(fileFullPath);
            stream.CopyTo(createdFileStream);
            createdFileStream.Close();
        }

        private static string CombineLocations(DirectoryInfo directory, Uri uri)
        {
            return Path.Combine(directory.FullName, uri.Host) + uri.LocalPath.Replace("/", @"\");
        }

        private static string RemoveInvalidSymbols(string filename)
        {
            var invalidSymbols = Path.GetInvalidFileNameChars();
            return new string(filename.Where(c => !invalidSymbols.Contains(c)).ToArray());
        }
    }
}
