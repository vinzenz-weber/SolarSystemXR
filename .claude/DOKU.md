# Dokumentation: Sonnensystem XR

> **Letzte Aktualisierung:** 2026-04-26

---

## Technisches Setup

| Bereich | Detail |
|---|---|
| Unity Version | 6000.3.10f1 |
| Render Pipeline | URP |
| Target Platform | Quest 3 Standalone (Android) |
| XR SDK | Meta XR SDK v85 |
| Platzierung | Depth API (`EnvironmentRaycastManager`) — **kein MRUK-Room** |
| Testing (Windows) | Meta Quest Link (Play Mode im Headset) |
| Testing (Mac) | Build auf Headset |

---

## Aktuelle Features

### Sonnensystem-Simulation

- **Status:** Fertig
- **Beschreibung:** Alle 8 Planeten umkreisen die Sonne auf Kepler-Ellipsen. Größen und Abstände sind korrekt skaliert (Abstände und Planetengrößen unabhängig voneinander skalierbar).
- **Umsetzung:**
  - `PlanetData` ScriptableObjects mit echten astronomischen Daten (Durchmesser, semiMajorAxis, Exzentrizität, Umlaufzeit) + `beschreibung`, `fakten[]` und Umgebungszonen-Felder
  - `PlanetBody.cs`: Kepler-Formel `r = a*(1-e²)/(1+e*cos(θ))`, angle-basiertes Trail-Sampling
  - `SolarSystemManager.cs`: Zentrale Skalierungsvariablen `distanceScale`, `planetSizeScale`, `exzentrizitaetMultiplikator`
  - Sonnengröße dynamisch: kann Merkur-Orbit nie überlappen
- **Skalierung:** `distanceScale = 0.006` → Neptun bei ~18 cm Radius (Tischgröße)
- **Assets:** 9 PlanetData-Assets (Merkur, Venus, Erde, Mars, Jupiter, Saturn, Uranus, Neptun, Sonne)

### Trail-Effekt (Planetenspuren)

- **Status:** Fertig
- **Beschreibung:** Jeder Planet zieht eine leuchtende Spur in seiner Farbe. Spur deckt exakt einen vollen Orbit ab und fadet zum alten Ende aus.
- **Umsetzung:** `LineRenderer` mit Ring-Buffer, HDR-Farben + URP Bloom, beidseitig sichtbar (`_Cull=0`), pre-allokierter Buffer + `SetPositions()`-Batch-API.

### Planet Shader & SunPasser

- **Status:** In Entwicklung
- **Beschreibung:** Eigene Planeten-Materialien mit dynamischer Beleuchtung durch die Sonne und Atmosphären-Effekt.
- **Umsetzung:** `SunPasser.cs` (`[ExecuteAlways]`) + `M_Planet.shadergraph` (3 Wolken-Layer, Fresnel-Atmosphäre via `_AtmosphereInnerColor` / `_AtmosphereOuterColor` mit `_AtmosphereBias`-Power-Kurve, Atmosphäre nur sonnenseitig sichtbar).

### Cloud-Animation (Erde)

- **Status:** Fertig
- **Beschreibung:** Rotierende Wolkenschicht auf dem Erde-Prefab.
- **Umsetzung:** `CloudBehaviour.cs` — Y-Rotation mit `cloudSpeed * Time.deltaTime`, `sizePercentage` (0–3 %), `OnValidate()` für Live-Preview.

### State Machine (Spielzustände)

- **Status:** Scripts fertig, Editor-Setup teils ausstehend
- **Beschreibung:** 3 Zustände: `MAIN_MENU → PLACEMENT → WORLD`.
- **Umsetzung:** `GameManager.cs` (Singleton)
  - `MAIN_MENU`: Hauptmenü-Canvas sichtbar; User wählt Planet oder Sonnensystem
  - `PLACEMENT`: Hauptmenü zu; `PlacementManager` zeigt Vorschau am Depth-Raycast
  - `WORLD`: Objekt steht; `OVRInput.Button.Start` öffnet das Hauptmenü erneut
- **Hinweis:** Vorgängerversionen mit 6 Zuständen + Raumstation/Immersive-Modus sind ins Archiv gewandert.

### Platzierung (Depth API)

- **Status:** Fertig (Code), Editor-Setup im Build noch testen
- **Beschreibung:** Im `PLACEMENT`-State folgt eine Vorschau dem Controller-Ray. Trifft der Ray eine annähernd horizontale Fläche (Boden/Tisch), färbt sich die `LineRenderer`-Linie grün; Trigger platziert das Objekt.
- **Umsetzung:** `PlacementManager.cs`
  - Raycast über `Meta.XR.EnvironmentRaycastManager.Raycast(Ray, out hit)` — direkt gegen das Depth-Mesh, **ohne MRUK-Room**
  - `IsHorizontal(normal)`: `Vector3.Dot(normal, Vector3.up) > 0.85` als Boden-Kriterium
  - **Zwei Modi:**
    - `SelectPlanet(PlanetData)` — instanziiert `previewPrefab`, skaliert mit `GetScaledSize(diameter)` (`earthDiameterInVR = 0.2 m` als Referenz), platziert mit Höhenoffset `planetHeight`
    - `SelectSolarSystem(GameObject)` — instanziiert das Sonnensystem-Prefab in eigener Größe, ohne Skalierung und ohne Höhenoffset
  - `placementVisualizerPrefab` (Ring/Marker) wird parallel zur Vorschau am Raycast-Hitpoint angezeigt
  - Platzierung mit `OVRInput.Button.SecondaryIndexTrigger` → spawnt das echte Prefab, wechselt in `WORLD`
  - **Alle platzierten Objekte werden in einer `_placedObjects`-Liste getrackt** und beim Tab-Wechsel im Hauptmenü via `ClearPlacedObjects()` zerstört

### Hauptmenü (Planeten / Sonnensystem)

- **Status:** Scripts fertig, Canvas-Setup ausstehend
- **Beschreibung:** World-Space-Canvas mit zwei umschaltbaren Panels und einer Tab-Bar unten:
  - **Planeten-Panel:** Headline, Beschreibung, „[Planet] hinzufügen"-Button, horizontaler Scroller mit Planet-Buttons
  - **Sonnensystem-Panel:** Headline, Beschreibung, „Start Experience"-Button
  - **Tabs:** „Planeten" / „Sonnensystem" — Wechsel zerstört alle bereits platzierten Objekte (`PlacementManager.ClearPlacedObjects()`)
- **Umsetzung:**
  - `MainMenuController.cs` — verwaltet beide Panels, Tab-Highlight via Background-Image-Color, hält `sonnensystemPrefab`-Referenz, ruft `placementManager.SelectPlanet(...)` bzw. `SelectSolarSystem(...)` auf
  - `PlanetMenuButton.cs` — auf jedem Planet-Button im Scroller, befüllt Label aus `PlanetData` und ruft im `OnClick` `MainMenuController.SelectPlanet(data)` auf
  - `PlanetButton.cs` — schmale Bridge zwischen `PlanetData` und Anzeige-Prefab (Inspector-Verbindung)

### Sonnensystem-Parameter-UI

- **Status:** Script fertig, Editor-Setup ausstehend
- **Beschreibung:** World-Space-Panel mit 4 Slidern zum Live-Anpassen der Simulation.
- **Umsetzung:** `SonnensystemUI.cs`
  - **Bahnabstände:** 0.002–0.025 → `SolarSystemManager.distanceScale`
  - **Planetengrößen:** 0.0005–0.01 → `SolarSystemManager.planetSizeScale`
  - **Geschwindigkeit:** 0–100 Tage/s → `SolarSystemManager.timeScale`
  - **Exzentrizität:** 0–5 → `SolarSystemManager.exzentrizitaetMultiplikator`
  - Reset-Button setzt alle Werte auf Defaults

### Desktop-Testing Tools

- **Status:** Fertig (nicht für Quest-Build relevant)
- **Beschreibung:** Scripts für Desktop-Entwicklung ohne Headset.
- **Umsetzung:**
  - `DesktopDebugCamera.cs` — Free-look (WASD + Maus, Shift = Boost, ESC = Cursor-Unlock)
  - `DesktopPlacement.cs` — platziert Prefab via Rechtsklick-Raycast
  - `PlacementTester.cs` — Spawn an RayInteractor-Hitpoint

### LazyFollowUI

- **Status:** Fertig
- **Beschreibung:** Wiederverwendbare Komponente; lässt einen Canvas träge der Spielerblickrichtung folgen, ohne vertikales Kippen.

---

## Wichtige technische Konventionen

| Konvention | Detail |
|---|---|
| **Depth API für Platzierung** | `EnvironmentRaycastManager.Raycast(Ray, out hit)` aus `Meta.XR.MRUtilityKit`. Liefert Position + Normal direkt aus dem Depth-Mesh — kein `MRUKRoom` und kein `Physics.Raycast` nötig. |
| **Horizontalität** | Boden/Tisch wird über `Vector3.Dot(normal, Vector3.up) > 0.85` erkannt. |
| **Skalierung Planeten** | `vrScale = (diameter / earthDiameterInKm) * earthDiameterInVR`. Sonnensystem-Prefab dagegen wird **nicht** skaliert — es muss bereits in VR-Größe gestaltet sein. |
| **Tab-Wechsel räumt auf** | Sowohl `ShowPlanetenTab()` als auch `ShowSonnensystemTab()` rufen `placementManager.ClearPlacedObjects()` auf, damit Modi nicht visuell vermischt werden. |
| **Naming Conventions** | `_camelCase` für private Felder, `camelCase` für public, `PascalCase` für Methoden/Klassen. Booleans mit `is/has/can`. |

---

## Aktuelle Entscheidungen

| Datum | Entscheidung | Begründung |
|---|---|---|
| 2026-04-12 | Meta Building Blocks bevorzugt | Einfacher, schneller, weniger eigener Code |
| 2026-04-12 | Code auf einfachem Level halten | Verständlichkeit und eigene Bearbeitbarkeit |
| 2026-04-12 | Code-Kommentare auf Deutsch | Muttersprache, einfacher für Doku |
| 2026-04-12 | World-Space Canvas statt Screen-Space | In XR gibt es keinen echten Screen — World-Space wird im 3D-Raum verankert |
| 2026-04-12 | PlanetData als ScriptableObject | Trennung von Daten und Logik; einfach erweiterbar ohne Code-Änderung |
| 2026-04-26 | Platzierung über Depth API statt MRUK-Room | Meta-SDK-Versionssprünge haben den MRUK-basierten Ansatz wiederholt gebrochen. `EnvironmentRaycastManager` liefert Position + Normal direkt aus dem Depth-Mesh — ohne Room-Setup, ohne Anchor-Kette, ohne weiteren SDK-Bruchpunkt |
| 2026-04-26 | State Machine auf 3 Zustände reduziert | Raumstation, Immersive-Modus, separate Detail-Panels und Info-Punkte sind aus dem Prototyp gestrichen. Der reduzierte Flow (`MAIN_MENU → PLACEMENT → WORLD`) deckt den aktuellen Wireframe vollständig ab |
| 2026-04-26 | Hauptmenü als Tab-Toggle | Wireframe sieht zwei nebeneinanderliegende Panels (Planeten / Sonnensystem) mit Tab-Bar unten vor — beim Wechsel wird die platzierte Welt geleert, damit Modi nicht vermischt werden |

---

## Bekannte Probleme / Offene Editor-Aufgaben

| Aufgabe | Status |
|---|---|
| Hauptmenü-Canvas im Editor aufbauen (`PlanetenPanel` / `SonnensystemPanel` / `TabBar` mit Buttons) | Offen |
| `MainMenuController`-Felder verkabeln (Panels, Texte, Tab-Backgrounds, Sonnensystem-Prefab) | Offen |
| Sonnensystem-Prefab in finaler VR-Größe gestalten (keine Laufzeit-Skalierung mehr!) | Offen |
| Planet-Buttons im Scroller mit jeweiligem `PlanetData` befüllen | Offen |
| `PlacementManager.placementVisualizerPrefab` zuweisen | Offen |
| Depth-API-Setup in der Szene prüfen (`EnvironmentRaycastManager`, Permissions, Build-Settings) | Offen |
| `StartPhase.cs` und `SpielerBewegung.cs` (Altlasten ohne Anbindung) aufräumen oder entfernen | Offen |
| `InfoPunkt.cs` ist noch im Repo, hat aber keine Anbindung mehr — entscheiden ob entfernen oder reaktivieren | Offen |
