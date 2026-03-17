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
    && chmod +x /usr/local/bin/mc

RUN WATCHTOWER_VERSION=$(curl -s https://api.github.com/repos/containrrr/watchtower/releases/latest | grep '"tag_name"' | cut -d'"' -f4) \
    && curl -L -o /tmp/watchtower_linux_amd64.tar.gz \
    "https://github.com/containrrr/watchtower/releases/download/${WATCHTOWER_VERSION}/watchtower_linux_amd64.tar.gz" \
    && tar -xzvf /tmp/watchtower_linux_amd64.tar.gz -C /tmp \
    && mv /tmp/watchtower /usr/local/bin/watchtower \
    && chmod +x /usr/local/bin/watchtower \
    && rm -rf /tmp/watchtower_linux_amd64.tar.gz

CMD ["/usr/bin/supervisord", "-c", "/etc/supervisor/conf.d/supervisord.conf"]
