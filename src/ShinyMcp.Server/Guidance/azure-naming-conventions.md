# Azure Resource Naming Conventions (Cloud Adoption Framework)

General pattern: `<resource-type-abbr>-<workload>-<environment>-<region>-<instance>`
e.g. `rg-navigator-prod`, `app-navigator-prod-001`, `kv-navigator-prod-001`.

Rules:
- Lowercase letters and hyphens; no underscores unless the resource type requires them.
- Zero-padded numeric instance suffix (`001`, `002`, ...).
- Never encode subscription/tenant IDs or secrets in a name.
- No more than two consecutive hyphens; avoid reserved words/trademarks on public-endpoint resources.

Resources that do **not** allow hyphens (use concatenated lowercase alphanumerics):

| Resource | Abbr | Pattern | Example |
|---|---|---|---|
| Storage account | `st` | `st{workload}{env}{instance}` | `stnavigatorprod001` |
| Container registry | `cr` | `cr{workload}{env}{instance}` | `crnavigatorprod001` |
| Azure Data Explorer cluster | `dec` | `dec{workload}{env}` | `decnavigatorprod` |

Common abbreviations:

| Resource | Abbr | Resource | Abbr |
|---|---|---|---|
| Resource group | `rg` | Key vault | `kv` |
| Virtual network | `vnet` | Managed identity | `id` |
| Subnet | `snet` | App Service plan | `asp` |
| Network security group | `nsg` | Web app | `app` |
| Log Analytics workspace | `log` | Function app | `func` |
| Application Insights | `appi` | Static web app | `stapp` |
| AKS cluster | `aks` | Container apps | `ca` |
| Azure SQL server | `sql` | Container apps environment | `cae` |
| Azure SQL database | `sqldb` | Cosmos DB | `cosmos` |
| API Management | `apim` | Service Bus namespace | `sbns` |
| Event Hubs namespace | `evhns` | App Configuration store | `appcs` |
| Azure OpenAI | `oai` | AI Search | `srch` |

Full table (all resource categories, scopes, length limits, and full examples): see
`github/awesome-copilot` `instructions/azure-naming.instructions.md` (MIT licensed).
