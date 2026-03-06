# Seed data for PostgreSQL

To seed the production database from SQLite, upload the file and run the seed.

## Option A: GitHub Actions (recommended)

1. **Upload the SQLite file** (from your machine, replace USER and HOST):
   ```bash
   scp ThaiReference/Resources/Raw/thai_sqlite.db USER@HOST:~/infra/seed/thai_sqlite.db
   ```

2. **Run the seed workflow**: GitHub → Actions → "Seed database from SQLite" → Run workflow

## Option B: Manual on server

```bash
scp ThaiReference/Resources/Raw/thai_sqlite.db USER@HOST:~/infra/seed/thai_sqlite.db
ssh USER@HOST
cd ~/infra
sudo docker compose run --rm -v $(pwd)/seed:/seed fast_api python scripts/seed_from_sqlite.py /seed/thai_sqlite.db
```

## Option C: Include in deploy

To run seed automatically on deploy, add the file to the repo (one-time):
```bash
cp ThaiReference/Resources/Raw/thai_sqlite.db infra/seed/
git add -f infra/seed/thai_sqlite.db
git commit -m "Add seed data"
git push
```
Deploy will copy it and run the seed. Consider removing it after the first successful seed to avoid a large binary in history.
