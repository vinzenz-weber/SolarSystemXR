# User Testing - Sonnensystem XR

> Arbeitsdatei fuer die Vorbereitung, Durchfuehrung und Auswertung des User Testings zur Masterarbeit.
> Stand: 2026-05-14
> Englische Materialien fuer die tatsaechliche Durchfuehrung mit internationalen Teilnehmer:innen liegen in `.claude/UserTesting_EN.md`.

---

## 1. Ziel des User Testings

Das User Testing soll nicht nur pruefen, ob der Prototyp technisch bedienbar ist, sondern gezielt Material fuer die Beantwortung der Forschungsfrage liefern:

> Wie kann eine XR-Lernanwendung zum Sonnensystem gestaltet werden, um astronomische Massstabsverhaeltnisse fachlich nachvollziehbar, raeumlich erfahrbar und spielerisch motivierend zu vermitteln?

Das Testing untersucht deshalb drei Ebenen:

- **Fachliche Nachvollziehbarkeit:** Verstehen die Teilnehmer:innen, dass Groessen, Distanzen, Orbitgeschwindigkeiten und Darstellungsparameter im Prototyp bewusst skaliert bzw. didaktisch vereinfacht werden muessen?
- **Raeumliche Erfahrbarkeit:** Hilft die Platzierung im Raum dabei, Massstabsverhaeltnisse, Anordnung, Abstaende und Umlaufbahnen koerperlich bzw. perspektivisch besser zu erfassen als in einer flachen Darstellung?
- **Spielerische Motivation:** Werden die Minigames und direkten Rueckmeldungen als motivierend erlebt und unterstuetzen sie das aktive Anwenden von Wissen zu Reihenfolge und Groessenverhaeltnissen?

Die Studie ist als **praxisorientierte, explorative Prototyp-Evaluation** angelegt. Sie muss keine grosse, allgemein repraesentative Wirkung nachweisen, sondern soll zeigen, ob der entwickelte Prototyp Hinweise auf Lernfoerderung, Motivation und geeignete Gestaltungskriterien liefert.

---

## 2. Untersuchungsdesign

### 2.1 Empfohlenes Studiendesign

Empfohlen wird ein **Within-Subject Pre-Post-Design**:

1. Teilnehmer:in beantwortet vor der XR-Nutzung einen kurzen Vorfragebogen.
2. Teilnehmer:in nutzt die App anhand konkreter Aufgaben.
3. Teilnehmer:in beantwortet danach denselben oder einen sehr aehnlichen Wissensfragebogen sowie UX-/Motivationsfragen.
4. Optional: kurzes Abschlussinterview oder Freitextfragen.

Warum dieses Design sinnvoll ist:

- Jede Person dient als eigener Vergleichswert.
- Auch bei kleiner Stichprobe kann eine Veraenderung zwischen Vorher und Nachher untersucht werden.
- Das Design passt gut zu einer Masterarbeit mit Prototyp und begrenzter Teilnehmerzahl.
- Der Einfluss unterschiedlicher Vorkenntnisse wird reduziert, weil nicht zwei verschiedene Gruppen verglichen werden.

### 2.2 Stichprobe

Zielwert:

- **Minimum:** 8-10 Personen fuer eine erste qualitative/prototypische Evaluation.
- **Besser:** 15-25 Personen, wenn eine einfache statistische Pre-Post-Auswertung belastbarer wirken soll.
- **Sehr gut, falls realistisch:** 30+ Personen, insbesondere wenn Subgruppen betrachtet werden sollen.

Wichtig:

- Keine repraesentative Stichprobe behaupten, wenn die Teilnehmer:innen aus Freundeskreis, Hochschule oder Umfeld kommen.
- In der Arbeit transparent als **Convenience Sample** oder **Gelegenheitsstichprobe** beschreiben.
- Alter, XR-Erfahrung und Vorwissen dokumentieren, aber nur vorsichtig interpretieren.

### 2.3 Dauer pro Testperson

Geplanter Ablauf:

| Phase | Inhalt | Dauer |
|---|---:|---:|
| Begruessung und Einwilligung | Studienziel, Datenschutz, Abbruchmoeglichkeit | 3-5 min |
| Pre-Fragebogen | Demografie, Vorwissen, Wissensfragen | 5-8 min |
| Einweisung | Controller, Sicherheit, kurze Bedienhinweise | 3-5 min |
| App-Aufgaben | Home-/Planet-Flow, Sonnensystem, Massstabs-Controls, Minigames | 15-25 min |
| Post-Fragebogen | Wissensfragen, UX, Motivation, Selbsteinschaetzung | 8-12 min |
| Kurzes Interview | Auffaelligkeiten, Verbesserungsvorschlaege | 5-10 min |
| Gesamt | | ca. 40-60 min |

> [NEU 2026-05-14 - START: Praktischer Headset-Setup-Modus]
>
> ### 2.4 Headset-Setup vor dem eigentlichen Teststart
>
> Vor der eigentlichen App-Nutzung kann ein technischer Vorbereitungsmodus genutzt werden. Die Testleitung aktiviert ihn durch gleichzeitiges Druecken beider Controller-Thumbsticks. Die App wechselt dann in Passthrough, sodass die Testperson ihre reale Umgebung sieht. Gleichzeitig werden App-Inhalte ausgeblendet bzw. zurueckgesetzt und Audio pausiert.
>
> Ziel fuer die Durchfuehrung:
>
> - Brille ruhig anpassen, bevor digitale Inhalte Aufmerksamkeit binden.
> - Testperson kurz im realen Raum orientieren lassen.
> - Controllerposition, Sitzkomfort und Sichtfeld pruefen.
> - Kognitive Belastung am Anfang senken: Die Person muss nicht gleichzeitig Headset, Umgebung, Controller und App-Inhalte verarbeiten.
> - Erst nach diesem Setup die eigentliche Startsequenz mit Logo/Tutorial und Hauptmenue beginnen.
>
> Ablauf fuer die Testleitung:
>
> 1. Nach Einwilligung und Pre-Fragebogen Headset aufsetzen lassen.
> 2. User-Testing-Setup per Doppel-Thumbstick aktivieren, falls es nicht bereits aktiv ist.
> 3. Brille anpassen, kurz nach Komfort/Schaerfe/Orientierung fragen.
> 4. Controller in die Haende geben und nur die noetigsten Sicherheitshinweise geben.
> 5. Mit erneutem Doppel-Thumbstick die App-Sequenz starten.
> 6. Beobachten und notieren, ob trotz Setup noch Unsicherheit, Ueberforderung oder Orientierungsprobleme auftreten.
>
> Methodischer Grund:
>
> - Der Vorbereitungsmodus soll verhindern, dass die ersten App-Minuten durch vermeidbaren Startstress verzerrt werden.
> - Fuer die Auswertung kann vermerkt werden, dass die technische Headset-Anpassung bewusst vor die eigentliche Testaufgabe ausgelagert wurde.
> - Hilfestellung waehrend dieser Setup-Phase sollte getrennt von Hilfestellung innerhalb der App-Aufgaben notiert werden.
>
> [NEU 2026-05-14 - ENDE]

---

## 3. Hypothesen / Annahmen

Da die Arbeit eher gestalterisch-praktisch ausgerichtet ist, koennen statt starker kausaler Hypothesen auch **evaluative Annahmen** formuliert werden. Fuer die Auswertung sind trotzdem testbare Hypothesen hilfreich.

### H1 - Fachliche Nachvollziehbarkeit

**Hypothese:** Nach der Nutzung von Sonnensystem XR koennen die Teilnehmer:innen astronomische Massstabsverhaeltnisse fachlich besser einordnen als vor der Nutzung.

- Messung: Pre-Score vs. Post-Score, z.B. 0-10 Punkte.
- Auswertung: Wilcoxon-Vorzeichen-Rang-Test bei kleiner Stichprobe oder nicht normalverteilten Differenzen; alternativ gepaarter t-Test, wenn die Differenzen annaehend normalverteilt sind.
- Bezug zur Forschungsfrage: Stuetzt den Aspekt der fachlichen Nachvollziehbarkeit. Besonders relevant sind Fragen zu Planetengroessen, Reihenfolge, Gas-/Eisriesen, Umlaufzeit und der Schwierigkeit massstabsgetreuer Sonnensystemdarstellungen.

### H2 - Raeumliche Erfahrbarkeit

**Hypothese:** Nach der Nutzung schaetzen die Teilnehmer:innen ihr Verstaendnis fuer raeumliche Groessen-, Distanz- und Orbitverhaeltnisse im Sonnensystem hoeher ein als vor der Nutzung.

- Messung: Likert-Items vor/nach der Nutzung, z.B. 1 = stimme gar nicht zu bis 5 = stimme voll zu.
- Auswertung: Wilcoxon-Vorzeichen-Rang-Test fuer gepaarte ordinalskalierte Daten; zusaetzlich Median und Interquartilsabstand berichten.
- Bezug zur Forschungsfrage: Stuetzt den Aspekt der raeumlichen Erfahrbarkeit. Zusaetzlich werden Beobachtungen aus Platzierung, Perspektivwechsel, Sonnensystem-Slidern und Immersive Mode qualitativ ausgewertet.

### H3 - Spielerische Motivation

**Hypothese:** Die spielerischen Aufgaben werden von den Teilnehmer:innen als motivierend, verstaendlich und lernunterstuetzend wahrgenommen.

- Messung: Post-Fragebogen mit Likert-Items zu Motivation, Spass, Feedback und Lernwert der Minigames.
- Auswertung: Deskriptiv, z.B. Median, Mittelwert, Verteilung; keine Pre-Post-Statistik noetig, da nur nach der Nutzung erhoben.
- Bezug zur Forschungsfrage: Stuetzt den Aspekt der spielerisch motivierenden Vermittlung.

### H4 - Bedienbarkeit

**Hypothese:** Der Prototyp ist fuer die Teilnehmer:innen grundsaetzlich bedienbar, auch wenn nicht alle Personen XR-Vorerfahrung haben.

- Messung: Task Completion, Beobachtung von Problemen, UX-Fragebogen, z.B. SUS oder kurze eigene Usability-Skala.
- Auswertung: Deskriptiv; bei SUS kann ein Gesamtscore berechnet werden.
- Bezug zur Forschungsfrage: Zeigt, ob die Lernwirkung nicht durch Bedienprobleme ueberlagert wird.

---

## 4. Messinstrumente

### 4.1 Online-Fragebogen

Geeignete Tools:

- Google Forms
- Microsoft Forms
- LimeSurvey, falls Hochschule verfuegbar
- Unipark, falls Hochschule verfuegbar

Empfehlung:

- Fuer diese Masterarbeit reicht wahrscheinlich Google Forms oder Microsoft Forms, wenn keine sensiblen personenbezogenen Daten erhoben werden.
- Wichtig ist eine anonyme Teilnehmer-ID, z.B. `P01`, damit Pre- und Post-Daten gepaart werden koennen.
- Pre- und Post-Fragebogen koennen in einem einzigen Formular mit Abschnitten oder in zwei Formularen erhoben werden. Ein Formular ist organisatorisch einfacher.

### 4.2 Beobachtungsbogen

Zusaetzlich zum Fragebogen sollte waehrend der App-Nutzung ein Beobachtungsbogen ausgefuellt werden:

- Welche Aufgaben wurden erfolgreich abgeschlossen?
- Wo brauchte die Person Hilfe?
- Gab es Bedienprobleme?
- Gab es Zeichen von Unwohlsein oder Motion Sickness?
- Welche Kommentare hat die Person spontan geaeussert?

Diese Daten sind wichtig, weil sie erklaeren koennen, warum ein Fragebogenergebnis so ausgefallen ist.

---

## 5. Pre-Fragebogen

### 5.1 Einwilligung

Textvorschlag:

> Ich nehme freiwillig an diesem User Testing teil. Mir ist bekannt, dass ich die Teilnahme jederzeit ohne Angabe von Gruenden abbrechen kann. Die erhobenen Daten werden anonymisiert und ausschliesslich im Rahmen der Masterarbeit zur Evaluation des XR-Prototyps "Sonnensystem XR" verwendet.

Antwort:

- Ja, ich stimme zu.
- Nein, ich stimme nicht zu. -> Test nicht durchfuehren.

### 5.2 Basisdaten

1. Teilnehmer-ID: Freitext, z.B. P01
2. Alter: Freitext oder Altersgruppen
3. Studien-/Berufshintergrund: Freitext oder Kategorien
4. Hast du bereits XR-/VR-Brillen genutzt?
   - Nein
   - Einmal oder selten
   - Mehrmals
   - Regelmaessig
5. Wie schaetzt du dein Wissen ueber das Sonnensystem ein?
   - 1 = sehr gering
   - 2 = gering
   - 3 = mittel
   - 4 = gut
   - 5 = sehr gut
6. Wie sicher fuehlst du dich beim Einschaetzen astronomischer Groessen und Distanzen?
   - 1 = sehr unsicher
   - 2 = eher unsicher
   - 3 = teils/teils
   - 4 = eher sicher
   - 5 = sehr sicher

### 5.3 Pre-Wissensfragen

Jede richtige Antwort = 1 Punkt. Maximalwert bei 10 Fragen = 10 Punkte.

1. Wie viele Planeten hat unser Sonnensystem nach aktueller Definition?
   - 7
   - 8 [richtig]
   - 9
   - 10

2. Welcher Planet ist der Sonne am naechsten?
   - Venus
   - Merkur [richtig]
   - Erde
   - Mars

3. Welche Reihenfolge der vier inneren Planeten von der Sonne nach aussen ist korrekt?
   - Merkur, Venus, Erde, Mars [richtig]
   - Venus, Merkur, Erde, Mars
   - Merkur, Erde, Venus, Mars
   - Erde, Venus, Merkur, Mars

4. Welcher Planet ist der groesste Planet im Sonnensystem?
   - Erde
   - Saturn
   - Jupiter [richtig]
   - Neptun

5. Welcher Planet ist der kleinste der acht Planeten?
   - Mars
   - Merkur [richtig]
   - Venus
   - Neptun

6. Welche Aussage zu Jupiter und Erde ist am ehesten korrekt?
   - Jupiter ist etwa gleich gross wie die Erde.
   - Jupiter ist etwa doppelt so gross wie die Erde.
   - Jupiter ist im Durchmesser etwa elfmal so gross wie die Erde. [richtig]
   - Jupiter ist kleiner als die Erde.

7. Welche Planeten werden als Gasriesen bezeichnet?
   - Merkur und Venus
   - Erde und Mars
   - Jupiter und Saturn [richtig]
   - Uranus und Neptun

8. Welche Planeten werden haeufig als Eisriesen bezeichnet?
   - Jupiter und Saturn
   - Uranus und Neptun [richtig]
   - Erde und Mars
   - Merkur und Venus

9. Was beschreibt die Umlaufzeit eines Planeten?
   - Die Zeit, die ein Planet fuer eine Drehung um die eigene Achse braucht.
   - Die Zeit, die ein Planet fuer einen Umlauf um die Sonne braucht. [richtig]
   - Die Zeit, die Licht von der Sonne zum Planeten braucht.
   - Die Zeit zwischen zwei Sonnenfinsternissen.

10. Warum sind massstabsgetreue Darstellungen des Sonnensystems schwierig?
   - Weil die Planeten alle gleich gross sind.
   - Weil die Distanzen im Verhaeltnis zu den Planetengroessen extrem gross sind. [richtig]
   - Weil Planeten keine festen Positionen haben.
   - Weil die Sonne kleiner als die Planeten ist.

### 5.4 Pre-Selbsteinschaetzung

Skala: 1 = stimme gar nicht zu, 5 = stimme voll zu.

1. Ich kann die Reihenfolge der Planeten sicher benennen.
2. Ich kann mir die Groessenunterschiede zwischen den Planeten gut vorstellen.
3. Ich kann mir die Distanzen zwischen Sonne und Planeten gut vorstellen.
4. Ich verstehe, was eine Umlaufbahn ist.
5. Ich finde das Sonnensystem als Thema interessant.

---

## 6. Aufgaben in der App

Die Aufgaben sollen den aktuellen App-Flow abdecken, aber nicht zu viele Sonderfaelle enthalten. Wichtig ist, dass sie direkt auf die Forschungsfrage einzahlen: astronomische Massstabsverhaeltnisse sollen fachlich nachvollziehbar, raeumlich erfahrbar und spielerisch motivierend vermittelt werden.

> [NEU 2026-05-14 - START: Tutorial vor den Aufgaben]
>
> Vor Aufgabe 1 kann die kurze Tutorial-Sequenz durchlaufen werden. Sie zaehlt nicht als eigene Lernaufgabe zum Sonnensystem, sondern als Bedien-Onboarding. Beobachtet werden kann trotzdem, ob die Person die gezeigten Bedienhandlungen versteht und ob die Erklaervideos die erste Orientierung erleichtern.
>
> Kurz notieren:
>
> - Tutorial vollstaendig durchlaufen: ja/nein
> - Verstandene Bedienhandlung: Button/Ray, Menue, Greifen oder sonstige
> - Auffaelligkeiten: Video unklar, Panel schlecht positioniert, Schritt nicht abgeschlossen, Hilfe noetig
>
> [NEU 2026-05-14 - ENDE]

### Aufgabe 1 - Home-Bereich starten und einen Planeten platzieren

Aufgabe fuer Teilnehmer:in:

> Starte im Home-Bereich eine Planeten-Ansicht, waehle einen Planeten aus und platziere ihn im Raum.

Beobachtung:

- Findet die Person den Home-/Planet-Bereich?
- Versteht sie die Menuefuehrung?
- Braucht sie Hilfe bei Controller-Ray oder Buttons?
- Ist die Platzierung im Raum nachvollziehbar?

Erfolgskriterium:

- Planet wird ohne oder mit maximal kleiner Hilfestellung ausgewaehlt und im Raum platziert.

Bezug zur Forschungsfrage:

- Einstieg in die raeumliche Erfahrbarkeit: Der Planet wird nicht als flaches Bild, sondern als Objekt im eigenen Raum betrachtet.

### Aufgabe 2 - Planetendetails fachlich einordnen

Aufgabe:

> Oeffne die Detailinformationen zu diesem Planeten und finde eine Information, die dir hilft, den Planeten fachlich einzuordnen.

Beobachtung:

- Wird das InfoPanel gefunden und gelesen?
- Sind die Texte lesbar?
- Wird die raeumliche Position des Panels als angenehm empfunden?
- Kann die Person einen fachlichen Fakt wiedergeben, ohne nur UI-Text wortwoertlich abzulesen?

Erfolgskriterium:

- Teilnehmer:in kann nach der Aufgabe einen Fakt zum Planeten nennen.

Bezug zur Forschungsfrage:

- Fachliche Nachvollziehbarkeit: Planetendaten und kurze Infotexte sollen als verstaendliche Orientierung dienen.

### Aufgabe 3 - Groessenvergleich zwischen Planeten

Aufgabe:

> Oeffne erneut das Menue, waehle mindestens einen weiteren Planeten aus und vergleiche ihn mit dem ersten. Achte besonders auf Groesse, Aussehen und Informationen im Detailpanel.

Beobachtung:

- Findet die Person selbststaendig zurueck ins Menue?
- Versteht sie, dass weitere Planeten ausgewaehlt und miteinander verglichen werden koennen?
- Vergleicht sie Planeten aktiv miteinander oder betrachtet sie nur einzelne Objekte isoliert?
- Werden Groessenverhaeltnisse, visuelle Unterschiede oder Planetendaten spontan angesprochen?
- Entsteht ein exploratives Verhalten, also eigenes Nachfragen, Ausprobieren oder Vergleichen?

Erfolgskriterium:

- Teilnehmer:in waehlt mindestens einen weiteren Planeten aus.
- Teilnehmer:in nennt mindestens einen beobachteten Unterschied zwischen zwei Planeten, z.B. Groesse, Farbe, Entfernung, Kategorie oder einen Fakt aus dem InfoPanel.

Messwerte:

- Anzahl betrachteter Planeten
- Anzahl spontan genannter Vergleiche
- Hilfestellung: keine / klein / deutlich
- Besonders genannte Unterschiede oder Aha-Momente

Bezug zur Forschungsfrage:

- Fachliche Nachvollziehbarkeit von Massstab: Die Person soll Unterschiede zwischen Planeten nicht nur auswendig nennen, sondern anhand der Darstellung und Daten begruenden.

### Aufgabe 4 - Sonnensystem platzieren und raeumlich betrachten

Aufgabe:

> Platziere das gesamte Sonnensystem im Raum und betrachte die Planeten, Orbitlinien und Abstaende aus verschiedenen Blickwinkeln.

Beobachtung:

- Funktioniert die Platzierung intuitiv?
- Wird die raeumliche Darstellung verstanden?
- Gibt es Probleme mit Blickrichtung, Distanz, Lesbarkeit oder Groesse?
- Bewegt sich die Person um das Modell oder bleibt sie an einer Position?
- Werden Anordnung, Umlaufbahnen oder Abstaende spontan kommentiert?

Erfolgskriterium:

- Sonnensystem wird platziert und mindestens drei Planeten oder Orbitbereiche werden bewusst betrachtet.

Bezug zur Forschungsfrage:

- Raeumliche Erfahrbarkeit: Die Anordnung soll als Raumstruktur erfahrbar werden, nicht nur als schematische Liste.

### Aufgabe 5 - Massstabs- und Orbit-Controls verwenden

Aufgabe:

> Veraendere die Darstellung mit den Steuerungen so, dass du besser erkennen kannst, wie sich Planetengroessen, Abstaende, Umlaufbahnen oder Bewegungen unterscheiden.

Beobachtung:

- Werden die Slider bzw. Steuerungen verstanden?
- Erkennen die Teilnehmer:innen, dass Groesse, Abstand, Orbitdarstellung und Bewegung didaktisch angepasst werden koennen?
- Verstaerkt die Interaktion das Verstaendnis oder fuehrt sie zu Verwirrung?
- Wird verstanden, dass eine XR-Darstellung des Sonnensystems nicht im echten Massstab in einen Raum passt?

Erfolgskriterium:

- Teilnehmer:in kann erklaeren, dass eine Darstellung des Sonnensystems oft skaliert oder vereinfacht werden muss, weil echte Distanzen im Vergleich zu Planetengroessen extrem gross sind.

Messwerte:

- Welche Controls wurden genutzt?
- Konnte die Person die Wirkung der Controls beschreiben?
- Aussage zur Skalierung: korrekt / teilweise korrekt / unklar
- Hilfestellung: keine / klein / deutlich

Bezug zur Forschungsfrage:

- Kernaufgabe fuer fachliche Nachvollziehbarkeit und raeumliche Erfahrbarkeit von Massstabsverhaeltnissen.

### Aufgabe 6 - Immersive Mode / Massstabseindruck pruefen

Aufgabe:

> Wechsle, falls verfuegbar, in die immersive Ansicht eines Planeten und beschreibe, wie sich dein Eindruck von Groesse oder Entfernung veraendert.

Beobachtung:

- Wird der Immersive Mode gefunden und wieder verlassen?
- Veraendert sich der wahrgenommene Massstab gegenueber der normalen Planet-Ansicht?
- Wirkt die grosse Entfernung bzw. Groessendarstellung beeindruckend, nachvollziehbar oder verwirrend?
- Bleibt die Orientierung erhalten?

Erfolgskriterium:

- Teilnehmer:in kann beschreiben, ob die immersive Ansicht den Massstabseindruck veraendert hat.

Hinweis:

- Falls der Immersive Mode im finalen Test-Build noch nicht stabil nutzbar ist, diese Aufgabe ueberspringen und als optional markieren.

Bezug zur Forschungsfrage:

- Raeumliche Erfahrbarkeit von Massstab: Die Aufgabe prueft, ob XR einen anderen Eindruck erzeugt als die normale Modellansicht.

### Aufgabe 7 - Reihenfolge-Minispiel

Aufgabe:

> Loese das Reihenfolge-Minispiel: Ordne die Planeten von der Sonne nach aussen.

Beobachtung:

- Ist die Grab-/Snap-Interaktion verstaendlich?
- Ist das Feedback nach richtiger oder falscher Zuordnung klar?
- Wirkt die Aufgabe motivierend oder frustrierend?

Messwerte:

- Aufgabe geschafft: ja/nein
- Anzahl Fehler oder Korrekturversuche
- Benoetigte Zeit
- Hilfestellung: keine / klein / deutlich

Bezug zur Forschungsfrage:

- Spielerisch motivierende Vermittlung: Die Reihenfolge wird nicht als Multiple Choice, sondern als raeumliche Grab-/Snap-Aufgabe geloest.

### Aufgabe 8 - Groessen-Minispiel

Aufgabe:

> Loese das Groessen-Minispiel: Ordne oder vergleiche die Planeten nach ihrer Groesse.

Beobachtung:

- Wird die relative Groesse der Planeten erkannt?
- Hilft das visuelle Feedback beim Lernen?
- Welche Planeten werden verwechselt?

Messwerte:

- Aufgabe geschafft: ja/nein
- Anzahl Fehler oder Korrekturversuche
- Benoetigte Zeit
- Hilfestellung: keine / klein / deutlich

Bezug zur Forschungsfrage:

- Direkte Pruefung des zentralen Massstabsthemas in spielerischer Form.

### Aufgabe 9 - Abschlussfrage direkt nach XR-Nutzung

Direkt im Anschluss, bevor der Post-Fragebogen beginnt:

> Was hast du ueber Groessen, Abstaende, Umlaufbahnen oder die Darstellung des Sonnensystems gelernt oder besser verstanden, was dir vorher weniger klar war?

Notieren:

- Spontane Antwort als qualitative Aussage.
- Besonders interessant sind Aussagen zu Groessen, Distanzen, Umlaufbahnen, Skalierung, Reihenfolge, Planetenvergleich und Motivation durch die Aufgaben.

---

## 7. Beobachtungsbogen

| Teilnehmer-ID | Aufgabe | Erfolgreich? | Zeit | Fehler/Korrekturen | Hilfe | Beobachtung/Kommentar |
|---|---|---:|---:|---:|---|---|
| P01 | Home/Planet starten und platzieren | | | | | |
| P01 | Planetendetails fachlich einordnen | | | | | |
| P01 | Groessenvergleich zwischen Planeten | | | | | |
| P01 | Sonnensystem raeumlich betrachten | | | | | |
| P01 | Massstabs-/Orbit-Controls verwenden | | | | | |
| P01 | Immersive Mode/Massstabseindruck | | | | | |
| P01 | Reihenfolge-Minispiel | | | | | |
| P01 | Groessen-Minispiel | | | | | |

> [NEU 2026-05-14 - START: Zusatzbeobachtung Setup/Tutorial]
>
> Zusatzzeilen fuer Setup und Tutorial:
>
> | Teilnehmer-ID | Phase | Erfolgreich? | Hilfe | Beobachtung/Kommentar |
> |---|---|---:|---|---|
> | P01 | Headset-Setup im Passthrough-Vorbereitungsmodus | | | |
> | P01 | Tutorial mit Erklaervideos | | | |
>
> Bei der Auswertung Setup-Hilfe und App-Hilfe getrennt betrachten. Hilfe beim Brillensitz ist kein Bedienproblem der App, kann aber als Hinweis auf die praktische Testsituation dokumentiert werden.
>
> [NEU 2026-05-14 - ENDE]

Zusaetzliche qualitative Codes fuer die Forschungsfrage:

- `Fachlich nachvollziehbar`: Person kann Skalierung, Groessenunterschiede oder Abstandsproblem korrekt erklaeren.
- `Raeumlich erfahrbar`: Person nutzt Perspektivwechsel, Bewegung im Raum oder immersive Ansicht, um das Modell zu verstehen.
- `Spielerisch motivierend`: Person wirkt durch Aufgabe, Feedback oder Herausforderung aktiviert statt nur durch das InfoPanel informiert.

Kodierung fuer Hilfe:

- 0 = keine Hilfe
- 1 = kleine verbale Hilfe
- 2 = deutliche Hilfe oder Eingreifen
- 3 = Aufgabe nicht ohne Hilfe moeglich

Kodierung fuer Motion Sickness / Unwohlsein:

- 0 = keine Anzeichen
- 1 = leichtes Unwohlsein
- 2 = deutliches Unwohlsein, Pause noetig
- 3 = Abbruch wegen Unwohlsein

---

## 8. Post-Fragebogen

### 8.1 Post-Wissensfragen

Empfehlung:

- Entweder dieselben 10 Wissensfragen wie im Pre-Fragebogen verwenden.
- Oder sehr aehnliche Parallelfragen verwenden, um reines Erinnern an den Fragebogen zu reduzieren.

Pragmatische Empfehlung fuer diese Masterarbeit:

- Dieselben Fragen verwenden, aber Reihenfolge der Fragen und Antwortoptionen im Online-Tool randomisieren, falls moeglich.
- In der Arbeit transparent machen, dass ein Wiederholungseffekt nicht vollstaendig ausgeschlossen werden kann.

Score:

- Jede richtige Antwort = 1 Punkt.
- `Knowledge_Pre` = 0-10
- `Knowledge_Post` = 0-10
- `Knowledge_Delta` = `Knowledge_Post - Knowledge_Pre`

### 8.2 Post-Selbsteinschaetzung

Skala: 1 = stimme gar nicht zu, 5 = stimme voll zu.

1. Ich kann die Reihenfolge der Planeten sicher benennen.
2. Ich kann mir die Groessenunterschiede zwischen den Planeten gut vorstellen.
3. Ich kann mir die Distanzen zwischen Sonne und Planeten gut vorstellen.
4. Ich verstehe, was eine Umlaufbahn ist.
5. Ich finde das Sonnensystem als Thema interessant.

Diese Items entsprechen den Pre-Items und koennen direkt vorher/nachher verglichen werden.

### 8.3 XR-Mehrwert

Skala: 1 = stimme gar nicht zu, 5 = stimme voll zu.

1. Die raeumliche Darstellung hat mir geholfen, das Sonnensystem besser zu verstehen.
2. Die Moeglichkeit, mich im Raum umzusehen, war fuer das Thema sinnvoll.
3. Die XR-Darstellung hat Groessen- und Distanzverhaeltnisse anschaulicher gemacht als eine normale Abbildung.
4. Die Bedienung hat mich nicht vom Lernen abgelenkt.
5. Ich hatte waehrend der Nutzung ein gutes Gefuehl fuer Orientierung im Raum.

### 8.4 Gamification und Motivation

Skala: 1 = stimme gar nicht zu, 5 = stimme voll zu.

1. Die Minigames haben mich motiviert, mich mit den Planeten zu beschaeftigen.
2. Das direkte Feedback in den Aufgaben hat mir beim Lernen geholfen.
3. Die spielerischen Aufgaben wirkten passend zum Thema Sonnensystem.
4. Die Aufgaben waren herausfordernd, aber nicht ueberfordernd.
5. Ich wuerde weitere Aufgaben oder Level in dieser Art ausprobieren.

### 8.5 Wissenschaftliche Korrektheit und Vertrauen

Skala: 1 = stimme gar nicht zu, 5 = stimme voll zu.

1. Die Anwendung wirkte fachlich glaubwuerdig.
2. Die Informationen zu den Planeten wirkten verstaendlich.
3. Die Anwendung hat deutlich gemacht, dass Darstellungen des Sonnensystems skaliert oder vereinfacht werden muessen.
4. Ich hatte den Eindruck, dass die Planeten nicht beliebig, sondern datenbasiert dargestellt werden.
5. Ich haette gerne Quellen oder weiterfuehrende Informationen direkt in der Anwendung.

### 8.6 Usability

Optional kann ein standardisierter Fragebogen wie SUS oder UEQ-S genutzt werden. Fuer eine kurze Masterarbeit-Evaluation ist eine reduzierte eigene Skala oft leichter auszuwerten, aber weniger standardisiert.

Empfehlung:

- Wenn du einen bekannten Score berichten moechtest: **SUS** verwenden.
- Wenn du pragmatische und hedonische UX trennen moechtest: **UEQ-S** verwenden.
- Wenn du es einfach halten willst: eigene 5-8 Usability-Items verwenden und nicht als standardisierten Score darstellen.

Eigene kurze Usability-Items:

Skala: 1 = stimme gar nicht zu, 5 = stimme voll zu.

1. Ich konnte die grundlegenden Funktionen der App gut bedienen.
2. Die Menues und Buttons waren gut erreichbar.
3. Die Texte waren in XR gut lesbar.
4. Ich wusste meistens, was ich als Naechstes tun sollte.
5. Die Interaktion mit Planeten und Aufgaben fuehlte sich nachvollziehbar an.
6. Ich habe mich waehrend der Nutzung koerperlich wohl gefuehlt.

### 8.7 Offene Fragen

1. Was hat dir an der Anwendung am meisten geholfen, das Sonnensystem zu verstehen?
2. Was war unklar oder schwierig?
3. Welche Funktion oder Information hat dir gefehlt?
4. Welche Szene oder Aufgabe war am motivierendsten?
5. Was sollte vor einer finalen Version verbessert werden?

---

## 9. Auswertung

### 9.1 Datenstruktur

Eine Zeile pro Teilnehmer:in.

| ID | Alter | XR_Erfahrung | Vorwissen_Selbsteinschaetzung | Knowledge_Pre | Knowledge_Post | Knowledge_Delta | Spatial_Pre | Spatial_Post | Spatial_Delta | ScaleExplanation | SpatialBehavior | OrderTask_Time | OrderTask_Errors | SizeTask_Time | SizeTask_Errors | XR_Mehrwert_Mean | Gamification_Mean | Usability_Mean |
|---|---:|---|---:|---:|---:|---:|---:|---:|---:|---|---|---:|---:|---:|---:|---:|---:|---:|
| P01 | | | | | | | | | | | | | | | | | | |

Metriken:

- `Knowledge_Pre`: Summe richtiger Antworten vor Nutzung.
- `Knowledge_Post`: Summe richtiger Antworten nach Nutzung.
- `Knowledge_Delta`: Differenz.
- `Spatial_Pre`: Mittelwert oder Median aus den Selbsteinschaetzungsitems zu Groesse, Distanz, Umlaufbahn.
- `Spatial_Post`: gleicher Wert nach Nutzung.
- `ScaleExplanation`: qualitative Kodierung der Abschluss- und Task-Aussagen: korrekt / teilweise korrekt / unklar.
- `SpatialBehavior`: qualitative Beobachtung, ob die Person den Raum aktiv nutzt: keine Bewegung / leichter Perspektivwechsel / aktives Umgehen oder Annahern.
- `XR_Mehrwert_Mean`: Mittelwert der XR-Mehrwert-Items.
- `Gamification_Mean`: Mittelwert der Gamification-Items.
- `Usability_Mean`: Mittelwert der eigenen Usability-Items oder separater SUS-/UEQ-S-Score.

### 9.2 Statistische Auswertung

#### Fuer H1: Fachliche Nachvollziehbarkeit

Primaere Analyse:

- Vergleich `Knowledge_Pre` vs. `Knowledge_Post`.
- Besonderer Fokus auf Items, die direkt mit Massstabsverhaeltnissen zusammenhaengen: Planetengroessen, groesster/kleinster Planet, Jupiter-Erde-Vergleich und Schwierigkeit echter Skalierung.
- Bei kleiner Stichprobe und nicht sicher normalverteilten Differenzen: **Wilcoxon-Vorzeichen-Rang-Test**.
- Gerichtete Hypothese moeglich: Post > Pre.
- Nullhypothese: Es gibt keinen systematischen Unterschied zwischen Pre- und Post-Wissensscore.
- Alternativhypothese: Der Post-Wissensscore ist hoeher als der Pre-Wissensscore.

Berichten:

- Median Pre, Median Post
- Mittelwert Pre/Post optional ergaenzend
- Wilcoxon-Teststatistik
- p-Wert
- Effektstaerke, z.B. `r = Z / sqrt(N)` oder rank-biserial correlation
- Anzahl Personen mit Verbesserung, Verschlechterung, keiner Veraenderung

Interpretation:

- Wenn p < .05 und Post > Pre: Hinweis auf fachlichen Wissenszuwachs.
- Wenn p nicht signifikant, aber viele Personen besser werden: vorsichtig als Tendenz beschreiben.
- Keine starke Kausalitaet behaupten, da es keinen Kontrollgruppenvergleich gibt.
- Zusaetzlich die qualitativen `ScaleExplanation`-Aussagen aus Aufgabe 5 und Aufgabe 9 nutzen, um zu zeigen, ob die Teilnehmer:innen die Notwendigkeit von Skalierung wirklich verstanden haben.

#### Fuer H2: Raeumliche Erfahrbarkeit

Analyse:

- Vergleich der Pre-/Post-Selbsteinschaetzung zu Groesse, Distanz und Umlaufbahn.
- Wegen Likert-Skalen: Wilcoxon-Vorzeichen-Rang-Test pro Item oder fuer einen zusammengefassten Skalenwert.
- Bei wenigen Teilnehmenden lieber Median/IQR plus qualitative Aussagen berichten.
- Beobachtungsdaten aus Aufgabe 4, 5 und 6 ergaenzen:
  - Hat die Person das Sonnensystem nur frontal betrachtet oder aktiv den Raum genutzt?
  - Konnte sie die Wirkung der Massstabs-/Orbit-Controls beschreiben?
  - Hat der Immersive Mode, falls genutzt, den Massstabseindruck veraendert?
  - Gab es Hinweise, dass die XR-Darstellung Abstaende und Groessen klarer machte oder eher verwirrte?

#### Fuer H3: Spielerische Motivation

Analyse:

- Deskriptive Auswertung der Gamification-Items.
- Task-Daten ergaenzen:
  - Erfolgsquote
  - Fehler/Korrekturen
  - Zeit
  - benoetigte Hilfe
- Offene Antworten thematisch clustern, z.B.:
  - Motivation durch Herausforderung
  - Motivation durch direktes Feedback
  - Frustration durch Bedienung
  - Lernen durch Ausprobieren
- Die Groessen-Aufgabe besonders gewichten, weil sie das zentrale Massstabsthema direkt spielerisch prueft.
- Das Reihenfolge-Minispiel getrennt berichten, weil es eher Ordnungswissen und raeumliche Zuordnung prueft.

#### Fuer H4: Bedienbarkeit

Analyse:

- Deskriptive Auswertung der Usability-Items oder SUS/UEQ-S.
- Beobachtungsdaten nutzen, um technische Probleme zu erklaeren.
- Wichtig: Wenn Bedienprobleme haeufig auftreten, koennen Lern- und Motivationswerte darunter leiden. Das sollte in der Diskussion nicht als Scheitern, sondern als relevante Erkenntnis fuer XR-Bildungsanwendungen eingeordnet werden.

### 9.3 Warum Wilcoxon hier sinnvoll ist

Der Wilcoxon-Vorzeichen-Rang-Test ist passend, wenn:

- dieselben Personen vor und nach der Nutzung gemessen werden,
- die Daten gepaart sind,
- die Stichprobe eher klein ist,
- keine Normalverteilung der Differenzen angenommen werden soll,
- Score- oder Likert-Daten verglichen werden.

Fuer den Wissensscore kann alternativ ein gepaarter t-Test verwendet werden, wenn die Differenzen ungefaehr normalverteilt sind. Bei einer typischen kleinen Masterarbeits-Stichprobe ist Wilcoxon aber defensiver und leichter zu begruenden.

Nicht ideal waere:

- Mann-Whitney-U-Test, weil er fuer unabhaengige Gruppen gedacht ist.
- Einfache Prozentvergleiche ohne gepaarte Auswertung, weil dadurch die Vorher-Nachher-Struktur verloren geht.

Zusaetzliche Option:

- Fuer einzelne Wissensfragen mit richtig/falsch vor/nachher kann ein **McNemar-Test** genutzt werden. Fuer diese Arbeit reicht aber wahrscheinlich der Gesamtscore plus qualitative Betrachtung der Fragen.

---

## 10. Auswertungstext fuer die Masterarbeit - Rohfassung

Moegliche Formulierung als Arbeitsgrundlage:

- Zur Evaluation des Prototyps wurde ein User Testing mit einem Pre-Post-Design durchgefuehrt. Ziel war es, Hinweise darauf zu gewinnen, ob eine XR-Lernanwendung zum Sonnensystem astronomische Massstabsverhaeltnisse fachlich nachvollziehbar, raeumlich erfahrbar und spielerisch motivierend vermitteln kann.
- Vor der Nutzung beantworteten die Teilnehmer:innen einen Fragebogen zu Vorerfahrung, Selbsteinschaetzung und Grundlagenwissen ueber das Sonnensystem. Anschliessend bearbeiteten sie mehrere Aufgaben innerhalb der XR-Anwendung, die zentrale Funktionen des aktuellen Prototyps abdeckten: Auswahl und Platzierung eines Planeten, Betrachtung von Planetendetails, Vergleich mehrerer Planeten, Platzierung des Sonnensystems, Anpassung von Massstabs- und Orbitparametern sowie die Nutzung der Minigames zur Planetenreihenfolge und zu Groessenverhaeltnissen.
- Nach der Nutzung wurde der Wissensfragebogen erneut erhoben. Ergaenzend wurden Items zu raeumlichem Verstaendnis, Motivation, Gamification, Usability und fachlicher Glaubwuerdigkeit abgefragt. Dadurch verbindet die Evaluation quantitative und qualitative Daten und orientiert sich direkt an den drei Teilaspekten der Forschungsfrage: fachliche Nachvollziehbarkeit, raeumliche Erfahrbarkeit und spielerische Motivation.
- Die Veraenderung des Wissensscores wurde aufgrund der gepaarten Messung und der voraussichtlich kleinen Stichprobe mit dem Wilcoxon-Vorzeichen-Rang-Test ausgewertet. Die Ergebnisse wurden nicht als allgemeingueltiger Wirkungsnachweis interpretiert, sondern als empirischer Hinweis darauf, ob der Prototyp das Verstaendnis der Teilnehmer:innen im Rahmen der Testsituation unterstuetzen konnte.
- Die offenen Antworten und Beobachtungsdaten wurden genutzt, um die quantitativen Ergebnisse einzuordnen. Besonders relevant waren dabei Aussagen dazu, ob die Teilnehmer:innen die Notwendigkeit didaktischer Skalierung verstanden, ob sie Groessen- und Distanzverhaeltnisse durch die XR-Darstellung raeumlich erfassen konnten und ob die spielerischen Aufgaben das aktive Anwenden dieses Wissens motivierend unterstuetzten.

---

## 11. Praktische Checkliste vor dem Testing

### App/Unity

> [NEU 2026-05-14 - START: Checkliste Setup/Tutorial]
>
> - User-Testing-Shortcut im finalen Build pruefen: beide Thumbsticks gleichzeitig -> Passthrough-Vorbereitungsmodus.
> - Erneuter Doppel-Thumbstick pruefen: App startet sauber mit Logo/Tutorial/Hauptmenue.
> - Pruefen, ob Passthrough im Setup wirklich sichtbar ist und keine App-Inhalte ablenken.
> - Pruefen, ob Audio im Setup pausiert und beim Appstart korrekt wiederhergestellt wird.
> - Tutorial-Videos im Headset pruefen: Loop, Lesbarkeit, Position, Ton falls vorhanden.
> - Tutorial-Abschlusssignale pruefen: Button/Menu/Geste/Grab-Schritt fuehrt zum naechsten Schritt.
> - Beobachtungsbogen um Setup/Tutorial-Notizen vorbereiten.
>
> [NEU 2026-05-14 - ENDE]

- Build auf Meta Quest 3 installieren.
- Controller-Ray, Buttons und Panels im Headset testen.
- Home-/Planet-Flow pruefen.
- Planetenauswahl pruefen.
- Planetendetail-Panel pruefen.
- Sonnensystem-Placement pruefen.
- Massstabs-/Orbit-Slider pruefen.
- Optionalen Immersive Mode pruefen und vor dem Testing entscheiden, ob Aufgabe 6 genutzt oder uebersprungen wird.
- Learn-/Test-Tab pruefen.
- Reihenfolge-Minispiel pruefen.
- Groessen-Minispiel pruefen.
- Rueckkehr ins Hauptmenue pruefen.
- Motion-Sickness-Risiken minimieren: keine ungewollten Kamerabewegungen, stabile Framerate, klare Orientierung.

### Studienmaterial

- Online-Fragebogen fertigstellen.
- Teilnehmer-IDs vorbereiten.
- Beobachtungsbogen vorbereiten.
- Einwilligungstext vorbereiten.
- Kurze Einweisung formulieren.
- Raum vorbereiten: freie Flaeche, Sitz-/Stehoption, keine Stolperquellen.
- Reinigung/Komfort: Headset-Polster, Brillentraeger:innen, Pausenmoeglichkeit.

### Daten

- Pre- und Post-Daten muessen ueber Teilnehmer-ID zusammenfuehrbar sein.
- Keine Klarnamen noetig.
- Daten direkt nach jedem Test sichern.
- Auffaellige Beobachtungen sofort notieren.

---

## 12. Quellen und TODOs

Bereits nuetzliche Quellen fuer Fakten und Methodik:

- NASA: Planetenuebersicht, acht Planeten und Klassifikation innerer/aeusserer Planeten: https://science.nasa.gov/solar-system/planets/
- NASA: Planetengroessen und Reihenfolge/Distanzen im Sonnensystem: https://science.nasa.gov/solar-system/planet-sizes-and-locations-in-our-solar-system/
- NASA: Solar System Sizes, relative Groessen der Planeten: https://science.nasa.gov/resource/solar-system-sizes/
- NASA Eyes: Beispiel fuer interaktive 3D-Datenvisualisierung des Sonnensystems: https://science.nasa.gov/eyes/
- SciPy-Dokumentation zum Wilcoxon-Vorzeichen-Rang-Test: https://docs.scipy.org/doc/scipy-1.12.0/reference/generated/scipy.stats.wilcoxon.html
- UEQ-Handbuch / UEQ-S als Option fuer UX-Auswertung: https://www.ueq-online.org/Material/Handbook.pdf

TODO:

- Pruefen, ob die Hochschule Vorgaben zu Datenschutz, Einwilligung, Ethik oder Studieninformationen hat.
- Entscheiden, ob SUS, UEQ-S oder eigene Usability-Items verwendet werden.
- Entscheiden, ob der Wissensfragebogen exakt gleich oder als Parallelversion Pre/Post eingesetzt wird.
- Vor dem finalen Einsatz 1-2 Pilot-Tests durchfuehren, um unklare Fragen und Aufgabenformulierungen zu erkennen.
- In der Masterarbeit klar markieren, dass die Evaluation bei kleiner Stichprobe explorativ ist und keine repraesentative Wirksamkeitsstudie ersetzt.
