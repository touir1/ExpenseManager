#!/bin/sh
# cron-runner.sh: Sets up environment and runs cron
set -e

# Inject your crontab
crontab /etc/cron.d/jobs-runner-crontab

# Start cron service
cron -f

