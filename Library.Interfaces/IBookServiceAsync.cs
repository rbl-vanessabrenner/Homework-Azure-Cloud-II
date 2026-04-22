using System;
using System.Collections.Generic;
using System.Text;
using Library.Common.DTOs;
using Library.Common.Entities;

namespace Library.Interfaces
{
    public interface IBookServiceAsync
    {
        Task<BookDto> GetById(Guid id);
        Task<IEnumerable<BookDto>> Get(int pageNumber = 1, int pageSize = 10);
        Task<BookDto> Add(BookDto entity);
        Task<BookDto> Update(BookDto updatedEntity);
        Task<bool> Remove(Guid id);
    }
}
