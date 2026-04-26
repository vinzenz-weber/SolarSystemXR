# Roadmap: Main Menu Umbau auf Learn/Test mit Minispielen

Schritt-für-Schritt Checkliste. Jede Phase ist isoliert testbar — nicht weitermachen, bevor die aktuelle Phase im Headset funktioniert.

Vollständiger Plan: `C:\Users\Vinni\.claude\plans\ic-habe-jetzt-mal-zippy-sketch.md`

---

## Ziel-Struktur

```
Main Menu (Passthrough)
├── Tab: Learn
│   ├── Planets       → Planet platzieren + Info-Panel
│   └── SolarSystem   → bestehendes Sonnensystem-Placement + Slider-UI
└── Tab: Test (Quiz, alles Passthrough)
    ├── Reihenfolge   → Planeten auf Bahnen snappen
    ├── Size          → Planeten nach Größe sortieren (Linie)
    └── Gravity       → "Coming Soon" Placeholder
```

**Architektur-Entscheidung:** Alles in `MainScene.unity`, kein Scene-Loading. Modi werden über Container-GameObjects (SetActive) und Prefab-Instanziierung gesteuert.

---

## Phasen

### Phase 1 — PlanetData ScriptableObjects
- [x] 8 Planeten-SOs befüllt ✅

---

### Phase 2 — Main Menu Skelett: Tabs Learn/Test
- [x] `MainMenuController.cs` Methoden + Felder umbenennen (Planeten/Sonnensystem → Learn/Test)
- [ ] Canvas im Editor: Tab-Buttons + Container-Panels neu verdrahten (Inspector)
- [x] Tab-Wechsel ruft `PlacementManager.ClearPlacedObjects()`
- [ ] Sample geprüft: `UISetExamples.unity` für Toggle-Buttons
- [ ] **Verifikation im Headset:** Tab-Wechsel funktioniert

---

### Phase 3 — Learn-Tab: Planets + SolarSystem
- [ ] Im Learn-Panel zwei Einstiege: "Planets" und "SolarSystem"
- [ ] "Planets" → Planeten-Auswahl → `PlacementManager.SelectPlanet(data)`
- [ ] "SolarSystem" → `PlacementManager.SelectSolarSystem(prefab)`
- [ ] Neues Script `PlanetInfoPanel.cs` mit `Bind(PlanetData)`
- [ ] Neues Prefab `PlanetInfoPanel.prefab` (Template: `PanelWithManipulators.unity`)
- [ ] `PlacementManager` erweitert: spawnt Info-Panel als Child beim Planet-Placement
- [ ] `SonnensystemUI`-Slider-Canvas als Child des platzierten Sonnensystems
- [ ] **Verifikation im Headset:** Erde platzieren → Info-Panel sichtbar; Sonnensystem → Slider funktionieren

---

### Phase 4 — Test-Tab: Skelett mit drei Einträgen + Reset-Mechanik
- [ ] Im Test-Panel drei Einträge: "Reihenfolge", "Size", "Gravity (Coming Soon)"
- [ ] Neues Script `MinigameManager.cs` mit `StartMinigame()`, `EndMinigame()`, `ResetMinigame()`
- [ ] `GameManager.cs` um Minispiel-States erweitern
- [ ] Pro Minispiel ein World-Space-UI: Reset-Button + Zurück-Button
- [ ] **Verifikation:** Drei Einträge schalten korrekt, Reset funktioniert, Zurück führt ins Hauptmenü

---

### Phase 5 — Minispiel 1: Reihenfolge
- [ ] Prefab `Minigame_Reihenfolge.prefab`: 8 Bahn-Ringe + 8 grabbable Planeten
- [ ] Snap-Mechanik aus `SnapExamples.unity` kopiert (`SnapInteractable` + `SnapInteractor`)
- [ ] Pro Bahn `correctOrbitIndex` (0=Merkur … 7=Neptun)
- [ ] Neues Script `ReihenfolgeChecker.cs`: Snap-Events → Material grün/rot
- [ ] "Geschafft!"-Hinweis bei allen 8 grün
- [ ] Reset-Button leert Snaps, randomisiert Bench
- [ ] **Verifikation im Headset:** Greifen, snappen, Farbwechsel, Reset

---

### Phase 6 — Minispiel 2: Size
- [ ] Prefab `Minigame_Size.prefab`: 8 lineare Snap-Slots + 8 grabbable Planeten
- [ ] `ListSnapPoseDelegate` aus `SnapExamples.unity`
- [ ] Neues Script `SizeChecker.cs`: vergleicht Sortierung mit `PlanetData.diameter`
- [ ] Globale Validierung: alle 8 grün ODER alle rot blinken
- [ ] Reset wie Phase 4
- [ ] **Verifikation im Headset:** Korrekt sortiert → grün; falsch → rot

---

### Phase 7 — Minispiel 3: Gravity (Placeholder)
- [ ] "Gravity"-Eintrag visuell als "Coming Soon" markiert (ausgegraut + Label)
- [ ] Klick zeigt optional Hinweis-Panel "Kommt später"
- [ ] Notiz in `DOKU.md` oder `ARCHIV.md`: vollständige Implementierung verschoben
- [ ] **Verifikation:** Eintrag sichtbar, kein Crash

---

### Phase 8 — Polish-Tag
- [ ] Visuelle Übergänge zwischen Tabs/Modi (Fade)
- [ ] Konsistente Farben (Highlight/Correct/Wrong)
- [ ] Hover-Highlight auf allen Buttons
- [ ] Audio-Feedback: Click, Correct, Wrong (`AudioSource.PlayOneShot`)
- [ ] Sauberes Cleanup beim Modus-Wechsel
- [ ] Performance-Pass: Profiler in MainScene, unnötige Lights raus
- [ ] Final-Check als APK-Build aufs Headset (nicht nur Quest Link)

---

## End-to-End-Test (nach allen Phasen)

1. App starten → Hauptmenü im Passthrough
2. Learn → Planets → Erde platzieren → Info-Panel mit Erde-Daten sichtbar
3. SolarSystem platzieren → Slider verändern Skala/Zeit
4. Test → Reihenfolge: 8 Planeten auf richtige Bahnen → alle grün → Reset → erneut spielbar
5. Test → Size: 8 Planeten sortieren → alle grün
6. Test → Gravity: "Coming Soon"-Hinweis

---

## Aufwand: ~4,7 Tage + 1 Tag Polish
