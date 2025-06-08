FROM node:20-alpine

WORKDIR /app

# ── 1. Copy Yarn config + lock files first (cacheable layer)
COPY .yarnrc.yml ./
COPY .yarn ./.yarn
COPY package.json yarn.lock ./

# ── 2. Do the deterministic install once
RUN yarn install --immutable --inline-builds

# ── 3. Now add the rest of the source code
COPY . .

# ── 4. Entrypoint
COPY entrypoint.sh .
CMD ["sh", "./entrypoint.sh"]
