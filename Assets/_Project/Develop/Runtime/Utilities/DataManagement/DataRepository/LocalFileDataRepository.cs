using System;
using System.IO;
using System.Threading.Tasks;

namespace _Project.Develop.Runtime.Utilities.DataManagement.DataRepository
{
    public class LocalFileDataRepository : IDataRepository
    {
        private readonly string _folderPath;
        private readonly string _saveFileExtension;

        public LocalFileDataRepository(string folderPath, string saveFileExtension)
        {
            if (string.IsNullOrWhiteSpace(folderPath))
                throw new ArgumentException(
                    "Save folder path must not be empty.", nameof(folderPath));

            ValidateFileNamePart(saveFileExtension, nameof(saveFileExtension));
            
            _folderPath = Path.GetFullPath(folderPath);
            _saveFileExtension = saveFileExtension;
        }

        public Task<bool> ExistsAsync(string key)
        {
            string filePath = FullPathFor(key);

            bool exists = File.Exists(filePath);

            return Task.FromResult(exists);
        }

        public Task<string> ReadAsync(string key)
        {
            string filePath = FullPathFor(key);

            return File.ReadAllTextAsync(filePath);
        }

        public Task RemoveAsync(string key)
        {
            string filePath = FullPathFor(key);

            File.Delete(filePath);

            return Task.CompletedTask;
        }

        public async Task WriteAsync(string key, string serializedData)
        {
            if (serializedData == null)
                throw new ArgumentNullException(nameof(serializedData));

            string filePath = FullPathFor(key);

            string temporaryPath = filePath + "." + Guid.NewGuid().ToString("N") + ".tmp";

            Directory.CreateDirectory(_folderPath);

            try
            {
                await File.WriteAllTextAsync(temporaryPath, serializedData);

                if (File.Exists(filePath))
                    File.Replace(temporaryPath, filePath, null);
                else
                    File.Move(temporaryPath, filePath);
            }
            finally
            {
                TryDeleteTemporaryFile(temporaryPath);
            }
        }

        private static void TryDeleteTemporaryFile(string path)
        {
            try
            {
                File.Delete(path);
            }
            catch (IOException)
            {
                // Cleanup failure must not hide the write result.
            }
            catch (UnauthorizedAccessException)
            {
                // The temporary file may remain if access is denied.
            }
        }

        private string FullPathFor(string key)
        {
            ValidateFileNamePart(key, nameof(key));

            return Path.Combine(_folderPath, key + "." + _saveFileExtension);
        }

        private static void ValidateFileNamePart(string value, string parameterName)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException(
                    "Value must not be empty.", parameterName);

            foreach (char character in value)
            {
                bool isAllowed = char.IsLetterOrDigit(character) || character == '_' || character == '-';

                if (!isAllowed)
                    throw new ArgumentException(
                        "Only letters, digits, underscores and hyphens are allowed.", parameterName);
            }
        }
    }
}