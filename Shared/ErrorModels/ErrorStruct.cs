using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Shared.ErrorModels
{
    public class ErrorStruct
    {
        public int StatusCode { get; set; }
        public string Message { get; set; }
        public List<string>? Errors { get; set; }
    }
}
