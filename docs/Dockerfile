FROM node:22-alpine AS build
WORKDIR /app
COPY package.json package-lock.json ./
RUN npm ci
COPY . .
RUN npx likec4 codegen webcomponent --no-use-dot -o ./public/likec4-views.js ./architecture/likec4
RUN npx vitepress build

FROM nginx:alpine
COPY --from=build /app/.vitepress/dist /usr/share/nginx/html
EXPOSE 80
