"""Admin endpoints unrelated to Thai data itself (e.g. DB export)."""

from datetime import datetime, timedelta, timezone

from fastapi import APIRouter, HTTPException, Header
from fastapi.responses import FileResponse
import jwt

from app.core.config import settings

router = APIRouter(prefix="/admin", tags=["admin"])


@router.post("/token")
def issue_admin_token(authorization: str = Header(..., alias="Authorization")) -> dict:
    """Mint a short-lived JWT for admin actions, using a shared secret.

    Call with: Authorization: Bearer <raw-admin-secret>
    The raw-admin-secret must match settings.db_export_token.
    """
    if not (raw := settings.db_export_token):
        raise HTTPException(status_code=500, detail="Admin token secret is not configured")

    scheme, _, credentials = authorization.partition(" ")
    if scheme.lower() != "bearer" or not credentials or credentials != raw:
        raise HTTPException(status_code=401, detail="Unauthorized")

    return {
        "access_token": jwt.encode(
            {
                "exp": datetime.now(timezone.utc) + timedelta(minutes=10)
            },
            raw,
            algorithm="HS256"
        )
    }


@router.get("/db", response_class=FileResponse)
def download_db(authorization: str = Header(..., alias="Authorization")) -> FileResponse:
    """Serve the SQLite database file, protected by an admin JWT."""
    if not (key := settings.db_export_token):
        raise HTTPException(status_code=500, detail="Admin token secret is not configured")

    scheme, _, credentials = authorization.partition(" ")
    if scheme.lower() != "bearer" or not credentials:
        raise HTTPException(status_code=401, detail="Unauthorized")

    try:
        jwt.decode(credentials, key, algorithms=["HS256"])
    except jwt.ExpiredSignatureError:
        raise HTTPException(status_code=401, detail="Token expired")
    except jwt.InvalidTokenError:
        raise HTTPException(status_code=401, detail="Invalid token")

    return FileResponse(path=settings.database_path, filename="thai_sqlite.db", media_type="application/vnd.sqlite3")
