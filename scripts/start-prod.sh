#!/usr/bin/env bash

mkdir -p logs/{nginx,postgres,prometheus,grafana}

docker compose -f docker-compose.yml -f docker-compose.prod.yml --env-file .env.prod down

docker compose -f docker-compose.yml -f docker-compose.prod.yml --env-file .env.prod up -d

