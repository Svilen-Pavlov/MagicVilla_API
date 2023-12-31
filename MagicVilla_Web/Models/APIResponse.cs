﻿using System.Net;
using System.Transactions;

namespace MagicVilla_Web.Models
{
    public class APIResponse
    {
        public HttpStatusCode StatusCode { get; set; }

        public bool IsSuccess { get; set; } = true;

        public List<string> ErrorMessages { get; set; }

        public object Result { get; set; }

        public List<KeyValuePair<string, List<string>>> Headers { get; set; }
    }
}
