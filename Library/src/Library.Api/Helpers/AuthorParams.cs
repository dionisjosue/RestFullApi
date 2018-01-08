using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Library.API.Helpers
{
    public class AuthorParams
    {
        private const int MaximunSize = 20;
        public int NumberPages { get; set; } = 1; 
        private int _pageSize = 10;
        public int PageSize 
        {
            get
            {
                return _pageSize;
            }

            set
            {
                _pageSize = (value > MaximunSize) ? MaximunSize : value;
            }
        }
        public bool IncludeBooks { get; set; } = false;

    }
}
