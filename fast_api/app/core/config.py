from pydantic_settings import BaseSettings, SettingsConfigDict


class Settings(BaseSettings):
    model_config = SettingsConfigDict(
        case_sensitive=True,
        env_file=".env",
        env_file_encoding="utf-8",
    )

    app_title: str = "Thai Reference"

    db_host: str | None = None
    db_port: str | None = "5432"
    db_name: str | None = None
    db_user: str | None = None
    db_password: str | None = None
    sqlite_url: str = "sqlite:///../ThaiReference/Resources/Raw/thai_sqlite.db"
    db_export_token: str | None = None
    cors_origins: str | None = None
    redis_url: str = "redis://redis:6379"

    @property
    def dsn(self) -> str:
        return (
            f"postgresql+psycopg2://"
            f"{self.db_user}:"
            f"{self.db_password}@"
            f"{self.db_host}:"
            f"{self.db_port}/"
            f"{self.db_name}"
        )

    @property
    def database_url(self) -> str:
        if all([self.db_host, self.db_name, self.db_user, self.db_password]):
            return self.dsn
        return self.sqlite_url

    @property
    def database_path(self) -> str | None:
        """Filesystem path for the SQLite database when using sqlite_url.

        Returns None when using Postgres or a non-sqlite URL.
        """
        url = self.database_url
        if not url.startswith("sqlite:///"):
            return None
        return url.removeprefix("sqlite:///")

    @property
    def cors_origins_list(self) -> list[str]:
        if not self.cors_origins:
            return []
        return [o.strip() for o in self.cors_origins.split(",") if o.strip()]


settings = Settings()
