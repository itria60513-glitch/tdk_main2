---
agent: speckit.implement
---

Follow .specify/memory/constitution.md as the authoritative implementation rule set.

When implementing or refactoring multi-method C# module files, add or maintain
meaningful #region grouping so the file is easy to trace by responsibility area.
Use semantic region names such as Constants, Construction, Public Operations,
Validation, Workflow, Helpers, or Event Handling. Do not use task IDs, step
numbers, or speckit task labels in region names.
