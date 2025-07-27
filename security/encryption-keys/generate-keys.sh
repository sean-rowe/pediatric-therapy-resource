#!/bin/bash
# Generate encryption keys for HIPAA-compliant data encryption
# These keys are used for Always Encrypted columns in MSSQL and application-level encryption

set -euo pipefail

KEY_DIR="$(dirname "$0")"

# Create directories if they don't exist
mkdir -p "$KEY_DIR"/{column-master-keys,column-encryption-keys,application-keys}

echo "Generating Column Master Keys for Always Encrypted..."
# Generate RSA key pair for Column Master Key (CMK)
openssl genrsa -out "$KEY_DIR/column-master-keys/cmk_private.pem" 4096
openssl rsa -in "$KEY_DIR/column-master-keys/cmk_private.pem" -pubout -out "$KEY_DIR/column-master-keys/cmk_public.pem"

# Create self-signed certificate for CMK (MSSQL prefers certificates)
openssl req -new -x509 -key "$KEY_DIR/column-master-keys/cmk_private.pem" \
  -out "$KEY_DIR/column-master-keys/cmk_certificate.crt" -days 3650 \
  -subj "/C=US/ST=State/L=City/O=UPTRMS/CN=UPTRMS Column Master Key"

# Convert to PKCS12 for Windows/MSSQL compatibility
openssl pkcs12 -export -out "$KEY_DIR/column-master-keys/cmk_certificate.pfx" \
  -inkey "$KEY_DIR/column-master-keys/cmk_private.pem" \
  -in "$KEY_DIR/column-master-keys/cmk_certificate.crt" \
  -password pass:CMK@P@ssw0rd123

echo "Generating Column Encryption Keys..."
# Generate AES-256 keys for Column Encryption Keys (CEK)
# These are typically encrypted with the CMK in the database
for i in {1..3}; do
  openssl rand -base64 32 > "$KEY_DIR/column-encryption-keys/cek_${i}.key"
  
  # Encrypt CEK with CMK public key (simulating what MSSQL does)
  openssl rsautl -encrypt -pubin -inkey "$KEY_DIR/column-master-keys/cmk_public.pem" \
    -in "$KEY_DIR/column-encryption-keys/cek_${i}.key" \
    -out "$KEY_DIR/column-encryption-keys/cek_${i}_encrypted.bin"
done

echo "Generating Application-level Encryption Keys..."
# Data Encryption Key (DEK) for application-level encryption
openssl rand -base64 32 > "$KEY_DIR/application-keys/data_encryption_key.key"

# Key Encryption Key (KEK) for key wrapping
openssl rand -base64 32 > "$KEY_DIR/application-keys/key_encryption_key.key"

# JWT signing key
openssl rand -base64 64 > "$KEY_DIR/application-keys/jwt_signing_key.key"

# API encryption key for sensitive API payloads
openssl rand -base64 32 > "$KEY_DIR/application-keys/api_encryption_key.key"

# Session encryption key
openssl rand -base64 32 > "$KEY_DIR/application-keys/session_encryption_key.key"

# File encryption key for uploaded documents
openssl rand -base64 32 > "$KEY_DIR/application-keys/file_encryption_key.key"

# Backup encryption key
openssl rand -base64 32 > "$KEY_DIR/application-keys/backup_encryption_key.key"

echo "Generating Transparent Data Encryption (TDE) keys..."
mkdir -p "$KEY_DIR/tde-keys"

# TDE Certificate for MSSQL
openssl req -new -x509 -days 3650 -nodes -newkey rsa:2048 \
  -keyout "$KEY_DIR/tde-keys/tde_certificate.key" \
  -out "$KEY_DIR/tde-keys/tde_certificate.crt" \
  -subj "/C=US/ST=State/L=City/O=UPTRMS/CN=UPTRMS TDE Certificate"

# Convert to PKCS12
openssl pkcs12 -export -out "$KEY_DIR/tde-keys/tde_certificate.pfx" \
  -inkey "$KEY_DIR/tde-keys/tde_certificate.key" \
  -in "$KEY_DIR/tde-keys/tde_certificate.crt" \
  -password pass:TDE@P@ssw0rd123

echo "Generating service-specific encryption keys..."
mkdir -p "$KEY_DIR/service-keys"

# Redis encryption key
openssl rand -base64 32 > "$KEY_DIR/service-keys/redis_encryption.key"

# RabbitMQ cookie (for cluster authentication)
openssl rand -hex 20 > "$KEY_DIR/service-keys/rabbitmq_erlang_cookie"

# Elasticsearch encryption keys
openssl rand -base64 32 > "$KEY_DIR/service-keys/elasticsearch_encryption.key"
openssl rand -base64 32 > "$KEY_DIR/service-keys/kibana_encryption.key"

# MinIO encryption keys
openssl rand -base64 32 > "$KEY_DIR/service-keys/minio_master_key.key"

echo "Generating HashiCorp Vault unseal keys (for production)..."
mkdir -p "$KEY_DIR/vault-keys"

# Generate 5 unseal keys (Shamir's Secret Sharing)
for i in {1..5}; do
  openssl rand -base64 32 > "$KEY_DIR/vault-keys/unseal_key_${i}.key"
done

# Vault root token
openssl rand -base64 32 > "$KEY_DIR/vault-keys/root_token.key"

echo "Creating key metadata file..."
cat > "$KEY_DIR/keys-metadata.json" <<EOF
{
  "generated_at": "$(date -u +"%Y-%m-%dT%H:%M:%SZ")",
  "key_rotation_schedule": "90_days",
  "compliance": "HIPAA",
  "keys": {
    "column_master_key": {
      "algorithm": "RSA-4096",
      "purpose": "Encrypts column encryption keys in MSSQL Always Encrypted",
      "location": "column-master-keys/cmk_certificate.pfx"
    },
    "column_encryption_keys": {
      "algorithm": "AES-256",
      "purpose": "Encrypts sensitive columns in database",
      "count": 3
    },
    "data_encryption_key": {
      "algorithm": "AES-256",
      "purpose": "Application-level data encryption",
      "rotation": "quarterly"
    },
    "jwt_signing_key": {
      "algorithm": "HS512",
      "purpose": "JWT token signing",
      "rotation": "monthly"
    },
    "tde_certificate": {
      "algorithm": "RSA-2048",
      "purpose": "Transparent Data Encryption for MSSQL",
      "location": "tde-keys/tde_certificate.pfx"
    }
  }
}
EOF

echo "Setting secure permissions..."
# Set restrictive permissions on all keys
find "$KEY_DIR" -type f -name "*.key" -exec chmod 600 {} \;
find "$KEY_DIR" -type f -name "*.pem" -exec chmod 600 {} \;
find "$KEY_DIR" -type f -name "*.pfx" -exec chmod 600 {} \;
find "$KEY_DIR" -type f -name "*.bin" -exec chmod 600 {} \;
find "$KEY_DIR" -type f -name "*cookie" -exec chmod 600 {} \;

# Public keys and certificates can be read-only
find "$KEY_DIR" -type f -name "*public.pem" -exec chmod 644 {} \;
find "$KEY_DIR" -type f -name "*.crt" -exec chmod 644 {} \;
chmod 644 "$KEY_DIR/keys-metadata.json"

echo "Creating .gitignore to prevent key commits..."
cat > "$KEY_DIR/.gitignore" <<EOF
# Ignore all keys and sensitive files
*.key
*.pem
*.pfx
*.bin
*.p12
*_cookie
unseal_key_*
root_token.key

# But allow these files
!.gitignore
!generate-keys.sh
!README.md
EOF

echo "Creating README for key management..."
cat > "$KEY_DIR/README.md" <<EOF
# Encryption Keys for UPTRMS

## ⚠️ CRITICAL SECURITY NOTICE ⚠️

These keys are for LOCAL DEVELOPMENT ONLY. 
NEVER commit these keys to version control.
NEVER use these keys in production.

## Production Key Management

In production, keys should be:
1. Generated in a secure environment
2. Stored in HashiCorp Vault or Azure Key Vault
3. Rotated regularly (see keys-metadata.json)
4. Backed up securely with encryption
5. Access-controlled with audit logging

## Key Hierarchy

1. **Column Master Key (CMK)**: Protects column encryption keys
2. **Column Encryption Keys (CEK)**: Encrypt sensitive database columns
3. **Data Encryption Key (DEK)**: Application-level encryption
4. **Key Encryption Key (KEK)**: Wraps other keys for storage
5. **Service Keys**: Individual keys for each service

## Key Rotation Schedule

- JWT Signing Key: Monthly
- API Encryption Keys: Quarterly
- Column Encryption Keys: Quarterly
- Master Keys: Annually
- TDE Certificate: Every 10 years

## HIPAA Compliance

All keys meet HIPAA encryption requirements:
- AES-256 for symmetric encryption
- RSA-4096 for asymmetric encryption
- Secure key generation using cryptographically secure random
- Proper key storage with restricted permissions
EOF

echo "Key generation complete!"
echo ""
echo "⚠️  IMPORTANT: These keys are for development only!"
echo "⚠️  Never commit these keys to version control!"
echo "⚠️  In production, use a proper key management service!"
echo ""
echo "Keys generated:"
echo "  - Column Master Keys: $KEY_DIR/column-master-keys/"
echo "  - Column Encryption Keys: $KEY_DIR/column-encryption-keys/"
echo "  - Application Keys: $KEY_DIR/application-keys/"
echo "  - TDE Keys: $KEY_DIR/tde-keys/"
echo "  - Service Keys: $KEY_DIR/service-keys/"
echo "  - Vault Keys: $KEY_DIR/vault-keys/"
echo ""
echo "Next steps:"
echo "1. Back up these keys securely"
echo "2. Configure your application to use these keys"
echo "3. Set up key rotation procedures"
echo "4. Implement proper key management for production"