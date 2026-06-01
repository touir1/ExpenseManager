# ExpenseManager — Project Wiki

**ExpenseManager** is a full-stack expense tracking application built with .NET 8 microservices, a React/TypeScript SPA, and a Docker-based infrastructure stack.

---

## Contents

| Document | Description |
|---|---|
| [Architecture](./architecture.md) | System design, service topology, request flow, deployment model |
| [Use Cases](./use-cases.md) | User-facing workflows and functional scope |
| [Backend — Users Service](./backend-users-service.md) | Auth, registration, password management, JWT, email, outbox |
| [Backend — Expenses Service](./backend-expenses-service.md) | Expense CRUD, categories, currencies, families, audit |
| [Family System](./family-system.md) | Family management — creation, invitations, roles, expense attribution |
| [Messaging](./messaging.md) | RabbitMQ topology, outbox pattern, inbox deduplication, event payloads |
| [Frontend Dashboard](./frontend.md) | React SPA, routing, auth context, component library |
| [Infrastructure & CI/CD](./infrastructure.md) | Docker Compose, nginx, Nexus, CI/CD pipeline, monitoring |
| [Data Models](./data-models.md) | Database schema, entity relationships, migration history |
| [API Reference](./api-reference.md) | Full endpoint listing with request/response shapes |
| [Testing Guide](./testing.md) | Test setup, helpers, patterns for all three codebases |

---

## Quick Reference

| Concern | Technology |
|---|---|
| Backend framework | .NET 8 / ASP.NET Core |
| ORM | Entity Framework Core 8 + Npgsql |
| Validation | FluentValidation 11 |
| Frontend framework | React 18 + TypeScript |
| Bundler | Vite 7 |
| Styling | Tailwind CSS v3 |
| Routing | React Router v6 |
| Testing (backend) | xUnit + Moq |
| Testing (frontend) | Vitest 4 + React Testing Library |
| Database | PostgreSQL |
| Message broker | RabbitMQ |
| Reverse proxy | nginx |
| Containerisation | Docker + Docker Compose |
| CI/CD | GitLab CI |
| Code quality | SonarQube, Semgrep SAST, OWASP Dependency Check, Trivy |
| Monitoring | Prometheus + Grafana |
| Local email | Mailpit |

---

## Service Ports

| Service | Port | Notes |
|---|---|---|
| nginx (HTTP) | 80 | Redirects to HTTPS |
| nginx (HTTPS) | 443 | Main ingress |
| Users service | 9100 | Internal; also exposed directly |
| Expenses service | 9200 | Internal; also exposed directly |
| RabbitMQ AMQP | 5672 | |
| RabbitMQ Management UI | 15672 | |
| Grafana | 3000 | |
| Prometheus | 9090 | |
| Mailpit SMTP | 1025 | Local dev only |
| Mailpit Web UI | 8025 | Local dev only |
| Docker Updater API | 8989 | jobs-runner sidecar |
| MinIO | 9000 | Artifact storage |

---

## Current Status (v0.109.3)

The project is in active development. The users service (auth, registration, password management, outbox messaging) and the frontend auth flow are production-ready. The expenses backend is fully implemented — CRUD for expenses, categories, currencies, tags, families, dashboard analytics, currency rate management, and user configuration. The frontend expenses UI is fully shipped: paginated list with filters, add/edit/delete (modal), and a live dashboard with Recharts visualisations (spend over time, category donut, same-month year comparison, currency breakdown, recent expenses). A full admin panel is live: user management (enable/disable/roles), category management, currency management (add/edit/delete/defaults), rate management, and rate conflict resolution. Admin access is guarded by `APP_ADMIN` role and reflected in `isAdmin` JWT claim.

**Open issues:**
- `S-2` — Missing CSP and security headers (nginx fix pending)
