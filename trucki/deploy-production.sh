#!/bin/bash
set -e

echo "🚀 Starting production deployment..."

# Load environment variables
source .env.production

# Stop and remove existing containers
docker-compose -f docker-compose.yml -f docker-compose.production.yml down --remove-orphans

# Remove old images
docker image prune -f

# Build and start new containers
docker-compose -f docker-compose.yml -f docker-compose.production.yml up -d --build

# Wait for container to be healthy
echo "⏳ Waiting for container to be healthy..."
timeout 60s bash -c 'until docker exec trucki-api-production curl -f http://localhost:80/health; do sleep 2; done'

echo "✅ Production deployment completed successfully!"
echo "🌐 Application is running at: https://api.trucki.co"