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
