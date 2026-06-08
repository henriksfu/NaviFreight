# NaviFreight Architecture

## High-Level Design

NaviFreight is structured as a multi-tenant platform with clear separation between the presentation, application, data, and infrastructure layers.

## Major Components

### Frontend

- Angular application
- Modular feature boundaries
- Strict dependency injection
- Tenant-aware state and routing
- Jasmine and Karma test suite

### Backend

- C# service layer
- Tenant-aware API contracts
- Authentication and authorization boundary
- Fleet, yard, routing, and synchronization modules
- Real-time event and sync integration points
- Endpoint groups for dashboard, fleet, yards, routes, alerts, reports, and settings
- Centralized tenant-aware operations service abstraction

### Data

- SQL relational schema optimized for transactional logistics workloads
- Stored procedures for high-volume read and write operations
- Seed scripts for local development
- Operational entities for drivers, routes, docks, alerts, reports, users, and roles

### Infrastructure

- Microsoft Azure deployment target
- Environment-specific configuration
- App service, database, monitoring, and secret management placeholders

## Initial Service Boundaries

- `Identity`: tenant and user access
- `Fleet`: vehicle, driver, and trip operations
- `Yard`: occupancy, dock, gate, and trailer tracking
- `Routing`: dispatch and route execution metadata
- `Alerts`: operational exception queue and escalation signals
- `Reporting`: operational KPI snapshots and trend summaries
- `Settings`: tenant configuration and dispatch rules
- `Sync`: outbound and inbound real-time synchronization

## Multi-Tenancy Strategy

- Each request carries a tenant context
- Core tables include `TenantId`
- Service and query layers enforce tenant scoping
- Frontend API clients include tenant metadata in headers
