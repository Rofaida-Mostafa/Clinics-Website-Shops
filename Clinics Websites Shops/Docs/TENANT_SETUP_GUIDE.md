# Test Tenant Setup Guide

## ✅ Tenant Created Successfully!

Your test tenant and admin user have been created successfully with the following details:

### 📋 Tenant Information

| Property | Value |
|----------|-------|
| **Tenant ID** | `445029cf-8991-472a-a4e0-03e5e962e45b` |
| **Tenant Name** | Test Clinic |
| **Domain** | test.localhost |
| **Database** | ClinicsWebsiteShops |
| **Status** | Active |
| **Supported Languages** | en, ar |

### 👤 Admin User Information

| Property | Value |
|----------|-------|
| **Email** | test@test.com |
| **Password** | 123456 |
| **Name** | Test Admin |
| **Role** | SuperAdmin |
| **Email Confirmed** | Yes |

---

## 🚀 How to Login

### Step 1: Update Your Hosts File

To access the tenant via the domain `test.localhost`, you need to add an entry to your hosts file.

**On macOS/Linux:**

1. Open Terminal
2. Edit the hosts file:
   ```bash
   sudo nano /etc/hosts
   ```

3. Add this line at the end:
   ```
   127.0.0.1    test.localhost
   ```

4. Save and exit (Ctrl+X, then Y, then Enter)

5. Flush DNS cache (macOS):
   ```bash
   sudo dscacheutil -flushcache
   sudo killall -HUP mDNSResponder
   ```

**On Windows:**

1. Open Notepad as Administrator
2. Open file: `C:\Windows\System32\drivers\etc\hosts`
3. Add this line at the end:
   ```
   127.0.0.1    test.localhost
   ```
4. Save the file
5. Flush DNS cache:
   ```cmd
   ipconfig /flushdns
   ```

### Step 2: Access the Login Page

1. Make sure the application is running:
   ```bash
   dotnet run
   ```

2. Open your browser and navigate to:
   ```
   http://test.localhost:5292/Identity/Account/Login
   ```

   Or with culture parameter:
   ```
   http://test.localhost:5292/en/Identity/Account/Login
   ```

### Step 3: Login

1. Enter the credentials:
   - **Email**: `test@test.com`
   - **Password**: `123456`

2. Click "Login"

3. You will be redirected to the Admin Dashboard: `/en/Admin/Home/Index`

---

## 🔐 What You Can Do as SuperAdmin

As a SuperAdmin, you have full access to all features:

✅ **User Management**
- Create, edit, delete users
- Assign roles to users

✅ **Role Management**
- Create, edit, delete roles
- Assign permissions to roles

✅ **Doctor Management**
- Create, edit, delete doctors
- View doctor details

✅ **Nurse Management**
- Create, edit, delete nurses
- View nurse details

✅ **Patient Management**
- Create, edit, delete patients
- View patient details

✅ **Department Management**
- Create, edit, delete departments
- Manage department translations

✅ **All Other Features**
- Full access to all modules
- All permissions granted

---

## 🔧 Development Endpoint

The tenant and admin user were created using the development endpoint:

```
GET http://localhost:5292/setup-test-tenant
```

**Note**: This endpoint is only available in Development environment and will be automatically disabled in Production.

If you need to create another tenant or reset the current one, you can call this endpoint again. It will:
- Create the tenant if it doesn't exist
- Create the admin user if it doesn't exist
- Update the tenant ID if the user already exists
- Assign SuperAdmin role if not already assigned

---

## 📊 Database Verification

You can verify the tenant and user in the database:

**Check Tenant:**
```sql
USE ClinicsMaster;
SELECT * FROM tenants WHERE Domain = 'test.localhost';
```

**Check Admin User:**
```sql
USE ClinicsWebsiteShops;
SELECT Id, Email, Name, TenantId FROM aspnetusers WHERE Email = 'test@test.com';
```

**Check User Role:**
```sql
USE ClinicsWebsiteShops;
SELECT u.Email, r.Name as RoleName 
FROM aspnetusers u 
JOIN aspnetuserroles ur ON u.Id = ur.UserId 
JOIN aspnetroles r ON ur.RoleId = r.Id 
WHERE u.Email = 'test@test.com';
```

---

## 🎯 Next Steps

1. ✅ **Login** with the test admin account
2. ✅ **Create additional users** (doctors, nurses, patients)
3. ✅ **Create roles** with specific permissions
4. ✅ **Test the permission system** by creating users with different roles
5. ✅ **Add localization** for the login page and other areas
6. ✅ **Customize the login page** to match your branding

---

## 🔒 Security Notes

⚠️ **Important**: The password `123456` is very weak and should only be used for testing!

For production:
1. Use strong passwords with uppercase, lowercase, numbers, and special characters
2. Enable password requirements in `Program.cs`:
   ```csharp
   options.Password.RequireDigit = true;
   options.Password.RequireLowercase = true;
   options.Password.RequireUppercase = true;
   options.Password.RequireNonAlphanumeric = true;
   options.Password.RequiredLength = 8;
   ```
3. Implement two-factor authentication
4. Use HTTPS in production
5. Remove or secure the `/setup-test-tenant` endpoint

---

## 🐛 Troubleshooting

**Problem**: Can't access `test.localhost`
- **Solution**: Make sure you added the hosts file entry and flushed DNS cache

**Problem**: "Tenant not found" error
- **Solution**: Verify the tenant exists in the ClinicsMaster database

**Problem**: "Invalid email or password"
- **Solution**: Make sure you're using the correct credentials (test@test.com / 123456)

**Problem**: Redirected to login after successful login
- **Solution**: Check browser console for errors, verify cookies are enabled

---

## 📝 Summary

✅ Tenant created: `test.localhost`  
✅ Admin user created: `test@test.com`  
✅ SuperAdmin role assigned  
✅ Database configured  
✅ Login system ready  

**You're all set! Happy testing! 🎉**

