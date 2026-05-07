# Plan: Integrate Sonatype Nexus Repository as Caching/Proxy

## Context

Currently the project pulls packages and images directly from public registries:
- Docker Hub, MCR (`mcr.microsoft.com`), GHCR (`ghcr.io`) — Docker images in CI & Dockerfiles
- npmjs.org — frontend deps (`node:20` CI job, `npm ci`)
- nuget.org — .NET backend packages (`mcr.microsoft.com/dotnet/sdk:8.0` CI job, `dotnet restore`)

Adding Nexus as a caching proxy layer:
- Speeds up CI (cache misses hit Nexus, not the internet)
- Survives upstream rate-limiting (Docker Hub pulls, npm)
- Single egress point for all package traffic

---

## What Nexus Will Proxy

| Repository name | Type | Upstream |
|---|---|---|
| `docker-hub-proxy` | Docker proxy | https://registry-1.docker.io |
| `mcr-proxy` | Docker proxy | https://mcr.microsoft.com |
| `ghcr-proxy` | Docker proxy | https://ghcr.io |
| `docker-group` | Docker group (port 8082) | aggregates above 3 |
| `npm-proxy` | npm proxy | https://registry.npmjs.org |
| `npm-group` | npm group | aggregates npm-proxy |
| `nuget-proxy` | NuGet (proxy) | https://api.nuget.org/v3/index.json |
| `nuget-group` | NuGet group | aggregates nuget-proxy |

Nexus is configured via UI or provisioning script after first boot; repositories are NOT auto-created.

---

## Files to Modify

### 1. `infrastructure/docker-compose-tools.yml`

Add `nexus` service. IP `172.50.0.50` is next free slot (gitlab=.10, sonarqube=.20, minio=.30, jobs-runner=.40).

```yaml
nexus:
  image: sonatype/nexus3:latest
  container_name: nexus
  networks:
    expenses_manager_tools_net:
      ipv4_address: 172.50.0.50
  ports:
    - "8081:8081"   # UI + npm + nuget
    - "8082:8082"   # Docker group (proxy mirror)
  volumes:
    - nexus-data:/nexus-data
  environment:
    - INSTALL4J_ADD_VM_PARAMS=-Xms512m -Xmx1g -XX:MaxDirectMemorySize=1g
```

Add `nexus-data` to the top-level `volumes:` block.

---

### 2. `infrastructure/configs/gitlab-runner/config.toml`

Add `nexus` to `extra_hosts` so the runner's Docker executor can resolve it:

```toml
extra_hosts = ["gitlab:172.50.0.10", "sonarqube:172.50.0.20", "minio:172.50.0.30", "jobs-runner:172.50.0.40", "nexus:172.50.0.50"]
```

---

### 3. `infrastructure/configs/gitlab-ci-templates/ci-docker.yml`

Two changes in `.docker-build` and `.docker-security`:

**a) Add insecure registry flags to DinD service:**
```yaml
services:
  - name: docker:24-dind
    command:
      - "--host=tcp://0.0.0.0:2375"
      - "--insecure-registry=gitlab:5050"
      - "--insecure-registry=172.50.0.10:5050"
      - "--insecure-registry=nexus:8082"          # ADD
      - "--registry-mirror=http://nexus:8082"     # ADD — Docker Hub mirror
```

**b) Add nexus login before `docker build`** (so DinD can pull base images from nexus):
```yaml
script:
  - echo "$NEXUS_PASSWORD" | docker login nexus:8082 -u "$NEXUS_USER" --password-stdin
  - FULL_IMAGE_NAME="$CI_REGISTRY_IMAGE/$IMAGE_NAME:$IMAGE_VERSION"
  - echo "$CI_JOB_TOKEN" | docker login $CI_REGISTRY -u gitlab-ci-token --password-stdin
  - docker build -t $FULL_IMAGE_NAME .
  - docker push $FULL_IMAGE_NAME
  - docker builder prune -af
```

Note: `--registry-mirror` only proxies Docker Hub pulls. MCR and GHCR images require Dockerfile changes (see §6).

---

### 4. `infrastructure/configs/gitlab-ci-templates/ci-init.yml`

Add Nexus variables alongside existing `CI_REGISTRY`:

```yaml
variables:
  CI_REGISTRY: "gitlab:5050"
  NEXUS_HOST: "nexus:8081"
  NEXUS_DOCKER_REGISTRY: "nexus:8082"
  NEXUS_NPM_REGISTRY: "http://nexus:8081/repository/npm-group/"
  NEXUS_NUGET_SOURCE: "http://nexus:8081/repository/nuget-group/index.json"
```

`NEXUS_USER` and `NEXUS_PASSWORD` are secrets — set in GitLab CI/CD variables, NOT in this file.

---

### 5. `infrastructure/configs/gitlab-ci-templates/ci-build.yml`

**a) `.node-build` — point npm to Nexus before `npm ci`:**
```yaml
.node-build:
  stage: build
  image: node:20
  script:
    - npm config set cache .npm --location=project
    - npm config set registry $NEXUS_NPM_REGISTRY
    - npm config set //${NEXUS_HOST}/repository/npm-group/:_authToken "$NEXUS_NPM_TOKEN"
    - npm ci
    - npm run build:prod
```

Or simpler (no auth if Nexus npm group allows anonymous read):
```yaml
    - npm config set registry $NEXUS_NPM_REGISTRY
```

**b) `.dotnet-build` — add Nexus as NuGet source:**
```yaml
.dotnet-build:
  stage: build
  image: mcr.microsoft.com/dotnet/sdk:8.0
  variables:
    DOTNET_CLI_TELEMETRY_OPTOUT: "1"
    DOTNET_NOLOGO: "1"
    BUILD_CONFIGURATION: Release
    NUGET_PACKAGES: "$CI_PROJECT_DIR/.nuget/packages"
  script:
    - dotnet nuget add source "$NEXUS_NUGET_SOURCE" --name nexus --username "$NEXUS_USER" --password "$NEXUS_PASSWORD" --store-password-in-clear-text
    - dotnet restore
    - dotnet build --configuration $BUILD_CONFIGURATION
```

The `--store-password-in-clear-text` flag is required on Linux (no credential manager); credentials never leave the CI runner.

---

### 6. `backend/users/Dockerfile` and `backend/expenses/Dockerfile`

**Optional but recommended:** replace MCR direct pulls with Nexus-proxied equivalents so Docker build cache hits Nexus instead of MCR.

Current:
```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS final
```

With Nexus (only effective when building via CI DinD logged into nexus:8082):
```dockerfile
FROM nexus:8082/dotnet/sdk:8.0 AS build
FROM nexus:8082/dotnet/aspnet:8.0-alpine AS final
```

**Tradeoff:** Breaks local `docker build` unless developer also has nexus:8082 accessible. Options:
- Use build arg: `ARG REGISTRY=mcr.microsoft.com` → `FROM ${REGISTRY}/dotnet/sdk:8.0`; CI passes `--build-arg REGISTRY=nexus:8082`
- Or keep Dockerfiles pointing at MCR and rely on DinD's `--registry-mirror` for Docker Hub only (MCR not mirrored this way)

Recommended: **build arg approach** — no local dev breakage, full nexus caching in CI.

---

### 7. `infrastructure/.env` and `infrastructure/.env.example`

Add Nexus credentials:

```env
# Nexus Repository Manager
NEXUS_ADMIN_PASSWORD=changeme
NEXUS_CI_USER=ci-user
NEXUS_CI_PASSWORD=changeme
```

---

## Nexus Initial Setup (Manual, post-first-boot)

After `docker compose up nexus`:

1. **Get admin password**: `docker exec nexus cat /nexus-data/admin.password`
2. **Complete setup wizard** at `http://localhost:8081`
3. **Create repositories** (via UI → Repository → Repositories → Create):
   - All repos in the table at the top of this plan
   - Docker group must expose HTTP connector on port **8082** (Nexus: Repository Connectors → HTTP)
4. **Create CI user**: Security → Users → Create user `ci-user` with role `nx-anonymous` + read on all repos (or use anonymous access for proxy repos)
5. **Set GitLab CI/CD variables**: `NEXUS_USER`, `NEXUS_PASSWORD`, `NEXUS_NPM_TOKEN`

Optional: script the above via Nexus REST API (`POST /service/rest/v1/repositories/...`) in `infrastructure/configs/nexus/provision.sh`.

---

## Summary of File Changes

| File | Change |
|---|---|
| `infrastructure/docker-compose-tools.yml` | Add `nexus` service + volume |
| `infrastructure/configs/gitlab-runner/config.toml` | Add `nexus:172.50.0.50` to `extra_hosts` |
| `infrastructure/configs/gitlab-ci-templates/ci-init.yml` | Add `NEXUS_*` variables |
| `infrastructure/configs/gitlab-ci-templates/ci-build.yml` | npm registry + nuget source config |
| `infrastructure/configs/gitlab-ci-templates/ci-docker.yml` | insecure-registry + registry-mirror + nexus login |
| `infrastructure/.env` + `.env.example` | Add nexus credentials |
| `backend/users/Dockerfile` | Optional: build arg for registry prefix |
| `backend/expenses/Dockerfile` | Optional: build arg for registry prefix |
| `infrastructure/configs/nexus/provision.sh` | Optional: automated repo creation script |

## Verification

1. `cd infrastructure && docker compose -f docker-compose-tools.yml up nexus -d`
2. Nexus UI accessible at `http://localhost:8081`
3. Push a test npm install through it: `npm install --registry http://localhost:8081/repository/npm-group/ react`; verify cached in Nexus browse UI
4. Push a test `dotnet restore` with the nexus source; verify nuget packages cached
5. Run a CI pipeline; check `docker pull` steps show `nexus:8082/...` or mirror hit in DinD logs
6. Verify cache hit on second pipeline run (pull time drops)
