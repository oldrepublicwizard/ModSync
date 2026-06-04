#!/bin/bash
#
# Generate a cryptographically secure signing secret for ModSync telemetry
#

set -e

OUTPUT_FILE="${1:-signing_secret.txt}"

echo "======================================================================"
echo "ModSync Signing Secret Generator"
echo "======================================================================"
echo ""

# Check if file already exists
if [ -f "$OUTPUT_FILE" ]; then
    echo "⚠️  File already exists: $OUTPUT_FILE"
    read -p "Overwrite? (yes/no): " -r
    echo ""
    if [[ ! $REPLY =~ ^[Yy]es$ ]]; then
        echo "Aborted. Keeping existing file."
        exit 0
    fi
fi

# Generate secret
echo "🔐 Generating cryptographically secure secret..."
openssl rand -hex 32 > "$OUTPUT_FILE"

# Secure the file
chmod 600 "$OUTPUT_FILE"

# Display result
echo "✅ Secret generated successfully!"
echo ""
echo "📁 Saved to: $OUTPUT_FILE"
echo "🔒 Permissions: 600 (owner read/write only)"
echo ""
echo "📋 Your signing secret:"
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
cat "$OUTPUT_FILE"
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
echo ""
echo "⚠️  IMPORTANT: "
echo "   - Keep this secret secure!"
echo "   - Never commit it to git (it's in .gitignore)"
echo "   - Add it to GitHub Actions secrets as KOTORMODSYNC_SIGNING_SECRET"
echo "   - Store a backup in a secure location"
echo ""
echo "📝 Next steps:"
echo "   1. Deploy the service:"
echo "      docker compose up -d"
echo ""
echo "   2. Test the service:"
echo "      ./scripts/test-auth.sh valid"
echo ""
echo "   3. Add to ModSync GitHub secrets:"
echo "      https://github.com/YOUR_ORG/ModSync/settings/secrets/actions"
echo ""
echo "======================================================================"

