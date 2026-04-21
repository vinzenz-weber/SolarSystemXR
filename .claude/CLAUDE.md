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

## Planeten & Skalierbarkeit

### Umfang

Das Sonnensystem umfasst **8 Planeten** (Merkur, Venus, Erde, Mars, Jupiter, Saturn, Uranus, Neptun). Pluto kann optional als 9. Objekt ergänzt werden, ist aber kein Pflichtbestandteil.

### Entwicklungsstrategie: Erst ein Planet, dann alle

Alle Systeme (UI, InfoPanel, Interaktion, Orbits, etc.) werden zunächst **mit einem einzigen Planeten** entwickelt und vollständig zum Laufen gebracht. Erst danach werden die restlichen Planeten mit Daten und Texturen befüllt.

**Ziel:** Plug-and-Play für jeden weiteren Planeten – minimaler Aufwand beim Hinzufügen neuer Objekte.

### Konsequenzen für den Code

- Planetenspezifische Daten (Name, Radius, Umlaufzeit, Infotexte, etc.) gehören **in ein `ScriptableObject`**, nicht hardcodiert ins Script.
- Systeme wie UI, InfoPanel oder Orbits sollen generisch auf das jeweilige Planet-Objekt reagieren – kein Planet-spezifischer Code in allgemeinen Systemen.
- Wenn ein neues Feature implementiert wird: **Immer fragen, ob es über ein ScriptableObject oder eine generische Komponente lösbar ist**, bevor eine planeten-spezifische Lösung gebaut wird.

---

## Bestehende Systeme bevorzugen

**Bevor neuer Code geschrieben wird**, wird immer zuerst geprüft, ob ein bereits implementiertes System verwendet oder erweitert werden kann.

### Regel

Wenn ein neues Feature angefragt wird, gilt folgende Reihenfolge:

1. **Bestehendes System nutzen** – passt ein vorhandenes Script oder eine Komponente bereits, wird dieses verwendet.
2. **Bestehendes System erweitern** – kann das vorhandene System mit minimalem Aufwand angepasst werden, wird es erweitert statt neu gebaut.
3. **Neues System bauen** – nur wenn die ersten beiden Optionen nicht sinnvoll umsetzbar sind.

### Warum

Mehrere parallel existierende Systeme, die ähnliche Aufgaben erfüllen, führen zu Konflikten, unerwartetem Verhalten und erhöhtem Wartungsaufwand. Das Projekt soll als kohärentes Ganzes funktionieren, nicht als Ansammlung isolierter Lösungen.

### In der Praxis

- Vor jedem neuen Feature werden die relevanten bestehenden Scripts gelesen und verstanden.
- Wenn eine Erweiterung eines bestehenden Scripts sinnvoller ist als ein neues Script, wird das explizit kommuniziert und vorgeschlagen.
- Neue Scripts werden nur angelegt, wenn eine klare, eigenständige Verantwortlichkeit vorliegt, die kein bestehendes System sinnvoll übernehmen kann.

---

## QA-Loop-Workflow

Wenn der User ein Feature mit dem Zusatz **"QA-Loop"** (oder sinngemäß) anfragt, läuft der Implementierungsprozess in einer Feedback-Schleife:

### Ablauf

1. `feature-implementer`-Agent implementiert das Feature
2. `qa-reviewer`-Agent reviewt das Ergebnis
3. Ist der Status **⚠️ oder ❌**: `feature-implementer` bekommt das QA-Feedback und korrigiert
4. Zurück zu Schritt 2 – bis der Status **✅** ist oder nach max. **3 Iterationen**
5. Nach dem letzten Durchgang: Zusammenfassung an den User, was umgesetzt wurde und was (falls noch offen) manuell im Unity Editor getan werden muss

### Wichtig

- Der Loop läuft vollständig durch, bevor der User um Bestätigung gebeten wird
- Nach max. 3 Iterationen wird der aktuelle Stand gezeigt, auch wenn der QA noch Einwände hat – dann entscheidet der User
- Der QA-Reviewer macht nur **statische Code-Analyse** – ob das Feature in Unity tatsächlich funktioniert, muss der User im Editor/Headset testen

### Aktivierung

Der User schreibt z.B.:
- "Implementiere Feature X, QA-Loop an"
- "Bau Feature X mit QA-Loop"
- "Feature X implementieren und iterieren bis es passt"

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

---

## Selbstständige CLAUDE.md-Pflege

Claude aktualisiert diese Datei **eigenständig und proaktiv**, ohne explizite Aufforderung, wenn im Laufe der Arbeit Informationen auftauchen, die den Entwicklungsprozess vereinfachen oder zukünftige Entscheidungen erleichtern.

### Was wird ergänzt

- Projektspezifische Konventionen, die sich im Verlauf herausbilden (z.B. wie Prefabs strukturiert sind, wie Events verdrahtet werden)
- Architekturentscheidungen, die einmal getroffen wurden und konsistent bleiben sollen
- Wiederkehrende Muster oder Unity-Verhaltensweisen, die für dieses Projekt relevant sind
- Klarstellungen zu bestehenden Abschnitten, wenn sich diese in der Praxis als unvollständig herausstellen

### Was nicht ergänzt wird

- Kurzlebige, task-spezifische Infos – die gehören ins Memory oder in DOKU/ARCHIV
- Code-Snippets oder fertige Implementierungen
- Redundantes, das sich aus dem Code selbst ergibt