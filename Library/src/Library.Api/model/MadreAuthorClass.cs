using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Library.API.model
{
    public abstract class MadreAuthorClass
    {
        [Required(ErrorMessage = "The firstName cannot be empty")]
        [MaxLength(100)]
        public string FirstName { get; set; }

        [MaxLength(100)]
        public string LastName { get; set; }
        [Required(ErrorMessage = "you have to put your dateofbirth")]
        public DateTimeOffset DateOfBirth { get; set; }
        [Required]
        [MaxLength(50)]
        public string Genre { get; set; }

        public IEnumerable<BookCreationDto> Books { get; set; }
        = new List<BookCreationDto>();
    }
}
