using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Library.API.model
{
    public abstract class MadreBookClass
    {
        [Required]
        public string Title { get; set; }
        [MaxLength(500)]
        public virtual string Description { get; set; }

        public ModelStateDictionary validTitle(string clase, string title, string desc, ModelStateDictionary Model)
        {
            if (title == desc)
            {
                Model.AddModelError(clase,
                    "the title and desc cannot be the same");
                return Model;
            }
            return Model;
        }
    }
}
