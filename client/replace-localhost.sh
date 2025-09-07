#!/bin/sh

# Script to replace localhost with environment variable in generated client.ts
# Usage: ./replace-localhost.sh

CLIENT_FILE="src/generated/api/client.ts"
API_BASE_URL="${SERVER_URL:-http://localhost}"

if [ ! -f "$CLIENT_FILE" ]; then
    echo "Error: $CLIENT_FILE not found!"
    exit 1
fi

echo "Replacing localhost with $API_BASE_URL in $CLIENT_FILE..."

# Use sed to replace all instances of http://localhost with the environment variable
sed -i "s|http://localhost|$API_BASE_URL|g" "$CLIENT_FILE"

echo "Replacement complete!"
echo "All localhost instances have been replaced with: $API_BASE_URL"