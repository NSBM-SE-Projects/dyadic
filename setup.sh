#!/usr/bin/env bash
# Author: Dwain
#
# Dyadic - One-shot Local Dev Setup.
# Run from project root: ./setup.sh
#
set -euo pipefail

# Helpers
info() { printf "\033[1;34m[info]\033[0m  %s\n" "$*"; }
ok()  { printf "\033[1;32m[ok]\033[0m    %s\n" "$*"; }
warn()  { printf "\033[1;33m[warn]\033[0m  %s\n" "$*"; }
fail()  { printf "\033[1;31m[fail]\033[0m  %s\n" "$*" >&2; exit 1; }

need() {
    command -v "$1" >/dev/null 2>&1 || fail "$1 NOT FOUND. INSTALL IT FIRST: $2"
}

# Checking Prerequisites
info "CHECKING PRE-REQUISITES..."
need dotnet "https://dotnet.microsoft.com/download/dotnet/8.0"
need docker "https://www.docker.com/products/docker-desktop/"
need git "https://git-scm.com/downloads"

DOTNET_VERSION=$(dotnet --version)
[[ "$DOTNET_VERSION" == 8.* ]] || fail ".NET 8 SDK REQUIRED (FOUND $DOTNET_VERSION)"
ok ".NET ${DOTNET_VERSION}"

docker info >/dev/null 2>&1 || fail "DOCKER DAEMON NOT RUNNING. START DOCKER DESKTOP FIRST."
ok "DOCKER DAEMON IS RUNNING"

# Creating .env (if missing)
if [[ ! -f .env ]]; then
    info "CREATING .env FROM .env.example..."
    cp .env.example .env
    warn "USING DEFAULT PASSWORD FROM .env.example. EDIT .env IF YOU WANT MORE SECURITY (>=8, upper+lower+digit+symbol)."
    ok ".env CREATED!"
fi
ok ".env EXISTS"

# Extract Password (stripping CR in case of CRLF line endings)
SA_PASSWORD=$(grep '^MSSQL_SA_PASSWORD=' .env | cut -d= -f2- | tr -d '\r')
[[ -n "$SA_PASSWORD" ]] || fail "MSSQL_SA_PASSWORD IS EMPTY IN .env"
# Validating SQL Server Password Complexity
validate_password() {
    local pw=$1

    # Quotes In .env
    if [[ "$pw" =~ ^[\"\'].*[\"\']$ ]]; then
        fail "REMOVE SORROUNDING QUOTES FROM MSSQL_SA_PASSWORD IN .env"
    fi

    # Whitespace - Warning
    if [[ "$pw" =~ ^[[:space:]] ]] || [[ "$pw" =~ [[:space:]]$ ]]; then
        warn "PASSWORD HAS WHITESPACE - PLEASE REMOVE FROM .env"
    fi

    
    # Length (More Than 8 Characters)
    [[ ${#pw} -ge 8 ]] || { fail "MSSQL_SA_PASSWORD MUST BE AT LEAST 8 CHARACTERS (got ${#pw})"; }

    # Password Complexity Check
    local missing=()
    [[ "$pw" =~ [A-Z] ]]            || missing+=("AN UPPERCASE LETTER(A-Z)")
    [[ "$pw" =~ [a-z] ]]            || missing+=("A LOWERCASE LETTER (a-z)")
    [[ "$pw" =~ [0-9] ]]            || missing+=("A DIGIT (0-9)")
    [[ "$pw" =~ [^a-zA-Z0-9] ]]     || missing+=("A SYMBOL (e.g. @, !, #)")

    if [[ ${#missing[@]} -gt 0 ]]; then
        fail "PASSWORD IS MISSING: $(IFS=', ';echo "${missing[*]}")"
    fi
    
    # SQL Server Rejects Passwords Containing The Login Name
    [[ "$pw" != *sa* ]] && [[ "$pw" != *SA* ]] || warn "PASSWORD CONTAINS 'sa' - SQL SERVER COULD REJECT IT"
}

validate_password "$SA_PASSWORD"
ok "PASSWORD MEETS COMPLEXITY REQUIREMENTS"

# Starting SQL Server Container
info "STARTING SQL SERVER CONTAINER..."
docker compose up -d

info "WAITING FOR SQL SERVER TO ACCEPT CONNECTIONS..."
for i in {1..30}; do
    if docker exec dyadic-sql //opt/mssql-tools18/bin/sqlcmd \
            -S localhost -U sa -P "$SA_PASSWORD" -C -Q "SELECT 1" >/dev/null 2>&1; then
        ok "SQL SERVER READY"
        break
    fi
    [[ $i -eq 30 ]] && fail "SQL SERVER NOT READY AFTER 60s. CHECK: docker compose logs db"
    sleep 2
done

# Configure User Secrets
info "CONFIGURING USER SECRETS..."
CONN_STR="Server=localhost,1433;Database=Dyadic;User Id=sa;Password=${SA_PASSWORD};TrustServerCertificate=True;"
(
    cd src/Dyadic.Web
    dotnet user-secrets set "ConnectionStrings:DefaultConnection" "$CONN_STR" >/dev/null
)
ok "USER SECRETS SET FOR Dyadic.Web"

# Restore Local Dotnet Tools
info "RESTORING DOTNET TOOLS (dotnet-ef etc.)..."
dotnet tool restore >/dev/null
ok "TOOLS RESTORED"

# Apply Migrations
info "APPLYING EF CORE MIGRATIONS..."
dotnet ef database update -p src/Dyadic.Infrastructure -s src/Dyadic.Web
ok "DATABASE MIGRATED"

# Build
info "BUILDING SOLUTION..."
dotnet build --nologo -v minimal
ok "BUILD SUCCEEDED!"

printf "\n\033[1;32mSETUP COMPLETE.\033[0m  RUN THE APP WITH:\n  dotnet run --project src/Dyadic.Web\n\n"
