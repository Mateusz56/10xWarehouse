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

-   Node.js (v18 or higher recommended)
-   .NET SDK
-   PostgreSQL
-   A free Supabase account for authentication.

### Installation & Setup

1.  **Clone the repository:**
    ```sh
    git clone https://github.com/your-username/10xWarehouse.git
    cd 10xWarehouse
    ```

2.  **Configure Environment Variables:**
    You will need to configure environment variables for both the frontend and backend. It's recommended to create `.env` files in the respective project directories.

    -   **Backend (`10xWarehouseNet/.env`):**
        ```env
        DB_CONNECTION_STRING="Your_PostgreSQL_Connection_String"
        SUPABASE_JWT_SECRET="Your_Supabase_JWT_Secret"
        ```

    -   **Frontend (`10xWarehouseAstro/vue/.env`):**
        ```env
        PUBLIC_SUPABASE_URL="Your_Supabase_Project_URL"
        PUBLIC_SUPABASE_ANON_KEY="Your_Supabase_Anon_Key"
        ```

3.  **Setup Frontend:**
    ```sh
    cd 10xWarehouseAstro/vue
    npm install
    ```

4.  **Setup Backend:**
    Navigate to the backend directory and restore the .NET dependencies.
    ```sh
    cd 10xWarehouseNet
    dotnet restore
    ```
    You will also need to run database migrations.
    ```sh
    dotnet ef database update
    ```

### Running the Application

1.  **Run the Backend Server (from the `10xWarehouseNet` directory):**
    ```sh
    dotnet run
    ```
    The API will be available at `http://localhost:5000` (or as configured).

2.  **Run the Frontend Development Server (from the `10xWarehouseAstro/vue` directory):**
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
