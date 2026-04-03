using System.Net;
using NUnit.Framework;
using LibraryManagerTests.Helpers;
using LibraryManagerTests.Models;
using LibraryManagerTests.TestSetup;

namespace LibraryManagerTests.Tests
{
    [TestFixture]
    public class AddBookTests : TestBase
    {
        [Test]
        public async Task AddBook_WithAllFields_ReturnsCreatedBookWithCorrectData()
        {
            // Arrange
            var book = BuildDefaultBook();

            // Act
            var response = await CreateAndTrackBook(book);

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            var created = await ApiClient.DeserializeResponse<Book>(response);
            Assert.That(created, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(created!.Id, Is.EqualTo(book.Id));
                Assert.That(created.Title, Is.EqualTo(book.Title));
                Assert.That(created.Description, Is.EqualTo(book.Description));
                Assert.That(created.Author, Is.EqualTo(book.Author),
                    "BUG #1a: Author field is not persisted on POST -- returned as null");
            });
        }

        [Test]
        public async Task AddBook_WithoutDescription_CreatesBookSuccessfully()
        {
            // Arrange -- Description is optional per the spec
            var book = BuildDefaultBook();
            book.Description = null;

            // Act
            var response = await CreateAndTrackBook(book);

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            var created = await ApiClient.DeserializeResponse<Book>(response);
            Assert.That(created, Is.Not.Null);
            Assert.That(created!.Id, Is.EqualTo(book.Id));
            Assert.That(created.Title, Is.EqualTo(book.Title));
            Assert.That(created.Description, Is.Null);
        }

        [Test]
        public async Task AddBook_MissingAuthor_ReturnsBadRequest()
        {
            // Arrange -- Author is required; send explicit null to distinguish from key-absent
            var id = NextId();
            CreatedBookIds.Add(id); 
            var json = $"{{\"Id\": {id}, \"Title\": \"SomeTitle\", \"Author\": null}}";

            // Act
            var response = await Client.CreateBookRawAsync(json);

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }

        [Test]
        public async Task AddBook_MissingTitle_ReturnsBadRequest()
        {
            // Arrange -- Title is required; send explicit null to distinguish from key-absent
            var id = NextId();
            CreatedBookIds.Add(id);
            var json = $"{{\"Id\": {id}, \"Author\": \"SomeAuthor\", \"Title\": null}}";

            // Act
            var response = await Client.CreateBookRawAsync(json);

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }

        [Test]
        public async Task AddBook_AuthorIsEmpty_ReturnsBadRequest()
        {
            // Arrange -- empty string should not satisfy the Author requirement
            var id = NextId();
            CreatedBookIds.Add(id);
            var json = $"{{\"Id\": {id}, \"Title\": \"SomeTitle\", \"Author\": \"\"}}";

            // Act
            var response = await Client.CreateBookRawAsync(json);

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }

        [Test]
        public async Task AddBook_TitleIsEmpty_ReturnsBadRequest()
        {
            // Arrange -- empty string should not satisfy the Title requirement
            var id = NextId();
            CreatedBookIds.Add(id);
            var json = $"{{\"Id\": {id}, \"Author\": \"SomeAuthor\", \"Title\": \"\"}}";

            // Act
            var response = await Client.CreateBookRawAsync(json);

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }

        [Test]
        public async Task AddBook_IdIsZero_ReturnsBadRequest()
        {
            // Arrange -- Id must be a positive integer
            CreatedBookIds.Add(0);
            var json = "{\"Id\": 0, \"Author\": \"Author\", \"Title\": \"Title\"}";

            // Act
            var response = await Client.CreateBookRawAsync(json);

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }

        [Test]
        public async Task AddBook_NegativeId_ReturnsBadRequest()
        {
            // Arrange
            var json = "{\"Id\": -1, \"Author\": \"Author\", \"Title\": \"Title\"}";

            // Act
            var response = await Client.CreateBookRawAsync(json);

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }

        [Test]
        public async Task AddBook_AuthorExceeds30Characters_ReturnsBadRequest()
        {
            // Arrange -- Author max is 30 chars
            var id = NextId();
            CreatedBookIds.Add(id);
            var longAuthor = new string('A', 31);
            var json = $"{{\"Id\": {id}, \"Author\": \"{longAuthor}\", \"Title\": \"ValidTitle\"}}";

            // Act
            var response = await Client.CreateBookRawAsync(json);

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }

        [Test]
        public async Task AddBook_TitleExceeds100Characters_ReturnsBadRequest()
        {
            // Arrange -- Title max is 100 chars
            var id = NextId();
            CreatedBookIds.Add(id);
            var longTitle = new string('T', 101);
            var json = $"{{\"Id\": {id}, \"Author\": \"ValidAuthor\", \"Title\": \"{longTitle}\"}}";

            // Act
            var response = await Client.CreateBookRawAsync(json);

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }

        [Test]
        public async Task AddBook_DuplicateId_ReturnsError()
        {
            // Arrange -- create a book, then try to create another with the same ID
            var book = BuildDefaultBook();
            await CreateAndTrackBook(book);

            var duplicate = new Book
            {
                Id = book.Id,
                Title = "DuplicateTitle",
                Author = "DuplicateAuthor"
            };

            // Act
            var response = await Client.CreateBookAsync(duplicate);

            // Assert -- should be either 400 Bad Request or 409 Conflict
            Assert.That((int)response.StatusCode, Is.AnyOf(400, 409),
                "Creating a book with a duplicate ID should return an error status code");
        }

        [Test]
        public async Task AddBook_AuthorAtMaxLength_Succeeds()
        {
            // Arrange -- Author exactly 30 chars should be accepted (max is documented as 30, inclusive)
            var book = BuildDefaultBook();
            book.Author = new string('A', 30);

            // Act
            var response = await CreateAndTrackBook(book);

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK),
                "BUG #4a: Author at exactly 30 characters should be accepted but is rejected on POST (off-by-one in length validation)");
        }

        [Test]
        public async Task AddBook_TitleAtMaxLength_Succeeds()
        {
            // Arrange -- Title exactly 100 chars should be accepted (max is documented as 100, inclusive)
            var book = BuildDefaultBook();
            book.Title = new string('T', 100);

            // Act
            var response = await CreateAndTrackBook(book);

            // Validation enforces length < 100 instead of length <= 100 (off-by-one).
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK),
                "BUG #5a: Title at exactly 100 characters should be accepted but is rejected on POST (off-by-one in length validation)");
        }

        [Test]
        public async Task AddBook_EmptyBody_ReturnsBadRequest()
        {
            // Act
            var response = await Client.CreateBookRawAsync("{}");

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }
    }
}
