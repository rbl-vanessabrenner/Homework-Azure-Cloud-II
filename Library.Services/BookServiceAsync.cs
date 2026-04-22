using System;
using System.Collections.Generic;
using System.Text;
using Library.Common.DTOs;
using Library.Common.Utils;
using Library.Interfaces;

namespace Library.Services
{
    public class BookServiceAsync : IBookServiceAsync
    {
        private readonly IBookRepositoryAsync _bookRepository;

        public BookServiceAsync(IBookRepositoryAsync bookRepository)
        {
            _bookRepository = bookRepository;
        }

        public async Task<BookDto> GetById(Guid id)
        {
            var book = await _bookRepository.GetByIdAsync(id);
            return book?.ToDto();
        }

        public async Task<IEnumerable<BookDto>> Get(int pageNumber = 1, int pageSize = 10)
        {
            var results = (await _bookRepository.GetAsync(pageNumber, pageSize)).ToList();
            return results.Any() ? results.Select(x => x.ToDto()) : new List<BookDto>();
        }


        public async Task<BookDto> Add(BookDto entity)
        {
            var result = await _bookRepository.AddAsync(entity.ToEntity());
            return result.ToDto();
        }

        public async Task<BookDto> Update(BookDto entityToBeUpdated)
        {
            var result = await _bookRepository.UpdateAsync(entityToBeUpdated.ToEntity());
            return result.ToDto();
        }

        public async Task<bool> Remove(Guid id)
        {
            return await _bookRepository.RemoveAsync(id);
        }
    }
}
