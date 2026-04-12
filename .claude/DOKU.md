# Dokumentation: Sonnensystem XR

> **Letzte Aktualisierung:** 2026-04-12

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
  - `PlanetData` ScriptableObjects mit echten astronomischen Daten (Durchmesser in km, semiMajorAxis in AU, Exzentrizität, Umlaufzeit in Tagen)
  - `PlanetBody.cs`: Kepler-Formel `r = a*(1-e²)/(1+e*cos(θ))`, angle-basiertes Trail-Sampling (frame-rate-unabhängig)
  - `SolarSystemManager.cs`: Zentrale Skalierungsvariablen `distanceScale` und `planetSizeScale`
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

- **Status:** Fertig
- **Beschreibung:** Eigene Planeten-Materialien mit dynamischer Beleuchtung durch die Sonne. Sonnenposition wird per Script in Echtzeit an alle Planeten-Materialien übergeben.
- **Umsetzung:** `SunPasser.cs`
  - `[ExecuteAlways]` — läuft im Editor und im Play Mode (Editor-Preview möglich)
  - Setzt `_SunPosition` Shader-Property direkt auf allen zugewiesenen Material-Instanzen
  - Kein SRP-Batching-Bruch, da auf `shared Material`-Instanzen geschrieben wird
- **Anmerkung:** Shader-Durchbruch war ein wichtiger Meilenstein (Commit: "PLANET MATERIAL GEHT ENDLICH!!!!")

### Cloud-Animation (Erde)

- **Status:** Fertig
- **Beschreibung:** Rotierende Wolkenschicht auf dem Erde-Prefab.
- **Umsetzung:** `CloudBehaviour.cs`
  - Rotiert Wolken-Kindsobjekt um Y-Achse mit `cloudSpeed * Time.deltaTime`
  - `sizePercentage` (0–3%) steuert Skalierung relativ zum Planeten
  - `OnValidate()` für Live-Preview im Editor

### State Machine (Spielzustände)

- **Status:** In Arbeit
- **Beschreibung:** Vier Zustände: START → AUSWAHL → EXPLORE. PLACEMENT ist reserviert, aber noch nicht implementiert.
- **Umsetzung:** `GameManager.cs`
  - START → AUSWAHL: Automatisch nach 1 Sekunde via Coroutine
  - AUSWAHL: World-Space-Menü erscheint vor dem Spieler; Hauptmenü und Planetenmenü verfügbar
  - EXPLORE: Ausgewähltes Objekt (Sonnensystem oder Einzel-Planet) erscheint `spawnAbstand` Meter vor der Kamera
  - Toggle zwischen AUSWAHL und EXPLORE: `OVRInput.GetDown(OVRInput.Button.Start)` (Menü-Button links am Controller)
- **Offene Punkte:** PLACEMENT-Zustand (AR-Platzierung im Raum) noch nicht implementiert

### UI-System (Menü)

- **Status:** In Arbeit
- **Beschreibung:** World-Space-Canvas mit Hauptmenü (Sonnensystem / Planeten) und Planeten-Untermenü.
- **Umsetzung:** `GameManager.cs` + `PlanetButton.cs`
  - Canvas wird beim Wechsel zu AUSWAHL via `Quaternion.LookRotation` direkt vor der Kamera positioniert
  - `hauptPanel`: Hauptmenü (Sonnensystem-Button, Planeten-Button)
  - `planetenPanel`: Untermenü mit je einem Button pro Planet + Zurück-Button
  - `PlanetButton.cs`: Verbindet `PlanetData`-ScriptableObject mit zugehörigem Prefab; `OnKlicken()` ruft `GameManager.PlanetAuswaehlen()` auf
- **Offene Punkte:** Nur Erde und Jupiter haben vollständige Prefabs; restliche Planeten-Detailansichten fehlen noch

### Planeten-Detailansicht

- **Status:** In Arbeit (Erde und Jupiter vorhanden)
- **Beschreibung:** Einzelne Planeten-Prefabs mit texturierten Materialien, die per Menü aufgerufen und vor dem Spieler eingeblendet werden.
- **Umsetzung:**
  - `Erde.prefab`: Texturierter Planet + separate Wolkenschicht (`CloudBehaviour`)
  - `Jupiter.prefab`: Texturierter Planet
  - Position beim Spawn: `kamera.forward * spawnAbstand` (default: 1.5 m), mit Blickrichtung zum Spieler gedreht
- **Offene Punkte:** Merkur, Venus, Mars, Saturn, Uranus, Neptun noch nicht als Detailprefabs vorhanden

### XR-Input

- **Status:** Fertig (Menü-Steuerung)
- **Beschreibung:** Menü-Toggle via OVR-Menü-Button (linker Controller).
- **Umsetzung:** `GameManager.cs`
  - `OVRInput.GetDown(OVRInput.Button.Start)` → wechselt zwischen AUSWAHL und EXPLORE

### Desktop-Testing Tools

- **Status:** Fertig
- **Beschreibung:** Drei Scripts für Desktop-Entwicklung und -Testing ohne Headset, ausgelegt auf Mouse + Keyboard.
- **Umsetzung:**
  - `DesktopDebugCamera.cs`: Free-look Kamera (WASD + Maus, Shift = Boost, ESC = Cursor-Unlock). Nutzt neues Input System.
  - `DesktopPlacement.cs`: Platziert SolarSystem-Prefab via Rechtsklick-Raycast gegen `placementLayer`. Distanz → automatische Skalierung (0.3 m → `distanceScale` 0.006; 3.0 m → 0.05). Leertaste = Fixieren, R = Freigeben.
  - `PlacementTester.cs`: Oculus Interaction SDK — spawnt Prefab an `RayInteractor`-Kollisionspunkt via `ISelector`-Event

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

---

## Bekannte Probleme

| Problem | Ursache | Status |
|---|---|---|
| PLACEMENT-Zustand nicht implementiert | AR-Platzierung via MRUK oder XR-Ray noch ausstehend | Offen |
| Nur Erde & Jupiter als Detailprefabs | Restliche 6 Planeten-Prefabs noch nicht erstellt | Offen |
| `StartPhase.cs` und `SpielerBewegung.cs` im Projekt | Aufgeräumte Altlasten aus frühen Experimenten; nicht integriert | Offen (aufräumen) |
| `OVRInteractionComprehensive` Ray nur bei Interactables | Interaction SDK Ray sichtbar nur bei hovering über `IPointable`-Objekt | Bekannt |
