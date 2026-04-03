using NUnit.Framework;
using LibraryManagerTests.Helpers;
using LibraryManagerTests.Models;

namespace LibraryManagerTests.TestSetup
{
  
    [TestFixture]
    public abstract class TestBase
    {
        protected const string BaseUrl = "http://localhost:9000";

        protected ApiClient Client = null!;

        protected List<int> CreatedBookIds = null!;

        private static int _idCounter = 1000;

        protected static int NextId() => Interlocked.Increment(ref _idCounter);

        [SetUp]
        public void BaseSetUp()
        {
            Client = new ApiClient(BaseUrl);
            CreatedBookIds = new List<int>();
        }

        [TearDown]
        public async Task BaseTearDown()
        {
            foreach (var id in CreatedBookIds)
            {
                try
                {
                    await Client.DeleteBookAsync(id);
                }
                catch
                {
                    // Swallow
                }
            }

            Client.Dispose();
        }

        protected async Task<System.Net.Http.HttpResponseMessage> CreateAndTrackBook(Book book)
        {
            CreatedBookIds.Add(book.Id);
            return await Client.CreateBookAsync(book);
        }

        protected async Task CleanAllBooks()
        {
            var response = await Client.GetBooksAsync();
            var books = await ApiClient.DeserializeBookList(response);
            foreach (var book in books)
            {
                try
                {
                    await Client.DeleteBookAsync(book.Id);
                }
                catch
                {
                    // Swallow
                }
            }
        }

        protected static Book BuildDefaultBook(int? id = null)
        {
            var bookId = id ?? NextId();
            return new Book
            {
                Id = bookId,
                Title = $"TestTitle{bookId}",
                Author = $"TestAuthor{bookId}",
                Description = $"TestDesc{bookId}"
            };
        }
    }
}
