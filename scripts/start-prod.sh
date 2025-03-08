#!/usr/bin/env bash

docker-compose down
docker-compose up -d --build

echo "(Prod) Services started using docker-compose.yml only."
