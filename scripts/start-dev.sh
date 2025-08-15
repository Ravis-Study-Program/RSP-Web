#!/usr/bin/env bash

mkdir -p logs/{nginx,postgres,prometheus,grafana}

docker compose -f docker-compose.yml -f docker-compose.dev.yml --env-file .env.dev down

docker compose -f docker-compose.yml -f docker-compose.dev.yml --env-file .env.dev up -d --build
