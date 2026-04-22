using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

using HttpClient client = new HttpClient();
string apiUrl = "http://localhost:5000/api/v2/books";
string authUrl = "http://localhost:5000/api/v2/login";
string usersUrl = "http://localhost:5000/api/v2/users";

string? currentAccessToken = null;
string? currentRefreshToken = null;
DateTime tokenExpirationTime = DateTime.MinValue;

bool isRunning = true;

Console.WriteLine("Welcome to your Library!");

while (isRunning)
{

    Console.WriteLine("0. Exit");
    Console.WriteLine("1. Get all books");
    Console.WriteLine("2. Get book by ID");
    Console.WriteLine("3. Add a new book");
    Console.WriteLine("4. Update a book");
    Console.WriteLine("5. Delete a book");
    Console.WriteLine("6. Login");
    Console.WriteLine("7. Logout");
    Console.WriteLine("8. Get all users (Admin only)");
    Console.WriteLine("9. Get user by ID (Admin only)");
    Console.WriteLine("10. Get user's refresh tokens (Admin only)");

    Console.WriteLine("Select an option: ");
    string? choice = Console.ReadLine();
    Console.WriteLine();

    try
    {
        if (choice is "1" or "2" or "3" or "4" or "5" or "8" or "9" or "10")
        {
            await EnsureTokenIsValid(client, authUrl);
        }

        switch (choice)
        {
            case "1":
                await GetAllBooks(client, apiUrl);
                break;
            case "2":
                await GetBookById(client, apiUrl);
                break;
            case "3":
                await AddBook(client, apiUrl);
                break;
            case "4":
                await UpdateBook(client, apiUrl);
                break;
            case "5":
                await DeleteBook(client, apiUrl);
                break;
            case "6":
                await Login(client, authUrl);
                break;
            case "7":
                await Logout(client, authUrl);
                break;
            case "8": 
                await GetAllUsers(client, usersUrl); 
                break;
            case "9": 
                await GetUserById(client, usersUrl); 
                break;
            case "10": 
                await GetUserRefreshTokens(client, usersUrl); 
                break;
            case "0":
                isRunning = false;
                Console.WriteLine("Bye!!!!");
                break;
            default:
                Console.WriteLine("Invalid option. Please try again.");
                break;
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"\nSomething went wrong:\n{ex.Message}");
    }

    Console.WriteLine();
}

async Task Login(HttpClient client, string url)
{
    Console.WriteLine("Login");
    Console.Write("Username:");
    string? username = Console.ReadLine();

    Console.Write("Password: ");
    string? password = Console.ReadLine();

    var loginRequest = new { Username = username, Password = password };
    var response = await client.PostAsJsonAsync(url, loginRequest);

    if (response.IsSuccessStatusCode)
    {
        var authResult = await response.Content.ReadFromJsonAsync<AuthenticateResponse>();
        if (authResult != null)
        {
            currentAccessToken = authResult.AccessToken;
            currentRefreshToken = authResult.RefreshToken;

            tokenExpirationTime = DateTime.Now.AddMinutes(5);

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", currentAccessToken);

            Console.WriteLine("Login successful!");
        }
    }
    else
    {
        Console.WriteLine($"Login failed: {response.StatusCode}");
    }
}

async Task Logout(HttpClient client, string url)
{
    if (!string.IsNullOrEmpty(currentRefreshToken))
    {
        var revokeRequest = new { RefreshToken = currentRefreshToken };
        var response = await client.PostAsJsonAsync($"{url}/revoke-token", revokeRequest);

        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine("Failed to revoke refresh token, but clearing local data.]");
        }
    }

    currentAccessToken = null;
    currentRefreshToken = null;

    client.DefaultRequestHeaders.Authorization = null;

    Console.WriteLine("Logout successfully!");
}

async Task EnsureTokenIsValid(HttpClient client, string url)
{
    if (string.IsNullOrEmpty(currentAccessToken))
        return;

    if (DateTime.Now >= tokenExpirationTime.AddSeconds(-30) && DateTime.Now < tokenExpirationTime)
    {
        Console.WriteLine("Access token is expiring soon. Renewing automatically.");
        
        var response = await client.PostAsync($"{url}/renew", null);

        if (response.IsSuccessStatusCode)
        {
            var authResult = await response.Content.ReadFromJsonAsync<AuthenticateResponse>();
            if (authResult != null)
            {
                currentAccessToken = authResult.AccessToken;
                tokenExpirationTime = DateTime.Now.AddMinutes(5); 
                
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", currentAccessToken);
            }
        }
        else
        {
            Console.WriteLine("Failed to renew token.]");
            currentAccessToken = null;
            client.DefaultRequestHeaders.Authorization = null;
        }
    }
}

static async Task GetAllUsers(HttpClient client, string url)
{
    Console.WriteLine("All Users:");
    var response = await client.GetAsync(url);

    if (response.IsSuccessStatusCode)
    {
        var users = await response.Content.ReadFromJsonAsync<List<UserModel>>();
        if (users != null)
        {
            foreach (var user in users)
            {
                Console.WriteLine(user);
            }
        }
    }
    else if (response.StatusCode == System.Net.HttpStatusCode.NoContent)
        Console.WriteLine("\nFAILED: No user found.");
    else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized) 
        Console.WriteLine("\nFAILED: Unauthorized. Login first.");
    else if (response.StatusCode == System.Net.HttpStatusCode.Forbidden) 
        Console.WriteLine("\nFAILED: Forbidden. You need Admin rights.");
    else 
        Console.WriteLine($"\nFAILED: Status {response.StatusCode}");
}

static async Task GetUserById(HttpClient client, string url)
{
    Console.Write("Enter user ID: ");
    string? idInput = Console.ReadLine();

    var response = await client.GetAsync($"{url}/{idInput}");

    if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        Console.WriteLine($"\nUser not found.");
    else if (response.IsSuccessStatusCode)
    {
        var user = await response.Content.ReadFromJsonAsync<UserModel>();
        Console.WriteLine(user);
    }
    else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized) 
        Console.WriteLine("\nFAILED: Unauthorized. Login first.");
    else if (response.StatusCode == System.Net.HttpStatusCode.Forbidden) 
        Console.WriteLine("\nFAILED: Forbidden. You can only view your own profile unless you are Admin.");
    else 
        Console.WriteLine($"\nFAILED: Status {response.StatusCode}");
}

static async Task GetUserRefreshTokens(HttpClient client, string url)
{
    Console.Write("Enter user ID: ");
    string? idInput = Console.ReadLine();

    var response = await client.GetAsync($"{url}/{idInput}/refresh-tokens");

    if (response.StatusCode == System.Net.HttpStatusCode.NoContent)
        Console.WriteLine("\nNo tokens found.");
    else if (response.IsSuccessStatusCode)
    {
        var tokens = await response.Content.ReadFromJsonAsync<List<RefreshTokenEntry>>();

        if (tokens != null)
        {
            foreach (var token in tokens)
                Console.WriteLine(token);
        }
    }
    else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized) 
        Console.WriteLine("\nFAILED: Unauthorized. Login first.");
    else if (response.StatusCode == System.Net.HttpStatusCode.Forbidden) 
        Console.WriteLine("\nFAILED: Forbidden.");
    else 
        Console.WriteLine($"\nFAILED: Status {response.StatusCode}");
}

static async Task GetAllBooks(HttpClient client, string url)
{
    Console.WriteLine("My books:");

    var response = await client.GetAsync(url);

    if (response.StatusCode == System.Net.HttpStatusCode.NoContent)
    {
        Console.WriteLine("Your library is empty.");
        return;
    }
    else if (response.IsSuccessStatusCode)
    {
        var books = await client.GetFromJsonAsync<List<BookModel>>(url);

        if (books != null && books.Count > 0)
        {
            foreach (var book in books)
            {
                Console.WriteLine(book);
            }
        }
    }
    else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
    {
        Console.WriteLine("\nFAILED: Unauthorized. Login first.");
    }
    else if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
    {
        Console.WriteLine("\nFAILED: Forbidden.");
    }
    else
    {
        Console.WriteLine($"\nFAILED: Status {response.StatusCode}");
    }
}

static async Task GetBookById(HttpClient client, string url)
{
    Console.WriteLine("Get a book by ID:");

    Console.Write("Enter book ID: ");
    string? idInput = Console.ReadLine();

    var response = await client.GetAsync($"{url}/{idInput}");

    if (response.IsSuccessStatusCode)
    {
        var book = await response.Content.ReadFromJsonAsync<BookModel>();
        Console.WriteLine($"\nBook found: \n{book}");
    }
    else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
    {
        Console.WriteLine($"\nNo book with ID {idInput}");
    }
    else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
    {
        Console.WriteLine("\nFAILED: Unauthorized. Login first.");
    }
    else if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
    {
        Console.WriteLine("\nFAILED: Forbidden.");
    }
    else
    {
        Console.WriteLine($"\nFAILED: Status {response.StatusCode}");
    }
}

static async Task AddBook(HttpClient client, string url)
{
    Console.WriteLine("Add a new book:");

    Console.Write("Enter Title: ");
    string title = Console.ReadLine();

    Console.Write("Enter Author: ");
    string author = Console.ReadLine();

    Console.Write("Enter Publisher: ");
    string publisher = Console.ReadLine();

    Console.Write("Enter Publication Year: ");
    int.TryParse(Console.ReadLine(), out int publicationYear);

    Console.Write("Enter Page Count: ");
    int.TryParse(Console.ReadLine(), out int pageCount);

    Console.WriteLine("Available Genres: Fantasy, ScienceFiction, Fiction, Classic, Thriller, Romance, Horror, Poetry");
    Console.Write("New Genre: ");
    Enum.TryParse(Console.ReadLine(), true, out Genre genre);

    var newBook = new BookModel
    {
        Id = Guid.Empty,
        Title = title,
        Author = author,
        Publisher = publisher,
        PublicationYear = publicationYear,
        PageCount = pageCount,
        Genre = genre
    };

    var response = await client.PostAsJsonAsync(url, newBook);

    if (response.IsSuccessStatusCode)
    {
        var createdBook = await response.Content.ReadFromJsonAsync<BookModel>();

        Console.WriteLine($"Book added: \n {createdBook}");
    }
    else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
    {
        Console.WriteLine("\nFAILED: Unauthorized. Login first.");
    }
    else if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
    {
        Console.WriteLine("\nFAILED: Forbidden.");
    }
    else
        Console.WriteLine($"FAILED: {response.StatusCode}");
}

static async Task UpdateBook(HttpClient client, string url)
{
    Console.WriteLine("Update a book");

    Console.Write("Enter book ID: ");
    string? idInput = Console.ReadLine();

    Guid.TryParse(idInput, out Guid bookId);

    Console.Write("Enter new Title: ");
    string title = Console.ReadLine();

    Console.Write("Enter new Author: ");
    string author = Console.ReadLine();

    Console.Write("Enter new Publisher: ");
    string publisher = Console.ReadLine();

    Console.Write("Enter new Publication Year: ");
    int.TryParse(Console.ReadLine(), out int publicationYear);

    Console.Write("Enter new Page Count: ");
    int.TryParse(Console.ReadLine(), out int pageCount);

    Console.WriteLine("Available genres: Fantasy, ScienceFiction, Fiction, Classic, Thriller, Romance, Horror, Poetry");
    Console.Write("New Genre: ");
    Enum.TryParse(Console.ReadLine(), true, out Genre genre);

    var updatedBook = new BookModel
    {
        Id = bookId,
        Title = title,
        Author = author,
        Publisher = publisher,
        PublicationYear = publicationYear,
        PageCount = pageCount,
        Genre = genre
    };

    var response = await client.PutAsJsonAsync($"{url}/{bookId}", updatedBook);

    if (response.IsSuccessStatusCode)
    {
        var result = await response.Content.ReadFromJsonAsync<BookModel>();
        Console.WriteLine($"\nBook updated: \n{result}");
    }
    else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
    {
        Console.WriteLine($"\nNo book with ID {idInput}");
    }
    else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
    {
        Console.WriteLine("\nFAILED: Unauthorized. Login first.");
    }
    else if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
    {
        Console.WriteLine("\nFAILED: Forbidden.");
    }
    else
    {
        Console.WriteLine($"\nFAILED: Status {response.StatusCode}");
    }
}

static async Task DeleteBook(HttpClient client, string url)
{
    Console.WriteLine("Delete a book:");

    Console.Write("Enter book ID: ");
    string? idInput = Console.ReadLine();

    var response = await client.DeleteAsync($"{url}/{idInput}");

    if (response.IsSuccessStatusCode)
    {
        Console.WriteLine("\nBook deleted.");
    }
    else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
    {
        Console.WriteLine($"\nNo book with ID {idInput}");
    }
    else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
    {
        Console.WriteLine("\nFAILED: Unauthorized. Login first.");
    }
    else if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
    {
        Console.WriteLine("\nFAILED: Forbidden.");
    }
    else
    {
        Console.WriteLine($"\nFAILED: Status {response.StatusCode}");
    }
}

public class AuthenticateResponse
{
    public string IdToken { get; set; } = string.Empty;
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
}

public class UserModel
{
    public Guid? Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string UserRole { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    public override string ToString()
    {
        return $"\n{nameof(Id)}: {Id}, " +
               $"\n{nameof(FirstName)}: {FirstName}, " +
               $"\n{nameof(LastName)}: {LastName}, " +
               $"\n{nameof(Username)}: {Username}, " +
               $"\n{nameof(UserRole)}: {UserRole}, " +
               $"\n{nameof(Email)}: {Email}";
    }
}

public class RefreshTokenEntry
{
    public string Token { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public DateTime Expiry { get; set; }

    public override string ToString()
    {
        return $"\n{nameof(Token)}: {Token}, " +
               $"\n{nameof(UserId)}: {UserId}, " +
               $"\n{nameof(Expiry)}: {Expiry}";
    }
}

public class BookModel
{
    public Guid? Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string Publisher { get; set; } = string.Empty;
    public int PublicationYear { get; set; }
    public int PageCount { get; set; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public Genre Genre { get; set; }

    public override string ToString()
    {
        return $"\n{nameof(Id)}: {Id}, " +
               $"\n{nameof(Title)}: {Title}, " +
               $"\n{nameof(Author)}: {Author}, " +
               $"\n{nameof(Publisher)}: {Publisher}, " +
               $"\n{nameof(PublicationYear)}: {PublicationYear}, " +
               $"\n{nameof(PageCount)}: {PageCount}, " +
               $"\n{nameof(Genre)}: {Genre}";
    }
}

public enum Genre
{
    Fantasy,
    ScienceFiction,
    Fiction,
    Classic,
    Thriller,
    Romance,
    Horror,
    Poetry
}