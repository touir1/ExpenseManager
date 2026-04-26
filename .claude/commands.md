## Commands

**Backend** (from `backend/expenses/` or `backend/users/`):
```bash
dotnet build --configuration Release
dotnet test --collect:"XPlat Code Coverage" --results-directory "./coverage" -- DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Format=opencover
dotnet test --filter FullyQualifiedName~ClassName.MethodName
dotnet run
```

**Frontend** (`frontend/dashboard`):
```bash
npm ci
npm run dev          # dev server
npm run build:prod   # production build
npm run typecheck    # type-check
npm test             # tests with coverage
```

**Infrastructure**:
```bash
cd infrastructure
./run-docker-compose-tools.bat  # RabbitMQ, Grafana, Prometheus
./run-docker-compose-apps.bat   # nginx, services
```
