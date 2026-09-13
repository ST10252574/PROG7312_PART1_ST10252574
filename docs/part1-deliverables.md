# Smart-X Part 1 deliverables

This package covers the two Part 1 tasks:

- `part1-research.md` is the research paper and reference list.
- `SmartX-Dataflow-Diagram.svg` is the IoT-specific Figure 1 to place in the Word research paper.
- `../visual-studio/SmartX.Gateway.sln` is the Visual Studio-ready .NET 10 Minimal API.
- `../artifacts/smartx-gateway` is the React dashboard client.
- `../artifacts/api-server` is the local development API mirror used by the Replit preview. Its routes follow the same contract as the .NET API.

The simulated gateway has seeded devices and telemetry so the dashboard is usable on first run. Storage is intentionally in-memory for the assessment simulation and can be replaced with a database in Part 2.