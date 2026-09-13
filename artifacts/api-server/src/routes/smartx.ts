import { Router, type IRouter } from "express";
import {
  CreateSensorBody,
  CreateSensorResponse,
  GetEngagementStrategyResponse,
  GetGatewaySummaryResponse,
  IngestTelemetryBody,
  IngestTelemetryResponse,
  ListActivityResponse,
  ListSensorsResponse,
  ListTelemetryQueryParams,
  ListTelemetryResponse,
  UpdateEngagementStrategyBody,
  UpdateEngagementStrategyResponse,
  ValidateTelemetryBody,
  ValidateTelemetryResponse,
} from "@workspace/api-zod";

const router: IRouter = Router();

const now = () => new Date().toISOString();

type SensorRecord = {
  id: string;
  macAddress: string;
  location: string;
  category: "environmental" | "power" | "actuator";
  status: "online" | "warning" | "offline";
  lastSeen: string;
  readingsToday: number;
};

type TelemetryRecord = {
  id: string;
  sensorId: string;
  sensorLabel: string;
  metric: string;
  value: number;
  unit: string;
  recordedAt: string;
  quality: "valid" | "warning" | "rejected";
};

type ActivityRecord = {
  id: string;
  title: string;
  detail: string;
  tone: "success" | "warning" | "info";
  occurredAt: string;
};

type EngagementRecord = {
  id: string;
  name: string;
  description: string;
  enabled: boolean;
  trigger: "threshold" | "disconnect" | "anomaly";
  delivery: "dashboard" | "email" | "webhook";
  updatedAt: string;
};

let sensors: SensorRecord[] = [
  {
    id: "sensor-001",
    macAddress: "A4:C1:38:7F:20:11",
    location: "Cape Town / Workshop",
    category: "environmental" as const,
    status: "online" as const,
    lastSeen: now(),
    readingsToday: 148,
  },
  {
    id: "sensor-002",
    macAddress: "B8:27:EB:44:19:02",
    location: "Johannesburg / Rooftop",
    category: "power" as const,
    status: "warning" as const,
    lastSeen: new Date(Date.now() - 1000 * 60 * 7).toISOString(),
    readingsToday: 96,
  },
  {
    id: "sensor-003",
    macAddress: "D0:39:72:AA:03:8C",
    location: "Durban / Pump Room",
    category: "actuator" as const,
    status: "online" as const,
    lastSeen: new Date(Date.now() - 1000 * 60 * 2).toISOString(),
    readingsToday: 83,
  },
  {
    id: "sensor-004",
    macAddress: "58:BF:25:9E:77:41",
    location: "Pretoria / Loading Bay",
    category: "environmental" as const,
    status: "offline" as const,
    lastSeen: new Date(Date.now() - 1000 * 60 * 48).toISOString(),
    readingsToday: 12,
  },
];

let telemetry: TelemetryRecord[] = [
  {
    id: "reading-001",
    sensorId: "sensor-001",
    sensorLabel: "Cape Town / Workshop",
    metric: "Temperature",
    value: 23.4,
    unit: "°C",
    recordedAt: new Date(Date.now() - 1000 * 60 * 2).toISOString(),
    quality: "valid" as const,
  },
  {
    id: "reading-002",
    sensorId: "sensor-002",
    sensorLabel: "Johannesburg / Rooftop",
    metric: "Power draw",
    value: 4.82,
    unit: "kW",
    recordedAt: new Date(Date.now() - 1000 * 60 * 4).toISOString(),
    quality: "warning" as const,
  },
  {
    id: "reading-003",
    sensorId: "sensor-003",
    sensorLabel: "Durban / Pump Room",
    metric: "Flow rate",
    value: 18.7,
    unit: "L/min",
    recordedAt: new Date(Date.now() - 1000 * 60 * 5).toISOString(),
    quality: "valid" as const,
  },
  {
    id: "reading-004",
    sensorId: "sensor-001",
    sensorLabel: "Cape Town / Workshop",
    metric: "Humidity",
    value: 51,
    unit: "%",
    recordedAt: new Date(Date.now() - 1000 * 60 * 8).toISOString(),
    quality: "valid" as const,
  },
];

let activity: ActivityRecord[] = [
  {
    id: "activity-001",
    title: "Mesh heartbeat restored",
    detail: "Durban / Pump Room is responding within the expected interval.",
    tone: "success" as const,
    occurredAt: new Date(Date.now() - 1000 * 60 * 3).toISOString(),
  },
  {
    id: "activity-002",
    title: "Power threshold crossed",
    detail: "Johannesburg / Rooftop reported 4.82 kW against a 4.5 kW warning threshold.",
    tone: "warning" as const,
    occurredAt: new Date(Date.now() - 1000 * 60 * 7).toISOString(),
  },
  {
    id: "activity-003",
    title: "Telemetry batch accepted",
    detail: "12 readings passed schema and range validation.",
    tone: "info" as const,
    occurredAt: new Date(Date.now() - 1000 * 60 * 12).toISOString(),
  },
];

let engagementStrategy: EngagementRecord = {
  id: "strategy-001",
  name: "Proactive operator alerts",
  description:
    "Surface threshold crossings, device disconnects, and validation warnings in the dashboard so operators can respond before data quality degrades.",
  enabled: true,
  trigger: "threshold" as const,
  delivery: "dashboard" as const,
  updatedAt: now(),
};

function validateTelemetry(input: {
  sensorId: string;
  metric: string;
  value: number;
  unit: string;
  recordedAt?: string;
}) {
  const errors: string[] = [];
  const warnings: string[] = [];
  const sensor = sensors.find((item) => item.id === input.sensorId);

  if (!sensor) errors.push("The sensor ID is not registered.");
  if (!Number.isFinite(input.value)) errors.push("The reading value must be numeric.");
  if (input.metric.trim().length < 2) errors.push("The metric name is too short.");
  if (input.unit.trim().length < 1) errors.push("The unit is required.");
  if (input.metric.toLowerCase().includes("temperature") && (input.value < -40 || input.value > 85)) {
    warnings.push("Temperature is outside the expected operating band of -40°C to 85°C.");
  }
  if (input.metric.toLowerCase().includes("power") && input.value > 4.5) {
    warnings.push("Power draw is above the configured warning threshold.");
  }
  if (input.recordedAt && Number.isNaN(Date.parse(input.recordedAt))) {
    errors.push("The recordedAt value must be an ISO timestamp.");
  }

  return { valid: errors.length === 0, errors, warnings };
}

router.get("/summary", (_req, res): void => {
  const accepted = telemetry.filter((item) => item.quality !== "rejected").length;
  const acceptedRate = telemetry.length === 0 ? 100 : Math.round((accepted / telemetry.length) * 100);
  const data = GetGatewaySummaryResponse.parse({
    registeredSensors: sensors.length,
    onlineSensors: sensors.filter((item) => item.status === "online").length,
    readingsToday: sensors.reduce((total, item) => total + item.readingsToday, 0),
    acceptedRate,
    alerts: activity.filter((item) => item.tone === "warning").length,
    lastIngestedAt: telemetry[0]?.recordedAt ?? now(),
  });
  res.json(data);
});

router.get("/sensors", (_req, res): void => {
  res.json(ListSensorsResponse.parse(sensors));
});

router.post("/sensors", (req, res): void => {
  const parsed = CreateSensorBody.safeParse(req.body);
  if (!parsed.success) {
    req.log.warn({ error: parsed.error.message }, "Invalid sensor registration");
    res.status(400).json({ error: parsed.error.message });
    return;
  }

  const sensor = CreateSensorResponse.parse({
    id: `sensor-${String(sensors.length + 1).padStart(3, "0")}`,
    ...parsed.data,
    status: "online",
    lastSeen: now(),
    readingsToday: 0,
  });
  sensors = [sensor, ...sensors];
  activity = [
    {
      id: `activity-${Date.now()}`,
      title: "Sensor registered",
      detail: `${sensor.location} is now available to the gateway.`,
      tone: "success" as const,
      occurredAt: now(),
    },
    ...activity,
  ];
  res.status(201).json(sensor);
});

router.get("/telemetry", (req, res): void => {
  const parsed = ListTelemetryQueryParams.safeParse(req.query);
  if (!parsed.success) {
    res.status(400).json({ error: parsed.error.message });
    return;
  }
  res.json(ListTelemetryResponse.parse(telemetry.slice(0, parsed.data.limit ?? 20)));
});

router.post("/validation", (req, res): void => {
  const parsed = ValidateTelemetryBody.safeParse(req.body);
  if (!parsed.success) {
    res.status(400).json({ error: parsed.error.message });
    return;
  }
  res.json(ValidateTelemetryResponse.parse(validateTelemetry(parsed.data)));
});

router.post("/telemetry", (req, res): void => {
  const parsed = IngestTelemetryBody.safeParse(req.body);
  if (!parsed.success) {
    res.status(400).json({ error: parsed.error.message });
    return;
  }

  const validation = validateTelemetry(parsed.data);
  if (!validation.valid) {
    res.status(202).json(
      IngestTelemetryResponse.parse({
        accepted: false,
        reading: null,
        validation,
      }),
    );
    return;
  }

  const sensor = sensors.find((item) => item.id === parsed.data.sensorId);
  const reading = {
    id: `reading-${Date.now()}`,
    sensorId: parsed.data.sensorId,
    sensorLabel: sensor?.location ?? "Unknown sensor",
    metric: parsed.data.metric,
    value: parsed.data.value,
    unit: parsed.data.unit,
    recordedAt: parsed.data.recordedAt ?? now(),
    quality: validation.warnings.length > 0 ? ("warning" as const) : ("valid" as const),
  };
  telemetry = [reading, ...telemetry];
  if (sensor) {
    sensor.lastSeen = reading.recordedAt;
    sensor.readingsToday += 1;
    if (sensor.status === "offline") sensor.status = "online";
  }
  activity = [
    {
      id: `activity-${Date.now()}`,
      title: "Telemetry accepted",
      detail: `${reading.metric} from ${reading.sensorLabel} passed gateway validation.`,
      tone: validation.warnings.length > 0 ? ("warning" as const) : ("success" as const),
      occurredAt: now(),
    },
    ...activity,
  ];
  res.status(202).json(IngestTelemetryResponse.parse({ accepted: true, reading, validation }));
});

router.get("/activity", (_req, res): void => {
  res.json(ListActivityResponse.parse(activity.slice(0, 10)));
});

router.get("/engagement", (_req, res): void => {
  res.json(GetEngagementStrategyResponse.parse(engagementStrategy));
});

router.put("/engagement", (req, res): void => {
  const parsed = UpdateEngagementStrategyBody.safeParse(req.body);
  if (!parsed.success) {
    res.status(400).json({ error: parsed.error.message });
    return;
  }
  engagementStrategy = {
    ...engagementStrategy,
    ...parsed.data,
    updatedAt: now(),
  };
  res.json(UpdateEngagementStrategyResponse.parse(engagementStrategy));
});

export default router;