# GameForum Seed Scripts

These scripts generate SQL seed files for a local PostgreSQL database.

## Requirements

- Python 3.11+
- PostgreSQL `psql` client for `seed/run_seed.sh`
- RAWG API key in `RAWG_API_KEY`

Install Python dependencies:

```bash
python -m pip install -r scripts/scraping/requirements.txt
```

PowerShell equivalent:

```powershell
python -m pip install -r .\scripts\scraping\requirements.txt
```

## Generate Games

```bash
export RAWG_API_KEY="your_rawg_key"
python scripts/scraping/fetch_rawg_games.py
```

PowerShell:

```powershell
$env:RAWG_API_KEY = "your_rawg_key"
python .\scripts\scraping\fetch_rawg_games.py
```

This writes `scripts/seed/games.sql` with 200 RAWG games ordered by Metacritic,
plus genres, platforms, and many-to-many join rows.

Optional arguments:

```bash
python scripts/scraping/fetch_rawg_games.py --limit 200 --page-size 40 --delay 0.25
```

## Generate Forum Data

```bash
python scripts/scraping/generate_forum_seed.py
```

PowerShell:

```powershell
python .\scripts\scraping\generate_forum_seed.py
```

This reads `scripts/seed/games.sql` when present and writes `scripts/seed/forum.sql`
with fake users, 5 categories, threads, posts, and votes. Fake users are included
because forum records require `UserId` foreign keys.

## Apply Seed

The database schema must already exist from EF Core migrations before applying
these files.

```bash
export PGHOST=localhost
export PGPORT=5432
export PGDATABASE=gameforum
export PGUSER=gameforum
export PGPASSWORD=gameforum

bash scripts/seed/run_seed.sh
```

The runner applies:

1. `scripts/seed/schema.sql`, only if present
2. `scripts/seed/games.sql`
3. `scripts/seed/forum.sql`

Generated `*.sql` files under `scripts/seed/` are ignored by git.
