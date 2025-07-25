#!/bin/bash
set -e

echo "🚀 Starting staging deployment..."

# Load environment variables
source .env.staging

# Stop and remove existing containers
docker-compose -f docker-compose.yml -f docker-compose.staging.yml down --remove-orphans

# Remove old images
docker image prune -f

# Build and start new containers
docker-compose -f docker-compose.yml -f docker-compose.staging.yml up -d --build

# Wait for container to be healthy
echo "⏳ Waiting for container to be healthy..."
timeout 60s bash -c 'until docker exec trucki-api-staging curl -f http://localhost:80/health; do sleep 2; done'

echo "✅ Staging deployment completed successfully!"
echo "🌐 Application is running at: http://staging-api.trucki.co"