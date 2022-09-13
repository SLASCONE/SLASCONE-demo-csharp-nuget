using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Slascone.Client;

namespace Slascone.Provisioning.Sample.NuGet
{
    public class ApiResponse<T> where T : class
    {
        public T Result { get; set; }
        public IEnumerable<ErrorResultObjects> Errors;
        public string StatusCode { get; set; }
        public string Message { get; set; }
    }
}
