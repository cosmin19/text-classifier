using System.Collections.Generic;

namespace BigDataProject.Models
{
    public class ClassificationDto
    {
        public decimal TextCoverage { get; set; }
        public List<ClassificationEntityDto> Classification { get; set; }
    }

    public class ClassificationEntityDto
    {
        public string ClassName { get; set; }
        public decimal P { get; set; }
    }
}
