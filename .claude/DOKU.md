# Dokumentation: Sonnensystem XR

> **Letzte Aktualisierung:** 2026-05-14

---

## Technisches Setup

| Bereich | Detail |
|---|---|
| Unity Version | 6000.3.10f1 |
| Render Pipeline | URP |
| Target Platform | Quest 3 Standalone (Android) |
| XR SDK | Meta XR SDK v85 |
| Platzierung | Quest-Build: Depth API (`EnvironmentRaycastManager`), Editor-Playmode: Ray-Fallback ohne Depth API |
| Testing (Windows) | Unity Playmode fuer UI/Flow; Quest-Build fuer echte Depth API |
| Testing (Mac) | Build auf Headset |

---

## Aktuelle Roadmap

- Aktuelle Arbeitsgrundlage ist `ROADMAP.md`: Main Menu Umbau auf `Learn` / `Test` mit spaeteren Minispielen.
- Architektur-Entscheidung bleibt: alles in `MainScene.unity`, kein Scene-Loading. Modi werden ueber Container-GameObjects, `SetActive` und Prefab-Instanziierung gesteuert.
- Phase 1 ist erledigt: 8 Planeten-`PlanetData`-Assets sind vorhanden, dazu ein `Sonne.asset` fuer die Zentralsonne im Sonnensystem.
- Phase 2 ist code-seitig weitgehend vorbereitet, Editor-Verkabelung und Headset-Verifikation sind noch offen.
- Phase 3 ist in Umsetzung: finales datenbasiertes `MenuRoot`, `Planets` und `SolarSystem`, neues `PlanetDetailRoot`, globales Planet-InfoPanel, Sonnensystem-Slider-UI.
- Zusatzmodus fuer Phase 3 ist in Umsetzung: Immersive Mode zeigt den ausgewaehlten Planeten in einer VR-Natur-/Nachtszene am Mond-Ort.
- Phase 4 ist erledigt: Test-Tab startet Minigame-Prefabs, Abschliessen/Beenden funktionieren, Quiz-Fortschritt kann im Main Menu zurueckgesetzt werden.
- Phase 6 ist in Umsetzung: `Size` nutzt jetzt einen eigenen Manager, Prefab-World-Layout, Live-Auswertung und planetenspezifisches Groessen-Feedback.
> [NEU 2026-05-14 - START: Tutorial und User-Testing-Vorbereitungsmodus]
- Startsequenz wurde erweitert: Nach Logo/Splash kann ein kurzes Tutorial mit Erklaervideos laufen, bevor ins Passthrough-Hauptmenue gewechselt wird.
- Fuer die Testsituation gibt es einen User-Testing-Vorbereitungsmodus: Beide Controller-Thumbsticks gleichzeitig klicken schaltet in eine ruhige Passthrough-Ansicht, damit die Testperson zuerst ihre reale Umgebung sieht und die Brille ohne inhaltliche Ueberforderung angepasst werden kann.
> [NEU 2026-05-14 - ENDE]

## User Testing / Evaluation

- **Status:** Vorbereitung aktualisiert; finaler Test-Build und Headset-Durchlauf noch offen.
- **Zentrale Forschungsfrage fuer die Evaluation:** Wie kann eine XR-Lernanwendung zum Sonnensystem gestaltet werden, um astronomische Massstabsverhaeltnisse fachlich nachvollziehbar, raeumlich erfahrbar und spielerisch motivierend zu vermitteln?
- **Dokumente:**
  - `.claude/UserTesting.md`: deutschsprachige Arbeitsdatei fuer Studienlogik, Hypothesen, Aufgaben, Beobachtung und Auswertung.
  - `.claude/UserTesting_EN.md`: englische teilnehmer:innennahe Texte fuer Durchfuehrung, Aufgaben und Fragebogenmaterial.
- **Evaluationslogik:** Explorative Prototyp-Evaluation mit Pre-/Post-Fragebogen, App-Aufgaben, Beobachtung und kurzem Interview. Die Auswertung soll keine starke Kausalitaet behaupten, sondern Hinweise auf Verstaendlichkeit, raeumlichen XR-Mehrwert und Motivation liefern.
- **Fokus der Hypothesen:**
  - H1: Fachliche Nachvollziehbarkeit von astronomischen Massstabsverhaeltnissen verbessert sich nach der Nutzung.
  - H2: Raeumliche Erfahrbarkeit von Groessen-, Distanz- und Orbitverhaeltnissen wird nach der Nutzung hoeher eingeschaetzt.
  - H3: Spielerische Aufgaben werden als motivierend, verstaendlich und lernunterstuetzend wahrgenommen.
  - H4: Bedienbarkeit soll ausreichend stabil sein, damit Lern- und Motivationsurteile nicht hauptsaechlich durch technische Reibung verzerrt werden.
- **Aktuelle App-Aufgaben im Testing:** Planet aus Home platzieren, Detailinformationen fachlich einordnen, Planetengroessen vergleichen, Sonnensystem raeumlich betrachten, Massstabs-/Orbit-Controls nutzen, optional Immersive Mode pruefen, Reihenfolge-Minispiel, Groessen-Minispiel, unmittelbare Abschlussreflexion.
- **Qualitative Codes:** `Fachlich nachvollziehbar`, `Raeumlich erfahrbar`, `Spielerisch motivierend`.
- **Wichtig fuer finalen Build:** Vor dem Testing entscheiden, ob der Immersive Mode stabil genug fuer die Pflichtaufgabe ist oder im Testplan als optional/uebersprungen markiert wird.
> [NEU 2026-05-14 - START: User-Testing-Setup in der Durchfuehrung]
- **User-Testing-Setup:** Vor Beginn der eigentlichen App-Nutzung kann per gleichzeitigem Klick auf beide Controller-Thumbsticks ein Vorbereitungsmodus aktiviert werden. In diesem Zustand bleibt Passthrough sichtbar, die eigentlichen App-Inhalte werden ausgeblendet bzw. zurueckgesetzt und Audio wird pausiert. Ziel ist, die Brille in Ruhe anzupassen, Orientierung im echten Raum zu geben und die kognitive Belastung vor dem eigentlichen Teststart gering zu halten. Ein erneuter gleichzeitiger Thumbstick-Klick startet die App-Sequenz wieder mit Logo/Tutorial.
> [NEU 2026-05-14 - ENDE]

---

## Aktuelle Features

### Sonnensystem-Simulation

- **Status:** Fertig, wird in Phase 3 in den Learn-Flow eingebunden.
- **Beschreibung:** Planeten umkreisen die Sonne auf Kepler-Ellipsen. Groessen, Abstaende, Simulationszeit und Exzentrizitaet sind getrennt steuerbar.
- **Umsetzung:**
  - `PlanetData` ScriptableObjects mit astronomischen Daten, Beschreibungen, Fakten und Prefab-Referenzen.
  - `SolarSystemManager.cs` erzeugt Planeten und Orbit-Linien aus `PlanetData`.
  - Zentrale Runtime-Parameter: `distanceScale`, `planetSizeScale`, `timeScale`, `exzentrizitaetMultiplikator`, `inklinationMultiplikator`.
- **Hinweis:** Das Sonnensystem-Prefab wird bei der AR-Platzierung nicht mehr pauschal skaliert; es soll im Prefab selbst in sinnvoller VR-Groesse vorbereitet sein.

### Sonnensystem-Slider-UI

- **Status:** Script fertig, Prefab/Editor-Setup offen. Script-Inhalte muessen noch an das finale Figma-Design angepasst werden, bevor das Prefab gebaut wird.
- **Beschreibung:** Ein World-Space-Slider-Panel erscheint nach dem Platzieren des Sonnensystems neben dem User und steuert genau die frisch platzierte Sonnensystem-Instanz.
- **Umsetzung:**
  - `SonnensystemUI.cs` hat `Bind(SolarSystemManager)` und sucht den Manager nur als Fallback.
  - `PlacementManager.cs` findet nach dem Placement den `SolarSystemManager` im platzierten Prefab und bindet das UI automatisch.
  - Optional kann ein `SonnensystemUI` bereits im Sonnensystem-Prefab liegen; alternativ wird ein `sonnensystemUIPrefab` aus dem `PlacementManager` instanziiert.
  - Aktuelle Slider-Implementierung in `SonnensystemUI.cs`: Distanz (`distanceScale`), Groesse (`planetSizeScale`), Zeit (`timeScale`), Exzentrizitaet (`exzentrizitaetMultiplikator`).
- **Finales Design (aus Figma):** Das Sonnensystem-Panel soll "Our Solar System" heissen und 6 Controls enthalten:
  - **Spacing Mode** (Toggle) – "Diameter from side to side"; zeigt bei echter Skalierung eine Warnung ("outer planets may be far outside the room").
  - **Planet Scale** (Slider) – "Size of each planet relative to the others"
  - **Orbital Speed** (Slider) – "How fast planets travel around the Sun"
  - **Axial Tilt** (Slider) – "How strongly each planet leans on its axis"
  - **Orbital Distance** (Slider) – "Space between planets and the Sun"
  - **Eccentricity** (Slider) – "How oval the orbits appear"
  - Alle Slider zeigen ihren aktuellen Prozentwert unter dem Slider-Label.
- **Wichtig:** `SonnensystemUI.cs` muss vor dem Prefab-Bau auf das Figma-Design angepasst werden: neues Toggle-Feld fuer Spacing Mode, neue Slider-Felder, englische Labels.
- **Editor-Aufgabe:** `SonnensystemUIPanel.prefab` nach finalem Figma-Design bauen und im `PlacementManager` referenzieren.

### Globales Planet-InfoPanel / PlanetDetailRoot

- **Status:** Neues `PlanetDetailRoot` ist als bevorzugtes Detailpanel angebunden; Headset-Verifikation offen.
- **Beschreibung:** Es gibt ein einziges globales Detailpanel. Es wird neben dem User angezeigt und zeigt immer die Daten des aktuell ausgewaehlten Planeten.
- **Umsetzung:**
  - `PlanetInfoPanelManager.cs`: sucht automatisch `PlanetDetailRoot` und nutzt dieses Panel statt dem alten `PlanetInfoPanel Root`.
  - `PlanetInfoPanel.cs`: `Bind(PlanetData)` fuellt das neue Layout automatisch aus der Hierarchie (`ContentText`, `ContentRotSpeed`, `ContentRelSize`).
  - Anzeigen im neuen Layout: Name, Beschreibung, Planet Size/Durchmesser, Orbital Distance, Orbital Speed, Axial Tilt, Eccentricity und Planet Scale.
  - `PlanetData` enthaelt zusaetzliche Kurztextfelder fuer die Detailkarten: `planetSizeInfoText`, `orbitalDistanceInfoText`, `orbitalSpeedInfoText`, `axialTiltInfoText`, `eccentricityInfoText`, `planetScaleInfoText`, `gravityInfoText`.
  - Leere Kurztextfelder werden im Panel als `Lorem ipsum dolor sit amet.` angezeigt, damit fehlende Inhalte sichtbar bleiben.
  - `PlanetInfoPanelManager.PositionPanelNextToUser()` positioniert das Panel links-vorne relativ zur horizontalen Blickrichtung des Users, nicht relativ zur Kopfneigung. Aktuelle Werte: `distanceInFront = 0.45`, `sideOffset = -0.45`, `heightOffset = -0.6`.
  - Einzelplaneten werden nach dem Placement bevorzugt als generisches `planetInteractable.prefab` platziert. `InteractablePlanetVisual` setzt darunter im `VisualRoot` das eigentliche `PlanetData.planetPrefab` ein.
  - `PlanetSelectable.cs`: generische Komponente auf Planet-Objekten, die ihr `PlanetData` ans Panel meldet und als Fallback Daten aus `InteractablePlanetVisual` oder `PlanetBody` lesen kann.
  - `PlanetRaySelector.cs`: Controller-Ray-Auswahl per Trigger im `WORLD`-State. Treffer auf `PlanetSelectable`, `InteractablePlanetVisual` oder `PlanetBody` aktualisieren das globale `PlanetDetailRoot`; bei alter Layer-Maske faellt die Auswahl auf alle Layer zurueck und filtert trotzdem nur Objekte mit `PlanetData`.
  - `PlacementManager.cs`: fuegt nach Einzelplanet-Placement automatisch `PlanetSelectable` hinzu und zeigt das Detailpanel direkt mit den Daten des platzierten Planeten.
- **Entscheidung:** Kein eigenes Panel pro Planet. Layout und Logik bleiben zentral, Inhalte kommen aus `PlanetData`.
- **Datenbasiertes Prinzip:** Das Detailpanel soll ohne planetenspezifischen Code erweiterbar bleiben. Neue Planeten bekommen nur ein `PlanetData`-Asset mit Daten und Texten; UI und Placement reagieren generisch auf dieses Asset.

### Immersive Mode: Planet am Mond-Ort

- **Status:** Code umgesetzt, Editor-Setup/Headset-Verifikation offen.
- **Beschreibung:** Aus dem Learn-Flow heraus kann der User nach dem Platzieren eines Planeten ueber das Detail-Panel in eine VR-Natur-/Nachtszene wechseln. Dort wird der ausgewaehlte Planet an einem weit entfernten SpawnPoint angezeigt, als wuerde er an der Stelle des Mondes stehen.
- **Umsetzung:**
  - `ImmersiveModeController.cs` aktiviert ein vorhandenes `ImmersiveModeRoot`/`ImmersiveSceneRoot`/`OutdoorScene`-Root oder nutzt ein referenziertes Root aus dem Inspector.
  - Der Planet wird aus `PlanetData.planetPrefab` am `PlanetSpawnPoint`, `ImmersivePlanetSpawnPoint`, `SpawnPoint` oder `MoonAnchor` instanziiert.
  - `PlanetBody` und `PlanetSelectable` werden auf der Immersive-Kopie deaktiviert, damit keine Orbit-/Ray-Auswahl-Logik in die Ansicht hineinspielt.
  - Passthrough wird ueber den vorhandenen `PassthroughDissolver` nach VR gedissolved, statt eine neue Szene zu laden.
  - Das normale platzierte Learn-/Sonnensystem-Objekt wird ueber `PlacementManager.SetPlacedContentVisible(false)` ausgeblendet und beim Verlassen wieder eingeblendet.
  - Das InfoPanel bleibt sichtbar und dient gleichzeitig als Exit-UI: Button-Text `Leave Immersive Mode`.
- **Scaling:**
  - Standard ist `calculateMoonDiameterFromSpawnDistance = true`: Die sichtbare Mondgroesse wird aus der echten Entfernung zwischen Kamera und SpawnPoint berechnet.
  - Formel fuer den Mond-Durchmesser im World-Space: `2 * distanceToSpawnPoint * tan(moonAngularDiameterDegrees / 2)`.
  - Planetendurchmesser: `moonVisualDiameter * (planetData.diameter / 3474.8f)`.
  - Bei einem SpawnPoint um ca. 57.2 km Entfernung ergibt sich ein Mond-Vergleichsdurchmesser von ca. 499 m; Erde ca. 1830 m, Jupiter ca. 20096 m.
  - Die Kamera-`farClipPlane` wird im Immersive Mode temporaer an die SpawnPoint-Entfernung angepasst und beim Verlassen zurueckgesetzt.
- **Editor-Aufgabe:** Immersive-Environment in der `MainScene` unter/als Immersive-Root vorbereiten, SpawnPoint sinnvoll platzieren und den App-Flow in der `MainScene` testen.

### State Machine

- **Status:** Aktiv genutzt.
- **Beschreibung:** Der App-Flow nutzt `MAIN_MENU`, `PLACEMENT`, `WORLD`, `IMMERSIVE` und die Test-Zustaende `TEST_REIHENFOLGE`, `TEST_SIZE`, `TEST_GRAVITY_PLACEHOLDER`.
- **Umsetzung:** `GameManager.cs`
  - `MAIN_MENU`: Hauptmenue-Canvas sichtbar.
  - `PLACEMENT`: Hauptmenue ausgeblendet, `PlacementManager` zeigt Vorschau am Depth-Raycast.
  - `WORLD`: Objekt steht, Planet-Ray-Auswahl ist aktiv, Options-Taste oeffnet das Hauptmenue wieder.
  - `IMMERSIVE`: Hauptmenue ausgeblendet, Immersive-Root aktiv; Options-Taste verlaesst den Immersive Mode sauber.
  - `TEST_*`: Hauptmenue ausgeblendet, aktives Minigame-Prefab ist sichtbar; Options-Taste beendet das Minigame und fuehrt ins Test-Menue zurueck.
- **Hinweis:** `PlanetRaySelector` reagiert nur im `WORLD`-State, damit er nicht mit dem Placement-Trigger kollidiert.

> [NEU 2026-05-14 - START: Startsequenz und Tutorial]
### Startsequenz / Tutorial

- **Status:** Code umgesetzt; finale Tutorial-Step-Prefabs, Videos und Headset-Verifikation pruefen.
- **Beschreibung:** Die App kann vor dem Hauptmenue eine kurze Startsequenz zeigen. Nach Logo/Splash laeuft optional ein Tutorial mit kurzen Erklaervideos und einfachen Interaktionsschritten. Erst danach wird ins Passthrough-Hauptmenue gewechselt.
- **Umsetzung:**
  - `GameManager.cs` startet bei `playStartupSequence` die Startsequenz, blendet Passthrough zunaechst aus und zeigt `startupOnlyObjects` bzw. Partikel.
  - `TutorialController.cs` verwaltet eine Reihenfolge von Step-Prefabs, platziert sie einmalig vor dem User und wartet pro Schritt auf ein Abschlusssignal.
  - `TutorialStepSignal.cs` startet vorhandene `VideoPlayer` im Loop und bietet Methoden fuer Button-, Grab- oder Unselect-Events (`CompleteStep()`, `OnGrabStart()`, `OnGrabEnd()`).
  - `TutorialMenuGestureDetector.cs` erkennt fuer den Menue-Schritt optional die Start/Menu-Taste oder eine einfache Handpose als Fallback.
- **Didaktischer Zweck:** Das Tutorial soll die wichtigsten XR-Bedienhandlungen niedrigschwellig vorbereiten, damit die eigentliche Lern- und Testphase weniger durch Unsicherheit bei Controller, Menue oder Objektinteraktion belastet wird.
> [NEU 2026-05-14 - ENDE]

> [NEU 2026-05-14 - START: User-Testing-Vorbereitungsmodus]
### User-Testing-Vorbereitungsmodus

- **Status:** Code umgesetzt; im finalen Test-Build im Headset pruefen.
- **Beschreibung:** Fuer die Testsituation gibt es einen versteckten Vorbereitungsmodus. Wenn beide Controller-Thumbsticks innerhalb eines kurzen Zeitfensters gleichzeitig gedrueckt werden, wechselt die App in eine ruhige Passthrough-Ansicht. Die Testperson sieht zuerst ihre reale Umgebung durch die Brille, waehrend die Testleitung Sitz, Headset-Position, IPD/Tragekomfort und Orientierung pruefen kann.
- **Umsetzung:**
  - `GameManager.cs` enthaelt den Bereich `User Testing Setup`.
  - `enableUserTestingShortcut` aktiviert/deaktiviert den Shortcut.
  - `userTestingShortcutWindow` definiert das Zeitfenster fuer den gemeinsamen Thumbstick-Klick.
  - `EnterUserTestingSetup()` stoppt die Startsequenz, setzt Minigame-Fortschritt und platzierte Inhalte zurueck, pausiert Audio, blendet nicht essenzielle Root-Objekte aus, aktiviert Passthrough sofort und zeigt optional Logo-Objekte.
  - Ein erneuter Shortcut ruft `StartGameFromUserTestingSetup()` auf: Logo wird ausgeblendet, Passthrough kurz nach VR gedissolved, Root-Objekte und Audio werden wiederhergestellt und anschliessend startet die normale Startsequenz bzw. das Hauptmenue.
- **Warum:** Der Modus reduziert kognitive Belastung vor dem eigentlichen Test. Die Person muss nicht gleichzeitig die Brille ausrichten, die reale Umgebung verstehen, Controller finden und bereits App-Inhalte verarbeiten. Fuer die Evaluation ist das wichtig, weil Bedien- und Startstress sonst die Wahrnehmung von Lernwirkung, Motivation und Usability verzerren koennte.
> [NEU 2026-05-14 - ENDE]

### Passthrough / VR Toggle

- **Status:** Aktiv implementiert im Main Menu.
- **Beschreibung:** Der User kann im Hauptmenue ueber einen Toggle zwischen AR-Modus (Passthrough: sieht den echten Raum) und VR-Modus (schwarzer Hintergrund) wechseln. Das Panel bleibt im Raum stehen; nur der Hintergrund der Welt wechselt.
- **Umsetzung:**
  - `MainMenuController.cs` hat `_passthroughModeToggle` (Unity Toggle), `_passthroughDissolver` (MR Motifs `PassthroughDissolver`) und `_isPassthroughOnAtStart` (Startzustand).
  - `SetPassthroughMode(bool isActive)` ruft `_passthroughDissolver.SetPassthroughActive(isActive)` auf.
  - Das Toggle-Objekt wird automatisch aus dem `MenuRoot` gesucht (Name: "Passthrough"); alternativ im Inspector zuweisbar.
  - Der `PassthroughDissolver` stammt aus dem MR Motifs-Paket und koordiniert OVR-Passthrough-Layer und Kamera-Flags.
  - `_isPassthroughOnAtStart` steuert den Zustand beim App-Start (Standard: AR-Modus).
- **Hinweis:** Der `PassthroughDissolver` muss in der MainScene als Komponente vorhanden sein; `MainMenuController` sucht ihn per `FindFirstObjectByType` als Fallback.

### Platzierung per Depth API

- **Status:** Code fertig; Editor-Playmode nutzt einen Depth-freien Fallback; Headset-Input wurde robuster gemacht.
- **Beschreibung:** Im `PLACEMENT`-State folgt eine Vorschau dem Controller-Ray. Eine horizontale Flaeche wird ueber die Hit-Normal erkannt; Trigger platziert das Objekt.
- **Umsetzung:** `PlacementManager.cs`
  - Quest-Build: Raycast ueber `EnvironmentRaycastManager.Raycast(Ray, out hit)`.
  - Unity Editor: `useEditorFallbackPlacement` setzt den Hitpunkt in fixer Distanz vor den Controller-Ray und deaktiviert den `EnvironmentRaycastManager`, damit die Depth API im Playmode nicht laeuft.
  - `IsHorizontal(normal)`: `Vector3.Dot(normal, Vector3.up) >= horizontalSurfaceThreshold`, aktuell `0.75`.
  - Placement akzeptiert rechten und linken Index-Trigger (`SecondaryIndexTrigger`, `PrimaryIndexTrigger`) sowie optional Raw-Trigger (`RIndexTrigger`, `LIndexTrigger`).
  - Wenn der Trigger gedrueckt wird, die Flaeche aber nicht horizontal genug ist, loggt der `PlacementManager` die blockierende Normal.
  - `SelectPlanet(PlanetData)`: Vorschau aus `PlanetData.previewPrefab`; das echte Objekt kommt ueber `PlanetData.interactablePlanetPrefab`, sonst ueber das gemeinsame `PlacementManager.interactablePlanetPrefab`, sonst als Fallback direkt aus `PlanetData.planetPrefab`.
  - `SetupPlacedPlanetVisual(...)`: bindet beim generischen Interactable-Wrapper `InteractablePlanetVisual.PlanetData` an den gewaehlten Planeten und laedt dessen echtes `planetPrefab` in den `VisualRoot`.
  - `SelectSolarSystem(GameObject)`: Sonnensystem-Prefab ohne Laufzeit-Skalierung.
  - Beim Platzieren eines Planeten wird das globale InfoPanel aktualisiert.
  - Beim Platzieren eines Sonnensystems wird die Sonnensystem-Slider-UI gebunden und das Planet-InfoPanel versteckt.

### Main Menu / MenuRoot

- **Status:** Finales `MenuRoot` ist aktiv angebunden; altes `Main Menu`/`MainMenuCanvas` wurde aus der `MainScene` entfernt.
- **Beschreibung:** Das Hauptmenue nutzt das neue `MenuRoot` mit Tab-System. Inaktive Tabs blenden nur ihren Background aus.
- **Umsetzung:**
  - `GameManager.mainMenuCanvas` zeigt auf `MenuRoot`.
  - `MainMenuController.cs` bindet `MenuRoot` zur Laufzeit automatisch: Navigation, Hauptpanel, Planet-Buttons, Sonnensystem-Button und Challenge-Buttons.
  - Planet-Buttons werden aus `menuPlanetData` in Button-Reihenfolge befuellt und rufen generisch `SelectPlanet(PlanetData)` auf.
  - Der `Start Experience`-Button ist initial versteckt und wird erst nach Auswahl eines Planeten oder des Sonnensystems eingeblendet.
  - Active Tab Background: `#FFFFFF` mit Alpha `0.4`; inactive: Alpha `0`.
  - `MainMenuController.cs` startet Minigames ueber `MinigameManager` und faerbt Quiz-Buttons bei abgeschlossenem Quiz um.
  - `GameManager` blendet `MenuRoot` beim Placement/WORLD/TEST-State aus.
- **Datenbasiertes Prinzip:** Das Menue soll ebenfalls datenbasiert bleiben. Neue Planeten werden ueber `PlanetData` und die `menuPlanetData`-Liste ergaenzt, nicht durch planetenspezifische UI-Methoden.

### Test-Tab / Minigame-Skelett

- **Status:** Phase 4 fertig; Reihenfolge-Minispiel ist als echtes Snap-Minispiel in Umsetzung und code-seitig vorbereitet.
- **Beschreibung:** Der Test-Tab startet drei Modi: `Reihenfolge`, `Size` und `Gravity (Coming Soon)`.
- **Umsetzung:**
  - `MinigameManager.cs` instanziiert pro aktivem Modus genau ein Minigame-Prefab und positioniert es vor dem User.
  - `Minigame_Reihenfolge.prefab` und `Minigame_Size.prefab` enthalten eigene World-Space-UIs aus dem Editor; es wird keine UI mehr per Code erzeugt.
  - `MinigameUIActions.cs` stellt Button-Methoden fuer Prefab-UIs bereit: `CompleteAndReturnToMenu()` und `ExitWithoutCompleting()`.
  - `CompleteCurrentMinigame()` speichert den Erfolg via `PlayerPrefs`; der jeweilige Quiz-Button zeigt vorlaeufig "(geschafft)" und eine andere Farbe.
  - `ResetQuizProgress()` setzt `Reihenfolge` und `Size` im Main Menu wieder auf offen, als waeren beide Quizzes noch nicht gemacht.
  - Beim Start von `Reihenfolge` aktiviert `ReihenfolgeControllerHandMode` temporaer Meta `controllerDrivenHandPosesType = Natural`, damit Controller wie in den Meta SnapExamples Hand-Grabs ausloesen koennen. Beim Verlassen wird der vorherige Modus wiederhergestellt.
- **Wichtig:** World-Space-Canvases in Minigame-Prefabs muessen im Editor mit `Interaction SDK > Add Ray Interaction to Canvas` vorbereitet werden.

### Reihenfolge-Minispiel

- **Status:** Funktionslogik code-seitig vorbereitet; finale Headset-Verifikation offen.
- **Beschreibung:** Planeten werden aus einer Liste gegriffen und auf die richtigen Umlaufbahn-Ringe gesnappt. Jeder Ring prueft, ob der richtige Planet auf ihm liegt, und gibt visuelles Feedback.
- **Umsetzung:**
  - Grundlage ist das Meta Interaction SDK `SnapExamples`-Setup, nicht eine selbst geschriebene Grab-/Snap-Logik.
  - `Minigame_Reihenfolge.prefab` enthaelt die kopierten Orbit-/Snap-Elemente und die snappable Planeten.
  - `ReihenfolgePlanet.cs` haelt `PlanetData`, `OrbitIndex`, `Grabbable`, `Rigidbody` und den `SnapInteractor`.
  - `ReihenfolgeOrbitSlot.cs` liest den aktuellen Planet aus `SnapInteractable.SelectingInteractorViews`, prueft den `OrbitIndex` und faerbt den Ring.
  - `ReihenfolgeChecker.cs` sammelt Slots/Planeten automatisch aus den Kindern des Prefabs und bewertet, ob alle aktiven Slots korrekt belegt sind.
  - `InteractablePlanetVisual.cs` erzeugt das Planet-Visual unter einem `VisualRoot`; lokale Position und Rotation sind im Inspector anpassbar.
- **Wichtig:** Das Prefab bleibt manuell im Editor aufgebaut. `ReihenfolgeMinigameBuilder` ist im Prefab deaktiviert, damit der manuelle SnapExamples-Aufbau nicht zur Laufzeit ueberschrieben wird.

### Size-Minispiel

- **Status:** Grundlogik und Prefab-Anbindung umgesetzt; finale Headset-Verifikation offen.
- **Beschreibung:** Planeten werden aus der Meta-Snap-Liste auf Groessen-Slots gelegt. Korrekt platzierte Planeten wachsen sofort auf ihre relative Zielgroesse an, falsch platzierte Planeten fliegen nach kurzer Wartezeit zur Liste zurueck.
- **Umsetzung:**
  - `SizeChecker.cs` sammelt Planeten und Slots aus `Minigame_Size.prefab`, prueft die Belegung live und nutzt `PlanetData.diameter` fuer die erwartete Reihenfolge.
  - Die Zielgroesse wird relativ zu einem zentralen Referenzwert berechnet: aktuell `EarthSizeMeters = 0.04`, also Erde = 4 cm.
  - Sobald ein Planet korrekt auf seinem Slot liegt, wird das sichtbare `InteractablePlanetVisual.VisualRoot` skaliert und alle `Rigidbody`/`Collider` des Planeten deaktiviert, damit spaetere Ueberlappungen ihn nicht wegschieben.
  - Falsch platzierte Planeten behalten kurzes Rot-Feedback und werden nach `WrongReturnDelay` wieder zur Listenflaeche zurueck animiert.
  - `FinishButtonPressed()` liegt jetzt auf `SizeChecker`; der Fertig-Button darf das Minispiel nicht mehr direkt ohne korrekte Loesung abschliessen.
  - `SizeMinigameWorldLayout.cs` positioniert `List` und Slot-/Orbit-Gruppe beim Spawn automatisch vor dem User, parallel zum Boden und auf ca. 1.10 m Hoehe.
- **Hinweis:** Die sichtbare Groessenanpassung passiert am Visual-Root des jeweiligen Interactable-Planeten, nicht am Grab-/Snap-Root.

### Meta Snap-Listen in Minigames

- **Status:** Aktiv genutzt in `Minigame_Reihenfolge.prefab` und als Vorlage fuer `Minigame_Size.prefab`.
- **Beschreibung:** Die automatisch wachsende Planeten-Liste kommt aus der Meta Interaction SDK `SnapExamples`-Szene. Die Planeten werden nicht per eigenem Layout-Script verteilt, sondern ueber SDK-Snap-Komponenten dynamisch in einer Liste angeordnet.
- **Umsetzung:**
  - `List > SnapInteractable` enthaelt einen `SnapInteractable` und einen `ListSnapPoseDelegate`.
  - Im `SnapInteractable` ist `Snap Pose Delegate` auf den lokalen `ListSnapPoseDelegate` gesetzt.
  - `List > Border` enthaelt `ListSnapPoseDelegateRoundedBoxVisual`; dieses Visual liest die aktuelle Listengroesse aus dem `ListSnapPoseDelegate` und skaliert den Rahmen automatisch.
  - Jeder Planet/Kugel hat einen `SnapInteractor` mit `Default Interactable` und `Time Out Interactable` auf `List > SnapInteractable`.
  - Beim Start oder nach Timeout snappen die Planeten dadurch automatisch in die Liste. Beim Hinzufuegen/Entfernen registriert der `ListSnapPoseDelegate` die aktiven Interactors und berechnet neue Positionen entlang der lokalen X-Achse der Liste.
- **Wichtig:** Wenn die Planeten erst nach Beruehrung/Pinch in die Liste fliegen, aber nicht automatisch beim Start, ist die Liste selbst meist korrekt. Dann fehlen wahrscheinlich auf den Planeten-`SnapInteractor`s die Referenzen `Default Interactable` und/oder `Time Out Interactable` auf das Listen-`SnapInteractable`.

### Minigame-Spawn und Controller-Hand-Modus

- **Status:** Aktiv genutzt.
- **Beschreibung:** Snap-Minispiele brauchen neben dem Meta-Snap-Setup auch die passende Spawn-Position im Raum und temporaer den Meta-Controller-Hand-Modus.
- **Umsetzung:**
  - `MinigameManager.cs` behandelt `Reihenfolge` und `Size` jetzt gleich fuer den Controller-Hand-Modus.
  - `ReihenfolgeControllerHandMode.cs` bleibt das zentrale Umschalt-Script und wird fuer beide Snap-Minispiele verwendet.
  - `SizeMinigameWorldLayout.cs` richtet das `Size`-Prefab relativ zur Hauptkamera aus, damit Liste und Slots nicht von einer zufaelligen Prefab-Authoring-Position abhaengen.

### Planet Shader, SunPasser und Cloud-Animation

- **Status:** Weiterhin aktiv.
- **Beschreibung:** Planeten-Materialien mit dynamischer Beleuchtung und Atmosphaeren-Effekt; Erde hat eine rotierende Wolkenschicht.
- **Umsetzung:** `SunPasser.cs`, `M_Planet.shadergraph`, `CloudBehaviour.cs`.

### Desktop-Testing Tools

- **Status:** Vorhanden, nicht fuer Quest-Build zentral.
- **Beschreibung:** Hilfsscripts fuer Entwicklung ohne Headset: `DesktopDebugCamera.cs`, `DesktopPlacement.cs`, `PlacementTester.cs`.

---

## Wichtige technische Konventionen

| Konvention | Detail |
|---|---|
| Datenquelle Planeten | Planetenspezifische Daten liegen in `PlanetData`, nicht in UI- oder Interaktionsscripts. |
| Datenbasierte UI | Menue und Detailpanel lesen Daten aus `PlanetData` und sollen ohne planetenspezifischen Code erweiterbar bleiben. |
| Globales InfoPanel | Ein `PlanetInfoPanel` fuer alle Planeten; Umschalten per `Bind(PlanetData)`. Das neue `PlanetDetailRoot` wird bevorzugt. |
| PlanetDetail-Kurztexte | Detailkarten nutzen Kurztextfelder in `PlanetData`; leere Felder fallen sichtbar auf Lorem Ipsum zurueck. |
| Interactable-Einzelplaneten | Platzierte Einzelplaneten nutzen bevorzugt das gemeinsame `planetInteractable.prefab`; das echte visuelle Planet-Prefab wird per `InteractablePlanetVisual` aus `PlanetData.planetPrefab` eingesetzt. |
| Detailpanel-Position | Das Detailpanel spawnt links-vorne relativ zur horizontalen Blickrichtung des Users, nicht relativ zur Kopfneigung. |
| Immersive Mode | Kein Scene-Loading; Immersive-Environment bleibt Teil der MainScene und wird ueber Root-GameObject + Dissolve aktiviert. |
| Immersive Spawn | Planet wird am referenzierten SpawnPoint angezeigt; bei grosser Distanz berechnet `ImmersiveModeController` die Mond-Scheingroesse aus dem Spawn-Abstand. |
| Passthrough Toggle | Der AR/VR-Toggle im Main Menu steuert den `PassthroughDissolver` (MR Motifs). Das Toggle-Objekt in `MenuRoot` heisst "Passthrough". Der Startzustand ist per `_isPassthroughOnAtStart` im Inspector steuerbar. |
| **NEU 2026-05-14 START** | Konventionen Tutorial/User Testing |
| Startup-Tutorial | Kurze Erklaervideos und Interaktionsschritte laufen optional in der Startsequenz ueber `TutorialController`; Step-Prefabs melden ihren Abschluss ueber `TutorialStepSignal`. |
| User-Testing-Setup | Beide Controller-Thumbsticks gleichzeitig schalten in eine Passthrough-Vorbereitungsansicht; der Modus dient nur Testleitung/Onboarding und ist kein regulaeres Lernfeature. |
| **NEU 2026-05-14 ENDE** | Konventionen Tutorial/User Testing |
| Einzelplanet-Groessenmodus | Einzelplaneten werden immer relativ zur echten Planetengroesse skaliert. Der fruehere Main-Menu-Toggle fuer gleich grosse 50-cm-Planeten ist verworfen und wird im Code ignoriert/ausgeblendet. |
| Sonnensystem-UI | Das Slider-Panel referenziert den `SolarSystemManager` nicht im Prefab, sondern bekommt ihn nach Placement ueber `Bind(SolarSystemManager)`. |
| Depth API fuer Platzierung | Im Quest-Build liefert `EnvironmentRaycastManager.Raycast(Ray, out hit)` Position und Normal aus dem Depth-Mesh. |
| Editor-Placement | Im Unity Editor ist die Depth API deaktiviert; die Platzierung nutzt einen festen Punkt vor dem Controller-Ray. |
| Horizontalitaet | Boden/Tisch wird ueber `Vector3.Dot(normal, Vector3.up) >= horizontalSurfaceThreshold` erkannt; aktuell `0.75`. |
| Placement-Input | Placement akzeptiert beide Index-Trigger plus optional Raw-Index-Trigger, damit linke/rechte Controller-Hand nicht blockiert. |
| Skalierung Einzelplanet | `vrScale = (diameter / earthDiameterInKm) * earthDiameterInVR`. |
| Skalierung Sonnensystem | Das Sonnensystem-Prefab bleibt in eigener VR-Groesse; Runtime-Slider veraendern interne Manager-Werte. |
| State-Grenze | Placement-Trigger und Planet-Auswahl werden durch `GameState.PLACEMENT` vs. `GameState.WORLD` getrennt. |
| Minigame-UI | Minigame-UIs werden als Prefab-Inhalt im Editor gebaut, nicht zur Laufzeit per Code erzeugt. |
| Reihenfolge-Interaktion | Reihenfolge nutzt Meta SnapExamples-Komponenten. Eigener Code prueft nur Daten/Feedback, nicht das Greifen/Snappen selbst. |
| Snap-Listen | Dynamische Planeten-Listen nutzen `ListSnapPoseDelegate` + `ListSnapPoseDelegateRoundedBoxVisual` aus den Meta SnapExamples; automatische Startplatzierung braucht `Default Interactable`/`Time Out Interactable` auf den Planeten-`SnapInteractor`s. |
| Controller-Hand-Modus | `controllerDrivenHandPosesType = Natural` wird waehrend der Snap-Minispiele `Reihenfolge` und `Size` gesetzt und danach wiederhergestellt. |
| Size-Wachstum | Die Echtgroessen-Darstellung im Size-Minispiel skaliert das sichtbare `InteractablePlanetVisual.VisualRoot`, nicht den Interactable-Root. |
| Size-Spawn | `Minigame_Size.prefab` darf als Prefab global instanziiert werden; die sinnvolle Raumpositionierung von Liste und Slots passiert danach zur Laufzeit ueber `SizeMinigameWorldLayout`. |

---

## Aktuelle Entscheidungen

| Datum | Entscheidung | Begruendung |
|---|---|---|
| 2026-04-12 | Meta Building Blocks bevorzugt | Einfacher, schneller, weniger eigener Code |
| 2026-04-12 | Code auf einfachem Level halten | Verstaendlichkeit und eigene Bearbeitbarkeit |
| 2026-04-12 | Code-Kommentare auf Deutsch | Muttersprache, einfacher fuer Doku |
| 2026-04-12 | PlanetData als ScriptableObject | Trennung von Daten und Logik; einfach erweiterbar ohne Code-Aenderung |
| 2026-04-26 | Platzierung ueber Depth API statt MRUK-Room | Stabiler fuer reine Boden-/Tisch-Platzierung, weniger SDK-Bruchpunkte |
| 2026-04-26 | State Machine auf 3 Zustaende reduziert | Der reduzierte Flow deckt den aktuellen Wireframe ab |
| 2026-04-27 | Ein globales Planet-InfoPanel statt Panel pro Planet | Weniger Prefab-Duplizierung, besser wartbar, passt zum `PlanetData`-Ansatz |
| 2026-04-27 | Sonnensystem-Slider-UI bindet Runtime-Instanz | Der `SolarSystemManager` existiert erst nach Placement; Inspector-Referenz im UI-Prefab waere falsch |
| 2026-04-27 | Editor-Placement ohne Depth API | Die Depth API macht den Playmode auf Windows traege; UI- und Flow-Tests sollen ohne Quest-Build moeglich sein |
| 2026-04-27 | Minigame-UIs nicht per Code erzeugen | Meta Interaction SDK Canvas-Setup muss im Editor/Prefab passieren, damit Quest-Controller-Ray-Interaktion funktioniert |
| 2026-04-27 | Reihenfolge-Minispiel auf Meta SnapExamples aufbauen | Snap/Grab ist im SDK bereits geloest; eigener Code bleibt auf Planetendaten, Slot-Pruefung und Feedback beschraenkt |
| 2026-04-27 | Controller-Hand-Modus nur fuer Snap-Minispiele aktivieren | Controller sollen in anderen Szenen sichtbar und ray-faehig bleiben; nur `Reihenfolge` und `Size` brauchen controller-driven hand poses |
| 2026-04-27 | Snap-Listen aus Meta SnapExamples uebernehmen | `ListSnapPoseDelegate` loest die automatische Anordnung und Groessenanpassung bereits; eigener Layout-Code waere unnoetig und fehleranfaelliger |
| 2026-04-28 | Size-Minispiel bewertet ueber einen zentralen Manager statt ueber den Fertig-Button | Die Loesung soll live pruefbar sein, den Fertig-Button gegen falsches Abschliessen absichern und planetenspezifisches Groessen-Feedback steuern |
| 2026-04-28 | Korrekt platzierte Size-Planeten skalieren am `VisualRoot` | Das sichtbare Planet-Visual liegt im Interactable-Prefab unter `InteractablePlanetVisual`; Root-Skalierung allein ist fuer das unmittelbare Feedback zu unzuverlaessig |
| 2026-04-28 | Immersive Mode bleibt in der MainScene und nutzt Dissolve statt Scene-Loading | Fruehere Raumstations-/Szenenwechsel-Ansatz fuehrte zu schwarzem Bildschirm, Ruckeln und komplexem XR-State; Root-Activation + Passthrough-Dissolve ist stabiler |
| 2026-04-28 | Immersive-Planet wird ueber scheinbare Mondgroesse skaliert | Echte Planetengroessen sind unbrauchbar; die visuelle Lernidee ist die korrekte scheinbare Groesse am Mond-Ort |
| 2026-04-30 | Passthrough/VR Toggle im Main Menu ueber `PassthroughDissolver` | Nutzer sollen wählen können, ob sie das Sonnensystem in AR (Raum sichtbar) oder VR (schwarzer Hintergrund) erleben; MR Motifs-Dissolver löst den Übergang bereits sauber |
| 2026-04-30 | Finales `MenuRoot` ersetzt altes `Main Menu` | Finales UI-Layout soll die Quelle sein; alte UI-Hierarchie erzeugte doppelte Systeme und falsche Referenzen |
| 2026-04-30 | `PlanetDetailRoot` nutzt bestehendes globales `PlanetInfoPanel` | Kein zweites InfoPanel-System; vorhandener Manager bleibt zentrale Stelle, neues Layout wird automatisch gebunden |
| 2026-04-30 | Planet-Detailsystem bleibt datenbasiert ueber `PlanetData` | Erweiterbarkeit: neue Planeten brauchen Daten/Textfelder im ScriptableObject, keine neuen UI-Scripts |
| 2026-04-30 | Placement-Input akzeptiert beide Controller-Trigger | Ghost war sichtbar, Placement konnte aber je nach Hand/Rig am falschen Trigger-Button haengen bleiben |
| 2026-04-30 | Einzelplanet-Placement nutzt generisches `planetInteractable.prefab` | Meta-Interactable-Komponenten bleiben zentral im Wrapper; das eigentliche Planet-Visual wird datenbasiert aus `PlanetData.planetPrefab` geladen |
| 2026-05-05 | Relative Einzelplanet-Groessen sind immer aktiv | Wissenschaftlich korrekte Groessenvergleiche sind fuer die Lernziele wichtiger als ein vereinfachter Gleichgroessen-Modus; alte Toggle-Events duerfen den Modus nach Immersive-Rueckkehr nicht mehr deaktivieren |
| 2026-05-06 | User Testing auf Massstabsverhaeltnisse fokussiert | Die Evaluation richtet sich jetzt explizit auf fachliche Nachvollziehbarkeit, raeumliche Erfahrbarkeit und spielerische Motivation statt auf allgemeinen Wissenszuwachs allein |
| **NEU 2026-05-14 START** | Entscheidungen Tutorial/User Testing | |
| 2026-05-14 | Startup-Tutorial mit kurzen Erklaervideos vor dem Hauptmenue | Bedienhandlungen sollen vor der eigentlichen Exploration kurz vorbereitet werden, ohne die Lernphase mit langen Erklaertexten zu ueberladen |
| 2026-05-14 | User-Testing-Vorbereitungsmodus per Doppel-Thumbstick | Testpersonen sollen zuerst Passthrough und reale Umgebung sehen; Brillenanpassung und Orientierung passieren vor dem eigentlichen Test, um kognitive Belastung und Startstress zu reduzieren |
| **NEU 2026-05-14 ENDE** | Entscheidungen Tutorial/User Testing | |

---

## Bekannte Probleme / Offene Editor-Aufgaben

| Aufgabe | Status |
|---|---|
| `MenuRoot` als alleiniges Hauptmenue in `MainScene` nutzen | Fertig, Headset-Test offen |
| Planet-Buttons in `menuPlanetData` gegen sichtbare Button-Reihenfolge pruefen | Offen |
| `PlanetInfoPanelSystem` in `MainScene` anlegen und `PlanetInfoPanelManager` verkabeln | Fertig, Manager bevorzugt automatisch `PlanetDetailRoot` |
| `PlanetDetailRoot` World-Space-Canvas mit TMP-Feldern an neues Datenbinding anbinden | Fertig, Lesbarkeit/Position im Headset pruefen |
| Neue `PlanetData`-Kurztexte fuer Detailkarten befuellen | Offen |
| `PlacementManager.planetInfoPanelManager` zuweisen | Fertig, Headset-Test offen |
| Immersive-Root/Environment in der `MainScene` final organisieren | In Arbeit |
| Immersive-SpawnPoint pruefen: Name/Referenz, Entfernung, Sichtbarkeit, Far Clip und Skalierung | In Arbeit |
| Build Settings pruefen: App-Flow muss aus `MainScene.unity` starten; reine `OutdoorScene` ist nur Environment/Test | Offen |
| Headset-Test: Planet platzieren -> InfoPanel -> Immersive Mode -> Planet am SpawnPoint sichtbar | Offen |
| Headset-Test: InfoPanel bleibt im Immersive Mode sichtbar und Button verlaesst den Modus | Offen |
| `PlanetRaySelector` mit richtigem Controller-`rayOrigin` in der Szene verkabeln | Vorhanden; Code-seitig fertig fuer beide Trigger, Interactable-Wrapper und Layer-Fallback. Headset-Verkabelung pruefen |
| `SonnensystemUI.cs` an Figma-Design anpassen: Spacing Mode Toggle + 5 Slider statt 4 Slider, englische Labels | Offen |
| `SonnensystemUIPanel.prefab` nach finalem Figma-Design bauen | Offen |
| `PlacementManager.sonnensystemUIPrefab` zuweisen | Offen |
| `MainMenuController`-Felder fuer `MenuRoot`, `menuPlanetData`, Minigames und SolarSystem-Prefab im Inspector pruefen | Offen |
| Test-Panel Phase 4: Buttons, Minigame-Prefabs, Abschliessen/Beenden, Reset-Fortschritt | Fertig |
| Reihenfolge-Prefab: alle Planeten, SnapInteractor und OrbitSlots final im Inspector pruefen | In Arbeit |
| Size-Prefab: Planeten-`SnapInteractor`s auf `List > SnapInteractable` als `Default Interactable` und `Time Out Interactable` pruefen | In Arbeit |
| Size-Prefab im Headset pruefen: korrekter Slot -> sofortige Groessenanpassung, Collider/Rigidbody aus | Offen |
| Size-Prefab im Headset pruefen: falscher Slot -> Rot-Feedback und Rueckflug nach 2 Sekunden | Offen |
| Size-Prefab im Headset pruefen: `List` und Slots werden vor dem User auf ca. 1.10 m Hoehe gespawnt | Offen |
| Headset-Test: Reihenfolge mit Controller greifen, snappen, Ring-Feedback und Abschluss pruefen | Offen |
| `UISetExamples.unity` und `PanelWithManipulators.unity` als Vorlage fuer VR-UI/Panel pruefen | Offen |
| Headset-Test: Erde platzieren -> InfoPanel sichtbar und lesbar | Offen |
| Headset-Test: Venus/Planet platzieren -> Ghost sichtbar -> beide Trigger platzieren korrekt | Offen |
| Headset-Test: Sonnensystem platzieren -> Slider sichtbar und wirksam | Offen |
| Finaler User-Testing-Build: Home-/Planet-Flow, Sonnensystem-Controls, Learn-/Test-Tab und Rueckwege einmal komplett im Headset durchlaufen | Offen |
| User Testing: Immersive Mode vorab testen und als Pflicht- oder optionale Aufgabe festlegen | Offen |
| **NEU 2026-05-14 START** | Offene Aufgaben Tutorial/User Testing |
| Tutorial-Step-Prefabs und Erklaervideos im Headset pruefen: Position, Lesbarkeit, Ton/Loop, Abschluss-Signale | Offen |
| User-Testing-Shortcut im finalen Build pruefen: beide Thumbsticks -> Passthrough-Setup, erneuter Shortcut -> Logo/Tutorial/Appstart | Offen |
| User-Testing-Setup pruefen: App-Inhalte, Minigames, Audio und platzierte Objekte werden sauber zurueckgesetzt bzw. wiederhergestellt | Offen |
| **NEU 2026-05-14 ENDE** | Offene Aufgaben Tutorial/User Testing |
| Collider/Layer auf `planetInteractable.prefab` pruefen, damit `PlanetRaySelector` sie treffen kann | Code-Fallback vorhanden; Prefab-Layer im Headset trotzdem pruefen |
| `StartPhase.cs`, `SpielerBewegung.cs`, `InfoPunkt.cs` auf Altlasten pruefen | Offen |

---

## Letzte technische Verifikation

- **2026-04-27:** `dotnet build Assembly-CSharp.csproj --no-restore` erfolgreich.
- **2026-04-27:** Reihenfolge-Code nach SnapExamples-Integration kompiliert: `dotnet build Assembly-CSharp.csproj --no-restore` erfolgreich.
- **2026-04-27:** Phase-4-Test-Flow im Playmode verifiziert: Quiz-Buttons starten passende Panels, Abschliessen speichert Erfolg, Beenden speichert nicht, Main-Menu-Reset loescht Fortschritt.
- **2026-04-28:** Size-Minispiel nach Phase-6-Integration kompiliert: `dotnet build Assembly-CSharp.csproj --no-restore` erfolgreich.
- **2026-04-28:** Immersive Mode nach InfoPanel-/SpawnPoint-/Dissolve-Integration kompiliert: `dotnet build Assembly-CSharp.csproj --no-restore` erfolgreich.
- **2026-04-30:** `MenuRoot`, `PlanetDetailRoot`, datenbasierte `PlanetData`-Detailtexte und Placement-Input-Fix kompiliert: `dotnet build Assembly-CSharp.csproj --no-restore` erfolgreich.
- **2026-04-30:** Interactable-Wrapper-Auswahl und `PlanetDetailRoot`-Update bei Planet-Klick kompiliert: `dotnet build Assembly-CSharp.csproj --no-restore` erfolgreich.
- Warnungen bleiben aus bestehenden UISet/OpenXR/Altlasten, keine neuen Compile-Fehler.
