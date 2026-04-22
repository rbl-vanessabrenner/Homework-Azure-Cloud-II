using System;
using System.Collections.Generic;
using System.Text;
using Library.Common.Entities;
using Library.Interfaces;

namespace Library.Services {
    public class BookRepositoryAsync : IBookRepositoryAsync {

        private IList<Book> _books;

        public BookRepositoryAsync()
        {
            _books = new List<Book>();
        }
    
        public async Task<Book> GetByIdAsync(Guid id)
        {
            return _books.FirstOrDefault(x => x.Id == id);
        }
        public async Task<IEnumerable<Book>> GetAsync(int pageNumber, int pageSize)
        {
            return _books
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize);
        }
        public async Task<Book> AddAsync(Book entity)
        {
            entity.Id = Guid.NewGuid();
            _books.Add(entity);

            return entity;
        }
        public async Task<Book> UpdateAsync(Book _entityToBeUpdated)
        {
            var entityToBeUpdated = _books.FirstOrDefault(x => x.Id == _entityToBeUpdated.Id);

            if (entityToBeUpdated == null)
                throw new KeyNotFoundException("Item with given id does not exist");

            entityToBeUpdated.Title = _entityToBeUpdated.Title;
            entityToBeUpdated.Author = _entityToBeUpdated.Author;
            entityToBeUpdated.Publisher = _entityToBeUpdated.Publisher;
            entityToBeUpdated.PublicationYear = _entityToBeUpdated.PublicationYear;
            entityToBeUpdated.Genre = _entityToBeUpdated.Genre;
            entityToBeUpdated.PageCount = _entityToBeUpdated.PageCount;

            return entityToBeUpdated;

        }
        public async Task<bool> RemoveAsync(Guid id)
        {
            var entityToBeRemoved = _books.FirstOrDefault(x => x.Id == id);

            if (entityToBeRemoved == null)
                throw new KeyNotFoundException("Item with given id does not exist");

            return _books.Remove(entityToBeRemoved);    
        }
    }
}
