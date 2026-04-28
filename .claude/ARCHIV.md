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

### Immersive Mode: Raumstation/Szenenwechsel -> MainScene-Dissolve mit Natur-Root

- **Datum:** 2026-04-28
- **Vorher:** Fuer den Immersive Mode gab es bereits den Ansatz, eine Raumstation bzw. eine eigene Immersive-Szene zu laden. Teilweise wurde dafuer eine neue Szene verwendet bzw. additiv geladen. Die Idee war, den Immersive-Kontext sauber von der MainScene zu trennen.
- **Problem:** Der Szenenwechsel funktionierte im XR-Kontext nicht stabil genug. Beim Laden traten schwarze Bildschirme, Ruckeln und Timing-/State-Probleme auf. Ausserdem wurde der App-Flow schwerer wartbar, weil Kamera-Rig, Passthrough, UI, GameManager und geladene Szene gleichzeitig koordiniert werden mussten.
- **Nachher:** Der Immersive Mode bleibt in der bestehenden `MainScene`. Ein vorbereiteter Immersive-/Outdoor-Root wird per `SetActive` eingeblendet, Passthrough wird ueber den vorhandenen `PassthroughDissolver` nach VR gedissolved, und der gewaehlte Planet wird am vorhandenen SpawnPoint instanziiert. Das Detail-Panel bleibt sichtbar; sein Button wechselt zu `Leave Immersive Mode`.
- **Grund:** Fuer Quest 3 Standalone ist ein kontinuierlicher, nicht ladender XR-Flow stabiler als ein Szenenwechsel waehrend der Experience. Die visuelle Transition ist ohnehin bereits durch den Dissolve geloest.
- **Erkenntnisse:**
  - XR-Szenenwechsel koennen deutlich empfindlicher sein als normale Unity-Szenenwechsel, weil Camera Rig, Passthrough-Layer, UI-Raycaster und Runtime-State zeitgleich betroffen sind.
  - Fuer Modi innerhalb derselben Experience ist ein deaktivierter Root in der MainScene oft robuster als `SceneManager.LoadScene` oder additive Szenen.
  - Der Immersive Mode muss nicht architektonisch eine eigene Szene sein; fachlich reicht ein eigener Root mit Environment, SpawnPoint und Mode-Controller.
  - Der fruehere Raumstation-Ansatz bleibt als Lernmoment relevant: gute visuelle Idee, aber fuer den aktuellen Masterarbeits-Prototyp zu teuer und fehleranfaellig.

### Phase 6 / Size-Minispiel: vom statischen Prefab zum live geprueften Snap-Minispiel

- **Datum:** 2026-04-28
- **Vorher:** `Minigame_Size.prefab` war zwar als eigenes Prefab vorhanden, aber die Logik war noch nicht auf den finalen Snap-Flow angepasst. Der vorhandene Fertig-Button haette das Minispiel direkt abschliessen koennen, auch ohne korrekte Loesung. Ausserdem war unklar, wie Liste und Snap-Slots bei globaler Prefab-Instanziierung sinnvoll relativ zum User ausgerichtet werden sollen.
- **Nachher:** Phase 6 fuehrt einen zentralen `SizeChecker.cs` als Manager fuer das komplette `Size`-Minispiel ein. Er sammelt Planeten und Slots automatisch aus dem Prefab, prueft die aktuelle Belegung live ueber die vorhandenen Meta-Snap-Komponenten und verwendet `PlanetData.diameter` fuer die erwartete Reihenfolge. Der Fertig-Button ruft jetzt `FinishButtonPressed()` auf dem `SizeChecker` auf, statt direkt `CompleteAndReturnToMenu()` auszufuehren. Fuer die Raumplatzierung richtet `SizeMinigameWorldLayout.cs` `List` und Slot-Gruppe nach dem Spawn vor dem User, parallel zum Boden und auf etwa 1.10 m Hoehe aus.
- **Grund:** Das Size-Minispiel sollte sich an dieselbe Projektlogik halten wie `Reihenfolge`: Meta SDK fuer Grab/Snap, eigener Projektcode nur fuer Regeln, Feedback und Zustand. Die Positionierung durfte nicht von einer festen Authoring-Position im Prefab abhaengen, weil das Minigame als Ganzes vom `MinigameManager` instanziiert wird.
- **Erkenntnisse:**
  - Bei XR-Prefabs ist ein nachgelagertes Layout-Script oft robuster als eine hart eingebackene Weltposition im Prefab.
  - Ein Fertig-Button sollte in Wissens- und Sortier-Minispielen nie direkt den Erfolg markieren, sondern nur die Fachlogik anstossen.
  - Wenn die Meta-Snap-Komponenten bereits die Interaktion loesen, bleibt eigener Code am stabilsten, wenn er nur `SelectingInteractorViews`, Slot-Reihenfolge und Feedback ausliest.

### Size-Groessenfeedback: Root-Skalierung -> sichtbares VisualRoot

- **Datum:** 2026-04-28
- **Vorher:** Die erste Groessenlogik fuer das Size-Minispiel skalierte den `ReihenfolgePlanet`-Root. Gleichzeitig setzte die Slot-Auswertung den Zustand bei einem neu platzierten, korrekten Planeten zu frueh auf "bereits korrekt". In der Praxis fuehrte das dazu, dass das direkte Wachstum teilweise gar nicht sichtbar oder komplett uebersprungen war.
- **Nachher:** Die Skalierung laeuft jetzt ueber das sichtbare `InteractablePlanetVisual.VisualRoot`. Dessen Anfangsskalierung wird beim Start gecacht, damit ein Reset oder ein Rueckflug auf den Ausgangszustand zuruecksetzen kann. Sobald ein Planet neu korrekt auf seinem Slot liegt, wird sein Visual sofort auf die relative Zielgroesse skaliert; danach werden `Rigidbody` und `Collider` deaktiviert, damit Ueberlappungen ihn nicht physikalisch wegschieben.
- **Grund:** Das sichtbare Planet-Visual liegt im Prefab nicht direkt auf dem Interactable-Root, sondern unter einem separaten `VisualRoot`. Gleichzeitig musste die Statuslogik so geordnet werden, dass "neu korrekt" und "war schon korrekt" sauber unterschieden werden.
- **Erkenntnisse:**
  - Bei Interactable-Prefabs mit separatem Visual-Container sollte visuelles Feedback immer am sichtbaren Child-Hierarchie-Zweig ansetzen, nicht pauschal am Root.
  - Zustandsmaschinen fuer Live-Feedback brauchen eine klare Reihenfolge: alten Zustand lesen, neuen Zustand ableiten, erst danach den State aktualisieren.
  - Physik-Deaktivierung nach korrekter Platzierung ist im XR-Snap-Kontext nicht nur Optimierung, sondern verhindert echte Fehlbewegungen durch Ueberlappung.

### Meta Snap-Listen: automatische Platzierung ueber Default/TimeOut Interactable verstanden

- **Datum:** 2026-04-27
- **Vorher:** Bei `Minigame_Size.prefab` war unklar, warum die Planeten nicht automatisch in das `List`-Objekt einsortiert wurden, obwohl das Rechteck nach manuellem Pinch korrekt wuchs. Zunaechst lag der Verdacht auf dem `List > SnapInteractable`-Setup bzw. dessen Rigidbody-Referenz.
- **Nachher:** Das Listen-Setup aus der Meta Interaction SDK `SnapExamples`-Szene wurde als funktionierender Kern bestaetigt: `ListSnapPoseDelegate` berechnet die Positionen der gesnappten Elemente, `ListSnapPoseDelegateRoundedBoxVisual` skaliert den sichtbaren Rahmen. Der entscheidende Unterschied lag bei den Planeten-`SnapInteractor`s: In `Minigame_Reihenfolge.prefab` zeigen `Default Interactable` und `Time Out Interactable` auf `List > SnapInteractable`; in `Minigame_Size.prefab` standen diese Felder noch auf `None`.
- **Grund:** Ohne `Default Interactable`/`Time Out Interactable` kennt ein Planet beim Start kein Ziel fuer automatisches Snapping. Sobald der Planet im Playmode beruehrt/gepincht wird, sucht der `SnapInteractor` aktiv nach einem passenden `SnapInteractable`, findet die Liste und registriert sich dann korrekt.
- **Erkenntnisse:**
  - Wenn die Liste nach manuellem Snappen korrekt waechst, funktionieren `ListSnapPoseDelegate`, Border-Visual und Listen-`SnapInteractable` bereits.
  - Automatische Startplatzierung ist eine Eigenschaft der einzelnen Planeten-`SnapInteractor`s, nicht der Liste selbst.
  - Fuer kopierte SnapExamples-Setups muessen nicht nur Komponenten kopiert werden, sondern auch die Referenzen zwischen Planet-`SnapInteractor` und Listen-`SnapInteractable`.

### Reihenfolge-Minispiel: eigener Aufbau -> Meta SnapExamples + leichte Projektlogik

- **Datum:** 2026-04-27
- **Vorher:** Das Reihenfolge-Minispiel war nur als Test-Tab/Prefab-Skelett dokumentiert. Es war unklar, ob die Snap-/Grab-Funktionalitaet selbst gebaut oder aus der Meta Sample-Szene uebernommen werden soll. Erste Versuche, Controller-Grab direkt an den Planeten-Scripts nachzuruesten, waeren zu viel eigene Interaktionslogik geworden.
- **Nachher:** `Minigame_Reihenfolge.prefab` basiert auf dem Meta Interaction SDK `SnapExamples`-Aufbau. Die Planeten bleiben normale SDK-Interactables mit `Grabbable`, `SnapInteractor` und `HandGrabInteractable`; die eigene Logik liegt nur in kleinen Projekt-Scripts:
  - `ReihenfolgePlanet.cs` speichert `PlanetData` und `OrbitIndex`.
  - `ReihenfolgeOrbitSlot.cs` liest den gesnappten Planeten aus `SnapInteractable.SelectingInteractorViews` und setzt Ring-Feedback.
  - `ReihenfolgeChecker.cs` sammelt Slots/Planeten aus dem Prefab und prueft, ob alle aktiven Slots korrekt belegt sind.
  - `ReihenfolgeControllerHandMode.cs` schaltet beim Reihenfolge-Minispiel temporaer `OVRManager.controllerDrivenHandPosesType = Natural`, wie in `SnapExamples`, und stellt danach den alten Modus wieder her.
- **Grund:** Die Meta Samples loesen Snap, HandGrab und Controller-driven hand poses bereits robust. Fuer das Projekt ist nur die fachliche Regel relevant: welcher Planet gehoert auf welchen Orbit.
- **Erkenntnisse:**
  - Der entscheidende Unterschied zur `SnapExamples`-Szene war nicht am Planetenobjekt, sondern am Rig/OVRManager: `controllerDrivenHandPosesType` stand im Sample auf `Natural`, in der MainScene auf `None`.
  - Controller sollen global sichtbar und ray-faehig bleiben. Deshalb wird der Controller-Hand-Modus nicht dauerhaft in der Szene aktiviert, sondern nur waehrend `MinigameType.Reihenfolge`.
  - Meta-Ringe aus dem Sample nutzen teils `RoundedBoxProperties`; Ring-Feedback muss daher nicht nur `Renderer.material`, sondern auch diese Properties aktualisieren.
  - Bei SDK-Sample-Prefabs ist es stabiler, den manuellen Editor-Aufbau zu behalten und nur Daten-/Feedback-Scripts zu ergaenzen, statt das komplette Setup per Builder-Script neu zu erzeugen.

### Phase 4: Runtime-Placeholder-UI -> editorbasierte Minigame-Prefabs

- **Datum:** 2026-04-27
- **Vorher:** Das Test-Tab-Skelett erzeugte zeitweise Placeholder-Buttons und Minigame-Panels per Code. Diese UI funktionierte nicht sauber mit Quest-Controller-Rays, weil das notwendige Meta Interaction SDK Canvas-Setup (`Add Ray Interaction to Canvas`) nicht zur Runtime-Erzeugung passte. Der Reset-Button war zunaechst als Reset des aktiven Minigames gedacht.
- **Nachher:** `Button_Reihenfolge`, `Button_Size` und `Button_Gravity` sind Editor-UI im Main Menu. `MinigameManager` instanziiert nur noch zugewiesene Minigame-Prefabs und erzeugt keine Fallback-UI. `Minigame_Reihenfolge.prefab` und `Minigame_Size.prefab` enthalten eigene World-Space-UIs mit `MinigameUIActions`: `Abschliessen` speichert den Quiz-Erfolg, `Beenden` kehrt ohne Erfolg ins Main Menu zurueck. Der Reset-Button im Main Menu setzt den gespeicherten Fortschritt beider Quizzes zurueck.
- **Grund:** XR-UI braucht die Komponenten und Event-Pipeline des Meta Interaction SDK. Diese im Code nachzubauen waere fehleranfaellig und widerspricht der Projektregel, SDK-Samples/Editor-Setups zu bevorzugen.
- **Erkenntnisse:**
  - World-Space-Canvas fuer Quest-Controller-Ray-Interaktion sollte als Prefab/Editor-Objekt gebaut und mit `Add Ray Interaction to Canvas` vorbereitet werden.
  - Runtime-erzeugte UI ist fuer schnelle Desktop-Prototypen praktisch, aber in XR schnell eine Sackgasse, wenn Interaction-SDK-Komponenten fehlen.
  - Completion-Status gehoert nicht in das UI-Prefab, sondern in eine zentrale Minigame-Verwaltung (`MinigameManager` + `PlayerPrefs`), damit Buttons und zukuenftige Checker-Scripts denselben Zustand verwenden.

### Editor-Placement: Depth API im Playmode deaktiviert

- **Datum:** 2026-04-27
- **Vorher:** `PlacementManager` rief auch im Unity Editor `EnvironmentRaycastManager.Raycast(...)` auf. Dadurch war der Playmode auf Windows traege, obwohl die echte Depth API fuer die finale Platzierung ohnehin nur im Quest-Build relevant ist.
- **Nachher:** `PlacementManager.useEditorFallbackPlacement` ist standardmaessig aktiv. Im Editor wird der `EnvironmentRaycastManager` deaktiviert und die Vorschau an einen festen Punkt vor dem Controller-Ray gesetzt. Im Quest-Build bleibt die Depth-API-Platzierung unveraendert aktiv.
- **Grund:** UI-, Menue- und State-Flow sollen schnell im Editor testbar sein, ohne auf langsame oder nicht verfuegbare Depth-API-Daten zu warten.
- **Erkenntnisse:** Entwicklungs-Fallbacks muessen die teuren XR-Subsysteme wirklich umgehen, nicht nur deren Ergebnis ignorieren. Sonst bleibt die Performance-Strafe im Playmode bestehen.

### Phase 3: Einzelne InfoPanels -> globales InfoPanel + Runtime-Binding

- **Datum:** 2026-04-27
- **Vorher:** Die Doku ging noch von getrennten Planeten-/Sonnensystem-Panels und einem weitgehend verworfenen InfoPanel/InfoPunkt-Ansatz aus. Fuer das Sonnensystem-UI war unklar, wie ein `SolarSystemManager` referenziert werden soll, obwohl dieser erst durch das platzierte Prefab entsteht.
- **Nachher:** Phase 3 nutzt ein globales, wiederverwendbares `PlanetInfoPanel`. Es zeigt per `Bind(PlanetData)` immer die Daten des aktuell platzierten oder per Ray angeklickten Planeten. `PlanetRaySelector` arbeitet nur im `WORLD`-State, damit er nicht mit dem Placement-Trigger kollidiert. Das Sonnensystem-Slider-Panel bindet sich nach Placement per `Bind(SolarSystemManager)` an die frisch instanziierte Sonnensystem-Instanz; der Manager muss im UI-Prefab nicht im Inspector gesetzt werden.
- **Grund:** Ein Panel pro Planet wuerde Layout-Duplizierung, fehleranfaellige Prefab-Pflege und unnoetige Objekte erzeugen. Beim Sonnensystem ist eine statische Inspector-Referenz technisch falsch, weil das Zielobjekt erst zur Laufzeit platziert wird.
- **Erkenntnisse:**
  - UI-Panels sollten generisch und datengetrieben bleiben. `PlanetData` ist die Quelle, nicht das Prefab-Layout.
  - Runtime-instanzierte Objekte sollten ihre abhaengigen UIs explizit binden, statt ueber feste Inspector-Referenzen oder globale Suche zu arbeiten.
  - `GameState.WORLD` ist eine sinnvolle Grenze fuer Auswahl-Interaktion: Placement bestaetigt Objekte, WORLD waehlt oder veraendert bestehende Objekte.

### Großer Rebuild: MRUK-Stack & Prototyp-Vollumfang → Depth-API + Tab-Menü

- **Datum:** 2026-04-26
- **Vorher:** State Machine mit 6 Zuständen (`START → AUSWAHL → PLACEMENT → SONNENSYSTEM / PLANET_SCHWEBEND / PLANET_IMMERSIV`); MRUK-basierte Platzierung via `MRUKRoom.Raycast()`; additive Raumstation-Szene mit Spieler-Teleport und begehbarer Umgebungszone; eigenes InfoPanel-Singleton mit InfoPunkt-Hotspots auf jedem Planeten; Detail-Panel für schwebenden Einzel-Planeten; ImmersivePlanetView als Fallback. Insgesamt 8 Scripts (`ArPlanetInstanz`, `ArPlanetManager`, `ImmersivePlanetView`, `InfoPanel`, `PlanetDetailUI`, `PlanetUmgebungsZone`, `PlatzierungManager`, `RaumstationController`).
- **Nachher:** State Machine auf 3 Zustände reduziert (`MAIN_MENU → PLACEMENT → WORLD`); Platzierung über Meta **Depth API** (`EnvironmentRaycastManager.Raycast`) **ohne MRUK-Room**; Hauptmenü mit Tab-Toggle (Planeten / Sonnensystem) — der Tab-Wechsel zerstört platzierte Objekte über `PlacementManager.ClearPlacedObjects()`. Der `PlacementManager` unterstützt zwei Modi (Einzel-Planet mit km→VR-Skalierung, oder Sonnensystem-Prefab in fester Größe). Acht Scripts ersatzlos entfernt; neu hinzugekommen sind nur `MainMenuController.cs` und `PlanetMenuButton.cs`. `GameManager.cs` von 357 auf ~80 Zeilen.
- **Grund:** Durch Versionssprünge der Meta-XR-SDKs haben viele Teile des bisherigen Stacks immer wieder gebrochen — `OVRSceneManager` deprecated, MRUK-API geändert, `OneGrabFreeTransformer` ersetzt, Anchor-Kette anders verkabelt. Jeder Update-Versuch hat einen anderen Teil zerschossen, weil mehrere SDK-Generationen gleichzeitig benutzt wurden. Die Konsequenz: kompletter Rebuild auf der minimal nötigen Oberfläche. Die Depth API liefert Position + Normal direkt aus dem Depth-Mesh und braucht weder ein `MRUKRoom` noch eine Anchor-Hierarchie — damit fällt der häufigste Bruchpunkt weg.
- **Erkenntnisse:**
  - Meta-SDK-Stacks dürfen nicht versionsübergreifend gemischt werden. Wenn ein Subsystem auf v60 angefangen wurde und ein anderes auf v85 dazukommt, gibt es bei jedem Update Versionskonflikte. Lieber eine schmale API-Oberfläche pro Feature wählen und SDK-Updates komplett mitnehmen.
  - **Depth API > MRUK-Room für reine Boden-/Tisch-Platzierung.** MRUK lohnt sich erst bei semantischer Raumkenntnis (Wand, Decke, Möbel-Anchors). Für „lege ein Objekt auf eine annähernd horizontale Fläche" ist `EnvironmentRaycastManager.Raycast` + `Dot(normal, Vector3.up) > 0.85` der einfachere und stabilere Weg.
  - Prototyp-Scope-Reduktion zahlt sich aus: 8 entfernte Scripts haben den Codebase-Footprint massiv vereinfacht, ohne dass der Wireframe darunter leidet — er sieht ohnehin nur noch zwei Modi vor.
  - Tracking-Liste platzierter Objekte (`_placedObjects`) ist die richtige Stelle für „Modus-Wechsel räumt die Welt auf"-Logik. Alternative wäre ein Tag/Layer-basiertes `FindObjectsOfType` gewesen — Liste ist günstiger und expliziter.

### GrabHandle: Manueller Raycast → Meta Interaction SDK + Prefab

- **Datum:** 2026-04-21
- **Vorher:** Drei parallele Ray-/Interaktions-Systeme
  1. `GrabHandle.cs` (242 Zeilen) — eigener `Physics.Raycast` gegen einen Trigger-Collider, eigene `OVRInput.GetDown/GetUp`-Polling-Logik, eigener Material-Tausch via `HervorhebungSetzen()`. Ray-Origins waren die Transforms der Meta-RayInteractor-GameObjects (parasitär kopiert, nicht abonniert).
  2. `ControllerRay.cs` (Quickfix für dieses Ticket) — eigener `LineRenderer` + zweiter `Physics.Raycast` um den unsichtbaren Controller-Ray im Passthrough sichtbar zu machen.
  3. Meta Interaction SDK — wurde nur fürs Canvas-UI (Buttons im AuswahlCanvas) verwendet und blieb sonst als Black Box.
  - `ArPlanetInstanz.GrabHandleErstellen()` baute das Handle mit 45 Zeilen Code: `new GameObject` + `AddComponent<CapsuleCollider>` + `GameObject.CreatePrimitive(Capsule)` + zwei neuen Materials + Script-Konfiguration. Keine Prefab-Wiederverwendung.
- **Nachher:** Eine einzige Ray-Pipeline auf Basis des Meta Interaction SDK
  - **Prefab `Assets/Prefabs/GrabHandle.prefab`** mit vollständiger Komponenten-Kette: `CapsuleCollider` → `ColliderSurface` → `RayInteractable` → `Grabbable` + `GrabFreeTransformer` + `InteractableUnityEventWrapper` + Visual mit `MaterialPropertyBlockEditor` + `InteractableColorVisual`.
  - **`GrabHandleGlue.cs`** (~70 Zeilen): nur projektspezifische Hooks — LazyFollow-Pause beim Greifen, optionales Kamera-Ausrichten beim Loslassen. Verdrahtung über `WhenSelect`/`WhenUnselect` im Inspector (kein `OVRInput`-Polling mehr).
  - **`ArPlanetInstanz.GrabHandleErstellen()`** auf ~15 Zeilen eingedampft: `Instantiate(grabHandlePrefab, transform)` + `Grabbable.InjectOptionalTargetTransform(transform)` — Pille wird gegriffen, Wrapper wird bewegt.
  - Ray-Visual via `RayInteractorRayVisual` im `OVRInteractionComprehensive` (Nachfolger des deprecated `ControllerRayVisual`). `_hideWhenNoInteractable` löst den offenen DOKU-Punkt „Ray nur bei Interactables sichtbar" ohne eigenen Code.
- **Grund:** Drei parallele Raycasts pro Frame + zwei konkurrierende Input-Pfade (OVRInput direkt vs. ISelector aus dem SDK) waren Bug-Multiplikator. Die fehlende visuelle Ray-Rückmeldung im Passthrough-Modus hätte man mit `ControllerRay.cs` zwar isoliert lösen können, aber das vierte Raycast-System zu ergänzen wäre die falsche Richtung gewesen — die Duplikation zog sich bereits durch zwei Features (GrabHandle + InfoPanel haben identische manuelle Raycast-Blöcke).
- **Erkenntnisse:**
  - Meta Interaction SDK: `RayInteractable` nimmt KEINEN `Collider` direkt, sondern eine `ISurface`. Der Adapter ist `ColliderSurface` — einfach übersehen, wenn man von Unity's Physics-Denken kommt.
  - `OneGrabFreeTransformer` ist als deprecated markiert; der Nachfolger `GrabFreeTransformer` implementiert **beide** Interfaces (1- und 2-Hand-Grab) und muss daher in **beide** Slots der `Grabbable` (`_oneGrabTransformer` + `_twoGrabTransformer`) eingetragen werden.
  - `Grabbable.InjectOptionalTargetTransform(t)` ist die offizielle API für „Ich greife Objekt A, aber bewege Objekt B" — exakt die GrabHandle-Mechanik, die vorher 40 Zeilen Offset-Rechnung im Update-Loop brauchte.
  - Code-Saldo: **−280 Zeilen** (GrabHandle.cs 242 + ControllerRay.cs 38) **+70 Zeilen** (GrabHandleGlue.cs). Minus ~210 Zeilen bei gleichzeitig besserer SDK-Integration.
  - Der Prefab-Ansatz öffnet Plug-and-Play: jedes grabbare Objekt (Canvases, einzelne Planeten, später InfoPunkte) = Prefab spawnen + `InjectOptionalTargetTransform`. Keine Kopie des Setup-Codes.

### Immersive Ansicht: Planet in Main-Szene skalieren → Raumstation additiv laden

- **Datum:** 2026-04-16
- **Vorher:** `ImmersivePlanetView.cs` skalierte das schwebende Planeten-Objekt aus `PLANET_SCHWEBEND` direkt hoch (Erde = 10 m VR-Radius). Planet wurde `vrRadius + abstandVonOberflaeche` vor der Kamera positioniert. Passthrough wurde ausgeschaltet → schwarzer Weltraum-Hintergrund.
- **Nachher:** `PLANET_IMMERSIV` lädt `Raumstation.unity` additiv via `SceneManager.LoadSceneAsync(..., Additive)`. Spieler wird in die Station teleportiert. `RaumstationController` spawnt das Planeten-Prefab in echter Größe am `PlanetSpawnPunkt`. `ImmersivePlanetView` wird deaktiviert solange die Station geladen ist.
- **Grund:** Raumstation als eigenständiger Kontext (baked Lighting, Umgebungszone, eigene Geometrie) ist mit der alten Approach nicht kombinierbar. Additive Szene trennt Verantwortlichkeiten sauber.
- **Erkenntnisse:** Bei additivem Szenen-Laden bleibt die erste Szene (Main.unity) vollständig aktiv — GameManager.Instance, Spieler-GameObject und alle Main-Objekte bleiben erhalten. Kein `DontDestroyOnLoad` nötig. Wichtig: Die zweite Szene teilt denselben World-Space; Objekte bei (0,0,0) in Raumstation.unity liegen bei (0,0,0) in World-Space.

### Bug: OVR-Teleport mit Rotation → Szene klebt am Kopf

- **Datum:** 2026-04-16
- **Symptom:** Nach dem Teleport in die Raumstation drehte sich die gesamte Szene mit dem Kopf mit — als wäre sie am Headset festgeklebt.
- **Ursache:** `spielerRoot.SetPositionAndRotation(pos, zielRotation)` setzte die Rotation des OVR-Player-Roots auf die Rotation des SpawnPunkt-GameObjects. In OVR läuft das gesamte Tracking relativ zur Root-Rotation. Wenn die Root-Rotation verändert wird, rotiert der Tracking-Raum mit — das Headset-Tracking wird dann in einem gedrehten Koordinatensystem interpretiert, was dazu führt dass physische Kopfbewegungen die scheinbare Szenenrotation verändern.
- **Fix:** Beim Teleport nur Position setzen, Rotation beibehalten: `spielerRoot.SetPositionAndRotation(zielPosition, spielerRoot.rotation)`.
- **Erkenntnisse:** Bei OVR/XR auf Quest gilt: die Rotation des Player-Roots (OVRPlayerController) NIEMALS beim Teleport verändern. Position kann frei gesetzt werden. Für eine Richtungsänderung beim Teleport gibt es separate OVR-Mechanismen (z.B. Snap Turn), aber das einfache Setzen der Root-Rotation bricht das Tracking.

### Bug: SpawnPunkt auf Bodenhöhe → Spieler halb im Boden

- **Datum:** 2026-04-16
- **Symptom:** Spieler spawnierte halb im Boden der Raumstation.
- **Ursache:** Der `CharacterController` hat eine Kapsel-Höhe (typisch ~1.8 m). `transform.position` ist die Mitte der Kapsel. Wenn der SpawnPunkt auf Bodenhöhe (Y = 0) liegt, liegt die Kapsel-Unterkante 0.9 m unterhalb des Bodens.
- **Fix:** SpawnPunkt-GameObject im Editor ~0.9–1.0 m über dem Boden positionieren (Y anheben), sodass die Kapsel-Mitte auf der richtigen Höhe liegt.
- **Erkenntnisse:** Unity-`CharacterController`-Teleport immer mit Kapselgröße im Kopf. Der `controller.center`-Offset (Default: `(0, 0, 0)` bei einer Kapsel mit `height = 2`) bedeutet, dass `transform.position` die Mitte der Kapsel ist. Für stehende Figuren: SpawnPunkt = gewünschte Fußposition + `capsuleHeight / 2`.



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
| 2026-04-13 | State Machine mit 6 Zuständen + Raumstation/Immersive-Modus | Durch Meta-SDK-Versionssprünge wiederholt gebrochen; Prototyp auf 3 States reduziert (MAIN_MENU/PLACEMENT/WORLD) |
| 2026-04-13 | InfoPanel + InfoPunkt-Hotspots auf Planeten | Im Rebuild gestrichen — UI-Fokus liegt jetzt auf dem Hauptmenü-Tab-Toggle |
| 2026-04-16 | Raumstation additiv laden + begehbare Umgebungszone | Im Rebuild gestrichen — Scope-Reduktion nach SDK-Versionschaos |
| 2026-04-21 | GrabHandle-Prefab als Bewegungsmechanik | Vorerst nicht mehr verdrahtet; bleibt als Code/Prefab im Repo, ist aber an keinen aktiven Flow angebunden |
| 2026-04-26 | MRUKRoom-Raycast für Platzierung | Endgültig durch Depth API (`EnvironmentRaycastManager.Raycast`) ersetzt — Boden/Tisch-Erkennung über Normal-Vergleich, kein Room-Setup nötig |

---

## Gescheiterte Experimente

### OVRSceneManager in Main.unity via YAML eintragen

- **Datum:** 2026-04-12
- **Ziel:** OVRSceneManager-Komponente direkt in die `.unity`-Datei schreiben ohne Unity Editor, inklusive `PlanePrefab`-Referenz auf das SurfacePlane-Prefab
- **Ergebnis:** Technisch möglich, aber unnötig komplex — `OVRSceneManager.PlanePrefab` ist vom Typ `OVRSceneAnchor`, das Prefab muss eine `OVRSceneAnchor`-Komponente haben. Kurz danach stellte sich heraus, dass die gesamte API deprecated ist.
- **Erkenntnisse:** Deprecated APIs möglichst früh erkennen. Meta-SDKs haben oft parallele Generationen die gleichzeitig im Package Cache vorhanden sind. Immer prüfen ob `[Obsolete]`-Attribute gesetzt sind.
