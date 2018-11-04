using BigDataProject.Entities.UserForm;
using System.Collections.Generic;

namespace BigDataProject.Services
{
    public interface IDocumentService
    {
        void Create(Document entity);
        Document GetDocumentById(int id);
        IList<Document> GetAllDocuments();
    }
}
