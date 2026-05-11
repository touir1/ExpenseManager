# Infrastructure & CI/CD

← [Wiki Index](./index.md)

---

## Overview

All services are containerized with Docker and orchestrated via Docker Compose. The CI/CD pipeline runs on GitLab CI and covers build, test, code quality, security scanning, container build, and deployment. Monitoring is provided by Prometheus and Grafana.

**Infrastructure root:** `infrastructure/`

---

## Docker Compose Stacks

There are two separate Compose files, intentionally split to allow starting app containers independently from tooling containers.

### App Stack (`docker-compose-apps.yml`)

Contains the services that make up the running application:

| Service | Image | Port | Description |
|---|---|---|---|
| `nginx` | `nginx:latest` | 80, 443 | Reverse proxy, TLS termination, SPA serving, auth gate |
| `users-service` | GitLab Registry | 9100 | Users/auth backend |
| `expenses-service` | GitLab Registry | 9200 | Expenses backend |

**Network:** `expenses_manager_apps_net` — bridge network, subnet `172.100.0.0/24`

All services share this network so nginx can reach backend services by container name (e.g. `http://users-service:9100`).

```bash
# Start app containers
./run-docker-compose-apps.bat
```

### Tools Stack (`docker-compose-tools.yml`)

Supporting infrastructure services:

| Service | Port(s) | Description |
|---|---|---|
| PostgreSQL | 5432 | Primary database for all services |
| RabbitMQ | 5672 (AMQP), 15672 (Management UI) | Message broker |
| Redis | 6379 | (Reserved for future use) |
| Grafana | 3000 | Metrics dashboards |
| Prometheus | 9090 | Metrics collection |
| Mailpit | 1025 (SMTP), 8025 (Web UI) | Local email testing |
| MinIO | 9000, 9001 | Artifact storage (frontend builds) |
| GitLab | 80, 443 | Source code management + CI runner |
| SonarQube | 9001 | Code quality analysis |

**Network:** `expenses_manager_tools_net` — bridge network, subnet `172.50.0.0/24`

```bash
# Start tool containers
./run-docker-compose-tools.bat

# Start everything
./start-expenses-manager-apps.bat
```

---

## nginx Configuration

**Config location:** `infrastructure/configs/nginx/`

### nginx.conf

Standard nginx configuration with:
- `worker_processes auto`
- `keepalive_timeout 65`
- Includes all files in `sites-enabled/`

### expenses-manager.conf

The main virtual host configuration (`sites-available/expenses-manager.conf`):

**HTTP server (port 80):**
- Redirects all traffic to HTTPS (`301`)

**HTTPS server (port 443):**
- TLS: `ssl_certificate` + `ssl_certificate_key` from `ssl/`
- Protocols: TLS 1.2, TLS 1.3
- `Strict-Transport-Security: max-age=1209600`
- `X-Frame-Options: sameorigin`
- `proxy_cookie_path / "/; HTTPOnly; Secure; SameSite=strict"` — enforces secure cookie attrs on all proxied Set-Cookie headers

**Route table:**

| Path | Behavior |
|---|---|
| `/` | Serve static SPA files; unknown paths rewrite to `index.html` |
| `/internal/auth/check` | `internal` location — proxies to `users-service:9100/auth/check`; not externally reachable |
| `/api/users/auth/check` | Returns 404 (blocks external access to auth check endpoint) |
| `/api/users/auth/*` | Proxies to `users-service:9100/auth/*` — **no auth_request** (public) |
| `/api/users/*` | `auth_request /internal/auth/check` + proxy to `users-service:9100/` |
| `/api/expenses/*` | `auth_request /internal/auth/check` + proxy to `expenses-service:9200/` |

**CORS:** `map` blocks derive `Access-Control-Allow-Origin`, `Access-Control-Allow-Methods`, `Access-Control-Allow-Headers` from the request `Origin` header. Credentials allowed. `OPTIONS` preflight returns `204`.

**Open issues:**
- `S-2` — `Content-Security-Policy` header not yet added

---

## CI/CD Pipeline

### Pipeline Structure

Defined by individual service `.gitlab-ci.yml` files that reference shared templates from `infrastructure/configs/gitlab-ci-templates/`.

**Stages (in order):**

```
build → test → quality → security → docker → docker-security → deploy
```

Defined in `ci-stages.yml`.

### Job Templates

| Template file | Jobs |
|---|---|
| `ci-init.yml` | Global variables and `before_script` |
| `ci-build.yml` | `.dotnet-build`, `.node-build` |
| `ci-test.yml` | `.dotnet-test`, `.node-test` |
| `ci-quality.yml` | `.dotnet-sonarqube`, `.node-sonarqube` |
| `ci-security.yml` | `.sca` (OWASP Dependency Check), Semgrep SAST |
| `ci-docker.yml` | `.docker-build` — builds and pushes image to GitLab registry |
| `ci-docker-security.yml` | `.docker-security` — Trivy image scan |
| `ci-deploy.yml` | `.deploy-web-dev`, `.docker-deploy-dev` |

### Build Jobs

**`.dotnet-build`** — restores and builds the .NET solution  
**`.node-build`** — runs `npm ci` and `npm run build:prod`; outputs `dist/`

### Test Jobs

**`.dotnet-test`** — runs `dotnet test` with XPlat Code Coverage (OpenCover format)  
**`.node-test`** — runs `npm test` with V8 coverage

### Quality Jobs

**`.dotnet-sonarqube`** — runs SonarQube scanner on .NET project; enforces quality gate  
**`.node-sonarqube`** — runs SonarQube scanner on frontend project

### Security Jobs

**`.sca`** — OWASP Dependency Check; scans NuGet and npm packages for known CVEs  
**Semgrep SAST** — static code analysis for security patterns

### Docker Jobs

**`.docker-build`** — builds Docker image from `Dockerfile`; tags with commit SHA and `latest`; pushes to GitLab Container Registry (`localhost:5050`)

**`.docker-security`** — runs Trivy against the built image; fails on high/critical CVEs

### Deploy Jobs (manual)

**`.deploy-web-dev`** — frontend deployment:
1. Runs `npm run build:prod` → `dist/`
2. Zips `dist/` with a timestamp-based version name
3. Uploads zip to MinIO bucket `expenses-manager-web-artifacts`
4. Copies files to MinIO bucket `expenses-manager-web`

**`.docker-deploy-dev`** — backend deployment:
1. Calls `GET http://jobs-runner:8989/v1/update?servicename=$SERVICE_NAME` with Bearer token
2. The Docker Updater API pulls the latest image and recreates the container

---

## Docker Updater API

**Location:** `infrastructure/jobs/`  
**Runtime:** Python, deployed via supervisord inside the `jobs-runner` container  
**Port:** `8989`

A custom lightweight HTTP API that enables the CI/CD pipeline to hot-update running containers without shell access to the Docker host.

**Endpoint:** `GET /v1/update?servicename=<name>`  
**Auth:** Bearer token (`DOCKER_UPDATER_HTTP_API_TOKEN`)

**Process:**
1. Validates Bearer token
2. Finds container by `com.touir.expensesmanager.dockerupdater.servicename` label
3. Pulls latest image from registry
4. Recreates the container (stop → remove → create → start)

**Container labels** (set in `docker-compose-apps.yml`):
```yaml
labels:
  - "com.touir.expensesmanager.dockerupdater.enable=true"
  - "com.touir.expensesmanager.dockerupdater.servicename=backend-users"
```

---

## Monitoring

### Prometheus

Scrapes metrics from backend services and infrastructure. Configuration: `infrastructure/configs/prometheus/prometheus.yml`.

Access: `http://localhost:9090`

### Grafana

Dashboards built on top of Prometheus metrics. Pre-configured datasources and dashboards are persisted in `infrastructure/volumes/grafana/`.

Access: `http://localhost:3000`

---

## Mailpit (Local Email Testing)

Mailpit is a local SMTP server that captures all outbound emails during development.

- **SMTP:** `host.docker.internal:1025` (used by `users-service` in dev mode; `EnableSsl=false`)
- **Web UI:** `http://localhost:8025` — browse all sent emails, inspect HTML content

Configure `users-service` for Mailpit in `appsettings.Development.json` or via environment variables:
```
EXPENSES_MANAGEMENT_USERS_EMAILAUTH_HOST=host.docker.internal
EXPENSES_MANAGEMENT_USERS_EMAILAUTH_PORT=1025
EXPENSES_MANAGEMENT_USERS_EMAILAUTH_ENABLESSL=false
```

---

## Persistent Volumes

All service data is persisted in `infrastructure/volumes/`:

| Volume | Service |
|---|---|
| `postgres/` | PostgreSQL data |
| `rabbitmq/` | RabbitMQ messages and queues |
| `grafana/` | Grafana dashboards and datasources |
| `nginx/` | nginx logs and static web files |
| `gitlab/` | GitLab repositories and configuration |
| `sonarqube/` | SonarQube analysis data |
| `minio/` | MinIO object storage (frontend artifacts) |
| `redis/` | Redis data |
| `dind/` | Docker-in-Docker for GitLab CI |
| `jobs-runner/` | jobs-runner container data |
| `mailpit/` | Mailpit email storage |

---

## Nexus Repository Manager

**Location:** `infrastructure/configs/nexus/`

Sonatype Nexus Repository Manager is used as a caching proxy for Docker images and npm packages, reducing CI/CD dependency on external registries.

### Startup

Nexus uses a custom Docker image. `provision.sh` runs in the background at container startup via `docker-entrypoint.sh`. The script is idempotent — it checks for a `/nexus-data/.provisioned` flag before running.

Repository definitions are in `repos.json` and applied via direct REST API calls (no Groovy scripts).

### Repositories

| Type | Purpose |
|---|---|
| Docker proxy | Proxies `mcr.microsoft.com`, `docker.io`, etc. |
| Docker group | Aggregates Docker proxies; exposed on port `8082` |
| npm proxy | Proxies `registry.npmjs.org` |
| npm group | Aggregates npm proxies |

### CI/CD Integration

- Docker builds: `ARG REGISTRY=mcr.microsoft.com` in Dockerfiles — CI passes `--build-arg REGISTRY=$NEXUS_DOCKER_REGISTRY` to redirect pulls through Nexus
- Docker DinD `command` array: `nexus:8082` hardcoded (shell variable expansion is not available in `command` arrays)
- npm builds: `NEXUS_NPM_TOKEN` CI/CD secret used for npm auth against the Nexus npm group
- Secrets: `NEXUS_USER`, `NEXUS_PASSWORD`, `NEXUS_NPM_TOKEN` are GitLab CI/CD secrets — not in any yml file
- `forceBasicAuth: true` set on both proxy and group repos

---

## Secrets Management

All secrets are passed via environment variables. Never hardcoded. Key variables:

- `EXPENSES_MANAGEMENT_USERS_JWT_SECRET_KEY` — JWT signing key
- `EXPENSES_MANAGEMENT_USERS_DATABASE_PASSWORD` — PostgreSQL password
- `EXPENSES_MANAGEMENT_USERS_EMAILAUTH_PASSWORD` — SMTP credentials
- `DOCKER_UPDATER_HTTP_API_TOKEN` — Docker Updater API auth token
- `MINIO_ACCESS_KEY`, `MINIO_SECRET_KEY` — MinIO credentials

A `.env` file at the infrastructure root provides these to Docker Compose. This file is git-ignored. See `.env.example` (if present) for required keys.

Secret scanning: **Gitleaks** runs in CI on every push (configured via `.gitleaks.toml`).
