# Infrastructure


This directory contains Docker Compose files, configuration folders, scripts, and persistent volumes for orchestrating all services and tools in the ExpenseManager project.


## Structure

- `docker-compose-apps.yml` - Compose file for application containers (nginx, etc.)
- `docker-compose-tools.yml` - Compose file for supporting tools (RabbitMQ, Grafana, Prometheus, etc.)
- `run-docker-compose-apps.bat` - Batch script to start app containers
- `run-docker-compose-tools.bat` - Batch script to start tool containers
- `start-expenses-manager-apps.bat` - Batch script to start all containers

- `configs/` - Service configuration files
	- `gitlab/`, `nginx/`, `prometheus/`, `rabbitmq/`, `sql_database_scripts/`, etc.

- `jobs/` - Job runner and cron-related files
	- `crontab`, `Dockerfile`, `supervisord.conf`, `configs/`, `scripts/`

- `scripts/` - Utility scripts (e.g., `gitlab-register-runner.sh`)

- `volumes/` - Persistent data volumes for services
	- Subfolders for each service: `dind/`, `gitlab/`, `grafana/`, `jobs-runner/`, `minio/`, `nginx/`, `postgres/`, `rabbitmq/`, `redis/`, `sonarqube/`


## Usage

- Start app containers:
	```sh
	run-docker-compose-apps.bat
	```
- Start tool containers:
	```sh
	run-docker-compose-tools.bat
	```
- Start all containers:
	```sh
	start-expenses-manager-apps.bat
	```
