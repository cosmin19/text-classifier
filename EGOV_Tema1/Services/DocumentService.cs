using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BigDataProject.Context;
using BigDataProject.Entities.UserForm;

namespace BigDataProject.Services
{
    public class DocumentService : IDocumentService

    {
        #region Fields
        private readonly DbApplicationContext _context;
        #endregion

        #region Ctor
        public DocumentService(DbApplicationContext context)
        {
            this._context = context;
        }
        #endregion

        public void Create(Document entity)
        {
            _context.Documents.Add(entity);

            _context.SaveChanges();
        }

        public IList<Document> GetAllDocuments()
        {
            return _context.Documents.OrderByDescending(d => d.CreatedOnUtc).ToList();
        }

        public Document GetDocumentById(int id)
        {
            return _context.Documents.Where(d => d.Id == id).FirstOrDefault();
        }
    }
}
