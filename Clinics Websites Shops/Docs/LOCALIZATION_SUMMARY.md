# Multi-Language Localization Implementation Summary

## What Was Implemented

A complete multi-language localization system for Department names with URL-based culture selection.

## Files Created/Modified

### New Files Created:
1. **Models/DepartmentTranslation.cs** - Translation entity for storing department names in multiple languages
2. **Services/IServices/ILocalizationService.cs** - Interface for localization service
3. **Services/LocalizationService.cs** - Service to retrieve localized department names
4. **Middlewares/RouteDataRequestCultureProvider.cs** - Custom culture provider for URL-based culture selection
5. **LOCALIZATION_USAGE.md** - Comprehensive usage guide and examples

### Modified Files:
1. **Models/Department.cs** - Added `Translations` collection property
2. **DataAccess/ApplicationDbContext.cs** - Added `DepartmentTranslations` DbSet and configured relationships
3. **Program.cs** - Configured localization with URL route support and registered LocalizationService

### Database Migration:
- **AddDepartmentTranslations** - Creates `DepartmentTranslations` table with proper relationships

## Key Features

### 1. URL-Based Culture Selection
Users can access different languages via URL:
- English: `/en-US/Customer/Home/Index`
- Arabic: `/ar-EG/Customer/Home/Index`
- German: `/de-DE/Customer/Home/Index`
- Default: `/Customer/Home/Index` (uses en-US)

### 2. Database Schema
```
Department
├── Id (PK)
├── Name (default/fallback)
├── TenantId
└── Translations (Collection)
    └── DepartmentTranslation
        ├── Id (PK)
        ├── DepartmentId (FK)
        ├── LanguageCode (e.g., "en-US", "ar-EG", "de-DE")
        └── Name (translated name)
```

### 3. Localization Service
```csharp
// Inject the service
private readonly ILocalizationService _localizationService;

// Get localized name based on current culture
var localizedName = _localizationService.GetLocalizedDepartmentName(department);

// Get localized name for specific culture
var arabicName = _localizationService.GetLocalizedDepartmentName(department, "ar-EG");
```

### 4. Supported Cultures
- **en-US** - English (United States)
- **ar-EG** - Arabic (Egypt)
- **de-DE** - German (Germany)

## How to Use

### Step 1: Apply Database Migration
```bash
cd "Clinics Websites Shops"
dotnet ef database update --context ApplicationDbContext
```

### Step 2: Add Translations to Departments
```csharp
var department = new Department
{
    Name = "Cardiology", // Default name
    TenantId = "clinic1",
    Translations = new List<DepartmentTranslation>
    {
        new() { LanguageCode = "en-US", Name = "Cardiology" },
        new() { LanguageCode = "ar-EG", Name = "أمراض القلب" },
        new() { LanguageCode = "de-DE", Name = "Kardiologie" }
    }
};
```

### Step 3: Retrieve Localized Names
```csharp
// In Controller
public class DepartmentController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ILocalizationService _localizationService;

    public async Task<IActionResult> Index()
    {
        var departments = await _context.Departments
            .Include(d => d.Translations)
            .ToListAsync();

        var viewModels = departments.Select(d => new
        {
            Id = d.Id,
            Name = _localizationService.GetLocalizedDepartmentName(d)
        }).ToList();

        return View(viewModels);
    }
}
```

### Step 4: Use in Views
```cshtml
@inject ILocalizationService LocalizationService

@foreach (var department in Model)
{
    <div>@LocalizationService.GetLocalizedDepartmentName(department)</div>
}
```

### Step 5: Create Language Switcher
```cshtml
<a asp-route-culture="en-US">English</a>
<a asp-route-culture="ar-EG">العربية</a>
<a asp-route-culture="de-DE">Deutsch</a>
```

## Architecture Overview

```
Request with culture in URL (e.g., /ar-EG/Customer/Home/Index)
    ↓
RouteDataRequestCultureProvider extracts culture from route
    ↓
RequestLocalizationMiddleware sets CurrentCulture
    ↓
LocalizationService.GetLocalizedDepartmentName()
    ↓
Queries DepartmentTranslation table for matching culture
    ↓
Returns translated name or falls back to Department.Name
```

## Configuration Details

### Program.cs Configuration:
1. **Localization Service Registration**: `builder.Services.AddScoped<ILocalizationService, LocalizationService>()`
2. **Supported Cultures**: en-US, ar-EG, de-DE
3. **Route Culture Provider**: Custom `RouteDataRequestCultureProvider` for URL-based culture
4. **Route Configuration**: 
   - Localized route: `{culture}/{area}/{controller}/{action}/{id?}`
   - Default route: `{area}/{controller}/{action}/{id?}`

### Database Configuration:
- Unique index on `(DepartmentId, LanguageCode)` to prevent duplicate translations
- Cascade delete: When department is deleted, translations are automatically deleted
- Required fields: `LanguageCode` (max 10 chars), `Name` (max 200 chars)

## Benefits

1. **SEO-Friendly**: Culture in URL helps with search engine optimization
2. **User-Friendly**: Users can bookmark language-specific URLs
3. **Scalable**: Easy to add more languages by adding new translations
4. **Fallback Support**: Always shows default name if translation missing
5. **Type-Safe**: Uses strongly-typed service instead of magic strings
6. **Tenant-Aware**: Works seamlessly with multi-tenant architecture

## Next Steps

1. **Apply Migration**: Run `dotnet ef database update`
2. **Seed Data**: Add translations for existing departments
3. **Update Views**: Use `ILocalizationService` in department-related views
4. **Add Language Switcher**: Implement UI for users to change language
5. **Extend to Other Entities**: Apply same pattern to other entities (Doctor specializations, etc.)

## Testing

### Test URLs:
- `https://clinic1.localhost/en-US/Customer/Home/Index`
- `https://clinic1.localhost/ar-EG/Customer/Home/Index`
- `https://clinic1.localhost/de-DE/Customer/Home/Index`

### Verify:
1. Culture is correctly extracted from URL
2. Department names display in correct language
3. Fallback works when translation is missing
4. Language switcher changes URL and content

## Support

For detailed usage examples and troubleshooting, see `LOCALIZATION_USAGE.md`.

