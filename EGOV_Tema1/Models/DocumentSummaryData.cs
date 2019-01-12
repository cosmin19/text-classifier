using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BigDataProject.Models
{
    public class DocumentSummaryData
    {
        public int Id { get; set; }
        public string Summary { get; set; }
        public string Title { get; set; }
        public string[] Classification { get; set; }
    }
}
