# Dispatcher Feature - Complete API Documentation

## Overview

The Dispatcher feature enables US market transportation companies to manage multiple drivers and place bids on cargo orders on their behalf. This document provides accurate, implementation-based API documentation.

---

## Table of Contents

1. [Authentication & Authorization](#authentication--authorization)
2. [Dispatcher Management](#dispatcher-management)
3. [Driver Onboarding](#driver-onboarding)
4. [Driver Management](#driver-management)
5. [Bidding Operations](#bidding-operations)
6. [Order Management](#order-management)
7. [Document Types](#document-types)
8. [Data Models](#data-models)
9. [Enumerations](#enumerations)
10. [Error Responses](#error-responses)
11. [Business Logic](#business-logic)

---

## Authentication & Authorization

### Required Roles

All dispatcher endpoints require authentication with one of the following roles:
- `dispatcher`: Dispatcher user role
- `admin`: Administrative access

**Authorization Header:**
```
Authorization: Bearer {jwt_token}
```

---

## Dispatcher Management

### 1. Register New Dispatcher

Creates a new dispatcher account with user credentials.

**Endpoint:**
```http
POST /api/dispatcher/register
```

**Authentication:** Not required

**Request Body:**
```json
{
  "name": "Swift Transportation LLC",
  "emailAddress": "dispatch@swift.com",
  "phone": "+14805551234",
  "address": "123 Transport Ave, Phoenix, AZ 85001",
  "password": "SecurePass123!",
  "country": "US",
  "idCard": "https://storage.url/id-card.jpg",
  "profilePicture": "https://storage.url/profile.jpg",
  "bankName": "Chase Bank",
  "bankAccountNumber": "1234567890",
  "bankAccountName": "Swift Transportation LLC"
}
```

**Request Body Schema:**
| Field | Type | Required | Description |
|-------|------|----------|-------------|
| name | string | Yes | Company or dispatcher name |
| emailAddress | string | Yes | Email for login (becomes username) |
| phone | string | Yes | Contact phone number |
| address | string | Yes | Business address |
| password | string | Yes | Account password (must meet security requirements) |
| country | string | Yes | Country code (default: "US") |
| idCard | string | No | URL to ID card/business license document |
| profilePicture | string | No | URL to profile/company logo |
| bankName | string | No | Bank name for payments |
| bankAccountNumber | string | No | Bank account number |
| bankAccountName | string | No | Account holder name |

**Success Response (201):**
```json
{
  "message": "Dispatcher registered successfully",
  "data": true,
  "isSuccessful": true,
  "statusCode": 201
}
```

**Implementation Details:**
- Creates user account with email as username
- Assigns `dispatcher` role automatically
- User is marked as `EmailConfirmed: true` and `IsActive: true`
- Creates TruckOwner entity with `OwnerType: Dispatcher` and `CanBidOnBehalf: true`
- Creates BankDetails entity if bank information provided
- Sets initial status to `OwnersStatus.Pending`

---

### 2. Get Dispatcher Profile

Retrieves complete dispatcher profile information.

**Endpoint:**
```http
GET /api/dispatcher/{dispatcherId}
```

**Authentication:** Required (Roles: `dispatcher`, `admin`)

**Path Parameters:**
| Parameter | Type | Description |
|-----------|------|-------------|
| dispatcherId | string | Dispatcher's unique ID |

**Success Response (200):**
```json
{
  "message": "Success",
  "data": {
    "id": "550e8400-e29b-41d4-a716-446655440000",
    "name": "Swift Transportation LLC",
    "emailAddress": "dispatch@swift.com",
    "phone": "+14805551234",
    "address": "123 Transport Ave, Phoenix, AZ 85001",
    "idCardUrl": "https://storage.url/id-card.jpg",
    "profilePictureUrl": "https://storage.url/profile.jpg",
    "noOfTrucks": "12",
    "isProfileSetupComplete": true,
    "isAccountApproved": true,
    "hasBankDetails": true,
    "bankDetails": {
      "bankName": "Chase Bank",
      "bankAccountNumber": "1234567890",
      "bankAccountName": "Swift Transportation LLC"
    },
    "createdAt": "2024-01-15T10:30:00Z",
    "updatedAt": "2024-01-20T14:22:00Z",
    "ownersStatus": 1
  },
  "isSuccessful": true,
  "statusCode": 200
}
```

**Response Model: TruckOwnerResponseModel**
| Field | Type | Description |
|-------|------|-------------|
| id | string | Dispatcher unique identifier |
| name | string | Dispatcher/company name |
| emailAddress | string | Contact email |
| phone | string | Contact phone |
| address | string | Business address |
| idCardUrl | string | URL to ID/license document |
| profilePictureUrl | string | URL to profile picture |
| noOfTrucks | string | Count of trucks (drivers) managed |
| isProfileSetupComplete | boolean | True if ID card and profile picture uploaded |
| isAccountApproved | boolean | True if OwnersStatus == Approved |
| hasBankDetails | boolean | True if bank details exist |
| bankDetails | object | Bank account information |
| createdAt | datetime | Account creation timestamp |
| updatedAt | datetime | Last update timestamp |
| ownersStatus | enum | Status: 0=Pending, 1=Approved, 2=NotApproved, 3=Blocked |

**Error Response (404):**
```json
{
  "message": "Truck owner not found",
  "data": null,
  "isSuccessful": false,
  "statusCode": 404
}
```

---

## Driver Onboarding

The driver onboarding process consists of 4 sequential steps:

1. **Create Driver** - Basic information and account creation
2. **Upload Documents** - Required documentation by country
3. **Add Truck** - Vehicle registration and details
4. **Complete Onboarding** - Final validation and submission for approval

### 3. Create Driver Account

Creates a new driver under dispatcher management.

**Endpoint:**
```http
POST /api/dispatcher/{dispatcherId}/drivers
```

**Authentication:** Required (Roles: `dispatcher`, `admin`)

**Path Parameters:**
| Parameter | Type | Description |
|-----------|------|-------------|
| dispatcherId | string | Dispatcher's unique ID |

**Request Body:**
```json
{
  "name": "John Doe",
  "email": "john.doe@example.com",
  "number": "+14805559876",
  "address": "456 Driver Lane, Phoenix, AZ 85002",
  "password": "DriverPass123!",
  "dispatcherId": "550e8400-e29b-41d4-a716-446655440000",
  "commissionPercentage": 15.5,
  "isDispatcherManaged": true
}
```

**Request Body Schema:**
| Field | Type | Required | Description |
|-------|------|----------|-------------|
| name | string | Yes | Driver's full name |
| email | string | Yes | Driver's email (becomes username) |
| number | string | Yes | Driver's phone number |
| address | string | Yes | Driver's address |
| password | string | Yes | Driver's account password |
| dispatcherId | string | Yes | Managing dispatcher ID |
| commissionPercentage | decimal | Yes | Commission rate (0-50%) |
| isDispatcherManaged | boolean | Yes | Should be true |

**Success Response (200):**
```json
{
  "message": "Driver created successfully. Complete onboarding by uploading documents and adding truck.",
  "data": "Driver created successfully. Complete onboarding by uploading documents and adding truck.",
  "isSuccessful": true,
  "statusCode": 200
}
```

**Implementation Details:**
- Creates user account with email as username
- Assigns `driver` role
- Inherits country from dispatcher
- Sets `OnboardingStatus: OboardingPending` (0)
- Sets `OwnershipType: DispatcherManaged` (2)
- Creates initial commission structure record
- Links driver to dispatcher via `ManagedByDispatcherId` and `TruckOwnerId`

**Error Response (404):**
```json
{
  "message": "Dispatcher not found",
  "data": null,
  "isSuccessful": false,
  "statusCode": 404
}
```

**Error Response (400):**
```json
{
  "message": "Failed to create user: Email 'john.doe@example.com' is already taken.",
  "data": null,
  "isSuccessful": false,
  "statusCode": 400
}
```

---

### 4. Upload Driver Documents

Uploads required documents for driver verification.

**Endpoint:**
```http
POST /api/dispatcher/{dispatcherId}/drivers/{driverId}/documents
```

**Authentication:** Required (Roles: `dispatcher`, `admin`)

**Path Parameters:**
| Parameter | Type | Description |
|-----------|------|-------------|
| dispatcherId | string | Dispatcher's unique ID |
| driverId | string | Driver's unique ID |

**Request Body:**
```json
{
  "dispatcherId": "550e8400-e29b-41d4-a716-446655440000",
  "driverId": "660e8400-e29b-41d4-a716-446655440001",
  "documents": [
    {
      "documentTypeId": "doc-type-cdl-id",
      "fileUrl": "https://storage.url/cdl-front.jpg"
    },
    {
      "documentTypeId": "doc-type-medical-id",
      "fileUrl": "https://storage.url/medical-cert.pdf"
    }
  ],
  "profilePictureUrl": "https://storage.url/driver-photo.jpg",
  "dotNumber": "DOT1234567",
  "mcNumber": "MC123456"
}
```

**Request Body Schema:**
| Field | Type | Required | Description |
|-------|------|----------|-------------|
| dispatcherId | string | Yes | Managing dispatcher ID |
| driverId | string | Yes | Driver ID |
| documents | array | Yes | Array of document objects |
| documents[].documentTypeId | string | Yes | Document type identifier |
| documents[].fileUrl | string | Yes | URL to uploaded document |
| profilePictureUrl | string | No | URL to driver's photo |
| dotNumber | string | No | DOT number (US drivers) |
| mcNumber | string | No | MC number (US drivers) |

**Success Response (200):**
```json
{
  "message": "Driver documents uploaded successfully",
  "data": true,
  "isSuccessful": true,
  "statusCode": 200
}
```

**Implementation Details:**
- Creates DriverDocument record for each document
- Updates driver's `PassportFile` with profilePictureUrl
- Updates driver's `DotNumber` and `McNumber` if provided
- Changes `OnboardingStatus: OnboardingInReview` (1)

**Error Response (400):**
```json
{
  "message": "Driver not found or not managed by this dispatcher",
  "data": false,
  "isSuccessful": false,
  "statusCode": 400
}
```

---

### 5. Add Truck for Driver

Registers a truck for the driver.

**Endpoint:**
```http
POST /api/dispatcher/{dispatcherId}/drivers/{driverId}/truck
```

**Authentication:** Required (Roles: `dispatcher`, `admin`)

**Path Parameters:**
| Parameter | Type | Description |
|-----------|------|-------------|
| dispatcherId | string | Dispatcher's unique ID |
| driverId | string | Driver's unique ID |

**Request Body:**
```json
{
  "driverId": "660e8400-e29b-41d4-a716-446655440001",
  "dispatcherId": "550e8400-e29b-41d4-a716-446655440000",
  "plateNumber": "AZ-ABC-1234",
  "truckName": "Unit 42",
  "truckCapacity": "53ft",
  "truckType": "Dry Van",
  "truckLicenseExpiryDate": "2025-12-31",
  "roadWorthinessExpiryDate": "2025-12-31",
  "insuranceExpiryDate": "2025-12-31",
  "documents": [
    "https://storage.url/truck-registration.pdf",
    "https://storage.url/insurance-card.pdf"
  ],
  "externalTruckPictureUrl": "https://storage.url/truck-external.jpg",
  "cargoSpacePictureUrl": "https://storage.url/truck-interior.jpg"
}
```

**Request Body Schema:**
Inherits from `DriverAddTruckRequestModel` plus:
| Field | Type | Required | Description |
|-------|------|----------|-------------|
| driverId | string | Yes | Driver ID |
| dispatcherId | string | Yes | Dispatcher ID |
| plateNumber | string | Yes | License plate number |
| truckName | string | Yes | Truck identifier/name |
| truckCapacity | string | Yes | Cargo capacity |
| truckType | string | Yes | Type of truck |
| truckLicenseExpiryDate | string | Yes | License expiry (ISO 8601) |
| roadWorthinessExpiryDate | string | Yes | Inspection expiry |
| insuranceExpiryDate | string | Yes | Insurance expiry |
| documents | array | No | URLs to truck documents |
| externalTruckPictureUrl | string | No | External photo URL |
| cargoSpacePictureUrl | string | No | Interior/cargo photo URL |

**Success Response (200):**
```json
{
  "message": "Truck added successfully",
  "data": true,
  "isSuccessful": true,
  "statusCode": 200
}
```

**Implementation Details:**
- Uses existing `ITruckService.AddDriverOwnedTruck()` method
- Changes `OnboardingStatus: OnboardingCompleted` (2) upon success
- Links truck to driver via `DriverId` field

**Error Response (404):**
```json
{
  "message": "Driver not found or not managed by this dispatcher",
  "data": false,
  "isSuccessful": false,
  "statusCode": 404
}
```

---

### 6. Complete Driver Onboarding

Validates all requirements and submits for admin approval.

**Endpoint:**
```http
POST /api/dispatcher/{dispatcherId}/drivers/{driverId}/complete-onboarding
```

**Authentication:** Required (Roles: `dispatcher`, `admin`)

**Path Parameters:**
| Parameter | Type | Description |
|-----------|------|-------------|
| dispatcherId | string | Dispatcher's unique ID |
| driverId | string | Driver's unique ID |

**Request Body:**
```json
{
  "dispatcherId": "550e8400-e29b-41d4-a716-446655440000",
  "driverId": "660e8400-e29b-41d4-a716-446655440001",
  "confirmAllDocumentsUploaded": true,
  "confirmTruckAdded": true
}
```

**Request Body Schema:**
| Field | Type | Required | Description |
|-------|------|----------|-------------|
| dispatcherId | string | Yes | Managing dispatcher ID |
| driverId | string | Yes | Driver ID |
| confirmAllDocumentsUploaded | boolean | Yes | Confirmation flag |
| confirmTruckAdded | boolean | Yes | Confirmation flag |

**Success Response (200):**
```json
{
  "message": "Driver onboarding completed and submitted for admin approval",
  "data": true,
  "isSuccessful": true,
  "statusCode": 200
}
```

**Validation Rules:**
1. All required documents for driver's country must be uploaded
2. Driver must have a truck assigned
3. Driver must be managed by the requesting dispatcher

**Implementation Details:**
- Validates required documents using `ValidateRequiredDocuments()` method
- Checks if driver has associated truck
- Changes `OnboardingStatus: OnboardingInReview` (1)
- Awaits admin approval to become active

**Error Response (404):**
```json
{
  "message": "Driver not found or not managed by this dispatcher",
  "data": false,
  "isSuccessful": false,
  "statusCode": 404
}
```

**Error Response (400 - Missing Documents):**
```json
{
  "message": "Required documents not uploaded",
  "data": false,
  "isSuccessful": false,
  "statusCode": 400
}
```

**Error Response (400 - No Truck):**
```json
{
  "message": "Truck not added for driver",
  "data": false,
  "isSuccessful": false,
  "statusCode": 400
}
```

---

## Driver Management

### 7. Get Dispatcher's Drivers

Retrieves all drivers managed by the dispatcher.

**Endpoint:**
```http
GET /api/dispatcher/{dispatcherId}/drivers
```

**Authentication:** Required (Roles: `dispatcher`, `admin`)

**Path Parameters:**
| Parameter | Type | Description |
|-----------|------|-------------|
| dispatcherId | string | Dispatcher's unique ID |

**Success Response (200):**
```json
{
  "message": "drivers retrieved successfully",
  "data": [
    {
      "id": "660e8400-e29b-41d4-a716-446655440001",
      "name": "John Doe",
      "phone": "+14805559876",
      "emailAddress": "john.doe@example.com",
      "truckId": "truck-uuid-here",
      "userId": "user-uuid-here",
      "driversLicence": "https://storage.url/cdl-front.jpg",
      "dotNumber": "DOT1234567",
      "mcNumber": "MC123456",
      "truck": null,
      "bankAccounts": [],
      "passportFile": "https://storage.url/driver-photo.jpg",
      "country": "US",
      "isActive": true,
      "onboardingStatus": 1,
      "hasAcceptedTerms": false
    }
  ],
  "isSuccessful": true,
  "statusCode": 200
}
```

**Response Model: DriverResponseModel**
| Field | Type | Description |
|-------|------|-------------|
| id | string | Driver's unique ID |
| name | string | Driver's full name |
| phone | string | Contact phone |
| emailAddress | string | Contact email |
| truckId | string | Associated truck ID |
| userId | string | User account ID |
| driversLicence | string | URL to driver's license |
| dotNumber | string | DOT number |
| mcNumber | string | MC number |
| truck | object | Truck details (if loaded) |
| bankAccounts | array | Bank account information |
| passportFile | string | Profile photo URL |
| country | string | Country code |
| isActive | boolean | Active status |
| onboardingStatus | int | 0=Pending, 1=InReview, 2=Completed |
| hasAcceptedTerms | boolean | Terms acceptance status |

**Query Implementation:**
- Filters by `ManagedByDispatcherId`
- Includes Truck, BankAccounts, and CommissionStructures
- Returns full DriverResponseModel for each driver

---

### 8. Update Driver Commission

Updates commission percentage for a driver.

**Endpoint:**
```http
PUT /api/dispatcher/drivers/{driverId}/commission
```

**Authentication:** Required (Roles: `dispatcher`, `admin`)

**Path Parameters:**
| Parameter | Type | Description |
|-----------|------|-------------|
| driverId | string | Driver's unique ID |

**Request Body:**
```json
{
  "driverId": "660e8400-e29b-41d4-a716-446655440001",
  "dispatcherId": "550e8400-e29b-41d4-a716-446655440000",
  "newCommissionPercentage": 20.0
}
```

**Request Body Schema:**
| Field | Type | Required | Description |
|-------|------|----------|-------------|
| driverId | string | Yes | Driver ID |
| dispatcherId | string | Yes | Dispatcher ID |
| newCommissionPercentage | decimal | Yes | New rate (0-50) |

**Success Response (200):**
```json
{
  "message": "Commission updated successfully",
  "data": true,
  "isSuccessful": true,
  "statusCode": 200
}
```

**Implementation Details:**
- Validates percentage is between 0 and 50
- Ends current commission structure (sets `IsActive: false`, `EffectiveTo: now`)
- Creates new commission structure with `EffectiveFrom: now`
- Maintains historical commission records

**Error Response (400):**
```json
{
  "message": "Commission percentage must be between 0 and 50",
  "data": false,
  "isSuccessful": false,
  "statusCode": 400
}
```

**Error Response (404):**
```json
{
  "message": "No active commission structure found for this driver-dispatcher pair",
  "data": false,
  "isSuccessful": false,
  "statusCode": 404
}
```

---

## Bidding Operations

### 9. Create Bid on Behalf of Driver

Places a bid for a cargo order on behalf of a managed driver.

**Endpoint:**
```http
POST /api/dispatcher/bids
```

**Authentication:** Required (Roles: `dispatcher`, `admin`)

**Request Body:**
```json
{
  "orderId": "order-uuid-here",
  "driverId": "660e8400-e29b-41d4-a716-446655440001",
  "dispatcherId": "550e8400-e29b-41d4-a716-446655440000",
  "amount": 2500.00,
  "notes": "Experienced driver, on-time delivery guaranteed"
}
```

**Request Body Schema:**
| Field | Type | Required | Description |
|-------|------|----------|-------------|
| orderId | string | Yes | Cargo order ID to bid on |
| driverId | string | Yes | Driver placing the bid |
| dispatcherId | string | Yes | Dispatcher submitting bid |
| amount | decimal | Yes | Bid amount in USD |
| notes | string | No | Additional notes |

**Success Response (200):**
```json
{
  "message": "Bid created successfully",
  "data": true,
  "isSuccessful": true,
  "statusCode": 200
}
```

**Business Logic:**
1. **Validation:**
   - Dispatcher must manage the driver
   - Driver must have `OnboardingStatus: OnboardingCompleted` (2)
   - Driver must have an assigned truck
   - Order must have `Status: OpenForBidding`
   - Driver cannot have existing bid on this order

2. **Commission Calculation:**
   ```
   System Fee = Bid Amount × 0.20 (20%)
   Net Amount = Bid Amount - System Fee
   Dispatcher Commission = Net Amount × (Commission Percentage / 100)
   Driver Earnings = Net Amount - Dispatcher Commission
   ```

3. **Bid Creation:**
   - Bid references driver's truck (not dispatcher)
   - Sets `SubmitterType: Dispatcher` (1)
   - Records `SubmittedByDispatcherId`
   - Records `DispatcherCommissionAmount`
   - Prepends notes with "Bid submitted by dispatcher on behalf of driver."

4. **Notification:**
   - Sends notification to cargo owner about new bid

**Error Response (404):**
```json
{
  "message": "Driver not found or not managed by dispatcher",
  "data": false,
  "isSuccessful": false,
  "statusCode": 404
}
```

**Error Response (400 - Onboarding):**
```json
{
  "message": "Driver has not completed onboarding",
  "data": false,
  "isSuccessful": false,
  "statusCode": 400
}
```

**Error Response (400 - No Truck):**
```json
{
  "message": "Driver does not have a truck assigned",
  "data": false,
  "isSuccessful": false,
  "statusCode": 400
}
```

**Error Response (400 - Order Status):**
```json
{
  "message": "Order is not open for bidding",
  "data": false,
  "isSuccessful": false,
  "statusCode": 400
}
```

**Error Response (400 - Duplicate):**
```json
{
  "message": "Driver already has a bid for this order",
  "data": false,
  "isSuccessful": false,
  "statusCode": 400
}
```

---

### 10. Get Available Orders

Retrieves cargo orders available for bidding in dispatcher's country.

**Endpoint:**
```http
GET /api/dispatcher/{dispatcherId}/orders/available
```

**Authentication:** Required (Roles: `dispatcher`, `admin`)

**Path Parameters:**
| Parameter | Type | Description |
|-----------|------|-------------|
| dispatcherId | string | Dispatcher's unique ID |

**Success Response (200):**
```json
{
  "message": "Orders retrieved successfully",
  "data": [
    {
      "id": "order-uuid-here",
      "cargoOwnerId": "owner-uuid-here",
      "cargoOwner": {
        "name": "ABC Shipping Co"
      },
      "driver": null,
      "pickupLocation": "Phoenix, AZ",
      "pickupLocationLat": "33.4484",
      "pickupLocationLong": "-112.0740",
      "deliveryLocation": "Los Angeles, CA",
      "deliveryLocationLat": "34.0522",
      "deliveryLocationLong": "-118.2437",
      "status": 1,
      "items": null,
      "bids": [
        {
          "id": null,
          "orderId": null,
          "truckId": null,
          "amount": 2300.00,
          "status": null,
          "createdAt": "0001-01-01T00:00:00",
          "updatedAt": "0001-01-01T00:00:00",
          "truck": null,
          "driver": null,
          "driverRating": null
        }
      ],
      "acceptedBid": null,
      "pickupDateTime": "2024-02-15T08:00:00Z",
      "actualPickupDateTime": null,
      "deliveryDateTime": "2024-02-17T18:00:00Z",
      "totalAmount": 0,
      "systemFee": 0,
      "tax": 0,
      "invoiceNumber": null,
      "paymentStatus": 0,
      "paymentDueDate": null,
      "documents": [],
      "deliveryDocuments": [],
      "totalWeight": 0,
      "totalVolume": 0,
      "hasFragileItems": false,
      "itemTypeBreakdown": null,
      "specialHandlingRequirements": null,
      "driverBidInfo": null,
      "nextAction": null,
      "acceptedAmount": 0,
      "createdAt": "2024-01-20T10:00:00Z",
      "updatedAt": "2024-01-20T10:00:00Z"
    }
  ],
  "isSuccessful": true,
  "statusCode": 200
}
```

**Response Model: CargoOrderResponseModel[]**

Full `CargoOrderResponseModel` with the following populated fields:
| Field | Type | Description |
|-------|------|-------------|
| id | string | Order unique ID |
| cargoOwnerId | string | Order creator ID |
| cargoOwner | object | Cargo owner info with name |
| pickupLocation | string | Pickup address |
| deliveryLocation | string | Delivery address |
| status | enum | Order status (1 = OpenForBidding) |
| pickupDateTime | datetime | Scheduled pickup |
| deliveryDateTime | datetime | Expected delivery |
| createdAt | datetime | Order creation time |
| bids | array | Existing bids (amount only) |

**Query Details:**
- Filters: `Status == OpenForBidding` AND `CountryCode == Dispatcher.Country`
- Includes: Items, CargoOwner, Bids
- Ordered by: CreatedAt descending
- No pagination (returns all matching orders)

**Error Response (404):**
```json
{
  "message": "Dispatcher not found",
  "data": [],
  "isSuccessful": false,
  "statusCode": 404
}
```

---

## Order Management

### 11. Upload Manifest on Behalf of Driver

Uploads manifest documents for an accepted order.

**Endpoint:**
```http
POST /api/dispatcher/orders/{orderId}/manifest
```

**Authentication:** Required (Roles: `dispatcher`, `admin`)

**Path Parameters:**
| Parameter | Type | Description |
|-----------|------|-------------|
| orderId | string | Cargo order unique ID |

**Request Body:**
```json
{
  "orderId": "order-uuid-here",
  "dispatcherId": "550e8400-e29b-41d4-a716-446655440000",
  "driverId": "660e8400-e29b-41d4-a716-446655440001",
  "manifestUrls": [
    "https://storage.url/manifest-page1.pdf",
    "https://storage.url/manifest-page2.pdf"
  ]
}
```

**Request Body Schema:**
| Field | Type | Required | Description |
|-------|------|----------|-------------|
| orderId | string | Yes | Order ID |
| dispatcherId | string | Yes | Dispatcher ID |
| driverId | string | Yes | Driver ID |
| manifestUrls | array | Yes | URLs to manifest documents |

**Success Response (200):**
```json
{
  "message": "Manifest uploaded successfully on behalf of driver",
  "data": true,
  "isSuccessful": true,
  "statusCode": 200
}
```

**Implementation Details:**
- Validates dispatcher manages the driver
- Validates order belongs to driver's truck
- Sets order `Documents` property to manifestUrls
- Changes order `Status: InTransit`
- Sends notification to cargo owner

**Error Response (404):**
```json
{
  "message": "Driver not found or not managed by this dispatcher",
  "data": false,
  "isSuccessful": false,
  "statusCode": 404
}
```

---

### 12. Complete Delivery on Behalf of Driver

Submits delivery completion documents.

**Endpoint:**
```http
POST /api/dispatcher/orders/{orderId}/delivery
```

**Authentication:** Required (Roles: `dispatcher`, `admin`)

**Path Parameters:**
| Parameter | Type | Description |
|-----------|------|-------------|
| orderId | string | Cargo order unique ID |

**Request Body:**
```json
{
  "orderId": "order-uuid-here",
  "dispatcherId": "550e8400-e29b-41d4-a716-446655440000",
  "driverId": "660e8400-e29b-41d4-a716-446655440001",
  "deliveryDocumentUrls": [
    "https://storage.url/pod-signed.pdf",
    "https://storage.url/delivery-receipt.pdf"
  ]
}
```

**Request Body Schema:**
| Field | Type | Required | Description |
|-------|------|----------|-------------|
| orderId | string | Yes | Order ID |
| dispatcherId | string | Yes | Dispatcher ID |
| driverId | string | Yes | Driver ID |
| deliveryDocumentUrls | array | Yes | Delivery proof documents |

**Success Response (200):**
```json
{
  "message": "Delivery completed successfully",
  "data": true,
  "isSuccessful": true,
  "statusCode": 200
}
```

**Implementation Details:**
- Validates dispatcher manages the driver
- Validates order belongs to driver's truck
- Sets order `DeliveryDocuments` property
- Updates order status based on business logic
- May trigger payment processing

---

## Document Types

### 13. Get Document Types by Country

Retrieves required document types for driver onboarding.

**Endpoint:**
```http
GET /api/dispatcher/document-types/{country}
```

**Authentication:** Required (Roles: `dispatcher`, `admin`)

**Path Parameters:**
| Parameter | Type | Description |
|-----------|------|-------------|
| country | string | Country code (e.g., "US", "NG") |

**Success Response (200):**
```json
{
  "message": "Document types retrieved successfully",
  "data": [
    {
      "id": "doc-type-cdl-id",
      "country": "US",
      "entityType": "Driver",
      "name": "Commercial Driver's License",
      "isRequired": true,
      "description": "Valid CDL Class A or B for commercial vehicle operation",
      "hasTemplate": false,
      "templateUrl": null
    },
    {
      "id": "doc-type-medical-id",
      "country": "US",
      "entityType": "Driver",
      "name": "DOT Medical Certificate",
      "isRequired": true,
      "description": "Current DOT medical examiner's certificate",
      "hasTemplate": false,
      "templateUrl": null
    }
  ],
  "isSuccessful": true,
  "statusCode": 200
}
```

**Response Model: DocumentTypeResponseModel**
| Field | Type | Description |
|-------|------|-------------|
| id | string | Document type ID |
| country | string | Country code |
| entityType | string | Always "Driver" |
| name | string | Document type name |
| isRequired | boolean | Required for onboarding |
| description | string | Document description |
| hasTemplate | boolean | Template availability |
| templateUrl | string | Template download URL |

**Implementation Details:**
- EntityType is hardcoded to "Driver"
- Queries `DocumentTypes` table with country and entityType filters

---

## Data Models

### ApiResponseModel<T>

Standard response wrapper for all API endpoints.

```json
{
  "message": "string",
  "data": T,
  "isSuccessful": boolean,
  "statusCode": integer
}
```

### TruckOwnerResponseModel

Complete dispatcher/truck owner profile.

| Field | Type | Description |
|-------|------|-------------|
| id | string | Unique identifier |
| name | string | Company/dispatcher name |
| emailAddress | string | Contact email |
| phone | string | Contact phone |
| address | string | Business address |
| idCardUrl | string | ID/license document URL |
| profilePictureUrl | string | Profile/logo URL |
| noOfTrucks | string | Count of managed trucks/drivers |
| isProfileSetupComplete | boolean | Profile completion status |
| isAccountApproved | boolean | Approval status |
| hasBankDetails | boolean | Bank details existence |
| bankDetails | BankDetailsResponseModel | Bank information |
| createdAt | datetime | Creation timestamp |
| updatedAt | datetime | Last update timestamp |
| ownersStatus | OwnersStatus | Account status enum |

### DriverResponseModel

Driver profile information.

| Field | Type | Description |
|-------|------|-------------|
| id | string | Driver unique ID |
| name | string | Full name |
| phone | string | Phone number |
| emailAddress | string | Email address |
| truckId | string | Associated truck ID |
| userId | string | User account ID |
| driversLicence | string | License document URL |
| dotNumber | string | DOT number |
| mcNumber | string | MC number |
| truck | AllTruckResponseModel | Truck details |
| bankAccounts | DriverBankAccountResponseModel[] | Bank accounts |
| passportFile | string | Profile photo URL |
| country | string | Country code |
| isActive | boolean | Active status |
| onboardingStatus | DriverOnboardingStatus | Onboarding status |
| hasAcceptedTerms | boolean | Terms acceptance |

### CargoOrderResponseModel

Cargo order details.

| Field | Type | Description |
|-------|------|-------------|
| id | string | Order unique ID |
| cargoOwnerId | string | Owner ID |
| cargoOwner | CargoOwnerResponseModel | Owner details |
| driver | DriverProfileResponseModel | Assigned driver |
| pickupLocation | string | Pickup address |
| pickupLocationLat | string | Latitude |
| pickupLocationLong | string | Longitude |
| deliveryLocation | string | Delivery address |
| deliveryLocationLat | string | Latitude |
| deliveryLocationLong | string | Longitude |
| status | CargoOrderStatus | Order status |
| items | CargoOrderItemResponseModel[] | Cargo items |
| bids | BidResponseModel[] | All bids |
| acceptedBid | BidResponseModel | Winning bid |
| pickupDateTime | datetime | Scheduled pickup |
| actualPickupDateTime | datetime | Actual pickup |
| deliveryDateTime | datetime | Expected delivery |
| totalAmount | decimal | Total order amount |
| systemFee | decimal | Platform fee |
| tax | decimal | Tax amount |
| invoiceNumber | string | Invoice reference |
| paymentStatus | PaymentStatus | Payment status |
| paymentDueDate | datetime | Due date |
| documents | string[] | Manifest documents |
| deliveryDocuments | string[] | POD documents |
| totalWeight | decimal | Total weight |
| totalVolume | decimal | Total volume |
| hasFragileItems | boolean | Fragile flag |
| itemTypeBreakdown | Dictionary | Item type counts |
| specialHandlingRequirements | string[] | Special instructions |
| driverBidInfo | DriverBidInfo | Driver's bid info |
| nextAction | string | Next action required |
| acceptedAmount | decimal | Accepted bid amount |
| createdAt | datetime | Creation time |
| updatedAt | datetime | Update time |

### BidResponseModel

Bid information.

| Field | Type | Description |
|-------|------|-------------|
| id | string | Bid unique ID |
| orderId | string | Associated order ID |
| truckId | string | Truck ID |
| amount | decimal | Bid amount |
| status | string | Bid status |
| createdAt | datetime | Submission time |
| updatedAt | datetime | Last update |
| truck | AllTruckResponseModel | Truck details |
| driver | DriverProfileResponseModel | Driver details |
| driverRating | DriverRatingSummaryModel | Rating info |

### BankDetailsResponseModel

Bank account information.

| Field | Type | Description |
|-------|------|-------------|
| bankName | string | Bank name |
| bankAccountNumber | string | Account number |
| bankAccountName | string | Account holder name |

### DocumentTypeResponseModel

Document type specification.

| Field | Type | Description |
|-------|------|-------------|
| id | string | Document type ID |
| country | string | Country code |
| entityType | string | Entity type (Driver) |
| name | string | Document name |
| isRequired | boolean | Required flag |
| description | string | Description |
| hasTemplate | boolean | Template available |
| templateUrl | string | Template URL |

---

## Enumerations

### DriverOnboardingStatus

Driver onboarding progress states.

| Value | Name | Description |
|-------|------|-------------|
| 0 | OboardingPending | Initial state, onboarding not started |
| 1 | OnboardingInReview | Documents uploaded, awaiting approval |
| 2 | OnboardingCompleted | Approved and active |

### OwnersStatus

Account approval status.

| Value | Name | Description |
|-------|------|-------------|
| 0 | Pending | Awaiting approval |
| 1 | Approved | Active and approved |
| 2 | NotApproved | Rejected |
| 3 | Blocked | Account blocked |

### TruckOwnerType

Owner type classification.

| Value | Name | Description |
|-------|------|-------------|
| 0 | TruckOwner | Traditional truck owner |
| 1 | Transporter | Nigerian market transporter |
| 2 | Dispatcher | US market dispatcher |

### DriverOwnershipType

Driver employment relationship.

| Value | Name | Description |
|-------|------|-------------|
| 0 | Independent | Owner-operator |
| 1 | TruckOwnerEmployee | Employee of truck owner |
| 2 | DispatcherManaged | Managed by dispatcher |

### CargoOrderStatus

Order lifecycle states.

| Value | Name |
|-------|------|
| 0 | Draft |
| 1 | OpenForBidding |
| 2 | BiddingInProgress |
| 3 | DriverSelected |
| 4 | DriverAcknowledged |
| 5 | ReadyForPickup |
| 6 | InTransit |
| 7 | Delivered |
| 8 | PaymentPending |
| 9 | PaymentComplete |
| 10 | PaymentOverdue |
| 11 | Cancelled |

---

## Error Responses

### Standard Error Format

All error responses follow this structure:

```json
{
  "message": "Error description",
  "data": null | false,
  "isSuccessful": false,
  "statusCode": 400 | 404 | 500
}
```

### Common Error Codes

| Status Code | Description | Common Causes |
|-------------|-------------|---------------|
| 400 | Bad Request | Validation errors, invalid data, business rule violations |
| 401 | Unauthorized | Missing or invalid authentication token |
| 403 | Forbidden | Insufficient permissions for requested resource |
| 404 | Not Found | Resource doesn't exist or user lacks access |
| 500 | Internal Server Error | Unexpected server-side errors |

### Validation Errors

**Commission Out of Range:**
```json
{
  "message": "Commission percentage must be between 0 and 50",
  "data": false,
  "isSuccessful": false,
  "statusCode": 400
}
```

**Duplicate Bid:**
```json
{
  "message": "Driver already has a bid for this order",
  "data": false,
  "isSuccessful": false,
  "statusCode": 400
}
```

**Onboarding Incomplete:**
```json
{
  "message": "Driver has not completed onboarding",
  "data": false,
  "isSuccessful": false,
  "statusCode": 400
}
```

**Missing Required Documents:**
```json
{
  "message": "Required documents not uploaded",
  "data": false,
  "isSuccessful": false,
  "statusCode": 400
}
```

**No Truck Assigned:**
```json
{
  "message": "Truck not added for driver",
  "data": false,
  "isSuccessful": false,
  "statusCode": 400
}
```

**Order Not Available:**
```json
{
  "message": "Order is not open for bidding",
  "data": false,
  "isSuccessful": false,
  "statusCode": 400
}
```

### Authorization Errors

**Dispatcher Validation:**
```json
{
  "message": "Driver not found or not managed by this dispatcher",
  "data": false,
  "isSuccessful": false,
  "statusCode": 404
}
```

**Resource Not Found:**
```json
{
  "message": "Dispatcher not found",
  "data": null,
  "isSuccessful": false,
  "statusCode": 404
}
```

---

## Business Logic

### Commission Structure

**Commission Calculation:**
```
Total Bid Amount: $2,500.00
├─ System Fee (20%): $500.00
└─ Net Amount: $2,000.00
   ├─ Dispatcher Commission (15.5% of net): $310.00
   └─ Driver Earnings: $1,690.00
```

**Formula:**
```csharp
systemFee = bidAmount * 0.20m;
netAmount = bidAmount - systemFee;
dispatcherCommission = netAmount * (commissionPercentage / 100);
driverEarnings = netAmount - dispatcherCommission;
```

### Commission Management

- **Range:** 0% to 50% of net amount (after system fee)
- **Historical Tracking:** All commission changes are recorded
- **Active Structure:** Only one active commission per driver-dispatcher pair
- **Update Process:**
  1. Set current structure `IsActive = false`
  2. Set current structure `EffectiveTo = DateTime.UtcNow`
  3. Create new structure with `EffectiveFrom = DateTime.UtcNow`
  4. Set new structure `IsActive = true`

### Onboarding Workflow

**Sequential Steps:**

1. **Create Driver (POST /drivers)**
   - Status: `OboardingPending` (0)
   - Creates user account and driver record
   - Creates initial commission structure

2. **Upload Documents (POST /drivers/{id}/documents)**
   - Status: `OnboardingInReview` (1)
   - Validates all required documents uploaded
   - Updates DOT/MC numbers if provided

3. **Add Truck (POST /drivers/{id}/truck)**
   - Status: `OnboardingCompleted` (2)
   - Registers truck and links to driver

4. **Complete Onboarding (POST /drivers/{id}/complete-onboarding)**
   - Status: `OnboardingInReview` (1)
   - Final validation of all requirements
   - Submits for admin approval

**Document Validation:**
- Checks `DocumentTypes` table for required documents
- Filters by country and entityType="Driver"
- Validates `isRequired=true` documents are uploaded
- All required documents must be present to complete onboarding

### Bidding Rules

**Eligibility Requirements:**
1. Dispatcher must manage the driver (`ManagedByDispatcherId` match)
2. Driver must have `OnboardingStatus == OnboardingCompleted` (2)
3. Driver must have a truck assigned (`TruckId != null`)
4. Order must have `Status == OpenForBidding` (1)
5. Driver cannot have existing bid on the order (one bid per truck per order)

**Bid Creation:**
- Bid is associated with driver's `TruckId` (not dispatcher)
- Sets `SubmitterType = Dispatcher` (1)
- Records `SubmittedByDispatcherId`
- Calculates and stores `DispatcherCommissionAmount`
- Prepends notes: "Bid submitted by dispatcher on behalf of driver. {originalNotes}"
- Sends notification to cargo owner

**Bid Visibility:**
- **Cargo Owners:** See driver information, truck details, and bid amount
  - Dispatcher information is hidden (`SubmittedByDispatcherId` not exposed)
  - `IsFromDispatcher` flag indicates dispatcher submission
- **Admins:** See full bid details including dispatcher info and commission amounts

### Order Filtering

**Available Orders (GET /orders/available):**
- Filter: `Status == OpenForBidding` AND `CountryCode == Dispatcher.Country`
- Includes: Items, CargoOwner, existing Bids
- Order: CreatedAt descending (newest first)
- No pagination applied

### Data Inheritance

**Driver Creation:**
- Inherits `Country` from dispatcher
- Links via `ManagedByDispatcherId` and `TruckOwnerId`
- Sets `OwnershipType = DispatcherManaged` (2)

**Profile Completion:**
- Dispatcher profile is complete when: `IdCardUrl != null` AND `ProfilePictureUrl != null`
- Driver is active when: `OnboardingStatus == OnboardingCompleted` (2) AND admin approved

---

## Integration Notes

### Authentication Flow

1. Dispatcher registers via `POST /api/dispatcher/register`
2. Dispatcher logs in via standard login endpoint (receives JWT token)
3. Token includes `dispatcher` role claim
4. All subsequent requests include: `Authorization: Bearer {token}`

### Typical Workflow

**Dispatcher Onboarding:**
1. Register dispatcher account
2. Upload ID card and profile picture
3. Add bank details
4. Wait for admin approval

**Driver Onboarding (per driver):**
1. Create driver account
2. Get required document types for country
3. Upload all required documents
4. Add truck details
5. Complete onboarding (submit for approval)
6. Wait for admin approval

**Bidding Process:**
1. Get available orders
2. Review order details
3. Select appropriate driver
4. Submit bid with amount and notes
5. Monitor bid status

**Order Fulfillment:**
1. Bid accepted by cargo owner
2. Upload manifest documents
3. Update order status to InTransit
4. Complete delivery with POD documents

---

## Version Information

- **API Version:** 1.0
- **Documentation Last Updated:** 2025-01-08
- **Based on Implementation:** trucki_api (C# .NET)

---

## Support

For issues, questions, or clarifications, contact the development team or refer to the implementation code in:
- Controllers: `/trucki/Controllers/DispatcherController.cs`
- Services: `/trucki/Services/TruckOwnerService.cs`, `/trucki/Services/CargoOrderService.cs`
- Models: `/trucki/Models/RequestModel/DispatcherRequestModels.cs`, `/trucki/Models/ResponseModels/`
