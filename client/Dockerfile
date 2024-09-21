FROM node:18-alpine

USER $APP_UID
WORKDIR /usr/src/app

COPY package.json yarn.lock ./
RUN yarn

COPY . .

EXPOSE 3000

CMD ["yarn", "run", "dev"]
