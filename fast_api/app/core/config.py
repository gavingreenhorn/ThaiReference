from pydantic_settings import BaseSettings, SettingsConfigDict


class Settings(BaseSettings):
    model_config = SettingsConfigDict(
        case_sensitive=False,
        env_file=".env",
        env_file_encoding="utf-8",
    )

    app_title: str = "Thai Reference"
    db_host: str = "localhost"
    db_port: str = "5432"
    db_name: str = "thai"
    db_user: str = ""
    db_password: str = ""
    cors_origins: str | None = None
    admin_username: str = "admin"
    admin_password: str = ""
    admin_secret_key: str = "change-me-in-production"

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
    def cors_origins_list(self) -> list[str]:
        if not self.cors_origins:
            return []
        return [o.strip() for o in self.cors_origins.split(",") if o.strip()]


settings = Settings()
