# Engineering Standards

## Frontend

- Use feature-driven Angular modules
- Keep shared utilities in dedicated `shared` boundaries
- Prefer constructor-free dependency injection patterns where appropriate
- Keep components focused and testable
- Maintain strict TypeScript compiler settings

## Backend

- Keep API contracts explicit
- Isolate domain models from transport models
- Enforce tenant context centrally
- Keep configuration strongly typed

## Database

- Use explicit naming for primary keys and foreign keys
- Keep stored procedures versioned in source control
- Optimize high-volume dashboard queries with targeted indexing

## Quality

- Add tests with each feature slice
- Keep builds free of warnings where practical
- Document environment assumptions in-repo
