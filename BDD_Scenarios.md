# BDD Test Scenarios -- Library Manager API

All scenarios target the Library Manager API running at `http://localhost:9000`.

---

## Feature: Get All Books

### Scenario: Retrieve books when none exist
```gherkin
Given no books exist in the system
When I send a GET request to "/api/books"
Then the response status code should be 200
And the response body should be an empty JSON array
```

### Scenario: Retrieve all books
```gherkin
Given the following books exist:
  | Id | Title       | Author       | Description  |
  | 1  | First Book  | First Author | A first book |
  | 2  | Second Book | Second Author| A second book|
When I send a GET request to "/api/books"
Then the response status code should be 200
And the response body should contain 2 books
And the response should include a book with Id 1
And the response should include a book with Id 2
```

### Scenario: Filter books by title (partial match)
```gherkin
Given the following books exist:
  | Id | Title            | Author  |
  | 1  | UniqueSearchable | Author1 |
  | 2  | DifferentTitle   | Author2 |
When I send a GET request to "/api/books?title=Searchable"
Then the response status code should be 200
And the response body should contain 1 book
And the response should include a book with Title "UniqueSearchable"
And the response should not include a book with Title "DifferentTitle"
```

### Scenario: Filter books by title with no match
```gherkin
Given a book with Title "ExistingTitle" exists
When I send a GET request to "/api/books?title=NonExistentTitle"
Then the response status code should be 200
And the response body should be an empty JSON array
```

### Scenario: Title filter is case-insensitive (BUG #3)
```gherkin
Given a book with Title "CaseSensitivityTest" exists
When I send a GET request to "/api/books?title=casesensitivitytest"
Then the response status code should be 200
And the response body should contain 1 book with Title "CaseSensitivityTest"
```
> **BUG #3**: Title filter is case-sensitive -- the API returns an empty array instead of the matching book.

### Scenario: Empty title filter returns all books
```gherkin
Given a book exists in the system
When I send a GET request to "/api/books?title="
Then the response status code should be 200
And the response body should contain all books
```

---

## Feature: Get Book by ID

### Scenario: Retrieve an existing book by ID
```gherkin
Given a book exists with Id 1, Title "TestTitle", Author "TestAuthor", Description "TestDesc"
When I send a GET request to "/api/books/1"
Then the response status code should be 200
And the response body should contain:
  | Field       | Value       |
  | Id          | 1           |
  | Title       | TestTitle   |
  | Author      | TestAuthor  |
  | Description | TestDesc    |
```

### Scenario: Retrieve a non-existent book
```gherkin
Given no book exists with Id 99999
When I send a GET request to "/api/books/99999"
Then the response status code should be 404
And the response body should contain a "Message" field referencing Id 99999
```

### Scenario: GET returns all fields correctly
```gherkin
Given a book is created with all fields populated
When I send a GET request to "/api/books/{id}"
Then the response status code should be 200
And all fields (Id, Title, Description) should match the created values
```

---

## Feature: Add a New Book (POST)

### Scenario: Create a book with all fields (BUG #1a)
```gherkin
Given the request body contains:
  | Field       | Value         |
  | Id          | 1             |
  | Title       | TestTitle     |
  | Author      | TestAuthor    |
  | Description | TestDesc      |
When I send a POST request to "/api/books"
Then the response status code should be 200
And the response body should contain:
  | Field       | Value         |
  | Id          | 1             |
  | Title       | TestTitle     |
  | Author      | TestAuthor    |
  | Description | TestDesc      |
```
> **BUG #1a**: Author field is not persisted on POST -- the POST response returns Author as `null`.

### Scenario: Author is persisted and retrievable after POST (BUG #1b)
```gherkin
Given I create a book with Author "TestAuthor" via POST
When I send a GET request to "/api/books/{id}"
Then the response body "Author" field should be "TestAuthor"
```
> **BUG #1b**: Author is not persisted on POST -- GET returns `null` even after creation.

### Scenario: Create a book without description
```gherkin
Given the request body contains Id, Title, and Author but no Description
When I send a POST request to "/api/books"
Then the response status code should be 200
And the response body Description should be null
```

### Scenario: Missing Author returns 400
```gherkin
Given the request body contains Id and Title but no Author
When I send a POST request to "/api/books"
Then the response status code should be 400
```

### Scenario: Missing Title returns 400
```gherkin
Given the request body contains Id and Author but no Title
When I send a POST request to "/api/books"
Then the response status code should be 400
```

### Scenario: Id of zero returns 400
```gherkin
Given the request body contains Id 0, a valid Author, and a valid Title
When I send a POST request to "/api/books"
Then the response status code should be 400
```

### Scenario: Negative Id returns 400
```gherkin
Given the request body contains Id -1, a valid Author, and a valid Title
When I send a POST request to "/api/books"
Then the response status code should be 400
```

### Scenario: Author exceeding 30 characters returns 400
```gherkin
Given the request body contains an Author of 31 characters
When I send a POST request to "/api/books"
Then the response status code should be 400
```

### Scenario: Title exceeding 100 characters returns 400
```gherkin
Given the request body contains a Title of 101 characters
When I send a POST request to "/api/books"
Then the response status code should be 400
```

### Scenario: Duplicate Id returns error
```gherkin
Given a book with Id 1 already exists
And the request body contains Id 1 with different Title and Author
When I send a POST request to "/api/books"
Then the response status code should be 400 or 409
```

### Scenario: Author at exactly 30 characters is accepted on POST (BUG #4a)
```gherkin
Given the request body contains an Author of exactly 30 characters
When I send a POST request to "/api/books"
Then the response status code should be 200
```
> **BUG #4a**: POST returns 400 because Author length validation is exclusive (< 30) instead of inclusive (<= 30).

### Scenario: Title at exactly 100 characters is accepted on POST (BUG #5a)
```gherkin
Given the request body contains a Title of exactly 100 characters
When I send a POST request to "/api/books"
Then the response status code should be 200
```
> **BUG #5a**: POST returns 400 because Title length validation is exclusive (< 100) instead of inclusive (<= 100).

### Scenario: Empty body returns 400
```gherkin
Given the request body is an empty JSON object "{}"
When I send a POST request to "/api/books"
Then the response status code should be 400
```

---

## Feature: Update a Book (PUT)

### Scenario: Update all fields (BUG #2)
```gherkin
Given a book exists with Id 1, Title "OldTitle", Author "OldAuthor", Description "OldDesc"
And the request body contains:
  | Field       | Value              |
  | Id          | 1                  |
  | Title       | UpdatedTitle       |
  | Author      | UpdatedAuthor      |
  | Description | UpdatedDescription |
When I send a PUT request to "/api/books/1"
Then the response status code should be 200
And the response body should contain:
  | Field       | Value              |
  | Title       | UpdatedTitle       |
  | Author      | UpdatedAuthor      |
  | Description | UpdatedDescription |
```
> **BUG #2**: Description is not updated by PUT -- it retains its original value.

### Scenario: Description persisted after PUT (BUG #2)
```gherkin
Given a book exists with Description "OriginalDescription"
When I send a PUT request updating Description to "NewDescription"
And I send a GET request to "/api/books/{id}"
Then the response body Description should be "NewDescription"
```
> **BUG #2**: GET still returns "OriginalDescription" after PUT -- Description is ignored by the update endpoint.

### Scenario: Update Title and Author only
```gherkin
Given a book exists with Id 1
And the request body updates Title to "NewTitle" and Author to "NewAuthor"
When I send a PUT request to "/api/books/1"
Then the response status code should be 200
And the Title should be "NewTitle"
```

### Scenario: Update a non-existent book returns 404
```gherkin
Given no book exists with Id 99998
When I send a PUT request to "/api/books/99998" with a valid book body
Then the response status code should be 404
And the error message should reference Id 99998
```

### Scenario: Body Id does not match URL Id returns 400
```gherkin
Given a book exists with Id 1
And the request body contains Id 2
When I send a PUT request to "/api/books/1"
Then the response status code should be 400
```

### Scenario: Author exceeding 30 characters on update returns 400
```gherkin
Given a book exists with Id 1
And the request body contains an Author of 31 characters
When I send a PUT request to "/api/books/1"
Then the response status code should be 400
```

### Scenario: Author at exactly 30 characters is accepted on PUT (BUG #4b)
```gherkin
Given a book exists with Id 1
And the request body contains an Author of exactly 30 characters
When I send a PUT request to "/api/books/1"
Then the response status code should be 200
```
> **BUG #4b**: PUT returns 400 because Author length validation is exclusive (< 30) instead of inclusive (<= 30).

### Scenario: Title exceeding 100 characters on update returns 400
```gherkin
Given a book exists with Id 1
And the request body contains a Title of 101 characters
When I send a PUT request to "/api/books/1"
Then the response status code should be 400
```

### Scenario: Title at exactly 100 characters is accepted on PUT (BUG #5b)
```gherkin
Given a book exists with Id 1
And the request body contains a Title of exactly 100 characters
When I send a PUT request to "/api/books/1"
Then the response status code should be 200
```
> **BUG #5b**: PUT returns 400 because Title length validation is exclusive (< 100) instead of inclusive (<= 100).

### Scenario: Missing required fields on update returns 400
```gherkin
Given a book exists with Id 1
And the request body contains only the Id field
When I send a PUT request to "/api/books/1"
Then the response status code should be 400
```

---

## Feature: Delete a Book

### Scenario: Delete an existing book
```gherkin
Given a book exists with Id 1
When I send a DELETE request to "/api/books/1"
Then the response status code should be 204
```

### Scenario: Deleted book is no longer accessible
```gherkin
Given a book exists with Id 1
When I send a DELETE request to "/api/books/1"
And I send a GET request to "/api/books/1"
Then the GET response status code should be 404
```

### Scenario: Delete a non-existent book returns 404
```gherkin
Given no book exists with Id 99997
When I send a DELETE request to "/api/books/99997"
Then the response status code should be 404
And the error message should reference Id 99997
```

### Scenario: Delete the same book twice
```gherkin
Given a book exists with Id 1
When I send a DELETE request to "/api/books/1"
And I send a second DELETE request to "/api/books/1"
Then the first DELETE response status code should be 204
And the second DELETE response status code should be 404
```

### Scenario: Deleting a book does not affect other books
```gherkin
Given books exist with Id 1 and Id 2
When I send a DELETE request to "/api/books/1"
And I send a GET request to "/api/books/2"
Then the GET response status code should be 200
And the response body should contain the book with Id 2
```
