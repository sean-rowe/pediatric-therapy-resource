#!/bin/bash
# Local development infrastructure setup for UPTRMS
# This script sets up the complete local Kubernetes environment with all services

set -euo pipefail

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Configuration
CLUSTER_NAME="uptrms-local"
NAMESPACES=("uptrms-production" "uptrms-staging" "uptrms-development" "uptrms-data" "uptrms-monitoring" "uptrms-logging")

# Logging functions
log_info() {
    echo -e "${GREEN}[INFO]${NC} $1"
}

log_warn() {
    echo -e "${YELLOW}[WARN]${NC} $1"
}

log_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

log_step() {
    echo -e "${BLUE}[STEP]${NC} $1"
}

# Check prerequisites
check_prerequisites() {
    log_step "Checking prerequisites..."
    
    local missing_tools=()
    
    # Check for required tools
    for tool in docker kubectl kind helm istioctl; do
        if ! command -v $tool &> /dev/null; then
            missing_tools+=($tool)
        fi
    done
    
    if [ ${#missing_tools[@]} -ne 0 ]; then
        log_error "Missing required tools: ${missing_tools[*]}"
        log_info "Please install the missing tools and try again."
        exit 1
    fi
    
    # Check Docker is running
    if ! docker info &> /dev/null; then
        log_error "Docker is not running. Please start Docker and try again."
        exit 1
    fi
    
    log_info "All prerequisites satisfied!"
}

# Create Kind cluster
create_cluster() {
    log_step "Creating Kind cluster..."
    
    if kind get clusters | grep -q "^${CLUSTER_NAME}$"; then
        log_warn "Cluster ${CLUSTER_NAME} already exists. Deleting..."
        kind delete cluster --name="${CLUSTER_NAME}"
    fi
    
    kind create cluster --config=kubernetes/kind-cluster.yaml --name="${CLUSTER_NAME}"
    
    # Set kubectl context
    kubectl cluster-info --context "kind-${CLUSTER_NAME}"
    
    log_info "Kind cluster created successfully!"
}

# Create namespaces
create_namespaces() {
    log_step "Creating namespaces..."
    
    for ns in "${NAMESPACES[@]}"; do
        kubectl create namespace "$ns" --dry-run=client -o yaml | kubectl apply -f -
        kubectl label namespace "$ns" name="$ns" --overwrite
    done
    
    log_info "Namespaces created!"
}

# Generate certificates
generate_certificates() {
    log_step "Generating certificates..."
    
    if [ ! -d "security/certificates/ca" ]; then
        cd security/certificates
        ./generate-certs.sh
        cd ../..
    else
        log_info "Certificates already exist. Skipping generation."
    fi
    
    # Create certificate secrets
    kubectl create secret generic ca-certificate \
        --from-file=ca.crt=security/certificates/ca/ca.crt \
        --namespace=uptrms-data \
        --dry-run=client -o yaml | kubectl apply -f -
    
    kubectl create secret generic mssql-certificate \
        --from-file=tls.crt=security/certificates/mssql/mssql.crt \
        --from-file=tls.key=security/certificates/mssql/mssql.key \
        --from-file=mssql.pfx=security/certificates/mssql/mssql.pfx \
        --namespace=uptrms-data \
        --dry-run=client -o yaml | kubectl apply -f -
    
    log_info "Certificates generated and secrets created!"
}

# Generate encryption keys
generate_keys() {
    log_step "Generating encryption keys..."
    
    if [ ! -d "security/encryption-keys/application-keys" ]; then
        cd security/encryption-keys
        ./generate-keys.sh
        cd ../..
    else
        log_info "Encryption keys already exist. Skipping generation."
    fi
    
    # Create key secrets
    kubectl create secret generic encryption-keys \
        --from-file=dek=security/encryption-keys/application-keys/data_encryption_key.key \
        --from-file=kek=security/encryption-keys/application-keys/key_encryption_key.key \
        --from-file=jwt=security/encryption-keys/application-keys/jwt_signing_key.key \
        --namespace=uptrms-production \
        --dry-run=client -o yaml | kubectl apply -f -
    
    log_info "Encryption keys generated and secrets created!"
}

# Install Istio service mesh
install_istio() {
    log_step "Installing Istio service mesh..."
    
    # Download Istio if not present
    if [ ! -d "istio-1.20.0" ]; then
        curl -L https://istio.io/downloadIstio | ISTIO_VERSION=1.20.0 sh -
    fi
    
    # Install Istio
    ./istio-1.20.0/bin/istioctl install --set profile=demo -y
    
    # Enable sidecar injection for namespaces
    for ns in "uptrms-production" "uptrms-staging" "uptrms-development"; do
        kubectl label namespace "$ns" istio-injection=enabled --overwrite
    done
    
    # Apply Istio configuration
    kubectl apply -f kubernetes/istio-setup.yaml
    
    log_info "Istio installed successfully!"
}

# Deploy data tier
deploy_data_tier() {
    log_step "Deploying data tier services..."
    
    # Deploy MSSQL
    kubectl apply -f kubernetes/mssql-deployment.yaml
    
    # Deploy Redis
    kubectl apply -f kubernetes/redis-deployment.yaml
    
    # Deploy RabbitMQ
    kubectl apply -f kubernetes/rabbitmq-deployment.yaml
    
    # Deploy MinIO
    kubectl apply -f kubernetes/minio-deployment.yaml
    
    log_info "Data tier deployed! Waiting for services to be ready..."
    
    # Wait for deployments
    kubectl wait --for=condition=available --timeout=300s deployment/mssql -n uptrms-data
    kubectl wait --for=condition=available --timeout=300s deployment/redis-master -n uptrms-data
    kubectl wait --for=condition=available --timeout=300s deployment/rabbitmq -n uptrms-data
    kubectl wait --for=condition=available --timeout=300s deployment/minio -n uptrms-data
    
    log_info "Data tier services are ready!"
}

# Run database migrations
run_migrations() {
    log_step "Running database migrations..."
    
    # Build migration container
    docker build -f docker/Dockerfile.migrations -t uptrms/migrations:latest .
    
    # Load image into Kind
    kind load docker-image uptrms/migrations:latest --name="${CLUSTER_NAME}"
    
    # Create migration job
    kubectl apply -f - <<EOF
apiVersion: batch/v1
kind: Job
metadata:
  name: database-migrations
  namespace: uptrms-data
spec:
  template:
    spec:
      restartPolicy: OnFailure
      containers:
      - name: migrations
        image: uptrms/migrations:latest
        imagePullPolicy: Never
        env:
        - name: DB_SERVER
          value: mssql-service
        - name: DB_PORT
          value: "1433"
        - name: DB_USER
          value: sa
        - name: DB_PASSWORD
          valueFrom:
            secretKeyRef:
              name: mssql-secret
              key: sa-password
        - name: DB_NAME
          value: UPTRMS
EOF
    
    # Wait for migration to complete
    kubectl wait --for=condition=complete --timeout=300s job/database-migrations -n uptrms-data
    
    log_info "Database migrations completed!"
}

# Deploy monitoring stack
deploy_monitoring() {
    log_step "Deploying monitoring stack..."
    
    # Deploy Prometheus
    kubectl apply -f monitoring/prometheus-config.yaml
    kubectl apply -f monitoring/alerts.yaml
    
    # Deploy Grafana
    kubectl apply -f monitoring/grafana-config.yaml
    
    # Create dashboard ConfigMap
    kubectl create configmap grafana-dashboards \
        --from-file=api-dashboard.json=monitoring/grafana-dashboards/api-dashboard.json \
        --from-file=database-dashboard.json=monitoring/grafana-dashboards/database-dashboard.json \
        --from-file=security-dashboard.json=monitoring/grafana-dashboards/security-dashboard.json \
        --namespace=uptrms-monitoring \
        --dry-run=client -o yaml | kubectl apply -f -
    
    log_info "Monitoring stack deployed!"
}

# Deploy logging stack
deploy_logging() {
    log_step "Deploying logging stack..."
    
    kubectl apply -f monitoring/logs-aggregation.yaml
    
    log_info "Logging stack deployed!"
}

# Apply security policies
apply_security_policies() {
    log_step "Applying security policies..."
    
    kubectl apply -f security/policies/pod-security.yaml
    kubectl apply -f security/policies/network-policies.yaml
    
    log_info "Security policies applied!"
}

# Deploy application (placeholder)
deploy_application() {
    log_step "Application deployment..."
    
    log_warn "Application containers not yet built. Skipping deployment."
    log_info "To deploy the application:"
    log_info "  1. Build API: docker build -f docker/Dockerfile.api -t uptrms/api:latest ."
    log_info "  2. Build Web: docker build -f docker/Dockerfile.web -t uptrms/web:latest ."
    log_info "  3. Load images: kind load docker-image uptrms/api:latest uptrms/web:latest --name=${CLUSTER_NAME}"
    log_info "  4. Deploy: kubectl apply -f kubernetes/app-deployment.yaml"
}

# Print access information
print_access_info() {
    log_step "Setup complete! Access information:"
    
    echo -e "${GREEN}═══════════════════════════════════════════════════════════════${NC}"
    echo -e "${GREEN}UPTRMS Local Development Environment${NC}"
    echo -e "${GREEN}═══════════════════════════════════════════════════════════════${NC}"
    
    # Get node port for services
    local api_port=$(kubectl get svc istio-ingressgateway -n istio-system -o jsonpath='{.spec.ports[?(@.name=="http2")].nodePort}')
    local grafana_port=$(kubectl get svc istio-ingressgateway -n istio-system -o jsonpath='{.spec.ports[?(@.name=="https")].nodePort}')
    
    echo -e "API Gateway: ${BLUE}https://localhost:${api_port}${NC}"
    echo -e "Grafana: ${BLUE}https://localhost:${grafana_port}/grafana${NC}"
    echo -e "RabbitMQ Management: ${BLUE}http://localhost:30002${NC}"
    echo -e "MinIO Console: ${BLUE}http://localhost:30003${NC}"
    
    echo -e "\n${YELLOW}Default Credentials:${NC}"
    echo -e "MSSQL SA Password: Get from secret: ${BLUE}kubectl get secret mssql-secret -n uptrms-data -o jsonpath='{.data.sa-password}' | base64 -d${NC}"
    echo -e "Grafana Admin: admin / ${BLUE}kubectl get secret grafana-secrets -n uptrms-monitoring -o jsonpath='{.data.admin-password}' | base64 -d${NC}"
    echo -e "RabbitMQ: guest / guest"
    echo -e "MinIO: minioadmin / minioadmin"
    
    echo -e "\n${YELLOW}Useful Commands:${NC}"
    echo -e "View pods: ${BLUE}kubectl get pods -A${NC}"
    echo -e "View logs: ${BLUE}kubectl logs -n <namespace> <pod-name>${NC}"
    echo -e "Port forward: ${BLUE}kubectl port-forward -n <namespace> <pod-name> <local-port>:<pod-port>${NC}"
    echo -e "Delete cluster: ${BLUE}kind delete cluster --name=${CLUSTER_NAME}${NC}"
    
    echo -e "${GREEN}═══════════════════════════════════════════════════════════════${NC}"
}

# Main execution
main() {
    log_info "Starting UPTRMS local infrastructure setup..."
    
    check_prerequisites
    create_cluster
    create_namespaces
    generate_certificates
    generate_keys
    install_istio
    deploy_data_tier
    run_migrations
    deploy_monitoring
    deploy_logging
    apply_security_policies
    deploy_application
    print_access_info
    
    log_info "Setup completed successfully!"
}

# Run main function
main "$@"