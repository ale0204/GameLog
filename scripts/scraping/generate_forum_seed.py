#!/usr/bin/env python3
"""
Generate fake forum seed SQL from games.sql.

Output:
  scripts/seed/forum.sql
"""

from __future__ import annotations

import argparse
import random
import re
import uuid
from datetime import datetime, timedelta, timezone
from pathlib import Path

from faker import Faker


SCRIPT_DIR = Path(__file__).resolve().parent
SCRIPTS_DIR = SCRIPT_DIR.parent
SEED_DIR = SCRIPTS_DIR / "seed"
DEFAULT_GAMES_SQL = SEED_DIR / "games.sql"
OUTPUT_FILE = SEED_DIR / "forum.sql"

GAME_INSERT_RE = re.compile(
    r"\('(?P<id>[0-9a-fA-F-]{36})',\s*'[^']+',\s*'[^']+',\s*\d+,\s*'(?P<title>(?:''|[^'])*)'",
    re.MULTILINE,
)

CATEGORY_DATA = [
    ("Game Discussions", "General gameplay, mechanics, strategy, and lore discussions.", "chat", 1),
    ("Looking for Players", "Find squads, co-op partners, guilds, and competitive teams.", "users", 2),
    ("Reviews & Recommendations", "Share reviews and ask for what to play next.", "star", 3),
    ("Gaming News", "Announcements, updates, releases, and industry news.", "newspaper", 4),
    ("Off-topic", "Casual community discussion outside specific games.", "coffee", 5),
]

THREAD_TEMPLATES = {
    "Game Discussions": [
        "What is your favorite build in {game}?",
        "Best early-game tips for {game}",
        "What makes {game} still worth playing?",
        "Hardest mission or boss in {game}",
    ],
    "Looking for Players": [
        "Looking for players for {game} this weekend",
        "Need a co-op partner for {game}",
        "Casual group forming for {game}",
        "Anyone up for ranked matches in {game}?",
    ],
    "Reviews & Recommendations": [
        "Is {game} worth starting now?",
        "Recommend me something like {game}",
        "My short review after finishing {game}",
        "Games with the same vibe as {game}",
    ],
    "Gaming News": [
        "Latest update discussion for {game}",
        "Patch notes reaction: {game}",
        "What do you expect from the next {game} update?",
        "Release window rumors around {game}",
    ],
    "Off-topic": [
        "What are you playing this week?",
        "Best gaming soundtrack lately",
        "Controller or keyboard for long sessions?",
        "Backlog check-in thread",
    ],
}

POST_SENTENCES = [
    "I tried it recently and the pacing felt much better than I expected.",
    "The community around this game is still active enough for new players.",
    "My main issue is the onboarding, but after a few hours it starts to click.",
    "I would recommend checking a beginner guide before jumping into harder content.",
    "The art direction does a lot of work, even when the mechanics feel familiar.",
    "For co-op, scheduling matters more than skill level in my experience.",
    "The latest balance changes made a few older strategies viable again.",
    "It is not perfect, but the core loop is strong enough to keep me coming back.",
    "I had more fun after disabling a few HUD options and playing slower.",
    "The best moments come from experimenting instead of following the meta exactly.",
]


def sql_string(value: object) -> str:
    if value is None:
        return "NULL"
    return "'" + str(value).replace("'", "''") + "'"


def sql_uuid(value: uuid.UUID | str) -> str:
    return f"'{value}'"


def sql_bool(value: bool) -> str:
    return "TRUE" if value else "FALSE"


def sql_datetime(value: datetime) -> str:
    return sql_string(value.isoformat())


def read_games(path: Path) -> list[tuple[str, str]]:
    if not path.exists():
        return []

    content = path.read_text(encoding="utf-8")
    games: list[tuple[str, str]] = []
    for match in GAME_INSERT_RE.finditer(content):
        title = match.group("title").replace("''", "'")
        games.append((match.group("id"), title))
    return games


def pick_game(games: list[tuple[str, str]]) -> tuple[str | None, str]:
    if not games or random.random() < 0.2:
        return None, "games"
    game_id, title = random.choice(games)
    return game_id, title


def build_post_content(fake: Faker) -> str:
    sentences = random.sample(POST_SENTENCES, k=random.randint(2, 4))
    if random.random() < 0.35:
        sentences.append(fake.sentence(nb_words=random.randint(8, 14)))
    return " ".join(sentences)


def build_sql(games: list[tuple[str, str]], seed: int) -> str:
    random.seed(seed)
    fake = Faker("en_US")
    Faker.seed(seed)

    now = datetime.now(timezone.utc)
    users: list[dict[str, object]] = []
    categories: list[dict[str, object]] = []
    threads: list[dict[str, object]] = []
    posts: list[dict[str, object]] = []
    votes: list[dict[str, object]] = []

    for index in range(20):
        username = fake.unique.user_name().lower()[:40]
        users.append(
            {
                "id": uuid.uuid4(),
                "created_at": now - timedelta(days=random.randint(7, 120)),
                "updated_at": now,
                "username": username,
                "display_name": fake.name(),
                "avatar_url": f"https://api.dicebear.com/8.x/pixel-art/svg?seed={username}",
                "bio": fake.sentence(nb_words=random.randint(8, 16)),
                "last_seen_at": now - timedelta(hours=random.randint(1, 240)),
                "is_profile_public": random.random() > 0.1,
                "default_game_visibility": random.choice([0, 0, 0, 1, 2]),
            }
        )

    for name, description, icon_url, sort_order in CATEGORY_DATA:
        categories.append(
            {
                "id": uuid.uuid4(),
                "created_at": now,
                "updated_at": now,
                "name": name,
                "description": description,
                "icon_url": icon_url,
                "sort_order": sort_order,
            }
        )

    for category in categories:
        thread_count = random.randint(3, 5)
        templates = THREAD_TEMPLATES[str(category["name"])]
        for _ in range(thread_count):
            game_id, game_title = pick_game(games)
            author = random.choice(users)
            created_at = now - timedelta(days=random.randint(1, 45), hours=random.randint(0, 23))
            title = random.choice(templates).format(game=game_title)
            thread = {
                "id": uuid.uuid4(),
                "created_at": created_at,
                "updated_at": created_at,
                "title": title,
                "user_id": author["id"],
                "category_id": category["id"],
                "game_id": game_id,
                "is_pinned": random.random() < 0.08,
                "is_locked": random.random() < 0.05,
                "trending_score": 0,
                "last_activity_at": created_at,
            }
            threads.append(thread)

            for post_index in range(random.randint(2, 4)):
                post_author = author if post_index == 0 else random.choice(users)
                post_created_at = created_at + timedelta(hours=random.randint(post_index, post_index + 48))
                post = {
                    "id": uuid.uuid4(),
                    "created_at": post_created_at,
                    "updated_at": post_created_at,
                    "thread_id": thread["id"],
                    "user_id": post_author["id"],
                    "content": build_post_content(fake),
                    "trending_score": 0,
                    "is_deleted": False,
                }
                posts.append(post)
                thread["last_activity_at"] = max(thread["last_activity_at"], post_created_at)

                voters = random.sample(users, k=random.randint(0, min(8, len(users))))
                for voter in voters:
                    if voter["id"] == post_author["id"] and random.random() < 0.8:
                        continue
                    votes.append(
                        {
                            "id": uuid.uuid4(),
                            "created_at": post_created_at + timedelta(minutes=random.randint(5, 500)),
                            "updated_at": post_created_at + timedelta(minutes=random.randint(5, 500)),
                            "post_id": post["id"],
                            "user_id": voter["id"],
                            "value": 1 if random.random() < 0.82 else -1,
                        }
                    )

    lines = [
        "-- Generated by scripts/scraping/generate_forum_seed.py",
        "-- Includes fake users because forum entities require UserId foreign keys.",
        "BEGIN;",
        "",
    ]

    user_rows = [
        "("
        f"{sql_uuid(user['id'])}, "
        f"{sql_datetime(user['created_at'])}, "
        f"{sql_datetime(user['updated_at'])}, "
        f"{sql_string(user['username'])}, "
        f"{sql_string(user['display_name'])}, "
        f"{sql_string(user['avatar_url'])}, "
        f"{sql_string(user['bio'])}, "
        f"{sql_datetime(user['last_seen_at'])}, "
        f"{sql_bool(bool(user['is_profile_public']))}, "
        f"{user['default_game_visibility']}, "
        "NULL, "
        "NULL"
        ")"
        for user in users
    ]
    lines.extend(
        [
            'INSERT INTO "Users" ("Id", "CreatedAt", "UpdatedAt", "Username", "DisplayName", "AvatarUrl", "Bio", "LastSeenAt", "IsProfilePublic", "DefaultGameVisibility", "CurrentlyPlayingGameId", "FavoriteGameId") VALUES',
            ",\n".join(user_rows) + ";",
            "",
        ]
    )

    category_rows = [
        "("
        f"{sql_uuid(category['id'])}, "
        f"{sql_datetime(category['created_at'])}, "
        f"{sql_datetime(category['updated_at'])}, "
        f"{sql_string(category['name'])}, "
        f"{sql_string(category['description'])}, "
        f"{sql_string(category['icon_url'])}, "
        f"{category['sort_order']}"
        ")"
        for category in categories
    ]
    lines.extend(
        [
            'INSERT INTO "Categories" ("Id", "CreatedAt", "UpdatedAt", "Name", "Description", "IconUrl", "SortOrder") VALUES',
            ",\n".join(category_rows) + ";",
            "",
        ]
    )

    thread_rows = [
        "("
        f"{sql_uuid(thread['id'])}, "
        f"{sql_datetime(thread['created_at'])}, "
        f"{sql_datetime(thread['updated_at'])}, "
        f"{sql_string(thread['title'])}, "
        f"{sql_uuid(thread['user_id'])}, "
        f"{sql_uuid(thread['category_id'])}, "
        f"{sql_uuid(thread['game_id']) if thread['game_id'] else 'NULL'}, "
        f"{sql_bool(bool(thread['is_pinned']))}, "
        f"{sql_bool(bool(thread['is_locked']))}, "
        f"{thread['trending_score']}, "
        f"{sql_datetime(thread['last_activity_at'])}"
        ")"
        for thread in threads
    ]
    lines.extend(
        [
            'INSERT INTO "Threads" ("Id", "CreatedAt", "UpdatedAt", "Title", "UserId", "CategoryId", "GameId", "IsPinned", "IsLocked", "TrendingScore", "LastActivityAt") VALUES',
            ",\n".join(thread_rows) + ";",
            "",
        ]
    )

    post_rows = [
        "("
        f"{sql_uuid(post['id'])}, "
        f"{sql_datetime(post['created_at'])}, "
        f"{sql_datetime(post['updated_at'])}, "
        f"{sql_uuid(post['thread_id'])}, "
        f"{sql_uuid(post['user_id'])}, "
        f"{sql_string(post['content'])}, "
        f"{post['trending_score']}, "
        f"{sql_bool(bool(post['is_deleted']))}"
        ")"
        for post in posts
    ]
    lines.extend(
        [
            'INSERT INTO "Posts" ("Id", "CreatedAt", "UpdatedAt", "ThreadId", "UserId", "Content", "TrendingScore", "IsDeleted") VALUES',
            ",\n".join(post_rows) + ";",
            "",
        ]
    )

    if votes:
        vote_rows = [
            "("
            f"{sql_uuid(vote['id'])}, "
            f"{sql_datetime(vote['created_at'])}, "
            f"{sql_datetime(vote['updated_at'])}, "
            f"{sql_uuid(vote['post_id'])}, "
            f"{sql_uuid(vote['user_id'])}, "
            f"{vote['value']}"
            ")"
            for vote in votes
        ]
        lines.extend(
            [
                'INSERT INTO "Votes" ("Id", "CreatedAt", "UpdatedAt", "PostId", "UserId", "Value") VALUES',
                ",\n".join(vote_rows) + ";",
                "",
            ]
        )

    lines.append("COMMIT;")
    lines.append("")
    return "\n".join(lines)


def parse_args() -> argparse.Namespace:
    parser = argparse.ArgumentParser(description="Generate forum.sql with fake forum data.")
    parser.add_argument("--games-sql", type=Path, default=DEFAULT_GAMES_SQL, help="Path to games.sql.")
    parser.add_argument("--output", type=Path, default=OUTPUT_FILE, help="Output SQL file.")
    parser.add_argument("--seed", type=int, default=42, help="Random seed for deterministic output.")
    return parser.parse_args()


def main() -> int:
    args = parse_args()
    games = read_games(args.games_sql)
    if not games:
        print(f"No games found in {args.games_sql}. Forum seed will still be generated without game links.")

    args.output.parent.mkdir(parents=True, exist_ok=True)
    args.output.write_text(build_sql(games, args.seed), encoding="utf-8")
    print(f"Wrote {args.output}.")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
