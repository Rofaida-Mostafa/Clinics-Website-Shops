# MySQL Setup Guide for Clinics Website Shops

## Current Issue
You're getting a "Unable to connect to any of the specified MySQL hosts" error. This means the application can't connect to your MySQL server.

## Quick Fix Solutions

### Option 1: Fix MySQL Connection (Recommended)

1. **Set up MySQL root password** (if not already set):
   ```bash
   mysql -u root
   ALTER USER 'root'@'localhost' IDENTIFIED BY 'your_password_here';
   FLUSH PRIVILEGES;
   EXIT;
   ```

2. **Create the required databases**:
   ```bash
   mysql -u root -p
   CREATE DATABASE IF NOT EXISTS ClinicsWebsiteShops;
   CREATE DATABASE IF NOT EXISTS MasterDb;
   SHOW DATABASES;
   EXIT;
   ```

3. **Update your .env file** with the correct password:
   ```env
   DB_CONNECTION=mysql
   DB_HOST=localhost
   DB_PORT=3306
   DB_DATABASE=ClinicsWebsiteShops
   DB_USERNAME=root
   DB_PASSWORD=your_password_here
   
   MASTER_DB_DATABASE=MasterDb
   MASTER_DB_USERNAME=root
   MASTER_DB_PASSWORD=your_password_here
   ```

### Option 2: Switch to SQL Server (Easier for Windows/Mac)

Update your `.env` file to use SQL Server instead:
```env
DB_CONNECTION=sqlserver
DB_HOST=localhost
DB_PORT=1433
DB_DATABASE=ClinicsWebsiteShops
DB_USERNAME=
DB_PASSWORD=

MASTER_DB_HOST=localhost
MASTER_DB_PORT=1433
MASTER_DB_DATABASE=MasterDb
MASTER_DB_USERNAME=
MASTER_DB_PASSWORD=
```

## Troubleshooting Steps

### Check MySQL Status
```bash
# Check if MySQL is running
brew services list | grep mysql

# Start MySQL if not running
brew services start mysql@8.4
```

### Test MySQL Connection
```bash
# Test connection with password
mysql -h localhost -P 3306 -u root -p

# Test connection without password (if no password set)
mysql -h localhost -P 3306 -u root
```

### Create MySQL User (Alternative)
Instead of using root, create a dedicated user:
```sql
CREATE USER 'clinic_user'@'localhost' IDENTIFIED BY 'clinic_password_123';
GRANT ALL PRIVILEGES ON ClinicsWebsiteShops.* TO 'clinic_user'@'localhost';
GRANT ALL PRIVILEGES ON MasterDb.* TO 'clinic_user'@'localhost';
FLUSH PRIVILEGES;
```

Then update `.env`:
```env
DB_USERNAME=clinic_user
DB_PASSWORD=clinic_password_123
MASTER_DB_USERNAME=clinic_user
MASTER_DB_PASSWORD=clinic_password_123
```

## Common MySQL Issues on macOS

### Issue 1: Socket Connection Problems
If you get socket errors, try TCP connection:
```env
DB_HOST=127.0.0.1  # Instead of localhost
```

### Issue 2: Authentication Plugin Issues
MySQL 8+ uses different authentication. Update with:
```sql
ALTER USER 'root'@'localhost' IDENTIFIED WITH mysql_native_password BY 'your_password';
```

### Issue 3: SSL/Security Issues
The connection string now includes `SslMode=None` for local development.

## Recommended .env Configuration

For **MySQL** (after setting up password):
```env
DB_CONNECTION=mysql
DB_HOST=127.0.0.1
DB_PORT=3306
DB_DATABASE=ClinicsWebsiteShops
DB_USERNAME=root
DB_PASSWORD=your_mysql_password

MASTER_DB_HOST=127.0.0.1
MASTER_DB_PORT=3306
MASTER_DB_DATABASE=MasterDb
MASTER_DB_USERNAME=root
MASTER_DB_PASSWORD=your_mysql_password
```

For **SQL Server** (easier setup):
```env
DB_CONNECTION=sqlserver
DB_HOST=localhost
DB_PORT=1433
DB_DATABASE=ClinicsWebsiteShops
DB_USERNAME=
DB_PASSWORD=

MASTER_DB_HOST=localhost
MASTER_DB_PORT=1433
MASTER_DB_DATABASE=MasterDb
MASTER_DB_USERNAME=
MASTER_DB_PASSWORD=
```

## After Fixing Connection

1. **Run migrations**:
   ```bash
   dotnet ef database update
   ```

2. **Start the application**:
   ```bash
   dotnet run
   ```

Choose the option that works best for your setup!