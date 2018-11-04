using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using BigDataProject.Models;
using BigDataProject.Services;
using BigDataProject.Entities.UserForm;
using System.Xml.Linq;

using System.IO;
using Microsoft.AspNetCore.Http;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using Microsoft.Office.Interop.Word;

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

                    using (var stream = new MemoryStream())
                    {
                        formFile.CopyTo(stream);
                        Entities.UserForm.Document entity = new Entities.UserForm.Document()
                        {
                            Title = formFile.FileName,
                            Stream = stream.ToArray(),
                            ContentType = formFile.ContentType,
                            Size = size,
                            CreatedOnUtc = DateTime.Now
                        };

                        _documentService.Create(entity);

                        var result = ReadText(ext, entity.Stream, formFile.ContentType);
                        var dto = new DocumentSummaryData()
                        {
                            Id = entity.Id,
                            Text = result,
                            Title = entity.Title
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

            var ext = System.IO.Path.GetExtension(doc.Title).ToLower();
            var result = ReadText(ext, doc.Stream, doc.ContentType);

            var dto = new DocumentSummaryData()
            {
                Id = doc.Id,
                Text = result,
                Title = doc.Title
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
                Text = "No result.",
                Title = ""
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
            catch (Exception e)
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
            catch (Exception e)
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
