#!/bin/bash
set -e

# Timestamped logging helper and error trap
log() { printf "%s %s\n" "$(date -Iseconds)" "$*"; }
trap 'log "ERROR at line $LINENO"; exit 1' ERR
log "Mirror job started"

# Ensure SSH key has correct permissions (Git may refuse to use it otherwise)
mkdir -p /root/.ssh
cp /root/.ssh/id_ed25519 /tmp/id_ed25519
chmod 600 /tmp/id_ed25519
export GIT_SSH_COMMAND="ssh -i /tmp/id_ed25519 -o StrictHostKeyChecking=yes"

LIST="/opt/jobs/configs/github-gitlab-sync-repos.txt"
WORKDIR="/work"
mkdir -p "$WORKDIR"

if [ ! -f "$LIST" ]; then
  log "Repo list not found: $LIST"
  exit 1
fi

# Each line: <SRC_URL> <DEST_URL>
grep -Ev '^\s*$|^\s*#' "$LIST" | while read -r SRC DEST; do
  if [ -z "$SRC" ] || [ -z "$DEST" ]; then
    log "Skipping invalid line: SRC or DEST missing"
    continue
  fi

  ID=$(echo -n "$SRC" | sha1sum | awk '{print $1}')
  REPO_DIR="$WORKDIR/$ID.git"

  log "Mirroring: $SRC -> $DEST (repo dir: $REPO_DIR)"
  START_SEC=$(date +%s)

  if [ -d "$REPO_DIR" ]; then
    git -C "$REPO_DIR" remote update --prune
  else
    git clone --mirror "$SRC" "$REPO_DIR"
  fi

  git -C "$REPO_DIR" push --mirror "$DEST"

  # Optional LFS
  if git -C "$REPO_DIR" lfs version >/dev/null 2>&1; then
    git -C "$REPO_DIR" lfs fetch --all || true
    git -C "$REPO_DIR" lfs push --all "$DEST" || true
  fi

  END_SEC=$(date +%s)
  log "Completed: $SRC -> $DEST in $((END_SEC-START_SEC))s"
done

log "Mirror job finished"
