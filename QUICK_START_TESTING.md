# Quick Start - Testing Guide

## 🚀 Quick Steps to Test

### 1. Stop Current Application
The app is running and locking files. Stop it first:

**Option A - Using Task Manager:**
- Open Task Manager
- Find "Bookify.Api" process (PID 20552)
- End Task

**Option B - Using PowerShell:**
```powershell
taskkill /F /PID 20552
```

**Option C - In Your IDE:**
- Stop the debug/run session

### 2. Rebuild Application
```powershell
dotnet build Bookify.Api/Bookify.Api.csproj
```

### 3. Run Application
```powershell
dotnet run --project Bookify.Api
```

Wait for: `Now listening on: http://localhost:5080`

### 4. Test Endpoints

#### Quick Test Script:
```powershell
powershell -ExecutionPolicy Bypass -File quick-test.ps1
```

#### Or Test Manually:

**Test 1: Health Check**
```powershell
Invoke-WebRequest -Uri "http://localhost:5080/health" -Method GET
```

**Test 2: Register User (FIXED)**
```powershell
$body = @{
    firstName = "John"
    lastName = "Doe"
    email = "john.doe@test.com"
    password = "Test123!"
    confirmPassword = "Test123!"
} | ConvertTo-Json

$response = Invoke-WebRequest -Uri "http://localhost:5080/api/auth/register" -Method POST -Body $body -ContentType "application/json"
$result = $response.Content | ConvertFrom-Json
$token = $result.token
Write-Host "Token: $token"
```

**Test 3: Add to Cart (FIXED)**
```powershell
$checkIn = (Get-Date).AddDays(7).ToString("yyyy-MM-ddTHH:mm:ssZ")
$checkOut = (Get-Date).AddDays(10).ToString("yyyy-MM-ddTHH:mm:ssZ")
$body = @{
    roomId = 1
    checkInDate = $checkIn
    checkOutDate = $checkOut
} | ConvertTo-Json

Invoke-WebRequest -Uri "http://localhost:5080/api/bookings/cart" -Method POST -Body $body -ContentType "application/json"
```

**Test 4: Get Cart**
```powershell
Invoke-WebRequest -Uri "http://localhost:5080/api/bookings/cart" -Method GET
```

**Test 5: Get Profile (requires token)**
```powershell
$headers = @{ Authorization = "Bearer $token" }
Invoke-WebRequest -Uri "http://localhost:5080/api/profile" -Method GET -Headers $headers
```

---

## ✅ What's Fixed

1. ✅ **Registration** - No longer requires UserName/Role (auto-filled)
2. ✅ **Cart** - No longer requires CustomerName/Email (only needs roomId + dates)

---

## 📋 All Endpoints Ready to Test

See `API_TESTING_GUIDE.md` for complete endpoint list and examples.

---

**Ready to test!** Just stop the app, rebuild, and run the tests.

