"""Tortoise model for fastapi-admin dashboard login."""
from fastapi_admin.models import AbstractAdmin
from tortoise import fields


class Admin(AbstractAdmin):
    """Admin user for /admin dashboard (username/password via UsernamePasswordProvider)."""
    email = fields.CharField(max_length=200, default="")
    created_at = fields.DatetimeField(auto_now_add=True)

    def __str__(self) -> str:
        return f"{self.pk}#{self.username}"
