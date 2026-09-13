# Smart-X IoT Gateway — PROG7312 Part 1

## My project

I built this Smart-X IoT Gateway as my PROG7312 Part 1 submission. The system represents a simulated IoT mesh used to register sensors, receive telemetry, validate readings, monitor gateway activity, and support proactive operator engagement.

For the main submission, I use the Visual Studio/.NET application. It includes the dashboard and API on the same server, so I do not need to run Node.js, pnpm, Vite, or a second terminal to demonstrate the solution.

## What I implemented

I implemented the following Part 1 features:

- A simulated Smart-X gateway with seeded sensor and telemetry data.
- Sensor registration using MAC address, location, and sensor category.
- Generic telemetry packet handling.
- Telemetry validation for supported readings and value ranges.
- Recent telemetry and gateway summary views.
- Gateway activity and mesh heartbeat information.
- Historical telemetry data across multiple dimensions.
- Recursive validation of a nested deployment topology.
- Media and configuration attachment handling for photos, configuration files, and hardware logs.
- Configurable proactive engagement strategies using threshold, disconnect, or anomaly triggers.
- A dashboard that shows the architecture pillars, including the features reserved for Part 2 and the final PoE stage.
- A research paper, references, deliverable notes, and a data-flow diagram.

## Technology used

- C# and ASP.NET Core Minimal API
- .NET 10
- Visual Studio 2022
- HTML, CSS, and JavaScript for the self-contained dashboard
- TypeScript/React source retained in `artifacts/smartx-gateway` for development and comparison
- In-memory seeded data for the Part 1 simulation

## How I run the submission

### Requirements

I use:

1. Visual Studio 2022 with the **ASP.NET and web development** workload.
2. The .NET 10 SDK.
3. A web browser.

### Starting the application

I open this solution:

```text
visual-studio/SmartX.Gateway.sln
```

Then I press **F5** in Visual Studio and open:

```text
http://localhost:5180
```

The dashboard and API are served by the same ASP.NET application. I do not need to start a second terminal for the main demonstration.

### Health check

I can confirm that the API is running by opening:

```text
http://localhost:5180/api/healthz
```

The expected response is:

```json
{"status":"ok"}
```

### Main API routes

The main routes I implemented include:

```text
GET  /api/healthz
GET  /api/summary
GET  /api/sensors
POST /api/sensors
GET  /api/telemetry/recent
POST /api/telemetry/validate
POST /api/telemetry
GET  /api/activity
GET  /api/uploads
POST /api/uploads
GET  /api/topology/validation
```

## How I demonstrate the features

1. I open the dashboard and show the operational overview.
2. I show the registered sensors and their status.
3. I submit or validate telemetry and show the accepted or rejected result.
4. I open the architecture section to explain the gateway, telemetry, validation, and engagement layers.
5. I open the attachments section to demonstrate media, configuration, or log upload handling.
6. I open the engagement section and change the active trigger or delivery channel.
7. I refresh the dashboard and show that the summary and recent activity are loaded from the API.

## Project structure

```text
docs/
  Part1-Research-SmartX.docx
  SmartX-Dataflow-Diagram.svg
  part1-deliverables.md
  part1-research.md

lib/
  api-client-react/
  api-spec/
  api-zod/

artifacts/
  api-server/
  smartx-gateway/

visual-studio/
  SmartX.Gateway.sln
  SmartX.Gateway.Api/
    Contracts.cs
    GatewayStore.cs
    Program.cs
    wwwroot/index.html
```

## Design decisions and limitations

I used an in-memory store because this is a Part 1 simulation and the assessment allows simulated gateway data. The data resets when the application stops. In Part 2, I would replace the in-memory store with persistent storage and connect the gateway to real devices.

The React client and local API mirror are included for development and comparison, but the reliable assessment run path is the Visual Studio solution. The main demonstration does not depend on Node.js or pnpm.

The project does not claim to implement the disabled Part 2 or final PoE features. Those features are represented in the architecture and clearly marked as future stages.

## Research documents

My research paper is in:

```text
docs/Part1-Research-SmartX.docx
```

The Markdown research version and data-flow diagram are also included:

```text
docs/part1-research.md
docs/SmartX-Dataflow-Diagram.svg
```

## My GitHub commit history

I created the GitHub history using small, meaningful commits. I did not use empty commits or repeatedly change whitespace just to increase the number. Each commit groups one part of my implementation or documentation.

The following is the commit plan I used:

1. `Create project README and Git ignore rules`
2. `Add workspace package configuration`
3. `Add workspace TypeScript configuration`
4. `Add OpenAPI contract`
5. `Add API client and validation packages`
6. `Add local API server package`
7. `Add React dashboard package`
8. `Add React dashboard application`
9. `Add Visual Studio solution`
10. `Add gateway domain contracts`
11. `Add seeded gateway store`
12. `Add gateway API endpoints`
13. `Configure Visual Studio launch settings`
14. `Add self-contained Smart-X dashboard`
15. `Add research paper Markdown`
16. `Add Part 1 deliverable notes`
17. `Add Smart-X data-flow diagram`
18. `Add research Word document`
19. `Document Visual Studio run path`
20. `Finalize Part 1 submission documentation`

## Important submission note

I submit the complete source code, the research documents, and the GitHub repository link. I do not submit only screenshots because the marker must be able to inspect and run the source code.
