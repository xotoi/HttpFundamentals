using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using HtmlAgilityPack;
using HttpFundamentals.Constraints;

namespace HttpFundamentals.Services
{
    public interface IMainParser
    {
        int MaxDeepLevel { get; set; }
        void LoadFromUrl(string url);
    }

    public class MainDownLoader : IMainParser
    {
        private readonly IContentSaver _contentSaver;
        private readonly List<IConstraint> _urlConstraints;
        private readonly List<IConstraint> _fileConstraints;
        private const string HtmlDocumentMediaType = "text/html";
        private readonly ISet<Uri> _visitedUrls = new HashSet<Uri>();

        public int MaxDeepLevel { get; set; }

        public MainDownLoader(IContentSaver contentSaver, IEnumerable<IConstraint> constraints, int maxDeepLevel = 0)
        {
            _contentSaver = contentSaver;
            _urlConstraints = constraints.Where(c => (c.ConstraintType & ConstraintType.UrlConstraint) != 0).ToList();
            _fileConstraints = constraints.Where(c => (c.ConstraintType & ConstraintType.FileConstraint) != 0).ToList();

            MaxDeepLevel = maxDeepLevel;
        }

        public void LoadFromUrl(string url)
        {
            _visitedUrls.Clear();
            using (var httpClient = new HttpClient())
            {
                httpClient.BaseAddress = new Uri(url);
                ScanUrl(httpClient, httpClient.BaseAddress, 0);
            }
        }

        private void ScanUrl(HttpClient httpClient, Uri uri, int level)
        {
            if (level > MaxDeepLevel || _visitedUrls.Contains(uri) || !IsValidScheme(uri.Scheme))
            {
                return;
            }

            _visitedUrls.Add(uri);

            var head = httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Head, uri)).Result;

            if (!head.IsSuccessStatusCode)
            {
                return;
            }

            if (head.Content.Headers.ContentType?.MediaType == HtmlDocumentMediaType)
            {
                ProcessHtmlDocument(httpClient, uri, level);
            }
            else
            {
                ProcessFile(httpClient, uri);
            }
        }
        private void ProcessFile(HttpClient httpClient, Uri uri)
        {
            Console.WriteLine($"File founded: {uri}");
            if (!IsAcceptableUri(uri, _fileConstraints))
            {
                return;
            }

            var response = httpClient.GetAsync(uri).Result;
            Console.WriteLine($"File loaded: {uri}");
            _contentSaver.SaveFile(uri, response.Content.ReadAsStreamAsync().Result);
        }

        private void ProcessHtmlDocument(HttpClient httpClient, Uri uri, int level)
        {
            Console.WriteLine($"Url founded: {uri}");
            if (!IsAcceptableUri(uri, _urlConstraints))
            {
                return;
            }

            var response = httpClient.GetAsync(uri).Result;
            var document = new HtmlDocument();
            document.Load(response.Content.ReadAsStreamAsync().Result, Encoding.UTF8);
            Console.WriteLine($"Html loaded: {uri}");
            _contentSaver.SaveHtmlDocument(uri, GetDocumentFileName(document), GetDocumentStream(document));

            var attributesWithLinks = document.DocumentNode.Descendants().SelectMany(d => d.Attributes.Where(IsAttributeWithLink));
            foreach (var attributesWithLink in attributesWithLinks)
            {
                ScanUrl(httpClient, new Uri(httpClient.BaseAddress, attributesWithLink.Value), level + 1);
            }
        }

        private static string GetDocumentFileName(HtmlDocument document)
        {
            return document.DocumentNode.Descendants("title").FirstOrDefault()?.InnerText + ".html";
        }

        private static Stream GetDocumentStream(HtmlDocument document)
        {
            var memoryStream = new MemoryStream();
            document.Save(memoryStream);
            memoryStream.Seek(0, SeekOrigin.Begin);
            return memoryStream;
        }

        private static bool IsValidScheme(string scheme)
        {
            switch (scheme)
            {
                case "http":
                case "https":
                    return true;
                default:
                    return false;
            }
        }

        private static bool IsAttributeWithLink(HtmlAttribute attribute)
        {
            return attribute.Name == "src" || attribute.Name == "href";
        }

        private bool IsAcceptableUri(Uri uri, IEnumerable<IConstraint> constraints)
        {
            return constraints.All(c => c.IsAcceptable(uri));
        }
    }
}
