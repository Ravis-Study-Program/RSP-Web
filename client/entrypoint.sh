#!/bin/sh

if [ "$NODE_ENV" = "production" ]; then
  echo "Running production build..."
  exec yarn run build
else
  echo "Running development server..."
  exec yarn run dev
fi
