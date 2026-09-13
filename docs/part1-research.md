# Part 1 Research: User Engagement and Dashboard Interactivity for Smart-X IoT Telemetry

## 1. Five strategies recommended for Smart-X

1. **Role-based, glanceable overview with progressive disclosure.** Show the small set of signals an operator needs first: device health, accepted-reading rate, active warnings, and last ingestion time. Let users open a sensor or reading for detail instead of making every screen equally dense.
2. **Real-time feedback plus historical comparison.** Pair live values with recent trends, previous readings, and an explicit data-quality state. A number becomes actionable when the operator can see whether it is stable, drifting, or anomalous.
3. **Configurable proactive alerts.** Allow operators to select the trigger (threshold, disconnect, or anomaly), choose a delivery channel, and receive a short message that states the condition and the next useful action. This changes the dashboard from a passive display into an early-warning system.
4. **User control and contextual customisation.** Support filters by location, sensor category, metric, and quality; allow users to configure thresholds and delivery preferences. Different users can therefore focus on the risks relevant to their role without changing the underlying telemetry.
5. **Trust-building data provenance and feedback.** Show validation results, timestamps, sensor identity, and clear explanations for warnings or rejected values. Provide an activity history so an operator can confirm what changed and whether the system responded.

## 2. Evaluation (approximately 500 words)

**Figure 1. Smart-X engagement and data-flow model**

```text
[Sensors and deployment evidence]
              |
              v
[Gateway validation + activity feedback] ---> [Configurable alerts]
              |
              v
[Glanceable dashboard + historical comparison]
```

IoT systems often fail at the human interface rather than at sensing. Smart-X users will abandon a dashboard if it is difficult to interpret, interrupts them with irrelevant messages, or produces values they cannot trust. The five strategies above are therefore designed to improve both engagement and operational decision-making.

First, a role-based overview reduces cognitive load. An operator should not need to scan every chart to discover that a device is offline or that the gateway is rejecting data. Showing registered devices, online devices, alerts, and accepted-rate at a glance supports quick orientation. Progressive disclosure keeps the initial view understandable while still preserving technical detail for investigation. This matters because the industrial-IoT user-experience literature treats usability as a multi-factor outcome influenced by the system, its stakeholders, and the way people interact with the provider (Rojas et al., 2023).

Second, real-time feedback is stronger when it is compared with history. A temperature of 23°C may be normal in one location and unusual in another; a power value is meaningful only against a threshold or previous pattern. Interactive filtering and short trend views let users compare the current situation with recent behaviour. This interface principle helps Smart-X users move from simply viewing telemetry to deciding what to do.

Third, proactive alerts should be configurable rather than noisy. Threshold, disconnect, and anomaly triggers reflect different operational needs. A message should explain what happened and point to the affected sensor, rather than merely displaying a red status. The user must also be able to choose dashboard, email, or webhook delivery. This makes alerts relevant and preserves attention, which is a scarce resource in monitoring environments.

Fourth, customisation gives users control. Filters by location and category help a facilities operator focus on a site while an engineer focuses on data quality. Configurable thresholds and delivery channels increase perceived usefulness and reduce notification fatigue. This aligns with the IoT adoption review by Marikyan et al. (2022), which identifies usefulness, ease of use, control, trust, and integration into existing practices as important acceptance factors.

Finally, trust must be visible in the interface. Smart-X should show sensor identity, timestamps, validation warnings, and rejected-value reasons. The activity feed gives operators a trace of what the gateway accepted and why. Samhale (2022) found a relationship between trust and engagement in IoT use, while Broughton (2025) found that trust in both the dashboard and the data is central to adoption. The Smart-X implementation therefore treats validation feedback as a user-engagement feature, not only a backend concern. Together, these strategies create a dashboard that is clear, actionable, configurable, and credible.

## References

Broughton, T. (2025). From data to improvement: Social mechanisms as a key to understanding dashboard adoption. *American Journal of Medical Quality, 40*(2), 31–37. https://doi.org/10.1097/JMQ.0000000000000225

Marikyan, D., Papagiannidis, S., & Alamanos, E. (2022). User adoption of intelligent environments: A review of technology adoption models, challenges, and prospects. *International Journal of Human–Computer Interaction*. https://doi.org/10.1080/10447318.2022.2118851

Rojas, R., et al. (2023). User experience key performance indicators for industrial IoT systems: A multivocal literature review. *Digital Business*. https://www.sciencedirect.com/science/article/pii/S2666954423000054

Samhale, K. (2022). The impact of trust in the internet of things for health on user engagement. *Digital Business, 2*(1), 100021. https://doi.org/10.1016/j.digbus.2022.100021

Aaqib, M., Ali, A., Chen, L., & Nibouche, O. (2023). IoT trust and reputation: A survey and taxonomy. *Journal of Cloud Computing, 12*, 42. https://doi.org/10.1186/s13677-023-00416-8
