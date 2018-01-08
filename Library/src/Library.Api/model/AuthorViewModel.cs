using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Library.API.model
{
    public class AuthorToReturn
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Genre { get; set; }
        public int Age { get; set; }
        public IEnumerable<BookToReturn> Books { get; set; }
       = new List<BookToReturn>();
    }
}
