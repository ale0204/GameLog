#!/usr/bin/env python3
"""
Fetch top RAWG games and generate SQL seed data.

Output:
  scripts/seed/games.sql
"""

from __future__ import annotations

import argparse
import os
import sys
import time
import uuid
from datetime import datetime, timezone
from pathlib import Path
from typing import Any

import requests
from requests import HTTPError


RAWG_BASE_URL = "https://api.rawg.io/api"
DEFAULT_LIMIT = 200
DEFAULT_PAGE_SIZE = 40

SCRIPT_DIR = Path(__file__).resolve().parent
SCRIPTS_DIR = SCRIPT_DIR.parent
SEED_DIR = SCRIPTS_DIR / "seed"
OUTPUT_FILE = SEED_DIR / "games.sql"


def sql_string(value: Any) -> str:
    if value is None:
        return "NULL"
    return "'" + str(value).replace("'", "''") + "'"


def sql_uuid(value: uuid.UUID | str) -> str:
    return f"'{value}'"


def sql_int(value: Any) -> str:
    if value is None:
        return "NULL"
    return str(int(value))


def sql_float(value: Any) -> str:
    if value is None:
        return "0"
    return str(float(value))


def release_year(released: str | None) -> int | None:
    if not released:
        return None
    try:
        return int(released[:4])
    except ValueError:
        return None


def normalize_name(name: str | None) -> str | None:
    if not name:
        return None
    normalized = " ".join(name.split())
    return normalized or None


def fetch_games(api_key: str, limit: int, page_size: int, delay: float) -> list[dict[str, Any]]:
    session = requests.Session()
    games: list[dict[str, Any]] = []
    page = 1

    while len(games) < limit:
        response = session.get(
            f"{RAWG_BASE_URL}/games",
            params={
                "key": api_key,
                "ordering": "-metacritic",
                "page": page,
                "page_size": min(page_size, limit - len(games)),
            },
            timeout=30,
        )
        try:
            response.raise_for_status()
        except HTTPError as ex:
            if response.status_code in (401, 403):
                raise RuntimeError(
                    "RAWG rejected the API key. Set RAWG_API_KEY to a real key from https://rawg.io/apidocs."
                ) from ex
            raise
        payload = response.json()
        results = payload.get("results", [])
        if not results:
            break

        games.extend(results)
        print(f"Fetched page {page}: {len(games)}/{limit} games", file=sys.stderr)

        if not payload.get("next"):
            break

        page += 1
        if delay > 0:
            time.sleep(delay)

    return games[:limit]


def build_sql(games: list[dict[str, Any]]) -> str:
    now = datetime.now(timezone.utc).isoformat()
    genre_ids: dict[str, uuid.UUID] = {}
    platform_ids: dict[str, uuid.UUID] = {}
    game_rows: list[str] = []
    genre_rows: list[str] = []
    platform_rows: list[str] = []
    game_genre_rows: list[str] = []
    game_platform_rows: list[str] = []

    for raw_game in games:
        rawg_id = raw_game.get("id")
        if rawg_id is None:
            continue

        game_id = uuid.uuid4()
        title = normalize_name(raw_game.get("name")) or f"RAWG Game {rawg_id}"
        average_rating = raw_game.get("rating") or 0
        total_players = raw_game.get("added") or 0

        game_rows.append(
            "("
            f"{sql_uuid(game_id)}, "
            f"{sql_string(now)}, "
            f"{sql_string(now)}, "
            f"{sql_int(rawg_id)}, "
            f"{sql_string(title)}, "
            f"{sql_string(raw_game.get('background_image'))}, "
            "NULL, "
            f"{sql_int(release_year(raw_game.get('released')))}, "
            "NULL, "
            f"{sql_float(average_rating)}, "
            f"{sql_int(total_players)}"
            ")"
        )

        for genre in raw_game.get("genres", []):
            name = normalize_name(genre.get("name"))
            if not name:
                continue
            if name not in genre_ids:
                genre_ids[name] = uuid.uuid4()
                genre_rows.append(
                    "("
                    f"{sql_uuid(genre_ids[name])}, "
                    f"{sql_string(now)}, "
                    f"{sql_string(now)}, "
                    f"{sql_string(name)}"
                    ")"
                )
            game_genre_rows.append(f"({sql_uuid(game_id)}, {sql_uuid(genre_ids[name])})")

        for platform_wrapper in raw_game.get("platforms", []):
            platform = platform_wrapper.get("platform") or {}
            name = normalize_name(platform.get("name"))
            if not name:
                continue
            if name not in platform_ids:
                platform_ids[name] = uuid.uuid4()
                platform_rows.append(
                    "("
                    f"{sql_uuid(platform_ids[name])}, "
                    f"{sql_string(now)}, "
                    f"{sql_string(now)}, "
                    f"{sql_string(name)}"
                    ")"
                )
            game_platform_rows.append(f"({sql_uuid(game_id)}, {sql_uuid(platform_ids[name])})")

    lines = [
        "-- Generated by scripts/scraping/fetch_rawg_games.py",
        "-- Table names follow the GameForum EF Core model conventions planned for Stage 4.",
        "BEGIN;",
        "",
    ]

    if game_rows:
        lines.extend(
            [
                'INSERT INTO "Games" ("Id", "CreatedAt", "UpdatedAt", "RawgId", "Title", "CoverUrl", "Description", "ReleaseYear", "Developer", "AverageRating", "TotalPlayers") VALUES',
                ",\n".join(game_rows) + ";",
                "",
            ]
        )

    if genre_rows:
        lines.extend(
            [
                'INSERT INTO "Genres" ("Id", "CreatedAt", "UpdatedAt", "Name") VALUES',
                ",\n".join(genre_rows) + ";",
                "",
            ]
        )

    if platform_rows:
        lines.extend(
            [
                'INSERT INTO "Platforms" ("Id", "CreatedAt", "UpdatedAt", "Name") VALUES',
                ",\n".join(platform_rows) + ";",
                "",
            ]
        )

    if game_genre_rows:
        lines.extend(
            [
                'INSERT INTO "GameGenres" ("GameId", "GenreId") VALUES',
                ",\n".join(game_genre_rows) + ";",
                "",
            ]
        )

    if game_platform_rows:
        lines.extend(
            [
                'INSERT INTO "GamePlatforms" ("GameId", "PlatformId") VALUES',
                ",\n".join(game_platform_rows) + ";",
                "",
            ]
        )

    lines.append("COMMIT;")
    lines.append("")
    return "\n".join(lines)


def parse_args() -> argparse.Namespace:
    parser = argparse.ArgumentParser(description="Generate games.sql with top RAWG games.")
    parser.add_argument("--limit", type=int, default=DEFAULT_LIMIT, help="Number of games to fetch.")
    parser.add_argument("--page-size", type=int, default=DEFAULT_PAGE_SIZE, help="RAWG page size.")
    parser.add_argument("--delay", type=float, default=0.25, help="Delay in seconds between RAWG pages.")
    parser.add_argument("--output", type=Path, default=OUTPUT_FILE, help="Output SQL file.")
    return parser.parse_args()


def main() -> int:
    args = parse_args()
    api_key = os.getenv("RAWG_API_KEY")
    if not api_key:
        print("RAWG_API_KEY environment variable is required.", file=sys.stderr)
        return 1

    games = fetch_games(api_key, args.limit, args.page_size, args.delay)
    if not games:
        print("RAWG returned no games.", file=sys.stderr)
        return 1

    args.output.parent.mkdir(parents=True, exist_ok=True)
    args.output.write_text(build_sql(games), encoding="utf-8")
    print(f"Wrote {args.output} with {len(games)} games.")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
