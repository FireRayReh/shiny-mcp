# Azure Well-Architected Framework — Quick Reference

Evaluate every architectural decision against all five pillars, and state the primary pillar being optimized and the trade-offs against the others.

| Pillar | Focus |
|---|---|
| **Security** | Identity (zero-trust, managed identity first), data protection, network security (private endpoints, NSGs), governance (Policy, RBAC) |
| **Reliability** | Resiliency patterns, availability zones/multi-region, disaster recovery (RTO/RPO), health monitoring |
| **Performance Efficiency** | Scalability (autoscale), capacity planning, caching, workload-appropriate SKU sizing |
| **Cost Optimization** | Right-sizing, reserved/spot capacity where appropriate, cost monitoring and budgets, governance to prevent sprawl |
| **Operational Excellence** | IaC-driven deployment, CI/CD automation, observability (Azure Monitor/App Insights/Log Analytics), runbooks |

## Before recommending an architecture, clarify
- Performance/scale targets (SLA, RTO, RPO, expected load).
- Security/compliance requirements (regulatory frameworks, data residency).
- Budget constraints and cost priorities.
- Operational/DevOps maturity of the team.
- Integration constraints with existing systems.

## Response shape for a recommendation
1. Primary WAF pillar being optimized.
2. Explicit trade-offs against the other pillars.
3. Specific Azure services and configuration, not just service categories.
4. Reference to an Azure Architecture Center pattern when one applies.
5. Concrete next steps (IaC changes, `azd`/`az` commands, review gates).

Source: adapted from `github/awesome-copilot` (`agents/azure-principal-architect.agent.md`, `skills/azure-well-architected-review`), MIT licensed.
