using System.Net;
using NUnit.Framework;
using LibraryManagerTests.Helpers;
using LibraryManagerTests.Models;
using LibraryManagerTests.TestSetup;

namespace LibraryManagerTests.Tests
{
    [TestFixture]
    public class GetBookByIdTests : TestBase
    {
        [Test]
        public async Task GetBookById_ExistingBook_ReturnsCorrectBook()
        {
            // Arrange
            var book = BuildDefaultBook();
            await CreateAndTrackBook(book);

            // Act
            var response = await Client.GetBookByIdAsync(book.Id);

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            var returned = await ApiClient.DeserializeResponse<Book>(response);
            Assert.That(returned, Is.Not.Null);
            Assert.That(returned!.Id, Is.EqualTo(book.Id));
            Assert.That(returned.Title, Is.EqualTo(book.Title));
            Assert.That(returned.Description, Is.EqualTo(book.Description));
            Assert.That(returned.Author, Is.EqualTo(book.Author),
                "BUG #1b: Author is not persisted on POST -- GET returns null");
        }

        [Test]
        public async Task GetBookById_NonExistentId_Returns404WithMessage()
        {
            // Arrange
            var nonExistentId = 99999;

            // Act
            var response = await Client.GetBookByIdAsync(nonExistentId);

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
            var error = await ApiClient.DeserializeResponse<ErrorResponse>(response);
            Assert.That(error, Is.Not.Null);
            Assert.That(error!.Message, Does.Contain(nonExistentId.ToString()),
                "Error message should reference the requested ID");
        }
    }
}
