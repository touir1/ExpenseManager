FROM sonatype/nexus3:latest

USER root
RUN apk add --no-cache bash curl jq

COPY provision.sh         /nexus-config/provision.sh
COPY repos.json           /nexus-config/repos.json
COPY docker-entrypoint.sh /docker-entrypoint.sh
RUN chmod +x /nexus-config/provision.sh /docker-entrypoint.sh

USER nexus
ENTRYPOINT ["/docker-entrypoint.sh"]
