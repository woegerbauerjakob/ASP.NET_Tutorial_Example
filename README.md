# 1 Initial Setup
In this chapter, we create the base solution containing all projects needed for this tutorial.

## 1.1 Solution Creation
* **Create Blank Solution:**
    1. Open Visual Studio.
    2. Click **Create a new project**.
    3. In the search bar, type **Blank Solution**.
    4. Select the **Blank Solution** template and click **Next**.
    5. Enter the Project name: `CinemaApp`.
    6. Click **Create**.

## 1.2 Add the Domain Project (Class Library)
* **Create Domain Project:**
    1. In the Solution Explorer, right-click on Solution `CinemaApp`.
    2. Select **Add > New Project**.
    3. Search for **Class Library** (choose the C# one).
    4. Click **Next**.
    5. Project name: `CinemaApp.Domain`.
    6. Click **Next**, then **Create** (ensure .NET 8 or latest is selected).

## 1.3 Add the Data Project (Class Library)
* **Create Data Project:**
    1. Right-click on Solution `CinemaApp` again.
    2. Select **Add > New Project**.
    3. Select **Class Library**.
    4. Project name: `CinemaApp.Data`.
    5. Click **Create**.

## 1.4 Add the API Project (Web API)
* **Create API Project:**
    1. Right-click on Solution `CinemaApp`.
    2. Select **Add > New Project**.
    3. Search for **ASP.NET Core Web API**.
    4. Project name: `CinemaApp.API`.
    5. Click **Next**.
    6. Settings: Ensure **Use controllers** is checked and **Enable OpenAPI support** is checked.
    7. Click **Create**.

## 1.5 Add the Client Project (Blazor)
* **Create Blazor Client Project:**
    1. Right-click on Solution `CinemaApp`.
    2. Select **Add > New Project**.
    3. Search for **Blazor WebAssembly Standalone App**.
    4. Project name: `CinemaApp.Client`.
    5. Click **Create**.

## 1.6 Set up Dependencies
Now we link them together so they can share code.

* **Link Data to Domain:**
    1. In Solution Explorer, expand `CinemaApp.Data`.
    2. Right-click **Dependencies** → **Add Project Reference**.
    3. Check the box for `CinemaApp.Domain`. Click **OK**.

* **Link API to Data & Domain:**
    1. Expand `CinemaApp.API`.
    2. Right-click **Dependencies** → **Add Project Reference**.
    3. Check `CinemaApp.Data` **and** `CinemaApp.Domain`. Click **OK**.

* **Link Client to Domain:**
    1. Expand `CinemaApp.Client`.
    2. Right-click **Dependencies** → **Add Project Reference**.
    3. Check `CinemaApp.Domain`. Click **OK**.



# 2 Database

## 2.1 Create Domain Classes
In Visual Studio Solution Explorer, expand the `CinemaApp.Domain` project.
Delete the default `Class1.cs` file (right-click → Delete).
Right-click `CinemaApp.Domain` → **Add** → **Class**. Name it `Movie.cs`.
Right-click `CinemaApp.Domain` → **Add** → **Class**. Name it `Showtime.cs`.

**File: `Movie.cs`**
```csharp
using System.Collections.Generic;

namespace CinemaApp.Domain
{
    public class Movie
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        
        // This will be handled automatically by the DbContext later
        public DateTime LastModified { get; set; }

        // Relationship: One Movie has many Showtimes
        // We initialize it to an empty list to avoid NullReferenceExceptions
        public List<Showtime> Showtimes { get; set; } = new List<Showtime>();
    }
}
```

**File: `Showtime.cs`**
```csharp
namespace CinemaApp.Domain
{
    public class Showtime
    {
        public int Id { get; set; }
        public DateTime StartTime { get; set; }
        public decimal TicketPrice { get; set; }
        
        public DateTime LastModified { get; set; }

        // Foreign Key: Links this showtime to a specific Movie ID
        public int MovieId { get; set; }

        // Navigation Property: Allows us to access the full Movie object if needed
        public Movie? Movie { get; set; }
    }
}
```
## 2.2 Add EF Core

**1. Open NuGet Package Manager for the Solution**
* Right-click on the Solution `Solution 'CinemaApp'` at the very top of Solution Explorer.
* Select **Manage NuGet Packages for Solution...**.
* Click on the **Browse** tab in the top left.

**2. Install `Microsoft.EntityFrameworkCore`**
* Search for: `Microsoft.EntityFrameworkCore.SqlServer`
* Click on it in the list.
* Check the box for **`CinemaApp.Data`** and **`CinemaApp.API`**.
* Click **Install** (and Accept any license prompts).

**3. Install `Microsoft.EntityFrameworkCore.SqlServer`**
* Search for: `Microsoft.EntityFrameworkCore.SqlServer`
* Click on it in the list.
* Check the box for **`CinemaApp.Data`** and **`CinemaApp.API`**.
* Click **Install** (and Accept any license prompts).

**4. Install `Microsoft.EntityFrameworkCore.Tools`**
* Search for: `Microsoft.EntityFrameworkCore.Tools`
* Check the box for **`CinemaApp.Data`** and **`CinemaApp.API`**.
* Click **Install**.

**5. Install `Microsoft.EntityFrameworkCore.Design`**
* Search for: `Microsoft.EntityFrameworkCore.Design`
* Check the box for **`CinemaApp.Data`** and **`CinemaApp.API`**.
* Click **Install**.


## 2.3 Add the DBContext

The `DbContext` is the bridge between your C\# code and the database. It handles saving, retrieving, and tracking changes. We will put this in the **`CinemaApp.Data`** project.

1.  In Visual Studio Solution Explorer, right-click **`CinemaApp.Data`**.
2.  Select **Add** -\> **Class**.
3.  Name it `CinemaDbContext.cs`.
4.  Click **Add**.

Paste this code into the file. This includes the logic to automatically update `LastModified` whenever you save, so you never have to think about it.

**File: `CinemaDbContext.cs`**

```csharp
using CinemaApp.Domain;
using Microsoft.EntityFrameworkCore;

namespace CinemaApp.Data
{
    public class CinemaDbContext : DbContext
    {
        // Constructor: Passes configuration (like connection strings) to the base class
        public CinemaDbContext(DbContextOptions<CinemaDbContext> options) : base(options)
        {
        }

        // These properties act as tables in your database
        public DbSet<Movie> Movies { get; set; }
        public DbSet<Showtime> Showtimes { get; set; }

        // Logic to automate LastModified
        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            foreach (var entry in ChangeTracker.Entries<Movie>())
            {
                if (entry.State == EntityState.Added || entry.State == EntityState.Modified)
                {
                    entry.Entity.LastModified = DateTime.UtcNow;
                }
            }

            foreach (var entry in ChangeTracker.Entries<Showtime>())
            {
                if (entry.State == EntityState.Added || entry.State == EntityState.Modified)
                {
                    entry.Entity.LastModified = DateTime.UtcNow;
                }
            }

            return base.SaveChangesAsync(cancellationToken);
        }
    }
}
```

You now have a fully configured Database Context with automatic auditing.


## 2.4 Connection String & Registration

We need to tell the API *where* the database is (the Connection String) and *how* to use it (Dependency Injection).

**1. Add Connection String (`appsettings.json`)**
  * Open `appsettings.json` in the **`CinemaApp.API`** project.
  * Add the `"ConnectionStrings"` section before `"Logging"`.
  * We will use **LocalDB**, which comes installed with Visual Studio, so you don't need to install a separate SQL Server.

**File: `appsettings.json`**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=CinemaDb;Trusted_Connection=True;MultipleActiveResultSets=true"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

**2. Register DbContext (`Program.cs`)**
  * Open `Program.cs` in the **`CinemaApp.API`** project.
  * Add the following code below `var builder = WebApplication.CreateBuilder(args);`.
  * **Note:** You will need to add `using CinemaApp.Data;` and `using Microsoft.EntityFrameworkCore;` at the top of the file (hover over the red squiggles to fix them).


**File: `Program.cs`**
```csharp
using CinemaApp.Data; // Add this
using Microsoft.EntityFrameworkCore; // Add this

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// --- START: Add this block ---
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<CinemaDbContext>(options =>
    options.UseSqlServer(connectionString, 
        // CRITICAL: Tell EF Core that migrations live in the Data project, not here.
        b => b.MigrationsAssembly("CinemaApp.Data")));
// --- END: Add this block ---

builder.Services.AddControllers();
// ... rest of the file
```

## 2.5 Migrations

**1. Open a Terminal**

  * Right-click on the **Solution 'CinemaApp'**.
  * Select **Open in Terminal**.
  * A Developer PowerShell window will open inside VS.

**2. Install the EF Tool**

```bash
dotnet tool install --global dotnet-ef
```

*(If it says it's already installed, that's fine).*

**3. Run the Migration Command**
```bash
dotnet ef migrations add InitialCreate --project CinemaApp.Data --startup-project CinemaApp.API
```

**4. Update the Database**

```bash
dotnet ef database update --project CinemaApp.Data --startup-project CinemaApp.API
```

After the "Build Succeeded" message, just say "Next".
Now, we have a database, but it is empty.

## 2.6 Seeding Data

We will create a helper class that inserts some sample movies (like "Dune" and "Barbie") automatically when the app starts.

**1. Create the Seeder Class**
  * In **Solution Explorer**, right-click the **`CinemaApp.Data`** project.
  * Select **Add** -\> **Class**.
  * Name it `DbSeeder.cs`.

**2. Paste the Seeder Logic**
Copy this code into `DbSeeder.cs`. It checks if the DB is empty, and if so, adds data.


**File: `DbSeeder.cs`**
```csharp
using CinemaApp.Domain;

namespace CinemaApp.Data
{
    public static class DbSeeder
    {
        public static void Seed(CinemaDbContext context)
        {
            // 1. Check if database is already populated
            if (context.Movies.Any())
            {
                return; // DB has been seeded
            }

            // 2. Create Dummy Data
            var dune = new Movie
            {
                Title = "Dune: Part Two",
                Description = "Paul Atreides unites with Chani and the Fremen.",
                Showtimes = new List<Showtime>
                {
                    new Showtime { StartTime = DateTime.UtcNow.AddDays(1).AddHours(18), TicketPrice = 14.50m },
                    new Showtime { StartTime = DateTime.UtcNow.AddDays(1).AddHours(21), TicketPrice = 14.50m }
                }
            };

            var barbie = new Movie
            {
                Title = "Barbie",
                Description = "Barbie suffers a crisis that leads her to question her world and her existence.",
                Showtimes = new List<Showtime>
                {
                    new Showtime { StartTime = DateTime.UtcNow.AddDays(2).AddHours(16), TicketPrice = 12.00m }
                }
            };

            // 3. Add to Context and Save
            context.Movies.AddRange(dune, barbie);
            context.SaveChanges();
        }
    }
}
```

**3. Run the Seeder on Startup**
  * Open `Program.cs` in the **`CinemaApp.API`** project.
  * Find the line `var app = builder.Build();`.
  * Paste this code block **immediately after** that line:

**File: `Program.cs`**
```csharp
var app = builder.Build(); // This line already exists

// --- START: Add this Seeding Block ---
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<CinemaDbContext>();
    DbSeeder.Seed(context);
}
// --- END: Seeding Block ---
```

**4. Test It**
  * Run the API project (Press **F5** or click the Green Play button).
  * You won't see the data yet (because we haven't built the API endpoints), but the code will run and populate your SQL table silently.
  * You can Close the browser window that opens.

# 3 API

## 3.1: Creating the Shared DTO Project

We will create a new library specifically for things shared between Client and Server.

**1. Create the Project**
  * Right-click on **Solution 'CinemaApp'**.
  * Select **Add** -\> **New Project**.
  * Select **Class Library**.
  * Name it: **`CinemaApp.Shared`**.
  * Click **Create**.

**2. Add References (Wiring it up)**
  * **API needs Shared:** Right-click `CinemaApp.API` -\> **Add** -\> **Project Reference** -\> Check `CinemaApp.Shared`.
  * **Client needs Shared:** Right-click `CinemaApp.Client` -\> **Add** -\> **Project Reference** -\> Check `CinemaApp.Shared`.

**3. Move the DTOs**
  * In the new **`CinemaApp.Shared`** project, create a new folder named `DTOs`.
  * Add the classes `MovieDto.cs` and `ShowtimeDto.cs` inside that folder.

**4. The Code**

**File: `ShowtimeDto.cs`**
```csharp
namespace CinemaApp.Shared.DTOs
{
    public class ShowtimeDto
    {
        public int Id { get; set; }
        public DateTime StartTime { get; set; }
        public decimal TicketPrice { get; set; }
    }
}
```

**File: `MovieDto.cs`**
```csharp
namespace CinemaApp.Shared.DTOs
{
    public class MovieDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<ShowtimeDto> Showtimes { get; set; } = new();
    }
}
```

## 3.2 Automapper Setup
Now we need to teach the API how to automatically copy data from your Database Entities (Domain) to your public DTOs (Shared).

**1. Install the AutoMapper Package**

  * Right-click **`CinemaApp.API`** -\> **Manage NuGet Packages**.
  * Search for: `AutoMapper.Extensions.Microsoft.DependencyInjection`
  * Click **Install**.

**2. Create the Mapping Profile**

  * In **`CinemaApp.API`**, create a new folder named `Mappings`.
  * Add a new class named `MappingProfile.cs`.
  * Paste this code:

**File: `MappingProfile.cs`**
```csharp
using AutoMapper;
using CinemaApp.Domain;
using CinemaApp.Shared.DTOs;

namespace CinemaApp.API.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Map Domain -> DTO
            CreateMap<Movie, MovieDto>();
            CreateMap<Showtime, ShowtimeDto>();

            // Map DTO -> Domain (for creating new items later)
            CreateMap<MovieDto, Movie>();
            CreateMap<ShowtimeDto, Showtime>();
        }
    }
}
```

**3. Register it in Program.cs**

  * Open `Program.cs` in **`CinemaApp.API`**.
  * Add this line just before `var app = builder.Build();`:

**File: `Program.cs`**
```csharp
var mapperConfig = new MapperConfiguration(mc =>
{
    mc.AddProfile(new MappingProfile());
}, NullLoggerFactory.Instance);
    
IMapper mapper = mapperConfig.CreateMapper();
builder.Services.AddSingleton(mapper);
```

## 3.3 Adding the MoviesController
We will now create the API endpoint that fetches data from the Database, converts it to DTOs using AutoMapper, and sends it to the client.

1.  In **Solution Explorer**, expand the **`CinemaApp.API`** project.
2.  Right-click the **`Controllers`** folder.
3.  Select **Add** -\> **Controller**.
4.  Select **API Controller - Empty**.
5.  Name it `MoviesController.cs`.
6.  Click **Add**.

**File: `MoviesController.cs`**
```csharp
using AutoMapper;
using CinemaApp.Data;
using CinemaApp.Domain;
using CinemaApp.Shared.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CinemaApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MoviesController : ControllerBase
    {
        private readonly CinemaDbContext _context;
        private readonly IMapper _mapper;

        // Dependency Injection: Requesting the DB and the Mapper
        public MoviesController(CinemaDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // GET: api/movies
        [HttpGet]
        public async Task<ActionResult<List<MovieDto>>> GetMovies()
        {
            // 1. Fetch from DB (Entities)
            // We use .Include() to fetch the related Showtimes
            var movies = await _context.Movies
                                       .Include(m => m.Showtimes)
                                       .ToListAsync();

            // 2. Convert to DTOs using AutoMapper
            var movieDtos = _mapper.Map<List<MovieDto>>(movies);

            // 3. Return JSON
            return Ok(movieDtos);
        }

        // GET: api/movies/5
        [HttpGet("{id}")]
        public async Task<ActionResult<MovieDto>> GetMovie(int id)
        {
            var movie = await _context.Movies
                                      .Include(m => m.Showtimes)
                                      .FirstOrDefaultAsync(m => m.Id == id);

            if (movie == null)
            {
                return NotFound();
            }

            var movieDto = _mapper.Map<MovieDto>(movie);
            
            return Ok(movieDto);
        }
    }
}
```

You now have a working read-only API!

## 3.4 Add Swagger
To add Swagger to the project we need to add its Nuget Package.

**1. Open NuGet Package Manager for the Solution**
* Right-click on the Solution `Solution 'CinemaApp'` at the very top of Solution Explorer.
* Select **Manage NuGet Packages for Solution...**.
* Click on the **Browse** tab in the top left.

**2. Install `Swashbuckle`, `Swashbuckle.AspNetCore` and `Swashbuckle.AspNetCore.Swagger`**
* Search for: `Microsoft.EntityFrameworkCore.Swashbuckle`
* Select the respective package in the list.
* Check the box for **`CinemaApp.API`**.
* Click **Install** (and Accept any license prompts).
* Do this for all 3 packages

We also need to add Swagger in our Program.cs in the CinemaApp.API

**File: `Program.cs`**

```csharp
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
```

```csharp
app.UseSwagger();
app.UseSwaggerUI();
```

We also need to add a new launchConfig in the CinameApp.API/Properties/launchSettings.json

**File: `launchSettings.json`**
```json
"CinemaApp.API": {
  "commandName": "Project",
  "dotnetRunMessages": true,
  "launchBrowser": true,
  "launchUrl": "swagger",
  "applicationUrl": "https://localhost:7153;http://localhost:5153",
  "environmentVariables": {
    "ASPNETCORE_ENVIRONMENT": "Development"
  }
}
```

Now you can select **CinemaApp.API** in the dropdown next to the green Play button to run the API. If doing so the Swagger UI will open in a browser where you can test the implemented Endpoints.


# 4 Client
In this section we will build a Blazor webpage wich fetches data from the api and displays it.

## 4.1 Enable CORS (Cross-Origin Resource Sharing)

**Why do we need to do this?**
Your API is running on `https://localhost:7000`.
Your Blazor App will run on a *different* port (like `localhost:5000`).
Browsers block this communication by default for security. We must tell the API it is okay to accept requests from outside.

**1. Open `Program.cs` in `CinemaApp.API`**
You need to add two pieces of code here.

**2. Add the Service (Before `builder.Build()`)**
Scroll up to where `builder.Services...` lines are, and add this block:

**File: `Program.cs`**
```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy => 
    {
        policy.AllowAnyOrigin()  // Allows Blazor (on any port) to connect
              .AllowAnyMethod()  // Allows GET, POST, PUT, etc.
              .AllowAnyHeader();
    });
});
```

**3. Use the Middleware (After `builder.Build()`)**
Scroll down. 
**Crucial:** Place this line **before** `app.UseAuthorization()` and `app.MapControllers()`.

**File: `Program.cs`**
```csharp
app.UseCors("AllowAll");
```



## 4.2 Configuring the Blazor Client

We need to tell the Frontend exactly where the Backend lives. By default, Blazor tries to talk to itself, so we must change that to point to your API port (`7000`).

**1. Open `Program.cs` in `CinemaApp.Client`**

  * Make sure you are in the **Client** project, not the API.

**2. Update the HttpClient**

  * Find the line starting with `builder.Services.AddScoped...`.
  * It usually looks like this:
    `sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) }`
  * **Replace** that entire line with this code:

**File: `Program.cs`**
```csharp
// Point the HttpClient to your API's specific URL
builder.Services.AddScoped(sp => 
    new HttpClient { BaseAddress = new Uri("https://localhost:7000") });
```

Your Frontend now has a direct line to the Backend.

## 4.3 The Movie Card Component

We will create a reusable UI block (a "Component") to display a single movie. This keeps our code clean and lets us display 10 or 100 movies easily by just repeating this block.

**1. Create a Components Folder**

  * In **Solution Explorer**, right-click the **`CinemaApp.Client`** project.
  * Select **Add** -\> **New Folder**.
  * Name it `Components`.

**2. Create the Razor Component**

  * Right-click the new `Components` folder.
  * Select **Add** -\> **Razor Component**.
  * Name it `MovieCard.razor`.
  * Click **Add**.

**3. The Code**
Paste this code into `MovieCard.razor`. We use standard Bootstrap classes (which come pre-installed with Blazor).

**File: `MovieCard.razor`**
```csharp
@using CinemaApp.Shared.DTOs

<div class="card h-100" style="width: 18rem;">
    <img src="https://via.placeholder.com/286x180?text=Cinema+Movie" class="card-img-top" alt="...">
    
    <div class="card-body">
        <h5 class="card-title">@Movie.Title</h5>
        <p class="card-text">@ShortDescription</p>
        
        <a href="/movie/@Movie.Id" class="btn btn-primary">
            View Showtimes
        </a>
    </div>
</div>

@code {
    // [Parameter] means "Parent, please pass this data to me!"
    [Parameter]
    public MovieDto Movie { get; set; } = new();

    // Helper to truncate long descriptions
    public string ShortDescription 
    {
        get 
        {
            if (string.IsNullOrEmpty(Movie.Description)) return "";
            if (Movie.Description.Length > 50) return Movie.Description.Substring(0, 50) + "...";
            return Movie.Description;
        }
    }
}
```


## 4.4 The Movie Grid Page

We will now modify the home page to fetch the list of movies from your API and display them in a responsive grid using the component we just made.

**1. Open `Home.razor`**
  * In **`CinemaApp.Client`**, expand the **`Pages`** folder.
  * Open **`Home.razor`**.
  * **Delete everything** currently in that file.

**2. The Code**
Paste this code in. It handles the "Loading" state (while fetching data) and then loops through the results.

**File: `Home.razor`**
```csharp
@page "/"
@using CinemaApp.Shared.DTOs
@using CinemaApp.Client.Components
@inject HttpClient Http

<PageTitle>Cinema Schedule</PageTitle>

<div class="container">
    <h1 class="my-4">Now Showing</h1>

    @if (movies == null)
    {
        <div class="alert alert-info">Loading movies...</div>
    }
    else
    {
        <div class="row">
            @foreach (var movie in movies)
            {
                <div class="col-md-4 col-sm-6 mb-4">
                    <MovieCard Movie="movie" />
                </div>
            }
        </div>
    }
</div>

@code {
    private List<MovieDto>? movies;

    protected override async Task OnInitializedAsync()
    {
        // Fetch the data from the API
        movies = await Http.GetFromJsonAsync<List<MovieDto>>("api/movies");
    }
}
```



## 4.5 Running the Full Stack

Now it's time to bring everything together.
We will run both the API and the Client at the same time so they can communicate with each other.

**1. Configure Multiple Startup Projects**
* In **Solution Explorer**, right-click the very top **Solution 'CinemaApp'**.
* Select **Properties** (it might be labeled "Set Startup Projects...").
* In the dialog that appears:
    1.  Select **Multiple startup projects**.
    2.  Find **`CinemaApp.API`**: Change Action to **Start**.
    3.  Find **`CinemaApp.Client`**: Change Action to **Start**.
    4.  (Optional) Click the "Up" arrow on `CinemaApp.API` to make sure it starts first.
* Click **OK** (or Apply).
![Screenshot 2025-11-29 144406](https://hackmd.io/_uploads/B1e2hOO_--e.png)

**2. Run the Solution**
* Press **F5** (or click the Green Play button).
* **Two** things should happen:
    1.  A console window opens (The API).
    2.  A browser window opens (The Blazor Client).

**3. Verify**
* Look at the web page.
* **Success:** You should see "Now Showing" and two nice cards for "Dune: Part Two" and "Barbie".
* **Failure:** If you see "Loading..." forever, check the browser console (F12) for red errors (usually CORS or Port mismatches).


## 4.6 The Details Page

Now we will create the page that shows the specific showtimes when a user clicks on a movie card. This connects the "View Showtimes" button we made earlier to actual data.

**1. Create the Razor Page**

  * In **`CinemaApp.Client`**, right-click the **`Pages`** folder.
  * Select **Add** -\> **Razor Component**.
  * Name it `MovieDetail.razor`.

**2. The Code**
Paste this code. Notice the `@page` directive at the top—it tells Blazor this page handles URLs like `/movie/1`.

**File: `MovieDetail.razor`**
```csharp
@page "/movie/{id:int}"
@using CinemaApp.Shared.DTOs
@inject HttpClient Http

<PageTitle>Movie Details</PageTitle>

@if (movie == null)
{
    <div class="alert alert-info">Loading details...</div>
}
else
{
    <div class="container mt-4">
        <div class="card mb-4">
            <div class="card-body">
                <h2 class="card-title">@movie.Title</h2>
                <p class="card-text text-muted">@movie.Description</p>
                <a href="/" class="btn btn-outline-secondary">← Back to List</a>
            </div>
        </div>

        <h3>Showtimes</h3>
        @if (movie.Showtimes == null || !movie.Showtimes.Any())
        {
            <p>No showtimes available.</p>
        }
        else
        {
            <table class="table table-striped table-hover">
                <thead class="table-dark">
                    <tr>
                        <th>Date</th>
                        <th>Time</th>
                        <th>Price</th>
                        <th>Action</th>
                    </tr>
                </thead>
                <tbody>
                    @foreach (var show in movie.Showtimes)
                    {
                        <tr>
                            <td>@show.StartTime.ToLocalTime().ToString("MMM dd, yyyy")</td>
                            <td>@show.StartTime.ToLocalTime().ToString("HH:mm")</td>
                            
                            <td>$@show.TicketPrice.ToString("F2")</td>
                            <td>
                                <button class="btn btn-success btn-sm">Buy Ticket</button>
                            </td>
                        </tr>
                    }
                </tbody>
            </table>
        }
    </div>
}

@code {
    [Parameter]
    public int Id { get; set; }

    private MovieDto? movie;

    protected override async Task OnInitializedAsync()
    {
        // Fetch the specific movie (including showtimes) by ID
        movie = await Http.GetFromJsonAsync<MovieDto>($"api/movies/{Id}");
    }
}
```

**3. Test the Full Flow**

  * Press **F5** to run the app again.
  * On the Home page, click the **"View Showtimes"** button on "Dune: Part Two".
  * You should go to a new page showing the description and a table with the times we seeded earlier (tomorrow's date).


You now have a fully functional public application. Users can browse movies and see schedules.

# 5 Identity

## 5.1 Installing Packages and Updating Context

We need to upgrade our database to handle Users, Passwords, and Roles. Microsoft provides a pre-built system for this called **ASP.NET Core Identity**.

**1. Install the Identity Package**
  * Right-click **`CinemaApp.Data`** -\> **Manage NuGet Packages**.
  * Search for: `Microsoft.AspNetCore.Identity.EntityFrameworkCore`.
  * Click **Install**.

**2. Update the DbContext**
  * Open `CinemaDbContext.cs` in **`CinemaApp.Data`**.
  * Change the inheritance from `DbContext` to `IdentityDbContext<IdentityUser>`.
  * *Note:* You will need to add a specific `using` statement for this to work.

**It should now look like this:**

**File: `DbContex.cs`**
```csharp
using CinemaApp.Domain;
using Microsoft.AspNetCore.Identity; // Needed for IdentityUser
using Microsoft.AspNetCore.Identity.EntityFrameworkCore; // Needed for IdentityDbContext
using Microsoft.EntityFrameworkCore;

namespace CinemaApp.Data
{
    // Change: Inherit from IdentityDbContext, not just DbContext
    public class CinemaDbContext : IdentityDbContext<IdentityUser>
    {
        ...
    }
}
```

## 5.2 Identity Migrations

We updated the C\# code to include Users (`IdentityDbContext`), so now we must update the SQL database to create the necessary tables (like `AspNetUsers`, `AspNetRoles`, etc.).

**1. Open the Terminal**

  * Right-click the **Solution 'CinemaApp'** -\> **Open in Terminal**.

**2. Create the Migration**
Run this command to create the instructions for the new tables:

```bash
dotnet ef migrations add AddIdentity --project CinemaApp.Data --startup-project CinemaApp.API
```

**3. Update the Database**
Run this command to actually create the tables in your SQL database:

```bash
dotnet ef database update --project CinemaApp.Data --startup-project CinemaApp.API
```

**4. Verify**
If you see **`Done.`**, the tables are created.

## 5.3 Configuring Identity & JWT

We need to tell the API to use the Identity system we just added to the database, and we need to set up **JWT (JSON Web Tokens)** so users can log in and get a "Key" to access the system.

**1. Install JWT Package**

  * Right-click **`CinemaApp.API`** -\> **Manage NuGet Packages**.
  * Search for: `Microsoft.AspNetCore.Authentication.JwtBearer`.
  * Click **Install**.
  * 
**2. Add Settings to `appsettings.json`**
  * Open `appsettings.json` in **`CinemaApp.API`**.
  * Add a `Jwt` section. The "Key" must be at least 32 characters long, or it will crash.


**File: `appsettings.json`**
```json
{
  "ConnectionStrings": { ... },
  "Jwt": {
    "Key": "ThisIsMySuperSecretKeyForCinemaApp2024!",
    "Issuer": "https://localhost:7153",
    "Audience": "https://localhost:7153"
  },
  "Logging": { ... }
}
```

**3. Update `Program.cs`**

  * Open `Program.cs` in **`CinemaApp.API`**.
  * You need to add the services (Identity + Auth) and the Middleware (Turn on the security check).

**Add this BLOCK before `builder.Build`:**

**File: `Program.cs`**
```csharp
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

// ... existing code ...

// 1. Register Identity (Users & Roles)
builder.Services.AddIdentity<IdentityUser, IdentityRole>()
    .AddEntityFrameworkStores<CinemaDbContext>()
    .AddDefaultTokenProviders();

// 2. Register Authentication (JWT)
var jwtSettings = builder.Configuration.GetSection("Jwt");
var key = Encoding.ASCII.GetBytes(jwtSettings["Key"]!);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(key)
    };
});
```

**Add this BLOCK after `builder.Build`, specifically after `UseCors`:**

**File: `Program.cs`**
```csharp
app.UseCors("AllowAll"); // We added this earlier

// CRITICAL: Order matters! Auth must come before Authorization.
app.UseAuthentication(); 
app.UseAuthorization();

app.MapControllers();
```


## 5.4 The Auth Controller

We need endpoints where users can send their Email/Password to either create an account or get a token.


**1. Create Auth DTOs (In Shared Project)**
The Client needs to send login data, so these DTOs must be in the **Shared** project.

  * In **`CinemaApp.Shared`**, go to the `DTOs` folder.
  * Add class `UserRegisterDto.cs`.
  * Add class `UserLoginDto.cs`.

**File: `UserRegisterDto.cs`**
```csharp
using System.ComponentModel.DataAnnotations;

namespace CinemaApp.Shared
{
    public class UserRegisterDto
    {
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required, MinLength(6)]
        public string Password { get; set; } = string.Empty;
    }
}
```

**File: `UserLoginDto.cs`**
```csharp
using System.ComponentModel.DataAnnotations;

namespace CinemaApp.Shared
{
    public class UserLoginDto
    {
        [Required]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
    }
}
```

**2. Create the Controller (In API Project)**

  * In **`CinemaApp.API`**, right-click `Controllers` -\> **Add** -\> **Controller (API Empty)**.
  * Name it `AuthController.cs`.

**3. The Controller Code**
This code takes care of the critical tasks: registering new users and generating their JWT access token.

**File: `AuthController.cs`**
```csharp
using CinemaApp.Shared;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace CinemaApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IConfiguration _configuration;

        public AuthController(UserManager<IdentityUser> userManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _configuration = configuration;
        }

        // POST: api/auth/register
        [HttpPost("register")]
        public async Task<IActionResult> Register(UserRegisterDto request)
        {
            // 1. Create the IdentityUser object
            var user = new IdentityUser { UserName = request.Email, Email = request.Email };
            
            // 2. Hash the password and save to DB
            var result = await _userManager.CreateAsync(user, request.Password);

            if (result.Succeeded)
            {
                return Ok(new { message = "Registration successful" });
            }

            return BadRequest(result.Errors);
        }

        // POST: api/auth/login
        [HttpPost("login")]
        public async Task<IActionResult> Login(UserLoginDto request)
        {
            // 1. Check if user exists
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null) return BadRequest("User not found.");

            // 2. Check password
            var isPasswordValid = await _userManager.CheckPasswordAsync(user, request.Password);
            if (!isPasswordValid) return BadRequest("Wrong password.");

            // 3. Generate JWT Token
            var token = GenerateJwtToken(user);

            return Ok(new { Token = token });
        }

        private string GenerateJwtToken(IdentityUser user)
        {
            var jwtSettings = _configuration.GetSection("Jwt");
            var key = Encoding.ASCII.GetBytes(jwtSettings["Key"]!);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.UserName!)
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddDays(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                Issuer = jwtSettings["Issuer"],
                Audience = jwtSettings["Audience"]
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
```

-----

## 5.5 Testing Auth with Swagger

Before we secure the API, we need to make sure we can actually create users and generate keys (Tokens).

**1. Run the App**

  * Press **F5** to start the project.
  * Go to the API Swagger page (e.g., `https://localhost:7153/swagger`).

**2. Register a User**

  * Expand **`POST /api/Auth/register`**.
  * Click **Try it out**.
  * In the Request Body, paste this JSON (Identity requires a strong password by default: Uppercase, Lowercase, Number, Symbol):
    ```json
    {
      "email": "admin@cinema.com",
      "password": "Password123!"
    }
    ```
  * Click **Execute**.
  * **Success:** You should see Code 200 and `{"message":"Registration successful"}`.

**3. Login**

  * Expand **`POST /api/Auth/login`**.
  * Click **Try it out**.
  * Paste the same JSON credentials.
  * Click **Execute**.

**4. The Result**
Look at the **Response body**. You will see a long string of random characters. **This is your JWT (Access Token).**

```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc..."
}
```

## 5.6 Securing the Endpoints

**1. Protect the Controller (`MoviesController.cs`)**

  * Open `MoviesController.cs` in **`CinemaApp.API`**.
  * Add `using Microsoft.AspNetCore.Authorization;` at the top.
  * Add this **POST** endpoint inside the class. Notice the `[Authorize]` attribute.

**File: `MoviesController.cs`**
```csharp
// POST: api/movies
[HttpPost]
[Authorize] // <--- This locks the door!
public async Task<ActionResult<MovieDto>> CreateMovie(MovieDto movieDto)
{
    // 1. Map DTO -> Domain Entity
    var movie = _mapper.Map<Movie>(movieDto);

    // 2. Add to DB
    _context.Movies.Add(movie);
    await _context.SaveChangesAsync();

    // 3. Map back to DTO for response
    var returnDto = _mapper.Map<MovieDto>(movie);

    return CreatedAtAction(nameof(GetMovie), new { id = returnDto.Id }, returnDto);
}
```

**2. Test the Security**

  * Run the app (**F5**).
  * **Test:** Try to use `POST /api/movies` *without* doing anything else. You should get **401 Unauthorized**.

## 5.7 Add Admin Page

Let's build a simple **Admin Page** in Blazor. This page will handle two things:

1.  **Login:** To get the Token.
2.  **Create Movie:** To send the data **with** the Token attached.


**1. Create the Admin Page**

  * In **`CinemaApp.Client`**, right-click **`Pages`** -\> **Add** -\> **Razor Component**.
  * Name it: `Admin.razor`.

**2. The Code**

**File: `Admin.razor`**
```csharp
@page "/admin"
@using CinemaApp.Shared
@using System.Net.Http.Headers
@using System.Text.Json
@inject HttpClient Http

<PageTitle>Admin Dashboard</PageTitle>

<div class="container mt-4">
    <h1>Admin Dashboard</h1>

    @if (string.IsNullOrEmpty(jwtToken))
    {
        <div class="card p-4" style="max-width: 400px;">
            <h3>Login</h3>
            <div class="mb-3">
                <label>Email</label>
                <input @bind="loginModel.Email" class="form-control" placeholder="admin@cinema.com" />
            </div>
            <div class="mb-3">
                <label>Password</label>
                <input @bind="loginModel.Password" type="password" class="form-control" placeholder="Password123!" />
            </div>
            <button class="btn btn-primary" @onclick="Login">Login</button>
            <p class="text-danger mt-2">@loginMessage</p>
        </div>
    }
    else
    {
        <div class="card p-4">
            <h3>Add New Movie</h3>
            <div class="mb-3">
                <label>Title</label>
                <input @bind="newMovie.Title" class="form-control" />
            </div>
            <div class="mb-3">
                <label>Description</label>
                <textarea @bind="newMovie.Description" class="form-control"></textarea>
            </div>
            
            <button class="btn btn-success" @onclick="CreateMovie">Create Movie</button>
            <p class="mt-2">@createMessage</p>
        </div>
    }
</div>

@code {
    // State
    private string jwtToken = "";
    private string loginMessage = "";
    private string createMessage = "";

    // Models
    private UserLoginDto loginModel = new();
    private MovieDto newMovie = new();

    private async Task Login()
    {
        var response = await Http.PostAsJsonAsync("api/Auth/login", loginModel);
        
        if (response.IsSuccessStatusCode)
        {
            // Extract the token from the JSON response
            var result = await response.Content.ReadFromJsonAsync<JsonElement>();
            jwtToken = result.GetProperty("token").GetString()!;
            loginMessage = "";
        }
        else
        {
            loginMessage = "Login failed. Check credentials.";
        }
    }

    private async Task CreateMovie()
    {
        // 1. Create the Request manually
        var request = new HttpRequestMessage(HttpMethod.Post, "api/Movies");
        
        // 2. Attach the Body (The Movie Data)
        request.Content = JsonContent.Create(newMovie);

        // 3. ATTACH THE HEADER (This is what Swagger was failing to do)
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);

        // 4. Send
        var response = await Http.SendAsync(request);

        if (response.IsSuccessStatusCode)
        {
            createMessage = "Success! Movie created.";
            newMovie = new MovieDto(); // Reset form
        }
        else
        {
            createMessage = $"Error: {response.StatusCode}";
        }
    }
}
```

**3. Test it**

1.  Run the app (**F5**).
2.  Navigate to `/admin`.
3.  **Login** with your admin user (`admin@cinema.com` / `Password123!`).
      * *The UI should switch to the "Add New Movie" form.*
4.  Enter a Title (e.g., "The Dark Knight") and Description.
5.  Click **Create Movie**.

**Did you get the "Success\! Movie created" message?**
If yes, check the "Home" page, and you should see your new movie there\!



## 5.8 Linking the Admin Page and add Showtime Input
Now we want to link the Admin Page on the Sidebar and also add Showtimes

**Step 1: Add the Link to the Navigation Menu**

Open **`Shared/NavMenu.razor`** in the **Client** project.

Add this block inside the `<nav>` section (usually at the bottom of the list):

**File: `NavMenu.razor`**
```razor
<div class="nav-item px-3">
    <NavLink class="nav-link" href="admin">
        <span class="bi bi-lock-fill" aria-hidden="true"></span> Admin
    </NavLink>
</div>
```

-----

**Step 2: Update the Admin Page (With Showtimes)**

We need to upgrade `Admin.razor`. We will add a "Mini Form" inside the main form.

1.  **User enters Movie info** (Title/Description).
2.  **User adds Showtimes** (Date/Price) -\> clicks "Add Time". (This adds it to a temporary list).
3.  **User clicks "Create Movie"** -\> Sends the Movie **plus** all the Showtimes to the API in one go.

**Replace the entire content of `Admin.razor` with the following:**

```csharp
@page "/admin"
@using CinemaApp.Shared
@using System.Net.Http.Headers
@using System.Text.Json
@inject HttpClient Http

<PageTitle>Admin Dashboard</PageTitle>

<div class="container mt-4">
    <h1>Admin Dashboard</h1>

    @if (string.IsNullOrEmpty(jwtToken))
    {
        <div class="card p-4 shadow-sm" style="max-width: 400px;">
            <h3>Login</h3>
            <div class="mb-3">
                <label>Email</label>
                <input @bind="loginModel.Email" class="form-control" placeholder="admin@cinema.com" />
            </div>
            <div class="mb-3">
                <label>Password</label>
                <input @bind="loginModel.Password" type="password" class="form-control" placeholder="Password123!" />
            </div>
            <button class="btn btn-primary w-100" @onclick="Login">Login</button>
            <p class="text-danger mt-2">@loginMessage</p>
        </div>
    }
    else
    {
        <div class="card p-4 shadow-sm">
            <h3>Add New Movie</h3>
            
            <div class="row">
                <div class="col-md-6">
                    <div class="mb-3">
                        <label class="form-label">Title</label>
                        <input @bind="newMovie.Title" class="form-control" placeholder="e.g. Inception" />
                    </div>
                    <div class="mb-3">
                        <label class="form-label">Description</label>
                        <textarea @bind="newMovie.Description" class="form-control" rows="3"></textarea>
                    </div>
                </div>

                <div class="col-md-6">
                    <div class="card bg-light p-3">
                        <h5>Add Showtimes</h5>
                        <div class="d-flex gap-2 mb-2">
                            <input @bind="tempShowtime.StartTime" type="datetime-local" class="form-control" />
                            <input @bind="tempShowtime.TicketPrice" type="number" class="form-control" placeholder="Price" style="width: 100px;" />
                            <button class="btn btn-secondary" @onclick="AddShowtimeToList">Add</button>
                        </div>

                        <ul class="list-group">
                            @foreach (var show in newMovie.Showtimes)
                            {
                                <li class="list-group-item d-flex justify-content-between align-items-center">
                                    <span>@show.StartTime.ToString("g") - $@show.TicketPrice</span>
                                    <button class="btn btn-sm btn-danger" @onclick="() => newMovie.Showtimes.Remove(show)">X</button>
                                </li>
                            }
                            @if (newMovie.Showtimes.Count == 0)
                            {
                                <li class="list-group-item text-muted">No showtimes added yet.</li>
                            }
                        </ul>
                    </div>
                </div>
            </div>

            <hr />
            <button class="btn btn-success btn-lg" @onclick="CreateMovie">Save Movie & Showtimes</button>
            <p class="mt-2 fw-bold @(isSuccess ? "text-success" : "text-danger")">@createMessage</p>
        </div>
    }
</div>

@code {
    // State
    private string jwtToken = "";
    private string loginMessage = "";
    private string createMessage = "";
    private bool isSuccess = false;

    // Models
    private UserLoginDto loginModel = new();
    private MovieDto newMovie = new();
    
    // Temporary object for the inputs
    private ShowtimeDto tempShowtime = new() { StartTime = DateTime.Now, TicketPrice = 12 };

    private async Task Login()
    {
        var response = await Http.PostAsJsonAsync("api/Auth/login", loginModel);
        
        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<JsonElement>();
            jwtToken = result.GetProperty("token").GetString()!;
            loginMessage = "";
        }
        else
        {
            loginMessage = "Login failed. Check credentials.";
        }
    }

    private void AddShowtimeToList()
    {
        // Add a COPY of the temp object to the list
        newMovie.Showtimes.Add(new ShowtimeDto 
        { 
            StartTime = tempShowtime.StartTime, 
            TicketPrice = tempShowtime.TicketPrice 
        });
    }

    private async Task CreateMovie()
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "api/Movies");
        request.Content = JsonContent.Create(newMovie);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);

        var response = await Http.SendAsync(request);

        if (response.IsSuccessStatusCode)
        {
            isSuccess = true;
            createMessage = "Success! Movie and Showtimes created.";
            newMovie = new MovieDto(); // Reset form
            newMovie.Showtimes = new List<ShowtimeDto>(); // Reset list
        }
        else
        {
            isSuccess = false;
            createMessage = $"Error: {response.StatusCode}";
        }
    }
}
```

How to Test This Final Flow:

1.  Run the app.
2.  Click **Admin** in the side menu.
3.  Login (`admin@cinema.com` / `Password123!`).
4.  Enter a Title ("Interstellar").
5.  On the right side, pick a date and price, then click **Add**. Do this 2 or 3 times.
6.  Click **Save Movie & Showtimes**.
7.  Go back to **Home**.
8.  Click **View Showtimes** on your new card. You should see all the times you just added\!

