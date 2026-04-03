# Bug Reports -- Library Manager API

---

## BUG-001a: Author field not persisted on POST (response)

| Field            | Value                                                      |
|------------------|------------------------------------------------------------|
| **Bug ID**       | BUG-001a                                                   |
| **Title**        | Author field is not returned in POST response              |
| **Severity**     | High                                                       |
| **Priority**     | P1                                                         |
| **Component**    | POST /api/books                                            |
| **Status**       | Open                                                       |

### Steps to Reproduce

1. Send a POST request to `http://localhost:9000/api/books` with a valid book payload including an Author value.
2. Observe the Author field in the response body.

### HTTP Request
```
POST /api/books HTTP/1.1
Host: localhost:9000
Content-Type: application/json

{
    "Id": 100,
    "Title": "TestTitle",
    "Author": "TestAuthor",
    "Description": "TestDescription"
}
```

### Expected Result
The POST response should return the created book with the Author field populated:
```json
{
    "Id": 100,
    "Title": "TestTitle",
    "Description": "TestDescription",
    "Author": "TestAuthor"
}
```

### Actual Result
The Author field is `null` in the POST response:
```json
{
    "Id": 100,
    "Title": "TestTitle",
    "Description": "TestDescription",
    "Author": null
}
```

### Impact
- The Author field is a required field per the API specification, yet it is never stored.
- All books in the system will have a `null` Author regardless of what was provided during creation.
- This makes the Author field completely non-functional, rendering any author-related business logic broken.

### Test Cases
- `AddBookTests.AddBook_WithAllFields_ReturnsCreatedBookWithCorrectData`

### Related Bug
BUG-001b — the same root cause is observable when retrieving the book via GET.

---

## BUG-001b: Author field not persisted on POST (subsequent GET)

| Field            | Value                                                      |
|------------------|------------------------------------------------------------|
| **Bug ID**       | BUG-001b                                                   |
| **Title**        | Author field is null when retrieving a book via GET after POST |
| **Severity**     | High                                                       |
| **Priority**     | P1                                                         |
| **Component**    | GET /api/books/{id}                                        |
| **Status**       | Open                                                       |

### Steps to Reproduce

1. Send a POST request to `http://localhost:9000/api/books` with a valid book payload including an Author value.
2. Send a GET request to `http://localhost:9000/api/books/{id}` with the same Id.
3. Observe the Author field in the GET response.

### HTTP Requests

**Step 1 -- Create the book:**
```
POST /api/books HTTP/1.1
Host: localhost:9000
Content-Type: application/json

{
    "Id": 100,
    "Title": "TestTitle",
    "Author": "TestAuthor",
    "Description": "TestDescription"
}
```

**Step 2 -- Retrieve the book:**
```
GET /api/books/100 HTTP/1.1
Host: localhost:9000
```

### Expected Result
```json
{
    "Id": 100,
    "Title": "TestTitle",
    "Description": "TestDescription",
    "Author": "TestAuthor"
}
```

### Actual Result
```json
{
    "Id": 100,
    "Title": "TestTitle",
    "Description": "TestDescription",
    "Author": null
}
```

### Impact
- Any GET request for a book will always return `null` for Author, regardless of what was set on creation.
- Author-based filtering or display in client applications is entirely broken.

### Test Cases
- `GetBookByIdTests.GetBookById_ExistingBook_ReturnsCorrectBook`

### Related Bug
BUG-001a — the same root cause is also observable in the POST response.

---

## BUG-002: Description field not updated by PUT

| Field            | Value                                                      |
|------------------|------------------------------------------------------------|
| **Bug ID**       | BUG-002                                                    |
| **Title**        | PUT endpoint does not update the Description field         |
| **Severity**     | High                                                       |
| **Priority**     | P1                                                         |
| **Component**    | PUT /api/books/{id}                                        |
| **Status**       | Open                                                       |

### Steps to Reproduce

1. Create a book via POST with `Description = "OriginalDescription"`.
2. Send a PUT request to `http://localhost:9000/api/books/{id}` with `Description = "NewDescription"`.
3. Send a GET request to `http://localhost:9000/api/books/{id}`.
4. Observe the Description field in the GET response.

### HTTP Requests

**Step 1 -- Create the book:**
```
POST /api/books HTTP/1.1
Host: localhost:9000
Content-Type: application/json

{
    "Id": 200,
    "Title": "TestTitle",
    "Author": "TestAuthor",
    "Description": "OriginalDescription"
}
```

**Step 2 -- Update the book:**
```
PUT /api/books/200 HTTP/1.1
Host: localhost:9000
Content-Type: application/json

{
    "Id": 200,
    "Title": "UpdatedTitle",
    "Author": "UpdatedAuthor",
    "Description": "NewDescription"
}
```

**Step 3 -- Retrieve the book:**
```
GET /api/books/200 HTTP/1.1
Host: localhost:9000
```

### Expected Result
After the PUT request, subsequent GET should return:
```json
{
    "Id": 200,
    "Title": "UpdatedTitle",
    "Description": "NewDescription",
    "Author": "UpdatedAuthor"
}
```

### Actual Result
The Description field retains its original value:
```json
{
    "Id": 200,
    "Title": "UpdatedTitle",
    "Description": "OriginalDescription",
    "Author": "UpdatedAuthor"
}
```
Note: Title and Author are updated correctly; only Description is ignored.

### Impact
- Users cannot modify the Description of an existing book through the update endpoint.
- Any workflow that depends on updating book descriptions will silently fail (no error is returned, the field is simply ignored).

### Test Cases
- `UpdateBookTests.UpdateBook_DescriptionOfExistingBook_UpdatesSuccessfully`

---

## BUG-003: GET /api/books title filter is case-sensitive

| Field            | Value                                                      |
|------------------|------------------------------------------------------------|
| **Bug ID**       | BUG-003                                                    |
| **Title**        | Title search filter is case-sensitive instead of case-insensitive |
| **Severity**     | Medium                                                     |
| **Priority**     | P2                                                         |
| **Component**    | GET /api/books?title={title}                               |
| **Status**       | Open                                                       |

### Steps to Reproduce

1. Create a book with `Title = "CaseSensitivityTest"`.
2. Send a GET request to `http://localhost:9000/api/books?title=casesensitivitytest` (all lowercase).
3. Observe the response.

### HTTP Requests

**Step 1 -- Create the book:**
```
POST /api/books HTTP/1.1
Host: localhost:9000
Content-Type: application/json

{
    "Id": 300,
    "Title": "CaseSensitivityTest",
    "Author": "TestAuthor",
    "Description": "TestDesc"
}
```

**Step 2 -- Search with lowercase:**
```
GET /api/books?title=casesensitivitytest HTTP/1.1
Host: localhost:9000
```

### Expected Result
The filter should perform a case-insensitive "contains" match. The response should return the matching book:
```json
[
    {
        "Id": 300,
        "Title": "CaseSensitivityTest",
        "Description": "TestDesc",
        "Author": null
    }
]
```

### Actual Result
The API returns an empty array because the filter performs a case-sensitive comparison:
```json
[]
```

Searching with the exact case (`CaseSensitivityTest`) does return the book.

### Impact
- Users must know the exact casing of a book's title to find it via the search endpoint.
- This is a poor user experience and inconsistent with typical search behavior in library/catalogue systems, where case-insensitive search is the standard.
- End-user-facing applications built on this API will appear to have broken search functionality.

### Test Cases
- `GetAllBooksTests.GetAllBooks_WithTitleFilter_IsCaseInsensitive`

---

## BUG-004a: Author field rejects value at documented maximum length on POST (off-by-one)

| Field            | Value                                                      |
|------------------|------------------------------------------------------------|
| **Bug ID**       | BUG-004a                                                   |
| **Title**        | Author length validation rejects exactly 30 characters on POST |
| **Severity**     | Medium                                                     |
| **Priority**     | P2                                                         |
| **Component**    | POST /api/books                                            |
| **Status**       | Open                                                       |

### Steps to Reproduce

1. Send a POST request to `http://localhost:9000/api/books` with Author set to exactly 30 characters.
2. Observe the response status code.

### HTTP Request
```
POST /api/books HTTP/1.1
Host: localhost:9000
Content-Type: application/json

{
    "Id": 400,
    "Title": "TestTitle",
    "Author": "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAA",
    "Description": "TestDescription"
}
```
(Author is exactly 30 'A' characters)

### Expected Result
`200 OK` — the documented maximum of 30 characters should be an inclusive upper bound.

### Actual Result
`400 Bad Request` — the API rejects the value, indicating validation uses `length < 30` (exclusive) instead of `length <= 30` (inclusive).

### Impact
- Any consumer sending an Author of exactly 30 characters will receive an unexpected rejection.
- The documented and enforced limits are inconsistent, creating confusion for API consumers.

### Test Cases
- `AddBookTests.AddBook_AuthorAtMaxLength_Succeeds`

### Related Bug
BUG-004b — the same off-by-one validation error is present on the PUT endpoint.

---

## BUG-004b: Author field rejects value at documented maximum length on PUT (off-by-one)

| Field            | Value                                                      |
|------------------|------------------------------------------------------------|
| **Bug ID**       | BUG-004b                                                   |
| **Title**        | Author length validation rejects exactly 30 characters on PUT |
| **Severity**     | Medium                                                     |
| **Priority**     | P2                                                         |
| **Component**    | PUT /api/books/{id}                                        |
| **Status**       | Open                                                       |

### Steps to Reproduce

1. Create a book via POST.
2. Send a PUT request to `http://localhost:9000/api/books/{id}` with Author set to exactly 30 characters.
3. Observe the response status code.

### HTTP Request
```
PUT /api/books/400 HTTP/1.1
Host: localhost:9000
Content-Type: application/json

{
    "Id": 400,
    "Title": "TestTitle",
    "Author": "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAA",
    "Description": "TestDescription"
}
```
(Author is exactly 30 'A' characters)

### Expected Result
`200 OK` — the documented maximum of 30 characters should be an inclusive upper bound.

### Actual Result
`400 Bad Request` — the API rejects the value, indicating validation uses `length < 30` (exclusive) instead of `length <= 30` (inclusive).

### Impact
- Any consumer updating an Author to exactly 30 characters will receive an unexpected rejection.
- The validation rule is identical to the POST bug, confirming a shared validation implementation defect.

### Test Cases
- `UpdateBookTests.UpdateBook_AuthorAtMaxLength_Succeeds`

### Related Bug
BUG-004a — the same off-by-one validation error is present on the POST endpoint.

---

## BUG-005a: Title field rejects value at documented maximum length on POST (off-by-one)

| Field            | Value                                                      |
|------------------|------------------------------------------------------------|
| **Bug ID**       | BUG-005a                                                   |
| **Title**        | Title length validation rejects exactly 100 characters on POST |
| **Severity**     | Medium                                                     |
| **Priority**     | P2                                                         |
| **Component**    | POST /api/books                                            |
| **Status**       | Open                                                       |

### Steps to Reproduce

1. Send a POST request to `http://localhost:9000/api/books` with Title set to exactly 100 characters.
2. Observe the response status code.

### HTTP Request
```
POST /api/books HTTP/1.1
Host: localhost:9000
Content-Type: application/json

{
    "Id": 500,
    "Title": "TTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTT",
    "Author": "TestAuthor",
    "Description": "TestDescription"
}
```
(Title is exactly 100 'T' characters)

### Expected Result
`200 OK` — the documented maximum of 100 characters should be an inclusive upper bound.

### Actual Result
`400 Bad Request` — the API rejects the value, indicating validation uses `length < 100` (exclusive) instead of `length <= 100` (inclusive).

### Impact
- Any consumer sending a Title of exactly 100 characters will receive an unexpected rejection.
- The documented and enforced limits are inconsistent, creating confusion for API consumers.

### Test Cases
- `AddBookTests.AddBook_TitleAtMaxLength_Succeeds`

### Related Bug
BUG-005b — the same off-by-one validation error is present on the PUT endpoint.

---

## BUG-005b: Title field rejects value at documented maximum length on PUT (off-by-one)

| Field            | Value                                                      |
|------------------|------------------------------------------------------------|
| **Bug ID**       | BUG-005b                                                   |
| **Title**        | Title length validation rejects exactly 100 characters on PUT |
| **Severity**     | Medium                                                     |
| **Priority**     | P2                                                         |
| **Component**    | PUT /api/books/{id}                                        |
| **Status**       | Open                                                       |

### Steps to Reproduce

1. Create a book via POST.
2. Send a PUT request to `http://localhost:9000/api/books/{id}` with Title set to exactly 100 characters.
3. Observe the response status code.

### HTTP Request
```
PUT /api/books/500 HTTP/1.1
Host: localhost:9000
Content-Type: application/json

{
    "Id": 500,
    "Title": "TTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTT",
    "Author": "TestAuthor",
    "Description": "TestDescription"
}
```
(Title is exactly 100 'T' characters)

### Expected Result
`200 OK` — the documented maximum of 100 characters should be an inclusive upper bound.

### Actual Result
`400 Bad Request` — the API rejects the value, indicating validation uses `length < 100` (exclusive) instead of `length <= 100` (inclusive).

### Impact
- Any consumer updating a Title to exactly 100 characters will receive an unexpected rejection.
- The validation rule is identical to the POST bug, confirming a shared validation implementation defect.

### Test Cases
- `UpdateBookTests.UpdateBook_TitleAtMaxLength_Succeeds`

### Related Bug
BUG-005a — the same off-by-one validation error is present on the POST endpoint.
