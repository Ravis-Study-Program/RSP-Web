#!/bin/sh

if [ "$NODE_ENV" = "production" ]; then
  echo "Running production build..."
  yarn run build
else
  echo "Running development server..."
  yarn run dev
fi
