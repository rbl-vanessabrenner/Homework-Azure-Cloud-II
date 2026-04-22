using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Text;
using Library.API.Models;
using Library.Common.DTOs;
using Library.Common.Entities;

namespace Library.API.Utils
{
    public static class MapperUtil
    {
        public static BookModel ToModel(this BookDto dto)
        {
            return new BookModel
            {
                Id = dto.Id ?? Guid.Empty,
                Title = dto.Title,
                Author = dto.Author,
                Publisher = dto.Publisher,
                PublicationYear = dto.PublicationYear,
                PageCount = dto.PageCount,
                Genre = dto.Genre               
            };
        }

        public static BookDto ToDto(this BookModel model, Guid? id, Guid addedByUserId = default)
        {
            return new BookDto
            {
                Id = id,
                Title = model.Title,
                Author = model.Author,
                Publisher = model.Publisher,
                PublicationYear = model.PublicationYear,
                PageCount = model.PageCount,
                Genre = model.Genre,
                AddedByUserId = addedByUserId
            };
        }

        // The Service layer should ideally
        // return a UserDto to separate the Domain layer from the API layer. 
        // For the scope and simplicity of this demo, mapping directly 
        // from the User entity to the API Model is sufficient.
        public static UserModel ToModel(this User user)
        {
            return new UserModel
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Username = user.Username,
                UserRole = user.UserRole,
                Email = user.Email
            };
        }
    }
}
