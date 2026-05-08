#!/bin/sh
set -e
/nexus-config/provision.sh &
exec /opt/sonatype/nexus/bin/nexus run
