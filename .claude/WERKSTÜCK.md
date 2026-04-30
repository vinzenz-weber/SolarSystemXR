# Werkstück – Projekt- und Prozesskapitel Sonnensystem XR

Diese Datei ist die Arbeitsfassung für die Kapitel, die den Entwicklungsprozess und das konkrete Projekt „Sonnensystem XR“ beschreiben. Der theoretische Hintergrund ist bereits separat in Word geschrieben und wird hier nicht erneut gesammelt.

## Arbeitstitel

TODO

## Leitidee

Die Masterarbeit untersucht eine XR-Anwendung für die Meta Quest 3, mit der Nutzerinnen und Nutzer das Sonnensystem erkunden und dabei astronomisches Wissen auf interaktive Weise vermittelt bekommen.

## 1. Einleitung

Wie in den vorangegangenen Kapiteln bereits dargestellt, ist für den nachhaltigen Lernerfolg in digitalen Anwendungen entscheidend, dass Wissen nicht nur passiv präsentiert, sondern durch aktive Anwendung und Überprüfung gefestigt wird. Gleichzeitig wurden die spezifischen Herausforderungen von XR-Anwendungen erörtert, insbesondere die Gefahr von Motion Sickness, die durch technische Unzulänglichkeiten wie eine niedrige oder instabile Bildwiederholrate erheblich verstärkt werden kann. Um die Motivation der Nutzenden langfristig zu fördern, wurden zudem Ansätze der Gamification als wesentlicher Bestandteil des Konzepts identifiziert.

Aus diesen grundlegenden Anforderungen leiten sich die zwei zentralen Leitlinien für die Konzeption und technische Entwicklung des „Sonnensystem XR“-Prototyps ab:

1. **Performance und Stabilität als oberste Priorität:** Eine durchgehend hohe und flüssige Bildrate ist nicht nur ein technisches Qualitätsmerkmal, sondern eine grundlegende Voraussetzung, um ein komfortables Nutzererlebnis ohne Motion Sickness zu gewährleisten. Wie die Projektdokumentation zeigt, führte dieses Prinzip zu weitreichenden Architekturentscheidungen, etwa dem Wechsel von instabilen, additiven Szenen-Ladevorgängen zu einer robusteren Single-Scene-Architektur oder der bewussten Entscheidung für die einfachere Meta Depth API anstelle des komplexeren MRUK-Stacks, um die Stabilität zu erhöhen.

2. **Aktive Wissensvermittlung durch Interaktion:** Anstatt die Nutzenden mit reinen Informationen zu konfrontieren, sollen sie durch interaktive Elemente dazu angeregt werden, das Gelernte anzuwenden. Dieses Prinzip manifestiert sich im Prototyp durch die Konzeption und Implementierung von Minispielen wie dem „Reihenfolge“- und „Größen“-Quiz. Anstatt eigene Interaktionslogiken von Grund auf neu zu entwickeln, wurde hierbei konsequent auf die bewährten Interaktions-Primitives des Meta XR SDK, wie die SnapExamples, zurückgegriffen, um den Fokus auf die Spielregeln und das didaktische Feedback legen zu können.

Dieses Kapitel dokumentiert den iterativen Entwicklungsprozess, der von diesen Leitlinien geprägt war – von der initialen Plattformwahl über entscheidende architektonische Umbauten bis hin zur konkreten Implementierung der Kern-Features, die diesen Prinzipien Rechnung tragen.

## Mögliche Kapitelstruktur

1. Einleitung
2. Ausgangspunkt und Anforderungen
3. Konzeption des Prototyps
4. Iterativer Entwicklungsprozess
5. Technische Architektur und zentrale Entscheidungen
6. Umsetzung der Kern-Features
7. Reflexion und Ausblick

## 5.1 Technisches Framework

Für die Nachvollziehbarkeit des Prototyps sind folgende technische Rahmendaten relevant:

- **Engine:** Unity `6000.3.10f1`
- **Programmiersprache:** C#
- **Zielhardware:** Meta Quest 3
- **Zielplattform:** Android / Standalone-XR
- **XR-Runtime:** OpenXR
- **XR-SDK:** Meta XR SDK `v85`
- **Render Pipeline:** Universal Render Pipeline (URP)
- **Grafiksystem:** URP-Materialien, Shader Graph, planetenspezifische Materialien, Atmosphären-/Lichteffekte und Wolkenanimation
- **Interaktionssystem:** Meta Interaction SDK
- **Meta Building Blocks / XR-Grundsetup:**
  - Controller-Tracking
  - Controller-Ray-Interaktion
  - Passthrough
  - Hand-Tracking
  - World-Space-Canvas-Interaktion
- **Raum- und Placement-System:** Meta Depth API über `EnvironmentRaycastManager`
- **Placement-Logik:** Controller-Ray, horizontale Flächenerkennung über Hit-Normal, Editor-Fallback ohne Depth API
- **UI-Technik:** World-Space-UI in Unity, TextMeshPro, manuell gestaltete UI-Prefabs und Panels
- **Zentrale UI-Strukturen:** `MenuRoot`, `PlanetDetailRoot`, `SolarSystemRoot`, Minigame-World-Space-UIs
- **App-Architektur:** Single-Scene-Architektur in `MainScene`, keine additiven Szenenwechsel im aktuellen Hauptflow
- **Zustandsverwaltung:** zentrale State Machine mit `MAIN_MENU`, `PLACEMENT`, `WORLD`, `IMMERSIVE` und `TEST_*`
- **Datenstruktur:** `PlanetData` als ScriptableObject für planetenspezifische Daten, Texte, Kennwerte und Prefab-Referenzen
- **Sonnensystem-Simulation:** `SolarSystemManager` für Planetenerzeugung, Orbit-Berechnung, Skalierung und Simulationsparameter
- **Planeten-Interaktion:** generischer Interactable-Wrapper, visuelles Planet-Prefab wird datenbasiert aus `PlanetData` eingesetzt
- **Info- und Detailanzeige:** globales Planet-InfoPanel mit datenbasiertem Binding über `PlanetData`
- **Minigame-Interaktion:** Grab- und Snap-Mechaniken aus dem Meta Interaction SDK, insbesondere nach Vorbild der `SnapExamples`
- **Minigame-Logik:** eigene Prüfsysteme für Reihenfolge und Größe, SDK übernimmt die technische Interaktion
- **Mixed-Reality-Funktionen:** Passthrough/VR-Umschaltung über `PassthroughDissolver`
- **Immersive-Ansicht:** aktivierbarer Immersive-Root innerhalb der MainScene, Planetendarstellung an einem entfernten Spawnpunkt
- **Testumgebung Windows:** Unity Playmode für UI- und Flow-Tests, Quest Link bzw. Headset für echte XR- und Depth-API-Tests
- **Build-/Verifikation:** C#-Kompilierung über `dotnet build Assembly-CSharp.csproj --no-restore`, finale Funktionsprüfung im Unity Editor und auf der Quest 3
- **Projektorganisation:** zentrale technische Dokumentation in `.claude/DOKU.md`, Iterationshistorie in `.claude/ARCHIV.md`, Projekt-/Prozesskapitel in `.claude/WERKSTÜCK.md`

## 5.2 User Experience und Interaction Design

Die User Experience von „Sonnensystem XR“ wird wesentlich durch die Besonderheiten einer XR-Anwendung bestimmt. Die Anwendung wird nicht auf einem flachen Bildschirm bedient, sondern im Raum vor dem User platziert. Dadurch verschieben sich zentrale Gestaltungsfragen: Menüs müssen nicht nur lesbar und funktional sein, sondern auch räumlich sinnvoll positioniert werden; Interaktionen müssen mit Controllern, Raycasts, Grab- und Snap-Mechaniken funktionieren; und die Nutzerinnen und Nutzer müssen jederzeit verstehen, in welchem Zustand der Anwendung sie sich befinden.

Für den Prototyp war deshalb nicht nur entscheidend, welche Inhalte gezeigt werden, sondern wie diese Inhalte in eine verständliche räumliche Handlung übersetzt werden. Die Anwendung verbindet drei Formen der Nutzung: Auswahl und Orientierung im Home-Bereich, explorative Betrachtung von Planeten und Sonnensystem sowie aktive Wissensanwendung in den Minigames. Diese Bereiche dürfen sich nicht wie voneinander getrennte technische Einzelprototypen anfühlen, sondern müssen in einem gemeinsamen Bedienfluss zusammenhängen.

Ein zentrales Gestaltungsprinzip ist dabei die Reduktion technischer Reibung. Nutzerinnen und Nutzer sollen nicht lernen müssen, wie ein Unity-Prototyp funktioniert, sondern sich auf das Sonnensystem und die Aufgaben konzentrieren können. Deshalb werden wiederkehrende Bedienmuster verwendet: Auswahl über Ray-Interaktion, Platzierung über Controller-Ray auf horizontalen Flächen, Rückkehr über das Hauptmenü bzw. definierte Exit-Buttons und direkte Rückmeldung in den Minigames. Die State Machine des Projekts unterstützt diese UX-Struktur technisch, indem sie klar zwischen Hauptmenü, Placement, World-Ansicht, Immersive Mode und Minigame-Zuständen trennt.

Für die Masterarbeit ist diese Ebene besonders relevant, weil sie die theoretischen Schwerpunkte miteinander verbindet. Wissensvermittlung in XR entsteht hier nicht allein durch Informationstexte, sondern durch räumliche Darstellung und Handlung. Gamification entsteht nicht nur durch Punkte oder Fortschritt, sondern durch Aufgaben, Feedback und Korrekturschleifen. Wissenschaftliche Korrektheit bleibt dabei an die Datenstruktur gekoppelt: Planetendaten und Größenverhältnisse werden nicht frei in UI-Elemente geschrieben, sondern über `PlanetData` und zentrale Simulations- bzw. Prüfsysteme verarbeitet.

TODO: Hier später kurz auf den finalen Figma-/UI-Designprozess eingehen: Ausgangspunkt der UI-Entwürfe, wichtigste Layoutentscheidungen, visuelle Sprache, Iterationen zwischen Figma und Unity sowie Begründung, warum die finalen Panels als manuell gestaltete World-Space-UI umgesetzt wurden.

### 5.2.1 Spatial UI

Die Benutzeroberfläche des Prototyps ist als Spatial UI angelegt. Das bedeutet, dass Menüs, Detailansichten und Steuerpanels nicht als klassische Overlay-UI über dem Bild liegen, sondern als World-Space-Elemente in der XR-Szene erscheinen. Diese Entscheidung passt zum Medium, erzeugt aber eigene Anforderungen: Die Panels müssen in einer angenehmen Entfernung stehen, groß genug lesbar sein, mit Controller-Rays getroffen werden können und dürfen den Blick auf die räumlichen Inhalte nicht unnötig verdecken.

Im aktuellen Prototyp werden die zentralen UI-Panels nicht mehr zur Laufzeit per Code erzeugt. Stattdessen sind `MenuRoot`, `PlanetDetailRoot`, `SolarSystemRoot` bzw. die Minigame-UIs als gestaltete UI-Strukturen im Unity Editor vorbereitet und mit der Logik verbunden. Diese Entscheidung entstand aus der praktischen Erfahrung, dass World-Space-UI in XR sehr empfindlich auf Canvas-Setup, Skalierung, Raycast-Konfiguration und Hierarchie reagiert. Eine zur Laufzeit erzeugte UI kann im Editor schnell funktionieren, im Headset aber durch falsche Größe, schlechte Lesbarkeit oder fehlende Interaction-SDK-Komponenten unzuverlässig werden.

Das Hauptmenü bildet den räumlichen Einstiegspunkt. Es ist kein dekorativer Startscreen, sondern ein funktionales Auswahlpanel. Nutzerinnen und Nutzer wählen dort zwischen Planet-Ansicht, Sonnensystem-Ansicht und Learn- bzw. Testbereich. Zusätzlich ermöglicht ein Toggle den Wechsel zwischen Passthrough-AR und VR-Hintergrund. Dadurch wird die räumliche Darstellung flexibel: Im AR-Modus erscheint das Sonnensystem im realen Raum, während der VR-Modus eine reduzierte dunkle Umgebung bietet, in der die Himmelskörper visuell stärker im Vordergrund stehen.

Das globale `PlanetDetailRoot` ist ein zweites zentrales Spatial-UI-Element. Es wird nach der Auswahl oder Platzierung eines Planeten mit den Daten des aktuellen `PlanetData`-Assets befüllt. Dadurch bleibt das Bedienkonzept konsistent: Es gibt nicht für jeden Planeten ein eigenes Detailfenster, sondern ein gemeinsames Panel, dessen Inhalt wechselt. Für die User Experience ist das wichtig, weil die Nutzerinnen und Nutzer nicht für jeden Planeten eine neue UI-Logik interpretieren müssen. Für die technische Struktur ist es gleichzeitig wartbarer, weil Layout und Datenbindung getrennt bleiben.

Die Positionierung des Detailpanels wurde bewusst räumlich gedacht. Das Panel erscheint links-vorne relativ zur horizontalen Blickrichtung des Users. Dadurch hängt seine Position nicht direkt von der Kopfneigung ab. Wenn der User nach oben oder unten schaut, soll das Panel nicht unkontrolliert in diese Richtung wandern, sondern als stabiler Bedienanker im Raum bleiben. Diese Entscheidung zeigt, dass Spatial UI nicht nur aus dem Platzieren eines Canvas im Raum besteht, sondern aus einer genauen Abstimmung zwischen Blickrichtung, Lesbarkeit und Interaktionskomfort.

Auch das Sonnensystem-Panel folgt dieser Logik. Es steuert die aktuell platzierte Sonnensystem-Instanz und wird nicht über eine statische Inspector-Referenz an irgendein Modell gebunden. Die UI erhält den passenden `SolarSystemManager` nach dem Placement. Dadurch entspricht die Bedienung der Erwartung: Wenn ein Sonnensystem im Raum platziert wurde, verändern die Slider genau dieses sichtbare Modell. Die Steuerung wird damit zu einem didaktischen Werkzeug, weil Nutzerinnen und Nutzer Größen, Abstände, Geschwindigkeit, Achsneigung und Exzentrizität direkt am räumlichen Modell nachvollziehen können.

Für die Minigames wird Spatial UI ebenfalls als vorbereiteter Bestandteil der jeweiligen Prefabs genutzt. Die Aufgabe, die Buttons und das Feedback liegen im Raum vor dem User und werden zusammen mit dem Minigame platziert. Das ist für die Konsistenz wichtig, weil die Tests nicht als separate 2D-Menüs erscheinen, sondern Teil der XR-Erfahrung bleiben. Zugleich vermeidet diese Struktur, dass jedes Minigame seine UI zur Laufzeit selbst erzeugt und dabei eigene Layout- oder Interaktionsprobleme einführt.

TODO: Figma-Prozess ergänzen: Welche UI-Flächen wurden zuerst in Figma gestaltet? Welche Elemente wurden aus Figma nach Unity übertragen? Welche Anpassungen waren in Unity nötig, weil eine 2D-Figma-Komposition in World-Space-UI anders wirkt? Welche Panels sind final, welche noch Platzhalter?

TODO: Falls vorhanden, Screenshots oder Abbildungsverweise ergänzen: Figma-Entwurf des `MenuRoot`, finale Unity-Umsetzung, `PlanetDetailRoot`, Sonnensystem-Slider-Panel und Minigame-UI.

### 5.2.2 Steuerung, Orientierung und Präsenz

Die Steuerung des Prototyps basiert auf klar getrennten Interaktionszuständen. Im Hauptmenü wählen Nutzerinnen und Nutzer eine Experience aus. Im Placement-State wird das gewählte Objekt zunächst als Vorschau entlang des Controller-Rays angezeigt und kann auf einer geeigneten horizontalen Fläche platziert werden. Nach der Platzierung wechselt die Anwendung in die World-Ansicht. Dort stehen die räumlichen Inhalte im Vordergrund: ein einzelner Planet mit Detailpanel oder das Sonnensystem mit Steuerpanel. Für Minigames gibt es eigene Test-Zustände, damit Grab-, Snap- und Feedbacklogik nicht mit der normalen Planeten-Auswahl kollidieren.

Diese Trennung ist aus UX-Sicht entscheidend, weil sie Mehrdeutigkeiten reduziert. Derselbe Controller-Trigger darf nicht gleichzeitig ein Objekt platzieren, einen Planeten auswählen und ein Minigame-Objekt greifen. Der Prototyp löst das über die State Machine: `PLACEMENT` ist für Platzierung zuständig, `WORLD` für Auswahl und Betrachtung, `IMMERSIVE` für die VR-Naturansicht und `TEST_*` für die Minigames. Dadurch wird nicht jede Interaktion global ausgewertet, sondern nur in dem Zustand, in dem sie fachlich sinnvoll ist.

Die Platzierung im Raum unterstützt Orientierung und Präsenz, weil Nutzerinnen und Nutzer den Startpunkt der Erfahrung selbst bestimmen. Ein Planet oder das Sonnensystem erscheint nicht an einer abstrakten Standardposition, sondern wird über den Controller-Ray auf Boden oder Tisch gesetzt. Die Anwendung prüft dabei die Normalenrichtung der getroffenen Fläche, sodass vor allem horizontale Flächen akzeptiert werden. Im Quest-Build nutzt dieser Vorgang die Depth API; im Unity-Editor gibt es einen vereinfachten Fallback, damit UI- und Flow-Tests auch ohne Headset möglich bleiben.

Durch die Platzierung entsteht eine erste Form von Präsenz: Der Planet oder das Sonnensystem wird als Objekt im eigenen Umfeld wahrgenommen. Gerade für eine Bildungsanwendung ist diese räumliche Verankerung wichtig, weil sie das Thema Sonnensystem aus der reinen Bildschirmdarstellung herauslöst. Nutzerinnen und Nutzer betrachten die Inhalte nicht nur, sondern bewegen sich zu ihnen in Beziehung: Sie wählen eine Position, schauen auf das Objekt, bedienen ein Panel daneben und können Parameter oder Aufgaben im Raum manipulieren.

Die Orientierung wird zusätzlich durch wiederkehrende Rückwege stabilisiert. Die Options-Taste öffnet aus den World- und Test-Zuständen wieder das Hauptmenü bzw. beendet aktive Sonderzustände. Der Immersive Mode nutzt das bestehende Detailpanel als Exit-UI. Dadurch müssen Nutzerinnen und Nutzer nicht mehrere unabhängige Navigationsprinzipien lernen. Stattdessen gibt es wiedererkennbare Anker: Hauptmenü, Detailpanel, Steuerpanel und Minigame-Buttons.

Für die Interaktion mit Planeten und Minigames wird bewusst auf Komponenten des Meta Interaction SDK zurückgegriffen. Ray-Auswahl, Canvas-Interaktion, Grab- und Snap-Mechaniken sind typische XR-Interaktionsmuster, die im SDK bereits als getestete Bausteine vorhanden sind. Der eigene Projektcode übernimmt vor allem die fachliche Logik: Welcher Planet wurde ausgewählt? Welche Daten werden angezeigt? Welcher Slot ist richtig? Welche Größenreihenfolge ergibt sich aus den Durchmessern? Dadurch bleibt die Bedienung näher an etablierten Quest-Interaktionsmustern und die projektspezifische Komplexität wird reduziert.

Die Minigames nutzen Steuerung nicht nur als Eingabemethode, sondern als Teil der Lernhandlung. Im Reihenfolge-Minispiel werden Planeten gegriffen und auf Orbit-Slots gelegt. Im Größen-Minispiel werden Planetenkörper in eine Größenordnung gebracht und bei korrekter Platzierung sichtbar skaliert. Die Nutzerinnen und Nutzer beantworten also keine rein abstrakten Fragen, sondern handeln im Raum. Diese körperlich-räumliche Komponente unterstützt den XR-Anspruch des Projekts: Wissen soll nicht nur gelesen, sondern durch Interaktion und unmittelbares Feedback erfahrbar werden.

Der Wechsel zwischen Passthrough-AR und VR-Hintergrund beeinflusst ebenfalls die Präsenz. Passthrough bindet die Anwendung stärker an den realen Raum und eignet sich für das Platzieren von Planeten oder Sonnensystem auf realen Flächen. Der VR-Hintergrund reduziert dagegen visuelle Ablenkung und kann die Himmelskörper stärker inszenieren. Der Immersive Mode geht noch einen Schritt weiter, indem ein ausgewählter Planet in einer VR-Natur- bzw. Nachtszene am entfernten Spawnpunkt dargestellt wird. Damit verschiebt sich die Experience von der objektbezogenen Betrachtung im Raum zu einer stärker atmosphärischen Himmelskörper-Ansicht.

Gleichzeitig bleibt die Präsenz an technische Stabilität gebunden. Ruckeln, schwarze Übergänge, falsch positionierte Panels oder unklare Controllerzustände würden die Aufmerksamkeit sofort von den Lerninhalten abziehen. Deshalb wurden im Prototyp mehrere spektakulärere, aber instabilere Ansätze reduziert oder ersetzt: keine additiven Szenenwechsel für den immersiven Modus, keine zur Laufzeit generierten Kern-Panels, keine eigene Grab-/Snap-Logik für Minigames, wenn die Meta-Samples diese Aufgabe bereits abdecken. Die UX-Entscheidung ist hier also auch eine technische Entscheidung: Stabilität und Verständlichkeit haben Vorrang vor unnötiger Komplexität.

TODO: Nach Headset-Tests ergänzen: Wie gut funktionieren Lesbarkeit, Panelabstand, Controller-Ray-Treffer, Placement und Snap-Interaktion tatsächlich auf der Quest 3? Welche Anpassungen wurden nach Tests im Headset vorgenommen?

## 5.3 Prototyping & Iterativer Designprozess

Die Entwicklung des Prototyps verlief nicht als geradlinige Umsetzung eines vorab vollständig festgelegten Systems, sondern als iterativer Design- und Technikprozess. Das war für dieses Projekt besonders relevant, weil mehrere Ebenen gleichzeitig überprüft werden mussten: die grundsätzliche Machbarkeit auf der Meta Quest 3, die Stabilität der XR-Interaktion, die visuelle Wirkung der Planeten, die Verständlichkeit der Lerninhalte und die Frage, ob gamifizierte Elemente tatsächlich sinnvoll in den Ablauf eingebunden werden können.

Der Prototyp wurde daher schrittweise aufgebaut, getestet, verworfen und neu strukturiert. Viele Entscheidungen entstanden nicht aus rein theoretischen Überlegungen, sondern aus konkreten Entwicklungserfahrungen: veraltete SDK-Komponenten, instabile Szenenwechsel, schwer wartbare UI-Hierarchien oder Interaktionslogiken, die im Editor funktionierten, aber für den Quest-XR-Kontext zu fehleranfällig gewesen wären. Das Archiv dokumentiert diese Änderungen als Abfolge von Umbauten und Lernmomenten; die Git-Historie zeigt ergänzend, dass sich der Fokus von frühen Visualisierungs- und Shader-Experimenten über XR-Grundlagen und Placement hin zu einem stabileren, datengetriebenen Lern- und Testsystem verschoben hat.

Für die Masterarbeit ist dieser iterative Verlauf ein wichtiger Bestandteil des Werkstücks. Die Anwendung soll nicht nur technisch funktionieren, sondern zugleich die in der Theorie herausgearbeiteten Anforderungen erfüllen: Sie muss für Lernende verständlich sein, Motivation durch Interaktion und Aufgaben erzeugen und auf mobiler XR-Hardware stabil genug laufen, um nicht selbst zum Hindernis für die Wissensvermittlung zu werden. Der Prototyping-Prozess kann daher als fortlaufende Annäherung an drei Leitfragen verstanden werden:

1. Welche Form der Darstellung ist visuell überzeugend, aber performant genug für Standalone-XR?
2. Welche Interaktionen unterstützen Lernen und Exploration, ohne die Nutzerinnen und Nutzer technisch zu überfordern?
3. Welche Architektur erlaubt es, astronomische Daten und Lerninhalte zu erweitern, ohne bei jeder Änderung neue Speziallösungen bauen zu müssen?

### 5.3.1 Erster Prototyp

Der erste Prototyp entstand zunächst nicht in Unity, sondern in Unreal Engine. Dieser Einstieg war vor allem visuell motiviert. Schon frühe Tests konnten mit vergleichsweise wenig Aufwand eine überzeugende Raumwirkung und eine hochwertige Darstellung erzeugen. Für ein Projekt über das Sonnensystem war dieser erste Eindruck wichtig, weil die Faszination des Themas stark über Maßstab, Licht, Dunkelheit und die visuelle Präsenz der Himmelskörper vermittelt wird.

Gleichzeitig zeigte sich in dieser frühen Phase, dass eine gute visuelle Wirkung allein nicht ausreicht. Die technische Integration der Meta-Quest-XR-Funktionen erwies sich als schwer planbar. Die verfügbaren Integrationswege wirkten teilweise veraltet oder nicht ausreichend dokumentiert, und bei der Umsetzung traten wiederholt Probleme auf, die weniger mit der eigentlichen Lernidee als mit der XR-Grundinfrastruktur zusammenhingen. Für einen Masterarbeits-Prototyp, der in begrenzter Zeit entstehen muss, wurde dadurch das Risiko zu groß, zu viel Entwicklungszeit in die Stabilisierung der Plattform statt in die eigentliche Experience zu investieren.

Aus diesem Grund wurde die Entwicklung auf Unity umgestellt. Diese Entscheidung war nicht nur eine technische Präferenz, sondern ein erster größerer Iterationsschritt: Der Fokus verschob sich von maximaler visueller Qualität zu einer Umgebung, in der Meta-XR-Integration, C#-basierte Logik und datengetriebene Systeme besser zusammenpassen. Gerade für die Berechnung von Planetenbahnen, Skalierungsfaktoren und späteren Quizlogiken war C# nachvollziehbarer als ein stark visuell verdrahteter Blueprint-Ansatz. Die Entscheidung für Unity markierte damit den Übergang vom reinen Darstellungsprototyp zu einem System, das langfristig erweiterbar und für den Entwickler kontrollierbarer sein sollte.

Die frühen Unity-Versionen zeigen diesen Suchprozess deutlich. Zunächst wurden Projektstruktur, Planetenobjekte, Shader und grundlegende Sonnensystemlogik aufgebaut. Inhaltlich ging es in dieser Phase darum, überhaupt ein glaubwürdiges und bedienbares Grundmodell zu erhalten: Planeten mussten sichtbar sein, Materialien mussten atmosphärisch wirken, Umlaufbahnen mussten nachvollziehbar berechnet werden und eine erste UI musste den Einstieg in die Experience ermöglichen. Die Git-Historie wurde hier vor allem als Rekonstruktionshilfe genutzt; im Fließtext steht jedoch die Entwicklungslinie im Vordergrund, nicht die Benennung einzelner Commits.

Bereits hier wurde ein zentrales Muster sichtbar, das den weiteren Prozess prägen sollte: Funktionen wurden zunächst schnell gebaut, anschließend aber anhand der XR-Anforderungen wieder vereinfacht oder neu zugeschnitten. Der frühe Prototyp war dadurch weniger ein abgeschlossenes Produkt als ein Testfeld, auf dem grundlegende Annahmen überprüft wurden. Besonders wichtig war die Erkenntnis, dass die Anwendung nicht nur „schön aussehen“ darf, sondern robust bedienbar sein muss. In XR führt jede instabile Interaktion, jedes schwer lesbare Panel und jeder ruckelige Übergang unmittelbar zu einem schlechteren Nutzungserlebnis. Für eine Bildungsanwendung ist das besonders problematisch, weil technische Reibung die Aufmerksamkeit von den Lerninhalten abzieht.

### 5.3.2 Vom Funktionsprototyp zum XR-Prototyp

Nach dem Wechsel zu Unity verlagerte sich der Schwerpunkt auf die Frage, wie aus einzelnen Funktionen eine zusammenhängende XR-Erfahrung werden kann. Früh entstanden erste Zustandsmodelle, die den Ablauf der Anwendung steuern sollten. Das Archiv dokumentiert mehrere Iterationen: Zunächst war ein sehr einfacher Ablauf aus Start, Platzierung und Exploration vorgesehen. Anschließend wurde ein Auswahlzustand ergänzt, damit Nutzerinnen und Nutzer zuerst entscheiden können, ob sie ein Sonnensystem oder einen einzelnen Planeten betrachten möchten. In einer weiteren Iteration wurde der Umfang deutlich erweitert: Placement, Sonnensystemansicht, schwebender Einzelplanet und ein immersiver Modus wurden als getrennte Zustände gedacht.

Diese Erweiterung war aus konzeptioneller Sicht nachvollziehbar, weil sie viele Ideen des späteren Prototyps bereits vorwegnahm. Technisch führte sie aber zu einer hohen Komplexität. Mehrere UI-Panels, unterschiedliche Passthrough-Zustände, Placement-Logik, InfoPanel-Systeme und ein immersiver Szenenwechsel mussten gleichzeitig koordiniert werden. In dieser Phase wurde deutlich, dass ein XR-Prototyp nicht nur durch neue Features wächst, sondern auch durch die Fähigkeit, den Umfang wieder zu reduzieren, wenn die technische Stabilität darunter leidet.

Ein Beispiel dafür ist das frühe InfoPanel-System. Zunächst lag das steuernde Script direkt auf dem World-Space-Canvas, der zur Laufzeit ausgeblendet wurde. Dadurch deaktivierte Unity nicht nur das sichtbare Panel, sondern auch das Script selbst; die eigentlich dauerhaft benötigte Raycast-Logik konnte nicht mehr weiterlaufen. Die Lösung bestand darin, die steuernde Logik auf ein persistent aktives Systemobjekt auszulagern und nur das sichtbare Canvas ein- oder auszublenden. Diese kleine technische Korrektur hatte größere Bedeutung für den weiteren Entwurf: Steuerlogik und sichtbare UI sollten getrennt werden, damit die Anwendung nicht durch das Aktivieren oder Deaktivieren von UI-Elementen ihren eigenen Zustand verliert.

Ähnlich verlief die Entwicklung der Eingabe- und Placement-Logik. Erste Varianten mischten Desktop-Eingaben, OVRInput, Hand-Tracking und unterschiedliche Raycast-Ansätze. Für Tests im Editor war das hilfreich, für eine stabile Quest-Anwendung jedoch schwer wartbar. Deshalb wurde der Input in mehreren Schritten vereinfacht und stärker an den tatsächlichen XR-Fluss gebunden. Aus diesen Iterationen entstand die spätere Projektregel, vorhandene Meta-XR-Komponenten und Building Blocks bevorzugt zu nutzen und eigenen Interaktionscode nur dort zu schreiben, wo wirklich projektspezifische Logik benötigt wird.

### 5.3.3 Technische Sackgassen und bewusste Reduktion

Eine der wichtigsten Iterationen betraf die Erkennung und Platzierung von Objekten im Raum. Zunächst wurden verschiedene Wege über `OVRSceneManager`, `OVRSceneAnchor`, Physics-Collider und später MRUK ausprobiert. Das Archiv zeigt, dass diese Ansätze nicht einfach „falsch“ waren, sondern jeweils auf dem damaligen Kenntnisstand nahelagen. Sie wurden jedoch durch die Entwicklung des Meta SDK zunehmend problematisch: Bestimmte APIs waren veraltet, andere hatten sich zwischen SDK-Versionen geändert, und die Kombination mehrerer SDK-Generationen erzeugte zusätzliche Fehlerquellen.

Der entscheidende Umbau erfolgte im Zuge des Rebuilds von Placement und Hauptmenü. Dabei wurde der vorherige Prototyp-Vollumfang bewusst reduziert. Das Zustandsmodell wurde von einem komplexeren Modell mit mehreren Spezialzuständen auf einen schlankeren Ablauf zurückgeführt. Mehrere alte Implementierungen wurden entfernt und die Platzierung auf die Meta Depth API umgestellt. Statt einen vollständigen MRUK-Raum mit semantischen Raumdaten zu benötigen, nutzt die neue Platzierung einen Raycast gegen das Depth-Mesh und prüft anhand der Flächennormale, ob eine annähernd horizontale Fläche getroffen wurde.

Dieser Umbau ist für den Designprozess besonders aussagekräftig, weil er zeigt, dass Fortschritt im Prototyping nicht immer durch Hinzufügen entsteht. In diesem Fall entstand Fortschritt durch Weglassen. Für die konkrete Anwendung war keine vollständige semantische Raumerkennung nötig; es reichte, Planeten oder das Sonnensystem stabil auf Boden- oder Tischflächen platzieren zu können. Damit wurde eine einfachere technische Lösung gewählt, die besser zum tatsächlichen Nutzungsszenario passte.

Auch der immersive Modus durchlief eine solche Reduktion. Zwischenzeitlich gab es den Ansatz, eine Raumstation oder separate immersive Szene additiv zu laden. Diese Idee war visuell reizvoll, führte aber im XR-Kontext zu schwarzen Bildschirmen, Ruckeln und State-Problemen. Zudem musste beim Teleportieren des Spielers beachtet werden, dass die Rotation des OVR-Player-Roots nicht verändert werden darf, weil sonst der Tracking-Raum selbst mitrotiert. Aus diesen Problemen entstand später die Entscheidung, immersive Inhalte nicht mehr über Szenenwechsel, sondern innerhalb der MainScene über aktivierbare Root-Objekte und Passthrough-Dissolve zu organisieren. Auch hier wurde also eine spektakulärere technische Idee zugunsten eines stabileren Prototyp-Flusses zurückgenommen.

### 5.3.4 Datengetriebene Struktur als Ergebnis der Iterationen

Parallel zu den technischen Umbauten wurde immer deutlicher, dass planetenspezifische Informationen nicht in UI- oder Interaktionslogik verteilt werden dürfen. Der Prototyp sollte zunächst mit einzelnen Planeten funktionieren, später aber ohne großen Umbau auf alle acht Planeten erweitert werden können. Diese Anforderung führte zum datengetriebenen Ansatz mit zentralen Planeten-Datenassets.

Diese Entscheidung ist nicht nur eine technische Ordnungshilfe, sondern eine direkte Antwort auf die vorherigen Iterationen. Wenn Features verworfen oder umgebaut werden, dürfen astronomische Daten, Beschreibungstexte, Prefab-Referenzen und Skalierungswerte nicht in der jeweiligen Featurelogik verloren gehen. Durch die Auslagerung in ScriptableObjects können UI, Placement, Sonnensystemsimulation und Minigames generisch auf dieselben Daten zugreifen. Die planetenspezifische Information bleibt an einer Stelle, während die Systeme austauschbar bleiben.

Für den didaktischen Anspruch der Arbeit ist das wichtig, weil fachliche Inhalte dadurch besser pflegbar werden. Wenn Werte oder Texte später mit belastbaren Quellen abgeglichen werden, müssen sie nicht in mehreren Scripts gesucht und angepasst werden. Zugleich unterstützt die Struktur den Gamification-Ansatz: Quizlogik kann zum Beispiel den Durchmesser, die Reihenfolge oder weitere Eigenschaften aus denselben Daten lesen, die auch für die erklärende Ansicht verwendet werden. Dadurch entsteht eine engere Verbindung zwischen Exploration und Wissensprüfung.

TODO: Astronomische Daten und Beschreibungstexte vor finaler Abgabe noch systematisch mit belastbaren Quellen abgleichen und Quellen im Methodik-/Materialteil dokumentieren.

### 5.3.5 Von eigener Interaktionslogik zu Meta-SDK-nahen Systemen

Ein weiterer prägender Lernprozess betraf die XR-Interaktion. In frühen Versionen wurden Raycasts, Controller-Eingaben und Grab-Mechaniken teilweise selbst umgesetzt. Ein Beispiel ist der frühere Ansatz für Greif-Handles, bei dem eigene Raycasts, Input-Abfragen, Materialwechsel und zur Laufzeit erzeugte Handle-Objekte kombiniert wurden. Später wurde dieser Ansatz durch ein Prefab auf Basis des Meta Interaction SDK ersetzt. Die projektspezifische Logik schrumpfte dadurch deutlich: Statt Greifen, Raycast und Transformation selbst zu steuern, musste nur noch die Verbindung zwischen SDK-Interaktion und dem zu bewegenden Objekt hergestellt werden.

Diese Erfahrung beeinflusste direkt die späteren Minigames. Für das Reihenfolge-Minispiel wurde nicht versucht, eine eigene Grab- und Snap-Logik zu schreiben. Stattdessen wurde das Setup der Meta Interaction SDK Samples als Grundlage verwendet. Der eigene Code beschränkt sich auf die fachliche Regel: Welcher Planet gehört auf welchen Orbit, wie wird ein Slot bewertet und welches Feedback erhält der User? Das Greifen, Snappen und die Controller-Hand-Posen bleiben beim SDK.

Auch beim Größen-Minispiel wiederholte sich diese Logik. Zunächst war die genaue Funktionsweise der automatischen Snap-Liste unklar. Erst durch den Vergleich mit den Meta-Samples wurde deutlich, dass nicht die Liste selbst das Problem war, sondern fehlende Referenzen an den einzelnen Planetenobjekten. Diese Erkenntnis war ein typischer Prototyping-Moment: Das sichtbare Symptom lag an einer anderen Stelle als zunächst vermutet. Aus der Fehlersuche entstand ein stabileres Verständnis der SDK-Komponenten und die Entscheidung, die Snap-Liste aus den Samples als wiederverwendbares Muster im Projekt zu behandeln.

Für den Designprozess bedeutete das eine klare Arbeitsteilung: Die Meta-Samples liefern robuste Interaktions-Primitives, der eigene Projektcode ergänzt nur die Lernregel, das Feedback und die Anbindung an die Planetendaten. Dadurch bleibt die Anwendung näher an getesteten SDK-Komponenten und die Entwicklungszeit kann stärker in Inhalte, Aufgabenstruktur und Verständlichkeit investiert werden.

### 5.3.6 Iteration von UI, Lernfluss und Feedback

Neben der technischen XR-Grundlage wurde auch der Lernfluss mehrfach überarbeitet. Frühe UI-Varianten arbeiteten teilweise mit zur Laufzeit erzeugten Panels oder Platzhalter-Elementen. Für Desktop-Tests war das schnell, im Quest-Kontext aber ungeeignet, weil World-Space-Canvases für Controller-Ray-Interaktion korrekt mit den Meta Interaction SDK Komponenten vorbereitet werden müssen. Deshalb wurde die UI schrittweise stärker als Editor- und Prefab-System aufgebaut.

Der Test- bzw. Learn-Bereich und die Minigame-Prefabs sind ein Beispiel für diesen Wechsel. Statt Buttons und Panels zur Laufzeit zu erzeugen, enthalten die Minigame-Prefabs eigene World-Space-UIs. Ein zentrales Verwaltungssystem startet nur noch die passenden Prefabs und verwaltet den Fortschritt. Dadurch wird die UI nicht mehr als schnell erzeugter Debug-Ersatz behandelt, sondern als eigener Bestandteil der XR-Erfahrung.

Auch das Hauptmenü wurde iterativ bereinigt. Zeitweise existierten alte und neue UI-Hierarchien parallel, was zu falschen Referenzen und schwer nachvollziehbarem Verhalten führte. Der spätere Umbau auf ein einziges Hauptmenü und ein globales Detailpanel folgte derselben Erkenntnis wie die vorherigen technischen Reduktionen: Ein Prototyp wird nicht stabiler, wenn alte Systeme aus Vorsicht weiter aktiv bleiben. Für die weitere Arbeit ist eine eindeutige UI-Quelle besser als mehrere teilweise verdrahtete Alternativen.

Die Positionierung des Detailpanels zeigt zusätzlich, wie stark XR-UI von kleinen räumlichen Entscheidungen abhängt. Die erste Logik orientierte sich direkt an der Kamerablickrichtung. Dadurch beeinflusste auch die Kopfneigung nach oben oder unten die Panelposition. Später wurde die horizontale Blickrichtung verwendet, sodass das Panel links-vorne relativ zum User erscheint und aufrecht bleibt. Diese Änderung ist klein, aber für Lesbarkeit und Bedienkomfort zentral. Sie zeigt, dass Prototyping in XR nicht nur aus großen Architekturentscheidungen besteht, sondern auch aus vielen räumlichen Korrekturen, die den Unterschied zwischen „funktioniert technisch“ und „fühlt sich benutzbar an“ ausmachen.

### 5.3.7 Zwischenfazit des iterativen Prozesses

Der bisherige Entwicklungsverlauf zeigt, dass das Projekt nicht durch eine einzige große Implementierungsphase entstanden ist, sondern durch wiederholtes Prüfen, Vereinfachen und Neuordnen. Mehrere frühe Ansätze wurden nicht verworfen, weil sie grundsätzlich uninteressant waren, sondern weil sie für den konkreten Kontext der Masterarbeit zu instabil, zu aufwendig oder zu schwer wartbar wurden. Dazu gehören etwa der Raumstations-Szenenwechsel, MRUK-basierte Placement-Ansätze für einfache Boden- und Tischplatzierung, runtime-erzeugte XR-UI oder eigene Interaktionslogiken für Aufgaben, die das Meta SDK bereits zuverlässig abdeckt.

Aus diesen Iterationen entstanden mehrere Gestaltungsprinzipien, die den weiteren Prototyp tragen: datengetriebene Planeteninformationen, ein möglichst schlanker App-State, SDK-nahe Interaktionen, editorbasierte World-Space-UI und eine bewusste Trennung zwischen fachlicher Lernlogik und technischer XR-Grundfunktion. Der aktuelle Stand des Prototyps wird in einem späteren Kapitel detailliert beschrieben. Für dieses Kapitel ist entscheidend, dass diese Architektur nicht von Anfang an feststand, sondern aus konkreten Problemen, Tests und Umbauten hervorgegangen ist.

## 5.4 Aktueller Stand des Prototyps

Der aktuelle Prototyp von „Sonnensystem XR“ ist als zusammenhängende XR-Anwendung für die Meta Quest 3 aufgebaut. Im Unterschied zu früheren Entwicklungsständen werden die zentralen UI-Panels nicht mehr zur Laufzeit erzeugt, sondern sind als handgestaltete World-Space-UI in Unity angelegt und mit der jeweiligen Anwendungslogik verbunden. Dadurch wirken Menü, Detailansichten, Sonnensystemsteuerung und Minigames nicht wie voneinander getrennte technische Experimente, sondern wie Teile eines gemeinsamen Designsystems.

Diese Entscheidung betrifft nicht nur die visuelle Qualität, sondern auch die technische Zuverlässigkeit. World-Space-UI in XR benötigt eine saubere Vorbereitung der Canvas-, Ray- und Interaktionskomponenten. Wenn Panels zur Laufzeit erzeugt werden, entstehen schnell Abweichungen bei Layout, Skalierung, Controller-Ray-Interaktion und Lesbarkeit. Die aktuelle Umsetzung vermeidet diese Brüche, indem die Panels im Editor gestaltet, geprüft und anschließend nur noch datenbasiert befüllt oder ein- und ausgeblendet werden. Inhalte können dadurch dynamisch wechseln, während das visuelle Grundsystem stabil bleibt.

Der App-Flow gliedert sich in zwei zentrale Bereiche: den Home-Bereich mit Planet- und Solar-System-Auswahl sowie den Learn-Bereich mit interaktiven Wissensaufgaben. Die Anwendung verbindet damit exploratives Lernen mit aktiver Überprüfung. Nutzerinnen und Nutzer können zunächst Planeten oder das gesamte Sonnensystem betrachten und steuern; anschließend können sie ihr Wissen in Minispielen anwenden.

### 5.4.1 Home-Bereich

Der Home-Bereich bildet den Einstieg in die Experience. Er dient nicht als klassische Start- oder Marketingseite, sondern als funktionales Hauptmenü im Raum. Von hier aus wählen die Nutzerinnen und Nutzer, ob sie einen einzelnen Planeten oder das Sonnensystem als Ganzes erleben möchten. Zusätzlich kann hier zwischen AR- und VR-Hintergrund gewechselt werden, sodass das System entweder im realen Raum über Passthrough oder vor einem reduzierten dunklen Hintergrund betrachtet werden kann.

Das Home-Menü basiert auf einem einheitlichen, manuell gestalteten Menü-Root. Alle sichtbaren UI-Elemente folgen demselben Designsystem: Typografie, Abstände, Button-Stile, Karten und Tab-Zustände sind aufeinander abgestimmt. Die UI wird nicht durch Code nachgebaut, sondern im Unity Editor gepflegt. Die Anwendungslogik übernimmt nur die Funktionen dahinter, zum Beispiel das Binden der Planetendaten, das Umschalten von Bereichen oder das Starten der jeweiligen Experience.

### 5.4.2 Home: Planet-Bereich

Im Planet-Bereich können die acht Hauptplaneten des Sonnensystems ausgewählt werden: Merkur, Venus, Erde, Mars, Jupiter, Saturn, Uranus und Neptun. Jeder Planet ist über ein zentrales `PlanetData`-Asset beschrieben. Dieses enthält die planetenspezifischen Informationen, etwa Name, astronomische Kennwerte, Beschreibungstexte, Kurztexte für Detailkarten und Referenzen auf die visuellen Prefabs.

Nach der Auswahl eines Planeten startet die Platzierung im Raum. Der Nutzer richtet den Controller-Ray auf eine geeignete horizontale Fläche, etwa Boden oder Tisch, und bestätigt die Platzierung. Die Anwendung erzeugt dabei nicht für jeden Planeten ein eigenes Interaktionssystem, sondern nutzt einen generischen Interactable-Wrapper. Das sichtbare Planetmodell wird aus dem jeweiligen `PlanetData` geladen und in den gemeinsamen Wrapper eingesetzt. Dadurch bleibt die Interaktion für alle Planeten identisch, während Aussehen und Daten je nach Auswahl wechseln.

Nach der Platzierung erscheint das globale `PlanetDetailRoot`. Dieses Panel ist ebenfalls handgestaltet und wird zur Laufzeit nur mit den Daten des aktuell ausgewählten Planeten befüllt. Es zeigt zentrale Informationen wie Beschreibung, Durchmesser, Umlaufdistanz, Umlaufgeschwindigkeit, Achsneigung, Exzentrizität und relative Planetengröße. Wenn ein anderer Planet ausgewählt oder per Ray angeklickt wird, wird dasselbe Panel aktualisiert, statt ein neues planetenspezifisches Panel zu erzeugen.

Aus dem Planet-Detail heraus kann außerdem in eine immersive Ansicht gewechselt werden. Dabei wird der ausgewählte Planet nicht mehr nur als nah platzierbares Objekt gezeigt, sondern in einer separaten VR-Natur- bzw. Nachtszene an einem entfernten Spawnpunkt dargestellt, sodass er wie ein Himmelskörper am Mond-Ort erscheint. Das bestehende Detailpanel bleibt dabei Teil des Flows und dient gleichzeitig als Rückweg aus der immersiven Ansicht. Auch dieser Modus folgt somit dem Prinzip, vorhandene UI-Strukturen weiterzuverwenden, statt für jede Ansicht ein eigenes Bedienkonzept einzuführen.

Der Planet-Bereich unterstützt damit eine einfache Form explorativen Lernens. Die Nutzerinnen und Nutzer wählen ein Objekt aus, platzieren es im eigenen Raum und erhalten direkt zugehörige Informationen. Die räumliche Platzierung macht den Planeten nicht nur zu einem Bild auf einem Bildschirm, sondern zu einem Objekt im eigenen Umfeld. Gleichzeitig bleibt die Darstellung kontrolliert und verständlich, weil nicht mehrere UI-Systeme oder überladene Informationsfenster parallel erscheinen.

### 5.4.3 Home: Solar-System-Bereich

Der Solar-System-Bereich zeigt nicht einen einzelnen Planeten, sondern das gesamte Sonnensystem als räumliches Modell. Nach der Auswahl wird das Sonnensystem im Raum platziert. Die Sonne bildet den Mittelpunkt, die Planeten bewegen sich auf berechneten Bahnen um sie herum. Für die Umlaufbahnen sind visuelle Orbit-Linien vorhanden, sodass die Bewegungen nicht nur als einzelne wandernde Kugeln wahrgenommen werden, sondern als nachvollziehbares System aus Bahnen, Abständen und Umlaufgeschwindigkeiten.

Das Sonnensystem wird über ein zentrales Simulationssystem gesteuert. Dieses erzeugt die Planeten auf Grundlage der vorhandenen `PlanetData`-Assets und berechnet ihre Positionen über skalierbare Parameter. Dadurch können reale astronomische Größenverhältnisse didaktisch angepasst werden. Da echte Distanzen und Durchmesser im selben Maßstab für eine Raum-XR-Anwendung kaum sinnvoll darstellbar wären, arbeitet der Prototyp mit steuerbaren Skalierungsfaktoren. Diese machen sichtbar, was im echten Sonnensystem sonst entweder zu klein, zu groß oder zu weit entfernt wäre.

Zum Solar-System-Bereich gehört das `SolarSystemRoot`-Panel. Es ist korrekt mit der jeweils platzierten Sonnensystem-Instanz verbunden und steuert nicht irgendeinen statischen Inspector-Verweis, sondern genau das aktuell erzeugte Modell im Raum. Die Slider funktionieren als didaktische Steuerung: Sie verändern unter anderem Planetengröße, Umlaufgeschwindigkeit, Achsneigung, Orbit-Abstände und Exzentrizität. Zusätzlich gibt es einen Spacing-Modus, der die Darstellung der Abstände beeinflusst und den Unterschied zwischen kompakter didaktischer Ansicht und stärker realitätsnaher Entfernung erfahrbar macht.

Die Orbit-Visualizer unterstützen diese Steuerung visuell. Wenn Nutzerinnen und Nutzer Parameter verändern, reagieren nicht nur die Planetenkörper, sondern auch die Darstellung der Bahnen. Dadurch wird das Sonnensystem als dynamisches Modell erfahrbar. Es geht nicht darum, eine physikalisch vollständige Simulation zu erzeugen, sondern um eine verständliche Lernvisualisierung: Die Nutzerinnen und Nutzer können Zusammenhänge zwischen Umlaufbahn, Abstand, Geschwindigkeit und Maßstab direkt ausprobieren.

### 5.4.4 Learn-Bereich

Der Learn-Bereich ergänzt die freie Exploration um gamifizierte Aufgaben. Während der Home-Bereich vor allem der Betrachtung und Orientierung dient, fordert der Learn-Bereich eine aktive Anwendung des Wissens. Die Nutzerinnen und Nutzer sollen Planeten nicht nur ansehen, sondern ihre Eigenschaften und Reihenfolgen handelnd einsetzen.

Auch hier sind die Panels und Minigame-Oberflächen nicht zur Laufzeit generiert, sondern als gestaltete Prefabs in Unity vorbereitet. Die Minigames werden zentral gestartet und im Raum vor dem User platziert. Die Interaktion basiert auf Komponenten aus dem Meta Interaction SDK, insbesondere auf Snap- und Grab-Mechaniken. Der eigene Projektcode ergänzt die fachliche Prüfung, das Feedback und die Fortschrittslogik.

Der Learn-Bereich enthält aktuell zwei umgesetzte Aufgaben: Reihenfolge und Größe. Ein drittes Gravity-Minispiel ist als zukünftige Erweiterung vorgesehen, aber noch nicht implementiert. Es bleibt daher im aktuellen Prototyp als Platzhalter oder Ausblick erhalten und wird nicht als fertige Funktion beschrieben.

### 5.4.5 Learn: Reihenfolge-Minispiel

Das Reihenfolge-Minispiel prüft, ob die Nutzerinnen und Nutzer die Reihenfolge der Planeten im Sonnensystem verstanden haben. Die Planeten liegen als greifbare Objekte in einer Snap-Liste bereit und können mit den XR-Controllern aufgenommen und auf Orbit-Slots gelegt werden. Jeder Slot steht für eine Position im Sonnensystem, ausgehend von der Nähe zur Sonne.

Die Aufgabe ist bewusst körperlich-räumlich umgesetzt. Anstatt eine Liste anzuklicken oder Multiple-Choice-Antworten auszuwählen, müssen die Planeten aktiv an die passenden Orte bewegt werden. Dadurch verbindet das Minispiel Wissensabfrage mit räumlicher Handlung. Die Reihenfolge wird nicht nur abstrakt abgefragt, sondern im Modell des Sonnensystems nachvollzogen.

Technisch basiert das Minispiel auf dem Snap-Aufbau des Meta Interaction SDK. Die Planeten nutzen vorhandene Grab- und Snap-Komponenten; der eigene Code prüft nur, ob der Planet mit dem richtigen Orbit-Slot verbunden ist, gibt visuelles Feedback und bewertet am Ende, ob alle aktiven Slots korrekt belegt sind.

Für die Lernwirkung ist dieses Feedback entscheidend. Die Nutzerinnen und Nutzer sehen unmittelbar, ob eine Platzierung richtig oder falsch ist, und können ihre Entscheidung korrigieren. Damit entsteht eine niedrigschwellige Übungsschleife: ausprobieren, Rückmeldung erhalten, neu ordnen. Diese Struktur unterstützt den gamifizierten Charakter der Anwendung, ohne die Aufgabe in ein reines Punktesystem zu verwandeln.

### 5.4.6 Learn: Größen-Minispiel

Das Größen-Minispiel konzentriert sich auf die relativen Größenverhältnisse der Planeten. Auch hier greifen die Nutzerinnen und Nutzer Planeten aus einer Liste und ordnen sie passenden Slots zu. Im Unterschied zum Reihenfolge-Minispiel geht es jedoch nicht um die Position im Sonnensystem, sondern um den Vergleich der Planetendurchmesser.

Die besondere didaktische Herausforderung liegt darin, dass reale Planetengrößen in einer XR-Anwendung nur schwer im echten Maßstab darstellbar sind. Jupiter ist im Vergleich zur Erde sehr groß, während Merkur deutlich kleiner ist. Würde man alle Objekte vollständig realistisch skalieren, könnten einzelne Planeten zu klein zum Greifen oder andere zu groß für den Raum werden. Das Minispiel nutzt deshalb eine relative Darstellung: Die Größen werden auf einen sinnvollen XR-Maßstab übertragen, sodass die Unterschiede sichtbar und zugleich handhabbar bleiben.

Die Auswertung übernimmt eine zentrale Prüflogik. Sie sammelt die Planeten und Slots aus dem Prefab, prüft die aktuelle Belegung und nutzt den Durchmesser aus den Planetendaten, um die erwartete Reihenfolge zu bestimmen. Korrekt platzierte Planeten wachsen sichtbar auf ihre relative Zielgröße an. Falsch platzierte Planeten erhalten Feedback und kehren nach kurzer Zeit in die Liste zurück. Dadurch wird die Aufgabe nicht nur als richtig oder falsch bewertet, sondern die Größenrelation wird unmittelbar visualisiert.

Auch dieses Minispiel verwendet die Snap-Mechaniken des Meta SDK. Die Projektlogik bleibt auf die Lernregel beschränkt: Welche Reihenfolge ergibt sich aus den Durchmessern, wie wird eine Platzierung bewertet und welches Feedback wird angezeigt? Damit bleibt die technische Interaktion robust, während die fachliche Aussage im Vordergrund steht.

### 5.4.7 Zusammenfassung des Ist-Zustands

Der aktuelle Prototyp verbindet drei zentrale Ebenen: eine räumliche Darstellung des Sonnensystems, ein einheitliches handgestaltetes UI-System und gamifizierte Lernaufgaben. Der Home-Bereich ermöglicht die Auswahl und Betrachtung einzelner Planeten sowie des gesamten Sonnensystems. Der Planet-Bereich arbeitet mit einem globalen Detailpanel und datenbasierten Planet-Assets. Der Solar-System-Bereich stellt Planetenbahnen, Orbit-Visualisierung und steuerbare Skalierungsparameter bereit. Der Learn-Bereich ergänzt diese Exploration durch die Minigames Reihenfolge und Größe.

Damit erfüllt der Prototyp die grundlegende Zielrichtung der Arbeit: astronomische Inhalte werden nicht nur präsentiert, sondern in einer XR-Umgebung räumlich erfahrbar und durch Interaktion überprüfbar gemacht. Gleichzeitig bleibt die Umsetzung auf Erweiterbarkeit ausgelegt. Neue Planeteninformationen, weitere Aufgaben oder zusätzliche Lerntexte können auf Grundlage der bestehenden Datenstruktur ergänzt werden, ohne das gesamte System neu aufzubauen.

## Offene Arbeitsnotizen

- TODO: Kapitelstruktur an die finale Gliederung der Word-Datei anpassen.
- TODO: Relevante Punkte aus `DOKU.md` und `ARCHIV.md` für den Prozessbericht übernehmen.
- TODO: Stellen markieren, an denen auf den bereits geschriebenen theoretischen Hintergrund verwiesen wird.
