using Library.API.Entities;
using Library.API.Helpers;
using System;
using System.Collections.Generic;

namespace Library.API.Services
{
    public interface ILibraryRepository
    {
        IEnumerable<Author> GetAuthors(AuthorParams pagination);
        Author GetAuthor(Guid authorId, bool includeitems);
        IEnumerable<Author> GetAuthors(IEnumerable<Guid> authorIds);
        void AddAuthor(Author author);
        void DeleteAuthor(Author author);
        void UpdateAuthor(Author author);
        bool AuthorExists(Guid authorId);
        IEnumerable<Book> GetBooksForAuthor(Guid authorId, AuthorParams BookParams);
        Book GetBookForAuthor(Guid authorId, Guid bookId);
        void AddBookForAuthor(Guid authorId, Book book);
        void UpdateBookForAuthor(Book book);
        void DeleteBook(Book book);
        void DeleteAuthors(IEnumerable<Author> authors);
        IEnumerable<Book> GetBooksCollectAuthor(IEnumerable<Guid> BookIds);
        void DeleteBooks(IEnumerable<Book> book);

        bool Save();
    }
}
