using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Library.Common.DTOs;
using Library.Common.Entities;

namespace Library.Common.Utils
{
    public static class MapperUtil
    {
        public static Book ToEntity(this BookDto dto)
        {
            return new Book
            {
                Id = dto.Id ?? Guid.Empty,
                Title = dto.Title,
                Author = dto.Author,
                Publisher = dto.Publisher,
                PublicationYear = dto.PublicationYear,
                PageCount = dto.PageCount,
                Genre = dto.Genre,
                AddedByUserId = dto.AddedByUserId
            };
        }

        public static BookDto ToDto(this Book book)
        {
            return new BookDto
            {
                Id = book.Id,
                Title = book.Title,
                Author = book.Author,
                Publisher = book.Publisher,
                PublicationYear = book.PublicationYear,
                PageCount = book.PageCount,
                Genre = book.Genre
            };
        }
    }
}
