using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Json.Serialization;
using Library.Common.Enums;

namespace Library.API.Models
{
    public class BookModel
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Title is required")]
        [MinLength(2, ErrorMessage = "Length must be at least 2 characters")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Author is required")]
        [MinLength(2, ErrorMessage = "Length must be at least 2 characters")]
        public string Author { get; set; } = string.Empty;

        [Required(ErrorMessage = "Publisher is required")]
        [MinLength(2, ErrorMessage = "Length must be at least 2 characters")]
        public string Publisher { get; set; } = string.Empty;

        [Required(ErrorMessage = "PublicationYear is required")]
        [Range(1450, 2026, ErrorMessage = "PublicationYear must be in range 1450-2026")]
        public int PublicationYear { get; set; }

        [Required(ErrorMessage = "PageCount is required")]
        [Range(1, 10000, ErrorMessage = "PageCount must be in range 1-10000")]
        public int PageCount { get; set; }

        [Required]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public Genre Genre { get; set; }
    }
}
