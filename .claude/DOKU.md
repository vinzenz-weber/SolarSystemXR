# Dokumentation: Sonnensystem XR

> **Letzte Aktualisierung:** 2026-04-28

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
- Phase 1 ist erledigt: 8 Planeten-`PlanetData`-Assets sind vorhanden.
- Phase 2 ist code-seitig weitgehend vorbereitet, Editor-Verkabelung und Headset-Verifikation sind noch offen.
- Phase 3 ist in Umsetzung: Learn-Tab mit `Planets` und `SolarSystem`, globales Planet-InfoPanel, Sonnensystem-Slider-UI.
- Phase 4 ist erledigt: Test-Tab startet Minigame-Prefabs, Abschliessen/Beenden funktionieren, Quiz-Fortschritt kann im Main Menu zurueckgesetzt werden.
- Phase 6 ist in Umsetzung: `Size` nutzt jetzt einen eigenen Manager, Prefab-World-Layout, Live-Auswertung und planetenspezifisches Groessen-Feedback.

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

- **Status:** Script fertig, Prefab/Editor-Setup offen.
- **Beschreibung:** Ein World-Space-Slider-Panel erscheint nach dem Platzieren des Sonnensystems neben dem User und steuert genau die frisch platzierte Sonnensystem-Instanz.
- **Umsetzung:**
  - `SonnensystemUI.cs` hat `Bind(SolarSystemManager)` und sucht den Manager nur als Fallback.
  - `PlacementManager.cs` findet nach dem Placement den `SolarSystemManager` im platzierten Prefab und bindet das UI automatisch.
  - Optional kann ein `SonnensystemUI` bereits im Sonnensystem-Prefab liegen; alternativ wird ein `sonnensystemUIPrefab` aus dem `PlacementManager` instanziiert.
  - Slider: Distanz, Groesse, Zeit und Exzentrizitaet.
- **Editor-Aufgabe:** `SonnensystemUIPanel.prefab` mit 4 Slidern bauen und im `PlacementManager` referenzieren.

### Globales Planet-InfoPanel

- **Status:** Script fertig, Prefab/Editor-Setup offen.
- **Beschreibung:** Es gibt ein einziges globales InfoPanel. Es wird neben dem User angezeigt und zeigt immer die Daten des aktuell ausgewaehlten Planeten.
- **Umsetzung:**
  - `PlanetInfoPanel.cs`: `Bind(PlanetData)` fuellt Headline, Subheadline, Beschreibung, Fakten, Durchmesser, Schwerkraft und optional Bild.
  - `PlanetInfoPanelManager.cs`: verwaltet das eine Panel, zeigt/versteckt es und positioniert es neben dem User.
  - `PlanetSelectable.cs`: generische Komponente auf Planet-Objekten, die ihr `PlanetData` ans Panel meldet.
  - `PlanetRaySelector.cs`: Controller-Ray-Auswahl per Trigger im `WORLD`-State.
  - `PlacementManager.cs`: fuegt nach Einzelplanet-Placement automatisch `PlanetSelectable` hinzu und zeigt das InfoPanel direkt mit den Daten des platzierten Planeten.
- **Entscheidung:** Kein eigenes Panel pro Planet. Layout und Logik bleiben zentral, Inhalte kommen aus `PlanetData`.

### State Machine

- **Status:** Aktiv genutzt.
- **Beschreibung:** Der App-Flow nutzt `MAIN_MENU`, `PLACEMENT`, `WORLD` und die Test-Zustaende `TEST_REIHENFOLGE`, `TEST_SIZE`, `TEST_GRAVITY_PLACEHOLDER`.
- **Umsetzung:** `GameManager.cs`
  - `MAIN_MENU`: Hauptmenue-Canvas sichtbar.
  - `PLACEMENT`: Hauptmenue ausgeblendet, `PlacementManager` zeigt Vorschau am Depth-Raycast.
  - `WORLD`: Objekt steht, Planet-Ray-Auswahl ist aktiv, Options-Taste oeffnet das Hauptmenue wieder.
  - `TEST_*`: Hauptmenue ausgeblendet, aktives Minigame-Prefab ist sichtbar; Options-Taste beendet das Minigame und fuehrt ins Test-Menue zurueck.
- **Hinweis:** `PlanetRaySelector` reagiert nur im `WORLD`-State, damit er nicht mit dem Placement-Trigger kollidiert.

### Platzierung per Depth API

- **Status:** Code fertig; Editor-Playmode nutzt einen Depth-freien Fallback.
- **Beschreibung:** Im `PLACEMENT`-State folgt eine Vorschau dem Controller-Ray. Eine horizontale Flaeche wird ueber die Hit-Normal erkannt; Trigger platziert das Objekt.
- **Umsetzung:** `PlacementManager.cs`
  - Quest-Build: Raycast ueber `EnvironmentRaycastManager.Raycast(Ray, out hit)`.
  - Unity Editor: `useEditorFallbackPlacement` setzt den Hitpunkt in fixer Distanz vor den Controller-Ray und deaktiviert den `EnvironmentRaycastManager`, damit die Depth API im Playmode nicht laeuft.
  - `IsHorizontal(normal)`: `Vector3.Dot(normal, Vector3.up) > 0.85`.
  - `SelectPlanet(PlanetData)`: Vorschau und echtes Planet-Prefab aus `PlanetData`, Groessenskalierung relativ zur Erde.
  - `SelectSolarSystem(GameObject)`: Sonnensystem-Prefab ohne Laufzeit-Skalierung.
  - Beim Platzieren eines Planeten wird das globale InfoPanel aktualisiert.
  - Beim Platzieren eines Sonnensystems wird die Sonnensystem-Slider-UI gebunden und das Planet-InfoPanel versteckt.

### Main Menu Learn/Test

- **Status:** Learn-Flow weiter in Arbeit; Test-Tab-Skelett fuer Phase 4 fertig.
- **Beschreibung:** Das Hauptmenue wird auf die Roadmap-Struktur umgebaut:
  - `Learn`: `Planets` und `SolarSystem`
  - `Test`: `Reihenfolge`, `Size`, `Gravity`
- **Umsetzung:**
  - `MainMenuController.cs` verwaltet Auswahl zwischen Planet und Sonnensystem und startet Placement.
  - `PlanetMenuButton.cs` befuellt Planet-Buttons aus `PlanetData` und ruft `SelectPlanet(data)` auf.
  - `MainMenuController.cs` startet die Minigames ueber `MinigameManager` und faerbt Quiz-Buttons bei abgeschlossenem Quiz um.
  - `GameManager` blendet das Menue beim Placement/WORLD/TEST-State aus.
- **Offen:** Die sichtbare Learn-Canvas-Struktur in `MainScene.unity` muss im Editor final auf `Planets` + `SolarSystem` angepasst werden.

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
| Globales InfoPanel | Ein `PlanetInfoPanel` fuer alle Planeten; Umschalten per `Bind(PlanetData)`. |
| Sonnensystem-UI | Das Slider-Panel referenziert den `SolarSystemManager` nicht im Prefab, sondern bekommt ihn nach Placement ueber `Bind(SolarSystemManager)`. |
| Depth API fuer Platzierung | Im Quest-Build liefert `EnvironmentRaycastManager.Raycast(Ray, out hit)` Position und Normal aus dem Depth-Mesh. |
| Editor-Placement | Im Unity Editor ist die Depth API deaktiviert; die Platzierung nutzt einen festen Punkt vor dem Controller-Ray. |
| Horizontalitaet | Boden/Tisch wird ueber `Vector3.Dot(normal, Vector3.up) > 0.85` erkannt. |
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

---

## Bekannte Probleme / Offene Editor-Aufgaben

| Aufgabe | Status |
|---|---|
| `PlanetInfoPanelSystem` in `MainScene` anlegen und `PlanetInfoPanelManager` verkabeln | Offen |
| `PlanetInfoPanel` World-Space-Canvas/Prefab mit TMP-Feldern bauen | Offen |
| `PlacementManager.planetInfoPanelManager` zuweisen | Offen |
| `PlanetRaySelector` mit richtigem Controller-`rayOrigin` in der Szene verkabeln | Offen |
| `SonnensystemUIPanel.prefab` mit 4 Slidern bauen | Offen |
| `PlacementManager.sonnensystemUIPrefab` zuweisen | Offen |
| `MainMenuController`-Felder fuer Learn/Test und SolarSystem-Prefab im Inspector pruefen | Offen |
| Learn-Panel im Editor auf `Planets` + `SolarSystem` umbauen | Offen |
| Test-Panel Phase 4: Buttons, Minigame-Prefabs, Abschliessen/Beenden, Reset-Fortschritt | Fertig |
| Reihenfolge-Prefab: alle Planeten, SnapInteractor und OrbitSlots final im Inspector pruefen | In Arbeit |
| Size-Prefab: Planeten-`SnapInteractor`s auf `List > SnapInteractable` als `Default Interactable` und `Time Out Interactable` pruefen | In Arbeit |
| Size-Prefab im Headset pruefen: korrekter Slot -> sofortige Groessenanpassung, Collider/Rigidbody aus | Offen |
| Size-Prefab im Headset pruefen: falscher Slot -> Rot-Feedback und Rueckflug nach 2 Sekunden | Offen |
| Size-Prefab im Headset pruefen: `List` und Slots werden vor dem User auf ca. 1.10 m Hoehe gespawnt | Offen |
| Headset-Test: Reihenfolge mit Controller greifen, snappen, Ring-Feedback und Abschluss pruefen | Offen |
| `UISetExamples.unity` und `PanelWithManipulators.unity` als Vorlage fuer VR-UI/Panel pruefen | Offen |
| Headset-Test: Erde platzieren -> InfoPanel sichtbar und lesbar | Offen |
| Headset-Test: Sonnensystem platzieren -> Slider sichtbar und wirksam | Offen |
| Collider auf Planet-Prefabs pruefen, damit `PlanetRaySelector` sie treffen kann | Offen |
| `StartPhase.cs`, `SpielerBewegung.cs`, `InfoPunkt.cs` auf Altlasten pruefen | Offen |

---

## Letzte technische Verifikation

- **2026-04-27:** `dotnet build Assembly-CSharp.csproj --no-restore` erfolgreich.
- **2026-04-27:** Reihenfolge-Code nach SnapExamples-Integration kompiliert: `dotnet build Assembly-CSharp.csproj --no-restore` erfolgreich.
- **2026-04-27:** Phase-4-Test-Flow im Playmode verifiziert: Quiz-Buttons starten passende Panels, Abschliessen speichert Erfolg, Beenden speichert nicht, Main-Menu-Reset loescht Fortschritt.
- **2026-04-28:** Size-Minispiel nach Phase-6-Integration kompiliert: `dotnet build Assembly-CSharp.csproj --no-restore` erfolgreich.
- Warnungen bleiben aus bestehenden UISet/OpenXR/Altlasten, keine neuen Compile-Fehler.
