#!/usr/bin/env bash

docker compose -f docker-compose.yml -f docker-compose.prod.yml --env-file .env.prod down
docker compose -f docker-compose.yml -f docker-compose.prod.yml --env-file .env.prod up -d --build

echo "(Prod) Services started using docker-compose.yml only."
