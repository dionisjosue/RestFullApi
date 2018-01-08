using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Library.API.Helpers
{
    public class UnProccessableObjectResult : ObjectResult
    {
        public UnProccessableObjectResult(ModelStateDictionary value) :
            base(new SerializableError(value))
        {
            if(value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }
            StatusCode = 422;
        }
    }
}
