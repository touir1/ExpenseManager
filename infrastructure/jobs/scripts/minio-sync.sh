#!/bin/bash
# minio-sync.sh: Syncs MinIO bucket to /shared and watches for changes

set -e

# Set up MinIO alias
mc alias set minio http://minio:9000 "$MINIO_ACCESS_KEY" "$MINIO_SECRET_KEY"

# Initial mirror
mc mirror --overwrite minio/expenses-manager-web /shared

# Watch for changes and mirror on event
mc watch minio/expenses-manager-web | while read event; do
    mc mirror --overwrite minio/expenses-manager-web /shared
done
