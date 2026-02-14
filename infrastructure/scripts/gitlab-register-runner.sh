#!/bin/sh
set -euo pipefail

# Registers the GitLab runner using a project or group REGISTRATION_TOKEN.
# Requires env: CI_SERVER_URL, REGISTRATION_TOKEN. Optional: RUNNER_NAME, RUNNER_TAGS.
# This script is intended to run inside the gitlab-runner container.

# Prescript: always sync mounted config into /etc/gitlab-runner
SRC_DIR=/gitlab-runner-config
DEST_DIR=/etc/gitlab-runner
mkdir -p "$DEST_DIR"
if [ -d "$SRC_DIR" ]; then
  # include dotfiles by copying the directory contents (the trailing /. trick)
  cp -a "$SRC_DIR"/. "$DEST_DIR"/ || true
  echo "[prescript] Synced runner config from $SRC_DIR to $DEST_DIR"
fi

CI_URL=${CI_SERVER_URL:-"http://gitlab:80"}
RUNNER_TOKEN=${RUNNER_TOKEN:-""}
RUNNER_NAME=${RUNNER_NAME:-"dind-runner"}
RUNNER_TAGS=${RUNNER_TAGS:-"docker"}

# Fallback (not recommended): if RUNNER_TOKEN not provided, try REGISTRATION_TOKEN
if [ -z "$RUNNER_TOKEN" ] && [ -n "${REGISTRATION_TOKEN:-}" ]; then
  echo "[prescript] RUNNER_TOKEN not set; attempting to use REGISTRATION_TOKEN as token (not recommended)."
  RUNNER_TOKEN=$REGISTRATION_TOKEN
fi

CONFIG=/etc/gitlab-runner/config.toml
if [ ! -f "$CONFIG" ]; then
  echo "ERROR: $CONFIG not found after sync; ensure /gitlab-runner-config contains config.toml"
  exit 1
fi

# Substitute all ${VAR_NAME} placeholders using current environment
env_substitute_file() {
  file="$1"
  # Iterate over environment variables and replace ${NAME} occurrences
  env | while IFS='=' read -r name value; do
    # Only valid shell env names
    echo "$name" | grep -Eq '^[A-Za-z_][A-Za-z0-9_]*$' || continue
    # Escape sed replacement special chars
    esc_value=$(printf '%s' "$value" | sed -e 's/[&\\/]/\\&/g')
    sed -i -e "s|\${$name}|$esc_value|g" "$file"
  done
}

env_substitute_file "$CONFIG"
echo "[prescript] Replaced environment placeholders in $CONFIG"

# After substitution, if token lines remain unresolved or invalid, inject RUNNER_TOKEN if provided
extract_token() {
  sed -n 's/.*token\s*=\s*"\(.*\)".*/\1/p' "$CONFIG" | head -n1 || true
}

TOKEN_VAL=$(extract_token)
if echo "$TOKEN_VAL" | grep -q '{\$'; then
  echo "[prescript] Token still contains placeholder after substitution."
fi

if [ -n "$RUNNER_TOKEN" ]; then
  if [ -z "$TOKEN_VAL" ] || echo "$TOKEN_VAL" | grep -q '^glrt-'; then
    tmpfile=$(mktemp)
    awk -v rt="$RUNNER_TOKEN" '{
      if ($0 ~ /^[[:space:]]*token[[:space:]]*=/) {
        sub(/=.*/, "= \"" rt "\"")
      }
      print
    }' "$CONFIG" > "$tmpfile" && mv "$tmpfile" "$CONFIG"
    echo "[prescript] Injected runner token into $CONFIG"
  fi
else
  if [ -z "$TOKEN_VAL" ] || echo "$TOKEN_VAL" | grep -q '^glrt-'; then
    echo "[prescript] WARNING: RUNNER_TOKEN not provided and token appears invalid. Runner may fail to fetch jobs."
  fi
fi

exec gitlab-runner run
