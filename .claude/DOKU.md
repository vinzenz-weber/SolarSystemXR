# Dokumentation: Sonnensystem XR

> **Letzte Aktualisierung:** 2026-04-21

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
  - `PLANET_IMMERSIV`: Raumstation-Szene lädt additiv; Spieler wird in die Station teleportiert; Planet erscheint draußen in echter Größe
  - Menü-Button (`OVRInput.Button.Start`):
    - In `PLANET_IMMERSIV`: zeigt nur das Raumstation-Mini-Menü ("Verlassen"), KEIN Hauptmenü
    - In allen anderen aktiven Zuständen: zurück zu `AUSWAHL`
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

### GrabHandle (Planeten/Canvas verschieben)

- **Status:** Script + Prefab fertig; Editor-Verdrahtung für Canvas-Usecases ausstehend
- **Beschreibung:** „Pille" unter einem grabbaren Objekt (Planet oder Canvas). Controller-Ray + Trigger → Objekt folgt dem Controller durch den Raum. Trigger loslassen → Objekt bleibt stehen.
- **Umsetzung:** Reines Meta Interaction SDK + dünner Glue-Layer
  - Prefab `Assets/Prefabs/GrabHandle.prefab` enthält die gesamte Interaktions-Kette: `CapsuleCollider` → `ColliderSurface` → `RayInteractable` → `Grabbable` (mit `GrabFreeTransformer` in `OneGrabTransformer`- und `TwoGrabTransformer`-Slot)
  - Visual: `GrabHandle_Visual` (Capsule-Mesh) mit `MaterialPropertyBlockEditor` + `InteractableColorVisual` (4 Color-States: Normal/Hover/Select/Disabled)
  - `GrabHandleGlue.cs`: nur projektspezifische Logik — LazyFollowUI-Pause beim Greifen, optionales Kamera-Ausrichten beim Loslassen. Wird via `InteractableUnityEventWrapper` (`WhenSelect`/`WhenUnselect`) verdrahtet.
  - **Planeten-Usecase:** `ArPlanetInstanz` instanziert das Prefab unter dem Wrapper und ruft `Grabbable.InjectOptionalTargetTransform(wrapper)` — die Pille wird gegriffen, der Wrapper (= der ganze Planet) wird bewegt
  - **Canvas-Usecase:** Prefab als Kind des Canvas platzieren, `Grabbable._targetTransform` auf den Canvas-Root setzen, `GrabHandleGlue`-Felder (`zielTransform`, `lazyFollow`, `ausrichtenBeimLoslassen=true`) im Inspector füllen
- **Ray-Visual:** `RayInteractorRayVisual` aus `OVRInteractionComprehensive` — Farbe bei Hover/Select via `_hoverColor`/`_selectColor`, Sichtbarkeit bei leerem Raum via `_hideWhenNoInteractable`

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

### Raumstation & Immersive Planeten-Ansicht

- **Status:** Scripts fertig, Unity-Editor-Setup teilweise ausstehend (Mini-Menü Canvas, SpawnPunkte)
- **Beschreibung:** Beim Öffnen des Immersive Mode wird die Raumstation-Szene additiv geladen. Der Spieler wird in die Station teleportiert. Der ausgewählte Planet erscheint draußen in echter VR-Größe. Im Inneren der Station simuliert eine Trigger-Zone die Umgebungsbedingungen des gewählten Planeten (Schwerkraft, Atmosphäre, Wind).
- **Umsetzung:**
  - `GameManager.cs`: lädt `Raumstation.unity` additiv via `SceneManager.LoadSceneAsync(..., Additive)`, entlädt beim Verlassen
  - `RaumstationController.cs` (in Raumstation.unity): Singleton; teleportiert Spieler zum SpawnPunkt; spawnt Planeten-Prefab in echter VR-Größe am PlanetSpawnPunkt; verwaltet Mini-Menü-Toggle
  - Skalierungsformel (identisch zu `ImmersivePlanetView`): `vrRadius = (planet.diameter / 12756f) * erdeReferenzRadius` (Default: Erde = 10 m VR-Radius)
  - `ImmersivePlanetView.cs`: bleibt erhalten für Fallback, wird im Raumstation-Modus von GameManager deaktiviert
  - Teleport: nur Position, keine Rotation — OVR-Tracking-Basis bleibt erhalten
  - Spieler-Ursprungsposition wird in `Start()` gespeichert, in `OnDestroy()` wiederhergestellt
- **Mini-Menü:** WorldSpace Canvas in Raumstation.unity mit `LazyFollowUI`; ein Button „Immersive Mode verlassen" ruft `RaumstationController.VerlassenKlicken()` → `GameManager.ZurueckZuSchwebend()` auf

### Planet-Umgebungszone

- **Status:** Script fertig, Raumstation.unity-Setup erledigt (Zone vorhanden), Partikel-Tuning ausstehend
- **Beschreibung:** Begehbarer Bereich (5 × 3 × 20 m) in der Raumstation. Beim Betreten herrschen die Umgebungsbedingungen des aktuell ausgewählten Planeten: Schwerkraft, Atmosphären-Partikel (Nebel/Wolken), Wind und Turbulenzen.
- **Umsetzung:** `PlanetUmgebungsZone.cs` (`[RequireComponent(BoxCollider)]`)
  - Liest `GameManager.Instance.ausgewaehlterPlanet` automatisch beim Betreten
  - `Physics.gravity` + `SpielerBewegung.schwerkraft` werden auf Planetenwert gesetzt
  - Atmosphäre: Partikel-Emissionsrate proportional zu `planet.nebelDichte` (0–1), Farbe aus `planet.atmosphaereFarbe`
  - Wind: `ParticleSystem.NoiseModule` mit `planet.windTurbulenz`-Stärke, Geschwindigkeit aus `planet.windStaerke`
  - Beim Verlassen: alles zurücksetzen
  - Gizmo (Cyan-Quader) im Scene-View zur Sichtbarkeit
- **PlanetData-Erweiterung:** 7 neue Felder unter `[Header("Umgebungszone")]`: `schwerkraft`, `atmosphaereFarbe`, `nebelDichte`, `hatWind`, `windStaerke`, `windTurbulenz`, `windPartikelFarbe`
- **Echte Werte:** Alle 8 Planeten-Assets befüllt (z.B. Jupiter: 24,79 m/s², dichte orange Atmosphäre, extreme Winde; Mars: 3,71 m/s², roter Staub, Sandstürme; Merkur: 3,7 m/s², keine Atmosphäre)

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
| **Meta Interaction SDK — Ray-Chain** | `Collider` → `ColliderSurface` → `RayInteractable` (Surface-Feld). `RayInteractable` nimmt KEINEN Collider direkt entgegen, sondern ein `ISurface`. `ColliderSurface` ist der Adapter. |
| **Grabbable — Transformer-Slots** | `GrabFreeTransformer` (Nachfolger des deprecated `OneGrabFreeTransformer`) implementiert beide Interfaces (1- und 2-Hand-Grab). Daher muss er in `Grabbable._oneGrabTransformer` UND `_twoGrabTransformer` eingetragen werden. |
| **Grabbable — Target-Override** | `Grabbable.InjectOptionalTargetTransform(t)` bewegt beim Greifen das übergebene Transform statt des GameObjects mit der Grabbable-Komponente. Damit wird die „Pille greifen, Canvas bewegen"-Mechanik ohne eigenen Code ermöglicht. |

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
| 2026-04-13 | Erde = 10 m VR-Radius als Immersiv-Referenz | Person steht „an der Oberfläche"; Planet füllt den Horizont sichtbar |
| 2026-04-16 | Raumstation additiv laden statt Planet in Main-Szene skalieren | Eigenständige Szene mit baked Lighting, Umgebungszone und Teleport; saubere Trennung der Verantwortlichkeiten |
| 2026-04-16 | OVR-Teleport ohne Rotation | Rotation des `spielerRoot` beim Teleport NICHT ändern — OVR-Tracking läuft relativ zur Root-Rotation; geänderte Rotation lässt die Szene „am Kopf kleben" |
| 2026-04-16 | Prefab-Referenz in GameManager neben PlanetData | `PlanetButton` übergibt Prefab an `PlanetAuswaehlen()`, das jetzt `ausgewaehltesPrefab` speichert — RaumstationController kann so das richtige Prefab spawnen ohne PlanetData um ein `prefab`-Feld zu erweitern |
| 2026-04-21 | GrabHandle komplett auf Meta Interaction SDK umgestellt | Vorher drei parallele Raycast-Systeme (eigenes GrabHandle.cs + ControllerRay-Quickfix + Interaction SDK fürs Canvas). Jetzt eine einzige Ray-Pipeline via RayInteractable + Grabbable; `GrabHandleGlue.cs` übernimmt nur noch die projektspezifischen Hooks (LazyFollow-Pause, Kamera-Ausrichtung) |
| 2026-04-21 | GrabHandle als Prefab statt Code-Generation | `ArPlanetInstanz` instanziiert `Assets/Prefabs/GrabHandle.prefab` statt 45 Zeilen GameObject-Primitive-Aufbau. Plug-and-Play für weitere Usecases (jedes grabbare Objekt = Prefab spawnen + `InjectOptionalTargetTransform`) |

---

## Bekannte Probleme / Offene Editor-Aufgaben

| Problem | Status |
|---|---|
| SonnensystemPanel-Canvas im Unity-Editor erstellen (4 Slider + SonnensystemUI.cs) | Offen |
| PlanetDetailPanel-Canvas im Unity-Editor erstellen (PlanetDetailUI.cs verdrahten) | Offen |
| Raumstation: Mini-Menü Canvas erstellen (WorldSpace + LazyFollowUI + "Verlassen"-Button) | Offen |
| Raumstation: SpawnPunkt-GO (Spielerposition) und PlanetSpawnPunkt-GO (außen) korrekt positionieren | Offen |
| Raumstation: Atmosphäre- und Wind-PartikelSystems feintunen (Shape, Lifetime, Größe) | Offen |
| Raumstation: Raumstation.unity in Build Settings eintragen | Offen |
| „InfoSystem" leeres GameObject erstellen, InfoPanel.cs drauf, panelCanvas-Feld befüllen | Offen |
| InfoPanel-Canvas als World-Space-Canvas erstellen (TitelText, InhaltText, Schließen-Button) | Offen |
| 6 fehlende Planeten-Prefabs erstellen (Merkur, Venus, Mars, Saturn, Uranus, Neptun) | Offen |
| InfoPunkt-Empties auf Planeten-Prefabs verteilen (3–4 pro Planet) | Offen |
| PlanetData-Assets mit `beschreibung` und `fakten[]` befüllen | Offen |
| OVRPassthroughLayer-Referenz im GameManager-Inspector verdrahten | Offen |
| `StartPhase.cs` und `SpielerBewegung.cs` im Projekt aufräumen (nicht integrierte Altlasten) | Offen |
| GrabHandle-Prefab auch für UI-Canvases (AuswahlMenü, SonnensystemPanel, PlanetDetailPanel) anbringen | Offen |
| InfoPunkt ebenfalls auf `RayInteractable` + `InteractableUnityEventWrapper` migrieren (InfoPanel.cs hat heute denselben manuellen Raycast-Code wie das alte GrabHandle.cs) | Offen |
