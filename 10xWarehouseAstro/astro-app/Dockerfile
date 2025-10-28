FROM node:20-alpine AS base
WORKDIR /app

FROM base AS deps
COPY 10xWarehouseAstro/astro-app/package*.json ./
RUN npm ci --no-audit --no-fund

FROM deps AS build
COPY 10xWarehouseAstro/astro-app/ .
ARG API_BASE_URL
ARG SUPABASE_URL
ARG SUPABASE_ANON_KEY
RUN echo "PUBLIC_API_BASE_URL=${API_BASE_URL}" > .env && \
    echo "PUBLIC_SUPABASE_URL=${SUPABASE_URL}" >> .env && \
    echo "PUBLIC_SUPABASE_ANON_KEY=${SUPABASE_ANON_KEY}" >> .env && \
    chmod +x node_modules/.bin/* && npm run build

FROM base AS runner
ARG SUPABASE_URL
ARG SUPABASE_ANON_KEY
ARG API_BASE_URL
ENV NODE_ENV=production
WORKDIR /app
COPY --from=deps /app/node_modules ./node_modules
COPY --from=build /app/dist ./dist
COPY --from=build /app/.env ./.env
COPY 10xWarehouseAstro/astro-app/package*.json ./
EXPOSE 4321
ENV HOST=0.0.0.0
ENV PORT=4321
CMD ["node", "./dist/server/entry.mjs"]


