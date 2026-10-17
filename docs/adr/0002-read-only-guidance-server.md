# A deterministic, read-only guidance server

Playbook provides coding guidance over MCP; the calling agent interprets the developer's goal, inspects the project, and makes changes. Pingu chose this boundary to keep the tool small and deterministic, rather than coupling guidance delivery to project analysis, migration execution, or an embedded AI service. Future capabilities should preserve this boundary unless the decision is explicitly revisited.
