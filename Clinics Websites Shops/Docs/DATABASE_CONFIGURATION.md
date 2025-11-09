# Database Configuration Guide

This application now supports flexible database configuration using Laravel-style `.env` files. You can easily switch between SQL Server and MySQL without changing any code.

## Quick Start

1. **Copy the example environment file:**
   ```bash
   cp .env.example .env
   ```

2. **Edit the `.env` file with your database settings:**
   ```env
   DB_CONNECTION=sqlserver  # or mysql
   DB_HOST=localhost
   DB_PORT=1433            # 1433 for SQL Server, 3306 for MySQL
   DB_DATABASE=ClinicsWebsiteShops
   DB_USERNAME=your_username
   DB_PASSWORD=your_password
   ```

3. **Run migrations:**
   ```bash
   dotnet ef database update
   ```

## Supported Database Types

### SQL Server Configuration
```env
DB_CONNECTION=sqlserver
DB_HOST=localhost
DB_PORT=1433
DB_DATABASE=ClinicsWebsiteShops
DB_USERNAME=sa
DB_PASSWORD=YourPassword123!
```

**Connection String Generated:**
```
Server=localhost,1433;Database=ClinicsWebsiteShops;User Id=sa;Password=YourPassword123!;TrustServerCertificate=true;
```

**For Windows Authentication (leave username/password empty):**
```env
DB_CONNECTION=sqlserver
DB_HOST=localhost
DB_DATABASE=ClinicsWebsiteShops
DB_USERNAME=
DB_PASSWORD=
```

**Connection String Generated:**
```
Server=localhost;Database=ClinicsWebsiteShops;Trusted_Connection=true;TrustServerCertificate=true;
```

### MySQL Configuration
```env
DB_CONNECTION=mysql
DB_HOST=localhost
DB_PORT=3306
DB_DATABASE=clinics_website_shops
DB_USERNAME=root
DB_PASSWORD=your_mysql_password
```

**Connection String Generated:**
```
Server=localhost;Port=3306;Database=clinics_website_shops;User=root;Password=your_mysql_password;
```

## Multi-Tenant Database Configuration

The application supports separate master database configuration:

```env
# Main application database
DB_CONNECTION=sqlserver
DB_HOST=localhost
DB_DATABASE=ClinicsWebsiteShops

# Master database for tenant management
MASTER_DB_HOST=localhost
MASTER_DB_DATABASE=ClinicsMaster
MASTER_DB_USERNAME=sa
MASTER_DB_PASSWORD=YourPassword123!
```

If master database settings are not provided, the system uses the main database configuration.

## Language Configuration

Configure supported languages dynamically:

```env
DEFAULT_LANGUAGE=en
SUPPORTED_LANGUAGES=en,ar,fr,de
```

## Complete Configuration Examples

### Example 1: SQL Server with Windows Authentication
```env
APP_NAME="My Clinic System"
APP_ENV=production
DB_CONNECTION=sqlserver
DB_HOST=.\SQLEXPRESS
DB_DATABASE=ClinicsWebsiteShops
DB_USERNAME=
DB_PASSWORD=
MASTER_DB_DATABASE=ClinicsMaster
DEFAULT_LANGUAGE=en
SUPPORTED_LANGUAGES=en,ar
```

### Example 2: MySQL with User Authentication
```env
APP_NAME="My Clinic System"
APP_ENV=development
DB_CONNECTION=mysql
DB_HOST=localhost
DB_PORT=3306
DB_DATABASE=clinics_db
DB_USERNAME=clinic_user
DB_PASSWORD=secure_password_123
MASTER_DB_DATABASE=clinics_master
DEFAULT_LANGUAGE=ar
SUPPORTED_LANGUAGES=ar,en,fr
```

### Example 3: Remote Database Server
```env
APP_NAME="My Clinic System"
APP_ENV=production
DB_CONNECTION=sqlserver
DB_HOST=192.168.1.100
DB_PORT=1433
DB_DATABASE=ClinicsProduction
DB_USERNAME=clinic_app
DB_PASSWORD=VerySecurePassword123!
MASTER_DB_HOST=192.168.1.100
MASTER_DB_DATABASE=ClinicsMasterProd
MASTER_DB_USERNAME=clinic_app
MASTER_DB_PASSWORD=VerySecurePassword123!
```

## Environment Variables Reference

| Variable | Description | Default | Required |
|----------|-------------|---------|----------|
| `DB_CONNECTION` | Database type (`sqlserver` or `mysql`) | `sqlserver` | Yes |
| `DB_HOST` | Database server hostname/IP | `localhost` | Yes |
| `DB_PORT` | Database server port | `1433` (SQL) / `3306` (MySQL) | No |
| `DB_DATABASE` | Database name | | Yes |
| `DB_USERNAME` | Database username | | No* |
| `DB_PASSWORD` | Database password | | No* |
| `MASTER_DB_HOST` | Master database hostname | Same as `DB_HOST` | No |
| `MASTER_DB_PORT` | Master database port | Same as `DB_PORT` | No |
| `MASTER_DB_DATABASE` | Master database name | `ClinicsMaster` | No |
| `MASTER_DB_USERNAME` | Master database username | Same as `DB_USERNAME` | No |
| `MASTER_DB_PASSWORD` | Master database password | Same as `DB_PASSWORD` | No |
| `DEFAULT_LANGUAGE` | Default application language | `en` | No |
| `SUPPORTED_LANGUAGES` | Comma-separated supported languages | `en,ar` | No |

*Username and password are optional for SQL Server when using Windows Authentication.

## Database-Specific Features

### SQL Server Features
- **Windows Authentication**: Leave username/password empty
- **Retry Logic**: Automatic retry on connection failures
- **DateTime2**: Optimized datetime handling
- **TrustServerCertificate**: Enabled for development

### MySQL Features
- **UTF8MB4**: Full Unicode support with emojis
- **Case-Insensitive Collation**: `utf8mb4_unicode_ci`
- **Boolean Mapping**: Proper `tinyint(1)` mapping
- **Table Names**: Lowercase conventions
- **Auto-Detection**: Automatic server version detection

## Migration Commands

### Create Migration
```bash
dotnet ef migrations add MigrationName
```

### Update Database
```bash
dotnet ef database update
```

### Generate SQL Script
```bash
dotnet ef migrations script
```

## Security Best Practices

1. **Never commit `.env` files** - They're in `.gitignore`
2. **Use strong passwords** for database connections
3. **Restrict database user permissions** to minimum required
4. **Use environment-specific configurations**:
   - `.env.development` for development
   - `.env.production` for production
   - `.env.testing` for testing

## Troubleshooting

### Common Issues

**Connection String Error:**
- Verify database server is running
- Check firewall settings
- Confirm credentials are correct

**Migration Errors:**
- Ensure database exists
- Check user permissions for DDL operations
- Verify connection string format

**MySQL Connection Issues:**
- Install MySQL server
- Enable TCP/IP connections
- Check port 3306 is open

**SQL Server Connection Issues:**
- Enable SQL Server Authentication if using username/password
- Enable TCP/IP protocol
- Check SQL Server is running

### Debug Connection Strings

The application logs the actual connection strings being used (without passwords) in development mode. Check the console output for connection debugging.

## Production Deployment

1. **Copy `.env.example` to `.env` on server**
2. **Configure production database settings**
3. **Set `APP_ENV=production`**
4. **Run migrations**: `dotnet ef database update`
5. **Secure the `.env` file** with appropriate file permissions

## Adding New Database Providers

The system is extensible. To add support for PostgreSQL or other databases:

1. **Add NuGet package** for the provider
2. **Update `DatabaseProvider` enum** in `EnvironmentService.cs`
3. **Add connection string builder** method
4. **Add provider configuration** in `DbContextConfigurationExtensions.cs`
5. **Update documentation**

This flexible system allows any developer to use their preferred database system without touching the application code!