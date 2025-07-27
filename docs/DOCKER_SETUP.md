# Docker Setup for TherapyDocs

## Installing Docker Desktop with WSL2 Integration

### Step 1: Install Docker Desktop on Windows

1. Download Docker Desktop from: https://www.docker.com/products/docker-desktop/
2. Run the installer
3. During installation, ensure "Use WSL 2 instead of Hyper-V" is selected
4. Restart your computer when prompted

### Step 2: Enable WSL Integration

1. Open Docker Desktop
2. Go to Settings → Resources → WSL Integration
3. Enable integration with your WSL distro
4. Apply & Restart

### Step 3: Verify Installation

In your WSL terminal, run:
```bash
docker --version
docker-compose --version
```

## Alternative: Using Docker in WSL2 Directly

If you prefer not to use Docker Desktop, you can install Docker directly in WSL2:

```bash
# Update packages
sudo apt update

# Install prerequisites
sudo apt install -y \
    apt-transport-https \
    ca-certificates \
    curl \
    gnupg \
    lsb-release

# Add Docker's GPG key
curl -fsSL https://download.docker.com/linux/ubuntu/gpg | sudo gpg --dearmor -o /usr/share/keyrings/docker-archive-keyring.gpg

# Add Docker repository
echo \
  "deb [arch=$(dpkg --print-architecture) signed-by=/usr/share/keyrings/docker-archive-keyring.gpg] https://download.docker.com/linux/ubuntu \
  $(lsb_release -cs) stable" | sudo tee /etc/apt/sources.list.d/docker.list > /dev/null

# Install Docker
sudo apt update
sudo apt install -y docker-ce docker-ce-cli containerd.io docker-compose-plugin

# Add your user to docker group
sudo usermod -aG docker $USER

# Start Docker service
sudo service docker start

# Test installation
docker run hello-world
```

## Running TherapyDocs Database

Once Docker is installed and working:

```bash
cd /home/srowe/workspace/therapy-docs

# Start the database
./scripts/start-db.sh

# The database will be available at:
# Server: localhost,1433
# Database: TherapyDocs
# Username: SA
# Password: TherapyDocs2024!
```

## Troubleshooting

### If Docker service won't start in WSL2:
```bash
# Check if systemd is enabled
if [ -f /etc/wsl.conf ]; then
    cat /etc/wsl.conf
else
    echo "[boot]" | sudo tee /etc/wsl.conf
    echo "systemd=true" | sudo tee -a /etc/wsl.conf
fi

# Restart WSL
wsl --shutdown
# Then reopen your WSL terminal
```

### If port 1433 is already in use:
Edit `docker-compose.yml` and change the port mapping:
```yaml
ports:
  - "1434:1433"  # Use port 1434 instead
```

## Next Steps

After Docker is running:

1. Start the database: `./scripts/start-db.sh`
2. Connect to verify: `./scripts/connect-db.sh`
3. Check the logs: `docker-compose logs mssql`

The complete schema with all 32 tables will be automatically created!