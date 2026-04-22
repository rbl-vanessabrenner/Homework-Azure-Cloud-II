using System;
using System.Collections.Generic;
using System.Text;
using Library.Common.Enums;

namespace Library.Common.DTOs
{
    public class BookDto
    {
        public Guid? Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public string Publisher { get; set; } = string.Empty;
        public int PublicationYear { get; set; }    
        public int PageCount { get; set; }
        public Genre Genre { get; set; }

        public Guid AddedByUserId { get; set; }
    }
}
