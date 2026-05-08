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

## Nexus Initial Setup

### Option A: Manual (post-first-boot)

After `docker compose up nexus`:

1. **Get admin password**: `docker exec nexus cat /nexus-data/admin.password`
2. **Complete setup wizard** at `http://localhost:8081`
3. **Create repositories** (via UI → Repository → Repositories → Create):
   - All repos in the table at the top of this plan
   - Docker group must expose HTTP connector on port **8082** (Nexus: Repository Connectors → HTTP)
4. **Create CI user**: Security → Users → Create user `ci-user` with role `nx-anonymous` + read on all repos (or use anonymous access for proxy repos)
5. **Set GitLab CI/CD variables**: `NEXUS_USER`, `NEXUS_PASSWORD`, `NEXUS_NPM_TOKEN`

---

### Option B: Automated provisioning script (recommended)

Files (all under `infrastructure/configs/nexus/`):
- `Dockerfile` — custom Nexus image with `curl`+`jq` installed and scripts baked in
- `docker-entrypoint.sh` — wrapper: starts provision in background, then execs Nexus
- `provision.sh` — provisioning logic (never needs editing for repo changes)
- `repos.json` — repo definitions (edit this to add/remove proxies)

Uses Nexus direct REST API (`POST /service/rest/v1/repositories/{format}/{type}`) — no Groovy Script API required. No sidecar container needed.

#### `Dockerfile`

```dockerfile
FROM sonatype/nexus3:latest

USER root
RUN microdnf install -y curl jq && microdnf clean all

COPY provision.sh         /nexus-config/provision.sh
COPY repos.json           /nexus-config/repos.json
COPY docker-entrypoint.sh /docker-entrypoint.sh
RUN chmod +x /nexus-config/provision.sh /docker-entrypoint.sh

USER nexus
ENTRYPOINT ["/docker-entrypoint.sh"]
```

#### `docker-entrypoint.sh`

```bash
#!/bin/sh
set -e
/nexus-config/provision.sh &
exec /opt/sonatype/start-nexus-repository-manager.sh
```

provision.sh polls `GET /service/rest/v1/status` until Nexus is ready before doing anything, so the background launch is safe.

#### `repos.json`

Add/remove repos here without touching any script:

```json
[
  {"format": "docker", "type": "proxy", "name": "docker-hub-proxy", "remoteUrl": "https://registry-1.docker.io", "indexType": "HUB"},
  {"format": "docker", "type": "proxy", "name": "mcr-proxy",        "remoteUrl": "https://mcr.microsoft.com",    "indexType": "REGISTRY"},
  {"format": "docker", "type": "proxy", "name": "ghcr-proxy",       "remoteUrl": "https://ghcr.io",              "indexType": "REGISTRY"},
  {"format": "docker", "type": "group", "name": "docker-group",     "httpPort": 8082, "members": ["docker-hub-proxy", "mcr-proxy", "ghcr-proxy"]},
  {"format": "npm",    "type": "proxy", "name": "npm-proxy",        "remoteUrl": "https://registry.npmjs.org"},
  {"format": "npm",    "type": "group", "name": "npm-group",        "members": ["npm-proxy"]},
  {"format": "nuget",  "type": "proxy", "name": "nuget-proxy",      "remoteUrl": "https://api.nuget.org/v3/index.json"},
  {"format": "nuget",  "type": "group", "name": "nuget-group",      "members": ["nuget-proxy"]}
]
```

Override at runtime: `-v ./my-repos.json:/nexus-config/repos.json:ro`

#### `provision.sh`

Script flow:
1. **Wait for Nexus** — poll `GET /service/rest/v1/status` until HTTP 200
2. **Read initial password** — `cat /nexus-data/admin.password`
3. **Change admin password** — `PUT /service/rest/v1/security/users/admin/change-password`
4. **Loop over `repos.json`** — for each entry call `POST /service/rest/v1/repositories/{format}/{type}`; skip if already exists
5. **Create CI user** — `POST /service/rest/v1/security/users`
6. **Write flag file** — `touch /nexus-data/.provisioned` — re-runs are no-ops

```bash
#!/usr/bin/env bash
set -euo pipefail

NEXUS_URL="${NEXUS_URL:-http://localhost:8081}"
NEW_ADMIN_PASSWORD="${NEXUS_ADMIN_PASSWORD:-changeme}"
CI_USER="${NEXUS_CI_USER:-ci-user}"
CI_PASSWORD="${NEXUS_CI_PASSWORD:-changeme}"
REPOS_FILE="${REPOS_FILE:-/nexus-config/repos.json}"
FLAG="/nexus-data/.provisioned"

if [ -f "$FLAG" ]; then
  echo "Nexus already provisioned, skipping."
  exit 0
fi

echo "Waiting for Nexus..."
until curl -sf "$NEXUS_URL/service/rest/v1/status" > /dev/null; do sleep 5; done

INIT_PASS=$(cat /nexus-data/admin.password)

echo "Changing admin password..."
curl -sf -u "admin:$INIT_PASS" \
  -X PUT "$NEXUS_URL/service/rest/v1/security/users/admin/change-password" \
  -H "Content-Type: text/plain" \
  -d "$NEW_ADMIN_PASSWORD"

AUTH="admin:$NEW_ADMIN_PASSWORD"

echo "Creating repositories from $REPOS_FILE..."
jq -c '.[]' "$REPOS_FILE" | while read -r repo; do
  FORMAT=$(echo "$repo" | jq -r '.format')
  TYPE=$(echo   "$repo" | jq -r '.type')
  NAME=$(echo   "$repo" | jq -r '.name')

  STATUS=$(curl -s -o /dev/null -w "%{http_code}" -u "$AUTH" \
    "$NEXUS_URL/service/rest/v1/repositories/$FORMAT/$TYPE/$NAME")
  if [ "$STATUS" = "200" ]; then
    echo "  $NAME already exists, skipping."
    continue
  fi

  case "${FORMAT}/${TYPE}" in
    docker/proxy)
      REMOTE=$(echo "$repo" | jq -r '.remoteUrl')
      INDEX=$(echo  "$repo" | jq -r '.indexType // "REGISTRY"')
      PAYLOAD=$(jq -n --arg name "$NAME" --arg remote "$REMOTE" --arg index "$INDEX" '{
        name: $name, online: true,
        docker: {httpPort: null, httpsPort: null, forceBasicAuth: false, v1Enabled: false},
        dockerProxy: {indexType: $index, remoteUrl: $remote},
        httpClient: {blocked: false, autoBlock: true},
        storage: {blobStoreName: "default", strictContentTypeValidation: true},
        proxy: {remoteUrl: $remote, contentMaxAge: 1440, metadataMaxAge: 1440},
        negativeCache: {enabled: true, timeToLive: 1440}
      }')
      ;;
    docker/group)
      PORT=$(echo    "$repo" | jq '.httpPort // null')
      MEMBERS=$(echo "$repo" | jq '[.members[] | {name: .}]')
      PAYLOAD=$(jq -n --arg name "$NAME" --argjson port "$PORT" --argjson members "$MEMBERS" '{
        name: $name, online: true,
        docker: {httpPort: $port, httpsPort: null, forceBasicAuth: false, v1Enabled: false},
        group: {memberNames: [$members[].name]},
        storage: {blobStoreName: "default", strictContentTypeValidation: true}
      }')
      ;;
    npm/proxy)
      REMOTE=$(echo "$repo" | jq -r '.remoteUrl')
      PAYLOAD=$(jq -n --arg name "$NAME" --arg remote "$REMOTE" '{
        name: $name, online: true,
        httpClient: {blocked: false, autoBlock: true},
        storage: {blobStoreName: "default", strictContentTypeValidation: true},
        proxy: {remoteUrl: $remote, contentMaxAge: 1440, metadataMaxAge: 1440},
        negativeCache: {enabled: true, timeToLive: 1440}
      }')
      ;;
    npm/group|nuget/group)
      MEMBERS=$(echo "$repo" | jq -r '[.members[]]')
      PAYLOAD=$(jq -n --arg name "$NAME" --argjson members "$MEMBERS" '{
        name: $name, online: true,
        group: {memberNames: $members},
        storage: {blobStoreName: "default", strictContentTypeValidation: true}
      }')
      ;;
    nuget/proxy)
      REMOTE=$(echo "$repo" | jq -r '.remoteUrl')
      PAYLOAD=$(jq -n --arg name "$NAME" --arg remote "$REMOTE" '{
        name: $name, online: true,
        httpClient: {blocked: false, autoBlock: true},
        storage: {blobStoreName: "default", strictContentTypeValidation: true},
        proxy: {remoteUrl: $remote, contentMaxAge: 1440, metadataMaxAge: 1440},
        negativeCache: {enabled: true, timeToLive: 1440}
      }')
      ;;
    *)
      echo "  Unknown format/type ${FORMAT}/${TYPE}, skipping $NAME."
      continue
      ;;
  esac

  echo "  Creating $NAME ($FORMAT/$TYPE)..."
  curl -sf -u "$AUTH" \
    -X POST "$NEXUS_URL/service/rest/v1/repositories/$FORMAT/$TYPE" \
    -H "Content-Type: application/json" \
    -d "$PAYLOAD"
done

echo "Creating CI user '$CI_USER'..."
curl -sf -u "$AUTH" \
  -X POST "$NEXUS_URL/service/rest/v1/security/users" \
  -H "Content-Type: application/json" \
  -d "{\"userId\":\"$CI_USER\",\"firstName\":\"CI\",\"lastName\":\"User\",\"emailAddress\":\"ci@local\",\"password\":\"$CI_PASSWORD\",\"status\":\"active\",\"roles\":[\"nx-anonymous\"]}"

touch "$FLAG"
echo "Done."
```

#### docker-compose-tools.yml — `nexus` service

Replace `image:` with `build:` and add env vars:

```yaml
nexus:
  build: ./configs/nexus
  container_name: nexus
  networks:
    expenses_manager_tools_net:
      ipv4_address: 172.50.0.50
  ports:
    - "8081:8081"
    - "8082:8082"
  volumes:
    - nexus-data:/nexus-data
  environment:
    - INSTALL4J_ADD_VM_PARAMS=-Xms512m -Xmx1g -XX:MaxDirectMemorySize=1g
    - NEXUS_ADMIN_PASSWORD=${NEXUS_ADMIN_PASSWORD}
    - NEXUS_CI_USER=${NEXUS_CI_USER}
    - NEXUS_CI_PASSWORD=${NEXUS_CI_PASSWORD}
```

No sidecar service needed.

**Notes:**
- `ci-user` credentials come from `.env` via docker-compose; set `NEXUS_USER`/`NEXUS_PASSWORD`/`NEXUS_NPM_TOKEN` as matching GitLab CI/CD variables
- Flag file `/nexus-data/.provisioned` prevents re-provisioning on container restart
- To add a new proxy repo: add one JSON object to `repos.json`, rebuild the image (`docker compose build nexus`), delete the flag file, restart the container

---

## Summary of File Changes

| File | Change |
|---|---|
| `infrastructure/docker-compose-tools.yml` | Add `nexus` service (`build:` not `image:`), add `NEXUS_*` env vars, add volume |
| `infrastructure/configs/gitlab-runner/config.toml` | Add `nexus:172.50.0.50` to `extra_hosts` |
| `infrastructure/configs/gitlab-ci-templates/ci-init.yml` | Add `NEXUS_*` variables |
| `infrastructure/configs/gitlab-ci-templates/ci-build.yml` | npm registry + nuget source config |
| `infrastructure/configs/gitlab-ci-templates/ci-docker.yml` | insecure-registry + registry-mirror + nexus login |
| `infrastructure/.env` + `.env.example` | Add nexus credentials |
| `backend/users/Dockerfile` | Optional: build arg for registry prefix |
| `backend/expenses/Dockerfile` | Optional: build arg for registry prefix |
| `infrastructure/configs/nexus/Dockerfile` | Custom Nexus image — installs `curl`+`jq`, bakes in scripts |
| `infrastructure/configs/nexus/docker-entrypoint.sh` | Wrapper entrypoint: launches provision in background, execs Nexus |
| `infrastructure/configs/nexus/provision.sh` | Provisioning logic — reads `repos.json`, changes password, creates CI user |
| `infrastructure/configs/nexus/repos.json` | Repo definitions — edit to add/remove proxies without touching any script |

## Verification

1. `cd infrastructure && docker compose -f docker-compose-tools.yml up nexus -d`
2. Nexus UI accessible at `http://localhost:8081`
3. Push a test npm install through it: `npm install --registry http://localhost:8081/repository/npm-group/ react`; verify cached in Nexus browse UI
4. Push a test `dotnet restore` with the nexus source; verify nuget packages cached
5. Run a CI pipeline; check `docker pull` steps show `nexus:8082/...` or mirror hit in DinD logs
6. Verify cache hit on second pipeline run (pull time drops)
