## CI/CD (GitLab)

Pipeline: `.gitlab-ci.yml` root + `infrastructure/configs/gitlab-ci-templates/`. Stages: `build→test→quality→docker→security→deploy`.
- SonarQube quality gate blocks on failure (`sonar.qualitygate.wait=true`)
- Security: Semgrep, OWASP Dep Check, Gitleaks, Trivy
- `sonar.exclusions`: `Migrations/`, `obj/`, `bin/`, `*.Designer.cs`, `*.g.cs`
- `sonar.coverage.exclusions`: `Models/`, `Options/`, `EO/`, `Requests/`, `Responses/`, `Program.cs`
- Coverage: OpenCover (.NET), V8 (frontend)
