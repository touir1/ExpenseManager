#!/bin/bash
mkdir -p /root/.docker
AUTH=$(echo -n "${DOCKER_REGISTRY_USER}:${DOCKER_REGISTRY_TOKEN}" | base64 -w 0)
cat <<EOF > /config.json
{
  "auths": {
    "localhost:5050": {
      "auth": "$AUTH"
    },
    "gitlab:5050": {
      "auth": "$AUTH"
    }
  }
}
EOF

mkdir -p /root/.docker
cp /config.json /root/.docker/config.json

export DOCKER_CONFIG=/
export WATCHTOWER_HTTP_API_TOKEN="${WATCHTOWER_HTTP_API_TOKEN}"
exec /usr/local/bin/watchtower --api-version 1.41 --http-api-update --cleanup --label-enable
