# Dokumentation: Sonnensystem XR

> **Letzte Aktualisierung:** 2026-04-15

---

## Technisches Setup

| Bereich | Detail |
|---|---|
| Unity Version | 6000.3.10f1 |
| Render Pipeline | URP |
| Target Platform | Quest 3 Standalone (Android) |
| XR SDK | Meta XR SDK v85 |
| Input System | Unity Input System 1.18.0 |
| Testing (Windows) | Meta Quest Link (Play Mode im Headset) |
| Testing (Mac) | Build auf Headset |

---

## Aktuelle Features

### Sonnensystem-Simulation

- **Status:** Fertig
- **Beschreibung:** Alle 8 Planeten umkreisen die Sonne auf Kepler-Ellipsen. Größen und Abstände sind korrekt skaliert (Abstände und Planetengrößen unabhängig voneinander skalierbar).
- **Umsetzung:**
  - `PlanetData` ScriptableObjects mit echten astronomischen Daten (Durchmesser in km, semiMajorAxis in AU, Exzentrizität, Umlaufzeit in Tagen); seit 2026-04-13 zusätzlich `beschreibung` (string) und `fakten[]` (string-Array)
  - `PlanetBody.cs`: Kepler-Formel `r = a*(1-e²)/(1+e*cos(θ))`, angle-basiertes Trail-Sampling (frame-rate-unabhängig); Exzentrizität wird mit `manager.exzentrizitaetMultiplikator` multipliziert
  - `SolarSystemManager.cs`: Zentrale Skalierungsvariablen `distanceScale`, `planetSizeScale` und `exzentrizitaetMultiplikator` (Range 0–5, Default 1)
  - Sonnengröße dynamisch: `sunSizeRatio * 0.307 AU * distanceScale * 2` — kann Merkur-Orbit nie überlappen
- **Skalierung:** `distanceScale = 0.006` → Neptun bei ~18 cm Radius (Tischgröße)
- **Assets:** 9 PlanetData-Assets (Merkur, Venus, Erde, Mars, Jupiter, Saturn, Uranus, Neptun, Sonne)

### Trail-Effekt (Planetenspuren)

- **Status:** Fertig
- **Beschreibung:** Jeder Planet zieht eine leuchtende Spur in seiner Farbe. Spur deckt exakt einen vollen Orbit ab und fadet zum alten Ende aus.
- **Umsetzung:**
  - `LineRenderer` mit Ring-Buffer (angle-basiertes Sampling: 1 Punkt pro `360°/trailPoints` Grad)
  - HDR-Farben (Werte > 1.0) + URP Bloom für Leuchten
  - URP Unlit Shader: `_Cull=0` (beidseitig sichtbar), Alpha-Blending, kein ZWrite
  - Pre-allokierter `renderBuffer` + `SetPositions()` Batch-API für Performance

### Planet Shader & SunPasser

- **Status:** In Entwicklung
- **Beschreibung:** Eigene Planeten-Materialien mit dynamischer Beleuchtung durch die Sonne und Atmosphären-Effekt. Sonnenposition wird per Script in Echtzeit an alle Planeten-Materialien übergeben.
- **Umsetzung:** `SunPasser.cs` + `M_Planet.shadergraph`
  - `[ExecuteAlways]` — läuft im Editor und im Play Mode (Editor-Preview möglich)
  - Setzt `_SunPosition` Shader-Property direkt auf allen zugewiesenen Material-Instanzen
  - Kein SRP-Batching-Bruch, da auf `shared Material`-Instanzen geschrieben wird
- **Shader-Features (M_Planet.shadergraph):**
  - Unlit-Shader mit eigener Lichtberechnung via `_SunPosition`
  - 3 animierte Wolken-Layer (`_Cloud_01/02/03`) mit individuellen Geschwindigkeiten (gegenläufig möglich)
  - Fresnel-basierter Atmosphären-Effekt: Farbverlauf zwischen `_AtmosphereInnerColor` und `_AtmosphereOuterColor` via Lerp
  - `_AtmosphereBias` (Power-Node) steuert die Kurve des Übergangs (< 1 = Inner breiter, > 1 = Outer nur am Rand)
  - Atmosphäre wird mit Sun-Mask multipliziert → nur auf der sonnenzugewandten Seite sichtbar
  - Atmosphäre wird per `Add` auf Planet-Color gelegt (kein `Multiply` → keine Abdunklung der Textur)
- **Anmerkung:** Shader-Durchbruch war ein wichtiger Meilenstein (Commit: "PLANET MATERIAL GEHT ENDLICH!!!!")

### Cloud-Animation (Erde)

- **Status:** Fertig
- **Beschreibung:** Rotierende Wolkenschicht auf dem Erde-Prefab.
- **Umsetzung:** `CloudBehaviour.cs`
  - Rotiert Wolken-Kindsobjekt um Y-Achse mit `cloudSpeed * Time.deltaTime`
  - `sizePercentage` (0–3%) steuert Skalierung relativ zum Planeten
  - `OnValidate()` für Live-Preview im Editor

### State Machine (Spielzustände)

- **Status:** Scripts fertig, Unity-Editor-Setup ausstehend
- **Beschreibung:** 6 Zustände: `START → AUSWAHL → PLACEMENT → SONNENSYSTEM / PLANET_SCHWEBEND / PLANET_IMMERSIV`
- **Umsetzung:** `GameManager.cs` (Singleton)
  - `START → AUSWAHL`: Automatisch nach 1 Sekunde via Coroutine
  - `AUSWAHL`: World-Space-Menü vor dem Spieler, Hauptmenü + Planeten-Untermenü
  - `PLACEMENT`: AR-Platzierung; PlatzierungManager übernimmt, Sonnensystem-Spawn nach Bestätigung
  - `SONNENSYSTEM`: Sonnensystem steht; SonnensystemPanel mit Parametern sichtbar
  - `PLANET_SCHWEBEND`: Einzelplanet 1,5 m vor Spieler; PlanetDetailPanel + InfoPunkte aktiv
  - `PLANET_IMMERSIV`: Planet in echter Größenrelation (Passthrough aus, Weltraum-Skybox); ImmersivPanel sichtbar
  - Menü-Button (`OVRInput.Button.Start`): von überall zurück zu `AUSWAHL`
  - Passthrough-Toggle: `PassthroughEinschalten()` / `PassthroughAusschalten()` über `OVRPassthroughLayer`

### AR-Platzierung (Sonnensystem)

- **Status:** Script fertig, Unity-Editor-Setup ausstehend
- **Beschreibung:** Im `PLACEMENT`-Zustand folgt ein procedural erzeugter Ring-Indikator dem Controller-Ray. Trigger-Button platziert das Sonnensystem endgültig im Raum.
- **Umsetzung:** `PlatzierungManager.cs`
  - Cyan `LineRenderer`-Ring als Vorschau-Indikator (kein Vorschau-Prefab nötig)
  - Rechter Thumbstick Y: Abstand anpassen (Bereich `minAbstand`–`maxAbstand`, Default 0.5–3 m)
  - `OVRInput.GetDown(Button.PrimaryIndexTrigger)` → `PlatzierungBestaetigen()` → Sonnensystem spawnt, wechselt zu `SONNENSYSTEM`
  - Ruft `GameManager.Instance.SonnensystemPlatziert(sonnensystem)` auf

### Sonnensystem-Parameter-UI

- **Status:** Script fertig, Unity-Editor-Setup ausstehend
- **Beschreibung:** World-Space Panel mit 4 Slidern zum Live-Anpassen der Simulation.
- **Umsetzung:** `SonnensystemUI.cs` auf dem `SonnensystemPanel`-Canvas
  - **Bahnabstände:** 0.002–0.025 → `SolarSystemManager.distanceScale`
  - **Planetengrößen:** 0.0005–0.01 → `SolarSystemManager.planetSizeScale`
  - **Geschwindigkeit:** 0–100 Tage/s → `SolarSystemManager.timeScale`
  - **Exzentrizität:** 0–5 → `SolarSystemManager.exzentrizitaetMultiplikator`
  - Reset-Button setzt alle Werte auf Defaults zurück
  - `AddListener` in `OnEnable`, `RemoveListener` in `OnDisable`

### Planeten-Detailansicht (schwebend)

- **Status:** Script fertig, Unity-Editor-Setup ausstehend
- **Beschreibung:** Ausgewählter Planet schwebt 1,5 m vor dem Spieler. Daten-Panel erscheint rechts daneben.
- **Umsetzung:**
  - `PlanetDetailUI.cs` auf dem `PlanetDetailPanel`-Canvas
  - Befüllt: Name, Beschreibung, Fakten (Stichpunkte mit „•"), technische Daten (Durchmesser, Abstand, Umlaufzeit, Exzentrizität)
  - Panel-Positionierung: rechts vom Planet via `Vector3.Cross(Vector3.up, richtungZurKamera)`, Größe passt sich via `Renderer.bounds.extents.x` an
  - `Quaternion.LookRotation(-richtungZurKamera)` — entspricht Canvas-Konvention (Inhalt auf -Z-Face)
  - `LazyFollowUI.cs` kann auf beliebige World-Space-Canvas gelegt werden: folgt Spieler träge auf Augenhöhe (horizontale Blickrichtung, kein vertikales Kippen)

### Info-Punkte & Info-Panel

- **Status:** Scripts fertig, Unity-Editor-Setup ausstehend
- **Beschreibung:** Leuchtende Hotspot-Punkte auf Planeten-Prefabs. Controller-Ray hover → Hervorheben. Trigger-Button → Info-Panel einblenden.
- **Umsetzung:**
  - `InfoPunkt.cs` (`[RequireComponent(SphereCollider)]`):
    - Trigger-Collider für Raycast-Erkennung
    - Erstellt leuchtendes HDR-Kügelchen (40 % des `kolliderRadius`) als visuellen Marker
    - `Hervorheben(bool)`: Kugel skaliert auf 160 % (relativ zur gespeicherten `_basisSkalierung`)
  - `InfoPanel.cs` (Singleton):
    - **WICHTIG:** Script auf persistentes leeres GameObject „InfoSystem" legen, NICHT auf den Canvas
    - `public GameObject panelCanvas` — Referenz auf den eigentlichen World-Space-Canvas
    - `Physics.Raycast(..., QueryTriggerInteraction.Collide)` — trifft auch Trigger-Collider
    - `Debug.DrawRay` (cyan) für Scene-View-Sichtbarkeit beim Debuggen
    - Panel-Rotation: `Quaternion.LookRotation(-richtungZurKamera)` — Canvas-Konvention

### Immersive Planeten-Ansicht

- **Status:** Script fertig, Unity-Editor-Setup ausstehend
- **Beschreibung:** Planet wird in echter Größenrelation zur Person gezeigt (Passthrough aus, Weltraum). Referenz: Erde = 15 m VR-Radius.
- **Umsetzung:** `ImmersivePlanetView.cs`
  - Skalierungsformel: `vrRadius = (planet.diameter / 12756f) * 15f`
  - Planet wird `vrRadius + abstandVonOberflaeche` Meter vor der Kamera positioniert
  - Originalposition und -skalierung werden gespeichert und bei Verlassen des Zustands wiederhergestellt
  - `massstabLabel` zeigt „1 m ≈ X km" als TMP_Text

### UI-System (Menü)

- **Status:** Fertig (Scripts), Unity-Editor-Setup teils ausstehend
- **Beschreibung:** World-Space-Canvas mit Hauptmenü (Sonnensystem / Planeten) und Planeten-Untermenü.
- **Umsetzung:** `GameManager.cs` + `PlanetButton.cs`
  - Canvas wird beim Wechsel zu AUSWAHL direkt vor der Kamera positioniert
  - `hauptPanel`: Hauptmenü (Sonnensystem-Button → PLACEMENT, Planeten-Button → Untermenü)
  - `planetenPanel`: Untermenü mit je einem Button pro Planet + Zurück-Button
  - `PlanetButton.cs`: Verbindet `PlanetData` mit zugehörigem Prefab; `OnKlicken()` → `GameManager.PlanetAuswaehlen()`

### XR-Input

- **Status:** Fertig
- **Beschreibung:** Controller-Eingaben für Navigation und Interaktion.
- **Umsetzung:** `GameManager.cs` + `InfoPanel.cs`
  - `OVRInput.Button.Start` → Menü-Toggle (überall zurück zu AUSWAHL)
  - `OVRInput.Button.PrimaryIndexTrigger` → Platzierung bestätigen / InfoPunkt öffnen / Panel schließen
  - Rechter Thumbstick Y → Abstand beim Platzieren anpassen

### Desktop-Testing Tools

- **Status:** Fertig (nicht für Quest-Build relevant)
- **Beschreibung:** Scripts für Desktop-Entwicklung ohne Headset.
- **Umsetzung:**
  - `DesktopDebugCamera.cs`: Free-look Kamera (WASD + Maus, Shift = Boost, ESC = Cursor-Unlock)
  - `DesktopPlacement.cs`: Platziert SolarSystem-Prefab via Rechtsklick-Raycast
  - `PlacementTester.cs`: Oculus Interaction SDK — spawnt Prefab an RayInteractor-Kollisionspunkt

---

## Wichtige technische Konventionen

| Konvention | Detail |
|---|---|
| **World-Space Canvas Rotation** | Canvas-Inhalt liegt auf der lokalen **-Z-Fläche**. Alle Panel-Positionierungen nutzen `Quaternion.LookRotation(-richtungZurKamera)` (Richtung VON Kamera WEG). Nie ohne Minus! |
| **InfoPanel-Architektur** | `InfoPanel.cs` liegt auf einem persistenten leeren „InfoSystem"-GameObject, NICHT auf dem Canvas. Sonst stoppt `Update()` wenn der Canvas via `SetActive(false)` ausgeblendet wird. |
| **Trigger-Raycast** | `Physics.Raycast` ignoriert Trigger-Collider standardmäßig. `QueryTriggerInteraction.Collide` als letzten Parameter übergeben. |
| **Canvas-Konvention** | `CanvasVorSpielerPositionieren()` in GameManager.cs ist die Referenz-Implementierung für korrekte Panel-Positionierung. |

---

## Aktuelle Entscheidungen

| Datum | Entscheidung | Begründung |
|---|---|---|
| 2026-04-12 | Meta Building Blocks bevorzugt | Einfacher, schneller, weniger eigener Code |
| 2026-04-12 | Code auf einfachem Level halten | Verständlichkeit und eigene Bearbeitbarkeit |
| 2026-04-12 | Code-Kommentare auf Deutsch | Muttersprache, einfacher für Doku |
| 2026-04-12 | World-Space Canvas statt Screen-Space | In XR gibt es keinen echten Screen — World-Space wird im 3D-Raum verankert |
| 2026-04-12 | PlanetData als ScriptableObject | Trennung von Daten und Logik; einfach erweiterbar ohne Code-Änderung |
| 2026-04-12 | SunPasser mit `[ExecuteAlways]` | Editor-Preview der Beleuchtung ohne Play Mode |
| 2026-04-12 | `OVRInput.Button.Start` für Menü | Standard Quest-Menü-Button; unterstützt Controller und Wrist-Menu |
| 2026-04-13 | AR-Platzierung ohne Oberflächen-Erkennung | Sonnensystem soll überall im Raum platzierbar sein (nicht surface-locked) |
| 2026-04-13 | Ring-Indikator statt halbtransparentem Vorschau-Prefab | Einfacher, kein Duplikat-Prefab nötig |
| 2026-04-13 | InfoPanel auf separatem InfoSystem-Objekt | Canvas-`SetActive(false)` würde Update()-Loop töten; Singleton braucht persistentes Elternobjekt |
| 2026-04-13 | Erde = 15 m VR-Radius als Immersiv-Referenz | Person steht „an der Oberfläche"; Planet füllt den Horizont sichtbar |

---

## Bekannte Probleme / Offene Editor-Aufgaben

| Problem | Status |
|---|---|
| SonnensystemPanel-Canvas im Unity-Editor erstellen (4 Slider + SonnensystemUI.cs) | Offen |
| PlanetDetailPanel-Canvas im Unity-Editor erstellen (PlanetDetailUI.cs verdrahten) | Offen |
| ImmersivPanel-Canvas im Unity-Editor erstellen (TMP_Text Maßstab + Zurück-Button) | Offen |
| „InfoSystem" leeres GameObject erstellen, InfoPanel.cs drauf, panelCanvas-Feld befüllen | Offen |
| InfoPanel-Canvas als World-Space-Canvas erstellen (TitelText, InhaltText, Schließen-Button) | Offen |
| 6 fehlende Planeten-Prefabs erstellen (Merkur, Venus, Mars, Saturn, Uranus, Neptun) | Offen |
| InfoPunkt-Empties auf Planeten-Prefabs verteilen (3–4 pro Planet) | Offen |
| PlanetData-Assets mit `beschreibung` und `fakten[]` befüllen | Offen |
| OVRPassthroughLayer-Referenz im GameManager-Inspector verdrahten | Offen |
| `StartPhase.cs` und `SpielerBewegung.cs` im Projekt aufräumen (nicht integrierte Altlasten) | Offen |
| `OVRInteractionComprehensive` Ray nur bei Interactables sichtbar | Bekannt |
