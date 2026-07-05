FROM node:22-alpine AS build
WORKDIR /app

ARG VITE_API_BASE_URL=/api
ARG VITE_KEYCLOAK_URL=http://localhost:8080
ARG VITE_KEYCLOAK_REALM=hrportal
ARG VITE_KEYCLOAK_CLIENT_ID=hrportal-web
ARG VITE_TENANCY_MODE=multi
ARG VITE_TENANT_ID=demo

ENV VITE_API_BASE_URL=$VITE_API_BASE_URL
ENV VITE_KEYCLOAK_URL=$VITE_KEYCLOAK_URL
ENV VITE_KEYCLOAK_REALM=$VITE_KEYCLOAK_REALM
ENV VITE_KEYCLOAK_CLIENT_ID=$VITE_KEYCLOAK_CLIENT_ID
ENV VITE_TENANCY_MODE=$VITE_TENANCY_MODE
ENV VITE_TENANT_ID=$VITE_TENANT_ID

COPY src/frontend/package.json src/frontend/package-lock.json* ./
RUN npm install

COPY src/frontend/ ./
RUN npm run build

FROM nginx:1.27-alpine AS runtime
COPY docker/frontend/nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/dist /usr/share/nginx/html

EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]
