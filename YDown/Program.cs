using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using YoutubeExplode;
using YoutubeExplode.Models;

namespace YDown
{
    class Program
    {

        static void Main(string[] args)
        {
            Console.Title = "Youtube Downloader";
            
            string file = @"songs.txt";
            string dirName = Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase);
            dirName = dirName.Replace("file:\\", "");
            string filePath = Path.Combine(dirName, file);
            if (File.Exists(filePath))
            {
                foreach (string song in File.ReadAllLines(filePath))
                {
                    try
                    {
                        Console.Write($"Downloading {song}");
                        download(song);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Downloaderror ", e);
                    }
                }

            }
            else
            {
                Console.WriteLine($"Could not find {filePath} ");
            }
            Console.ReadLine();
        }

        /// <summary>
        /// If given a youtube url, parses video id from it.
        /// Otherwise returns the same string.
        /// </summary>
        private static string NormalizeId(string input)
        {
            string id;
            if (!YoutubeClient.TryParseVideoId(input, out id))
                id = input;
            return id;
        }

        /// <summary>
        /// Turns file size in bytes into human-readable string
        /// </summary>
        private static string NormalizeFileSize(long fileSize)
        {
            string[] units = { "B", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };
            double size = fileSize;
            int unit = 0;

            while (size >= 1024)
            {
                size /= 1024;
                ++unit;
            }

            return $"{size:0.#} {units[unit]}";
        }
        
        private static async void download(string link)
        {
            try
            {

                link = NormalizeId(link);

                // Get the video info
                Console.WriteLine("Loading...");
                // Client
                var client = new YoutubeClient();

                var videoInfo = await client.GetVideoInfoAsync(link);
                Console.WriteLine('-');

                // Print metadata
                Console.WriteLine($"Id: {videoInfo.Id} | Title: {videoInfo.Title} | Author: {videoInfo.Author.Title}");

                // Get the most preferable stream
                Console.WriteLine("Looking for the best mixed stream...");
                var streamInfo = videoInfo.AudioStreams
                    .OrderBy(s => s.Bitrate)
                    .Last();
                string normalizedFileSize = NormalizeFileSize(streamInfo.ContentLength);
                Console.WriteLine($"Quality: {streamInfo.Bitrate} | Container: {streamInfo.Container} | Size: {normalizedFileSize}");

                // Compose file name, based on metadata
                string fileExtension = streamInfo.Container.GetFileExtension();
                string downloadFolder = @"C:\temp\";
                string fileName = $"{downloadFolder}{videoInfo.Title}.{fileExtension}";

                // Remove illegal characters from file name

                // Download video
                Console.WriteLine($"Downloading to [{fileName}]...");

                var progress = new Progress<double>(p => Console.Title = $"YoutubeExplode Demo [{p:P0}]");
                await client.DownloadMediaStreamAsync(streamInfo, fileName, progress);

                Console.WriteLine("Download complete!");
            }
            catch (Exception e)
            {
                Console.WriteLine("Error downloading {0}", e);
            }
        }
    }
}
