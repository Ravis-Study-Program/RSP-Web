# Build stage
FROM node:20-alpine AS build
WORKDIR /app

COPY .yarnrc.yml ./
COPY .yarn ./.yarn
COPY package.json yarn.lock ./

RUN yarn install --immutable --inline-builds

COPY . .

ARG BUILD_CONFIGURATION=Release
ARG BUILD_ENVIRONMENT=Production
ARG SERVER_URL=http://localhost

# Replace with SERVER_URL
RUN sed -i "s|http://localhost:4000|${SERVER_URL}|g" src/generated/api/client.ts

RUN yarn run build

# Production stage
FROM nginx:1.25-alpine AS production
WORKDIR /usr/share/nginx/html

RUN apk add --no-cache curl

COPY --from=build /app/dist .

COPY nginx.conf /etc/nginx/conf.d/default.conf

EXPOSE 8080

HEALTHCHECK --interval=30s --timeout=10s --start-period=30s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1

CMD ["nginx", "-g", "daemon off;"]

# Development stage
FROM node:20-alpine AS development
WORKDIR /app

RUN apk add --no-cache curl

COPY .yarnrc.yml ./
COPY .yarn ./.yarn
COPY package.json yarn.lock ./

RUN yarn install --immutable --inline-builds

COPY . .

# Copy entrypoint script
COPY entrypoint.sh .
RUN chmod +x entrypoint.sh

EXPOSE 8080

HEALTHCHECK --interval=10s --timeout=5s --start-period=30s --retries=10 \
    CMD curl -f http://localhost:8080/health || exit 1

ENTRYPOINT ["./entrypoint.sh"]
