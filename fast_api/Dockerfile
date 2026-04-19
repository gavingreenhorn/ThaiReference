FROM python:3.12-slim

ENV PYTHONDONTWRITEBYTECODE=1 \
    PYTHONUNBUFFERED=1 \
    PIP_NO_CACHE_DIR=1

WORKDIR /app

RUN apt-get update && apt-get install -y --no-install-recommends \
    build-essential \
    curl \
 && rm -rf /var/lib/apt/lists/*

COPY requirements.txt .
RUN pip install --upgrade pip && pip install -r requirements.txt

COPY app ./app
COPY migrations ./migrations
COPY alembic.ini .

EXPOSE 5000

HEALTHCHECK --interval=10s --timeout=5s --start-period=40s --retries=3 \
  CMD curl -fsS http://127.0.0.1:5000/health >/dev/null || exit 1

CMD ["sh", "-c", "alembic revision --autogenerate && alembic upgrade head && uvicorn app.main:app --host 0.0.0.0 --port 5000"]

