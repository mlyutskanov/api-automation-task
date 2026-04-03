using System.Net;
using NUnit.Framework;
using LibraryManagerTests.Helpers;
using LibraryManagerTests.Models;
using LibraryManagerTests.TestSetup;

namespace LibraryManagerTests.Tests
{
    [TestFixture]
    public class DeleteBookTests : TestBase
    {
        [Test]
        public async Task DeleteBook_ExistingBook_Returns204NoContent()
        {
            // Arrange
            var book = BuildDefaultBook();
            await CreateAndTrackBook(book);

            // Act
            var response = await Client.DeleteBookAsync(book.Id);

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
        }

        [Test]
        public async Task DeleteBook_BookNoLongerAccessibleAfterDelete()
        {
            // Arrange
            var book = BuildDefaultBook();
            await CreateAndTrackBook(book);

            // Act -- delete, then try to GET
            await Client.DeleteBookAsync(book.Id);
            var getResponse = await Client.GetBookByIdAsync(book.Id);

            // Assert
            Assert.That(getResponse.StatusCode, Is.EqualTo(HttpStatusCode.NotFound),
                "Book should not be retrievable after deletion");
        }

        [Test]
        public async Task DeleteBook_NonExistentId_Returns404()
        {
            // Arrange
            var nonExistentId = 99997;

            // Act
            var response = await Client.DeleteBookAsync(nonExistentId);

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
            var error = await ApiClient.DeserializeResponse<ErrorResponse>(response);
            Assert.That(error?.Message, Does.Contain(nonExistentId.ToString()));
        }

        [Test]
        public async Task DeleteBook_DeleteSameBookTwice_SecondDeleteReturns404()
        {
            // Arrange
            var book = BuildDefaultBook();
            await CreateAndTrackBook(book);

            // Act
            var firstDelete = await Client.DeleteBookAsync(book.Id);
            var secondDelete = await Client.DeleteBookAsync(book.Id);

            // Assert
            Assert.That(firstDelete.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
            Assert.That(secondDelete.StatusCode, Is.EqualTo(HttpStatusCode.NotFound),
                "Deleting an already-deleted book should return 404");
        }

        [Test]
        public async Task DeleteBook_DoesNotAffectOtherBooks()
        {
            // Arrange
            var book1 = BuildDefaultBook();
            var book2 = BuildDefaultBook();
            await CreateAndTrackBook(book1);
            await CreateAndTrackBook(book2);

            // Act -- delete only book1
            await Client.DeleteBookAsync(book1.Id);

            // Assert -- book2 should still exist
            var getResponse = await Client.GetBookByIdAsync(book2.Id);
            Assert.That(getResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            var returned = await ApiClient.DeserializeResponse<Book>(getResponse);
            Assert.That(returned!.Id, Is.EqualTo(book2.Id));
        }
    }
}
