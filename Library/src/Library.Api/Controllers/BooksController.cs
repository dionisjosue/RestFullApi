using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Library.API.Services;
using AutoMapper;
using Library.API.model;
using Library.API.Entities;
using Library.API.Helpers;
using Microsoft.AspNetCore.JsonPatch;

namespace Library.API.Controllers
{
    [Route("api/Author/{AuthorId}/Books")]
    public class BooksController : Controller
    {
        private readonly ILibraryRepository _Repo;

        public BooksController(ILibraryRepository _repo)
        {
            _Repo = _repo;
        }

        [HttpGet]
        public IActionResult Books(Guid AuthorId, AuthorParams BooksParams)
        {
            if (!_Repo.AuthorExists(AuthorId))
            {
                return NotFound();
            }

            var Book = _Repo.GetBooksForAuthor(AuthorId, BooksParams);
            var BookDt = Mapper.Map<IEnumerable<BookToReturn>>(Book);

            return Ok(BookDt);

        }
        [HttpGet("{BookId}", Name = "Book")]
        public IActionResult Book(Guid AuthorId, Guid BookId)
        {
            if (!_Repo.AuthorExists(AuthorId))
            {
                return NotFound();
            }
            var Book = _Repo.GetBookForAuthor(authorId: AuthorId, bookId: BookId);
            var BookDt = Mapper.Map<BookToReturn>(Book);

            if (BookDt == null)
            {
                return NotFound();
            }

            return Ok(BookDt);

        }
        [HttpPost]
        public IActionResult CreateBook(Guid AuthorId,
            [FromBody] BookCreationDto book)
        {
            if (!_Repo.AuthorExists(AuthorId))
            {
                return NotFound();
            }

            if (book == null)
            {
                return BadRequest();
            }
            if(book.Title == book.Description)
            {
                ModelState.AddModelError(nameof(BookCreationDto),
                 "the title and desc cannot be the same");
            }
            if(!ModelState.IsValid)
            {
                return new UnProccessableObjectResult(ModelState);
            }

            var booktoSave = Mapper.Map<Book>(book);

            _Repo.AddBookForAuthor(AuthorId, booktoSave);
            if (!_Repo.Save())
            {
                throw new Exception("something go wron to the server");
            }
            var bookToResponse = Mapper.Map<BookToReturn>(booktoSave);

            return CreatedAtRoute("Book", new
            {
                authorId = bookToResponse.AuthorId,
                bookId = bookToResponse.Id
            }, bookToResponse);

        }
        /*  [HttpDelete("{BookId}")]
          public IActionResult DeleteBook(Guid AuthorId, Guid BookId)
          {
              if (!_Repo.AuthorExists(AuthorId))
              {
                  return NotFound();
              }
              var booktodelete = _Repo.GetBookForAuthor(AuthorId, BookId);
              if(booktodelete == null)
              {
                  return NotFound();
              }
              _Repo.DeleteBook(booktodelete);
              if (!_Repo.Save())
              {
                  throw new Exception($"failed to delete {BookId} this book");
              }
              return NoContent();
          }*/

        [HttpDelete("{BookId}")]
        public IActionResult DeleteCollectionBooks(Guid AuthorId, [ModelBinder(BinderType = typeof(ArrayModelBinder))]
           IEnumerable<Guid> BookId)
        {
            if (!_Repo.AuthorExists(AuthorId))
            {
                return NotFound();
            }
            if (BookId == null)
            {
                return NotFound();
            }

            var bookCollect = _Repo.GetBooksCollectAuthor(BookId);
            if (bookCollect.Count() != BookId.Count())
            {
                return NotFound();
            }
            _Repo.DeleteBooks(bookCollect);

            if (!_Repo.Save())
            {
                throw new Exception($"failed to delete {BookId} this book");
            }
            return NoContent();
        }

        [HttpPut("{BookId}")]
        public IActionResult Updatebook(Guid AuthorId, Guid BookId, [FromBody] BookUpdate BookUp)
        {

            if (!_Repo.AuthorExists(AuthorId))
            {
                return NotFound();
            }
            var booktoUpdate = _Repo.GetBookForAuthor(AuthorId, BookId);

            if(booktoUpdate == null)
            {
                var BookToAdd = Mapper.Map<BookUpdate, Book>(BookUp);
                _Repo.AddBookForAuthor(AuthorId,BookToAdd);
                BookToAdd.Id = BookId;
                if(BookToAdd.Title == BookToAdd.Description)
                {
                    ModelState.AddModelError(nameof(BookUpdate),
                      "the title and desc cannot be the same");
                }
                if (!ModelState.IsValid)
                {
                    return new UnProccessableObjectResult(ModelState);
                }
                if (!_Repo.Save())
                {
                    throw new Exception($"failed to upserting the book {BookId}");
                }
                var Bktr = Mapper.Map<BookToReturn>(BookToAdd);
                return CreatedAtRoute("Book", new { BookId = BookToAdd.Id, AuthorId = BookToAdd.AuthorId }, Bktr);
            }
            Mapper.Map(BookUp, booktoUpdate);
            if(BookUp.Title == BookUp.Description)
            {
                ModelState.AddModelError(nameof(BookUpdate),
                       "the title and desc cannot be the same");
            }
            if (!ModelState.IsValid)
            {
                return new UnProccessableObjectResult(ModelState);
            }
            if (!_Repo.Save())
            {
                throw new Exception($"Faile to Update {BookUp}");
            }
            return NoContent();
        }
        [HttpPatch("{BookId}")]
        public IActionResult ParcialUpdateBook(Guid AuthorId, Guid BookId,
            [FromBody] JsonPatchDocument<BookUpdate> PatchBook)
        {
            if (PatchBook == null || !_Repo.AuthorExists(AuthorId))
            {
                return NotFound();
            }
            var Bookcomplet = _Repo.GetBookForAuthor(AuthorId, BookId);
            if(Bookcomplet == null)
            {
                BookUpdate Bku = new BookUpdate();
                PatchBook.ApplyTo(Bku, ModelState);
                TryValidateModel(Bku);
                if(Bku.Title == Bku.Description)
                {
                    ModelState.AddModelError(nameof(BookUpdate),
                        "the title and desc cannot be the same");
                }
                if (!ModelState.IsValid)
                {
                    return new UnProccessableObjectResult(ModelState);
                }
                var BkToAdd = Mapper.Map<BookUpdate, Book>(Bku);
                _Repo.AddBookForAuthor(AuthorId,BkToAdd);
                BkToAdd.Id = BookId;
                if (!_Repo.Save())
                {
                    throw new Exception($"failed to upserting the book {BookId}");
                }
                BookToReturn Btr = Mapper.Map<Book, BookToReturn>(BkToAdd);
                return CreatedAtRoute("book", new { BookId = BkToAdd.Id, AuthorId = BkToAdd.AuthorId},Btr);
            }
            var BookToPatch = Mapper.Map<Book, BookUpdate>(Bookcomplet);

            PatchBook.ApplyTo(BookToPatch, ModelState);
            TryValidateModel(ModelState);
            if(BookToPatch.Title == BookToPatch.Description)
            {
                ModelState.AddModelError(nameof(BookUpdate),
                    "the title and desc cannot be the same");
            }
            if (!ModelState.IsValid)
            {
                return new UnProccessableObjectResult(ModelState);
            }

            var x = Mapper.Map(BookToPatch,Bookcomplet);

            if (!_Repo.Save())
            {
                throw new Exception($"failed to update this book {BookId}");
            }
            return NoContent();

            

        }
    }
}