#!/bin/sh

if [ "$NODE_ENV" = "production" ]; then
  echo "Running production build..."
  nginx -g daemon off
else
  echo "Running development server..."
  yarn run dev
fi
