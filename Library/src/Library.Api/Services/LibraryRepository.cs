using Library.API.Entities;
using Library.API.Helpers;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

namespace Library.API.Services
{
    public class LibraryRepository : ILibraryRepository
    {
        private LibraryContext _context;

        public LibraryRepository(LibraryContext context)
        {
            _context = context;
        }

        public void AddAuthor(Author author)
        {
            author.Id = Guid.NewGuid();
            _context.Authors.Add(author);

            // the repository fills the id (instead of using identity columns)
            if (author.Books.Any())
            {
                foreach (var book in author.Books)
                {
                    book.Id = Guid.NewGuid();
                }
            }
        }

        public void AddBookForAuthor(Guid authorId, Book book)
        {
            var author = GetAuthor(authorId, true);
            if (author != null)
            {
                // if there isn't an id filled out (ie: we're not upserting),
                // we should generate one
                if (book.Id == Guid.Empty)
                {
                    book.Id = Guid.NewGuid();
                }
                author.Books.Add(book);
            }
        }

        public bool AuthorExists(Guid authorId)
        {
            return _context.Authors.Any(a => a.Id == authorId);
        }

        public void DeleteAuthor(Author author)
        {
            _context.Authors.Remove(author);
        }

        public void DeleteBook(Book book)
        {
            _context.Books.Remove(book);
        }

        public Author GetAuthor(Guid authorId, bool includebooks)
        {
            if (!includebooks)
            {
                return _context.Authors.FirstOrDefault(a => a.Id == authorId);
            }
            return _context.Authors.
                Where(A => A.Id == authorId).Include(b => b.Books).FirstOrDefault();

        }

        public IEnumerable<Author> GetAuthors(AuthorParams pagination)
        {
            try
            {
                if (!pagination.IncludeBooks)
                {
                    return _context.Authors.
                        OrderBy(a => a.FirstName).
                        ThenBy(a => a.LastName)
                        .Skip(pagination.PageSize *
                        (pagination.NumberPages - 1)) //the amount of author that going to be skipped example: if the page 
                        .Take(pagination.PageSize)//number is 3 and page size 10 will be skipped the 20 author of the page 1 and 2
                        .ToList();
                }
                return _context.Authors.
                        OrderBy(a => a.FirstName).
                        ThenBy(a => a.LastName)
                        .Skip(pagination.PageSize *
                        (pagination.NumberPages - 1))
                        .Include(b => b.Books)
                        .ToList();//the amount of author that going to be skipped example: if the page 
                   
            }
            catch (Exception)
            {

                throw new Exception($"failed getting the author of {pagination.NumberPages}" +
                    $"maybe there are no more author to retrieve");
            }



        }

        public IEnumerable<Author> GetAuthors(IEnumerable<Guid> authorIds)
        {
            return _context.Authors.Where(a => authorIds.Contains(a.Id))
                .OrderBy(a => a.FirstName)
                .OrderBy(a => a.LastName)
                .ToList();
        }

        public void UpdateAuthor(Author author)
        {
            // no code in this implementation
        }

        public Book GetBookForAuthor(Guid authorId, Guid bookId)
        {
            return _context.Books
              .Where(b => b.AuthorId == authorId && b.Id == bookId).FirstOrDefault();
        }

        public IEnumerable<Book> GetBooksForAuthor(Guid authorId, AuthorParams BookParams)
        {
            return _context.Books
                        .Where(b => b.AuthorId == authorId).OrderBy(b => b.Title)
                        .Skip(BookParams.PageSize * (BookParams.NumberPages - 1))
                        .Take(BookParams.PageSize)
                        .ToList();
        }

        public void UpdateBookForAuthor(Book book)
        {
            // no code in this implementation
        }

        public bool Save()
        {
            try
            {
                return (_context.SaveChanges() >= 0);
            }
            catch (Exception)
            {
                return false;
            }
        }

        public void DeleteAuthors(IEnumerable<Author> authors)
        {
            _context.RemoveRange(authors);
        }

        public IEnumerable<Book> GetBooksCollectAuthor(IEnumerable<Guid> BookIds)
        {
            return _context.Books.Where(a => BookIds.Contains(a.Id))
                .OrderBy(a => a.Title).ToList();
        }

        public void DeleteBooks(IEnumerable<Book> book)
        {
            _context.Books.RemoveRange(book);
        }
    }
}
