#!/bin/bash
#
# Setup script for ModSync Telemetry Authentication
#
# This script:
# 1. Generates a secure HMAC signing secret
# 2. Saves it to the volumes directory
# 3. Builds the auth service container
# 4. Deploys the metrics stack
#

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(dirname "$SCRIPT_DIR")"
SECRET_FILE="${PROJECT_ROOT}/volumes/kotormodsync_signing_secret.txt"

echo "======================================================================"
echo "ModSync Telemetry Authentication Setup"
echo "======================================================================"
echo ""

# Check if secret already exists
if [ -f "$SECRET_FILE" ]; then
    echo "⚠️  Signing secret already exists at:"
    echo "   $SECRET_FILE"
    echo ""
    read -p "Do you want to regenerate it? (yes/no): " -r
    echo ""
    if [[ ! $REPLY =~ ^[Yy]es$ ]]; then
        echo "Keeping existing secret."
        REGENERATE=false
    else
        REGENERATE=true
    fi
else
    REGENERATE=true
fi

# Generate new secret
if [ "$REGENERATE" = true ]; then
    echo "🔐 Generating new HMAC signing secret..."
    openssl rand -hex 32 > "$SECRET_FILE"
    chmod 600 "$SECRET_FILE"
    echo "✅ Secret generated and saved to:"
    echo "   $SECRET_FILE"
    echo ""
fi

# Display secret
echo "📋 Your signing secret (copy this for GitHub Actions):"
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
cat "$SECRET_FILE"
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
echo ""
echo "⚠️  IMPORTANT: Keep this secret secure!"
echo "   - Add it to GitHub Actions secrets as KOTORMODSYNC_SIGNING_SECRET"
echo "   - Never commit it to git"
echo "   - Rotate it if compromised"
echo ""

# Build auth service
echo "🏗️  Building kotormodsync-auth service..."
cd "$PROJECT_ROOT"

if [ ! -f "projects/kotormodsync/telemetry-auth/Dockerfile" ]; then
    echo "❌ Error: Auth service Dockerfile not found"
    echo "   Expected: projects/kotormodsync/telemetry-auth/Dockerfile"
    exit 1
fi

docker compose build kotormodsync-auth
echo "✅ Auth service built"
echo ""

# Start services
echo "🚀 Starting services..."
docker compose up -d kotormodsync-auth otel-collector prometheus

echo ""
echo "⏳ Waiting for services to be healthy (30 seconds)..."
sleep 30

# Check status
echo ""
echo "📊 Service Status:"
docker compose ps | grep -E "kotormodsync-auth|otel-collector|prometheus" || true

echo ""
echo "======================================================================"
echo "✅ Setup Complete!"
echo "======================================================================"
echo ""
echo "Next Steps:"
echo ""
echo "1. Copy the signing secret above"
echo ""
echo "2. Add to GitHub Actions:"
echo "   - Go to: https://github.com/YOUR_ORG/ModSync/settings/secrets/actions"
echo "   - New repository secret"
echo "   - Name: KOTORMODSYNC_SIGNING_SECRET"
echo "   - Value: (paste secret)"
echo ""
echo "3. Test the endpoint:"
echo "   curl -X POST https://otlp.bolabaden.org/v1/metrics \\"
echo "     -H \"Content-Type: application/json\" \\"
echo "     -d '{\"resourceMetrics\":[]}'"
echo "   Expected: HTTP 401 (unauthorized without signature)"
echo ""
echo "4. View auth service logs:"
echo "   docker compose logs -f kotormodsync-auth"
echo ""
echo "5. Integration guide:"
echo "   cat docs/KOTORMODSYNC_CLIENT_INTEGRATION.md"
echo ""
echo "======================================================================"

