using Sharprompt;

using Vadapav;

namespace VadapavStrmGenerator
{
    internal class Program
    {
        private static string _baseAddress;
        private static VadapavClient _client;
        private static string _outputPath;

        static async Task Main()
        {
            Console.Title = "Vadapav strm generator";

            var directoryUrl = Prompt.Input<string>("URL to vadapav directory");
            var directoryUri = new Uri(directoryUrl);

            _baseAddress = directoryUri.GetLeftPart(UriPartial.Authority);
            _client = new VadapavClient(_baseAddress);

            var directoryId = directoryUri.Fragment.Replace("#", string.Empty);
            var directory = await _client.GetDirectoryAsync(directoryId);

            _outputPath = Prompt.Input<string>("Set output path");
            Directory.CreateDirectory(_outputPath);

            var selectionItems = new List<SelectionItem>();

            var directoryNames = directory
                .Directories
                .Where(d => !string.IsNullOrWhiteSpace(d.Name))
                .Select(d => new SelectionItem(d.Name!, d.Id, true));

            var fileNames = directory
                .Files
                .Where(d => !string.IsNullOrWhiteSpace(d.Name))
                .Select(d => new SelectionItem(d.Name!, d.Id, false));

            selectionItems.AddRange(directoryNames);
            selectionItems.AddRange(fileNames);

            var selection = Prompt.MultiSelect("Select items to be transformed to strm", selectionItems);

            foreach (var selectedItem in selection)
            {
                if (selectedItem.IsDirectory)
                {
                    await CreateDirectory(
                        selectedItem.Name,
                        selectedItem.Id,
                        _outputPath);   
                }
                else
                {
                    var content = $"{_baseAddress}/f/{selectedItem.Id}";

                    await CreateStrmFile(
                        selectedItem.Name,
                        content,
                        _outputPath);
                }
            }
        }

        private static async Task CreateDirectory(
            string name,
            Guid id,
            string outputPath)
        {
            var path = Path.Combine(outputPath, name);
            Directory.CreateDirectory(path);

            var currentDirectory = await _client.GetDirectoryAsync(id);

            foreach (var file in currentDirectory.Files)
            {
                if (string.IsNullOrWhiteSpace(file.Name))
                    continue;

                var content = $"{_baseAddress}/f/{file.Id}";
                await CreateStrmFile(file.Name, content, path);
            }

            foreach (var directory in currentDirectory.Directories)
            {
                if (string.IsNullOrWhiteSpace(directory.Name))
                    continue;

                await CreateDirectory(
                    directory.Name,
                    directory.Id,
                    path);

                Console.WriteLine($"Created directory '{directory.Name}'");
            }
        }

        private static async Task CreateStrmFile(
            string fileName,
            string content,
            string outputPath)
        {
            var extension = Path.GetExtension(fileName);
            var name = fileName.Replace(extension, ".strm");
            var path = Path.Combine(outputPath, name);

            await File.WriteAllTextAsync(path, content);

            Console.WriteLine($"Created strm file '{name}'");
        }
    }
}
