#!/usr/bin/env bash

docker-compose -f docker-compose.yml -f docker-compose.dev.yml down
docker-compose -f docker-compose.yml -f docker-compose.dev.yml up -d --build

echo "(Dev) Services started using docker-compose.yml AND docker-compose.dev.yml."
