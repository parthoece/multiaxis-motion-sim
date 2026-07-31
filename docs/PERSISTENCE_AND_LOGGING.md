# Persistence and Logging

<!-- DOC-NAV:START -->
[Home](../README.md) · [Docs](README.md) · [Start](GETTING_STARTED.md) · [Implement](IMPLEMENTATION_GUIDE.md) · [Architecture](ARCHITECTURE.md) · [Test](TEST_STRATEGY.md) · [Interview](INTERVIEW_PREP.md)
<!-- DOC-NAV:END -->


## Purpose

Operational records support workflow correctness, recovery, and traceability. Diagnostic events support troubleshooting.

The two channels have different failure policies:

- SQLite operational writes are required where the workflow depends on a durable record.
- JSONL diagnostics are best effort.
- A JSONL failure becomes an `OperationalWarning` and does not replace a valid primary machine fault.

## SQLite database

`SqliteOperationsStore` creates:

| Table | Content |
|---|---|
| `state_transitions` | Previous state, new state, reason, timestamp |
| `alarms` | Code, message, machine state, acknowledgement, rehome requirement |
| `cycles` | Recipe identity, part number, timing, overall result |
| `measurements` | Point, nominal, measured value, limits, result |

Cycle and measurement writes use one database transaction.

## Structured event log

`JsonLineEventLog` writes one JSON object per line:

```json
{
  "timestamp": "2026-07-28T12:00:00Z",
  "level": "Information",
  "eventName": "MachineStateTransition",
  "context": {
    "from": "Homing",
    "to": "Ready",
    "reason": "All axes homed."
  }
}
```

JSON Lines is append-friendly, readable without special tools, easy to parse, and suitable for diagnostic attachments.

Event logging is intentionally best effort. When the sink fails, `MachineControlContext` records an in-memory operational warning so the control workflow can continue when it is otherwise safe.

## Data-handling rules

- Do not store credentials or personal data.
- Store timestamps with offsets using ISO 8601.
- Preserve the recipe ID and version used for each cycle.
- Use transactions for logically complete records.
- Do not claim database records prove physical-machine performance.
- Remove `.runtime/` before committing.

## Inspecting SQLite

Using the SQLite command-line tool:

```bash
sqlite3 .runtime/operations.db
```

Example queries:

```sql
SELECT * FROM state_transitions ORDER BY id;
SELECT * FROM alarms ORDER BY raised_at DESC;
SELECT * FROM cycles ORDER BY started_at DESC;
SELECT * FROM measurements WHERE cycle_id = 'CYCLE-ID';
```

## Future changes

Schema changes should use explicit migrations before the project reaches a stable release. Migration design must include rollback or backup guidance.

---

<!-- DOC-FOOTER:START -->
[Documentation index](README.md) · [Back to top](#persistence-and-logging)
<!-- DOC-FOOTER:END -->
