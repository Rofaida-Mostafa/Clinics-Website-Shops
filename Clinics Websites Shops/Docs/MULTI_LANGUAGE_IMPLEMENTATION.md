# Multi-Language Department Implementation Guide

This implementation provides a complete multi-language solution for managing departments with dynamic language support.

## Features Implemented

### 1. **Dynamic Multi-Language Support**
- Departments can be saved with translations in multiple languages (English, Arabic, and easily extensible)
- Translations are stored in the `DepartmentTranslation` table with language codes
- Views dynamically display forms for all supported languages

### 2. **Localized Controller Messages**
- All controller response messages are now localized using `IStringLocalizer`
- Success/error messages appear in the user's current language
- Resource files (`en.json`, `ar.json`) contain all translatable strings

### 3. **Enhanced ViewModels**
- `CreateDepartmentViewModel`: Handles creation with multi-language input
- `EditDepartmentViewModel`: Handles editing with existing translations
- `DepartmentListViewModel`: Displays localized department information
- `DepartmentTranslationViewModel`: Manages individual language translations

### 4. **Improved Services**
- `LocalizationService`: Enhanced with department-specific localization methods
- Supports getting supported languages dynamically
- Handles both name and description localization

### 5. **Extension Methods**
- `DepartmentExtensions`: Provides easy conversion between entities and ViewModels
- Includes methods for getting localized content
- Simplifies working with multi-language data

## How to Use

### 1. **Creating a Department**
```csharp
// The Create GET action automatically initializes forms for all supported languages
public IActionResult Create()
{
    var supportedLanguages = _localizationService.GetSupportedLanguages();
    var viewModel = new CreateDepartmentViewModel();
    
    // Initialize translation forms
    foreach (var language in supportedLanguages)
    {
        viewModel.Translations.Add(new DepartmentTranslationViewModel
        {
            LanguageCode = language.Key,
            LanguageName = language.Value,
            Name = string.Empty,
            Description = string.Empty
        });
    }

    return View(viewModel);
}
```

### 2. **Displaying Localized Departments**
```csharp
// The Index action returns departments with localized content based on current culture
public async Task<IActionResult> Index()
{
    var departments = await _departmentRepository.GetAsync(
        includes: new Expression<Func<Department, object>>[] { d => d.Translations },
        sort: "DESC"
    );

    // Convert to localized ViewModels
    var departmentViewModels = departments
        .Select(d => d.ToListViewModel(_localizationService))
        .ToList();

    return View(departmentViewModels);
}
```

### 3. **Using Extension Methods**
```csharp
// Get localized name for current culture
var localizedName = department.GetLocalizedName();

// Get localized name for specific culture
var arabicName = department.GetLocalizedName("ar");

// Check if translation exists
if (department.HasTranslation("ar"))
{
    // Arabic translation is available
}

// Convert entity to ViewModel
var viewModel = department.ToEditViewModel(supportedLanguages);
```

### 4. **Adding New Languages**
To add support for new languages:

1. Update the `LocalizationService`:
```csharp
private readonly Dictionary<string, string> _supportedLanguages = new()
{
    { "en", "English" },
    { "ar", "العربية" },
    { "fr", "Français" }, // Add new language
    { "de", "Deutsch" }   // Add another language
};
```

2. Create corresponding resource files:
- `Resources/fr.json`
- `Resources/de.json`

3. No code changes needed - the system will automatically generate forms for new languages!

## View Implementation

### Example Create Form (CreateExample.cshtml)
The create form automatically generates input fields for all supported languages:

```html
@for (int i = 0; i < Model.Translations.Count; i++)
{
    <div class="translation-group mb-4 p-3 border rounded">
        <h6 class="text-primary">@Model.Translations[i].LanguageName</h6>
        
        <input asp-for="Translations[i].LanguageCode" type="hidden" />
        <input asp-for="Translations[i].LanguageName" type="hidden" />
        
        <div class="form-group">
            <label asp-for="Translations[i].Name">Name</label>
            <input asp-for="Translations[i].Name" class="form-control" />
        </div>

        <div class="form-group">
            <label asp-for="Translations[i].Description">Description</label>
            <textarea asp-for="Translations[i].Description" class="form-control"></textarea>
        </div>
    </div>
}
```

### Example Index Display (IndexExample.cshtml)
The index view shows localized content and indicates available languages:

```html
@foreach (var department in Model)
{
    <tr>
        <td>@department.Name</td> <!-- Automatically localized -->
        <td>@department.Description</td> <!-- Automatically localized -->
        <td>
            @if (department.OriginalDepartment.Translations.Any())
            {
                <div class="small text-muted">
                    <i class="fas fa-language"></i> 
                    @string.Join(", ", department.OriginalDepartment.Translations.Select(t => t.LanguageCode.ToUpper()))
                </div>
            }
        </td>
    </tr>
}
```

## Database Structure

### Department Table
- `Id`: Primary key
- `Name`: Default/fallback name
- `Description`: Default/fallback description
- `MainImg`: Image path
- `Status`: Active/inactive
- `TenantId`: Multi-tenant support

### DepartmentTranslation Table
- `Id`: Primary key
- `DepartmentId`: Foreign key to Department
- `LanguageCode`: Language code (e.g., "en", "ar", "fr")
- `Name`: Translated name
- `Description`: Translated description

## Benefits

1. **Scalability**: Easy to add new languages without code changes
2. **Performance**: Efficient querying with proper includes
3. **User Experience**: Dynamic forms that adapt to supported languages
4. **Maintainability**: Clean separation of concerns with ViewModels and services
5. **Localization**: Complete localization of all user-facing messages
6. **Flexibility**: Fallback to default language when translation is not available

## Migration Notes

If you have existing departments without translations:
1. Existing departments will continue to work with their default names/descriptions
2. You can gradually add translations through the edit interface
3. The system gracefully handles missing translations by falling back to defaults

This implementation provides a robust, scalable solution for multi-language department management that can easily be extended to other entities in your system.