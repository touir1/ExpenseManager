# Infrastructure

Contains Docker Compose files, configuration, and scripts for orchestrating all services and tools.

## Structure
- `docker-compose-apps.yml` - App containers (nginx, etc.)
- `docker-compose-tools.yml` - Tools (RabbitMQ, Grafana, Prometheus, etc.)
- `configs/` - Service configuration files
- `scripts/` - Utility scripts
- `volumes/` - Persistent data volumes

## Usage
- Start app containers: `run-docker-compose-apps.bat`
- Start tool containers: `run-docker-compose-tools.bat`
- Start all: `start-expenses-manager-apps.bat`
