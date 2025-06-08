FROM node:20-alpine

WORKDIR /app

COPY .yarnrc.yml ./
COPY .yarn ./.yarn
COPY package.json yarn.lock ./

RUN yarn install --immutable --inline-builds

COPY . .

COPY entrypoint.sh .
CMD ["sh", "./entrypoint.sh"]
