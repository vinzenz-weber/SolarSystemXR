# Archiv: Sonnensystem XR

> Verworfene Features, gescheiterte Ansätze und Iterationen.
> Dieses Dokument dient als Grundlage für das Progress-Kapitel der Masterarbeit.

---

## Verworfene Features

### DebugDisplay (HUD-Panel)

- **Zeitraum:** bis 2026-04-12
- **Beschreibung:** Schwebendes HUD-Panel direkt vor dem User via `DebugDisplay.cs`. Canvas wurde zur Laufzeit erzeugt und als Kind der Kamera gehängt. Zeigte State, Controller-Input, Hand-Tracking-Daten, MRUK-Status und Live-Raycast-Ergebnis mit TMP-Text und Rich-Text-Farben (grün/rot/gelb).
- **Grund für Verwerfung:** Mit der Vereinfachung des GameManagers (Entfernung von MRUK und Hand Tracking) wurde das DebugDisplay obsolet. Script komplett entfernt.
- **Erkenntnisse:** Debug-HUDs sind nützlich während der Entwicklung, aber schleppen Komplexität mit sich wenn die zugrunde liegende Architektur sich ändert. Unitys eigener Immersive Debugger (über `ImmersiveDebuggerSettings.asset` im Projekt) kann als Alternative dienen.

### SurfacePlane Prefab (Physics-Collider für Oberflächen)

- **Zeitraum:** 2026-04-12
- **Beschreibung:** Prefab mit `OVRSceneAnchor` + `BoxCollider` auf Layer "Placement". Sollte als `PlanePrefab` im `OVRSceneManager` registriert werden, damit erkannte Raumflächen physikalische Collider bekommen und `Physics.Raycast` dagegen treffen kann.
- **Grund für Verwerfung:** `OVRSceneAnchor` ist ebenfalls deprecated (wie `OVRSceneManager`). Der gesamte Ansatz — Physics-Collider auf erkannten Flächen — ist mit MRUK nicht mehr nötig, da `MRUKRoom.Raycast()` direkt gegen die Raumgeometrie raycastet ohne Physics-Collider.
- **Erkenntnisse:** Meta hat zwei parallele Generationen von Scene-Understanding-APIs. Die alte Generation (`OVRSceneManager`, `OVRSceneAnchor`, `OVRScenePlane`) ist als Block deprecated und wurde durch MRUK (MR Utility Kit) ersetzt. MRUK abstrahiert Physics komplett weg.

---

## Iterationen & Änderungen

### Planet Shader: Gradient-Node für Atmosphären-Farbverlauf

- **Datum:** 2026-04-15
- **Vorher:** Atmosphären-Farbverlauf sollte über einen `Gradient`-Node in ShaderGraph realisiert werden — ein einzelnes Property das im Blackboard als Farbverlauf definiert wird.
- **Problem:** Unity's `Gradient`-Property-Typ wird im Material-Inspector nicht angezeigt. Er ist nicht als serialisiertes Material-Property unterstützt und damit im Inspector nicht einstellbar — nur direkt im ShaderGraph editierbar.
- **Nachher:** Zwei separate `Color`-Properties (`_AtmosphereInnerColor`, `_AtmosphereOuterColor`) + `Lerp`-Node. Der Fresnel-Wert steuert den `T`-Input des Lerp. Zusätzlich ein `Float`-Property `_AtmosphereBias` (Power-Node vor dem Lerp-T-Input) für Feintuning der Kurve.
- **Erkenntnisse:** ShaderGraph-`Gradient`-Properties sind Editor-only und können nicht im Material-Inspector exponiert werden. Für Inspector-sichtbare Farbverläufe: entweder mehrere Color-Properties + Lerp, oder einen Farbverlauf als Textur (256×1 px) backen und als `Texture2D`-Property einbinden.

### GameManager: State Machine komplett umgebaut (erste Iteration)

- **Datum:** 2026-04-12
- **Vorher:** `START → PLACEMENT → EXPLORE`
  - PLACEMENT war der zweite Zustand, direkt nach Start
  - User platziert das Sonnensystem via XR-Ray im Raum (MRUK-Raycast)
  - EXPLORE war danach der Erkundungsmodus ohne Menü
- **Nachher:** `START → AUSWAHL → EXPLORE` (PLACEMENT reserviert, nicht implementiert)
  - AUSWAHL ist nun der zweite Zustand — ein World-Space-Menü erscheint vor dem User
  - User wählt was angezeigt werden soll (Sonnensystem oder einzelner Planet)
  - EXPLORE zeigt das gewählte Objekt vor dem User; Menü-Button togglet zurück zu AUSWAHL
- **Grund:** Die Priorität hat sich verschoben: zuerst ein funktionierendes Inhaltssystem (Menü + Planeten-Detailansichten), danach AR-Platzierung. Das Menü ermöglicht mehr Inhalt bevor die Platzierungs-Infrastruktur fertig ist.

### GameManager: State Machine auf Prototyp-Vollumfang erweitert (zweite Iteration)

- **Datum:** 2026-04-13
- **Vorher:** `START → AUSWAHL → EXPLORE`
  - `EXPLORE` war ein einzelner Zustand für beide Ansichten (Sonnensystem und Einzel-Planet)
  - PLACEMENT war reserviert aber nicht implementiert
  - Kein Passthrough-Toggle, keine separaten Panels pro Modus
- **Nachher:** `START → AUSWAHL → PLACEMENT → SONNENSYSTEM / PLANET_SCHWEBEND / PLANET_IMMERSIV`
  - `EXPLORE` durch drei spezialisierte Zustände ersetzt: `SONNENSYSTEM`, `PLANET_SCHWEBEND`, `PLANET_IMMERSIV`
  - `PLACEMENT` vollständig implementiert via `PlatzierungManager.cs` (Ring-Indikator, Abstandsanpassung, Trigger-Bestätigung)
  - Passthrough-Toggle (`PassthroughEinschalten`/`PassthroughAusschalten`) über `OVRPassthroughLayer`
  - Jeder Zustand aktiviert sein eigenes UI-Panel (`sonnensystemPanel`, `planetDetailPanel`, `immersivPanel`)
- **Grund:** Mit der Anforderung „so schnell wie möglich einen vollständigen Prototyp" wurden alle 5 Kernfeatures gleichzeitig implementiert. Der generische EXPLORE-Zustand war zu grob — die drei Ansichten haben grundlegend verschiedene UI, Input-Logik und Passthrough-Verhalten.

### GameManager: START-Transition

- **Datum:** 2026-04-12
- **Vorher:** `Input.GetKeyDown(KeyCode.Space)` → Wechsel zu PLACEMENT (Desktop-only)
- **Nachher:** Automatisch nach 1 Sekunde via Coroutine
- **Grund:** App läuft auf Quest — Keyboard-Input funktioniert nicht im Headset

### GameManager: MRUK komplett entfernt

- **Datum:** 2026-04-12
- **Vorher:** GameManager nutzte `MRUK.Instance.RegisterSceneLoadedCallback()` für Zustandsübergang, `MRUKRoom.Raycast()` für Oberflächenerkennung. `MRUK.SceneLoadedEvent` triggerte den Wechsel zu PLACEMENT.
- **Nachher:** Kein MRUK-Code mehr im GameManager. Platzierung via Desktop (`DesktopPlacement.cs`) oder Interaction SDK (`PlacementTester.cs`) — MRUK für den PLACEMENT-Zustand noch ausstehend.
- **Grund:** Mit der Neupriorisierung (Menü-System zuerst) wurde MRUK-Placement nach hinten verschoben. Der vorhandene MRUK-Code wurde entfernt um den GameManager übersichtlich zu halten.
- **Erkenntnisse:** MRUK bleibt die richtige Wahl für AR-Platzierung auf der Quest — der Code-Ansatz war korrekt, nur der Zeitpunkt der Implementierung hat sich verschoben.

### GameManager: XR-Input vereinfacht

- **Datum:** 2026-04-12
- **Vorher:** Komplexes Input-System mit zwei Modi:
  - Controller: `OVRInput.GetDown(PrimaryIndexTrigger, RTouch)` / `Button.One`
  - Hand Tracking: `OVRHand.GetFingerIsPinching()` mit eigenem GetDown-Emulations-State
  - Ray-Ursprung: Hand → `OVRHand.PointerPose`, Controller → `RightControllerAnchor`, Fallback → Kamera-Mitte
  - Zugewiesene Objekte: `rightControllerAnchor` + `rightHand` (OVRHand)
- **Nachher:** Nur noch `OVRInput.GetDown(OVRInput.Button.Start)` — der Standard Quest-Menü-Button (linker Controller)
- **Grund:** Der neue Menü-basierte Fluss braucht nur einen einzigen Toggle. Das Interaction SDK (`PlacementTester.cs`) übernimmt komplexere Ray-Interaktionen.

### GameManager: Placement-Input

- **Datum:** 2026-04-12
- **Vorher:** `Input.GetMouseButtonDown(1)` (Rechtsklick) → Objekt platzieren; `Input.GetKeyDown(Return)` → EXPLORE
- **Nachher:** `OVRInput` (Controller-Trigger, A-Button) + `OVRHand.GetFingerIsPinching()` (Hand Tracking) — inzwischen weiter vereinfacht (siehe oben)
- **Grund:** Desktop-Input funktioniert nicht im Headset; OVRInput unterstützt beide Eingabemethoden

### GameManager: Oberflächen-Raycast

- **Datum:** 2026-04-12
- **Vorher:** `Physics.Raycast` gegen `placementLayer` (LayerMask) — erfordert Collider in der Scene
- **Zwischenschritt:** `MRUKRoom.Raycast()` als primäre Methode (trifft erkannte Raumflächen direkt); `Physics.Raycast` bleibt als Fallback
- **Aktuell:** `Physics.Raycast` wieder die primäre Methode in `DesktopPlacement.cs`; MRUK-Raycast ausgelagert (noch nicht reimplementiert)
- **Grund:** Im Passthrough-Modus gibt es keine Collider in der Scene — MRUK ist die richtige Langfristlösung, aber Desktop-Raycast funktioniert für Testing

### GameManager: Szenen-Lade-Strategie (OVRSceneManager → MRUK)

- **Datum:** 2026-04-12
- **Vorher:** `OVRSceneManager` Komponente mit Events `SceneModelLoadedSuccessfully`, `NoSceneModelToLoad`, `SceneCaptureReturnedWithoutError` + manuelles `RequestSceneCapture()`
- **Nachher:** `MRUK.Instance.RegisterSceneLoadedCallback()` + MRUK lädt automatisch via `LoadSceneOnStartup = true` und öffnet Space Setup wenn nötig (`requestSceneCaptureIfNoDataFound = true` by default)
- **Anmerkung:** MRUK-Code inzwischen aus GameManager entfernt (siehe "MRUK komplett entfernt")
- **Grund:** `OVRSceneManager` ist seit Meta XR SDK v65 deprecated. MRUK ist die aktuelle, empfohlene API und vereinfacht den Ablauf erheblich (kein manuelles `RequestSceneCapture()` nötig).

### DebugDisplay: Canvas-Positionierung

- **Datum:** 2026-04-12
- **Vorher:** Canvas als Root-GameObject erstellt, Position in `LateUpdate()` manuell gesetzt (`_canvasTransform.position = camPos + forward * dist`)
- **Nachher:** Canvas als Kind der Kamera erzeugt (`SetParent(camTransform)`), `localPosition` mit festem Offset — folgt automatisch ohne LateUpdate-Logik
- **Anmerkung:** DebugDisplay insgesamt inzwischen entfernt
- **Grund:** `MissingReferenceException` auf `_canvasTransform` in LateUpdate. Unity-Transform-Referenz war nach dem Erstellen unter bestimmten Bedingungen ungültig. Als Kind der Kamera entfällt das Problem komplett.

### InfoPanel: Script-Placement auf Canvas (erster Ansatz)

- **Datum:** 2026-04-13
- **Vorher:** `InfoPanel.cs` lag direkt auf dem World-Space-Canvas-GameObject
  - `Start()` rief `gameObject.SetActive(false)` auf um den Canvas auszublenden
  - `Update()` lief scheinbar normal — bis der Canvas ausgeblendet wurde
- **Problem:** `gameObject.SetActive(false)` deaktiviert das gesamte GameObject inklusive aller darauf liegenden Scripts. Damit stoppt `Update()` dauerhaft — der Raycast lief nie mehr.
- **Nachher:** `InfoPanel.cs` liegt auf einem persistenten leeren GameObject „InfoSystem" (nicht auf dem Canvas)
  - `public GameObject panelCanvas` als separates Referenzfeld
  - Show/Hide über `panelCanvas.SetActive()` — das InfoSystem-Objekt selbst bleibt immer aktiv
- **Erkenntnisse:** In Unity gilt: ein Script dessen `Update()` dauerhaft laufen muss (z.B. Raycast-Logik), darf nie auf einem GameObject liegen das zur Laufzeit via `SetActive(false)` ausgeblendet wird. Singleton-Scripts die etwas steuern sind fast immer besser auf einem separaten persistenten Elternobjekt aufgehoben.

---

## Verworfene Entscheidungen

| Datum | Entscheidung | Warum revidiert |
|---|---|---|
| 2026-04-12 | OVRSceneManager für Oberflächen-Erkennung | Deprecated seit SDK v65; durch MRUK ersetzt |
| 2026-04-12 | SurfacePlane-Prefab mit OVRSceneAnchor | OVRSceneAnchor ebenfalls deprecated; mit MRUK überflüssig |
| 2026-04-12 | "Placement" Layer (Layer 6) als primäre Raycast-Methode | Mit MRUK.Raycast() nicht mehr nötig; bleibt nur als Desktop-Fallback |
| 2026-04-12 | MRUKRoom.Raycast() statt Physics.Raycast | MRUK temporär aus GameManager entfernt; DesktopPlacement nutzt Physics.Raycast |
| 2026-04-12 | MRUK als Singleton (MRUK.Instance) | MRUK aus GameManager entfernt; Placement noch ausstehend |

---

## Gescheiterte Experimente

### OVRSceneManager in Main.unity via YAML eintragen

- **Datum:** 2026-04-12
- **Ziel:** OVRSceneManager-Komponente direkt in die `.unity`-Datei schreiben ohne Unity Editor, inklusive `PlanePrefab`-Referenz auf das SurfacePlane-Prefab
- **Ergebnis:** Technisch möglich, aber unnötig komplex — `OVRSceneManager.PlanePrefab` ist vom Typ `OVRSceneAnchor`, das Prefab muss eine `OVRSceneAnchor`-Komponente haben. Kurz danach stellte sich heraus, dass die gesamte API deprecated ist.
- **Erkenntnisse:** Deprecated APIs möglichst früh erkennen. Meta-SDKs haben oft parallele Generationen die gleichzeitig im Package Cache vorhanden sind. Immer prüfen ob `[Obsolete]`-Attribute gesetzt sind.
