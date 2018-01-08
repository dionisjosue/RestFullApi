using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Library.API.Services;
using Library.API.model;
using AutoMapper;
using Library.API.Entities;
using Library.API.Helpers;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Library.API.Controllers
{
    [Route("api/[controller]")]
    public class AuthorCollectionController : Controller
    {
        private readonly ILibraryRepository repo;

        // GET: api/values
        public AuthorCollectionController(ILibraryRepository Repo)
        {
            repo = Repo;
        }

        [HttpPost]
        public IActionResult CreateCollectionAuthors([FromBody]List<AuthorDtoCreating> Authors)
        {
            if(Authors == null)
            {
                return BadRequest();
            }
            var AuthorToSave = Mapper.Map<IEnumerable<Author>>(Authors);
           
            foreach(var author in AuthorToSave)
            {
                repo.AddAuthor(author);
               
            }
            if (!repo.Save())
            {
                return StatusCode(500, "there was a problem with the server");
            }
            var idsList = string.Join(",",AuthorToSave.Select(x => x.Id));
            var autor = Mapper.Map<IEnumerable<AuthorToReturn>>(AuthorToSave);
               
            return CreatedAtRoute("GetCollectionAuthors",new { ids = idsList}, autor);


        }
        
         
        [HttpGet("{ids}", Name ="GetCollectionAuthors")]
        public IActionResult GetCollectionAuthors([ModelBinder(BinderType = typeof(ArrayModelBinder))]
        IEnumerable<Guid> ids)
        {
            if(ids == null)
            {
                return BadRequest();
            }

            var authors = repo.GetAuthors(ids);
            if(authors.Count() != ids.Count())
            {
                return NotFound();
            }
            var autorview= Mapper.Map<IEnumerable<AuthorToReturn>>(authors);
           
            return Ok(autorview);
        }

        [HttpDelete("{ids}")]
        public IActionResult DeleteCollectionAuthors([ModelBinder (BinderType= typeof(ArrayModelBinder))]
        IEnumerable<Guid> ids)
        {
            if(ids == null)
            {
                return BadRequest();
            }
            var author = repo.GetAuthors(ids);
            if(author.Count() != ids.Count())
            {
                return NotFound();
            }
            repo.DeleteAuthors(author);
            if (!repo.Save()){

                throw new Exception($"failed deleting authors {author.Select(a => a.Id)}");
            }
            return NoContent();
        }

    }
}
