# Stage 1: Build Angular
FROM node:20-alpine AS angular-build
WORKDIR /frontend
COPY frontend/package*.json ./
RUN npm ci
COPY frontend/ ./
RUN npx ng build --configuration production

# Stage 2: Build .NET and merge Angular output into wwwroot
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS dotnet-build
WORKDIR /app
COPY backend/ ./
COPY --from=angular-build /frontend/dist/frontend/browser ./src/NaviFreight.Api/wwwroot/
RUN dotnet publish src/NaviFreight.Api/NaviFreight.Api.csproj \
    --configuration Release \
    --output /publish

# Stage 3: Lean runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=dotnet-build /publish ./
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "NaviFreight.Api.dll"]
