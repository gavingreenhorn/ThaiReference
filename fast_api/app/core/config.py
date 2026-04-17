from pydantic_settings import BaseSettings, SettingsConfigDict


class Settings(BaseSettings):

    model_config = SettingsConfigDict(
        case_sensitive=False,
        env_file=".env",
        env_file_encoding="utf-8",
    )

    app_title: str = "Thai Reference"
    postgres_host: str = "localhost"
    postgres_port: str = "5432"
    postgres_db: str = "thai"
    postgres_user: str = ""
    postgres_password: str = ""
    cors_origins: str | None = None
    admin_username: str = "admin"
    admin_password: str = "admin"
    admin_secret_key: str = "change-me-in-production"

    @property
    def dsn(self) -> str:
        return (
            f"postgresql+psycopg2://"
            f"{self.postgres_user}:"
            f"{self.postgres_password}@"
            f"{self.postgres_host}:"
            f"{self.postgres_port}/"
            f"{self.postgres_db}"
        )

    @property
    def cors_origins_list(self) -> list[str]:
        if not self.cors_origins:
            return []
        return [o.strip() for o in self.cors_origins.split(",") if o.strip()]


settings = Settings()
