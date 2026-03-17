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
export DOCKER_UPDATER_HTTP_API_TOKEN="${DOCKER_UPDATER_HTTP_API_TOKEN}"
export DOCKER_UPDATER_HTTP_API_PORT="${DOCKER_UPDATER_HTTP_API_PORT}"
python3 /opt/jobs/scripts/docker_image_updater_api.py
