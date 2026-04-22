using System;
using System.Collections.Generic;
using System.Text;
using Library.Common.Entities;

namespace Library.Interfaces
{
    public interface IBookRepositoryAsync
    {
        Task<Book> GetByIdAsync(Guid id);
        Task<IEnumerable<Book>> GetAsync(int pageNumber, int pageSize);
        Task<Book> AddAsync(Book entity);
        Task<Book> UpdateAsync(Book updatedEntity);
        Task<bool> RemoveAsync(Guid id);
    }
}