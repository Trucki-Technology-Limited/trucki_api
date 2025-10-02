# Admin Notification System API Documentation

## Overview
The Admin Notification System allows administrators to send bulk notifications to users via push notifications, email, or both channels. Admins can target all users, specific users, or filter by user type.

## Base URL
```
/api/admin/notifications
```

## Authentication
All endpoints require **Admin role** authorization.
```
Authorization: Bearer <admin_jwt_token>
```

---

## Endpoints

### 1. Search Users for Notification Targeting
Search and retrieve users with pagination, filtering, and search capabilities for notification targeting.

**Endpoint:** `GET /api/admin/notifications/users`

**Authorization:** Admin role required

**Query Parameters:**

| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| `pageNumber` | int | No | 1 | Page number for pagination |
| `pageSize` | int | No | 20 | Number of users per page (1-100) |
| `searchQuery` | string | No | null | Search in email, first name, or last name |
| `userType` | string | No | null | Filter by user type: `driver`, `cargoowner`, `truckowner`, `manager` |
| `isActive` | boolean | No | null | Filter by active status |

**Example Request:**
```
GET /api/admin/notifications/users?pageNumber=1&pageSize=20&searchQuery=john&userType=driver&isActive=true
```

**Response:**
```json
{
  "isSuccess": true,
  "statusCode": 200,
  "message": "Users retrieved successfully",
  "data": {
    "data": [
      {
        "userId": "user123",
        "email": "john.doe@example.com",
        "firstName": "John",
        "lastName": "Doe",
        "fullName": "John Doe",
        "userType": "driver",
        "userTypeDisplay": "Driver",
        "isActive": true,
        "createdAt": "2025-01-15T10:30:00.000Z",
        "phone": "+1234567890",
        "companyName": null
      },
      {
        "userId": "user456",
        "email": "jane.smith@company.com",
        "firstName": "Jane",
        "lastName": "Smith",
        "fullName": "Jane Smith",
        "userType": "cargoowner",
        "userTypeDisplay": "Cargo Owner",
        "isActive": true,
        "createdAt": "2025-01-10T09:15:00.000Z",
        "phone": "+0987654321",
        "companyName": "Smith Logistics"
      }
    ],
    "pageNumber": 1,
    "pageSize": 20,
    "totalCount": 2,
    "totalPages": 1,
    "hasPreviousPage": false,
    "hasNextPage": false
  }
}
```

### 2. Get User Type Options
Get available user types and their current user counts for notification targeting.

**Endpoint:** `GET /api/admin/notifications/user-types`

**Authorization:** Admin role required

**Response:**
```json
{
  "isSuccess": true,
  "statusCode": 200,
  "message": "User types retrieved successfully",
  "data": [
    {
      "userType": "driver",
      "displayName": "Drivers",
      "userCount": 245
    },
    {
      "userType": "cargoowner",
      "displayName": "Cargo Owners",
      "userCount": 89
    },
    {
      "userType": "truckowner",
      "displayName": "Truck Owners",
      "userCount": 56
    },
    {
      "userType": "manager",
      "displayName": "Managers",
      "userCount": 12
    }
  ]
}
```

---

### 3. Send Bulk Notifications
Send notifications to users via push notifications, email, or both channels.

**Endpoint:** `POST /api/admin/notifications/send`

**Authorization:** Admin role required

**Request Body:**
```json
{
  "title": "System Maintenance Notice",
  "message": "The system will be under maintenance on Sunday from 2 AM to 6 AM EST. Please plan accordingly.",
  "channel": "Both",
  "targetType": "UserType",
  "userType": "driver",
  "data": {
    "action": "maintenance",
    "priority": "high"
  }
}
```

**Request Model Properties:**

| Field | Type | Required | Description | Validation |
|-------|------|----------|-------------|------------|
| `title` | string | Yes | Notification title | Max 200 characters |
| `message` | string | Yes | Notification message | Max 1000 characters |
| `channel` | enum | Yes | Delivery channel | `PushOnly`, `EmailOnly`, `Both` |
| `targetType` | enum | Yes | Target selection type | `AllUsers`, `SpecificUsers`, `UserType` |
| `userIds` | string[] | Conditional | Array of user IDs | Required when `targetType` is `SpecificUsers` |
| `userType` | string | Conditional | User type filter | Required when `targetType` is `UserType`. Values: `driver`, `cargoowner`, `truckowner`, `manager` |
| `data` | object | No | Additional push notification data | Key-value pairs |

**Response:**
```json
{
  "isSuccess": true,
  "statusCode": 200,
  "message": "Notifications sent successfully",
  "data": {
    "totalTargetUsers": 245,
    "successfulPushNotifications": 240,
    "failedPushNotifications": 5,
    "successfulEmails": 243,
    "failedEmails": 2,
    "errors": [
      "Push notification failed for user john@example.com: Invalid device token",
      "Email failed for user jane@example.com: Invalid email address"
    ],
    "sentAt": "2025-10-02T14:30:00.000Z"
  }
}
```

**Example Requests:**

**Send to All Users:**
```json
{
  "title": "Welcome to Trucki 2.0!",
  "message": "Experience our new features and improved performance.",
  "channel": "Both",
  "targetType": "AllUsers"
}
```

**Send to Specific Users:**
```json
{
  "title": "Account Verification Required",
  "message": "Please verify your account to continue using our services.",
  "channel": "EmailOnly",
  "targetType": "SpecificUsers",
  "userIds": ["user123", "user456", "user789"]
}
```

**Send to Drivers Only:**
```json
{
  "title": "New Load Opportunities",
  "message": "High-paying loads available in your area. Check the app now!",
  "channel": "PushOnly",
  "targetType": "UserType",
  "userType": "driver",
  "data": {
    "category": "loads",
    "action": "open_loads_screen"
  }
}
```

---

### 4. Send Custom Email Notifications
Send bulk email notifications with custom HTML content.

**Endpoint:** `POST /api/admin/notifications/send-email`

**Authorization:** Admin role required

**Request Body:**
```json
{
  "subject": "Important Account Update",
  "htmlBody": "<html><body><h1>Account Update Required</h1><p>Dear User,</p><p>Please update your account information to continue using our services.</p><p><a href='https://trucki.co/account'>Update Account</a></p><p>Best regards,<br/>Trucki Team</p></body></html>",
  "targetType": "UserType",
  "userType": "cargoowner"
}
```

**Request Model Properties:**

| Field | Type | Required | Description | Validation |
|-------|------|----------|-------------|------------|
| `subject` | string | Yes | Email subject line | Max 200 characters |
| `htmlBody` | string | Yes | HTML email content | Valid HTML |
| `targetType` | enum | Yes | Target selection type | `AllUsers`, `SpecificUsers`, `UserType` |
| `userIds` | string[] | Conditional | Array of user IDs | Required when `targetType` is `SpecificUsers` |
| `userType` | string | Conditional | User type filter | Required when `targetType` is `UserType`. Values: `driver`, `cargoowner`, `truckowner`, `manager` |

**Response:**
```json
{
  "isSuccess": true,
  "statusCode": 200,
  "message": "Emails sent successfully",
  "data": {
    "totalTargetUsers": 89,
    "successfulPushNotifications": 0,
    "failedPushNotifications": 0,
    "successfulEmails": 87,
    "failedEmails": 2,
    "errors": [
      "Email failed for user invalid@: Invalid email address",
      "Email failed for user bounce@example.com: Email bounced"
    ],
    "sentAt": "2025-10-02T14:35:00.000Z"
  }
}
```

---

## Response Models

### ApiResponseModel Structure
All endpoints return responses wrapped in the standard `ApiResponseModel<T>` format:

```json
{
  "isSuccess": boolean,
  "statusCode": number,
  "message": "string",
  "data": T
}
```

### AdminNotificationResponseModel
```json
{
  "totalTargetUsers": number,
  "successfulPushNotifications": number,
  "failedPushNotifications": number,
  "successfulEmails": number,
  "failedEmails": number,
  "errors": ["string"],
  "sentAt": "ISO 8601 datetime"
}
```

### UserTypeOptionResponseModel
```json
{
  "userType": "string",
  "displayName": "string",
  "userCount": number
}
```

---

## Enums

### NotificationChannelType
- `PushOnly`: Send only push notifications
- `EmailOnly`: Send only emails
- `Both`: Send both push notifications and emails

### NotificationTargetType
- `AllUsers`: Target all active users in the system
- `SpecificUsers`: Target specific users by their IDs
- `UserType`: Target users by their type (driver, cargoowner, etc.)

---

## Error Responses

### 400 Bad Request
```json
{
  "isSuccess": false,
  "statusCode": 400,
  "message": "Invalid request data",
  "data": null
}
```

### 401 Unauthorized
```json
{
  "isSuccess": false,
  "statusCode": 401,
  "message": "User not authenticated",
  "data": null
}
```

### 403 Forbidden
```json
{
  "isSuccess": false,
  "statusCode": 403,
  "message": "Admin access required",
  "data": null
}
```

### 500 Internal Server Error
```json
{
  "isSuccess": false,
  "statusCode": 500,
  "message": "Failed to send notifications: Database connection error",
  "data": null
}
```

---

## Workflow for Targeting Specific Users

When admins want to send notifications to specific users, they should follow this workflow:

### Step 1: Search for Users
Use the `/api/admin/notifications/users` endpoint to search and browse users:

```javascript
// Search for drivers with "john" in their name/email
GET /api/admin/notifications/users?searchQuery=john&userType=driver&pageSize=10

// Browse all active cargo owners
GET /api/admin/notifications/users?userType=cargoowner&isActive=true&pageSize=20

// Search across all user types
GET /api/admin/notifications/users?searchQuery=company@email.com&pageSize=50
```

### Step 2: Select Users
From the search results, collect the `userId` values of the users you want to target:

```json
{
  "userIds": ["user123", "user456", "user789"]
}
```

### Step 3: Send Notification
Use the collected user IDs in the notification request:

```json
{
  "title": "Important Update",
  "message": "Please check your account settings.",
  "channel": "Both",
  "targetType": "SpecificUsers",
  "userIds": ["user123", "user456", "user789"]
}
```

---

## Usage Examples

### cURL Examples

**Search Users:**
```bash
curl -X GET "https://api.trucki.co/api/admin/notifications/users?pageNumber=1&pageSize=10&searchQuery=john&userType=driver" \
  -H "Authorization: Bearer your_admin_jwt_token" \
  -H "Content-Type: application/json"
```

**Get User Types:**
```bash
curl -X GET "https://api.trucki.co/api/admin/notifications/user-types" \
  -H "Authorization: Bearer your_admin_jwt_token" \
  -H "Content-Type: application/json"
```

**Send Notification to All Drivers:**
```bash
curl -X POST "https://api.trucki.co/api/admin/notifications/send" \
  -H "Authorization: Bearer your_admin_jwt_token" \
  -H "Content-Type: application/json" \
  -d '{
    "title": "New Feature Alert",
    "message": "Check out the new route optimization feature!",
    "channel": "Both",
    "targetType": "UserType",
    "userType": "driver"
  }'
```

**Send Custom Email:**
```bash
curl -X POST "https://api.trucki.co/api/admin/notifications/send-email" \
  -H "Authorization: Bearer your_admin_jwt_token" \
  -H "Content-Type: application/json" \
  -d '{
    "subject": "Payment System Update",
    "htmlBody": "<h1>Payment System Update</h1><p>We have updated our payment system for faster processing.</p>",
    "targetType": "AllUsers"
  }'
```

---

## Best Practices

1. **Test with Small Groups First**: Use `SpecificUsers` to test notifications with a small group before sending to all users.

2. **Monitor Error Responses**: Check the `errors` array in responses to identify and fix delivery issues.

3. **Use Appropriate Channels**:
   - Use `PushOnly` for time-sensitive alerts
   - Use `EmailOnly` for detailed communications
   - Use `Both` for critical announcements

4. **Validate HTML**: When using custom HTML emails, ensure your HTML is well-formed and mobile-friendly.

5. **Rate Limiting**: The system processes notifications sequentially to avoid overwhelming email/push services.