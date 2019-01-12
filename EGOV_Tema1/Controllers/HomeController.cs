using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using BigDataProject.Models;
using BigDataProject.Services;

using System.IO;
using Microsoft.AspNetCore.Http;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using Microsoft.Office.Interop.Word;
using OpenTextSummarizer;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;
using System.Configuration;

namespace BigDataProject.Controllers
{
    public class HomeController : Controller
    {
        #region Services
        private readonly IDocumentService _documentService;
        #endregion

        #region Ctor
        public HomeController(IDocumentService documentService)
        {
            this._documentService = documentService;
        }
        #endregion

        #region Methods
        [HttpGet]
        public IActionResult Index()
        {
            return View(PrepareDocumentsList());
        }

        [HttpPost]
        public IActionResult Index(List<IFormFile> files)
        {
            // Get file
            if (files.Count == 0)
                return View(PrepareDocumentsList());
            var formFile = files[0];

            // Get size
            long size = files.Sum(f => f.Length);

            // Get filepath and extension
            var filePath = System.IO.Path.GetTempFileName();
            var ext = System.IO.Path.GetExtension(formFile.FileName).ToLower();

            // Check extension
            if (ext.Equals(".txt") || ext.Equals(".pdf") || ext.Equals(".doc") || ext.Equals(".docx"))
            {
                if (formFile.Length > 0)
                {
                    // Read from file
                    using (var stream = new MemoryStream())
                    {
                        //Prepare file to Db
                        formFile.CopyTo(stream);
                        Entities.UserForm.Document entity = new Entities.UserForm.Document()
                        {
                            Title = formFile.FileName,
                            Stream = stream.ToArray(),
                            ContentType = formFile.ContentType,
                            Size = size,
                            CreatedOnUtc = DateTime.Now
                        };

                        // Read text from file
                        string fullText = ReadText(ext, entity.Stream, formFile.ContentType);

                        // Summarize text
                        string textSummary = SummarizeText(fullText);

                        // Get summary classification
                        string classification = GetClassification(textSummary);

                        // Save file to db
                        entity.Summary = textSummary;
                        entity.Classification = classification;
                        _documentService.Create(entity);

                        // Transform json into an object
                        var classificationObjResult = JsonConvert.DeserializeObject<ClassificationDto[]>(classification)[0];

                        // Create Dto Response for View
                        var dto = new DocumentSummaryData()
                        {
                            Id = entity.Id,
                            Summary = textSummary,
                            Title = entity.Title,
                            Classification = classificationObjResult.Classification
                                                        .OrderByDescending(c => c.P)
                                                        .Select(t => string.Format(t.ClassName + ": " + "{0:0.0%}", t.P))
                                                        .ToArray()
                        };
                        return View("DocumentSummary", dto);
                    }
                }
            }
            // Invalid extension, add model state error
            else
                ModelState.AddModelError(string.Empty, "Extension not supported - (only *.txt, *.pdf, *.doc, *.docx)");

            // Return View
            return View(PrepareDocumentsList());
        }

        [HttpGet]
        public IActionResult GoToFile(int fileId)
        {
            if (fileId == 0)
                return RedirectToAction(nameof(Index));

            var doc = _documentService.GetDocumentById(fileId);
            if (doc == null)
                return RedirectToAction(nameof(Index));

            // Transform json into an object
            var classificationObjResult = JsonConvert.DeserializeObject<ClassificationDto[]>(doc.Classification)[0];

            // Create Dto Response for View
            var dto = new DocumentSummaryData()
            {
                Id = doc.Id,
                Summary = doc.Summary,
                Title = doc.Title,
                Classification = classificationObjResult.Classification
                                                        .OrderByDescending(c => c.P)
                                                        .Select(t => string.Format(t.ClassName + ": " + "{0:0.0%}", t.P))
                                                        .ToArray()
            };
            return View("DocumentSummary", dto);
        }

        [HttpGet]
        public IActionResult DownloadFile(int fileId)
        {
            if (fileId == 0)
                return RedirectToAction(nameof(Index));

            var doc = _documentService.GetDocumentById(fileId);
            if(doc == null)
                return RedirectToAction(nameof(Index));

            return File(doc.Stream, doc.ContentType, doc.Title);
        }


        [HttpGet]
        public IActionResult DocumentSummary()
        {
            var dto = new DocumentSummaryData()
            {
                Id = 0,
                Summary = "No result.",
                Title = "",
                Classification = new string[] { }
            };

            return View(dto);
        }
        #endregion

        #region Utils
        private IList<DocumentSmall> PrepareDocumentsList()
        {
            IList<Entities.UserForm.Document> docs = _documentService.GetAllDocuments();
            IList<DocumentSmall> docsDtos = new List<DocumentSmall>();

            foreach (var d in docs)
            {
                var temp = new DocumentSmall()
                {
                    Id = d.Id,
                    Title = d.Title
                };
                docsDtos.Add(temp);
            }
            return docsDtos;
        }

        private static string ReadPdfText(byte[] stream)
        {
            PdfReader reader = new PdfReader(stream);
            string text = string.Empty;
            for (int page = 1; page <= reader.NumberOfPages; page++)
            {
                text += PdfTextExtractor.GetTextFromPage(reader, page);
            }
            reader.Close();
            return text;
        }

        private static string ReadTxtText(byte[] stream)
        {
            string line = "";
            try { 
                using (Stream ms = new MemoryStream(stream))
                {
                    using (StreamReader reader = new StreamReader(ms, System.Text.Encoding.UTF8, true))
                    {
                        line = reader.ReadToEnd();
                        return line;
                    }
                }
            }
            catch (Exception)
            {
                return line;
            }
        }

        private string ReadDocText(byte[] stream, string contentType)
        {
            string text = "";
            Application application = null;
            Microsoft.Office.Interop.Word.Document document = null;

            try
            {
                var tmpFile = System.IO.Path.GetTempFileName();
                System.IO.File.WriteAllBytes(tmpFile, stream);

                application = new Application();
                document = application.Documents.Open(tmpFile);

                for (int i = 0; i < document.Paragraphs.Count; i++)
                {
                    string temp = document.Paragraphs[i + 1].Range.Text.Trim();
                    if (temp != string.Empty)
                        //data.Add(temp);
                        text += temp + "\n";
                }

                // Close word.
                document.Close();
                application.Quit();
            }
            catch (Exception)
            {
                if (document != null) document.Close();
                if (application != null) application.Quit();
            }

            return text;
        }

        private string ReadText(string ext, byte[] filePath, string contentType)
        {
            if (ext.Equals(".pdf"))
                return ReadPdfText(filePath);
            else if (ext.Equals(".txt"))
                return ReadTxtText(filePath);
            else if (ext.Equals(".doc"))
                return ReadDocText(filePath, contentType);
            else if (ext.Equals(".docx"))
                return ReadDocText(filePath, contentType);

            return "";
        }

        private string SummarizeText(string text)
        {
            // Set text summarize arguments
            SummarizerArguments sumargs = new SummarizerArguments
            {
                DictionaryLanguage = "en",
                DisplayLines = 5,
                DisplayPercent = 0,
                InputFile = "",
                InputString = text
            };

            // Summarize text
            SummarizedDocument doc = Summarizer.Summarize(sumargs);

            // Return result
            return string.Join("\r\n\r\n", doc.Sentences.ToArray());
        }

        private string GetClassification(string text)
        {
            string result = "";

            // Create JSON content
            var data = new { texts = new[] { text } };
            var dataString = JsonConvert.SerializeObject(data);

            // Open Http Client
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://api.uclassify.com/v1/uclassify/topics/classify/");
                client.DefaultRequestHeaders.Add("Authorization", "Token hN0yj3t6T0fp");

                // Set content
                var content = new StringContent(dataString.ToString(), Encoding.UTF8, "application/json");

                // Create request
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "");
                request.Content = content;

                // Make request and wait for finish
                HttpResponseMessage httpResponse = null;
                var req = client.SendAsync(request)
                .ContinueWith(response =>
                {
                    httpResponse = response.Result;
                });
                req.Wait();

                // Read content async and wait for response
                var reqContent = httpResponse.Content.ReadAsStringAsync().ContinueWith(res => {
                    result = res.Result;
                });
                reqContent.Wait();
            }

            return result;
        }
        #endregion



        #region Common
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        #endregion

    }
}
