# ThaiReference

## Deploy backend (GitHub Actions)

The backend deploy workflow is **workflow_dispatch** only. Run it from the Actions tab, or with [GitHub CLI](https://cli.github.com/):

```bash
gh auth login

gh run list --workflow=deploy_backend.yaml

# Build FastAPI image
gh workflow run deploy_backend.yaml --ref main -f backend=FastAPI
# Build Spring Boot image
gh workflow run deploy_backend.yaml --ref main -f backend=SpringBoot

gh run watch --exit-status
```
