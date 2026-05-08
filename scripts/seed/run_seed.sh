#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

: "${PGHOST:=localhost}"
: "${PGPORT:=5432}"
: "${PGDATABASE:=gameforum}"
: "${PGUSER:=gameforum}"

if ! command -v psql >/dev/null 2>&1; then
  echo "psql is required but was not found in PATH." >&2
  exit 1
fi

if [[ ! -f "$SCRIPT_DIR/games.sql" ]]; then
  echo "Missing $SCRIPT_DIR/games.sql. Run scripts/scraping/fetch_rawg_games.py first." >&2
  exit 1
fi

if [[ ! -f "$SCRIPT_DIR/forum.sql" ]]; then
  echo "Missing $SCRIPT_DIR/forum.sql. Run scripts/scraping/generate_forum_seed.py first." >&2
  exit 1
fi

if [[ -f "$SCRIPT_DIR/schema.sql" ]]; then
  psql \
    --host "$PGHOST" \
    --port "$PGPORT" \
    --username "$PGUSER" \
    --dbname "$PGDATABASE" \
    --set ON_ERROR_STOP=1 \
    --file "$SCRIPT_DIR/schema.sql"
fi

psql \
  --host "$PGHOST" \
  --port "$PGPORT" \
  --username "$PGUSER" \
  --dbname "$PGDATABASE" \
  --set ON_ERROR_STOP=1 \
  --file "$SCRIPT_DIR/games.sql"

psql \
  --host "$PGHOST" \
  --port "$PGPORT" \
  --username "$PGUSER" \
  --dbname "$PGDATABASE" \
  --set ON_ERROR_STOP=1 \
  --file "$SCRIPT_DIR/forum.sql"

echo "Seed SQL applied successfully."
