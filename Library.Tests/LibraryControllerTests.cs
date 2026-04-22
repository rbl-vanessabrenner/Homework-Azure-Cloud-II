using System;
using System.Collections.Generic;
using System.Text;
using Library.API.Controllers;
using Library.API.Models;
using Library.Common.DTOs;
using Library.Common.Enums;
using Library.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Library.Tests
{
    public class LibraryControllerTests
    {

        private readonly Mock<IBookServiceAsync> _mockBookService;

        private readonly LibraryController _controller;

        public LibraryControllerTests()
        {
            _mockBookService = new Mock<IBookServiceAsync>();

            _controller = new LibraryController(_mockBookService.Object);
        }

        // Get By Id

        [Fact]
        public async Task GetById_WhenIdIsEmpty_ReturnsBadRequest()
        {
            // Arrange
            var emptyId = Guid.Empty;

            // Act
            var result = await _controller.Get(emptyId);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Id cannot be empty.", badRequestResult.Value);
        }

        [Fact]
        public async Task GetById_WhenBookDoesNotExist_ReturnsNotFound()
        {
            // Arrange
            var validId = Guid.NewGuid();

            _mockBookService.Setup(service => service.GetById(validId))
                            .ReturnsAsync((BookDto)null);

            // Act
            var result = await _controller.Get(validId);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task GetById_WhenBookExists_ReturnsOkResultWithBookModel()
        {
            // Arrange
            var validId = Guid.NewGuid();
            var fakeBook = new BookDto
            {
                Id = validId,
                Title = "Test Book",
                Author = "Test Author",
                Publisher = "Test Publisher",
                PublicationYear = 2012,
                PageCount = 100,
                Genre = Genre.Romance
            };

            _mockBookService.Setup(service => service.GetById(validId))
                            .ReturnsAsync(fakeBook);

            // Act
            var result = await _controller.Get(validId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedModel = Assert.IsType<BookModel>(okResult.Value);
            Assert.Equal(fakeBook.Id, returnedModel.Id);
            Assert.Equal(fakeBook.Title, returnedModel.Title);
            Assert.Equal(fakeBook.Author, returnedModel.Author);
            Assert.Equal(fakeBook.Publisher, returnedModel.Publisher);
            Assert.Equal(fakeBook.PublicationYear, returnedModel.PublicationYear);
            Assert.Equal(fakeBook.PageCount, returnedModel.PageCount);
            Assert.Equal(fakeBook.Genre, returnedModel.Genre);
        }

        // Get all

        [Fact]
        public async Task GetAll_WhenNoBooksExist_ReturnsNoContent()
        {
            // Arrange
            var emptyList = new List<BookDto>();

            _mockBookService.Setup(service => service.Get(It.IsAny<int>(), It.IsAny<int>()))
                            .ReturnsAsync(emptyList);

            // Act
            var result = await _controller.Get();

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task GetAll_WhenBooksExist_ReturnsOkResultWithBooks()
        {
            // Arrange
            var fakeBooksList = new List<BookDto>
            {
                new BookDto {
                    Id = Guid.NewGuid(),
                    Title = "Test Book 1",
                    Author = "Test Author 1",
                    Publisher = "Test Publisher 1",
                    PublicationYear = 2012,
                    PageCount = 100,
                    Genre = Genre.Romance
                },
                new BookDto {
                    Id = Guid.NewGuid(),
                    Title = "Test Book 2",
                    Author = "Test Author 2",
                    Publisher = "Test Publisher 2",
                    PublicationYear = 2012,
                    PageCount = 100,
                    Genre = Genre.Romance
                }
            };

            _mockBookService.Setup(service => service.Get(It.IsAny<int>(), It.IsAny<int>()))
                            .ReturnsAsync(fakeBooksList);

            // Act
            var result = await _controller.Get();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedBooks = Assert.IsAssignableFrom<IEnumerable<BookModel>>(okResult.Value);

            var returnedBooksList = returnedBooks.ToList();
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
        public async Task Add_WhenModelStateIsInvalid_ReturnsBadRequest()
        {
            // Arrange
            var invalidModel = new BookModel();

            _controller.ModelState.AddModelError("Title", "The Title field is required.");

            // Act
            var result = await _controller.Add(invalidModel);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.IsType<SerializableError>(badRequestResult.Value);
        }

        [Fact]
        public async Task Add_WhenModelIsValid_ReturnsOkResultWithCreatedBook()
        {
            // Arrange
            var newBookId = Guid.NewGuid();

            var inputModel = new BookModel
            {
                Title = "Test Book 1",
                Author = "Test Author 1",
                Publisher = "Test Publisher 1",
                PublicationYear = 2012,
                PageCount = 100,
                Genre = Genre.Romance
            };

            var returnedDto = new BookDto
            {
                Id = newBookId,
                Title = "Test Book 1",
                Author = "Test Author 1",
                Publisher = "Test Publisher 1",
                PublicationYear = 2012,
                PageCount = 100,
                Genre = Genre.Romance
            };

            _mockBookService.Setup(service => service.Add(It.IsAny<BookDto>()))
                            .ReturnsAsync(returnedDto);

            // Act            
            _controller.ModelState.Clear();
            var result = await _controller.Add(inputModel);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedModel = Assert.IsType<BookModel>(okResult.Value);

            Assert.Equal(newBookId, returnedModel.Id);
            Assert.Equal(inputModel.Title, returnedModel.Title);
            Assert.Equal(inputModel.Author, returnedModel.Author);
            Assert.Equal(inputModel.Publisher, returnedModel.Publisher);
            Assert.Equal(inputModel.PublicationYear, returnedModel.PublicationYear);
            Assert.Equal(inputModel.PageCount, returnedModel.PageCount);
            Assert.Equal(inputModel.Genre, returnedModel.Genre);
        }

        // Update

        [Fact]
        public async Task Update_WhenRouteIdIsEmpty_ReturnsBadRequest()
        {
            // Arrange
            var emptyId = Guid.Empty;
            var model = new BookModel();

            // Act
            var result = await _controller.Update(emptyId, model);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Id cannot be empty.", badRequestResult.Value);
        }

        [Fact]
        public async Task Update_WhenIdsDoNotMatch_ReturnsBadRequest()
        {
            // Arrange
            var routeId = Guid.NewGuid();
            var model = new BookModel { Id = Guid.NewGuid() };

            // Act
            var result = await _controller.Update(routeId, model);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Ids do not match.", badRequestResult.Value);
        }

        [Fact]
        public async Task Update_WhenModelStateIsInvalid_ReturnsBadRequest()
        {
            // Arrange
            var id = Guid.NewGuid();
            var invalidModel = new BookModel { Id = id };

            _controller.ModelState.AddModelError("Title", "The Title field is required.");

            // Act
            var result = await _controller.Update(id, invalidModel);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.IsType<SerializableError>(badRequestResult.Value);
        }

        [Fact]
        public async Task Update_WhenBookNotFoundInDb_ReturnsNotFound()
        {
            // Arrange
            var id = Guid.NewGuid();
            var validModel = new BookModel { Id = id, Title = "Updated Title" };

            _controller.ModelState.Clear();

            _mockBookService.Setup(service => service.Update(It.IsAny<BookDto>()))
                            .ReturnsAsync((BookDto)null);

            // Act
            var result = await _controller.Update(id, validModel);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Update_WhenSuccessful_ReturnsOkResultWithUpdatedBook()
        {
            // Arrange
            var id = Guid.NewGuid();
            var inputModel = new BookModel
            {
                Id = id,
                Title = "Test Book 1",
                Author = "Test Author 1",
                Publisher = "Test Publisher 1",
                PublicationYear = 2012,
                PageCount = 100,
                Genre = Genre.Romance
            };

            var returnedDto = new BookDto
            {
                Id = id,
                Title = "Test Book 1",
                Author = "Test Author 1",
                Publisher = "Test Publisher 1",
                PublicationYear = 2012,
                PageCount = 100,
                Genre = Genre.Romance
            };

            _controller.ModelState.Clear();

            _mockBookService.Setup(service => service.Update(It.IsAny<BookDto>()))
                            .ReturnsAsync(returnedDto);

            // Act
            var result = await _controller.Update(id, inputModel);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedModel = Assert.IsType<BookModel>(okResult.Value);

            Assert.Equal(id, returnedModel.Id);
            Assert.Equal(inputModel.Title, returnedModel.Title);
            Assert.Equal(inputModel.Author, returnedModel.Author);
            Assert.Equal(inputModel.Publisher, returnedModel.Publisher);
            Assert.Equal(inputModel.PublicationYear, returnedModel.PublicationYear);
            Assert.Equal(inputModel.PageCount, returnedModel.PageCount);
            Assert.Equal(inputModel.Genre, returnedModel.Genre);
        }

        // Delete

        [Fact]
        public async Task Delete_WhenIdIsEmpty_ReturnsBadRequest()
        {
            // Arrange
            var emptyId = Guid.Empty;

            // Act
            var result = await _controller.Delete(emptyId);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Id cannot be empty.", badRequestResult.Value);
        }


        [Fact]
        public async Task Delete_WhenBookDoesNotExist_ReturnsNotFound()
        {
            // Arrange
            var id = Guid.NewGuid();

            _mockBookService.Setup(service => service.Remove(id))
                            .ReturnsAsync(false);

            // Act
            var result = await _controller.Delete(id);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("Entity not found.", notFoundResult.Value);
        }

        [Fact]
        public async Task Delete_WhenSuccessful_ReturnsOkResultWithMessage()
        {
            // Arrange
            var id = Guid.NewGuid();

            _mockBookService.Setup(service => service.Remove(id))
                            .ReturnsAsync(true);

            // Act
            var result = await _controller.Delete(id);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("Entity deleted succesfully.", okResult.Value);
        }
    }
}
