# Asset Management API

A RESTful API for managing dormitory assets, built with ASP.NET Core and PostgreSQL. This API provides comprehensive authentication, room management, and asset tracking capabilities.

## Features

- **User Authentication**: JWT-based authentication with email verification
- **Room Management**: Create, read, update, and delete rooms
- **Asset Tracking**: Manage assets within rooms with pagination support
- **Asset Categories**: Categorize assets for better organization
- **Email Notifications**: OTP verification and password reset via email
- **Secure Password Storage**: PBKDF2-based password hashing
- **CORS Support**: Configurable cross-origin resource sharing
- **API Documentation**: Interactive Swagger/OpenAPI documentation

## Tech Stack

- **Framework**: ASP.NET Core 6.0+
- **Database**: PostgreSQL
- **ORM**: Entity Framework Core
- **Authentication**: JWT Bearer Tokens
- **Validation**: FluentValidation
- **API Documentation**: Swagger/OpenAPI
- **Email**: SMTP (Gmail)

## Prerequisites

Before deploying, ensure you have:

- [.NET SDK 6.0+](https://dotnet.microsoft.com/download)
- PostgreSQL database (local or hosted)
- SMTP email credentials (Gmail recommended)
- Git

## Local Development

### 1. Clone the Repository

```bash
git clone https://github.com/adityawahyuuu/AssetManagement-API.git
cd AssetManagement-API
```

### 2. Configure Environment Variables

Create a `.env` file in the `API` directory for local development:

```bash
cd API
cp .env.example .env
```

Edit the `.env` file with your local configuration:

```env
# Database Configuration
ConnectionStrings__AssetManagementConnection=Host=localhost;Database=asset_management;Username=youruser;Password=yourpassword

# JWT Configuration (generate a secure random string, min 32 characters)
Jwt__Secret=YourSecureRandomString32CharactersOrMore

# Email Configuration (Gmail - use App Password)
Email__SenderEmail=your-email@gmail.com
Email__Username=your-email@gmail.com
Email__Password=your-gmail-app-password

# Swagger UI (optional)
Swagger__Username=admin
Swagger__Password=admin123
```

**Important:**
- ✅ `.env` is in `.gitignore` - your secrets are safe
- ✅ Never commit the `.env` file to version control
- ✅ Use `.env.example` as a template for team members
- ✅ For Gmail, create an [App Password](https://support.google.com/accounts/answer/185833)

### 3. Run Migrations

```bash
cd API
dotnet ef database update
```

### 4. Trust the HTTPS Development Certificate

For local HTTPS testing, trust the ASP.NET Core development certificate:

```bash
dotnet dev-certs https --trust
```

This allows your browser and tools to connect to the API over HTTPS without security warnings.

### 5. Run the Application

```bash
dotnet run --launch-profile https
```

The API will be available at:
- **HTTPS**: `https://localhost:7146` (recommended)
- **HTTP**: `http://localhost:5080` (will redirect to HTTPS)

**Note**: The application is configured to use HTTPS by default for security.

## Deploying to Render

### Step 1: Prepare Your Database

1. Create a PostgreSQL database on Render:
   - Go to [Render Dashboard](https://dashboard.render.com/)
   - Click "New +" → "PostgreSQL"
   - Name your database (e.g., `asset-management-db`)
   - Select a region close to your users
   - Click "Create Database"
   - Copy the **Internal Database URL** (starts with `postgresql://`)

### Step 2: Create Web Service on Render

1. Click "New +" → "Web Service"
2. Connect your GitHub repository
3. Configure the service:
   - **Name**: `asset-management-api` (or your preferred name)
   - **Region**: Same as your database
   - **Branch**: `main` (or your deployment branch)
   - **Root Directory**: `API`
   - **Runtime**: `.NET`
   - **Build Command**: `dotnet publish -c Release -o out`
   - **Start Command**: `cd out && dotnet API.dll`

### Step 3: Configure Environment Variables

Add the following environment variables in Render:

| Variable Name | Description | Example Value |
|--------------|-------------|---------------|
| `ASPNETCORE_ENVIRONMENT` | Application environment | `Production` |
| `ConnectionStrings__AssetManagementConnection` | PostgreSQL connection string | `postgresql://user:pass@host/db` |
| `Jwt__Secret` | JWT signing secret (min 32 chars) | `YourSecureRandomString32CharsOrMore` |
| `Jwt__Issuer` | JWT issuer name | `AssetManagementAPI` |
| `Jwt__Audience` | JWT audience | `AssetManagementClient` |
| `Email__SenderEmail` | SMTP sender email | `your-email@gmail.com` |
| `Email__Username` | SMTP username | `your-email@gmail.com` |
| `Email__Password` | SMTP password/app password | `your-app-password` |
| `Cors__AllowedOrigins` | Allowed frontend URLs | `https://your-frontend.com,https://www.your-frontend.com` |

**Important Notes:**
- Use double underscores (`__`) to represent nested configuration keys
- The database connection string should be the Internal Database URL from Step 1
- For Gmail, use an [App Password](https://support.google.com/accounts/answer/185833)
- Add all your frontend URLs to CORS allowed origins, separated by commas
- Keep your JWT secret secure and never commit it to version control

### Step 4: Deploy

1. Click "Create Web Service"
2. Render will automatically build and deploy your application
3. Wait for the deployment to complete (5-10 minutes)
4. Your API will be available at `https://your-service-name.onrender.com`

**HTTPS Configuration:**
- ✅ Render automatically provides free SSL/TLS certificates for all web services
- ✅ Your API is automatically accessible via HTTPS
- ✅ The application is configured with forwarded headers middleware to properly handle HTTPS behind Render's reverse proxy
- ✅ HTTP requests are automatically redirected to HTTPS
- ✅ No additional HTTPS configuration needed

### Step 5: Initialize Database

After first deployment, run migrations:

1. Go to your web service in Render
2. Click "Shell" tab
3. Run:
   ```bash
   cd out
   dotnet ef database update --project /path/to/API.csproj
   ```

Alternatively, use your local machine with the production database connection string:

```bash
dotnet ef database update --connection "your-production-connection-string"
```

## Environment Variables Reference

### Required Variables

- **ConnectionStrings__AssetManagementConnection**: PostgreSQL connection string
- **Jwt__Secret**: Secret key for JWT token signing (minimum 32 characters)
- **Email__SenderEmail**: Email address for sending notifications
- **Email__Username**: SMTP username
- **Email__Password**: SMTP password
- **Cors__AllowedOrigins**: Comma-separated list of allowed frontend URLs

### Optional Variables

- **Jwt__Issuer**: JWT issuer (default: `AssetManagementAPI`)
- **Jwt__Audience**: JWT audience (default: `AssetManagementClient`)
- **Jwt__ExpirationMinutes**: Token expiration time (default: `1440` = 24 hours)
- **Email__SmtpHost**: SMTP server (default: `smtp.gmail.com`)
- **Email__SmtpPort**: SMTP port (default: `587`)
- **Email__SenderName**: Sender name (default: `Asset Management System`)
- **Swagger__Username**: Swagger UI username (default: `admin`)
- **Swagger__Password**: Swagger UI password (default: `admin123`)
- **Swagger__AuthEnabled**: Enable Swagger UI basic auth (default: `true` in production, `false` in development)

## API Documentation

Once deployed, access the interactive API documentation at:

- **Local (HTTPS)**: `https://localhost:7146` (recommended)
- **Local (HTTP)**: `http://localhost:5080` (redirects to HTTPS)
- **Production**: `https://your-service-name.onrender.com` (HTTPS only)

The Swagger UI is available at the root URL and provides:
- Complete API endpoint documentation
- Request/response schemas
- Interactive testing capabilities
- JWT authentication support

### Swagger UI Authentication

For security, Swagger UI is protected with HTTP Basic Authentication in production environments.

**Local Development:**
- Swagger authentication is **disabled** by default
- Access Swagger UI directly without credentials

**Production:**
- Swagger authentication is **enabled** by default
- You'll be prompted to enter credentials when accessing Swagger UI
- Default credentials (change these in production):
  - Username: `admin`
  - Password: `admin123`

**To Configure Swagger Authentication:**

Add these environment variables in Render:

| Variable Name | Description | Example Value |
|--------------|-------------|---------------|
| `Swagger__Username` | Swagger UI username | `your-secure-username` |
| `Swagger__Password` | Swagger UI password | `your-secure-password` |
| `Swagger__AuthEnabled` | Enable/disable authentication | `true` or `false` |

**To Disable Swagger Authentication (Not Recommended for Production):**

Set `Swagger__AuthEnabled` to `false` in your environment variables.

**Security Best Practices:**
- Always use strong, unique credentials for Swagger UI in production
- Consider using environment-specific passwords
- Regularly rotate Swagger UI credentials
- Keep `Swagger__AuthEnabled` set to `true` in production

## Testing with Swagger

### 1. Access Swagger UI

Navigate to your API URL in a browser. You'll see the Swagger UI interface.

### 2. Register a New User

1. Expand `POST /api/user/register`
2. Click "Try it out"
3. Fill in the request body:
   ```json
   {
     "username": "testuser123",
     "email": "test@example.com",
     "password": "Test123!",
     "fullName": "Test User"
   }
   ```
4. Click "Execute"
5. Check your email for the OTP code

### 3. Verify Email with OTP

1. Expand `POST /api/user/verify`
2. Click "Try it out"
3. Enter:
   ```json
   {
     "email": "test@example.com",
     "otpCode": "123456"
   }
   ```
4. Click "Execute"

### 4. Login and Get JWT Token

1. Expand `POST /api/auth/login` or `POST /api/user/login`
2. Click "Try it out"
3. Enter credentials:
   ```json
   {
     "username": "testuser123",
     "password": "Test123!"
   }
   ```
4. Click "Execute"
5. Copy the `token` from the response

### 5. Authorize Swagger with JWT

1. Click the "Authorize" button (lock icon) at the top
2. Enter: `Bearer your-token-here` (include the word "Bearer" with a space)
3. Click "Authorize"
4. Click "Close"

### 6. Test Protected Endpoints

Now you can test any endpoint that requires authentication:

- **Get Current User**: `GET /api/user/auth/me`
- **Create Room**: `POST /api/rooms`
- **Get All Rooms**: `GET /api/rooms`
- **Create Asset**: `POST /api/assets`
- **Get All Assets**: `GET /api/assets`

## API Endpoints Overview

### Authentication Endpoints

| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|---------------|
| POST | `/api/auth/register` | Register new user (JSON) | No |
| POST | `/api/auth/login` | Login user (JSON) | No |
| POST | `/api/user/register` | Register new user (Form) | No |
| POST | `/api/user/login` | Login user (Form) | No |
| POST | `/api/user/verify` | Verify email with OTP | No |
| POST | `/api/user/resend-otp` | Resend OTP code | No |
| POST | `/api/user/forgot-password` | Request password reset | No |
| POST | `/api/user/reset-password` | Reset password with OTP | No |
| GET | `/api/user/auth/me` | Get current user info | Yes |
| POST | `/api/user/logout` | Logout (client-side) | Yes |

### Room Endpoints

| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|---------------|
| GET | `/api/rooms` | Get all user rooms | Yes |
| GET | `/api/rooms/{id}` | Get room by ID | Yes |
| POST | `/api/rooms` | Create new room | Yes |
| PUT | `/api/rooms/{id}` | Update room | Yes |
| DELETE | `/api/rooms/{id}` | Delete room and assets | Yes |

### Asset Endpoints

| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|---------------|
| GET | `/api/assets` | Get all assets (paginated) | Yes |
| GET | `/api/assets/{id}` | Get asset by ID | Yes |
| GET | `/api/assets/room/{roomId}` | Get assets by room ID | Yes |
| POST | `/api/assets` | Create new asset | Yes |
| PUT | `/api/assets/{id}` | Update asset | Yes |
| DELETE | `/api/assets/{id}` | Delete asset | Yes |

### Asset Category Endpoints

| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|---------------|
| GET | `/api/asset-categories` | Get all asset categories | No |

## Client-Side Integration

### JavaScript/React Example

```javascript
const API_BASE_URL = 'https://your-api.onrender.com';

// 1. Register User
async function registerUser(userData) {
  const response = await fetch(`${API_BASE_URL}/api/auth/register`, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
    },
    body: JSON.stringify({
      username: userData.username,
      email: userData.email,
      password: userData.password,
      fullName: userData.fullName
    })
  });
  return await response.json();
}

// 2. Login and Store Token
async function login(username, password) {
  const response = await fetch(`${API_BASE_URL}/api/auth/login`, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
    },
    body: JSON.stringify({ username, password })
  });

  const result = await response.json();

  if (result.type === 'success') {
    // Store token in localStorage
    localStorage.setItem('authToken', result.data.token);
    localStorage.setItem('userId', result.data.userId);
  }

  return result;
}

// 3. Make Authenticated Request
async function getRooms() {
  const token = localStorage.getItem('authToken');

  const response = await fetch(`${API_BASE_URL}/api/rooms`, {
    method: 'GET',
    headers: {
      'Authorization': `Bearer ${token}`,
      'Content-Type': 'application/json',
    }
  });

  return await response.json();
}

// 4. Create Room
async function createRoom(roomData) {
  const token = localStorage.getItem('authToken');

  const response = await fetch(`${API_BASE_URL}/api/rooms`, {
    method: 'POST',
    headers: {
      'Authorization': `Bearer ${token}`,
      'Content-Type': 'application/json',
    },
    body: JSON.stringify({
      roomNumber: roomData.roomNumber,
      description: roomData.description
    })
  });

  return await response.json();
}

// 5. Get Assets with Pagination
async function getAssets(page = 1, pageSize = 10) {
  const token = localStorage.getItem('authToken');

  const response = await fetch(
    `${API_BASE_URL}/api/assets?page=${page}&pageSize=${pageSize}`,
    {
      method: 'GET',
      headers: {
        'Authorization': `Bearer ${token}`,
        'Content-Type': 'application/json',
      }
    }
  );

  return await response.json();
}

// 6. Error Handling Example
async function apiRequest(url, options = {}) {
  try {
    const token = localStorage.getItem('authToken');

    const response = await fetch(`${API_BASE_URL}${url}`, {
      ...options,
      headers: {
        'Content-Type': 'application/json',
        ...(token && { 'Authorization': `Bearer ${token}` }),
        ...options.headers,
      },
    });

    const data = await response.json();

    if (!response.ok) {
      throw new Error(data.message || 'API request failed');
    }

    return data;
  } catch (error) {
    console.error('API Error:', error);
    throw error;
  }
}
```

### Using Axios

```javascript
import axios from 'axios';

const api = axios.create({
  baseURL: 'https://your-api.onrender.com',
  headers: {
    'Content-Type': 'application/json',
  }
});

// Request interceptor to add token
api.interceptors.request.use(
  (config) => {
    const token = localStorage.getItem('authToken');
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
  (error) => Promise.reject(error)
);

// Response interceptor for error handling
api.interceptors.response.use(
  (response) => response.data,
  (error) => {
    if (error.response?.status === 401) {
      // Token expired or invalid
      localStorage.removeItem('authToken');
      window.location.href = '/login';
    }
    return Promise.reject(error);
  }
);

// Usage
export const authAPI = {
  login: (credentials) => api.post('/api/auth/login', credentials),
  register: (userData) => api.post('/api/auth/register', userData),
  getCurrentUser: () => api.get('/api/user/auth/me'),
};

export const roomAPI = {
  getAll: () => api.get('/api/rooms'),
  getById: (id) => api.get(`/api/rooms/${id}`),
  create: (roomData) => api.post('/api/rooms', roomData),
  update: (id, roomData) => api.put(`/api/rooms/${id}`, roomData),
  delete: (id) => api.delete(`/api/rooms/${id}`),
};

export const assetAPI = {
  getAll: (page = 1, pageSize = 10) =>
    api.get(`/api/assets?page=${page}&pageSize=${pageSize}`),
  getById: (id) => api.get(`/api/assets/${id}`),
  getByRoom: (roomId) => api.get(`/api/assets/room/${roomId}`),
  create: (assetData) => api.post('/api/assets', assetData),
  update: (id, assetData) => api.put(`/api/assets/${id}`, assetData),
  delete: (id) => api.delete(`/api/assets/${id}`),
};
```

## Response Format

All API responses follow this consistent format:

### Success Response
```json
{
  "type": "success",
  "message": "Operation completed successfully",
  "data": {
    // Response data here
  }
}
```

### Error Response
```json
{
  "type": "failed" | "error" | "validation_error",
  "message": "Error description",
  "errors": {
    // Validation errors (if applicable)
  }
}
```

### Paginated Response
```json
{
  "type": "success",
  "message": "Assets retrieved successfully",
  "data": [...],
  "page": 1,
  "pageSize": 10,
  "totalCount": 100,
  "totalPages": 10,
  "hasPreviousPage": false,
  "hasNextPage": true
}
```

## Security Best Practices

1. **Use .env file for local development secrets**
   - Never commit `.env` to version control (already in `.gitignore`)
   - Use `.env.example` as a template for team members
   - Store database passwords, JWT secrets, and email credentials in `.env`

2. **Never expose sensitive environment variables** in your frontend code

3. **Store JWT tokens securely** (localStorage, sessionStorage, or httpOnly cookies)

4. **Always use HTTPS** in production

5. **Implement token refresh** logic for better security

6. **Handle token expiration** gracefully on the client side

7. **Validate CORS origins** carefully in production

8. **Use environment-specific configurations**:
   - Development: `.env` file (not committed)
   - Production: Environment variables on Render

9. **Enable rate limiting** if needed (consider Render's built-in features)

## Troubleshooting

### Common Issues

1. **Database Connection Errors**
   - Verify the connection string format
   - Ensure the database is running and accessible
   - Check network connectivity between services

2. **CORS Errors**
   - Add your frontend URL to `Cors__AllowedOrigins`
   - Ensure the URL format matches exactly (including protocol and port)
   - Check browser console for specific CORS error messages

3. **JWT Authentication Failures**
   - Verify the JWT secret is at least 32 characters
   - Ensure the token is sent with the `Bearer` prefix
   - Check token expiration time
   - Verify the token is being sent in the Authorization header

4. **Email Not Sending**
   - For Gmail, use an App Password, not your regular password
   - Enable "Less secure app access" or use OAuth2
   - Check SMTP settings and credentials
   - Verify email service is not blocked by firewall

5. **Migration Issues**
   - Ensure database connection is correct
   - Check if migrations folder exists
   - Run migrations manually if needed
   - Verify Entity Framework tools are installed

## Development Tools

### Running Tests

```bash
cd API.Test
dotnet test
```

### Database Migrations

Create a new migration:
```bash
dotnet ef migrations add MigrationName
```

Apply migrations:
```bash
dotnet ef database update
```

Rollback migration:
```bash
dotnet ef database update PreviousMigrationName
```

### Code Formatting

```bash
dotnet format
```

## Support

For issues, questions, or contributions:

- **GitHub Issues**: [Create an issue](https://github.com/adityawahyuuu/AssetManagement-API/issues)
- **Documentation**: Check this README and Swagger documentation
- **Email**: Contact your system administrator

## License

[Your License Here]

## Contributors

- Aditya Wahyu ([@adityawahyuuu](https://github.com/adityawahyuuu))

---

**Happy Coding!** 🚀
