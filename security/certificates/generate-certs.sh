#!/bin/bash
# Generate self-signed certificates for local HTTPS development
# HIPAA requires encryption in transit, so all services must use TLS

set -euo pipefail

CERT_DIR="$(dirname "$0")"
DAYS_VALID=365

# Create directories if they don't exist
mkdir -p "$CERT_DIR"/{ca,api,web,mssql,elasticsearch,kibana,rabbitmq,redis}

echo "Generating Certificate Authority (CA)..."
# Generate CA private key
openssl genrsa -out "$CERT_DIR/ca/ca.key" 4096

# Generate CA certificate
openssl req -new -x509 -days $DAYS_VALID -key "$CERT_DIR/ca/ca.key" -out "$CERT_DIR/ca/ca.crt" \
  -subj "/C=US/ST=State/L=City/O=UPTRMS/CN=UPTRMS Root CA"

echo "Generating API certificates..."
# Generate API private key
openssl genrsa -out "$CERT_DIR/api/api.key" 2048

# Generate API certificate request
openssl req -new -key "$CERT_DIR/api/api.key" -out "$CERT_DIR/api/api.csr" \
  -subj "/C=US/ST=State/L=City/O=UPTRMS/CN=uptrms-api.local"

# Create API certificate extensions file
cat > "$CERT_DIR/api/api.ext" <<EOF
authorityKeyIdentifier=keyid,issuer
basicConstraints=CA:FALSE
keyUsage = digitalSignature, nonRepudiation, keyEncipherment, dataEncipherment
subjectAltName = @alt_names

[alt_names]
DNS.1 = uptrms-api.local
DNS.2 = uptrms-api
DNS.3 = localhost
IP.1 = 127.0.0.1
IP.2 = ::1
EOF

# Sign API certificate
openssl x509 -req -in "$CERT_DIR/api/api.csr" -CA "$CERT_DIR/ca/ca.crt" -CAkey "$CERT_DIR/ca/ca.key" \
  -CAcreateserial -out "$CERT_DIR/api/api.crt" -days $DAYS_VALID -sha256 -extfile "$CERT_DIR/api/api.ext"

echo "Generating Web certificates..."
# Generate Web private key
openssl genrsa -out "$CERT_DIR/web/web.key" 2048

# Generate Web certificate request
openssl req -new -key "$CERT_DIR/web/web.key" -out "$CERT_DIR/web/web.csr" \
  -subj "/C=US/ST=State/L=City/O=UPTRMS/CN=uptrms-web.local"

# Create Web certificate extensions file
cat > "$CERT_DIR/web/web.ext" <<EOF
authorityKeyIdentifier=keyid,issuer
basicConstraints=CA:FALSE
keyUsage = digitalSignature, nonRepudiation, keyEncipherment, dataEncipherment
subjectAltName = @alt_names

[alt_names]
DNS.1 = uptrms-web.local
DNS.2 = uptrms-web
DNS.3 = localhost
DNS.4 = *.uptrms.local
IP.1 = 127.0.0.1
IP.2 = ::1
EOF

# Sign Web certificate
openssl x509 -req -in "$CERT_DIR/web/web.csr" -CA "$CERT_DIR/ca/ca.crt" -CAkey "$CERT_DIR/ca/ca.key" \
  -CAcreateserial -out "$CERT_DIR/web/web.crt" -days $DAYS_VALID -sha256 -extfile "$CERT_DIR/web/web.ext"

echo "Generating MSSQL certificates..."
# MSSQL requires special certificate format
openssl genrsa -out "$CERT_DIR/mssql/mssql.key" 2048

openssl req -new -key "$CERT_DIR/mssql/mssql.key" -out "$CERT_DIR/mssql/mssql.csr" \
  -subj "/C=US/ST=State/L=City/O=UPTRMS/CN=mssql.uptrms.local"

# MSSQL specific extensions
cat > "$CERT_DIR/mssql/mssql.ext" <<EOF
authorityKeyIdentifier=keyid,issuer
basicConstraints=CA:FALSE
keyUsage = digitalSignature, keyEncipherment
extendedKeyUsage = serverAuth
subjectAltName = @alt_names

[alt_names]
DNS.1 = mssql.uptrms.local
DNS.2 = mssql-service
DNS.3 = mssql-0.mssql-service
DNS.4 = localhost
IP.1 = 127.0.0.1
EOF

openssl x509 -req -in "$CERT_DIR/mssql/mssql.csr" -CA "$CERT_DIR/ca/ca.crt" -CAkey "$CERT_DIR/ca/ca.key" \
  -CAcreateserial -out "$CERT_DIR/mssql/mssql.crt" -days $DAYS_VALID -sha256 -extfile "$CERT_DIR/mssql/mssql.ext"

# Convert to PKCS12 for MSSQL
openssl pkcs12 -export -out "$CERT_DIR/mssql/mssql.pfx" -inkey "$CERT_DIR/mssql/mssql.key" \
  -in "$CERT_DIR/mssql/mssql.crt" -password pass:P@ssw0rd123

echo "Generating Elasticsearch certificates..."
# Elasticsearch uses PKCS12 format
openssl genrsa -out "$CERT_DIR/elasticsearch/elastic.key" 2048

openssl req -new -key "$CERT_DIR/elasticsearch/elastic.key" -out "$CERT_DIR/elasticsearch/elastic.csr" \
  -subj "/C=US/ST=State/L=City/O=UPTRMS/CN=elasticsearch.uptrms.local"

cat > "$CERT_DIR/elasticsearch/elastic.ext" <<EOF
authorityKeyIdentifier=keyid,issuer
basicConstraints=CA:FALSE
keyUsage = digitalSignature, keyEncipherment
extendedKeyUsage = serverAuth, clientAuth
subjectAltName = @alt_names

[alt_names]
DNS.1 = elasticsearch
DNS.2 = elasticsearch.uptrms-logging
DNS.3 = elasticsearch-0.elasticsearch
DNS.4 = elasticsearch-1.elasticsearch
DNS.5 = elasticsearch-2.elasticsearch
DNS.6 = localhost
IP.1 = 127.0.0.1
EOF

openssl x509 -req -in "$CERT_DIR/elasticsearch/elastic.csr" -CA "$CERT_DIR/ca/ca.crt" -CAkey "$CERT_DIR/ca/ca.key" \
  -CAcreateserial -out "$CERT_DIR/elasticsearch/elastic.crt" -days $DAYS_VALID -sha256 -extfile "$CERT_DIR/elasticsearch/elastic.ext"

# Convert to PKCS12
openssl pkcs12 -export -out "$CERT_DIR/elasticsearch/elastic-certificates.p12" \
  -inkey "$CERT_DIR/elasticsearch/elastic.key" -in "$CERT_DIR/elasticsearch/elastic.crt" \
  -CAfile "$CERT_DIR/ca/ca.crt" -caname "UPTRMS Root CA" -password pass:elastic123

echo "Generating Kibana certificates..."
openssl genrsa -out "$CERT_DIR/kibana/kibana.key" 2048

openssl req -new -key "$CERT_DIR/kibana/kibana.key" -out "$CERT_DIR/kibana/kibana.csr" \
  -subj "/C=US/ST=State/L=City/O=UPTRMS/CN=kibana.uptrms.local"

cat > "$CERT_DIR/kibana/kibana.ext" <<EOF
authorityKeyIdentifier=keyid,issuer
basicConstraints=CA:FALSE
keyUsage = digitalSignature, keyEncipherment
extendedKeyUsage = serverAuth
subjectAltName = @alt_names

[alt_names]
DNS.1 = kibana
DNS.2 = kibana.uptrms-logging
DNS.3 = localhost
IP.1 = 127.0.0.1
EOF

openssl x509 -req -in "$CERT_DIR/kibana/kibana.csr" -CA "$CERT_DIR/ca/ca.crt" -CAkey "$CERT_DIR/ca/ca.key" \
  -CAcreateserial -out "$CERT_DIR/kibana/kibana.crt" -days $DAYS_VALID -sha256 -extfile "$CERT_DIR/kibana/kibana.ext"

echo "Generating RabbitMQ certificates..."
openssl genrsa -out "$CERT_DIR/rabbitmq/rabbitmq.key" 2048

openssl req -new -key "$CERT_DIR/rabbitmq/rabbitmq.key" -out "$CERT_DIR/rabbitmq/rabbitmq.csr" \
  -subj "/C=US/ST=State/L=City/O=UPTRMS/CN=rabbitmq.uptrms.local"

cat > "$CERT_DIR/rabbitmq/rabbitmq.ext" <<EOF
authorityKeyIdentifier=keyid,issuer
basicConstraints=CA:FALSE
keyUsage = digitalSignature, keyEncipherment
extendedKeyUsage = serverAuth, clientAuth
subjectAltName = @alt_names

[alt_names]
DNS.1 = rabbitmq
DNS.2 = rabbitmq-service
DNS.3 = rabbitmq-service.uptrms-data
DNS.4 = localhost
IP.1 = 127.0.0.1
EOF

openssl x509 -req -in "$CERT_DIR/rabbitmq/rabbitmq.csr" -CA "$CERT_DIR/ca/ca.crt" -CAkey "$CERT_DIR/ca/ca.key" \
  -CAcreateserial -out "$CERT_DIR/rabbitmq/rabbitmq.crt" -days $DAYS_VALID -sha256 -extfile "$CERT_DIR/rabbitmq/rabbitmq.ext"

echo "Generating Redis certificates..."
openssl genrsa -out "$CERT_DIR/redis/redis.key" 2048

openssl req -new -key "$CERT_DIR/redis/redis.key" -out "$CERT_DIR/redis/redis.csr" \
  -subj "/C=US/ST=State/L=City/O=UPTRMS/CN=redis.uptrms.local"

cat > "$CERT_DIR/redis/redis.ext" <<EOF
authorityKeyIdentifier=keyid,issuer
basicConstraints=CA:FALSE
keyUsage = digitalSignature, keyEncipherment
extendedKeyUsage = serverAuth, clientAuth
subjectAltName = @alt_names

[alt_names]
DNS.1 = redis
DNS.2 = redis-service
DNS.3 = redis-service.uptrms-data
DNS.4 = redis-master
DNS.5 = redis-replica
DNS.6 = localhost
IP.1 = 127.0.0.1
EOF

openssl x509 -req -in "$CERT_DIR/redis/redis.csr" -CA "$CERT_DIR/ca/ca.crt" -CAkey "$CERT_DIR/ca/ca.key" \
  -CAcreateserial -out "$CERT_DIR/redis/redis.crt" -days $DAYS_VALID -sha256 -extfile "$CERT_DIR/redis/redis.ext"

# Clean up CSR and extension files
find "$CERT_DIR" -name "*.csr" -delete
find "$CERT_DIR" -name "*.ext" -delete

# Create combined certificate chains
for service in api web mssql elasticsearch kibana rabbitmq redis; do
  if [ -f "$CERT_DIR/$service/$service.crt" ]; then
    cat "$CERT_DIR/$service/$service.crt" "$CERT_DIR/ca/ca.crt" > "$CERT_DIR/$service/$service-chain.crt"
  fi
done

# Set appropriate permissions
find "$CERT_DIR" -name "*.key" -exec chmod 600 {} \;
find "$CERT_DIR" -name "*.crt" -exec chmod 644 {} \;
find "$CERT_DIR" -name "*.pfx" -exec chmod 600 {} \;
find "$CERT_DIR" -name "*.p12" -exec chmod 600 {} \;

echo "Certificate generation complete!"
echo ""
echo "CA certificate: $CERT_DIR/ca/ca.crt"
echo ""
echo "To trust the CA certificate on your system:"
echo "  - macOS: sudo security add-trusted-cert -d -r trustRoot -k /Library/Keychains/System.keychain $CERT_DIR/ca/ca.crt"
echo "  - Ubuntu: sudo cp $CERT_DIR/ca/ca.crt /usr/local/share/ca-certificates/uptrms-ca.crt && sudo update-ca-certificates"
echo "  - Windows: Import ca.crt into Trusted Root Certification Authorities"
echo ""
echo "Service certificates generated:"
echo "  - API: $CERT_DIR/api/"
echo "  - Web: $CERT_DIR/web/"
echo "  - MSSQL: $CERT_DIR/mssql/ (includes .pfx for SQL Server)"
echo "  - Elasticsearch: $CERT_DIR/elasticsearch/ (includes .p12)"
echo "  - Kibana: $CERT_DIR/kibana/"
echo "  - RabbitMQ: $CERT_DIR/rabbitmq/"
echo "  - Redis: $CERT_DIR/redis/"