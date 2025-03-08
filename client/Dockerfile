FROM node:20-alpine

WORKDIR /app

# Copy only essential files first
COPY package.json .
COPY yarn.lock .

# Ensure a clean install (force Yarn to not use the cache)
RUN yarn install

# Copy the rest of the application
COPY . .

CMD [ "yarn", "run", "build" ]

