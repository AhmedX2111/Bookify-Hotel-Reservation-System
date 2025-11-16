# Quick API Test Script
$baseUrl = "http://localhost:5080"

Write-Host "=== Bookify API Testing ===" -ForegroundColor Cyan

# Test 1: Health Check
Write-Host "`n[1] Health Check" -ForegroundColor Yellow
try {
    $r = Invoke-WebRequest -Uri "$baseUrl/health" -Method GET
    $h = $r.Content | ConvertFrom-Json
    Write-Host "   OK - Status: $($r.StatusCode), Health: $($h.status)" -ForegroundColor Green
} catch {
    Write-Host "   FAILED: $_" -ForegroundColor Red
}

# Test 2: Room Types
Write-Host "`n[2] Get Room Types" -ForegroundColor Yellow
try {
    $r = Invoke-WebRequest -Uri "$baseUrl/api/rooms/roomtypes" -Method GET
    $types = $r.Content | ConvertFrom-Json
    Write-Host "   OK - Status: $($r.StatusCode), Count: $($types.Count)" -ForegroundColor Green
} catch {
    Write-Host "   FAILED: $_" -ForegroundColor Red
}

# Test 3: Available Rooms
Write-Host "`n[3] Get Available Rooms" -ForegroundColor Yellow
try {
    $checkIn = (Get-Date).AddDays(7).ToString("yyyy-MM-dd")
    $checkOut = (Get-Date).AddDays(10).ToString("yyyy-MM-dd")
    $url = "$baseUrl/api/rooms/available?checkInDate=$checkIn&checkOutDate=$checkOut"
    $r = Invoke-WebRequest -Uri $url -Method GET
    $rooms = $r.Content | ConvertFrom-Json
    Write-Host "   OK - Status: $($r.StatusCode), Rooms: $($rooms.Count)" -ForegroundColor Green
    if ($rooms.Count -gt 0) {
        $script:testRoomId = $rooms[0].id
        Write-Host "   Test Room ID: $($script:testRoomId)" -ForegroundColor Gray
    }
} catch {
    Write-Host "   FAILED: $_" -ForegroundColor Red
}

# Test 4: Register User
Write-Host "`n[4] Register User" -ForegroundColor Yellow
try {
    $email = "testuser$(Get-Random)@test.com"
    $body = @{
        firstName = "Test"
        lastName = "User"
        email = $email
        password = "Test123!"
        confirmPassword = "Test123!"
    } | ConvertTo-Json
    
    $r = Invoke-WebRequest -Uri "$baseUrl/api/auth/register" -Method POST -Body $body -ContentType "application/json"
    $result = $r.Content | ConvertFrom-Json
    if ($result.success) {
        $script:token = $result.token
        Write-Host "   OK - User: $email" -ForegroundColor Green
        Write-Host "   Token: $($script:token.Substring(0,30))..." -ForegroundColor Gray
    }
} catch {
    Write-Host "   FAILED: $_" -ForegroundColor Red
}

# Test 5: Add to Cart
Write-Host "`n[5] Add to Cart" -ForegroundColor Yellow
if ($script:testRoomId) {
    try {
        $checkIn = (Get-Date).AddDays(7).ToString("yyyy-MM-ddTHH:mm:ssZ")
        $checkOut = (Get-Date).AddDays(10).ToString("yyyy-MM-ddTHH:mm:ssZ")
        $body = @{
            roomId = $script:testRoomId
            checkInDate = $checkIn
            checkOutDate = $checkOut
        } | ConvertTo-Json
        
        $r = Invoke-WebRequest -Uri "$baseUrl/api/bookings/cart" -Method POST -Body $body -ContentType "application/json"
        $cart = $r.Content | ConvertFrom-Json
        Write-Host "   OK - Room: $($cart.cartItem.roomNumber), Total: $($cart.cartItem.totalCost)" -ForegroundColor Green
    } catch {
        Write-Host "   FAILED: $_" -ForegroundColor Red
    }
} else {
    Write-Host "   SKIPPED - No room ID" -ForegroundColor Yellow
}

# Test 6: Get Profile (requires auth)
Write-Host "`n[6] Get User Profile" -ForegroundColor Yellow
if ($script:token) {
    try {
        $headers = @{ Authorization = "Bearer $($script:token)" }
        $r = Invoke-WebRequest -Uri "$baseUrl/api/profile" -Method GET -Headers $headers
        $profile = $r.Content | ConvertFrom-Json
        Write-Host "   OK - Name: $($profile.name), Bookings: $($profile.bookings.Count)" -ForegroundColor Green
    } catch {
        Write-Host "   FAILED: $_" -ForegroundColor Red
    }
} else {
    Write-Host "   SKIPPED - No token" -ForegroundColor Yellow
}

Write-Host "`n=== Testing Complete ===" -ForegroundColor Cyan

