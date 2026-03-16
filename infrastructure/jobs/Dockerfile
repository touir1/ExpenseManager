FROM python:3.11-slim


# Install cron, supervisor, bash, curl, and MinIO Client (mc)
RUN apt-get update && apt-get install -y \
    cron \
    supervisor \
    bash \
    curl \
    git \
    git-lfs \
    ca-certificates \
    openssh-client \
    && rm -rf /var/lib/apt/lists/* \
    && curl -L -o /usr/local/bin/mc "https://dl.min.io/client/mc/release/linux-amd64/mc" \
    && chmod +x /usr/local/bin/mc \
    && curl -L -o /usr/local/bin/watchtower \
    https://github.com/containrrr/watchtower/releases/latest/download/watchtower-linux-amd64 \
    && chmod +x /usr/local/bin/watchtower

CMD ["/usr/bin/supervisord", "-c", "/etc/supervisor/conf.d/supervisord.conf"]
