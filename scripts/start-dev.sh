#!/usr/bin/env bash

docker compose -f docker-compose.yml -f docker-compose.dev.yml --env-file .env.dev down
docker compose -f docker-compose.yml -f docker-compose.dev.yml --env-file .env.dev up -d --build

echo "(Dev) Services started using docker-compose.yml AND docker-compose.dev.yml."
