# WorkVault — Interview-Worthy Feature Roadmap

Features chosen for **engineering depth**, not feature count. Each one forces a concept
interviewers actually probe on, and each should produce a 5-minute story:
*problem → options considered → trade-off chosen → what broke → how I fixed it.*

---

## Tier 1 — Highest interview value (distributed systems + real engineering)

### 1. Real email delivery via background job queue
- Integrate SendGrid / AWS SES — but **never send emails inline in the request**
- Push to a queue (RabbitMQ — already known from I2V) and process with a background
  worker (`IHostedService` or Hangfire)
- **Interview topics:** retry with exponential backoff, dead-letter queues,
  idempotency (worker crashes after sending but before marking sent?), outbox pattern
- Replaces the current console-log stub for invite / reset links

### 2. Refresh token reuse detection
- `ReplacedByToken` already exists on the entity — build the detection:
  if a rotated (already-replaced) token is presented, **revoke the entire token family**
- **Interview topics:** token theft scenarios, session security,
  trade-offs vs. short-lived access tokens

### 3. Attendance module (check-in / check-out)
- Deceptively hard: timezone handling (`Asia/Kolkata` fights already fought),
  overlapping shifts, late / half-day rules, monthly aggregation queries
- **Interview topics:** temporal data modeling, idempotent check-ins
  (double-tap protection), efficient date-range queries with proper indexes

### 4. Concurrency control — EmployeeCode race + optimistic locking
- Fix the known race: two simultaneous creates → same EmployeeCode
  (per-company DB sequence, or unique constraint + retry)
- Add optimistic concurrency (`xmin` / rowversion) to Employee updates —
  two HR users editing the same person
- **Interview topics:** "how do you handle concurrent updates?" — a guaranteed question

---

## Tier 2 — Strong architecture signals

### 5. Leave management with approval workflow
- Leave types, balances, accrual rules, multi-step approval
  (employee → manager → HR), state machine for request status
- **Interview topics:** workflow / state-machine design, business rules,
  balance calculation ordering (accrual vs. deduction)

### 6. Audit log (who changed what, when)
- Intercept in `SaveChangesAsync`: capture entity diffs to an `AuditLog` table;
  admin viewer UI
- **Interview topics:** EF Core change-tracking internals, DPDP / compliance,
  append-only tables, storage growth strategy

### 7. Org-tree queries (the deferred "Model C")
- Recursive CTEs for department subtrees and manager chains;
  circular-reference prevention on update
- **Interview topics:** recursive SQL, adjacency list vs. materialized path
  vs. closure table trade-offs

### 8. Caching layer with Redis
- Cache department lists / permission lookups; invalidate on write
- **Interview topics:** cache invalidation strategies, cache-aside pattern,
  stampede protection

---

## Tier 3 — Product polish that still teaches

| # | Feature | What it teaches |
|---|---------|-----------------|
| 9 | File uploads (photos, documents) | Presigned URLs to S3/Azure Blob, virus-scan hooks, image resizing |
| 10 | ID card with QR code | Original Phase-1 finisher; QR encodes a verification URL |
| 11 | Notifications (SignalR) | Real-time "leave approved" toasts — SignalR known from I2V |
| 12 | CSV bulk import of employees | Streaming parse, row-level validation report, partial success (ties to EPPlus experience) |
| 13 | Full-text search | PostgreSQL `tsvector` on employees, ranked results |

---

## The two meta-features that impress most

### 14. Deploy it properly
- Docker Compose → cloud (Render / Azure)
- CI/CD pipeline (GitHub Actions), health checks,
  structured logging (Serilog + Seq)
- **"It's live at X" beats any feature.**

### 15. Write tests
- Integration tests with **Testcontainers** (real Postgres in Docker)
- Especially: tenant-isolation tests — *prove* Company A cannot read Company B
- A test suite proving the security model is genuinely rare in portfolios

---

## Priority order for interviews

1. **Deploy + CI/CD** — proof it's real
2. **Email via queue + background worker** — distributed-systems story, reuses RabbitMQ knowledge
3. **Tenant-isolation integration tests** — unique differentiator, almost nobody does this
4. **Attendance or Leave module** — business complexity
5. **Reuse detection + optimistic concurrency** — security + concurrency depth

---

## Known TODOs (carried from build sessions)

- Wire password eye-toggle into password inputs (CSS exists)
- Update / delete department (+ self-reference & circular parent check)
- Employee detail / edit page (assign dept via UI)
- Designation + Manager dropdowns in employee-create (needs designations list endpoint)
- Dashboard real content + "complete your profile" nudge
- Rehire flow (new Employee record with `PreviousEmployeeId`, reuse User)
- Onboarding wizard (admin creates own departments — never auto-assign)
- Handle 401 / deleted-account gracefully on frontend
- Email verification for new registrations
- Rate limiting on login/register + account lockout
- CORS whitelist, CSP headers, forwarded-headers for proxy
- localStorage → httpOnly cookies before production
- DPDP compliance (privacy policy, consent, data export/delete)
