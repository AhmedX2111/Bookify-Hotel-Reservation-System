# Endpoint Test Results

## Test Execution Date
**Date**: November 6, 2024
**Time**: Test execution in progress

## Application Status
- **Base URL**: `http://localhost:5080`
- **Database**: Updated with migration `MakePaymentIntentIdNullable`
- **Status**: ✅ Running

---

## Test Results

### 1. Health Check Endpoint ✅
**Endpoint**: `GET /health`

**Test Command**:
```powershell
Invoke-WebRequest -Uri "http://localhost:5080/health" -Method GET
```

**Expected Result**: 
- Status Code: `200 OK`
- Response: `{"status":"Healthy","entries":{"database":{"status":"Healthy"}}}`

**Status**: ⏳ Testing...

---

### 2. Public Room Endpoints

#### 2.1 Get Room Types ✅
**Endpoint**: `GET /api/rooms/roomtypes`

**Test Command**:
```powershell
Invoke-WebRequest -Uri "http://localhost:5080/api/rooms/roomtypes" -Method GET
```

**Expected Result**: 
- Status Code: `200 OK`
- Response: Array of room types

**Status**: ⏳ Testing...

#### 2.2 Get Available Rooms ✅
**Endpoint**: `GET /api/rooms/available?checkInDate={date}&checkOutDate={date}`

**Test Command**:
```powershell
$checkIn = (Get-Date).AddDays(7).ToString("yyyy-MM-dd")
$checkOut = (Get-Date).AddDays(10).ToString("yyyy-MM-dd")
Invoke-WebRequest -Uri "http://localhost:5080/api/rooms/available?checkInDate=$checkIn&checkOutDate=$checkOut" -Method GET
```

**Expected Result**: 
- Status Code: `200 OK`
- Response: Array of available rooms

**Status**: ⏳ Testing...

---

### 3. Authentication Endpoints

#### 3.1 Register User
**Endpoint**: `POST /api/auth/register`

**Test Command**:
```powershell
$body = @{
    firstName = "Test"
    lastName = "User"
    email = "testuser@example.com"
    password = "Test123!"
    confirmPassword = "Test123!"
} | ConvertTo-Json

Invoke-WebRequest -Uri "http://localhost:5080/api/auth/register" -Method POST -Body $body -ContentType "application/json"
```

**Expected Result**: 
- Status Code: `200 OK`
- Response: JWT token and user info

**Status**: ⏳ Pending...

#### 3.2 Login User
**Endpoint**: `POST /api/auth/login`

**Test Command**:
```powershell
$body = @{
    email = "testuser@example.com"
    password = "Test123!"
} | ConvertTo-Json

Invoke-WebRequest -Uri "http://localhost:5080/api/auth/login" -Method POST -Body $body -ContentType "application/json"
```

**Expected Result**: 
- Status Code: `200 OK`
- Response: JWT token

**Status**: ⏳ Pending...

---

### 4. Booking Endpoints (Requires Authentication)

#### 4.1 Add to Cart
**Endpoint**: `POST /api/bookings/cart`

**Test Command**:
```powershell
$body = @{
    roomId = 1
    checkInDate = (Get-Date).AddDays(7).ToString("yyyy-MM-ddTHH:mm:ssZ")
    checkOutDate = (Get-Date).AddDays(10).ToString("yyyy-MM-ddTHH:mm:ssZ")
} | ConvertTo-Json

Invoke-WebRequest -Uri "http://localhost:5080/api/bookings/cart" -Method POST -Body $body -ContentType "application/json"
```

**Expected Result**: 
- Status Code: `200 OK`
- Response: Cart item details

**Status**: ⏳ Pending...

#### 4.2 Get Cart
**Endpoint**: `GET /api/bookings/cart`

**Test Command**:
```powershell
Invoke-WebRequest -Uri "http://localhost:5080/api/bookings/cart" -Method GET
```

**Expected Result**: 
- Status Code: `200 OK`
- Response: Current cart item

**Status**: ⏳ Pending...

#### 4.3 Confirm Booking with Payment
**Endpoint**: `POST /api/bookings/confirm`

**Test Command**:
```powershell
$token = "YOUR_JWT_TOKEN_HERE"
$headers = @{
    "Authorization" = "Bearer $token"
}
$body = @{
    paymentMethodId = "pm_card_visa"
} | ConvertTo-Json

Invoke-WebRequest -Uri "http://localhost:5080/api/bookings/confirm" -Method POST -Body $body -ContentType "application/json" -Headers $headers
```

**Expected Result**: 
- Status Code: `201 Created`
- Response: Booking details with PaymentIntentId

**Status**: ⏳ Pending...

#### 4.4 Get User Bookings
**Endpoint**: `GET /api/bookings`

**Test Command**:
```powershell
$token = "YOUR_JWT_TOKEN_HERE"
$headers = @{
    "Authorization" = "Bearer $token"
}

Invoke-WebRequest -Uri "http://localhost:5080/api/bookings" -Method GET -Headers $headers
```

**Expected Result**: 
- Status Code: `200 OK`
- Response: Array of user bookings

**Status**: ⏳ Pending...

---

### 5. Profile Endpoints (Requires Authentication)

#### 5.1 Get User Profile
**Endpoint**: `GET /api/profile`

**Test Command**:
```powershell
$token = "YOUR_JWT_TOKEN_HERE"
$headers = @{
    "Authorization" = "Bearer $token"
}

Invoke-WebRequest -Uri "http://localhost:5080/api/profile" -Method GET -Headers $headers
```

**Expected Result**: 
- Status Code: `200 OK`
- Response: User profile with booking history

**Status**: ⏳ Pending...

#### 5.2 Get Booking History
**Endpoint**: `GET /api/profile/bookings`

**Test Command**:
```powershell
$token = "YOUR_JWT_TOKEN_HERE"
$headers = @{
    "Authorization" = "Bearer $token"
}

Invoke-WebRequest -Uri "http://localhost:5080/api/profile/bookings" -Method GET -Headers $headers
```

**Expected Result**: 
- Status Code: `200 OK`
- Response: Array of booking history

**Status**: ⏳ Pending...

---

## Quick Test Script

Save this as `test-endpoints.ps1`:

```powershell
# Bookify API Endpoint Testing Script
$baseUrl = "http://localhost:5080"

Write-Host "=== Testing Bookify API Endpoints ===" -ForegroundColor Cyan

# Test 1: Health Check
Write-Host "`n1. Testing Health Check..." -ForegroundColor Yellow
try {
    $response = Invoke-WebRequest -Uri "$baseUrl/health" -Method GET
    Write-Host "   Status: $($response.StatusCode)" -ForegroundColor Green
    $health = $response.Content | ConvertFrom-Json
    Write-Host "   Health Status: $($health.status)" -ForegroundColor Green
} catch {
    Write-Host "   Error: $_" -ForegroundColor Red
}

# Test 2: Get Room Types
Write-Host "`n2. Testing Get Room Types..." -ForegroundColor Yellow
try {
    $response = Invoke-WebRequest -Uri "$baseUrl/api/rooms/roomtypes" -Method GET
    Write-Host "   Status: $($response.StatusCode)" -ForegroundColor Green
    $roomTypes = $response.Content | ConvertFrom-Json
    Write-Host "   Room Types Found: $($roomTypes.Count)" -ForegroundColor Green
} catch {
    Write-Host "   Error: $_" -ForegroundColor Red
}

# Test 3: Get Available Rooms
Write-Host "`n3. Testing Get Available Rooms..." -ForegroundColor Yellow
try {
    $checkIn = (Get-Date).AddDays(7).ToString("yyyy-MM-dd")
    $checkOut = (Get-Date).AddDays(10).ToString("yyyy-MM-dd")
    $url = "$baseUrl/api/rooms/available?checkInDate=$checkIn&checkOutDate=$checkOut"
    $response = Invoke-WebRequest -Uri $url -Method GET
    Write-Host "   Status: $($response.StatusCode)" -ForegroundColor Green
    $rooms = $response.Content | ConvertFrom-Json
    Write-Host "   Available Rooms: $($rooms.Count)" -ForegroundColor Green
} catch {
    Write-Host "   Error: $_" -ForegroundColor Red
}

# Test 4: Register User
Write-Host "`n4. Testing User Registration..." -ForegroundColor Yellow
try {
    $body = @{
        firstName = "Test"
        lastName = "User"
        email = "testuser$(Get-Random)@example.com"
        password = "Test123!"
        confirmPassword = "Test123!"
    } | ConvertTo-Json

    $response = Invoke-WebRequest -Uri "$baseUrl/api/auth/register" -Method POST -Body $body -ContentType "application/json"
    Write-Host "   Status: $($response.StatusCode)" -ForegroundColor Green
    $result = $response.Content | ConvertFrom-Json
    if ($result.success) {
        Write-Host "   User Registered Successfully!" -ForegroundColor Green
        $script:authToken = $result.token
        Write-Host "   Token: $($authToken.Substring(0, 20))..." -ForegroundColor Gray
    }
} catch {
    Write-Host "   Error: $_" -ForegroundColor Red
}

# Test 5: Add to Cart (if we have rooms)
Write-Host "`n5. Testing Add to Cart..." -ForegroundColor Yellow
try {
    $body = @{
        roomId = 1
        checkInDate = (Get-Date).AddDays(7).ToString("yyyy-MM-ddTHH:mm:ssZ")
        checkOutDate = (Get-Date).AddDays(10).ToString("yyyy-MM-ddTHH:mm:ssZ")
    } | ConvertTo-Json

    $response = Invoke-WebRequest -Uri "$baseUrl/api/bookings/cart" -Method POST -Body $body -ContentType "application/json"
    Write-Host "   Status: $($response.StatusCode)" -ForegroundColor Green
    $cart = $response.Content | ConvertFrom-Json
    Write-Host "   Cart Item Added: Room $($cart.cartItem.roomNumber)" -ForegroundColor Green
} catch {
    Write-Host "   Error: $_" -ForegroundColor Red
}

Write-Host "`n=== Testing Complete ===" -ForegroundColor Cyan
```

---

## Manual Testing Instructions

### Using Postman/Thunder Client:

1. **Import Collection**: Create a new collection in Postman
2. **Set Base URL**: `http://localhost:5080`
3. **Test Endpoints**: Follow the order in `API_TESTING_GUIDE.md`

### Using Swagger UI:

1. **Open Browser**: Navigate to `http://localhost:5080/swagger`
2. **Authorize**: Click "Authorize" button, enter JWT token
3. **Test Endpoints**: Use the "Try it out" feature

### Using curl (Command Line):

```bash
# Health Check
curl http://localhost:5080/health

# Get Room Types
curl http://localhost:5080/api/rooms/roomtypes

# Register User
curl -X POST http://localhost:5080/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{"firstName":"Test","lastName":"User","email":"test@example.com","password":"Test123!","confirmPassword":"Test123!"}'
```

---

## Next Steps

1. ✅ Migration created and applied
2. ✅ Application running
3. ⏳ Test endpoints using the scripts above
4. ⏳ Verify all Week 3 & 4 features
5. ⏳ Check logs in `Bookify.Api/Logs/` folder

---

**Note**: Replace `YOUR_JWT_TOKEN_HERE` with actual token from login/register response.

