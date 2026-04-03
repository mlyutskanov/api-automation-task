using System.Net;
using NUnit.Framework;
using LibraryManagerTests.Helpers;
using LibraryManagerTests.Models;
using LibraryManagerTests.TestSetup;

namespace LibraryManagerTests.Tests
{
    [TestFixture]
    public class GetAllBooksTests : TestBase
    {
        [Test]
        public async Task GetAllBooks_WhenNoBooksExist_ReturnsEmptyList()
        {
            // Arrange -- ensure the system has no books (the API is stateful,
            // so leftover data from other test fixtures or manual testing must be cleared)
            await CleanAllBooks();

            // Act
            var response = await Client.GetBooksAsync();

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            var books = await ApiClient.DeserializeBookList(response);
            Assert.That(books, Is.Empty);
        }

        [Test]
        public async Task GetAllBooks_WhenBooksExist_ReturnsAllBooks()
        {
            // Arrange
            var book1 = BuildDefaultBook();
            var book2 = BuildDefaultBook();
            await CreateAndTrackBook(book1);
            await CreateAndTrackBook(book2);

            // Act
            var response = await Client.GetBooksAsync();

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            var books = await ApiClient.DeserializeBookList(response);
            Assert.That(books.Count, Is.GreaterThanOrEqualTo(2));
            Assert.That(books.Any(b => b.Id == book1.Id), Is.True, "Book 1 should be in the list");
            Assert.That(books.Any(b => b.Id == book2.Id), Is.True, "Book 2 should be in the list");
        }

        [Test]
        public async Task GetAllBooks_WithTitleFilter_ReturnsMatchingBooks()
        {
            // Arrange -- create books with distinct titles
            var matchingBook = BuildDefaultBook();
            matchingBook.Title = "UniqueSearchable";
            await CreateAndTrackBook(matchingBook);

            var nonMatchingBook = BuildDefaultBook();
            nonMatchingBook.Title = "CompletelyDifferent";
            await CreateAndTrackBook(nonMatchingBook);

            // Act -- search with partial match
            var response = await Client.GetBooksAsync("Searchable");

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            var books = await ApiClient.DeserializeBookList(response);
            Assert.That(books.Any(b => b.Id == matchingBook.Id), Is.True,
                "Matching book should appear in filtered results");
            Assert.That(books.All(b => b.Id != nonMatchingBook.Id), Is.True,
                "Non-matching book should NOT appear in filtered results");
        }

        [Test]
        public async Task GetAllBooks_WithTitleFilter_NoMatch_ReturnsEmptyList()
        {
            // Arrange
            var book = BuildDefaultBook();
            await CreateAndTrackBook(book);

            // Act
            var response = await Client.GetBooksAsync("ZZZNoSuchTitle999");

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            var books = await ApiClient.DeserializeBookList(response);
            Assert.That(books, Is.Empty);
        }

        [Test]
        public async Task GetAllBooks_WithTitleFilter_IsCaseInsensitive()
        {
            // Arrange
            var book = BuildDefaultBook();
            book.Title = "CaseSensitivityTest";
            await CreateAndTrackBook(book);

            // Act -- search with all-lowercase version of the title
            var response = await Client.GetBooksAsync("casesensitivitytest");

            // Assert -- should find the book regardless of case
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            var books = await ApiClient.DeserializeBookList(response);
            Assert.That(books.Any(b => b.Id == book.Id), Is.True,
                "BUG #3: Title filter is case-sensitive -- should be case-insensitive");
        }

        [Test]
        public async Task GetAllBooks_WithEmptyTitleFilter_ReturnsAllBooks()
        {
            // Arrange
            var book = BuildDefaultBook();
            await CreateAndTrackBook(book);

            // Act -- empty title param should behave like no filter
            var response = await Client.GetBooksAsync("");

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            var books = await ApiClient.DeserializeBookList(response);
            Assert.That(books.Count, Is.GreaterThanOrEqualTo(1));
        }
    }
}
