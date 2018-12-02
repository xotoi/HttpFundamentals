using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using HttpFundamentals.Constraints;
using HttpFundamentals.Services;

namespace HttpFundamentals
{
    class Program
    {
        public const string OutputDirectoryPath = @"c:\TestDownloader\";
        public const string StartingUrl = "https://www.epam.com/";
        public const string AvailableExtensions = "pdf,css,js";
        public const int DeepLevel = 1;
        public const CrossDomainTransition Cdt = CrossDomainTransition.All;

        static void Main(string[] args)
        {
            var rootDirectory = new DirectoryInfo(OutputDirectoryPath);
            var contentSaver = new ContentSaver(rootDirectory);
            var constraints = GetConstraintsFromOptions();
            var downloader = new MainDownLoader(contentSaver, constraints, DeepLevel);

            try
            {
                downloader.LoadFromUrl(StartingUrl);
                Console.ReadKey();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during site downloading: {ex.Message}");
            }
        }

        public static List<IConstraint> GetConstraintsFromOptions()
        {
            var constraints = new List<IConstraint>();

            if (!string.IsNullOrEmpty(AvailableExtensions))
            {
                constraints.Add(new FileTypesConstraint(AvailableExtensions.Split(',').Select(e => "." + e)));
            }

            constraints.Add(new CrossDomainTransitionConstraint(Cdt, new Uri(StartingUrl)));

            return constraints;
        }
    }
}
