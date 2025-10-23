# 10xWarehouse Azure VM Deployment Guide

This comprehensive guide will walk you through deploying your 10xWarehouse application to an Azure Virtual Machine using Docker Compose.

## Table of Contents

- [Prerequisites](#prerequisites)
- [Quick Start](#quick-start)
- [Step-by-Step Deployment](#step-by-step-deployment)
- [Configuration](#configuration)
- [Verification](#verification)
- [Troubleshooting](#troubleshooting)
- [Maintenance](#maintenance)

## Prerequisites

Before starting, ensure you have:

- ✅ Azure CLI installed and configured
- ✅ Azure subscription with sufficient permissions
- ✅ Git repository with your 10xWarehouse code
- ✅ Supabase project credentials
- ✅ SSH client installed

## Quick Start

### Option A: Automated Deployment (Recommended)

1. **Run the VM deployment script:**
   ```bash
   chmod +x deploy-to-azure-vm.sh
   ./deploy-to-azure-vm.sh
   ```

2. **SSH into the VM:**
   ```bash
   ssh azureuser@<VM_IP_ADDRESS>
   ```

3. **Deploy the application:**
   ```bash
   chmod +x deploy-application.sh
   ./deploy-application.sh
   ```

### Option B: Manual Step-by-Step Deployment

Follow the detailed steps below for complete control over the deployment process.

## Step-by-Step Deployment

### Step 1: Azure CLI Setup

#### 1.1 Login to Azure
```bash
az login
```

#### 1.2 Verify Subscription
```bash
az account show
```

#### 1.3 Set Default Subscription (if multiple)
```bash
az account set --subscription "Your Subscription Name"
```

### Step 2: Create Resource Group

```bash
# Create resource group in Poland Central
az group create \
  --name 10xWarehouseRG \
  --location polandcentral
```

### Step 3: Create Virtual Network

```bash
# Create virtual network
az network vnet create \
  --resource-group 10xWarehouseRG \
  --name 10xWarehouseVNet \
  --address-prefix 10.0.0.0/16 \
  --subnet-name 10xWarehouseSubnet \
  --subnet-prefix 10.0.1.0/24
```

### Step 4: Create Public IP Address

```bash
# Create static public IP
az network public-ip create \
  --resource-group 10xWarehouseRG \
  --name 10xWarehousePublicIP \
  --allocation-method Static \
  --sku Standard
```

### Step 5: Create Network Security Group

```bash
# Create NSG
az network nsg create \
  --resource-group 10xWarehouseRG \
  --name 10xWarehouseNSG
```

#### 5.1 Add Security Rules

```bash
# SSH access (port 22)
az network nsg rule create \
  --resource-group 10xWarehouseRG \
  --nsg-name 10xWarehouseNSG \
  --name "AllowSSH" \
  --priority 1000 \
  --source-address-prefixes "*" \
  --source-port-ranges "*" \
  --destination-address-prefixes "*" \
  --destination-port-ranges 22 \
  --access Allow \
  --protocol Tcp \
  --description "Allow SSH access"

# Frontend access (port 4321)
az network nsg rule create \
  --resource-group 10xWarehouseRG \
  --nsg-name 10xWarehouseNSG \
  --name "AllowFrontend" \
  --priority 1001 \
  --source-address-prefixes "*" \
  --source-port-ranges "*" \
  --destination-address-prefixes "*" \
  --destination-port-ranges 4321 \
  --access Allow \
  --protocol Tcp \
  --description "Allow Frontend access"

# Backend API access (port 8080)
az network nsg rule create \
  --resource-group 10xWarehouseRG \
  --nsg-name 10xWarehouseNSG \
  --name "AllowBackendAPI" \
  --priority 1002 \
  --source-address-prefixes "*" \
  --source-port-ranges "*" \
  --destination-address-prefixes "*" \
  --destination-port-ranges 8080 \
  --access Allow \
  --protocol Tcp \
  --description "Allow Backend API access"

# PostgreSQL access (VM subnet only)
az network nsg rule create \
  --resource-group 10xWarehouseRG \
  --nsg-name 10xWarehouseNSG \
  --name "AllowPostgreSQL" \
  --priority 1003 \
  --source-address-prefixes "10.0.1.0/24" \
  --source-port-ranges "*" \
  --destination-address-prefixes "*" \
  --destination-port-ranges 5432 \
  --access Allow \
  --protocol Tcp \
  --description "Allow PostgreSQL access from VM subnet only"
```

### Step 6: Create Network Interface

```bash
# Create NIC
az network nic create \
  --resource-group 10xWarehouseRG \
  --name 10xWarehouseVMNIC \
  --vnet-name 10xWarehouseVNet \
  --subnet 10xWarehouseSubnet \
  --public-ip-address 10xWarehousePublicIP \
  --network-security-group 10xWarehouseNSG
```

### Step 7: Create Virtual Machine

```bash
# Create VM with cloud-init
az vm create \
  --resource-group 10xWarehouseRG \
  --name 10xWarehouseVM \
  --image Ubuntu2204 \
  --size Standard_B2s \
  --admin-username azureuser \
  --generate-ssh-keys \
  --nics 10xWarehouseVMNIC \
  --custom-data cloud-init-docker.yml
```

### Step 8: Get VM IP Address

```bash
# Get public IP
az vm show \
  --resource-group 10xWarehouseRG \
  --name 10xWarehouseVM \
  --show-details \
  --query publicIps \
  --output tsv
```

### Step 9: Connect to VM

```bash
# SSH into VM (replace with your actual IP)
ssh azureuser@<VM_IP_ADDRESS>
```

### Step 10: Deploy Application

#### 10.1 Clone Repository

```bash
# Navigate to home directory
cd /home/azureuser

# Clone your repository
git clone https://github.com/your-username/10xWarehouse.git
cd 10xWarehouse
```

#### 10.2 Configure Environment Variables

```bash
# Copy environment template
cp env.template .env

# Edit environment file
nano .env
```

Required environment variables:
```env
SUPABASE_URL=https://your-project-id.supabase.co
SUPABASE_SERVICE_ROLE_KEY=your-service-role-key-here
```

#### 10.3 Deploy with Docker Compose

```bash
# Start all services
docker-compose up -d

# Check status
docker-compose ps

# View logs
docker-compose logs -f
```

## Configuration

### Docker Compose Configuration

Your `docker-compose.yml` should include:

```yaml
services:
  postgres:
    image: postgres:16-alpine
    environment:
      POSTGRES_USER: postgres
      POSTGRES_PASSWORD: postgres
      POSTGRES_DB: postgres
    ports:
      - "5432:5432"
    volumes:
      - pgdata:/var/lib/postgresql/data
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -U postgres || exit 1"]
      interval: 5s
      timeout: 3s
      retries: 10

  backend:
    build:
      context: .
      dockerfile: 10xWarehouseNet/Dockerfile
    environment:
      ASPNETCORE_ENVIRONMENT: Production
      ConnectionStrings__DefaultConnection: Host=postgres;Port=5432;Database=postgres;Username=postgres;Password=postgres
      Supabase__Url: ${SUPABASE_URL}
      Supabase__ServiceRoleKey: ${SUPABASE_SERVICE_ROLE_KEY}
    depends_on:
      postgres:
        condition: service_healthy
    ports:
      - "8080:8080"

  frontend:
    build:
      context: .
      dockerfile: 10xWarehouseAstro/astro-app/Dockerfile
      args:
        PUBLIC_API_BASE_URL: http://localhost:8080/api
    depends_on:
      - backend
    ports:
      - "4321:4321"

volumes:
  pgdata:
```

### Environment Variables

Create `.env` file with:

```env
# Supabase Configuration
SUPABASE_URL=https://your-project-id.supabase.co
SUPABASE_SERVICE_ROLE_KEY=your-service-role-key-here

# Optional: Custom configurations
# CUSTOM_VAR=value
```

## Verification

### Health Checks

#### Check Container Status
```bash
docker-compose ps
```

Expected output:
```
Name                     Command               State           Ports         
-----------------------------------------------------------------------------
10xwarehouse_backend_1   dotnet 10xWarehouseNet.dll   Up      0.0.0.0:8080->8080/tcp
10xwarehouse_frontend_1  node ./dist/server/entry.mjs   Up      0.0.0.0:4321->4321/tcp
10xwarehouse_postgres_1  docker-entrypoint.sh postgres  Up      0.0.0.0:5432->5432/tcp
```

#### Test PostgreSQL
```bash
docker-compose exec postgres pg_isready -U postgres
```

#### Test Backend API
```bash
curl http://localhost:8080/health
```

#### Test Frontend
```bash
curl http://localhost:4321
```

### Access URLs

- **Frontend**: `http://<VM_IP>:4321`
- **Backend API**: `http://<VM_IP>:8080/api`
- **Health Check**: `http://<VM_IP>:8080/health`

## Troubleshooting

### Common Issues

#### 1. SSH Connection Failed

**Symptoms**: Cannot connect to VM via SSH

**Solutions**:
```bash
# Check VM status
az vm show --name 10xWarehouseVM --resource-group 10xWarehouseRG

# Check NSG rules
az network nsg rule list --resource-group 10xWarehouseRG --nsg-name 10xWarehouseNSG

# Wait for VM initialization (2-3 minutes)
# Try connecting again
```

#### 2. Docker Not Running

**Symptoms**: Docker commands fail

**Solutions**:
```bash
# Check Docker status
sudo systemctl status docker

# Start Docker
sudo systemctl start docker
sudo systemctl enable docker

# Check Docker logs
sudo journalctl -u docker.service
```

#### 3. Application Not Accessible

**Symptoms**: Cannot access frontend/backend from browser

**Solutions**:
```bash
# Check NSG rules for ports 4321 and 8080
az network nsg rule list --resource-group 10xWarehouseRG --nsg-name 10xWarehouseNSG

# Check container status
docker-compose ps

# Check logs
docker-compose logs

# Test local connectivity
curl http://localhost:8080/health
curl http://localhost:4321
```

#### 4. Database Connection Issues

**Symptoms**: Backend cannot connect to PostgreSQL

**Solutions**:
```bash
# Check PostgreSQL container
docker-compose exec postgres pg_isready -U postgres

# Check connection string in docker-compose.yml
# Verify network connectivity between containers
docker network ls
docker network inspect 10xwarehouse_default
```

### Log Locations

- **Application logs**: `docker-compose logs`
- **System logs**: `journalctl -u 10xwarehouse.service`
- **Docker logs**: `sudo journalctl -u docker.service`

## Maintenance

### Regular Tasks

#### Update Application
```bash
# Pull latest changes
git pull origin main

# Rebuild and restart
docker-compose down
docker-compose up -d --build
```

#### Backup Database
```bash
# Create backup
docker-compose exec postgres pg_dump -U postgres postgres > backup_$(date +%Y%m%d_%H%M%S).sql

# Restore from backup
docker-compose exec -T postgres psql -U postgres postgres < backup_file.sql
```

#### Monitor Resources
```bash
# Check system resources
htop
df -h
free -h

# Check Docker resource usage
docker stats
```

### Auto-Start Configuration

Enable automatic startup on VM boot:

```bash
# Enable systemd service
sudo systemctl enable 10xwarehouse.service
sudo systemctl start 10xwarehouse.service

# Check status
sudo systemctl status 10xwarehouse.service
```

### Security Considerations

1. **Regular Updates**:
   ```bash
   # Update system packages
   sudo apt update && sudo apt upgrade -y
   
   # Update Docker images
   docker-compose pull
   ```

2. **Firewall Configuration**:
   ```bash
   # Install and configure UFW
   sudo ufw enable
   sudo ufw allow ssh
   sudo ufw allow 4321
   sudo ufw allow 8080
   ```

3. **SSL/TLS Setup** (Optional):
   - Use Let's Encrypt with Certbot
   - Configure reverse proxy with Nginx
   - Enable HTTPS for production use

## Cost Optimization

### VM Size Recommendations

- **Development**: Standard_B1s (1 vCPU, 1GB RAM) - ~$15/month
- **Production**: Standard_B2s (2 vCPUs, 4GB RAM) - ~$30/month
- **High Load**: Standard_B4s (4 vCPUs, 16GB RAM) - ~$60/month

### Cost Monitoring

```bash
# Set up billing alerts in Azure portal
# Monitor usage with Azure Cost Management
# Consider auto-shutdown for development environments
```

## Scaling Options

When your application grows:

1. **Azure Container Instances (ACI)**: Simpler than VM management
2. **Azure Kubernetes Service (AKS)**: For complex orchestration
3. **Azure App Service**: Managed application hosting
4. **Multiple VMs**: With load balancer for high availability

## Support

For additional help:

1. Check the troubleshooting section above
2. Review Azure VM logs in the Azure portal
3. Check Docker and application logs on the VM
4. Verify network security group rules
5. Consult Azure documentation for VM-specific issues

---

**Estimated Monthly Cost**: $30-50 (Standard_B2s VM)  
**Deployment Time**: 10-15 minutes  
**Maintenance**: Minimal with auto-start enabled
