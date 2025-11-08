# Bookify API Endpoint Testing Script
# Run this script to test all endpoints

$baseUrl = "http://localhost:5080"
$script:authToken = $null
$script:userId = $null

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Bookify API Endpoint Testing" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

# Test 1: Health Check
Write-Host "`n[1/10] Testing Health Check..." -ForegroundColor Yellow
try {
    $response = Invoke-WebRequest -Uri "$baseUrl/health" -Method GET -ErrorAction Stop
    $health = $response.Content | ConvertFrom-Json
    Write-Host "   ✓ Status: $($response.StatusCode) - $($health.status)" -ForegroundColor Green
    Write-Host "   ✓ Database: $($health.entries.database.status)" -ForegroundColor Green
} catch {
    Write-Host "   ✗ Error: $_" -ForegroundColor Red
}

# Test 2: Get Room Types
Write-Host "`n[2/10] Testing Get Room Types..." -ForegroundColor Yellow
try {
    $response = Invoke-WebRequest -Uri "$baseUrl/api/rooms/roomtypes" -Method GET -ErrorAction Stop
    $roomTypes = $response.Content | ConvertFrom-Json
    Write-Host "   ✓ Status: $($response.StatusCode)" -ForegroundColor Green
    Write-Host "   ✓ Room Types Found: $($roomTypes.Count)" -ForegroundColor Green
    if ($roomTypes.Count -gt 0) {
        Write-Host "   ✓ First Room Type: $($roomTypes[0].name)" -ForegroundColor Gray
    }
} catch {
    Write-Host "   ✗ Error: $_" -ForegroundColor Red
}

# Test 3: Get Available Rooms
Write-Host "`n[3/10] Testing Get Available Rooms..." -ForegroundColor Yellow
try {
    $checkIn = (Get-Date).AddDays(7).ToString("yyyy-MM-dd")
    $checkOut = (Get-Date).AddDays(10).ToString("yyyy-MM-dd")
    $url = "$baseUrl/api/rooms/available?checkInDate=$checkIn" + '&checkOutDate=' + $checkOut
    $response = Invoke-WebRequest -Uri $url -Method GET -ErrorAction Stop
    $rooms = $response.Content | ConvertFrom-Json
    Write-Host "   ✓ Status: $($response.StatusCode)" -ForegroundColor Green
    Write-Host "   ✓ Available Rooms: $($rooms.Count)" -ForegroundColor Green
    if ($rooms.Count -gt 0) {
        $script:testRoomId = $rooms[0].id
        Write-Host "   ✓ Test Room ID: $($script:testRoomId)" -ForegroundColor Gray
    }
} catch {
    Write-Host "   ✗ Error: $_" -ForegroundColor Red
}

# Test 4: Register User
Write-Host "`n[4/10] Testing User Registration..." -ForegroundColor Yellow
try {
    $randomEmail = "testuser$(Get-Random)@example.com"
    $body = @{
        firstName = "Test"
        lastName = "User"
        email = $randomEmail
        password = "Test123!"
        confirmPassword = "Test123!"
    } | ConvertTo-Json

    $response = Invoke-WebRequest -Uri "$baseUrl/api/auth/register" -Method POST -Body $body -ContentType "application/json" -ErrorAction Stop
    $result = $response.Content | ConvertFrom-Json
    if ($result.success) {
        Write-Host "   ✓ Status: $($response.StatusCode)" -ForegroundColor Green
        Write-Host "   ✓ User Registered: $randomEmail" -ForegroundColor Green
        $script:authToken = $result.token
        $script:userId = $result.user.id
        Write-Host "   ✓ Token Received: $($authToken.Substring(0, 30))..." -ForegroundColor Gray
    }
} catch {
    Write-Host "   ✗ Error: $_" -ForegroundColor Red
    if ($_.Exception.Response.StatusCode -eq 400) {
        Write-Host "   → User might already exist, trying login..." -ForegroundColor Yellow
        # Try login instead
        try {
            $loginBody = @{
                email = $randomEmail
                password = "Test123!"
            } | ConvertTo-Json
            $loginResponse = Invoke-WebRequest -Uri "$baseUrl/api/auth/login" -Method POST -Body $loginBody -ContentType "application/json" -ErrorAction Stop
            $loginResult = $loginResponse.Content | ConvertFrom-Json
            if ($loginResult.success) {
                $script:authToken = $loginResult.token
                $script:userId = $loginResult.user.id
                Write-Host "   ✓ Login Successful" -ForegroundColor Green
            }
        } catch {
            Write-Host "   ✗ Login also failed" -ForegroundColor Red
        }
    }
}

# Test 5: Add to Cart
Write-Host "`n[5/10] Testing Add to Cart..." -ForegroundColor Yellow
if ($script:testRoomId) {
    try {
        $checkInDate = (Get-Date).AddDays(7).ToString("yyyy-MM-ddTHH:mm:ssZ")
        $checkOutDate = (Get-Date).AddDays(10).ToString("yyyy-MM-ddTHH:mm:ssZ")
        $body = @{
            roomId = $script:testRoomId
            checkInDate = $checkInDate
            checkOutDate = $checkOutDate
        } | ConvertTo-Json

        $response = Invoke-WebRequest -Uri "$baseUrl/api/bookings/cart" -Method POST -Body $body -ContentType "application/json" -ErrorAction Stop
        $cart = $response.Content | ConvertFrom-Json
        Write-Host "   ✓ Status: $($response.StatusCode)" -ForegroundColor Green
        Write-Host "   ✓ Cart Item Added: Room $($cart.cartItem.roomNumber)" -ForegroundColor Green
        Write-Host "   ✓ Total Cost: $($cart.cartItem.totalCost)" -ForegroundColor Gray
    } catch {
        Write-Host "   ✗ Error: $_" -ForegroundColor Red
    }
} else {
    Write-Host "   ⚠ Skipped: No room ID available" -ForegroundColor Yellow
}

# Test 6: Get Cart
Write-Host "`n[6/10] Testing Get Cart..." -ForegroundColor Yellow
try {
    $response = Invoke-WebRequest -Uri "$baseUrl/api/bookings/cart" -Method GET -ErrorAction Stop
    $cart = $response.Content | ConvertFrom-Json
    Write-Host "   ✓ Status: $($response.StatusCode)" -ForegroundColor Green
    if ($cart.cartItem) {
        Write-Host "   ✓ Cart has item: $($cart.cartItem.roomTypeName)" -ForegroundColor Green
    } else {
        Write-Host "   ⚠ Cart is empty" -ForegroundColor Yellow
    }
} catch {
    Write-Host "   ✗ Error: $_" -ForegroundColor Red
}

# Test 7: Get User Profile (Requires Auth)
Write-Host "`n[7/10] Testing Get User Profile..." -ForegroundColor Yellow
if ($script:authToken) {
    try {
        $headers = @{
            "Authorization" = "Bearer $($script:authToken)"
        }
        $response = Invoke-WebRequest -Uri "$baseUrl/api/profile" -Method GET -Headers $headers -ErrorAction Stop
        $profile = $response.Content | ConvertFrom-Json
        Write-Host "   ✓ Status: $($response.StatusCode)" -ForegroundColor Green
        Write-Host "   ✓ Profile: $($profile.name) - $($profile.email)" -ForegroundColor Green
        Write-Host "   ✓ Bookings Count: $($profile.bookings.Count)" -ForegroundColor Gray
    } catch {
        Write-Host "   ✗ Error: $_" -ForegroundColor Red
    }
} else {
    Write-Host "   ⚠ Skipped: No auth token" -ForegroundColor Yellow
}

# Test 8: Get Booking History
Write-Host "`n[8/10] Testing Get Booking History..." -ForegroundColor Yellow
if ($script:authToken) {
    try {
        $headers = @{
            "Authorization" = "Bearer $($script:authToken)"
        }
        $response = Invoke-WebRequest -Uri "$baseUrl/api/profile/bookings" -Method GET -Headers $headers -ErrorAction Stop
        $bookings = $response.Content | ConvertFrom-Json
        Write-Host "   ✓ Status: $($response.StatusCode)" -ForegroundColor Green
        Write-Host "   ✓ Booking History Count: $($bookings.Count)" -ForegroundColor Green
    } catch {
        Write-Host "   ✗ Error: $_" -ForegroundColor Red
    }
} else {
    Write-Host "   ⚠ Skipped: No auth token" -ForegroundColor Yellow
}

# Test 9: Get User Bookings
Write-Host "`n[9/10] Testing Get User Bookings..." -ForegroundColor Yellow
if ($script:authToken) {
    try {
        $headers = @{
            "Authorization" = "Bearer $($script:authToken)"
        }
        $response = Invoke-WebRequest -Uri "$baseUrl/api/bookings" -Method GET -Headers $headers -ErrorAction Stop
        $bookings = $response.Content | ConvertFrom-Json
        Write-Host "   ✓ Status: $($response.StatusCode)" -ForegroundColor Green
        Write-Host "   ✓ User Bookings Count: $($bookings.Count)" -ForegroundColor Green
    } catch {
        Write-Host "   ✗ Error: $_" -ForegroundColor Red
    }
} else {
    Write-Host "   ⚠ Skipped: No auth token" -ForegroundColor Yellow
}

# Test 10: Swagger UI Check
Write-Host "`n[10/10] Checking Swagger UI..." -ForegroundColor Yellow
try {
    $response = Invoke-WebRequest -Uri "$baseUrl/swagger/index.html" -Method GET -ErrorAction Stop
    Write-Host "   ✓ Swagger UI Available: $($response.StatusCode)" -ForegroundColor Green
    Write-Host "   → Open in browser: $baseUrl/swagger" -ForegroundColor Cyan
} catch {
    Write-Host "   ⚠ Swagger UI not available (might be disabled in production)" -ForegroundColor Yellow
}

# Summary
Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "  Test Summary" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Base URL: $baseUrl" -ForegroundColor Gray
if ($script:authToken) {
    Write-Host "Auth Token: $($script:authToken.Substring(0, 30))..." -ForegroundColor Gray
}
Write-Host "`n✅ All tests completed!" -ForegroundColor Green
Write-Host "`nNext Steps:" -ForegroundColor Yellow
Write-Host "1. Test booking confirmation with payment (requires Stripe test payment method)" -ForegroundColor White
Write-Host "2. Test admin endpoints (requires admin user)" -ForegroundColor White
Write-Host "3. Check application logs in: Bookify.Api/Logs folder" -ForegroundColor White

