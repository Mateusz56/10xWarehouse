# 10xWarehouse

A simple, modern warehouse management system for small businesses. This project is an educational full-stack application built with the assistance of AI, from planning to deployment.

---

## Table of Contents

-   [Project Description](#project-description)
-   [Tech Stack](#tech-stack)
-   [Getting Started Locally](#getting-started-locally)
-   [Available Scripts](#available-scripts)
-   [CI/CD & Deployment](#cicd--deployment)
-   [Project Scope](#project-scope)
-   [Project Status](#project-status)
-   [License](#license)

---

## Project Description

Small business owners often struggle with inventory management, relying on error-prone spreadsheets or overly complex enterprise software. 10xWarehouse solves this by providing a clean, intuitive, and focused web application for core inventory tracking needs.

It is a multi-tenant system where users can create and manage multiple `Organizations`. Within an organization, they can define warehouses, specific locations, and product templates. The core functionality revolves around tracking stock levels through additions, withdrawals, internal movements, and reconciliations, with every action logged for a complete audit trail.

## Tech Stack

The technology stack was chosen to be modern, robust, and provide a great developer experience.

| Category      | Technology                                    |
| ------------- | --------------------------------------------- |
| **Frontend**  | Astro, Vue.js, Tailwind CSS, shadcn/ui        |
| **Backend**   | .NET (REST API)                               |
| **Database**  | PostgreSQL                                    |
| **Services**  | Supabase (Authentication & User Management)   |

---

## Getting Started Locally

To get a local copy up and running, please follow these steps.

### Prerequisites

**Option 1: Native Development**
-   Node.js (v18 or higher recommended)
-   .NET SDK
-   PostgreSQL
-   A free Supabase account for authentication.

**Option 2: Docker/Podman Development**
-   Docker or Podman installed
-   Docker Compose or Podman Compose
-   A free Supabase account for authentication.

### Installation & Setup

1.  **Clone the repository:**
    ```sh
    git clone https://github.com/your-username/10xWarehouse.git
    cd 10xWarehouse
    ```

### Running with Docker/Podman

The easiest way to get started is using Docker or Podman with Docker Compose:

1.  **Create environment file:**
    Copy the template file and fill in your Supabase credentials:
    ```sh
    cp env.template .env
    ```
    Edit `.env` and add your Supabase configuration:
    ```env
    SUPABASE_URL=https://your-project-id.supabase.co
    SUPABASE_SERVICE_ROLE_KEY=your-service-role-key-here
    SUPABASE_ANON_KEY=your-supabase-anon-key
    ```

2.  **Start all services:**
    
    **Using Docker:**
    ```sh
    docker compose -f docker-compose.local.yml up -d
    ```
    
    **Using Podman:**
    ```sh
    podman compose -f docker-compose.local.yml up -d
    ```

    This will start:
    - PostgreSQL database
    - Backend API server
    - Frontend application
    - Nginx reverse proxy

3.  **Access the application:**
    - Frontend: `http://localhost`
    - Backend API: `http://localhost/api`
    - Direct backend access (for debugging): `http://localhost:8080`

4.  **View logs:**
    ```sh
    # Docker
    docker compose -f docker-compose.local.yml logs -f
    
    # Podman
    podman compose -f docker-compose.local.yml logs -f
    ```

5.  **Stop services:**
    ```sh
    # Docker
    docker compose -f docker-compose.local.yml down
    
    # Podman
    podman compose -f docker-compose.local.yml down
    ```

**Note:** Database migrations run automatically when the backend starts. The database persists in a Docker volume named `pgdata`.

### Running with Native Development

If you prefer to run services natively without Docker:

1.  **Configure Environment Variables:**

    -   **Backend Configuration:**
        The backend uses `appsettings.json` for configuration. You can override values using environment variables or modify `appsettings.json` directly. The default connection string in `appsettings.json` points to `localhost:5432`.
        
        Set these environment variables (or update `appsettings.json`):
        ```bash
        # For PostgreSQL connection (if different from default)
        ConnectionStrings__DefaultConnection="Host=localhost;Port=5432;Database=postgres;Username=postgres;Password=postgres"
        
        # Required Supabase configuration
        Supabase__Url="https://your-project-id.supabase.co"
        Supabase__ServiceRoleKey="your-service-role-key-here"
        ```
        
        **Note:** .NET uses double underscores (`__`) for nested configuration in environment variables.

    -   **Frontend Configuration:**
        Create a `.env` file in the `10xWarehouseAstro/astro-app` directory:
        ```env
        PUBLIC_SUPABASE_URL="https://your-project-id.supabase.co"
        PUBLIC_SUPABASE_ANON_KEY="your-supabase-anon-key"
        PUBLIC_API_BASE_URL="http://localhost:5000/api"
        ```

2.  **Setup Frontend:**
    ```sh
    cd 10xWarehouseAstro/astro-app
    npm install
    ```

3.  **Setup Backend:**
    Navigate to the backend directory and restore the .NET dependencies.
    ```sh
    cd 10xWarehouseNet
    dotnet restore
    ```
    You will also need to run database migrations.
    ```sh
    dotnet ef database update
    ```

4.  **Run the Application:**

    -   **Backend Server (from the `10xWarehouseNet` directory):**
        ```sh
        dotnet run
        ```
        The API will be available at `http://localhost:5000` (or as configured).

    -   **Frontend Development Server (from the `10xWarehouseAstro/astro-app` directory):**
        ```sh
        npm run dev
        ```
        The application will be available at `http://localhost:4321`.

---

## Available Scripts

### Frontend (`10xWarehouseAstro/vue`)

-   `npm run dev`: Starts the development server.
-   `npm run build`: Builds the application for production.
-   `npm run preview`: Previews the production build locally.
-   `npm run astro`: Access the Astro CLI.

### Backend (`10xWarehouseNet`)

-   `dotnet run`: Starts the backend API server.
-   `dotnet build`: Compiles the project.
-   `dotnet test`: Runs the test suite.

---

## CI/CD & Deployment

The project includes a fully automated CI/CD pipeline that builds, tests, and deploys the application on every push to the `main` branch.

### Overview

The deployment pipeline consists of three main components:

1. **GitHub Actions Workflow** (`.github/workflows/deploy.yml`)
   - Automatically triggers on push to `main` branch
   - Builds Docker images for both backend and frontend
   - Pushes images to GitHub Container Registry (GHCR)
   - Deploys to Azure VM via SSH

2. **GitHub Container Registry (GHCR)**
   - Stores pre-built Docker images
   - Images are tagged with `latest` and commit SHA for traceability
   - Uses built-in `GITHUB_TOKEN` for authentication

3. **Azure VM Deployment**
   - Receives updated configuration files via SCP
   - Pulls latest images from GHCR
   - Stops existing containers and starts new ones
   - Verifies deployment health before completing

### Deployment Process

1. **Build Phase**: Docker images are built and pushed to GHCR with both `latest` and commit SHA tags
2. **Deploy Phase**: Configuration files are copied to the Azure VM, containers are stopped and restarted with new images
3. **Health Check**: The deployment verifies the backend health endpoint before marking the deployment as successful

### Prerequisites

To set up the CI/CD pipeline, you'll need:

- GitHub repository with Actions enabled
- GitHub Secrets configured (Azure VM credentials, Supabase keys, database credentials)
- Azure VM with Docker and Docker Compose installed
- SSH access configured for the Azure VM

### Manual Deployment

The workflow can be manually triggered via GitHub Actions UI:
- Go to **Actions** → **Build and Deploy** → **Run workflow**

For detailed setup instructions, troubleshooting, and rollback procedures, see [`.ai/cicd-deployment-plan.md`](.ai/cicd-deployment-plan.md).

---

## Project Scope

### Key MVP Features

-   **User & Organization Management:** Secure user registration and login, with multi-tenant data isolation. Support for `Owner`, `Member`, and `Viewer` roles.
-   **Warehouse Management:** Full CRUD for warehouses and their internal locations.
-   **Product Management:** Full CRUD for product templates with unique barcodes per organization.
-   **Inventory Tracking:** Core actions include Add, Withdraw, Move, and Reconcile stock, all with a full audit trail.
-   **Dashboard:** An at-a-glance view of recent activity and low-stock alerts.

### Out of Scope for MVP

-   Monetization and subscription tiers.
-   Native mobile applications.
-   Advanced reporting (PDF/CSV exports).
-   Barcode scanning via device camera.
-   Third-party integrations.

---

## Project Status

The project is currently in the post-planning phase. The Product Requirements Document (PRD) is complete, and the next step is to begin development based on the defined user stories and success metrics.

---

## License

This project is licensed under the MIT License. See the `LICENSE` file for more details.
