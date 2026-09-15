# Azure Developer CLI (azd) Safety Guardrails & Defaults

## Guardrails (always apply)
- Never commit `.azure/`, environment `.env` files, credentials, deployment outputs with secrets, or local Terraform state.
- Never put literal secrets in `azure.yaml`, IaC parameter files, hooks, or command arguments that get logged.
- Prefer managed identity + RBAC; fall back to Key Vault references and `azd env set-secret`, never `azd env set` for secret values.
- Before any command that creates/modifies/deletes resources, confirm target environment, subscription, tenant, region, and scope with the user.
- Treat an explicit user request to deploy/provision/destroy/configure a pipeline as approval for that named action only — never chain further mutating actions on an old approval.
- Do not swap Bicep for Terraform (or vice versa), or swap the hosting service, unless the user asks for that architectural change.

## Defaults
| Concern | Default |
|---|---|
| Project manifest | one `azure.yaml` at repo root |
| App code | `src/<service-name>` per deployable service |
| Infra | `infra/` with a thin `main.bicep`/`main.tf` entry point + reusable modules |
| IaC provider | Bicep, unless the repo/user already uses Terraform |
| Environments | separate named environments per stage (dev/test/staging/prod) |
| Secrets | managed identity/RBAC first, Key Vault references second |
| Automation | short, idempotent scripts under `scripts/azd`, invoked via azd hooks |
| CI auth | workload identity federation / OIDC |

## Validate before finishing a change
```
Bicep:      az bicep build --file infra/main.bicep     (bicep_build tool)
Terraform:  terraform fmt -check -recursive && terraform validate
AZD hooks:  azd hooks run <hook-name>
Packaging:  azd package                                 (azd_package tool)
What-if:    az deployment group what-if ...              (bicep_what_if tool)
```
Never claim a deployment succeeded unless it was actually run and verified.

Source: adapted from `github/awesome-copilot` (`skills/azure-developer-cli/SKILL.md`), MIT licensed.
