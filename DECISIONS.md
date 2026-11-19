# Engineering decisions and trade-offs

## Product concept
ATLAS uses a dark editorial visual identity, restrained lime accents, and a model directory instead of a generic CRUD dashboard. It is deliberately different in presentation; originality cannot be guaranteed.

## Architecture
Angular is the SPA; ASP.NET Core Minimal APIs act as a thin backend-for-frontend. A typed HttpClient isolates NHTSA transport. DTOs avoid coupling the UI to upstream field naming. No database, ORM, CQRS or repository layer: the app is read-only and NHTSA is the source of truth.

## Vehicle type semantics
NHTSA's type endpoint provides labels while its models endpoint accepts textual type filters. A documented alias mapping supports common types; unknown types are displayed but disabled instead of silently producing incorrect results. Vehicle type is optional so users can view all models for a make/year.

## Caching
IMemoryCache uses 12h for makes, 6h for types, and 3h for models. This is simple and cost-effective on one instance. Multiple replicas would each have their own cache; Redis could be introduced later. Cache misses can stampede under concurrency.

## Error handling
Invalid inputs return 400. Upstream HTTP failures return 502; upstream timeouts return 504; malformed responses return 502. No sensitive exception details are returned to clients.

## Frontend state
Angular RxJS `switchMap` cancels older model searches when a new search is triggered. Changing make clears type and result state. A local text filter lets users search the returned model list.

## Trade-offs / known gaps
- Type alias coverage is limited and should be validated against the full upstream taxonomy.
- Large manufacturer list is loaded once; filtering is local and selection is capped at 120 matches for rendering performance.
- The initial release prioritizes a small understandable architecture over a complex enterprise layering model.
- No persistence or user accounts are required.
- Public HTTP EC2 deployment is only a demo baseline; HTTPS should be added for production.
- Automated tests and CI should be expanded before a production release.

## Next steps
1. Add backend tests with a mocked HttpMessageHandler and Angular component tests.
2. Add request coalescing, resilience/backoff with jitter, and observability.
3. Improve make selection with a virtualized autocomplete and accessible keyboard interactions.
4. Add HTTPS, deployment automation, image pinning, and security scanning.

## AI use — fill in honestly before submission
AI was used to draft an initial architecture, scaffold code, and suggest documentation. I reviewed and adapted the generated code and will be able to explain and modify it in the follow-up interview.

**A real AI mistake and my fix:** TODO — replace this with an actual issue found during your review/testing, including the original suggestion, how you reproduced the issue, and the exact fix. Do not invent a mistake.

**Verification performed:** TODO — record actual commands, tests, and deployment results after running them.
