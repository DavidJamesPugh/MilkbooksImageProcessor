# ── Stage 1: Build Angular ────────────────────────────────────────────────────
FROM node:22-alpine AS angular-build

WORKDIR /app/client

COPY Milkbooks.Client/package*.json ./
RUN npm ci

COPY Milkbooks.Client/ ./
RUN npm run build
# angular.json outputPath is "../wwwroot/app" relative to Milkbooks.Client/
# so the built files land at /app/wwwroot/app


# ── Stage 2: Build .NET ───────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS dotnet-build

WORKDIR /src

COPY *.csproj ./
RUN dotnet restore -r linux-x64

COPY . ./

# Bring in the Angular output before publishing so it ends up in wwwroot
COPY --from=angular-build /app/wwwroot/app ./wwwroot/app

RUN dotnet publish -c Release -r linux-x64 --self-contained false -o /publish


# ── Stage 3: Runtime ──────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime

# SkiaSharp's native library (libSkiaSharp.so) is dynamically linked against
# these system libraries. The dotnet/aspnet slim image omits them by default,
# causing SkiaSharp to fail to load silently — resulting in resize 404s.
RUN apt-get update && apt-get install -y --no-install-recommends \
    libfontconfig1 \
    libglib2.0-0 \
    libharfbuzz0b \
    && rm -rf /var/lib/apt/lists/*

WORKDIR /app

COPY --from=dotnet-build /publish ./

# Pre-create the image storage folders so the app can write to them immediately
RUN mkdir -p wwwroot/images/full \
             wwwroot/images/256 \
             wwwroot/images/1024

EXPOSE 8080

CMD ["dotnet", "MilkbooksImageProcessor.dll"]
