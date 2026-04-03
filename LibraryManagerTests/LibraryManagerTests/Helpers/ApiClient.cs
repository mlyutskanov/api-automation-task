using System.Net;
using System.Text;
using Newtonsoft.Json;
using LibraryManagerTests.Models;

namespace LibraryManagerTests.Helpers
{
    /// <summary>
    /// HTTP client wrapper for the Library Manager API.
    /// Each method returns the raw HttpResponseMessage so tests can assert on
    /// status codes, headers, and deserialized bodies independently.
    /// </summary>
    public class ApiClient : IDisposable
    {
        private readonly HttpClient _client;

        public ApiClient(string baseUrl)
        {
            _client = new HttpClient { BaseAddress = new Uri(baseUrl) };
            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Accept.Add(
                new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
        }

        /// <summary>
        /// GET /api/books  or  GET /api/books?title={title}
        /// </summary>
        public async Task<HttpResponseMessage> GetBooksAsync(string? title = null)
        {
            var url = "/api/books";
            if (!string.IsNullOrEmpty(title))
                url += $"?title={Uri.EscapeDataString(title)}";

            return await _client.GetAsync(url);
        }

        /// <summary>
        /// GET /api/books/{id}
        /// </summary>
        public async Task<HttpResponseMessage> GetBookByIdAsync(int id)
        {
            return await _client.GetAsync($"/api/books/{id}");
        }

        /// <summary>
        /// POST /api/books with a Book JSON body.
        /// </summary>
        public async Task<HttpResponseMessage> CreateBookAsync(Book book)
        {
            var json = JsonConvert.SerializeObject(book);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            return await _client.PostAsync("/api/books", content);
        }

        /// <summary>
        /// POST /api/books with a raw JSON string body.
        /// Useful for sending malformed or edge-case payloads.
        /// </summary>
        public async Task<HttpResponseMessage> CreateBookRawAsync(string jsonBody)
        {
            var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
            return await _client.PostAsync("/api/books", content);
        }

        /// <summary>
        /// PUT /api/books/{id} with a Book JSON body.
        /// </summary>
        public async Task<HttpResponseMessage> UpdateBookAsync(int id, Book book)
        {
            var json = JsonConvert.SerializeObject(book);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            return await _client.PutAsync($"/api/books/{id}", content);
        }

        /// <summary>
        /// PUT /api/books/{id} with a raw JSON string body.
        /// </summary>
        public async Task<HttpResponseMessage> UpdateBookRawAsync(int id, string jsonBody)
        {
            var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
            return await _client.PutAsync($"/api/books/{id}", content);
        }

        /// <summary>
        /// DELETE /api/books/{id}
        /// </summary>
        public async Task<HttpResponseMessage> DeleteBookAsync(int id)
        {
            return await _client.DeleteAsync($"/api/books/{id}");
        }

        // ----- Convenience deserialization helpers -----

        public static async Task<T?> DeserializeResponse<T>(HttpResponseMessage response)
        {
            var body = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<T>(body);
        }

        public static async Task<List<Book>> DeserializeBookList(HttpResponseMessage response)
        {
            var body = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<Book>>(body) ?? new List<Book>();
        }

        public void Dispose()
        {
            _client.Dispose();
        }
    }
}
