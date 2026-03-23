# ExpenseManager

> **ExpenseManager** is a full-stack application for managing expenses, users, and dashboards, built with .NET 8 (backend), React 18 + Tailwind CSS (frontend), and Docker-based infrastructure.

---

## Workspace Structure

```
backend/         # .NET microservices (expenses, users, dashboard)
frontend/        # React dashboard app (Vite + TypeScript)
infrastructure/  # Docker Compose, configs, scripts
mobile/          # (Reserved for mobile app)
```

---

## Backend Services

- **expenses** (port 9200): Expense management API — .NET 8, EF Core 8, PostgreSQL
- **users** (port 9100): User management and JWT auth API — .NET 8, EF Core 8, PostgreSQL
- **dashboard**: (Planned) Dashboard API

Each service has its own solution and project files. Configurations use environment variables for secrets (see `appsettings.json` and Docker Compose files).

### Running Backend Locally

1. Navigate to the service folder (e.g., `backend/expenses/Touir.ExpensesManager.Expenses`)
2. Restore and run:
	```sh
	dotnet restore
	dotnet run
	```

---

## Frontend Dashboard

Location: `frontend/dashboard`

React 18 + TypeScript + Vite 7 SPA styled with Tailwind CSS v3. All traffic flows through nginx (port 80/443), which enforces JWT auth before forwarding to backend services.

### Quick Start

```sh
cd frontend/dashboard
npm ci
npm run dev
```

Configure API base URL in `.env`:
```
VITE_API_BASE="http://localhost:80" # nginx reverse proxy
```

---

## Infrastructure & Dev Environment

All services are containerized. Use Docker Compose for orchestration:

```sh
cd infrastructure
./run-docker-compose-apps.bat      # App containers (nginx, etc.)
./run-docker-compose-tools.bat      # Tools (RabbitMQ, Grafana, Prometheus, etc.)
```

### Key Services
- **nginx**: Reverse proxy
- **RabbitMQ**: Messaging (http://localhost:15672, default user: admin/admin)
- **Grafana**: Dashboards (http://localhost:3000)
- **Prometheus**: Monitoring (http://localhost:9090)
- **Mailpit**: Local email testing — SMTP on port 1025, web UI at http://localhost:8025
- **Docker Image Updater API**: Custom Python API running on port `8989` of the `jobs-runner` that updates tagged containers securely via webhooks.

---

## Database

SQL scripts for DB setup are in `backend/expenses/sql_database_scripts/`.

---

## Useful Links

- [RabbitMQ Management](http://localhost:15672/)
- [Grafana Dashboards](http://localhost:3000/)
- [Prometheus](http://localhost:9090/)
- [Mailpit Web UI](http://localhost:8025/)
- [RabbitMQ API Docs](https://rawcdn.githack.com/rabbitmq/rabbitmq-server/v3.12.12/deps/rabbitmq_management/priv/www/api/index.html)

---

## Contributing

1. Fork and clone the repo
2. Use feature branches
3. Open PRs for review
4. Ensure your code passes GitLab CI pipelines, including SonarQube quality gates, health checks, and unit tests.

---

## License

See [LICENSE](LICENSE)
