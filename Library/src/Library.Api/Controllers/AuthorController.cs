using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Library.API.Services;
using AutoMapper;
using Library.API.model;
using Library.API.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.Extensions.Logging;
using Library.API.Helpers;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Library.API.Controllers
{
    [Route("api/[controller]")]
    public class AuthorController : Controller
    {
        private ILibraryRepository Repo;
        private readonly ILogger<AuthorController> log;

        //private IMapper _Map;
        public AuthorController(ILibraryRepository _repo, ILogger<AuthorController> Log)
        {
            Repo = _repo;
            log = Log;
            //_Map = Map;
        }

        // GET: api/values
        [HttpGet]
        public IActionResult Authors(AuthorParams AuthParams)
        {
            //throw new Exception("random test exception ");
            var authors = Repo.GetAuthors(AuthParams);
            var Autho = Mapper.Map<IEnumerable<Author>, IEnumerable<AuthorToReturn>>(authors);
            return Ok(Autho);

        }

        // GET api/values/5
        [HttpGet("{id}", Name = "Author")]
        public IActionResult Author(Guid id, bool includeitems)
        {
            var authors = Repo.GetAuthor(id, includeitems);
            if (authors != null)
            {
                var Autho = Mapper.Map<Author, AuthorToReturn>(authors);
                return Ok(Autho);
            }
            return NotFound("The author do not exist");
            // return CreatedAtRoute("Author", new { id = Author.id})

        }

        // POST api/values
        [HttpPost]
        public IActionResult CreateAuthor([FromBody] AuthorDtoCreating Autor)
        {
            if (Autor == null)
            {
                return BadRequest();
            }
            if (Autor.FirstName == Autor.LastName)
            {
                ModelState.AddModelError(nameof(AuthorDtoCreating),
                    "the title and description cannot be the same");
            }
            if (!ModelState.IsValid)
            {

                return new UnProccessableObjectResult(ModelState);
            }
            Author _Autor = Mapper.Map<Author>(Autor);
            Repo.AddAuthor(_Autor);


            if (!Repo.Save())
            {
                return StatusCode(500, "There was a problem in the server");
            }
            var _AuthorToResponse = Mapper.Map<AuthorToReturn>(_Autor);

            //hace una especie de request al metodo Author de arriba por eso devuelve el objeto con la edad y no 
            //el dateofbirth 
            return CreatedAtRoute("Author", new { id = _AuthorToResponse.Id }, _AuthorToResponse);

        }


        // DELETE api/values/5
        [HttpDelete("{id}")]
        public IActionResult DeleteAuthor(Guid id)
        {
            var auth = Repo.GetAuthor(id, true);
            if (auth == null)
            {
                return NotFound();
            }
            Repo.DeleteAuthor(auth);
            if (!Repo.Save())
            {
                throw new Exception($"failed to delete {id} this author");
            }
            return NoContent();
        }
        [HttpPost("{id}")]
        public IActionResult CheckIfAuthorExist(Guid id)
        {
            if (Repo.AuthorExists(id))
            {
                return new StatusCodeResult(StatusCodes.Status409Conflict);
            }
            return NotFound();
        }
        [HttpPut("{id}")]
        public IActionResult UpdateAuthor(Guid Id, [FromBody] AuthorUpdate authUpda)
        {
            var auth = Repo.GetAuthor(Id, true);
            //upserting with put
            if (auth == null)
            {
                try
                {
                    if (authUpda.FirstName.ToLower() == authUpda.LastName.ToLower())
                    {
                        ModelState.AddModelError(nameof(AuthorUpdate),
                          "the firstname and lastname cannot be the same");
                    }
                }
                catch (Exception)
                {
                    if(authUpda.FirstName == authUpda.LastName)
                    {
                        ModelState.AddModelError(nameof(AuthorUpdate),
                         "the firstname and lastname cannot be the same");
                    }
                }


                if (!ModelState.IsValid)
                {
                    return new UnProccessableObjectResult(ModelState);
                }

                var autor = Mapper.Map<AuthorUpdate, Author>(authUpda);
                Repo.AddAuthor(autor);
                autor.Id = Id;

                if (!Repo.Save())
                {
                    throw new Exception($"failed to create this autor with the {Id}");
                }
                var authorcreated = Mapper.Map<Author, AuthorToReturn>(autor);
                return CreatedAtRoute("Author", new { id = autor.Id }, autor);
            }

            if (authUpda.FirstName == auth.LastName)
            {
                ModelState.AddModelError(nameof(AuthorUpdate),
                  "the firstname and lastname cannot be the same");
            }
            if (authUpda.Genre != auth.Genre)
            {
                ModelState.AddModelError(nameof(AuthorUpdate),
                    "the genre cannot be change");
            }
            if (!ModelState.IsValid)
            {
                return new UnProccessableObjectResult(ModelState);
            }
            Mapper.Map(authUpda, auth);

            if (!Repo.Save())
            {
                throw new Exception($"Failed to Update autor with the id: {Id}");
            }
            return NoContent();
        }
        [HttpPatch("{id}")]
        public IActionResult ParcialUpdateAuthor(Guid id,
            [FromBody] JsonPatchDocument<AuthorUpdate> auth)
        {
            if (auth == null)
            {
                return BadRequest();
            }
            var authore = Repo.GetAuthor(id, true);
            //upserting with patch
            if (authore == null)
            {
                
                var author = new AuthorUpdate();
                auth.ApplyTo(author, ModelState);
                if(author.FirstName == author.LastName)
                {
                    ModelState.AddModelError(nameof(AuthorUpdate),
                        "the name and lastname cannot be the same");
                }
                TryValidateModel(author);
                if (!ModelState.IsValid)
                {
                    return new UnProccessableObjectResult(ModelState);
                }
                var authToAdd = Mapper.Map<AuthorUpdate, Author>(author);
                Repo.AddAuthor(authToAdd);
                authToAdd.Id = id;

                if (!Repo.Save())
                {
                    throw new Exception($"failed to upserting the author {id}");
                }
                var Authtr = Mapper.Map<AuthorToReturn>(authToAdd);
                return CreatedAtRoute("Author", new { id = Authtr.Id }, Authtr);
            }
            var AuthToPatch = Mapper.Map<Author, AuthorUpdate>(authore);

            auth.ApplyTo(AuthToPatch,ModelState);
            TryValidateModel(AuthToPatch);
            if(AuthToPatch.FirstName == AuthToPatch.LastName)
            {
                ModelState.AddModelError(nameof(AuthorUpdate),
                    "the firstname and lastname cannot be the same");
            }
            if (!ModelState.IsValid)
            {
                return new UnProccessableObjectResult(ModelState);
            }

            Mapper.Map(AuthToPatch, authore);

            if (!Repo.Save())
            {
                throw new Exception($"Failed to edit this autor {id}");
            }
            log.LogInformation(204, $"the author with the {authore.Id} has been patch");

            return NoContent();


        }
    }
}
