# Asset Management API

Complete API documentation, endpoint reference, and client integration guide.

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

## API Security

### JWT Token Handling

- **Store tokens securely** (localStorage, sessionStorage, or httpOnly cookies)
- **Always include Bearer prefix** when sending token: `Authorization: Bearer your-token-here`
- **Handle token expiration** gracefully on the client side
- **Implement token refresh** logic for better security
- **Never expose tokens** in URL parameters or public code

### Best Practices

1. **Use HTTPS always** - All API communications must use HTTPS in production
2. **Validate server certificates** - Ensure SSL/TLS certificate is valid
3. **Implement CORS properly** - Only allow trusted frontend domains
4. **Sanitize user input** - Prevent SQL injection and XSS attacks
5. **Rate limiting** - Implement to prevent abuse
6. **Error handling** - Don't expose sensitive information in error messages

### CORS Configuration

The API is configured to accept requests from allowed origins. Make sure your frontend domain is added to the `Cors__AllowedOrigins` environment variable:

```env
# Single origin
Cors__AllowedOrigins=https://your-frontend.com

# Multiple origins (comma-separated)
Cors__AllowedOrigins=https://your-frontend.com,https://www.your-frontend.com,https://staging.your-frontend.com
```

## Common API Patterns

### Pagination Example

```javascript
const getAssetsWithPagination = async (page = 1, pageSize = 10) => {
  const response = await fetch(
    `/api/assets?page=${page}&pageSize=${pageSize}`,
    {
      headers: {
        'Authorization': `Bearer ${token}`,
      }
    }
  );

  const result = await response.json();

  return {
    items: result.data,
    page: result.page,
    pageSize: result.pageSize,
    total: result.totalCount,
    hasNext: result.hasNextPage,
    hasPrev: result.hasPreviousPage
  };
};
```

### Error Handling Pattern

```javascript
const handleApiError = (error) => {
  if (error.response?.status === 401) {
    // Handle unauthorized
    console.error('Unauthorized - token may be expired');
    localStorage.removeItem('authToken');
  } else if (error.response?.status === 403) {
    // Handle forbidden
    console.error('Forbidden - access denied');
  } else if (error.response?.status === 422) {
    // Handle validation error
    console.error('Validation error:', error.response.data.errors);
  } else if (error.response?.status >= 500) {
    // Handle server error
    console.error('Server error:', error.response.data.message);
  }
};
```

### Retry Logic with Exponential Backoff

```javascript
async function fetchWithRetry(url, options = {}, maxRetries = 3) {
  for (let i = 0; i < maxRetries; i++) {
    try {
      const response = await fetch(url, options);
      if (response.ok) return response;

      // Don't retry on client errors (4xx)
      if (response.status < 500) throw new Error(`${response.status}`);
    } catch (error) {
      if (i === maxRetries - 1) throw error;

      // Exponential backoff
      const delay = Math.pow(2, i) * 1000;
      await new Promise(resolve => setTimeout(resolve, delay));
    }
  }
}
```

## Rate Limiting

The API may implement rate limiting to prevent abuse. If you receive a 429 (Too Many Requests) response:

1. Check the `Retry-After` header for how long to wait
2. Implement exponential backoff in your client
3. Cache responses when possible
4. Implement request queuing

## API Versioning

The current API version is **v1** (default). All endpoints are under `/api/...`

For future versions:
- `/api/v1/...` - Current version
- `/api/v2/...` - Future versions will be under separate paths

---

For project setup and deployment information, see the [main README](../README.md).
