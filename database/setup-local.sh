#!/usr/bin/env bash
# NaviFreight — local SQL Server setup
# Installs colima + docker CLI (lightweight, no Docker Desktop needed),
# starts a SQL Server 2022 container, and applies the full schema + seed.
#
# Usage:  bash database/setup-local.sh
# Run from the repo root.

set -euo pipefail

REPO_ROOT="$(cd "$(dirname "$0")/.." && pwd)"
SA_PASSWORD="YourStrong!Passw0rd"
DB_NAME="NaviFreight"
CONTAINER="navifreight-sql"

echo ""
echo "╔══════════════════════════════════════════════════════╗"
echo "║       NaviFreight — local database setup             ║"
echo "╚══════════════════════════════════════════════════════╝"
echo ""

# ── 1. Install dependencies ───────────────────────────────────────────────────

install_if_missing() {
    local pkg="$1"
    if ! command -v "$pkg" &>/dev/null; then
        echo "→ Installing $pkg via Homebrew..."
        brew install "$pkg"
    else
        echo "✓ $pkg already installed"
    fi
}

install_if_missing colima
install_if_missing docker
install_if_missing sqlcmd   # Microsoft sqlcmd (Go edition)

# ── 2. Start colima (lightweight container runtime) ───────────────────────────

if ! colima status 2>/dev/null | grep -q "Running"; then
    echo ""
    echo "→ Starting colima (container runtime)..."
    # 4 CPU, 4 GB RAM — SQL Server requires at least 2 GB
    colima start --cpu 4 --memory 4 --disk 20
else
    echo "✓ colima is already running"
fi

# ── 3. Start SQL Server container ─────────────────────────────────────────────

echo ""
if docker ps --format '{{.Names}}' | grep -q "^${CONTAINER}$"; then
    echo "✓ Container '${CONTAINER}' is already running"
elif docker ps -a --format '{{.Names}}' | grep -q "^${CONTAINER}$"; then
    echo "→ Restarting existing container '${CONTAINER}'..."
    docker start "${CONTAINER}"
else
    echo "→ Pulling SQL Server 2022 image (first run only, ~1.5 GB)..."
    docker pull mcr.microsoft.com/mssql/server:2022-latest

    echo "→ Starting SQL Server container..."
    docker run -d \
        --name "${CONTAINER}" \
        -e ACCEPT_EULA=Y \
        -e MSSQL_SA_PASSWORD="${SA_PASSWORD}" \
        -e MSSQL_PID=Developer \
        -p 1433:1433 \
        mcr.microsoft.com/mssql/server:2022-latest
fi

# ── 4. Wait for SQL Server to accept connections ──────────────────────────────

echo ""
echo "→ Waiting for SQL Server to be ready..."
MAX_RETRIES=30
COUNT=0
until sqlcmd -S "localhost,1433" -U sa -P "${SA_PASSWORD}" \
             -No -Q "SELECT 1" &>/dev/null; do
    COUNT=$((COUNT + 1))
    if [ "$COUNT" -ge "$MAX_RETRIES" ]; then
        echo ""
        echo "✗ SQL Server did not become ready after ${MAX_RETRIES} attempts."
        echo "  Check container logs: docker logs ${CONTAINER}"
        exit 1
    fi
    printf "  attempt %d/%d...\r" "$COUNT" "$MAX_RETRIES"
    sleep 3
done
echo "✓ SQL Server is ready                     "

# ── 5. Run master init script ─────────────────────────────────────────────────

echo ""
echo "→ Applying schema, seeds, and stored procedures..."
sqlcmd -S "localhost,1433" -U sa -P "${SA_PASSWORD}" -No \
       -i "${REPO_ROOT}/database/init.sql"

# ── 6. Apply stored procedures ────────────────────────────────────────────────

SP_DIR="${REPO_ROOT}/database/stored-procedures"
SP_COUNT=0
for f in "${SP_DIR}"/*.sql; do
    sqlcmd -S "localhost,1433" -U sa -P "${SA_PASSWORD}" \
           -d "${DB_NAME}" -No -i "$f"
    SP_COUNT=$((SP_COUNT + 1))
done
echo "✓ Applied ${SP_COUNT} stored procedures"

# ── 7. Done ───────────────────────────────────────────────────────────────────

echo ""
echo "╔══════════════════════════════════════════════════════╗"
echo "║  Database ready. Next: switch backend to SqlServer.  ║"
echo "╚══════════════════════════════════════════════════════╝"
echo ""
echo "  Connection string (already in appsettings.json):"
echo "  Server=localhost;Database=NaviFreight;User Id=sa;"
echo "  Password=YourStrong!Passw0rd;TrustServerCertificate=True;"
echo ""
echo "  The backend appsettings.Development.json has been"
echo "  updated to use Provider = SqlServer."
echo ""
echo "  Restart the backend to pick up the change:"
echo "  cd backend && dotnet run --project src/NaviFreight.Api"
echo ""
