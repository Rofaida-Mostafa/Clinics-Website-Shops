# Department Multi-Language Localization Guide

## Overview
This guide explains how to use the multi-language localization system for Department names in the Clinics Website Shops application.

## Features
- **URL-based culture selection**: Access different languages via URL (e.g., `/en-US/`, `/ar-EG/`, `/de-DE/`)
- **Database-driven translations**: Department names stored in `DepartmentTranslations` table
- **Fallback support**: Falls back to default name if translation not found
- **Supported cultures**: 
  - `en-US` (English - United States)
  - `ar-EG` (Arabic - Egypt)
  - `de-DE` (German - Germany)

## Database Schema

### Department Table
```sql
- Id (int, PK)
- Name (string) -- Default/fallback name
- TenantId (string)
```

### DepartmentTranslation Table
```sql
- Id (int, PK)
- DepartmentId (int, FK)
- LanguageCode (string) -- e.g., "en-US", "ar-EG", "de-DE"
- Name (string) -- Translated name
```

## Usage Examples

### 1. Adding Translations to a Department

```csharp
// In your controller or service
var department = new Department
{
    Name = "Cardiology", // Default name
    TenantId = "clinic1"
};

// Add translations
department.Translations = new List<DepartmentTranslation>
{
    new DepartmentTranslation
    {
        LanguageCode = "en-US",
        Name = "Cardiology"
    },
    new DepartmentTranslation
    {
        LanguageCode = "ar-EG",
        Name = "أمراض القلب"
    },
    new DepartmentTranslation
    {
        LanguageCode = "de-DE",
        Name = "Kardiologie"
    }
};

_context.Departments.Add(department);
await _context.SaveChangesAsync();
```

### 2. Retrieving Localized Department Names

#### Using the LocalizationService (Recommended)

```csharp
public class DepartmentController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ILocalizationService _localizationService;

    public DepartmentController(
        ApplicationDbContext context,
        ILocalizationService localizationService)
    {
        _context = context;
        _localizationService = localizationService;
    }

    public async Task<IActionResult> Index()
    {
        // Load departments with translations
        var departments = await _context.Departments
            .Include(d => d.Translations)
            .ToListAsync();

        // Get localized names based on current culture
        var departmentViewModels = departments.Select(d => new
        {
            Id = d.Id,
            LocalizedName = _localizationService.GetLocalizedDepartmentName(d)
        }).ToList();

        return View(departmentViewModels);
    }

    public async Task<IActionResult> Details(int id)
    {
        var department = await _context.Departments
            .Include(d => d.Translations)
            .FirstOrDefaultAsync(d => d.Id == id);

        if (department == null)
            return NotFound();

        // Get localized name for specific culture
        var englishName = _localizationService.GetLocalizedDepartmentName(department, "en-US");
        var arabicName = _localizationService.GetLocalizedDepartmentName(department, "ar-EG");
        var germanName = _localizationService.GetLocalizedDepartmentName(department, "de-DE");

        ViewBag.EnglishName = englishName;
        ViewBag.ArabicName = arabicName;
        ViewBag.GermanName = germanName;

        return View(department);
    }
}
```

#### Using LINQ Directly

```csharp
public async Task<IActionResult> Index()
{
    var currentCulture = CultureInfo.CurrentCulture.Name;

    var departments = await _context.Departments
        .Include(d => d.Translations)
        .Select(d => new
        {
            Id = d.Id,
            LocalizedName = d.Translations
                .FirstOrDefault(t => t.LanguageCode == currentCulture).Name 
                ?? d.Name // Fallback to default name
        })
        .ToListAsync();

    return View(departments);
}
```

### 3. URL Routing Examples

The application supports culture-specific URLs:

- **English**: `https://clinic1.localhost/en-US/Customer/Home/Index`
- **Arabic**: `https://clinic1.localhost/ar-EG/Customer/Home/Index`
- **German**: `https://clinic1.localhost/de-DE/Customer/Home/Index`
- **Default** (no culture): `https://clinic1.localhost/Customer/Home/Index` (uses default culture: en-US)

### 4. Creating Links with Culture

In your Razor views:

```cshtml
@using System.Globalization

@{
    var currentCulture = CultureInfo.CurrentCulture.Name;
}

<!-- Link with current culture -->
<a asp-area="Customer" 
   asp-controller="Department" 
   asp-action="Index" 
   asp-route-culture="@currentCulture">
    Departments
</a>

<!-- Language switcher -->
<div class="language-switcher">
    <a asp-area="Customer" 
       asp-controller="Home" 
       asp-action="Index" 
       asp-route-culture="en-US">
        English
    </a>
    <a asp-area="Customer" 
       asp-controller="Home" 
       asp-action="Index" 
       asp-route-culture="ar-EG">
        العربية
    </a>
    <a asp-area="Customer" 
       asp-controller="Home" 
       asp-action="Index" 
       asp-route-culture="de-DE">
        Deutsch
    </a>
</div>
```

### 5. Displaying Localized Department Names in Views

```cshtml
@model IEnumerable<Department>
@inject ILocalizationService LocalizationService

<h2>Departments</h2>

<table class="table">
    <thead>
        <tr>
            <th>ID</th>
            <th>Name</th>
        </tr>
    </thead>
    <tbody>
        @foreach (var department in Model)
        {
            <tr>
                <td>@department.Id</td>
                <td>@LocalizationService.GetLocalizedDepartmentName(department)</td>
            </tr>
        }
    </tbody>
</table>
```

## Migration

To apply the database changes:

```bash
# Apply migration to update database
dotnet ef database update --context ApplicationDbContext
```

## Sample Data Seeding

```csharp
// In your DbContext or seed method
public static void SeedDepartments(ApplicationDbContext context)
{
    if (!context.Departments.Any())
    {
        var departments = new[]
        {
            new Department
            {
                Name = "Cardiology",
                TenantId = "clinic1",
                Translations = new List<DepartmentTranslation>
                {
                    new() { LanguageCode = "en-US", Name = "Cardiology" },
                    new() { LanguageCode = "ar-EG", Name = "أمراض القلب" },
                    new() { LanguageCode = "de-DE", Name = "Kardiologie" }
                }
            },
            new Department
            {
                Name = "Neurology",
                TenantId = "clinic1",
                Translations = new List<DepartmentTranslation>
                {
                    new() { LanguageCode = "en-US", Name = "Neurology" },
                    new() { LanguageCode = "ar-EG", Name = "طب الأعصاب" },
                    new() { LanguageCode = "de-DE", Name = "Neurologie" }
                }
            },
            new Department
            {
                Name = "Pediatrics",
                TenantId = "clinic1",
                Translations = new List<DepartmentTranslation>
                {
                    new() { LanguageCode = "en-US", Name = "Pediatrics" },
                    new() { LanguageCode = "ar-EG", Name = "طب الأطفال" },
                    new() { LanguageCode = "de-DE", Name = "Pädiatrie" }
                }
            }
        };

        context.Departments.AddRange(departments);
        context.SaveChanges();
    }
}
```

## API Endpoints

If you're building an API, you can return localized department names:

```csharp
[ApiController]
[Route("api/[controller]")]
public class DepartmentsApiController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILocalizationService _localizationService;

    public DepartmentsApiController(
        ApplicationDbContext context,
        ILocalizationService localizationService)
    {
        _context = context;
        _localizationService = localizationService;
    }

    [HttpGet]
    public async Task<IActionResult> GetDepartments([FromQuery] string? culture = null)
    {
        var departments = await _context.Departments
            .Include(d => d.Translations)
            .ToListAsync();

        var result = departments.Select(d => new
        {
            id = d.Id,
            name = culture != null 
                ? _localizationService.GetLocalizedDepartmentName(d, culture)
                : _localizationService.GetLocalizedDepartmentName(d)
        });

        return Ok(result);
    }
}
```

## Best Practices

1. **Always include translations**: When creating a new department, add translations for all supported cultures
2. **Use the service**: Use `ILocalizationService` instead of manually querying translations
3. **Include translations in queries**: Use `.Include(d => d.Translations)` when loading departments
4. **Provide fallbacks**: The default `Name` property serves as a fallback if translation is missing
5. **Consistent culture codes**: Always use the full culture code (e.g., "en-US", not "en")

## Troubleshooting

### Translation not showing
- Ensure you've included translations in your query: `.Include(d => d.Translations)`
- Check that the translation exists in the database for the current culture
- Verify the culture code matches exactly (case-sensitive)

### URL culture not working
- Ensure the culture parameter is in the route
- Check that the culture is in the supported cultures list
- Verify the RouteDataRequestCultureProvider is registered in Program.cs

