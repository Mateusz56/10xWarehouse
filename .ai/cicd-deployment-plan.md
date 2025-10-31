# CI/CD Deployment Plan for 10xWarehouse

## Overview

This document outlines the continuous integration and continuous deployment (CI/CD) setup for the 10xWarehouse application. The system automatically builds Docker images, pushes them to GitHub Container Registry (GHCR), and deploys to an Azure VM whenever code is pushed to the `main` branch.

## Architecture

### Components

1. **GitHub Actions Workflow** (`.github/workflows/deploy.yml`)
   - Automatically triggers on push to `main` branch
   - Builds Docker images for backend and frontend
   - Pushes images to GitHub Container Registry
   - Deploys to Azure VM via SSH

2. **GitHub Container Registry (GHCR)**
   - Stores pre-built Docker images
   - Images are tagged with `latest` and commit SHA
   - Authentication via built-in `GITHUB_TOKEN`

3. **Azure VM Deployment**
   - Receives updated `docker-compose.yml` and `nginx.conf` via SCP
   - Pulls latest images from GHCR
   - Stops existing containers and starts new ones
   - Verifies deployment health

4. **Docker Compose** (`docker-compose.yml`)
   - Production configuration using GHCR images
   - Environment variables from GitHub Secrets
   - No local builds required

## Prerequisites Setup

### 1. GitHub Repository Settings

Enable GitHub Container Registry permissions:
- Go to repository → **Settings** → **Actions** → **General**
- Under **Workflow permissions**, select **"Read and write permissions"**
- This allows `GITHUB_TOKEN` to push packages to GHCR

### 2. GitHub Secrets Required

Add these secrets in your repository (**Settings** → **Secrets and variables** → **Actions**):

| Secret Name | Description | Example |
|------------|-------------|---------|
| `AZURE_VM_HOST` | Azure VM IP address or hostname | `20.123.45.67` |
| `AZURE_VM_USER` | SSH username | `azureuser` |
| `AZURE_VM_SSH_KEY` | Contents of your `azure_vm.pem` file | `-----BEGIN RSA PRIVATE KEY-----...` |
| `SUPABASE_URL` | Supabase project URL | `https://xxx.supabase.co` |
| `SUPABASE_SERVICE_ROLE_KEY` | Supabase service role key | `eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...` |
| `SUPABASE_ANON_KEY` | Supabase anonymous key | `eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...` |
| `POSTGRES_USER` | Database username (optional) | `postgres` (default) |
| `POSTGRES_PASSWORD` | Database password (optional) | `postgres` (default) |
| `POSTGRES_DB` | Database name (optional) | `postgres` (default) |

**Note:** `GITHUB_TOKEN` is automatically available - no need to create a Personal Access Token.

### 3. Azure VM Prerequisites

On your Azure VM, ensure the following are installed and configured:

```bash
# Verify Docker and Docker Compose are installed
docker --version
docker compose version

# Create deployment directory
mkdir -p /home/azureuser/10xWarehouse
```

**Required:**
- Docker installed
- Docker Compose installed
- SSH access configured with the provided `azure_vm.pem` key
- Network ports open: 80 (HTTP), 443 (HTTPS), 8080 (for health checks)

## Deployment Process

### Workflow Overview

1. **Trigger**: Push to `main` branch or manual trigger via `workflow_dispatch`

2. **Build Job** (`build-and-push`):
   - Checks out code
   - Sets up Docker Buildx
   - Authenticates to GHCR using `GITHUB_TOKEN`
   - Builds backend Docker image with caching
   - Builds frontend Docker image with build args (Supabase config)
   - Pushes both images to GHCR with `latest` and commit SHA tags

3. **Deploy Job** (`deploy`):
   - Checks out code
   - Sets up SSH connection using `AZURE_VM_SSH_KEY`
   - Replaces `REPO_PLACEHOLDER` in `docker-compose.yml` with actual repository path
   - Copies `docker-compose.yml` and `nginx.conf` to Azure VM
   - SSH into Azure VM and executes deployment commands:
     - Logs in to GHCR using `GITHUB_TOKEN`
     - Stops all containers (`docker compose down`)
     - Sets environment variables from GitHub Secrets
     - Pulls latest images (`docker compose pull`)
     - Starts all containers (`docker compose up -d`)
     - Waits and verifies backend health
     - Cleans up old Docker images
   - Performs final health check from GitHub Actions

### Deployment Strategy

**Simple Stop-and-Start Approach:**
- No zero-downtime deployment
- All containers are stopped, then new containers are started
- Suitable for this application's use case

**Database Migrations:**
- Handled automatically in application code (EF Core migrations)
- No manual migration steps required

## Docker Compose Configuration

### Production `docker-compose.yml`

The production `docker-compose.yml` uses:
- **GHCR images** instead of local builds
- **Environment variables** from GitHub Secrets (via SSH environment)
- **Placeholder replacement** (`REPO_PLACEHOLDER`) that gets replaced during deployment

### Key Changes from Development

1. **Backend service**:
   - Uses `image: ghcr.io/REPO_PLACEHOLDER/backend:latest`
   - Removed `build:` section
   - Removed port 8080 mapping (only exposed internally, accessed via nginx)

2. **Frontend service**:
   - Uses `image: ghcr.io/REPO_PLACEHOLDER/frontend:latest`
   - Removed `build:` section and build args (handled during GitHub Actions build)

3. **Postgres service**:
   - Uses environment variables with defaults: `${POSTGRES_USER:-postgres}`

## Image Naming Convention

Images are pushed to GHCR with the following naming:

- Backend: `ghcr.io/<github-username>/<repo-name>/backend:latest`
- Frontend: `ghcr.io/<github-username>/<repo-name>/frontend:latest`

Additionally tagged with commit SHA for traceability:
- `ghcr.io/<github-username>/<repo-name>/backend:main-<sha>`

## Health Checks

### Backend Health Check

After deployment, the workflow checks:
- Backend health endpoint: `http://localhost:8080/api/up`
- Expects JSON response: `{"status":"up"}`
- Retries up to 30 times (60 seconds total)
- Fails deployment if health check doesn't pass

### External Health Check

Final verification checks:
- Public health endpoint: `http://<VM_IP>/health`
- Ensures nginx is routing correctly

## Rollback Procedure

If a deployment fails or issues are discovered:

### Manual Rollback

1. SSH into Azure VM:
   ```bash
   ssh -i azure_vm.pem azureuser@<VM_IP>
   ```

2. Navigate to deployment directory:
   ```bash
   cd /home/azureuser/10xWarehouse
   ```

3. Pull previous image version:
   ```bash
   docker compose pull
   # Or manually specify a previous SHA tag
   docker compose down
   docker compose up -d
   ```

### Automatic Rollback

The workflow will fail if health checks don't pass, preventing broken deployments. The previous containers remain stopped, allowing for quick manual intervention.

## Security Considerations

1. **Authentication**:
   - Uses `GITHUB_TOKEN` (no PAT needed) - automatically scoped and expired
   - SSH keys stored securely in GitHub Secrets
   - GHCR authentication scoped to repository

2. **Secrets Management**:
   - All sensitive values stored in GitHub Secrets
   - Never committed to repository
   - Passed securely to Azure VM via SSH environment variables

3. **Network Security**:
   - Backend only exposed internally (no public port 8080 in production)
   - Nginx acts as reverse proxy
   - Rate limiting configured in nginx

4. **Image Security**:
   - Images built in isolated GitHub Actions runners
   - Cached layers for faster builds
   - Images tagged with commit SHA for traceability

## Monitoring and Troubleshooting

### Viewing Deployment Logs

1. **GitHub Actions**:
   - Go to repository → **Actions** tab
   - Click on latest workflow run
   - View logs for each step

2. **Azure VM**:
   ```bash
   ssh -i azure_vm.pem azureuser@<VM_IP>
   cd /home/azureuser/10xWarehouse
   docker compose logs -f
   ```

### Common Issues

**Deployment fails at health check:**
- Check backend logs: `docker compose logs backend`
- Verify Supabase credentials are correct
- Check database connectivity

**Images fail to pull:**
- Verify GHCR authentication is working
- Check repository permissions
- Ensure `GITHUB_TOKEN` has `packages: write` permission

**SSH connection fails:**
- Verify `AZURE_VM_HOST` and `AZURE_VM_USER` are correct
- Check SSH key format (must include BEGIN/END markers)
- Verify Azure NSG allows SSH from GitHub Actions IPs

## Local Development

For local development, use `docker-compose.local.yml` which builds images locally. The production `docker-compose.yml` should not be used locally as it requires GHCR images and specific environment variables.

## Manual Deployment

The workflow can be manually triggered:
- Go to repository → **Actions** → **Build and Deploy**
- Click **"Run workflow"** button
- Select branch (usually `main`)
- Click **"Run workflow"**

## Future Enhancements

Potential improvements:
1. **Staging environment** deployment
2. **Blue-green deployment** for zero-downtime
3. **Automated rollback** on health check failure
4. **Deployment notifications** (Slack, email, etc.)
5. **Database backup** before deployment
6. **Multi-region deployment** support

## Summary

This CI/CD pipeline provides:
- ✅ Automated builds on every push to `main`
- ✅ Secure image storage in GHCR
- ✅ Automated deployment to Azure VM
- ✅ Health verification after deployment
- ✅ Simple rollback capabilities
- ✅ No manual intervention required for routine deployments

The setup is designed to be simple, secure, and maintainable while providing the necessary automation for continuous deployment.

