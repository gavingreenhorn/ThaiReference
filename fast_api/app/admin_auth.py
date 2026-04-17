"""SQLAdmin authentication: env-based username/password."""
from sqladmin.authentication import AuthenticationBackend
from starlette.requests import Request

from app.core.config import settings


class AdminAuth(AuthenticationBackend):
    async def login(self, request: Request) -> bool:
        form = await request.form()
        username = form.get("username")
        password = form.get("password")
        if not username or not password:
            return False
        if username != settings.admin_username or password != settings.admin_password:
            return False
        request.session.update({"authenticated": True})
        return True

    async def logout(self, request: Request) -> bool:
        request.session.clear()
        return True

    async def authenticate(self, request: Request) -> bool:
        return request.session.get("authenticated", False) is True
