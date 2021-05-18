using System.Collections.Generic;

namespace UKParliament.CodeTest.Services
{
    public class SearchResponse<T>
    {
        public List<T> Response { get; set; }
        public string ErrorMessage { get; set; }
    }
}
