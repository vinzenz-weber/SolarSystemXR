# Projekt: Sonnensystem XR

## Projektziel

Eine XR-App für die Meta Quest 3 (Standalone), in der der User das Sonnensystem erkunden und etwas darüber lernen kann. Das Projekt wird im Rahmen einer **Masterarbeit** entwickelt.

---

## Technisches Setup

- **Unity Version:** 6000.3.10f1
- **Render Pipeline:** URP
- **Target Platform:** Meta Quest 3 Standalone (Android)
- **XR SDK:** Meta XR SDK v85
- **Meta Building Blocks** bevorzugt verwenden, wenn möglich

---

## Code-Richtlinien

### Erfahrungslevel

Der Entwickler hat einen Processing-Hintergrund und bisher wenige Unity-Projekte umgesetzt. Der generierte und geschriebene Code soll dieses Level widerspiegeln – einfach, verständlich und nachvollziehbar, außer es geht nicht anders.

### Kommentare

- Sprache: **Deutsch**

### Naming Conventions (Unity/C# Standard)

- `camelCase` für lokale Variablen und private Felder
- `_camelCase` (Unterstrich-Prefix) für private Klassenfelder
- `PascalCase` für public Felder, Properties, Methoden und Klassen
- Booleans mit Präfix: `is`, `has`, `can` (z.B. `isOrbiting`, `hasAtmosphere`)
- Konstanten: `PascalCase` oder `UPPER_SNAKE_CASE`

---

## Performance & Optimierung

Fokus auf Optimierung – die App soll visuell gut aussehen und gleichzeitig flüssig auf der Quest 3 im Standalone-Modus laufen.

---

## Testing

- **Windows:** Meistens via Meta Quest Link
- **Mac:** Per Build direkt auf das Headset

---

## Dokumentations-Workflow (Masterarbeit)

Es gibt zwei Dokumentationsdateien im Projekt, die den Fortschritt der Masterarbeit festhalten:

### `DOKU.md` – Aktueller Stand
Enthält nur den **aktuellen, lebendigen Stand** des Projekts: aktive Features, technische Entscheidungen, bekannte Probleme.

### `ARCHIV.md` – Verworfenes & Iterationen
Enthält alles, was aus der DOKU entfernt wurde: verworfene Features, gescheiterte Experimente, grundlegende Umbauten. Dient als Grundlage für das **Progress-Kapitel** der Masterarbeit.

### Befehle

| Befehl | Was passiert |
|---|---|
| **"Update DOKU"** | DOKU.md wird mit dem aktuellen Stand aktualisiert. Gleichzeitig wird ARCHIV.md automatisch ergänzt: alles was sich seit dem letzten Stand verändert hat (umgebaute Features, verworfene Ansätze, behobene Bugs mit nicht-offensichtlicher Ursache) wird unter "Iterationen & Änderungen" eingetragen. |
| **"Feature X verworfen, Grund: Y"** | Feature wird aus DOKU.md gelöscht und mit Datum + Begründung ins ARCHIV.md verschoben. |
| **"Feature X umgebaut"** | DOKU.md wird aktualisiert, die alte Version wird unter "Iterationen & Änderungen" ins ARCHIV.md eingetragen. |

### Wichtig
- Beim Verschieben ins Archiv immer **Datum**, **Begründung** und **Erkenntnisse** festhalten.
- DOKU.md soll immer nur den tatsächlichen Ist-Zustand zeigen – nichts Veraltetes.
- ARCHIV.md dokumentiert nicht nur verworfenes, sondern auch **Lernmomente**: nicht-offensichtliche Unity-Verhaltensweisen, Bugs deren Ursache überraschend war, und Architekturentscheidungen die sich als falsch herausgestellt haben.