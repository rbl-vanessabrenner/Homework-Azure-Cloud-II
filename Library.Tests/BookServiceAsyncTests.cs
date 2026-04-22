using System;
using System.Threading.Tasks;
using Library.Common.DTOs;
using Library.Common.Entities;
using Library.Common.Enums;
using Library.Interfaces;
using Library.Services;
using Moq;
using Xunit;

namespace Library.Tests
{
    public class BookServiceAsyncTests
    {
        private readonly Mock<IBookRepositoryAsync> _mockBookRepo;

        private readonly BookServiceAsync _bookService;

        public BookServiceAsyncTests()
        {
            _mockBookRepo = new Mock<IBookRepositoryAsync>();
            _bookService = new BookServiceAsync(_mockBookRepo.Object);
        }

        // Get by id

        [Fact]
        public async Task GetById_WhenEntityDoesNotExist_ReturnsNull()
        {
            // Arrange
            var id = Guid.NewGuid();

            _mockBookRepo.Setup(repo => repo.GetByIdAsync(id))
                         .ReturnsAsync((Book)null);

            // Act
            var result = await _bookService.GetById(id);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetById_WhenEntityExists_ReturnsMappedBookDto()
        {
            // Arrange
            var id = Guid.NewGuid();

            var fakeBook = new Book
            {
                Id = id,
                Title = "Test Book",
                Author = "Test Author",
                Publisher = "Test Publisher",
                PublicationYear = 2012,
                PageCount = 100,
                Genre = Genre.Romance
            };

            _mockBookRepo.Setup(repo => repo.GetByIdAsync(id))
                         .ReturnsAsync(fakeBook);

            // Act
            var result = await _bookService.GetById(id);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<BookDto>(result);

            Assert.Equal(fakeBook.Id, result.Id);
            Assert.Equal(fakeBook.Title, result.Title);
            Assert.Equal(fakeBook.Author, result.Author);
            Assert.Equal(fakeBook.Publisher, result.Publisher);
            Assert.Equal(fakeBook.PublicationYear, result.PublicationYear);
            Assert.Equal(fakeBook.PageCount, result.PageCount);
            Assert.Equal(fakeBook.Genre, result.Genre);
        }

        // Get all

        [Fact]
        public async Task GetAll_WhenNoBooksExist_ReturnsEmptyList()
        {
            // Arrange
            _mockBookRepo.Setup(repo => repo.GetAsync(It.IsAny<int>(), It.IsAny<int>()))
                         .ReturnsAsync(new List<Book>());

            // Act
            var result = await _bookService.Get();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAll_WhenBooksExist_ReturnsMappedBookDtos()
        {
            // Arrange
            var fakeBooksList = new List<Book>
            {
                new Book {
                    Id = Guid.NewGuid(),
                    Title = "Test Book 1",
                    Author = "Test Author 1",
                    Publisher = "Test Publisher 1",
                    PublicationYear = 2012,
                    PageCount = 100,
                    Genre = Genre.Romance
                },
                new Book {
                    Id = Guid.NewGuid(),
                    Title = "Test Book 2",
                    Author = "Test Author 2",
                    Publisher = "Test Publisher 2",
                    PublicationYear = 2012,
                    PageCount = 100,
                    Genre = Genre.Romance
                }
            };

            _mockBookRepo.Setup(repo => repo.GetAsync(It.IsAny<int>(), It.IsAny<int>()))
                         .ReturnsAsync(fakeBooksList);

            // Act
            var result = await _bookService.Get();

            // Assert
            Assert.NotNull(result);

            var returnedBooksList = result.ToList();

            Assert.Equal(2, returnedBooksList.Count);

            Assert.Equal(fakeBooksList[0].Id, returnedBooksList[0].Id);
            Assert.Equal(fakeBooksList[0].Title, returnedBooksList[0].Title);
            Assert.Equal(fakeBooksList[0].Author, returnedBooksList[0].Author);
            Assert.Equal(fakeBooksList[0].Publisher, returnedBooksList[0].Publisher);
            Assert.Equal(fakeBooksList[0].PublicationYear, returnedBooksList[0].PublicationYear);
            Assert.Equal(fakeBooksList[0].PageCount, returnedBooksList[0].PageCount);
            Assert.Equal(fakeBooksList[0].Genre, returnedBooksList[0].Genre);

            Assert.Equal(fakeBooksList[1].Id, returnedBooksList[1].Id);
            Assert.Equal(fakeBooksList[1].Title, returnedBooksList[1].Title);
            Assert.Equal(fakeBooksList[1].Author, returnedBooksList[1].Author);
            Assert.Equal(fakeBooksList[1].Publisher, returnedBooksList[1].Publisher);
            Assert.Equal(fakeBooksList[1].PublicationYear, returnedBooksList[1].PublicationYear);
            Assert.Equal(fakeBooksList[1].PageCount, returnedBooksList[1].PageCount);
            Assert.Equal(fakeBooksList[1].Genre, returnedBooksList[1].Genre);
        }

        // Add

        [Fact]
        public async Task Add_WhenCalledWithValidDto_ReturnsCreatedBookDto()
        {
            // Arrange
            var newBookId = Guid.NewGuid();

            var inputDto = new BookDto
            {
                Title = "Test Book",
                Author = "Test Author",
                Publisher = "Test Publisher",
                PublicationYear = 2012,
                PageCount = 100,
                Genre = Genre.Romance
            };

            var createdDbEntity = new Book
            {
                Id = newBookId,
                Title = "Test Book",
                Author = "Test Author",
                Publisher = "Test Publisher",
                PublicationYear = 2012,
                PageCount = 100,
                Genre = Genre.Romance
            };

            _mockBookRepo.Setup(repo => repo.AddAsync(It.IsAny<Book>()))
                         .ReturnsAsync(createdDbEntity);

            // Act
            var result = await _bookService.Add(inputDto);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<BookDto>(result);

            Assert.Equal(newBookId, result.Id);
            Assert.Equal(inputDto.Title, result.Title);
            Assert.Equal(inputDto.Author, result.Author);
            Assert.Equal(inputDto.Publisher, result.Publisher);
            Assert.Equal(inputDto.PublicationYear, result.PublicationYear);
            Assert.Equal(inputDto.PageCount, result.PageCount);
            Assert.Equal(inputDto.Genre, result.Genre);
        }

        // Update 

        [Fact]
        public async Task Update_WhenCalledWithValidDto_ReturnsUpdatedBookDto()
        {
            // Arrange
            var existingBookId = Guid.NewGuid();

            var inputDto = new BookDto
            {
                Id = existingBookId,
                Title = "Test Book",
                Author = "Test Author",
                Publisher = "Test Publisher",
                PublicationYear = 2012,
                PageCount = 100,
                Genre = Genre.Romance
            };

            var updatedDbEntity = new Book
            {
                Id = existingBookId,
                Title = "Test Book",
                Author = "Test Author",
                Publisher = "Test Publisher",
                PublicationYear = 2012,
                PageCount = 100,
                Genre = Genre.Romance
            };

            _mockBookRepo.Setup(repo => repo.UpdateAsync(It.IsAny<Book>()))
                         .ReturnsAsync(updatedDbEntity);

            // Act
            var result = await _bookService.Update(inputDto);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<BookDto>(result);

            Assert.Equal(existingBookId, result.Id);
            Assert.Equal(inputDto.Title, result.Title);
            Assert.Equal(inputDto.Author, result.Author);
            Assert.Equal(inputDto.Publisher, result.Publisher);
            Assert.Equal(inputDto.PublicationYear, result.PublicationYear);
            Assert.Equal(inputDto.PageCount, result.PageCount);
            Assert.Equal(inputDto.Genre, result.Genre);
        }

        // Delete

        [Fact]
        public async Task Remove_WhenSuccessful_ReturnsTrue()
        {
            // Arrange
            var id = Guid.NewGuid();

            _mockBookRepo.Setup(repo => repo.RemoveAsync(id))
                         .ReturnsAsync(true);

            // Act
            var result = await _bookService.Remove(id);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task Remove_WhenEntityDoesNotExist_ReturnsFalse()
        {
            // Arrange
            var id = Guid.NewGuid();

            _mockBookRepo.Setup(repo => repo.RemoveAsync(id))
                         .ReturnsAsync(false);

            // Act
            var result = await _bookService.Remove(id);

            // Assert
            Assert.False(result);
        }
    }
}