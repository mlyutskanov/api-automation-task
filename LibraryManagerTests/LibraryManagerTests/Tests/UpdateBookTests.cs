using System.Net;
using NUnit.Framework;
using LibraryManagerTests.Helpers;
using LibraryManagerTests.Models;
using LibraryManagerTests.TestSetup;

namespace LibraryManagerTests.Tests
{
    [TestFixture]
    public class UpdateBookTests : TestBase
    {
        [Test]
        public async Task UpdateBook_AuthorOfExistingBook_Succeeds()
        {
            // Arrange
            var book = BuildDefaultBook();
            await CreateAndTrackBook(book);

            var updatedBook = new Book
            {
                Id = book.Id,
                Title = book.Title!,
                Author = "UpdatedAuthor",
                Description = book.Description
            };

            // Act
            var response = await Client.UpdateBookAsync(book.Id, updatedBook);

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            var returned = await ApiClient.DeserializeResponse<Book>(response);
            Assert.That(returned, Is.Not.Null);
            Assert.That(returned!.Author, Is.EqualTo("UpdatedAuthor"));
        }

        [Test]
        public async Task UpdateBook_TitleOfExistingBook_Succeeds()
        {
            // Arrange
            var book = BuildDefaultBook();
            await CreateAndTrackBook(book);

            var updatedBook = new Book
            {
                Id = book.Id,
                Title = "UpdatedTitle",
                Author = book.Author!,
                Description = book.Description
            };

            // Act
            var response = await Client.UpdateBookAsync(book.Id, updatedBook);

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            var returned = await ApiClient.DeserializeResponse<Book>(response);
            Assert.That(returned, Is.Not.Null);
            Assert.That(returned!.Title, Is.EqualTo("UpdatedTitle"));
        }

        [Test]
        public async Task UpdateBook_DescriptionOfExistingBook_UpdatesSuccessfully()
        {
            // Arrange
            var book = BuildDefaultBook();
            book.Description = "OriginalDescription";
            await CreateAndTrackBook(book);

            var updatedBook = new Book
            {
                Id = book.Id,
                Title = book.Title!,
                Author = book.Author!,
                Description = "NewDescription"
            };

            // Act
            await Client.UpdateBookAsync(book.Id, updatedBook);
            var getResponse = await Client.GetBookByIdAsync(book.Id);

            // Assert
            Assert.That(getResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            var retrieved = await ApiClient.DeserializeResponse<Book>(getResponse);
            Assert.That(retrieved, Is.Not.Null);
            Assert.That(retrieved!.Description, Is.EqualTo("NewDescription"),
                "BUG #2: Description is not updated by PUT -- GET still returns the original value");
        }

        [Test]
        public async Task UpdateBook_NonExistentId_Returns404()
        {
            // Arrange
            var nonExistentId = 99998;
            var book = new Book
            {
                Id = nonExistentId,
                Title = "Title",
                Author = "Author"
            };

            // Act
            var response = await Client.UpdateBookAsync(nonExistentId, book);

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
            var error = await ApiClient.DeserializeResponse<ErrorResponse>(response);
            Assert.That(error?.Message, Does.Contain(nonExistentId.ToString()));
        }

        [Test]
        public async Task UpdateBook_AuthorExceeds30Characters_ReturnsBadRequest()
        {
            // Arrange
            var book = BuildDefaultBook();
            await CreateAndTrackBook(book);

            var updatedBook = new Book
            {
                Id = book.Id,
                Title = "ValidTitle",
                Author = new string('A', 31),
                Description = "Desc"
            };

            // Act
            var response = await Client.UpdateBookAsync(book.Id, updatedBook);

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }

        [Test]
        public async Task UpdateBook_AuthorAtMaxLength_Succeeds()
        {
            // Arrange -- Author exactly 30 chars should be accepted on PUT (same rule as POST)
            var book = BuildDefaultBook();
            await CreateAndTrackBook(book);

            var updatedBook = new Book
            {
                Id = book.Id,
                Title = book.Title!,
                Author = new string('A', 30),
                Description = book.Description
            };

            // Act
            var response = await Client.UpdateBookAsync(book.Id, updatedBook);

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK),
                "BUG #4b: Author at exactly 30 characters should be accepted but is rejected on PUT (off-by-one in length validation)");
        }

        [Test]
        public async Task UpdateBook_TitleExceeds100Characters_ReturnsBadRequest()
        {
            // Arrange
            var book = BuildDefaultBook();
            await CreateAndTrackBook(book);

            var updatedBook = new Book
            {
                Id = book.Id,
                Title = new string('T', 101),
                Author = "ValidAuthor",
                Description = "Desc"
            };

            // Act
            var response = await Client.UpdateBookAsync(book.Id, updatedBook);

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }

        [Test]
        public async Task UpdateBook_TitleAtMaxLength_Succeeds()
        {
            // Arrange -- Title exactly 100 chars should be accepted on PUT (same rule as POST)
            var book = BuildDefaultBook();
            await CreateAndTrackBook(book);

            var updatedBook = new Book
            {
                Id = book.Id,
                Title = new string('T', 100),
                Author = book.Author!,
                Description = book.Description
            };

            // Act
            var response = await Client.UpdateBookAsync(book.Id, updatedBook);

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK),
                "BUG #5b: Title at exactly 100 characters should be accepted but is rejected on PUT (off-by-one in length validation)");
        }

        [Test]
        public async Task UpdateBook_MissingRequiredFields_ReturnsBadRequest()
        {
            // Arrange
            var book = BuildDefaultBook();
            await CreateAndTrackBook(book);

            // Send only the ID -- missing Title and Author
            var json = $"{{\"Id\": {book.Id}}}";

            // Act
            var response = await Client.UpdateBookRawAsync(book.Id, json);

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }
    }
}
