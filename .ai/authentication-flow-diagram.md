# Authentication Flow Diagram

This diagram illustrates the complete authentication flow in the 10xWarehouse application using Supabase authentication.

```mermaid
sequenceDiagram
    participant User as User
    participant Frontend as Frontend<br/>(Astro + Vue)
    participant Supabase as Supabase Auth
    participant Backend as .NET Backend
    participant Database as Database

    Note over User, Database: Authentication Flow

    User->>Frontend: Navigate to /login
    Frontend->>User: Display LoginForm component
    
    User->>Frontend: Enter credentials
    Frontend->>Supabase: Sign in with email/password
    
    alt Authentication Success
        Supabase->>Frontend: Return JWT token
        Frontend->>Frontend: Store token in localStorage
        Frontend->>Frontend: Update auth store (Pinia)
        Frontend->>User: Redirect to main page
        
        Note over Frontend: AuthGuard protects main page
        User->>Frontend: Access protected page
        Frontend->>Frontend: Check auth state
        Frontend->>User: Allow access to protected content
        
        Note over Frontend, Backend: API Requests with Authentication
        User->>Frontend: Trigger API call
        Frontend->>Frontend: Get token from auth store
        Frontend->>Backend: API request with JWT token
        
        Backend->>Backend: Validate JWT with Supabase
        Backend->>Supabase: Verify token signature
        
        alt Token Valid
            Supabase->>Backend: Token verification success
            Backend->>Backend: Extract user context from token
            Backend->>Database: Execute protected operation
            Database->>Backend: Return data
            Backend->>Frontend: Return API response
            Frontend->>User: Display data
        else Token Invalid
            Supabase->>Backend: Token verification failed
            Backend->>Frontend: Return 401 Unauthorized
            Frontend->>Frontend: Clear auth state
            Frontend->>User: Redirect to login
        end
        
    else Authentication Failed
        Supabase->>Frontend: Return error
        Frontend->>User: Display error message
    end

    Note over User, Database: Token Refresh Flow
    
    Frontend->>Supabase: Automatic token refresh
    Supabase->>Frontend: Return new JWT token
    Frontend->>Frontend: Update stored token
```

## Architecture Overview

```mermaid
graph TB
    subgraph "Frontend Layer"
        A[Login Page] --> B[LoginForm Component]
        B --> C[Auth Store - Pinia]
        C --> D[AuthGuard Component]
        D --> E[Protected Pages]
        E --> F[API Client]
    end
    
    subgraph "Authentication Layer"
        G[Supabase Client] --> H[JWT Token Storage]
        H --> I[Token Validation]
    end
    
    subgraph "Backend Layer"
        J[.NET API] --> K[JWT Middleware]
        K --> L[SupabaseJwtAuthenticationService]
        L --> M[Protected Controllers]
    end
    
    subgraph "External Services"
        N[Supabase Auth]
        O[Database]
    end
    
    F --> G
    G --> N
    K --> L
    L --> N
    M --> O
    
    style A fill:#e1f5fe
    style C fill:#f3e5f5
    style G fill:#e8f5e8
    style L fill:#fff3e0
    style N fill:#fce4ec
```

## Component Interaction Flow

```mermaid
flowchart TD
    Start([User starts app]) --> CheckAuth{Is user authenticated?}
    
    CheckAuth -->|No| LoginPage[Login Page]
    CheckAuth -->|Yes| MainApp[Main Application]
    
    LoginPage --> LoginForm[LoginForm Component]
    LoginForm --> SupabaseAuth[Supabase Authentication]
    
    SupabaseAuth -->|Success| UpdateAuthStore[Update Auth Store]
    SupabaseAuth -->|Failure| ShowError[Show Error Message]
    
    UpdateAuthStore --> RedirectMain[Redirect to Main App]
    RedirectMain --> MainApp
    
    MainApp --> AuthGuard[AuthGuard Component]
    AuthGuard -->|Authenticated| ProtectedContent[Protected Content]
    AuthGuard -->|Not Authenticated| RedirectLogin[Redirect to Login]
    
    ProtectedContent --> APICall[API Call]
    APICall --> AddToken[Add JWT Token to Headers]
    AddToken --> BackendAPI[Backend API]
    
    BackendAPI --> ValidateToken[Validate Token with Supabase]
    ValidateToken -->|Valid| ProcessRequest[Process Request]
    ValidateToken -->|Invalid| Return401[Return 401 Unauthorized]
    
    ProcessRequest --> ReturnData[Return Data]
    Return401 --> ClearAuth[Clear Auth State]
    ClearAuth --> RedirectLogin
    
    style Start fill:#e3f2fd
    style LoginPage fill:#fff3e0
    style MainApp fill:#e8f5e8
    style SupabaseAuth fill:#fce4ec
    style BackendAPI fill:#f3e5f5
```

## Security Features Diagram

```mermaid
mindmap
  root((Security Features))
    Authentication
      JWT Token Validation
        Supabase Signature Verification
        Token Expiration Check
        User Context Extraction
      Token Storage
        Browser localStorage
        Automatic Refresh
        Secure Transmission
    Authorization
      Protected Endpoints
        Organizations Controller
        Users Controller
        Role-based Access
      Middleware Protection
        JWT Middleware
        Custom Auth Handler
        Error Handling
    Frontend Security
      AuthGuard Component
        Route Protection
        Automatic Redirects
        State Management
      API Client Security
        Automatic Token Injection
        Request Interceptors
        Error Handling
```

## Data Flow Diagram

```mermaid
graph LR
    subgraph "User Interface"
        UI[User Interface]
    end
    
    subgraph "Frontend Store"
        AuthStore[Auth Store<br/>Pinia]
        OrgStore[Organization Store<br/>Pinia]
    end
    
    subgraph "API Layer"
        APIClient[API Client<br/>with JWT]
    end
    
    subgraph "Backend Services"
        AuthService[SupabaseJwt<br/>AuthenticationService]
        OrgService[Organization<br/>Service]
    end
    
    subgraph "External"
        Supabase[Supabase Auth]
        Database[(Database)]
    end
    
    UI --> AuthStore
    UI --> OrgStore
    AuthStore --> APIClient
    OrgStore --> APIClient
    APIClient --> AuthService
    APIClient --> OrgService
    AuthService --> Supabase
    OrgService --> Database
    
    style AuthStore fill:#e1f5fe
    style AuthService fill:#fff3e0
    style Supabase fill:#fce4ec
    style Database fill:#e8f5e8
```

This documentation provides a comprehensive view of how authentication works in your 10xWarehouse application, showing the flow from user login through protected API calls and the security measures in place.
