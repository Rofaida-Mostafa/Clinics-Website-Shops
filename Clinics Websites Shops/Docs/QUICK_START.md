# Quick Start Guide - Department Localization

## 🚀 Quick Setup (3 Steps)

### Step 1: Apply Database Migration
```bash
cd "Clinics Websites Shops"
dotnet ef database update --context ApplicationDbContext
```

### Step 2: Add Sample Data
```csharp
// In your controller or seed method
var department = new Department
{
    Name = "Cardiology",
    TenantId = "clinic1"
};

department.SetTranslation("en-US", "Cardiology");
department.SetTranslation("ar-EG", "أمراض القلب");
department.SetTranslation("de-DE", "Kardiologie");

_context.Departments.Add(department);
await _context.SaveChangesAsync();
```

### Step 3: Use in Your Code
```csharp
// Option 1: Using Extension Method (Simplest)
var localizedName = department.GetLocalizedName();

// Option 2: Using Service (Recommended for controllers)
var localizedName = _localizationService.GetLocalizedDepartmentName(department);

// Option 3: Using LINQ
var departments = await _context.Departments
    .Include(d => d.Translations)
    .Select(d => new {
        Id = d.Id,
        Name = d.GetLocalizedName()
    })
    .ToListAsync();
```

## 📝 Common Usage Patterns

### In Controllers
```csharp
public class DepartmentController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ILocalizationService _localizationService;

    public async Task<IActionResult> Index()
    {
        var departments = await _context.Departments
            .Include(d => d.Translations)
            .ToListAsync();

        return View(departments);
    }
}
```

### In Views
```cshtml
@model IEnumerable<Department>
@using Clinics_Websites_Shops.Extensions

@foreach (var dept in Model)
{
    <div>@dept.GetLocalizedName()</div>
}
```

### In Razor Pages
```cshtml
@inject ILocalizationService LocalizationService

<h2>@LocalizationService.GetLocalizedDepartmentName(Model.Department)</h2>
```

## 🌐 URL Examples

Access different languages via URL:
- English: `/en-US/Customer/Home/Index`
- Arabic: `/ar-EG/Customer/Home/Index`
- German: `/de-DE/Customer/Home/Index`

## 🔗 Language Switcher

```cshtml
<div class="language-switcher">
    <a asp-route-culture="en-US">English</a>
    <a asp-route-culture="ar-EG">العربية</a>
    <a asp-route-culture="de-DE">Deutsch</a>
</div>
```

## 📚 Available Methods

### Extension Methods (DepartmentExtensions)
```csharp
// Get localized name (current culture)
string name = department.GetLocalizedName();

// Get localized name (specific culture)
string name = department.GetLocalizedName("ar-EG");

// Check if translation exists
bool exists = department.HasTranslation("de-DE");

// Get all translations
Dictionary<string, string> all = department.GetAllTranslations();

// Add/Update translation
department.SetTranslation("en-US", "Cardiology");

// Remove translation
bool removed = department.RemoveTranslation("de-DE");
```

### Service Methods (ILocalizationService)
```csharp
// Get localized name (current culture)
string name = _localizationService.GetLocalizedDepartmentName(department);

// Get localized name (specific culture)
string name = _localizationService.GetLocalizedDepartmentName(department, "ar-EG");

// Get current culture
string culture = _localizationService.GetCurrentCulture();
```

## 🎯 Best Practices

1. **Always include translations** when loading departments:
   ```csharp
   .Include(d => d.Translations)
   ```

2. **Use extension methods** for simple scenarios:
   ```csharp
   department.GetLocalizedName()
   ```

3. **Use the service** in controllers:
   ```csharp
   _localizationService.GetLocalizedDepartmentName(department)
   ```

4. **Provide fallback** - The default `Name` property is used if translation is missing

## 🔍 Troubleshooting

**Translation not showing?**
- ✅ Check: Did you include translations? `.Include(d => d.Translations)`
- ✅ Check: Does the translation exist in the database?
- ✅ Check: Is the culture code correct? (e.g., "en-US" not "en")

**URL culture not working?**
- ✅ Check: Is the culture in the URL? `/en-US/...`
- ✅ Check: Is it a supported culture? (en-US, ar-EG, de-DE)

## 📖 Full Documentation

For detailed examples and advanced usage, see:
- `LOCALIZATION_USAGE.md` - Comprehensive usage guide
- `LOCALIZATION_SUMMARY.md` - Implementation summary

## 🎨 Sample Department Names

| English | Arabic | German |
|---------|--------|--------|
| Cardiology | أمراض القلب | Kardiologie |
| Neurology | طب الأعصاب | Neurologie |
| Pediatrics | طب الأطفال | Pädiatrie |
| Orthopedics | جراحة العظام | Orthopädie |
| Dermatology | الأمراض الجلدية | Dermatologie |
| Ophthalmology | طب العيون | Augenheilkunde |
| Psychiatry | الطب النفسي | Psychiatrie |
| Radiology | الأشعة | Radiologie |
| Emergency | الطوارئ | Notaufnahme |
| Surgery | الجراحة | Chirurgie |

