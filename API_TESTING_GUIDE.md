# Bookify Hotel Reservation System - API Testing Guide

## 📋 Table of Contents
1. [Overview](#overview)
2. [Prerequisites](#prerequisites)
3. [Base URL & Authentication](#base-url--authentication)
4. [API Endpoints Reference](#api-endpoints-reference)
5. [Test Cases & Examples](#test-cases--examples)
6. [Case Studies](#case-studies)
7. [Redundancy Analysis](#redundancy-analysis)
8. [Health Check Testing](#health-check-testing)

---

## Overview

This document provides comprehensive testing examples and case studies for all API endpoints in the Bookify Hotel Reservation System. The system follows N-Tier Architecture with Repository Pattern and Unit of Work.

**Base URL**: `http://localhost:5080` (HTTP) or `https://localhost:7280` (HTTPS)

---

## Prerequisites

1. **Database**: Ensure SQL Server is running and database `Bookify_Db_API` exists
2. **Stripe Keys**: Test keys are configured in `appsettings.json`
3. **Application**: Run the API using `dotnet run --project Bookify.Api`
4. **Tools**: Use Postman, curl, or any HTTP client for testing

---

## Base URL & Authentication

### Authentication Flow

1. **Register a new user** (Customer)
2. **Login** to get JWT token
3. **Use token** in Authorization header: `Bearer {token}`

### Example: Getting Authentication Token

```http
POST /api/auth/register
Content-Type: application/json

{
  "firstName": "John",
  "lastName": "Doe",
  "email": "john.doe@example.com",
  "password": "Password123!",
  "confirmPassword": "Password123!"
}
```

**Response:**
```json
{
  "success": true,
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiration": "2024-01-01T12:00:00Z",
  "user": {
    "id": "325c7e9a-9449-4b0a-92cb-ded1d7fc66c0",
    "email": "john.doe@example.com",
    "firstName": "John",
    "lastName": "Doe"
  }
}
```

---

## API Endpoints Reference

### 🔐 Authentication Endpoints

| Method | Endpoint | Auth Required | Description |
|--------|----------|---------------|-------------|
| POST | `/api/auth/register` | No | Register new user |
| POST | `/api/auth/login` | No | Login user |

### 🏨 Room Endpoints (Public)

| Method | Endpoint | Auth Required | Description |
|--------|----------|---------------|-------------|
| GET | `/api/rooms/available` | No | Get available rooms for dates |
| GET | `/api/rooms/search` | No | Search available rooms with filters |
| GET | `/api/rooms/roomtypes` | No | Get all room types |

### 📅 Booking Endpoints (Customer)

| Method | Endpoint | Auth Required | Description |
|--------|----------|---------------|-------------|
| POST | `/api/bookings/cart` | No | Add room to reservation cart |
| GET | `/api/bookings/cart` | No | Get current cart |
| DELETE | `/api/bookings/cart` | No | Clear cart |
| POST | `/api/bookings/confirm` | **Yes** | Confirm booking with payment |
| GET | `/api/bookings` | **Yes** | Get user's bookings |
| GET | `/api/bookings/{id}` | **Yes** | Get booking by ID |
| PUT | `/api/bookings/{id}/cancel` | **Yes** | Cancel booking |
| GET | `/api/bookings/{id}/status` | **Yes** | Get booking status |
| PUT | `/api/bookings/{id}/confirm` | **Yes** | Admin: Confirm booking |
| PUT | `/api/bookings/{id}/reject` | **Yes** | Admin: Reject booking |

### 👤 Profile Endpoints (Customer)

| Method | Endpoint | Auth Required | Description |
|--------|----------|---------------|-------------|
| GET | `/api/profile` | **Yes** | Get user profile with booking history |
| GET | `/api/profile/bookings` | **Yes** | Get user booking history |

### 👨‍💼 Admin Endpoints

| Method | Endpoint | Auth Required | Role | Description |
|--------|----------|---------------|------|-------------|
| GET | `/api/admin/bookings` | **Yes** | Admin | Get all bookings |
| GET | `/api/adminbookings` | **Yes** | Admin | Get paged bookings with search |
| PUT | `/api/adminbookings/{id}/approve` | **Yes** | Admin | Approve booking |
| PUT | `/api/adminbookings/{id}/cancel` | **Yes** | Admin | Cancel booking (admin) |
| GET | `/api/adminrooms` | **Yes** | Admin | Get all rooms |
| POST | `/api/adminrooms` | **Yes** | Admin | Create room |
| PUT | `/api/adminrooms/{id}` | **Yes** | Admin | Update room |
| DELETE | `/api/adminrooms/{id}` | **Yes** | Admin | Delete room |
| GET | `/api/adminroomtypes` | **Yes** | Admin | Get all room types |
| POST | `/api/adminroomtypes` | **Yes** | Admin | Create room type |
| PUT | `/api/adminroomtypes/{id}` | **Yes** | Admin | Update room type |
| DELETE | `/api/adminroomtypes/{id}` | **Yes** | Admin | Delete room type |

### 🏥 Health Check

| Method | Endpoint | Auth Required | Description |
|--------|----------|---------------|-------------|
| GET | `/health` | No | Check application health |

---

## Test Cases & Examples

### Test Case 1: Complete Booking Flow (Week 3 Feature)

**Scenario**: Customer books a room with payment

#### Step 1: Search Available Rooms
```http
GET /api/rooms/available?checkInDate=2024-12-15&checkOutDate=2024-12-20
```

**Expected Response:**
```json
[
  {
    "id": 1,
    "roomNumber": "101",
    "roomType": {
      "id": 1,
      "name": "Deluxe Suite",
      "pricePerNight": 150.00,
      "description": "Spacious suite with ocean view"
    },
    "isAvailable": true
  }
]
```

#### Step 2: Add to Cart
```http
POST /api/bookings/cart
Content-Type: application/json

{
  "roomId": 1,
  "checkInDate": "2024-12-15T14:00:00Z",
  "checkOutDate": "2024-12-20T11:00:00Z"
}
```

**Expected Response:**
```json
{
  "message": "Room added to reservation cart successfully.",
  "cartItem": {
    "roomId": 1,
    "roomNumber": "101",
    "roomTypeName": "Deluxe Suite",
    "pricePerNight": 150.00,
    "checkInDate": "2024-12-15T14:00:00Z",
    "checkOutDate": "2024-12-20T11:00:00Z",
    "numberOfNights": 5,
    "totalCost": 750.00
  }
}
```

#### Step 3: Get Cart
```http
GET /api/bookings/cart
```

#### Step 4: Confirm Booking with Payment
```http
POST /api/bookings/confirm
Authorization: Bearer {JWT_TOKEN}
Content-Type: application/json

{
  "paymentMethodId": "pm_card_visa"  // Stripe test payment method ID
}
```

**Expected Response:**
```json
{
  "message": "Booking created and payment processed successfully! It is now pending admin confirmation.",
  "booking": {
    "id": 1,
    "userId": "325c7e9a-9449-4b0a-92cb-ded1d7fc66c0",
    "roomId": 1,
    "roomNumber": "101",
    "roomType": "Deluxe Suite",
    "checkInDate": "2024-12-15T14:00:00Z",
    "checkOutDate": "2024-12-20T11:00:00Z",
    "numberOfNights": 5,
    "totalCost": 750.00,
    "status": "Pending",
    "paymentIntentId": "pi_1234567890",
    "createdAt": "2024-11-06T10:00:00Z"
  },
  "nextSteps": [
    "Your booking is pending admin confirmation",
    "You will receive a confirmation email once approved",
    "You can cancel the booking before it's confirmed"
  ]
}
```

---

### Test Case 2: User Profile & Booking History (Week 3 Feature)

#### Get User Profile
```http
GET /api/profile
Authorization: Bearer {JWT_TOKEN}
```

**Expected Response:**
```json
{
  "name": "John Doe",
  "email": "john.doe@example.com",
  "bookings": [
    {
      "id": 1,
      "roomType": "Deluxe Suite",
      "checkInDate": "2024-12-15T14:00:00Z",
      "checkOutDate": "2024-12-20T11:00:00Z",
      "totalAmount": 750.00,
      "status": "Pending"
    },
    {
      "id": 2,
      "roomType": "Standard Room",
      "checkInDate": "2024-10-01T14:00:00Z",
      "checkOutDate": "2024-10-05T11:00:00Z",
      "totalAmount": 400.00,
      "status": "Confirmed"
    }
  ]
}
```

#### Get Booking History Only
```http
GET /api/profile/bookings
Authorization: Bearer {JWT_TOKEN}
```

---

### Test Case 3: Admin Booking Management

#### Get All Bookings (Paged)
```http
GET /api/adminbookings?pageNumber=1&pageSize=10&search=john
Authorization: Bearer {ADMIN_JWT_TOKEN}
```

**Expected Response:**
```json
{
  "totalCount": 25,
  "bookings": [
    {
      "id": 1,
      "customerName": "John Doe",
      "customerEmail": "john.doe@example.com",
      "roomNumber": "101",
      "roomType": "Deluxe Suite",
      "checkInDate": "2024-12-15T14:00:00Z",
      "checkOutDate": "2024-12-20T11:00:00Z",
      "totalCost": 750.00,
      "status": "Pending"
    }
  ]
}
```

#### Approve Booking
```http
PUT /api/adminbookings/1/approve
Authorization: Bearer {ADMIN_JWT_TOKEN}
```

**Expected Response:** `204 No Content`

---

### Test Case 4: Health Check (Week 4 Feature)

```http
GET /health
```

**Expected Response (Healthy):**
```json
{
  "status": "Healthy",
  "totalDuration": "00:00:00.1234567",
  "entries": {
    "database": {
      "status": "Healthy",
      "description": "Database is available",
      "duration": "00:00:00.0123456"
    }
  }
}
```

**Expected Response (Unhealthy):**
```json
{
  "status": "Unhealthy",
  "totalDuration": "00:00:00.1234567",
  "entries": {
    "database": {
      "status": "Unhealthy",
      "description": "Database is not available",
      "duration": "00:00:00.0123456"
    }
  }
}
```

---

## Case Studies

### Case Study 1: Complete Customer Journey

**User Story**: Sarah wants to book a hotel room for her vacation.

1. **Search for Rooms**
   - Sarah searches for rooms from Dec 15-20, 2024
   - System returns 3 available rooms

2. **Select Room**
   - Sarah adds Deluxe Suite (Room 101) to cart
   - Cart shows: 5 nights × $150/night = $750 total

3. **Login/Register**
   - Sarah logs in (or registers if new)
   - Receives JWT token

4. **Confirm Booking**
   - Sarah confirms booking with Stripe payment
   - Payment processed: `pi_1234567890`
   - Booking created with status "Pending"
   - Cart cleared

5. **View Booking**
   - Sarah views her profile: `/api/profile`
   - Sees booking in history with "Pending" status

6. **Admin Approval**
   - Admin approves booking: `PUT /api/adminbookings/1/approve`
   - Booking status changes to "Confirmed"

7. **Check Status**
   - Sarah checks booking status: `GET /api/bookings/1/status`
   - Sees "Confirmed" status

---

### Case Study 2: Booking Cancellation with Refund

**User Story**: Customer cancels booking and receives refund calculation.

1. **Customer Cancels**
   ```http
   PUT /api/bookings/1/cancel
   Authorization: Bearer {JWT_TOKEN}
   Content-Type: application/json
   
   {
     "reason": "Change of plans"
   }
   ```

2. **System Response**
   ```json
   {
     "message": "Booking 1 has been cancelled successfully.",
     "refundAmount": 600.00,
     "cancellationFee": 150.00,
     "status": "Cancelled"
   }
   ```

   **Refund Calculation**:
   - Cancelled 7+ days before check-in: 80% refund
   - Total: $750
   - Refund: $600 (80%)
   - Fee: $150 (20%)

---

### Case Study 3: Room Availability Conflict

**User Story**: Two customers try to book the same room simultaneously.

1. **Customer A** adds Room 101 to cart (Dec 15-20)
2. **Customer B** adds Room 101 to cart (Dec 15-20)
3. **Customer A** confirms booking first → Success
4. **Customer B** tries to confirm → **Error**: "Room is not available for the selected dates"
5. System clears Customer B's cart automatically

---

## Redundancy Analysis

### ⚠️ Potential Redundancies Identified

#### 1. Booking History Endpoints
- **`GET /api/profile/bookings`** - Returns `BookingHistoryDto[]`
- **`GET /api/bookings`** - Returns `BookingDto[]`

**Analysis**: 
- **NOT redundant** - Different DTOs serve different purposes:
  - `BookingHistoryDto` - Simplified view for profile (id, roomType, dates, amount, status)
  - `BookingDto` - Full booking details (includes payment info, cancellation details, etc.)

**Recommendation**: Keep both - they serve different use cases.

#### 2. Admin Booking Endpoints
- **`GET /api/admin/bookings`** - Returns all bookings (simple list)
- **`GET /api/adminbookings`** - Returns paged bookings with search

**Analysis**:
- **NOT redundant** - Different purposes:
  - `/api/admin/bookings` - Simple list for small datasets
  - `/api/adminbookings` - Paged with search for large datasets

**Recommendation**: Consider deprecating `/api/admin/bookings` in favor of the paged version, or add pagination to it.

#### 3. Booking Confirmation
- **`PUT /api/bookings/{id}/confirm`** - Admin confirms booking
- **`PUT /api/adminbookings/{id}/approve`** - Admin approves booking

**Analysis**:
- **POTENTIALLY REDUNDANT** - Both seem to do the same thing

**Recommendation**: 
- Keep `PUT /api/adminbookings/{id}/approve` (more RESTful)
- Consider deprecating `PUT /api/bookings/{id}/confirm` or make it customer-facing for self-confirmation

---

## Health Check Testing

### Test Scenarios

#### Scenario 1: Healthy System
```bash
curl http://localhost:5080/health
```

**Expected**: `200 OK` with `"status": "Healthy"`

#### Scenario 2: Database Down
1. Stop SQL Server
2. Call `/health`
3. **Expected**: `503 Service Unavailable` with `"status": "Unhealthy"`

#### Scenario 3: Application Monitoring
- Health check can be used by monitoring tools (Azure, AWS, etc.)
- Endpoint should respond quickly (< 1 second)

---

## Testing Checklist

### ✅ Week 3 Features
- [x] Booking confirmation with Stripe payment
- [x] PaymentIntentId saved to booking
- [x] User profile endpoint
- [x] Booking history endpoint
- [x] Authentication on all protected endpoints

### ✅ Week 4 Features
- [x] Health check endpoint (`/health`)
- [x] Structured logging (check Logs folder)
- [x] Session state for cart

### ✅ General Testing
- [x] All endpoints return correct status codes
- [x] Error handling works correctly
- [x] Authentication/Authorization enforced
- [x] No redundant endpoints (verified)
- [x] Database transactions work correctly

---

## Common Test Data

### Test Users
```json
{
  "customer": {
    "email": "customer@test.com",
    "password": "Password123!",
    "firstName": "John",
    "lastName": "Doe"
  },
  "admin": {
    "email": "admin@test.com",
    "password": "Admin123!",
    "firstName": "Admin",
    "lastName": "User"
  }
}
```

### Test Room Data
```json
{
  "roomType": {
    "name": "Deluxe Suite",
    "description": "Spacious suite with ocean view",
    "pricePerNight": 150.00,
    "maxOccupancy": 2
  },
  "room": {
    "roomNumber": "101",
    "roomTypeId": 1
  }
}
```

### Stripe Test Payment Methods
- **Success**: `pm_card_visa`
- **Decline**: `pm_card_chargeDeclined`
- **3D Secure**: `pm_card_authenticationRequired`

---

## Notes

1. **Session State**: Cart uses session cookies - ensure cookies are enabled in your HTTP client
2. **JWT Expiration**: Tokens expire after 2 hours (configurable in `appsettings.json`)
3. **Stripe Test Mode**: All payment processing uses Stripe test keys
4. **Database**: Ensure migrations are applied before testing
5. **Logging**: Check `Bookify.Api/Logs/` folder for application logs

---

## Troubleshooting

### Issue: 401 Unauthorized
- **Solution**: Ensure JWT token is included in `Authorization: Bearer {token}` header
- Check token expiration

### Issue: 500 Internal Server Error
- **Solution**: Check application logs in `Logs/` folder
- Verify database connection
- Check Stripe API keys

### Issue: Cart Not Persisting
- **Solution**: Ensure session middleware is enabled (it is in `Program.cs`)
- Check that cookies are being sent/received

### Issue: Payment Fails
- **Solution**: Use valid Stripe test payment method IDs
- Check Stripe secret key in `appsettings.json`

---

**Last Updated**: November 6, 2024
**Version**: 1.0
**Author**: Bookify Development Team

