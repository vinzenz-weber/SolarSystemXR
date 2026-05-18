# Werkstück – Projekt- und Prozesskapitel Sonnensystem XR

Diese Datei ist die Arbeitsfassung für die Kapitel, die den Entwicklungsprozess und das konkrete Projekt „Sonnensystem XR“ beschreiben. Der theoretische Hintergrund ist bereits separat in Word geschrieben und wird hier nicht erneut gesammelt.

## Arbeitstitel

TODO

## Leitidee

Die Masterarbeit untersucht eine XR-Anwendung für die Meta Quest 3, mit der Nutzer:innen das Sonnensystem erkunden und dabei astronomisches Wissen auf interaktive Weise vermittelt bekommen.

## 1 Konzeption und Projektentwicklung

Wie in den vorangegangenen Kapiteln bereits dargestellt, ist für den nachhaltigen Lernerfolg in digitalen Anwendungen entscheidend, dass Wissen nicht nur passiv präsentiert, sondern durch aktive Anwendung und Überprüfung gefestigt wird. Gleichzeitig wurden die spezifischen Herausforderungen von XR-Anwendungen erörtert, insbesondere die Gefahr von Motion Sickness, die durch technische Unzulänglichkeiten wie eine niedrige oder instabile Bildwiederholrate erheblich verstärkt werden kann. Um die Motivation der Nutzenden langfristig zu fördern, wurden zudem Ansätze der Gamification als wesentlicher Bestandteil des Konzepts identifiziert.

Aus diesen grundlegenden Anforderungen leiten sich die zwei zentralen Leitlinien für die Konzeption und technische Entwicklung des „Sonnensystem XR“-Prototyps ab:

1. **Performance und Stabilität als oberste Priorität:** Eine durchgehend hohe und flüssige Bildrate ist nicht nur ein technisches Qualitätsmerkmal, sondern eine grundlegende Voraussetzung, um ein komfortables Nutzererlebnis ohne Motion Sickness zu gewährleisten. Wie die Projektdokumentation zeigt, führte dieses Prinzip zu weitreichenden Architekturentscheidungen, etwa dem Wechsel von instabilen, additiven Szenen-Ladevorgängen zu einer robusteren Single-Scene-Architektur oder der bewussten Entscheidung für die einfachere Meta Depth API anstelle des komplexeren MRUK-Stacks, um die Stabilität zu erhöhen.

2. **Aktive Wissensvermittlung durch Interaktion:** Anstatt die Nutzenden mit reinen Informationen zu konfrontieren, sollen sie durch interaktive Elemente dazu angeregt werden, das Gelernte anzuwenden. Dieses Prinzip manifestiert sich im Prototyp durch die Konzeption und Implementierung von Lernspielen wie dem „Reihenfolge“- und „Größen“-Quiz. Anstatt eigene Interaktionslogiken von Grund auf neu zu entwickeln, wurde hierbei konsequent auf die bewährten Interaktions-Primitives des Meta XR SDK, wie die SnapExamples, zurückgegriffen, um den Fokus auf die Spielregeln und das didaktische Feedback legen zu können.

Dieses Kapitel dokumentiert den iterativen Entwicklungsprozess, der von diesen Leitlinien geprägt war – von der initialen Plattformwahl über entscheidende architektonische Umbauten bis hin zur konkreten Implementierung der Kern-Features, die diesen Prinzipien Rechnung tragen.

## 1.1 Zielgruppe, Nutzungsszenario und Methodik

TODO

## 1.2 Technisches Framework

Für die Nachvollziehbarkeit des Prototyps sind folgende technische Rahmendaten relevant:

- **Engine:** Unity `6000.3.10f1`
- **Programmiersprache:** C#
- **Zielhardware:** Meta Quest 3
- **Zielplattform:** Android / Standalone-XR
- **XR-Runtime:** OpenXR
- **XR-SDK:** Meta XR SDK `v85`, Meta XR SDK `v201`
- **Render Pipeline:** Universal Render Pipeline (URP)
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
- **Zentrale UI-Strukturen:** `MenuRoot`, `PlanetDetailRoot`, `SolarSystemRoot`, Lernspiel-World-Space-UIs
- **App-Architektur:** Single-Scene-Architektur in `MainScene`, keine additiven Szenenwechsel im aktuellen Hauptflow
- **Zustandsverwaltung:** zentrale State Machine mit `MAIN_MENU`, `PLACEMENT`, `WORLD`, `IMMERSIVE` und `TEST_*`
- **Datenstruktur:** `PlanetData` als ScriptableObject für planetenspezifische Daten, Texte, Kennwerte und Prefab-Referenzen
- **Sonnensystem-Simulation:** `SolarSystemManager` für Planetenerzeugung, Orbit-Berechnung, Skalierung und Simulationsparameter
- **Planeten-Interaktion:** generischer Interactable-Wrapper, visuelles Planet-Prefab wird datenbasiert aus `PlanetData` eingesetzt
- **Info- und Detailanzeige:** globales Planet-InfoPanel mit datenbasiertem Binding über `PlanetData`
- **Startsequenz und Tutorial:** optionale Logo-/Splash-Sequenz mit anschließendem kurzem Tutorial aus Step-Prefabs und erklärenden Videoloops
- **Lernspiel-Interaktion:** Grab- und Snap-Mechaniken aus dem Meta Interaction SDK, insbesondere nach Vorbild der `SnapExamples`
- **Lernspiel-Logik:** eigene Prüfsysteme für Reihenfolge und Größe, SDK übernimmt die technische Interaktion
- **Mixed-Reality-Funktionen:** Passthrough/VR-Umschaltung über `PassthroughDissolver`
- **User-Testing-Vorbereitungsmodus:** versteckter Shortcut über beide Controller-Thumbsticks, der vor Testbeginn eine Passthrough-Ansicht für Headset-Anpassung und Orientierung aktiviert
- **Immersive-Ansicht:** aktivierbarer Immersive-Root innerhalb der MainScene, Planetendarstellung an einem entfernten Spawnpunkt
- **Testumgebung Windows:** Unity Playmode für UI- und Flow-Tests, Quest Link bzw. Headset für echte XR- und Depth-API-Tests
- **Build-/Verifikation:** C#-Kompilierung über `dotnet build Assembly-CSharp.csproj --no-restore`, finale Funktionsprüfung im Unity Editor und auf der Quest 3
- **Projektorganisation:** zentrale technische Dokumentation in `.claude/DOKU.md`, Iterationshistorie in `.claude/ARCHIV.md`, Projekt-/Prozesskapitel in `.claude/WERKSTÜCK.md`

## 1.3 User Experience und Interaction Design

Die User Experience von „Sonnensystem XR“ wird wesentlich durch die Besonderheiten einer XR-Anwendung bestimmt. Die Anwendung wird nicht auf einem flachen Bildschirm bedient, sondern im Raum vor dem User platziert. Dadurch verschieben sich zentrale Gestaltungsfragen: Menüs müssen nicht nur lesbar und funktional sein, sondern auch räumlich sinnvoll positioniert werden; Interaktionen müssen mit Controllern, Raycasts, Grab- und Snap-Mechaniken funktionieren; und die Nutzer:innen müssen jederzeit verstehen, in welchem Zustand der Anwendung sie sich befinden.

Für den Prototyp war deshalb nicht nur entscheidend, welche Inhalte gezeigt werden, sondern wie diese Inhalte in eine verständliche räumliche Handlung übersetzt werden. Die Anwendung verbindet drei Formen der Nutzung: Auswahl und Orientierung im Home-Bereich, explorative Betrachtung von Planeten und Sonnensystem sowie aktive Wissensanwendung in den Lernspielen. Diese Bereiche dürfen sich nicht wie voneinander getrennte technische Einzelprototypen anfühlen, sondern müssen in einem gemeinsamen Bedienfluss zusammenhängen.

Wie in Kapitel 3.2.2 beschrieben, ist es wichtig, die kognitive Belastung der Nutzer:innen möglichst gering zu halten. Nutzer:innen sollen nicht lernen müssen, wie ein Unity-Prototyp funktioniert, sondern sich auf das Sonnensystem und die Aufgaben konzentrieren können. Deshalb werden wiederkehrende Bedienmuster verwendet: Auswahl über Ray-Interaktion, Platzierung über Controller-Ray auf horizontalen Flächen, Rückkehr über das Hauptmenü bzw. definierte Exit-Buttons und direkte Rückmeldung in den Lernspielen. Die State Machine des Projekts unterstützt diese UX-Struktur technisch, indem sie klar zwischen Hauptmenü, Placement, World-Ansicht, Immersive Mode und Lernspiel-Zuständen trennt.

Ergänzend zur eigentlichen Anwendung wurde eine kurze Tutorial-Sequenz in den Startfluss integriert. Sie besteht aus einzelnen Step-Prefabs, die vor dem User im Raum platziert werden und kurze erklärende Videoloops enthalten können. Die Schritte sollen zentrale Bedienhandlungen wie Button-/Menüinteraktion oder Greifen nicht abstrakt erklären, sondern unmittelbar im Medium zeigen und durch eine einfache Nutzeraktion abschließen lassen.

Aus UX-Sicht dient dieses Tutorial nicht als zusätzlicher Lerninhalt über das Sonnensystem, sondern als Onboarding für die XR-Bedienung. Gerade bei Personen mit wenig VR-Erfahrung kann bereits der Umgang mit Controller-Ray, Menügesten oder Greifinteraktion Aufmerksamkeit binden. Wenn diese Basisinteraktionen kurz vorgeführt werden, kann die eigentliche Lernphase stärker auf Planetendaten, Maßstab, räumliche Darstellung und Lernspiele fokussiert bleiben.

Für die Masterarbeit ist diese Ebene besonders relevant, weil sie die theoretischen Schwerpunkte miteinander verbindet. Wissensvermittlung in XR entsteht hier nicht allein durch Informationstexte, sondern durch räumliche Darstellung und Handlung. Gamification entsteht nicht nur durch Punkte oder Fortschritt, sondern durch Aufgaben, Feedback und Korrekturschleifen. Wissenschaftliche Korrektheit bleibt dabei an die Datenstruktur gekoppelt: Planetendaten und Größenverhältnisse werden nicht frei in UI-Elemente geschrieben, sondern über `PlanetData` und zentrale Simulations- bzw. Prüfsysteme verarbeitet.

TODO: Hier später kurz auf den finalen Figma-/UI-Designprozess eingehen: Ausgangspunkt der UI-Entwürfe, wichtigste Layoutentscheidungen, visuelle Sprache, Iterationen zwischen Figma und Unity sowie Begründung, warum die finalen Panels als manuell gestaltete World-Space-UI umgesetzt wurden.

### 1.3.1 Spatial UI

Die Benutzeroberfläche des Prototyps ist als Spatial UI angelegt. Das bedeutet, dass Menüs, Detailansichten und Steuerpanels nicht als klassische Overlay-UI über dem Bild liegen, sondern als World-Space-Elemente in der XR-Szene erscheinen. Diese Entscheidung passt zum Medium, erzeugt aber eigene Anforderungen: Die Panels müssen in einer angenehmen Entfernung stehen, groß genug lesbar sein, mit Controller-Rays getroffen werden können und dürfen den Blick auf die räumlichen Inhalte nicht unnötig verdecken.

Im aktuellen Prototyp werden die zentralen UI-Panels nicht mehr zur Laufzeit per Code erzeugt. Stattdessen sind `MenuRoot`, `PlanetDetailRoot`, `SolarSystemRoot` bzw. die Lernspiel-UIs als gestaltete UI-Strukturen im Unity Editor vorbereitet und mit der Logik verbunden. Diese Entscheidung entstand aus der praktischen Erfahrung, dass World-Space-UI in XR sehr empfindlich auf Canvas-Setup, Skalierung, Raycast-Konfiguration und Hierarchie reagiert. Eine zur Laufzeit erzeugte UI kann im Editor schnell funktionieren, im Headset aber durch falsche Größe, schlechte Lesbarkeit oder fehlende Interaction-SDK-Komponenten unzuverlässig werden.

Das Hauptmenü bildet den räumlichen Einstiegspunkt. Es ist kein dekorativer Startscreen, sondern ein funktionales Auswahlpanel. Nutzer:innen wählen dort zwischen Planet-Ansicht, Sonnensystem-Ansicht und Learn- bzw. Testbereich. Zusätzlich ermöglicht ein Toggle den Wechsel zwischen Passthrough-AR und VR-Hintergrund. Dadurch wird die räumliche Darstellung flexibel: Im AR-Modus erscheint das Sonnensystem im realen Raum, während der VR-Modus eine reduzierte dunkle Umgebung bietet, in der die Himmelskörper visuell stärker im Vordergrund stehen.

Das globale `PlanetDetailRoot` ist ein zweites zentrales Spatial-UI-Element. Es wird nach der Auswahl oder Platzierung eines Planeten mit den Daten des aktuellen `PlanetData`-Assets befüllt. Dadurch bleibt das Bedienkonzept konsistent: Es gibt nicht für jeden Planeten ein eigenes Detailfenster, sondern ein gemeinsames Panel, dessen Inhalt wechselt. Für die User Experience ist das wichtig, weil die Nutzer:innen nicht für jeden Planeten eine neue UI-Logik interpretieren müssen. Für die technische Struktur ist es gleichzeitig wartbarer, weil Layout und Datenbindung getrennt bleiben.

Die Positionierung des Detailpanels wurde bewusst räumlich gedacht. Das Panel erscheint links-vorne relativ zur horizontalen Blickrichtung des Users. Dadurch hängt seine Position nicht direkt von der Kopfneigung ab. Wenn der User nach oben oder unten schaut, soll das Panel nicht unkontrolliert in diese Richtung wandern, sondern als stabiler Bedienanker im Raum bleiben. Diese Entscheidung zeigt, dass Spatial UI nicht nur aus dem Platzieren eines Canvas im Raum besteht, sondern aus einer genauen Abstimmung zwischen Blickrichtung, Lesbarkeit und Interaktionskomfort.

Auch das Sonnensystem-Panel folgt dieser Logik. Es steuert die aktuell platzierte Sonnensystem-Instanz und wird nicht über eine statische Inspector-Referenz an irgendein Modell gebunden. Die UI erhält den passenden `SolarSystemManager` nach dem Placement. Dadurch entspricht die Bedienung der Erwartung: Wenn ein Sonnensystem im Raum platziert wurde, verändern die Slider genau dieses sichtbare Modell. Die Steuerung wird damit zu einem didaktischen Werkzeug, weil Nutzer:innen Größen, Abstände, Geschwindigkeit, Achsneigung und Exzentrizität direkt am räumlichen Modell nachvollziehen können.

Für die Lernspiele wird Spatial UI ebenfalls als vorbereiteter Bestandteil der jeweiligen Prefabs genutzt. Die Aufgabe, die Buttons und das Feedback liegen im Raum vor dem User und werden zusammen mit dem Lernspiel platziert. Das ist für die Konsistenz wichtig, weil die Tests nicht als separate 2D-Menüs erscheinen, sondern Teil der XR-Erfahrung bleiben. Zugleich vermeidet diese Struktur, dass jedes Lernspiel seine UI zur Laufzeit selbst erzeugt und dabei eigene Layout- oder Interaktionsprobleme einführt.

TODO: Figma-Prozess ergänzen: Welche UI-Flächen wurden zuerst in Figma gestaltet? Welche Elemente wurden aus Figma nach Unity übertragen? Welche Anpassungen waren in Unity nötig, weil eine 2D-Figma-Komposition in World-Space-UI anders wirkt? Welche Panels sind final, welche noch Platzhalter?

TODO: Falls vorhanden, Screenshots oder Abbildungsverweise ergänzen: Figma-Entwurf des `MenuRoot`, finale Unity-Umsetzung, `PlanetDetailRoot`, Sonnensystem-Slider-Panel und Lernspiel-UI.

### 1.3.2 Steuerung, Orientierung und Präsenz

Die Steuerung des Prototyps basiert auf klar getrennten Interaktionszuständen. Im Hauptmenü wählen Nutzer:innen eine Experience aus. Im Placement-State wird das gewählte Objekt zunächst als Vorschau entlang des Controller-Rays angezeigt und kann auf einer geeigneten horizontalen Fläche platziert werden. Nach der Platzierung wechselt die Anwendung in die World-Ansicht. Dort stehen die räumlichen Inhalte im Vordergrund: ein einzelner Planet mit Detailpanel oder das Sonnensystem mit Steuerpanel. Für Lernspiele gibt es eigene Test-Zustände, damit Grab-, Snap- und Feedbacklogik nicht mit der normalen Planeten-Auswahl kollidieren.

Diese Trennung ist aus UX-Sicht entscheidend, weil sie Mehrdeutigkeiten reduziert. Derselbe Controller-Trigger darf nicht gleichzeitig ein Objekt platzieren, einen Planeten auswählen und ein Lernspiel-Objekt greifen. Der Prototyp löst das über die State Machine: `PLACEMENT` ist für Platzierung zuständig, `WORLD` für Auswahl und Betrachtung, `IMMERSIVE` für die VR-Naturansicht und `TEST_*` für die Lernspiele. Dadurch wird nicht jede Interaktion global ausgewertet, sondern nur in dem Zustand, in dem sie fachlich sinnvoll ist.

Die Platzierung im Raum unterstützt Orientierung und Präsenz, weil Nutzer:innen den Startpunkt der Erfahrung selbst bestimmen. Ein Planet oder das Sonnensystem erscheint nicht an einer abstrakten Standardposition, sondern wird über den Controller-Ray auf Boden oder Tisch gesetzt. Die Anwendung prüft dabei die Normalenrichtung der getroffenen Fläche, sodass vor allem horizontale Flächen akzeptiert werden. Im Quest-Build nutzt dieser Vorgang die Depth API; im Unity-Editor gibt es einen vereinfachten Fallback, damit UI- und Flow-Tests auch ohne Headset möglich bleiben.

Durch die Platzierung entsteht eine erste Form von Präsenz: Der Planet oder das Sonnensystem wird als Objekt im eigenen Umfeld wahrgenommen. Gerade für eine Bildungsanwendung ist diese räumliche Verankerung wichtig, weil sie das Thema Sonnensystem aus der reinen Bildschirmdarstellung herauslöst. Nutzer:innen betrachten die Inhalte nicht nur, sondern bewegen sich zu ihnen in Beziehung: Sie wählen eine Position, schauen auf das Objekt, bedienen ein Panel daneben und können Parameter oder Aufgaben im Raum manipulieren.

Die Orientierung wird zusätzlich durch wiederkehrende Rückwege stabilisiert. Die Options-Taste öffnet aus den World- und Test-Zuständen wieder das Hauptmenü bzw. beendet aktive Sonderzustände. Der Immersive Mode nutzt das bestehende Detailpanel als Exit-UI. Dadurch müssen Nutzer:innen nicht mehrere unabhängige Navigationsprinzipien lernen. Stattdessen gibt es wiedererkennbare Anker: Hauptmenü, Detailpanel, Steuerpanel und Lernspiel-Buttons.

Für die Interaktion mit Planeten und Lernspielen wird bewusst auf Komponenten des Meta Interaction SDK zurückgegriffen. Ray-Auswahl, Canvas-Interaktion, Grab- und Snap-Mechaniken sind typische XR-Interaktionsmuster, die im SDK bereits als getestete Bausteine vorhanden sind. Der eigene Projektcode übernimmt vor allem die fachliche Logik: Welcher Planet wurde ausgewählt? Welche Daten werden angezeigt? Welcher Slot ist richtig? Welche Größenreihenfolge ergibt sich aus den Durchmessern? Dadurch bleibt die Bedienung näher an etablierten Quest-Interaktionsmustern und die projektspezifische Komplexität wird reduziert.

Die Lernspiele nutzen Steuerung nicht nur als Eingabemethode, sondern als Teil der Lernhandlung. Im Reihenfolge-Minispiel werden Planeten gegriffen und auf Orbit-Slots gelegt. Im Größen-Minispiel werden Planetenkörper in eine Größenordnung gebracht und bei korrekter Platzierung sichtbar skaliert. Die Nutzer:innen beantworten also keine rein abstrakten Fragen, sondern handeln im Raum. Diese körperlich-räumliche Komponente unterstützt den XR-Anspruch des Projekts: Wissen soll nicht nur gelesen, sondern durch Interaktion und unmittelbares Feedback erfahrbar werden.

Der Wechsel zwischen Passthrough-AR und VR-Hintergrund beeinflusst ebenfalls die Präsenz. Passthrough bindet die Anwendung stärker an den realen Raum und eignet sich für das Platzieren von Planeten oder Sonnensystem auf realen Flächen. Der VR-Hintergrund reduziert dagegen visuelle Ablenkung und kann die Himmelskörper stärker inszenieren. Der Immersive Mode geht noch einen Schritt weiter, indem ein ausgewählter Planet in einer VR-Natur- bzw. Nachtszene am entfernten Spawnpunkt dargestellt wird. Damit verschiebt sich die Experience von der objektbezogenen Betrachtung im Raum zu einer stärker atmosphärischen Himmelskörper-Ansicht.

Gleichzeitig bleibt die Präsenz an technische Stabilität gebunden. Ruckeln, schwarze Übergänge, falsch positionierte Panels oder unklare Controllerzustände würden die Aufmerksamkeit sofort von den Lerninhalten abziehen. Deshalb wurden im Prototyp mehrere spektakulärere, aber instabilere Ansätze reduziert oder ersetzt: keine additiven Szenenwechsel für den immersiven Modus, keine zur Laufzeit generierten Kern-Panels, keine eigene Grab-/Snap-Logik für Lernspiele, wenn die Meta-Samples diese Aufgabe bereits abdecken. Die UX-Entscheidung ist hier also auch eine technische Entscheidung: Stabilität und Verständlichkeit haben Vorrang vor unnötiger Komplexität.

TODO: Nach Headset-Tests ergänzen: Wie gut funktionieren Lesbarkeit, Panelabstand, Controller-Ray-Treffer, Placement und Snap-Interaktion tatsächlich auf der Quest 3? Welche Anpassungen wurden nach Tests im Headset vorgenommen?

## 1.4 Prototyping & Iterativer Designprozess

Die Entwicklung des Prototyps verlief nicht als geradlinige Umsetzung eines vorab vollständig festgelegten Systems, sondern als iterativer Design- und Technikprozess. Das war für dieses Projekt besonders relevant, weil mehrere Ebenen gleichzeitig überprüft werden mussten: die grundsätzliche Machbarkeit auf der Meta Quest 3, die Stabilität der XR-Interaktion, die visuelle Wirkung der Planeten, die Verständlichkeit der Lerninhalte und die Frage, ob gamifizierte Elemente tatsächlich sinnvoll in den Ablauf eingebunden werden können.

Der Prototyp wurde daher schrittweise aufgebaut, getestet, verworfen und neu strukturiert. Viele Entscheidungen entstanden nicht aus rein theoretischen Überlegungen, sondern aus konkreten Entwicklungserfahrungen: veraltete SDK-Komponenten, instabile Szenenwechsel, schwer wartbare UI-Hierarchien oder Interaktionslogiken, die im Editor funktionierten, aber für den Quest-XR-Kontext zu fehleranfällig gewesen wären. Das Archiv dokumentiert diese Änderungen als Abfolge von Umbauten und Lernmomenten; die Git-Historie zeigt ergänzend, dass sich der Fokus von frühen Visualisierungs- und Shader-Experimenten über XR-Grundlagen und Placement hin zu einem stabileren, datengetriebenen Lern- und Testsystem verschoben hat.

Für die Masterarbeit ist dieser iterative Verlauf ein wichtiger Bestandteil des Werkstücks. Die Anwendung soll nicht nur technisch funktionieren, sondern zugleich die in der Theorie herausgearbeiteten Anforderungen erfüllen: Sie muss für Lernende verständlich sein, Motivation durch Interaktion und Aufgaben erzeugen und auf mobiler XR-Hardware stabil genug laufen, um nicht selbst zum Hindernis für die Wissensvermittlung zu werden. Der Prototyping-Prozess kann daher als fortlaufende Annäherung an drei Leitfragen verstanden werden:

1. Welche Form der Darstellung ist visuell überzeugend, aber performant genug für Standalone-XR?
2. Welche Interaktionen unterstützen Lernen und Exploration, ohne die Nutzer:innen technisch zu überfordern?
3. Welche Architektur erlaubt es, astronomische Daten und Lerninhalte zu erweitern, ohne bei jeder Änderung neue Speziallösungen bauen zu müssen?

### 1.4.1 Erster Prototyp

Der erste Prototyp entstand zunächst nicht in Unity, sondern in Unreal Engine. Dieser Einstieg war vor allem visuell motiviert. Schon frühe Tests konnten mit vergleichsweise wenig Aufwand eine überzeugende Raumwirkung und eine hochwertige Darstellung erzeugen. Für ein Projekt über das Sonnensystem war dieser erste Eindruck wichtig, weil die Faszination des Themas stark über Maßstab, Licht, Dunkelheit und die visuelle Präsenz der Himmelskörper vermittelt wird.

Gleichzeitig zeigte sich in dieser frühen Phase, dass eine gute visuelle Wirkung allein nicht ausreicht. Die technische Integration der Meta-Quest-XR-Funktionen erwies sich als schwer planbar. Die verfügbaren Integrationswege wirkten teilweise veraltet oder nicht ausreichend dokumentiert, und bei der Umsetzung traten wiederholt Probleme auf, die weniger mit der eigentlichen Lernidee als mit der XR-Grundinfrastruktur zusammenhingen. Für einen Masterarbeits-Prototyp, der in begrenzter Zeit entstehen muss, wurde dadurch das Risiko zu groß, zu viel Entwicklungszeit in die Stabilisierung der Plattform statt in die eigentliche Experience zu investieren.

Aus diesem Grund wurde die Entwicklung auf Unity umgestellt. Diese Entscheidung war nicht nur eine technische Präferenz, sondern ein erster größerer Iterationsschritt: Der Fokus verschob sich von maximaler visueller Qualität zu einer Umgebung, in der Meta-XR-Integration, C#-basierte Logik und datengetriebene Systeme besser zusammenpassen. Gerade für die Berechnung von Planetenbahnen, Skalierungsfaktoren und späteren Quizlogiken war C# nachvollziehbarer als ein stark visuell verdrahteter Blueprint-Ansatz. Die Entscheidung für Unity markierte damit den Übergang vom reinen Darstellungsprototyp zu einem System, das langfristig erweiterbar und für den Entwickler kontrollierbarer sein sollte.

Die frühen Unity-Versionen zeigen diesen Suchprozess deutlich. Zunächst wurden Projektstruktur, Planetenobjekte, Shader und grundlegende Sonnensystemlogik aufgebaut. Inhaltlich ging es in dieser Phase darum, überhaupt ein glaubwürdiges und bedienbares Grundmodell zu erhalten: Planeten mussten sichtbar sein, Materialien mussten atmosphärisch wirken, Umlaufbahnen mussten nachvollziehbar berechnet werden und eine erste UI musste den Einstieg in die Experience ermöglichen. Die Git-Historie wurde hier vor allem als Rekonstruktionshilfe genutzt; im Fließtext steht jedoch die Entwicklungslinie im Vordergrund, nicht die Benennung einzelner Commits.

Bereits hier wurde ein zentrales Muster sichtbar, das den weiteren Prozess prägen sollte: Funktionen wurden zunächst schnell gebaut, anschließend aber anhand der XR-Anforderungen wieder vereinfacht oder neu zugeschnitten. Der frühe Prototyp war dadurch weniger ein abgeschlossenes Produkt als ein Testfeld, auf dem grundlegende Annahmen überprüft wurden. Besonders wichtig war die Erkenntnis, dass die Anwendung nicht nur „schön aussehen“ darf, sondern robust bedienbar sein muss. In XR führt jede instabile Interaktion, jedes schwer lesbare Panel und jeder ruckelige Übergang unmittelbar zu einem schlechteren Nutzungserlebnis. Für eine Bildungsanwendung ist das besonders problematisch, weil technische Reibung die Aufmerksamkeit von den Lerninhalten abzieht.

### 1.4.2 Vom Funktionsprototyp zum XR-Prototyp

Nach dem Wechsel zu Unity verlagerte sich der Schwerpunkt auf die Frage, wie aus einzelnen Funktionen eine zusammenhängende XR-Erfahrung werden kann. Früh entstanden erste Zustandsmodelle, die den Ablauf der Anwendung steuern sollten. Das Archiv dokumentiert mehrere Iterationen: Zunächst war ein sehr einfacher Ablauf aus Start, Platzierung und Exploration vorgesehen. Anschließend wurde ein Auswahlzustand ergänzt, damit Nutzer:innen zuerst entscheiden können, ob sie ein Sonnensystem oder einen einzelnen Planeten betrachten möchten. In einer weiteren Iteration wurde der Umfang deutlich erweitert: Placement, Sonnensystemansicht, schwebender Einzelplanet und ein immersiver Modus wurden als getrennte Zustände gedacht.

Diese Erweiterung war aus konzeptioneller Sicht nachvollziehbar, weil sie viele Ideen des späteren Prototyps bereits vorwegnahm. Technisch führte sie aber zu einer hohen Komplexität. Mehrere UI-Panels, unterschiedliche Passthrough-Zustände, Placement-Logik, InfoPanel-Systeme und ein immersiver Szenenwechsel mussten gleichzeitig koordiniert werden. In dieser Phase wurde deutlich, dass ein XR-Prototyp nicht nur durch neue Features wächst, sondern auch durch die Fähigkeit, den Umfang wieder zu reduzieren, wenn die technische Stabilität darunter leidet.

Ein Beispiel dafür ist das frühe InfoPanel-System. Zunächst lag das steuernde Script direkt auf dem World-Space-Canvas, der zur Laufzeit ausgeblendet wurde. Dadurch deaktivierte Unity nicht nur das sichtbare Panel, sondern auch das Script selbst; die eigentlich dauerhaft benötigte Raycast-Logik konnte nicht mehr weiterlaufen. Die Lösung bestand darin, die steuernde Logik auf ein persistent aktives Systemobjekt auszulagern und nur das sichtbare Canvas ein- oder auszublenden. Diese kleine technische Korrektur hatte größere Bedeutung für den weiteren Entwurf: Steuerlogik und sichtbare UI sollten getrennt werden, damit die Anwendung nicht durch das Aktivieren oder Deaktivieren von UI-Elementen ihren eigenen Zustand verliert.

Ähnlich verlief die Entwicklung der Eingabe- und Placement-Logik. Erste Varianten mischten Desktop-Eingaben, OVRInput, Hand-Tracking und unterschiedliche Raycast-Ansätze. Für Tests im Editor war das hilfreich, für eine stabile Quest-Anwendung jedoch schwer wartbar. Deshalb wurde der Input in mehreren Schritten vereinfacht und stärker an den tatsächlichen XR-Fluss gebunden. Aus diesen Iterationen entstand die spätere Projektregel, vorhandene Meta-XR-Komponenten und Building Blocks bevorzugt zu nutzen und eigenen Interaktionscode nur dort zu schreiben, wo wirklich projektspezifische Logik benötigt wird.

### 1.4.3 Technische Sackgassen und bewusste Reduktion

Eine der wichtigsten Iterationen betraf die Erkennung und Platzierung von Objekten im Raum. Zunächst wurden verschiedene Wege über `OVRSceneManager`, `OVRSceneAnchor`, Physics-Collider und später MRUK ausprobiert. Das Archiv zeigt, dass diese Ansätze nicht einfach „falsch“ waren, sondern jeweils auf dem damaligen Kenntnisstand nahelagen. Sie wurden jedoch durch die Entwicklung des Meta SDK zunehmend problematisch: Bestimmte APIs waren veraltet, andere hatten sich zwischen SDK-Versionen geändert, und die Kombination mehrerer SDK-Generationen erzeugte zusätzliche Fehlerquellen.

Der entscheidende Umbau erfolgte im Zuge des Rebuilds von Placement und Hauptmenü. Dabei wurde der vorherige Prototyp-Vollumfang bewusst reduziert. Das Zustandsmodell wurde von einem komplexeren Modell mit mehreren Spezialzuständen auf einen schlankeren Ablauf zurückgeführt. Mehrere alte Implementierungen wurden entfernt und die Platzierung auf die Meta Depth API umgestellt. Statt einen vollständigen MRUK-Raum mit semantischen Raumdaten zu benötigen, nutzt die neue Platzierung einen Raycast gegen das Depth-Mesh und prüft anhand der Flächennormale, ob eine annähernd horizontale Fläche getroffen wurde.

Dieser Umbau ist für den Designprozess besonders aussagekräftig, weil er zeigt, dass Fortschritt im Prototyping nicht immer durch Hinzufügen entsteht. In diesem Fall entstand Fortschritt durch Weglassen. Für die konkrete Anwendung war keine vollständige semantische Raumerkennung nötig; es reichte, Planeten oder das Sonnensystem stabil auf Boden- oder Tischflächen platzieren zu können. Damit wurde eine einfachere technische Lösung gewählt, die besser zum tatsächlichen Nutzungsszenario passte.

Auch der immersive Modus durchlief eine solche Reduktion. Zwischenzeitlich gab es den Ansatz, eine Raumstation oder separate immersive Szene additiv zu laden. Diese Idee war visuell reizvoll, führte aber im XR-Kontext zu schwarzen Bildschirmen, Ruckeln und State-Problemen. Zudem musste beim Teleportieren des Spielers beachtet werden, dass die Rotation des OVR-Player-Roots nicht verändert werden darf, weil sonst der Tracking-Raum selbst mitrotiert. Aus diesen Problemen entstand später die Entscheidung, immersive Inhalte nicht mehr über Szenenwechsel, sondern innerhalb der MainScene über aktivierbare Root-Objekte und Passthrough-Dissolve zu organisieren. Auch hier wurde also eine spektakulärere technische Idee zugunsten eines stabileren Prototyp-Flusses zurückgenommen.

### 1.4.4 Datengetriebene Struktur als Ergebnis der Iterationen

Parallel zu den technischen Umbauten wurde immer deutlicher, dass planetenspezifische Informationen nicht in UI- oder Interaktionslogik verteilt werden dürfen. Der Prototyp sollte zunächst mit einzelnen Planeten funktionieren, später aber ohne großen Umbau auf alle acht Planeten erweitert werden können. Diese Anforderung führte zum datengetriebenen Ansatz mit zentralen Planeten-Datenassets.

Diese Entscheidung ist nicht nur eine technische Ordnungshilfe, sondern eine direkte Antwort auf die vorherigen Iterationen. Wenn Features verworfen oder umgebaut werden, dürfen astronomische Daten, Beschreibungstexte, Prefab-Referenzen und Skalierungswerte nicht in der jeweiligen Featurelogik verloren gehen. Durch die Auslagerung in ScriptableObjects können UI, Placement, Sonnensystemsimulation und Lernspiele generisch auf dieselben Daten zugreifen. Die planetenspezifische Information bleibt an einer Stelle, während die Systeme austauschbar bleiben.

Für den didaktischen Anspruch der Arbeit ist das wichtig, weil fachliche Inhalte dadurch besser pflegbar werden. Wenn Werte oder Texte später mit belastbaren Quellen abgeglichen werden, müssen sie nicht in mehreren Scripts gesucht und angepasst werden. Zugleich unterstützt die Struktur den Gamification-Ansatz: Quizlogik kann zum Beispiel den Durchmesser, die Reihenfolge oder weitere Eigenschaften aus denselben Daten lesen, die auch für die erklärende Ansicht verwendet werden. Dadurch entsteht eine engere Verbindung zwischen Exploration und Wissensprüfung.

TODO: Astronomische Daten und Beschreibungstexte vor finaler Abgabe noch systematisch mit belastbaren Quellen abgleichen und Quellen im Methodik-/Materialteil dokumentieren.

### 1.4.5 Von eigener Interaktionslogik zu Meta-SDK-nahen Systemen

Ein weiterer prägender Lernprozess betraf die XR-Interaktion. In frühen Versionen wurden Raycasts, Controller-Eingaben und Grab-Mechaniken teilweise selbst umgesetzt. Ein Beispiel ist der frühere Ansatz für Greif-Handles, bei dem eigene Raycasts, Input-Abfragen, Materialwechsel und zur Laufzeit erzeugte Handle-Objekte kombiniert wurden. Später wurde dieser Ansatz durch ein Prefab auf Basis des Meta Interaction SDK ersetzt. Die projektspezifische Logik schrumpfte dadurch deutlich: Statt Greifen, Raycast und Transformation selbst zu steuern, musste nur noch die Verbindung zwischen SDK-Interaktion und dem zu bewegenden Objekt hergestellt werden.

Diese Erfahrung beeinflusste direkt die späteren Lernspiele. Für das Reihenfolge-Minispiel wurde nicht versucht, eine eigene Grab- und Snap-Logik zu schreiben. Stattdessen wurde das Setup der Meta Interaction SDK Samples als Grundlage verwendet. Der eigene Code beschränkt sich auf die fachliche Regel: Welcher Planet gehört auf welchen Orbit, wie wird ein Slot bewertet und welches Feedback erhält der User? Das Greifen, Snappen und die Controller-Hand-Posen bleiben beim SDK.

Auch beim Größen-Minispiel wiederholte sich diese Logik. Zunächst war die genaue Funktionsweise der automatischen Snap-Liste unklar. Erst durch den Vergleich mit den Meta-Samples wurde deutlich, dass nicht die Liste selbst das Problem war, sondern fehlende Referenzen an den einzelnen Planetenobjekten. Diese Erkenntnis war ein typischer Prototyping-Moment: Das sichtbare Symptom lag an einer anderen Stelle als zunächst vermutet. Aus der Fehlersuche entstand ein stabileres Verständnis der SDK-Komponenten und die Entscheidung, die Snap-Liste aus den Samples als wiederverwendbares Muster im Projekt zu behandeln.

Für den Designprozess bedeutete das eine klare Arbeitsteilung: Die Meta-Samples liefern robuste Interaktions-Primitives, der eigene Projektcode ergänzt nur die Lernregel, das Feedback und die Anbindung an die Planetendaten. Dadurch bleibt die Anwendung näher an getesteten SDK-Komponenten und die Entwicklungszeit kann stärker in Inhalte, Aufgabenstruktur und Verständlichkeit investiert werden.

### 1.4.6 Iteration von UI, Lernfluss und Feedback

Neben der technischen XR-Grundlage wurde auch der Lernfluss mehrfach überarbeitet. Frühe UI-Varianten arbeiteten teilweise mit zur Laufzeit erzeugten Panels oder Platzhalter-Elementen. Für Desktop-Tests war das schnell, im Quest-Kontext aber ungeeignet, weil World-Space-Canvases für Controller-Ray-Interaktion korrekt mit den Meta Interaction SDK Komponenten vorbereitet werden müssen. Deshalb wurde die UI schrittweise stärker als Editor- und Prefab-System aufgebaut.

Der Test- bzw. Learn-Bereich und die Lernspiel-Prefabs sind ein Beispiel für diesen Wechsel. Statt Buttons und Panels zur Laufzeit zu erzeugen, enthalten die Lernspiel-Prefabs eigene World-Space-UIs. Ein zentrales Verwaltungssystem startet nur noch die passenden Prefabs und verwaltet den Fortschritt. Dadurch wird die UI nicht mehr als schnell erzeugter Debug-Ersatz behandelt, sondern als eigener Bestandteil der XR-Erfahrung.

Auch das Hauptmenü wurde iterativ bereinigt. Zeitweise existierten alte und neue UI-Hierarchien parallel, was zu falschen Referenzen und schwer nachvollziehbarem Verhalten führte. Der spätere Umbau auf ein einziges Hauptmenü und ein globales Detailpanel folgte derselben Erkenntnis wie die vorherigen technischen Reduktionen: Ein Prototyp wird nicht stabiler, wenn alte Systeme aus Vorsicht weiter aktiv bleiben. Für die weitere Arbeit ist eine eindeutige UI-Quelle besser als mehrere teilweise verdrahtete Alternativen.

Die Positionierung des Detailpanels zeigt zusätzlich, wie stark XR-UI von kleinen räumlichen Entscheidungen abhängt. Die erste Logik orientierte sich direkt an der Kamerablickrichtung. Dadurch beeinflusste auch die Kopfneigung nach oben oder unten die Panelposition. Später wurde die horizontale Blickrichtung verwendet, sodass das Panel links-vorne relativ zum User erscheint und aufrecht bleibt. Diese Änderung ist klein, aber für Lesbarkeit und Bedienkomfort zentral. Sie zeigt, dass Prototyping in XR nicht nur aus großen Architekturentscheidungen besteht, sondern auch aus vielen räumlichen Korrekturen, die den Unterschied zwischen „funktioniert technisch“ und „fühlt sich benutzbar an“ ausmachen.

### 1.4.7 Zwischenfazit des iterativen Prozesses

Der bisherige Entwicklungsverlauf zeigt, dass das Projekt nicht durch eine einzige große Implementierungsphase entstanden ist, sondern durch wiederholtes Prüfen, Vereinfachen und Neuordnen. Mehrere frühe Ansätze wurden nicht verworfen, weil sie grundsätzlich uninteressant waren, sondern weil sie für den konkreten Kontext der Masterarbeit zu instabil, zu aufwendig oder zu schwer wartbar wurden. Dazu gehören etwa der Raumstations-Szenenwechsel, MRUK-basierte Placement-Ansätze für einfache Boden- und Tischplatzierung, runtime-erzeugte XR-UI oder eigene Interaktionslogiken für Aufgaben, die das Meta SDK bereits zuverlässig abdeckt.

Aus diesen Iterationen entstanden mehrere Gestaltungsprinzipien, die den weiteren Prototyp tragen: datengetriebene Planeteninformationen, ein möglichst schlanker App-State, SDK-nahe Interaktionen, editorbasierte World-Space-UI und eine bewusste Trennung zwischen fachlicher Lernlogik und technischer XR-Grundfunktion. Der aktuelle Stand des Prototyps wird in einem späteren Kapitel detailliert beschrieben. Für dieses Kapitel ist entscheidend, dass diese Architektur nicht von Anfang an feststand, sondern aus konkreten Problemen, Tests und Umbauten hervorgegangen ist.

## 1.5 Gestaltung der Planeten

### 1.5.1 3D-Modell

Das 3D-Modell besteht im Wesentlichen aus einer einfachen Kugel, da planetare Verformungen in diesem Maßstab vernachlässigbar sind. Zur Optimierung der Performance wurden jedoch verschiedene LOD-Stufen angelegt, sodass die Planeten je nach Sichtbarkeit und Bedeutung im Bild in unterschiedlichen Detailgraden dargestellt werden. Auf diese Weise lässt sich die Gesamtzahl der Polygone gezielt reduzieren.

### 1.5.2 Materials

Die Planetenmaterialien sind ein zentraler Teil der visuellen Umsetzung, weil sie bestimmen, ob die Planeten nur wie einfache Kugeln wirken oder als unterschiedliche Himmelskörper erkennbar werden. Grundlage sind planetenspezifische Texturen, die aktuell von „Solar Textures“ stammen. Diese Texturen liefern die sichtbaren Oberflächen- bzw. Wolkenstrukturen.

Für die technische Umsetzung wurde ein gemeinsamer Shader Graph erstellt (`M_Planet.shadergraph`). Dieser Shader ist als allgemeiner Planeten-Shader angelegt und wird von mehreren Materialien genutzt. Die einzelnen Planeten bekommen also nicht jeweils eine komplett eigene Shaderlogik, sondern unterschiedliche Texturen und Materialeinstellungen innerhalb desselben Systems. Das passt zur Grundidee des Projekts: Zuerst wird ein belastbares generisches System aufgebaut, danach können weitere Planeten durch Material- und Datenanpassungen ergänzt werden.

Bei dem erstellten Shader handelt es sich um einen Unlit Shader. Das bedeutet: Die sichtbare Farbe wird nicht primär von Unitys normalem Lichtmodell berechnet, sondern im Shader selbst zusammengesetzt. Dieser Aufbau ist für das Projekt sinnvoll, weil die Planeten im Weltraum gezielt wie von der Sonne beleuchtete Körper erscheinen sollen. Ein klassisches Directional Light ist eher für allgemeine Szenenbeleuchtung gedacht, etwa für Tageslicht in einer Umgebung. Für die Planeten wäre es weniger flexibel, weil Lichtseite, Nachtseite, Atmosphäre, Wolken und Emission unabhängig von der restlichen Szene kontrolliert werden sollen.

#### 1.5.2.1 Grundstruktur des Shaders

Der Shader lässt sich als Pipeline verstehen, in der mehrere Bausteine nacheinander berechnet und am Ende zu einer finalen Planetenfarbe kombiniert werden. Die Grundstruktur des Shaders ist folgendermaßen aufgebaut: Links beginnt der Aufbau mit den UV-Koordinaten und der Textur, in der Mitte werden Licht, Atmosphäre, Wolken und Tiefenwirkung berechnet, rechts werden die Ergebnisse in einem finalen Farbwert zusammengeführt.

1. Zuerst werden die UV-Koordinaten der Kugel verwendet, um die Planetentextur korrekt auf der Oberfläche zu lesen.
2. Die Planetentextur liefert die Grundfarbe und damit die wichtigste visuelle Identität des Planeten.
3. Darauf wird eine eigene Lichtmaske berechnet, die Tagseite, Übergangsbereich und Nachtseite unterscheidet.
4. Optional werden Atmosphäreneffekt, Wolkenbewegung, Cloud Depth, Normal Map und Emission ergänzt.
5. Am Ende werden diese Anteile zu einer einzigen finalen Farbe kombiniert, die der Unlit-Shader ausgibt.

#### 1.5.2.2 Textur und UVs

Der erste Baustein ist die Texturabfrage. Der Shader berechnet die Texturkoordinaten neu, um die Verzerrung der Textur an den beiden Polen zu minimieren. Die Kugel besitzt UV-Koordinaten, über die festgelegt wird, welcher Bereich der Planetentextur an welcher Stelle der Kugel sichtbar ist.

In der Lernanwendung hat dieser Schritt eine didaktische Funktion. Die Planeten unterscheiden sich nicht nur durch Namen oder Zahlen im UI, sondern schon durch ihre visuelle Form. Der User kann also bereits vor dem Lesen eines Infotextes erkennen, ob es sich eher um einen felsigen Planeten oder einen Gasplaneten handelt. Gerade in XR ist diese unmittelbare Erkennbarkeit wichtig, weil die Nutzer:innen nicht nur Informationen lesen, sondern Objekte im Raum betrachten.

#### 1.5.2.3 Eigene Lichtberechnung

Der wichtigste technische Baustein ist die eigene Lichtberechnung. Dafür wird die Position der Sonne bzw. des Lichtobjekts über `SunPasser.cs` an die Planetenmaterialien weitergegeben. Der Shader vergleicht anschließend für jeden Punkt auf der Kugel, ob dessen Oberfläche zur Sonne zeigt oder von ihr wegzeigt.

Die Grundlage dafür ist das Punktprodukt zwischen Sonnenrichtung und Oberflächennormale. Die Normale beschreibt, in welche Richtung ein Punkt auf der Kugeloberfläche zeigt. Zeigt die Normale ungefähr zur Sonne, wird dieser Bereich hell. Zeigt sie seitlich zur Sonne, entsteht der Übergang zwischen Licht und Schatten. Zeigt sie von der Sonne weg, wird die Oberfläche abgedunkelt.

Aus dieser Berechnung entsteht eine einfache, aber gut verständliche Tag-/Nachtlogik:

- Die sonnenzugewandte Seite wird heller dargestellt.
- Der Rand zwischen Licht und Schatten bildet einen weichen Übergangsbereich.
- Die sonnenabgewandte Seite wird abgedunkelt.

Die Nachtseite wird dabei bewusst nicht zwingend vollständig schwarz. Das wäre zwar in manchen Weltraumdarstellungen naheliegend, würde in der XR-Anwendung aber wichtige Oberflächeninformationen verschlucken. Stattdessen kann die Stärke der Abdunklung pro Material angepasst werden. Dadurch bleibt die Lichtlogik verständlich, während die Planeten auch auf der dunkleren Seite noch als Objekt lesbar bleiben.

#### 1.5.2.4 Normal Map und Oberflächentiefe

Für feste Planeten kann zusätzlich eine Normal Map verwendet werden. Sie verändert nicht die tatsächliche Kugelgeometrie, sondern beeinflusst die Beleuchtungswirkung so, als hätte die Oberfläche feine Höhenunterschiede. Dadurch können Krater, Unebenheiten oder Oberflächenstrukturen plastischer wirken, ohne dass das Mesh selbst komplexer werden muss.

Dieser Ansatz ist besonders für die Quest 3 sinnvoll. Eine echte geometrische Oberfläche mit vielen Details wäre deutlich teurer als eine einfache Kugel mit einer guten Textur und Normal Map. Der Shader erzeugt also einen Tiefeneindruck, ohne zusätzliche Modellkomplexität zu benötigen.

Bei Gasplaneten ist die Situation anders. Sie besitzen keine feste sichtbare Oberfläche wie ein Gesteinsplanet. Eine klassische Normal Map wäre deshalb fachlich schnell missverständlich, wenn sie so gelesen würde, als hätten Gasbänder echte feste Erhebungen. Trotzdem sollen Jupiter, Saturn, Uranus oder Neptun nicht flach aussehen. Deshalb gibt es zusätzlich die Option Cloud Depth.

Cloud Depth leitet aus der Wolken- bzw. Gasband-Textur eine Normal-Map-artige Wirkung ab. Da diese Texturen im Shader bewegt werden können, kann sich auch die daraus abgeleitete Tiefenwirkung mitbewegen. Dadurch bekommen die Wolkenbänder eine subtile Plastizität, ohne dass behauptet wird, der Gasplanet habe eine feste Oberfläche. Es handelt sich also um einen bewussten visuellen Trick: Die Strukturen werden lesbarer und räumlicher, bleiben aber als atmosphärische bzw. gasförmige Muster interpretierbar.

#### 1.5.2.5 Atmosphäre und Randlicht

Ein weiterer Baustein ist die Atmosphäre. Sie wird über einen Fresnel-ähnlichen Effekt berechnet. Das bedeutet vereinfacht: Der Shader prüft, wie flach der Blick auf die Oberfläche fällt. In der Mitte der sichtbaren Planetenscheibe ist der Atmosphärenanteil schwächer. Am Rand wird er stärker, weil dort der Blick flacher über die Kugeloberfläche läuft.

Dadurch entsteht der Eindruck einer dünnen Hülle um den Planeten. Die Farbe und Stärke dieses Effekts kann pro Material angepasst werden. Bei der Erde kann die Atmosphäre z.B. bläulich wirken, während andere Planeten eine andere Randfärbung erhalten können. Auch hier geht es nicht um eine physikalisch exakte Simulation von Lichtstreuung, sondern um eine visuelle Annäherung, die in XR schnell verstanden wird.

Dieser Atmosphäreneffekt hat zwei Funktionen. Erstens macht er die Kugel räumlicher, weil der Rand nicht hart und flach wirkt. Zweitens unterstützt er die Lesbarkeit der Planeten als Himmelskörper, die nicht nur aus einer Textur bestehen, sondern eine visuelle Hülle besitzen.

#### 1.5.2.6 Wolken und Bewegung

Der Shader unterstützt mehrere zusätzliche Wolken- bzw. Gasband-Texturen. Diese können mit unterschiedlichen Geschwindigkeiten über die Oberfläche bewegt werden. Dadurch wirken Gasbänder und Wolken nicht vollständig statisch. Für Gasplaneten ist das besonders passend, weil ihre sichtbare Erscheinung stark von dynamischen Atmosphärenstrukturen geprägt ist.

Für die Erde gibt es zusätzlich eine eigene Wolkenschicht. Diese wird nicht nur im Shader als Texturanteil gedacht, sondern als leicht größere Schicht über dem Planeten platziert und durch `CloudBehaviour.cs` rotiert. Dadurch entsteht der Eindruck einer dünnen bewegten Atmosphäre über der Erdoberfläche. Die Wolken liegen nicht exakt auf derselben Oberfläche wie die Erde, sondern minimal darüber.

Die Wolkenbewegung erfüllt damit mehr als nur eine dekorative Aufgabe. Sie macht die Planeten lebendiger und hilft, zwischen festen Oberflächen und atmosphärischen Strukturen zu unterscheiden. Gerade in einer XR-Anwendung, in der der User die Planeten im Raum betrachtet, verhindert Bewegung, dass die Darstellung wie ein statisches Bild auf einer Kugel wirkt.

#### 1.5.2.7 Emission und finale Kombination

Am Ende werden alle berechneten Anteile zusammengeführt: Grundtextur, Lichtmaske, Abdunklung, optionale Normalwirkung, Wolken, Cloud Depth, Atmosphäre und Emission. Die Emission dient dabei als zusätzlicher Verstärker für die finale Sichtbarkeit und Farbwirkung. Da der Shader als Unlit-Shader arbeitet, ist diese finale Kombination besonders wichtig: Der Shader muss die gewünschte Lichtwirkung selbst erzeugen, anstatt sie vollständig Unitys Beleuchtungssystem zu überlassen.

#### 1.5.2.8 Begründung des gemeinsamen Shaders

Der gemeinsame Planeten-Shader ist vor allem aus Gründen der Wartbarkeit, Konsistenz und kontrollierten Komplexität sinnvoll. Alle Planeten folgen derselben visuellen Logik, können aber über ihre Materialien unterschiedlich eingestellt werden.

Das ist für die Entwicklung auf der Quest 3 relevant, weil viele einzelne Spezialshader schnell unübersichtlich werden würden. Jede Änderung an Beleuchtung, Atmosphäre oder Wolken müsste dann mehrfach gepflegt und getestet werden. Mit einem gemeinsamen Shader bleibt die Grundlogik an einer Stelle. Die Unterschiede zwischen den Planeten entstehen über Texturen und Parameter, nicht über getrennte technische Systeme.

Diese Entscheidung unterstützt auch die Skalierbarkeit des Projekts. Ein neuer Planet oder eine neue Materialvariante kann ergänzt werden, ohne dass ein komplett neuer Shader geschrieben werden muss. Damit folgt die visuelle Umsetzung demselben Prinzip wie die restliche Projektarchitektur: Planetenspezifische Unterschiede werden möglichst über Daten, Materialien und Einstellungen abgebildet, während die zugrunde liegenden Systeme generisch bleiben.

#### 1.5.2.9 Didaktische Einordnung

Für die wissenschaftliche Korrektheit ist wichtig, den Shader nicht als physikalisch exakte Simulation zu beschreiben. Die Lichtberechnung zeigt nachvollziehbar, dass ein Planet eine sonnenzugewandte und eine sonnenabgewandte Seite besitzt. Die Atmosphäre vermittelt den Eindruck einer Hülle, ohne echte atmosphärische Streuung vollständig zu berechnen. Cloud Depth gibt Gasstrukturen mehr Tiefe, ohne eine feste Oberfläche zu behaupten.

Damit ist der Shader vor allem eine didaktische Visualisierung. Er soll die Planeten in XR räumlich, unterscheidbar und verständlich machen. Für die Wissensvermittlung ist das zentral, weil der User astronomische Inhalte nicht nur als Text liest, sondern als visuelle und räumliche Objekte erlebt. Für die gamifizierten Teile des Projekts ist die Wiedererkennbarkeit ebenfalls wichtig: Wenn Planeten in Lernspielen sortiert oder verglichen werden, sollen sie nicht wie austauschbare Kugeln wirken, sondern als konkrete Himmelskörper erkennbar bleiben.

## 1.6 Status-Quo

Der aktuelle Prototyp von „Sonnensystem XR“ ist als zusammenhängende XR-Anwendung für die Meta Quest 3 aufgebaut. Im Unterschied zu früheren Entwicklungsständen werden die zentralen UI-Panels nicht mehr zur Laufzeit erzeugt, sondern sind als handgestaltete World-Space-UI in Unity angelegt und mit der jeweiligen Anwendungslogik verbunden. Dadurch wirken Menü, Detailansichten, Sonnensystemsteuerung und Lernspiele nicht wie voneinander getrennte technische Experimente, sondern wie Teile eines gemeinsamen Designsystems.

Diese Entscheidung betrifft nicht nur die visuelle Qualität, sondern auch die technische Zuverlässigkeit. World-Space-UI in XR benötigt eine saubere Vorbereitung der Canvas-, Ray- und Interaktionskomponenten. Wenn Panels zur Laufzeit erzeugt werden, entstehen schnell Abweichungen bei Layout, Skalierung, Controller-Ray-Interaktion und Lesbarkeit. Die aktuelle Umsetzung vermeidet diese Brüche, indem die Panels im Editor gestaltet, geprüft und anschließend nur noch datenbasiert befüllt oder ein- und ausgeblendet werden. Inhalte können dadurch dynamisch wechseln, während das visuelle Grundsystem stabil bleibt.

Der App-Flow gliedert sich in zwei zentrale Bereiche: den Home-Bereich mit Planet- und Solar-System-Auswahl sowie den Learn-Bereich mit interaktiven Wissensaufgaben. Die Anwendung verbindet damit exploratives Lernen mit aktiver Überprüfung. Nutzer:innen können zunächst Planeten oder das gesamte Sonnensystem betrachten und steuern; anschließend können sie ihr Wissen in Lernspielen anwenden.

### 1.6.1 Home-Bereich

Der Home-Bereich bildet den Einstieg in die Experience. Er dient nicht als klassische Start- oder Marketingseite, sondern als funktionales Hauptmenü im Raum. Von hier aus wählen die Nutzer:innen, ob sie einen einzelnen Planeten oder das Sonnensystem als Ganzes erleben möchten. Zusätzlich kann hier zwischen AR- und VR-Hintergrund gewechselt werden, sodass das System entweder im realen Raum über Passthrough oder vor einem reduzierten dunklen Hintergrund betrachtet werden kann.

Vor dem Home-Bereich kann der aktuelle Prototyp eine kurze Startsequenz aus Logo/Splash und Tutorial anzeigen. Das Tutorial ist modular aufgebaut: Ein `TutorialController` startet nacheinander vorbereitete Step-Prefabs, positioniert sie vor dem User und wartet darauf, dass der jeweilige Schritt abgeschlossen wird. Die Step-Prefabs können kurze Erklärvideos enthalten, die beim Einblenden automatisch geloopt werden. Dadurch lässt sich die Einweisung kompakt halten, ohne den Hauptscreen mit dauerhaften Hilfetexten zu überladen.

Für den aktuellen Prototyp ist diese Sequenz besonders relevant, weil die App mehrere Interaktionsformen kombiniert: World-Space-Buttons, Controller-Ray, räumliche Platzierung, Greifen und Snap-Interaktion. Das Tutorial schafft einen sanften Übergang zwischen dem Aufsetzen der Brille und der eigentlichen Exploration des Sonnensystems.

Das Home-Menü basiert auf einem einheitlichen, manuell gestalteten Menü-Root. Alle sichtbaren UI-Elemente folgen demselben Designsystem: Typografie, Abstände, Button-Stile, Karten und Tab-Zustände sind aufeinander abgestimmt. Die UI wird nicht durch Code nachgebaut, sondern im Unity Editor gepflegt. Die Anwendungslogik übernimmt nur die Funktionen dahinter, zum Beispiel das Binden der Planetendaten, das Umschalten von Bereichen oder das Starten der jeweiligen Experience.

### 1.6.2 Home: Planet-Bereich

Im Planet-Bereich können die acht Hauptplaneten des Sonnensystems ausgewählt werden: Merkur, Venus, Erde, Mars, Jupiter, Saturn, Uranus und Neptun. Jeder Planet ist über ein zentrales `PlanetData`-Asset beschrieben. Dieses enthält die planetenspezifischen Informationen, etwa Name, astronomische Kennwerte, Beschreibungstexte, Kurztexte für Detailkarten und Referenzen auf die visuellen Prefabs.

Nach der Auswahl eines Planeten startet die Platzierung im Raum. Der Nutzer richtet den Controller-Ray auf eine geeignete horizontale Fläche, etwa Boden oder Tisch, und bestätigt die Platzierung. Die Anwendung erzeugt dabei nicht für jeden Planeten ein eigenes Interaktionssystem, sondern nutzt einen generischen Interactable-Wrapper. Das sichtbare Planetmodell wird aus dem jeweiligen `PlanetData` geladen und in den gemeinsamen Wrapper eingesetzt. Dadurch bleibt die Interaktion für alle Planeten identisch, während Aussehen und Daten je nach Auswahl wechseln.

Nach der Platzierung erscheint das globale `PlanetDetailRoot`. Dieses Panel ist ebenfalls handgestaltet und wird zur Laufzeit nur mit den Daten des aktuell ausgewählten Planeten befüllt. Es zeigt zentrale Informationen wie Beschreibung, Durchmesser, Umlaufdistanz, Umlaufgeschwindigkeit, Achsneigung, Exzentrizität und relative Planetengröße. Wenn ein anderer Planet ausgewählt oder per Ray angeklickt wird, wird dasselbe Panel aktualisiert, statt ein neues planetenspezifisches Panel zu erzeugen.

Aus dem Planet-Detail heraus kann außerdem in eine immersive Ansicht gewechselt werden. Dabei wird der ausgewählte Planet nicht mehr nur als nah platzierbares Objekt gezeigt, sondern in einer separaten VR-Natur- bzw. Nachtszene an einem entfernten Spawnpunkt dargestellt, sodass er wie ein Himmelskörper am Mond-Ort erscheint. Das bestehende Detailpanel bleibt dabei Teil des Flows und dient gleichzeitig als Rückweg aus der immersiven Ansicht. Auch dieser Modus folgt somit dem Prinzip, vorhandene UI-Strukturen weiterzuverwenden, statt für jede Ansicht ein eigenes Bedienkonzept einzuführen.

Der Planet-Bereich unterstützt damit eine einfache Form explorativen Lernens. Die Nutzer:innen wählen ein Objekt aus, platzieren es im eigenen Raum und erhalten direkt zugehörige Informationen. Die räumliche Platzierung macht den Planeten nicht nur zu einem Bild auf einem Bildschirm, sondern zu einem Objekt im eigenen Umfeld. Gleichzeitig bleibt die Darstellung kontrolliert und verständlich, weil nicht mehrere UI-Systeme oder überladene Informationsfenster parallel erscheinen.

### 1.6.3 Home: Solar-System-Bereich

Der Solar-System-Bereich zeigt nicht einen einzelnen Planeten, sondern das gesamte Sonnensystem als räumliches Modell. Nach der Auswahl wird das Sonnensystem im Raum platziert. Die Sonne bildet den Mittelpunkt, die Planeten bewegen sich auf berechneten Bahnen um sie herum. Für die Umlaufbahnen sind visuelle Orbit-Linien vorhanden, sodass die Bewegungen nicht nur als einzelne wandernde Kugeln wahrgenommen werden, sondern als nachvollziehbares System aus Bahnen, Abständen und Umlaufgeschwindigkeiten.

Das Sonnensystem wird über ein zentrales Simulationssystem gesteuert. Dieses erzeugt die Planeten auf Grundlage der vorhandenen `PlanetData`-Assets und berechnet ihre Positionen über skalierbare Parameter. Dadurch können reale astronomische Größenverhältnisse didaktisch angepasst werden. Da echte Distanzen und Durchmesser im selben Maßstab für eine Raum-XR-Anwendung kaum sinnvoll darstellbar wären, arbeitet der Prototyp mit steuerbaren Skalierungsfaktoren. Diese machen sichtbar, was im echten Sonnensystem sonst entweder zu klein, zu groß oder zu weit entfernt wäre.

Zum Solar-System-Bereich gehört das `SolarSystemRoot`-Panel. Es ist korrekt mit der jeweils platzierten Sonnensystem-Instanz verbunden und steuert nicht irgendeinen statischen Inspector-Verweis, sondern genau das aktuell erzeugte Modell im Raum. Die Slider funktionieren als didaktische Steuerung: Sie verändern unter anderem Planetengröße, Umlaufgeschwindigkeit, Achsneigung, Orbit-Abstände und Exzentrizität. Zusätzlich gibt es einen Spacing-Modus, der die Darstellung der Abstände beeinflusst und den Unterschied zwischen kompakter didaktischer Ansicht und stärker realitätsnaher Entfernung erfahrbar macht.

Die Orbit-Visualizer unterstützen diese Steuerung visuell. Wenn Nutzer:innen Parameter verändern, reagieren nicht nur die Planetenkörper, sondern auch die Darstellung der Bahnen. Dadurch wird das Sonnensystem als dynamisches Modell erfahrbar. Es geht nicht darum, eine physikalisch vollständige Simulation zu erzeugen, sondern um eine verständliche Lernvisualisierung: Die Nutzer:innen können Zusammenhänge zwischen Umlaufbahn, Abstand, Geschwindigkeit und Maßstab direkt ausprobieren.

### 1.6.4 Learn-Bereich

Der Learn-Bereich ergänzt die freie Exploration um gamifizierte Aufgaben. Während der Home-Bereich vor allem der Betrachtung und Orientierung dient, fordert der Learn-Bereich eine aktive Anwendung des Wissens. Die Nutzer:innen sollen Planeten nicht nur ansehen, sondern ihre Eigenschaften und Reihenfolgen handelnd einsetzen.

Auch hier sind die Panels und Lernspiel-Oberflächen nicht zur Laufzeit generiert, sondern als gestaltete Prefabs in Unity vorbereitet. Die Lernspiele werden zentral gestartet und im Raum vor dem User platziert. Die Interaktion basiert auf Komponenten aus dem Meta Interaction SDK, insbesondere auf Snap- und Grab-Mechaniken. Der eigene Projektcode ergänzt die fachliche Prüfung, das Feedback und die Fortschrittslogik.

Der Learn-Bereich enthält aktuell zwei umgesetzte Aufgaben: Reihenfolge und Größe. Ein drittes Gravity-Minispiel ist als zukünftige Erweiterung vorgesehen, aber noch nicht implementiert. Es bleibt daher im aktuellen Prototyp als Platzhalter oder Ausblick erhalten und wird nicht als fertige Funktion beschrieben.

### 1.6.5 Learn: Reihenfolge-Minispiel

Das Reihenfolge-Minispiel prüft, ob die Nutzer:innen die Reihenfolge der Planeten im Sonnensystem verstanden haben. Die Planeten liegen als greifbare Objekte in einer Snap-Liste bereit und können mit den XR-Controllern aufgenommen und auf Orbit-Slots gelegt werden. Jeder Slot steht für eine Position im Sonnensystem, ausgehend von der Nähe zur Sonne.

Die Aufgabe ist bewusst körperlich-räumlich umgesetzt. Anstatt eine Liste anzuklicken oder Multiple-Choice-Antworten auszuwählen, müssen die Planeten aktiv an die passenden Orte bewegt werden. Dadurch verbindet das Minispiel Wissensabfrage mit räumlicher Handlung. Die Reihenfolge wird nicht nur abstrakt abgefragt, sondern im Modell des Sonnensystems nachvollzogen.

Technisch basiert das Minispiel auf dem Snap-Aufbau des Meta Interaction SDK. Die Planeten nutzen vorhandene Grab- und Snap-Komponenten; der eigene Code prüft nur, ob der Planet mit dem richtigen Orbit-Slot verbunden ist, gibt visuelles Feedback und bewertet am Ende, ob alle aktiven Slots korrekt belegt sind.

Für die Lernwirkung ist dieses Feedback entscheidend. Die Nutzer:innen sehen unmittelbar, ob eine Platzierung richtig oder falsch ist, und können ihre Entscheidung korrigieren. Damit entsteht eine niedrigschwellige Übungsschleife: ausprobieren, Rückmeldung erhalten, neu ordnen. Diese Struktur unterstützt den gamifizierten Charakter der Anwendung, ohne die Aufgabe in ein reines Punktesystem zu verwandeln.

### 1.6.6 Learn: Größen-Minispiel

Das Größen-Minispiel konzentriert sich auf die relativen Größenverhältnisse der Planeten. Auch hier greifen die Nutzer:innen Planeten aus einer Liste und ordnen sie passenden Slots zu. Im Unterschied zum Reihenfolge-Minispiel geht es jedoch nicht um die Position im Sonnensystem, sondern um den Vergleich der Planetendurchmesser.

Die besondere didaktische Herausforderung liegt darin, dass reale Planetengrößen in einer XR-Anwendung nur schwer im echten Maßstab darstellbar sind. Jupiter ist im Vergleich zur Erde sehr groß, während Merkur deutlich kleiner ist. Würde man alle Objekte vollständig realistisch skalieren, könnten einzelne Planeten zu klein zum Greifen oder andere zu groß für den Raum werden. Das Minispiel nutzt deshalb eine relative Darstellung: Die Größen werden auf einen sinnvollen XR-Maßstab übertragen, sodass die Unterschiede sichtbar und zugleich handhabbar bleiben.

Die Auswertung übernimmt eine zentrale Prüflogik. Sie sammelt die Planeten und Slots aus dem Prefab, prüft die aktuelle Belegung und nutzt den Durchmesser aus den Planetendaten, um die erwartete Reihenfolge zu bestimmen. Korrekt platzierte Planeten wachsen sichtbar auf ihre relative Zielgröße an. Falsch platzierte Planeten erhalten Feedback und kehren nach kurzer Zeit in die Liste zurück. Dadurch wird die Aufgabe nicht nur als richtig oder falsch bewertet, sondern die Größenrelation wird unmittelbar visualisiert.

Auch dieses Minispiel verwendet die Snap-Mechaniken des Meta SDK. Die Projektlogik bleibt auf die Lernregel beschränkt: Welche Reihenfolge ergibt sich aus den Durchmessern, wie wird eine Platzierung bewertet und welches Feedback wird angezeigt? Damit bleibt die technische Interaktion robust, während die fachliche Aussage im Vordergrund steht.

### 1.6.7 Zusammenfassung des Ist-Zustands

Der aktuelle Prototyp verbindet drei zentrale Ebenen: eine räumliche Darstellung des Sonnensystems, ein einheitliches handgestaltetes UI-System und gamifizierte Lernaufgaben. Der Home-Bereich ermöglicht die Auswahl und Betrachtung einzelner Planeten sowie des gesamten Sonnensystems. Der Planet-Bereich arbeitet mit einem globalen Detailpanel und datenbasierten Planet-Assets. Der Solar-System-Bereich stellt Planetenbahnen, Orbit-Visualisierung und steuerbare Skalierungsparameter bereit. Der Learn-Bereich ergänzt diese Exploration durch die Lernspiele Reihenfolge und Größe.

Damit erfüllt der Prototyp die grundlegende Zielrichtung der Arbeit: astronomische Inhalte werden nicht nur präsentiert, sondern in einer XR-Umgebung räumlich erfahrbar und durch Interaktion überprüfbar gemacht. Gleichzeitig bleibt die Umsetzung auf Erweiterbarkeit ausgelegt. Neue Planeteninformationen, weitere Aufgaben oder zusätzliche Lerntexte können auf Grundlage der bestehenden Datenstruktur ergänzt werden, ohne das gesamte System neu aufzubauen.

## 2 Evaluation

## 2.1 Methodik und User Testing

Die Evaluation des Prototyps wurde als exploratives User Testing angelegt. Ziel war nicht, mit einer großen Stichprobe einen allgemeingültigen Wirkungsnachweis zu erbringen, sondern den entwickelten Prototyp im Hinblick auf die zentrale Forschungsfrage zu prüfen: Wie kann eine XR-Lernanwendung zum Sonnensystem gestaltet werden, um astronomische Maßstabsverhältnisse fachlich nachvollziehbar, räumlich erfahrbar und spielerisch motivierend zu vermitteln?

Die Studie wurde mit sieben Teilnehmer:innen im Zeitraum vom 6. bis 11. Mai 2026 durchgeführt. Die Stichprobe ist als kleine, nicht repräsentative Gelegenheitsstichprobe zu verstehen. Die Teilnehmer:innen kamen aus unterschiedlichen beruflichen bzw. gestalterischen Hintergründen, darunter Interaction Design, Media Design, 3D Design, Videografie, Soziale Arbeit und Quality Management. Die XR-Erfahrung war unterschiedlich verteilt: Einige Personen hatten bereits mehrfach oder regelmäßig VR-/XR-Brillen genutzt, andere nur selten oder gar nicht. Der Altersbereich reichte von 25 bis 61 Jahren; der Durchschnitt der numerischen Altersangaben lag bei 33 Jahren.

Die Evaluation orientiert sich damit an drei Teilaspekten:

- **Fachliche Nachvollziehbarkeit:** Die Nutzer:innen sollen nicht nur einzelne Fakten wiedergeben, sondern verstehen, warum Darstellungen des Sonnensystems skaliert und didaktisch vereinfacht werden müssen. Besonders relevant sind Größenverhältnisse, Distanzen, Orbitbewegungen und der Unterschied zwischen realen astronomischen Daten und einer handhabbaren XR-Visualisierung.
- **Räumliche Erfahrbarkeit:** Die Anwendung soll prüfen lassen, ob XR über eine flache Darstellung hinaus einen Mehrwert bietet. Beobachtet wird deshalb, ob Teilnehmer:innen den Raum aktiv nutzen, Perspektiven wechseln, sich dem Modell nähern oder die Wirkung von Planetengrößen, Abständen und Orbits im Raum beschreiben können.
- **Spielerische Motivation:** Die Lernspiele sollen nicht nur als Zusatzfunktion erscheinen, sondern als motivierende Form der Wissensanwendung. Die Evaluation fragt deshalb danach, ob Reihenfolge- und Größenaufgaben verständlich sind, direktes Feedback bieten und das Lernen aktivieren.

Der Ablauf kombinierte quantitative und qualitative Elemente. Vor der Nutzung beantworteten die Teilnehmer:innen Fragen zu Vorerfahrung, Selbsteinschätzung und Grundlagenwissen über das Sonnensystem. Danach bearbeiteten sie zentrale Aufgaben im Prototyp: Sie starteten den Home-/Planet-Flow, platzierten einen Planeten im Raum, lasen Detailinformationen, verglichen mehrere Planeten, platzierten das Sonnensystem, nutzten Maßstabs- und Orbit-Controls und bearbeiteten die beiden Lernspiele Reihenfolge und Größe. Anschließend folgten ein Post-Fragebogen und offene Abschlussfragen.

Die Aufgaben waren so gewählt, dass sie nicht nur technische Bedienbarkeit prüfen, sondern direkt Material für die Forschungsfrage erzeugen. Wenn eine Person beispielsweise die Slider nutzt, ist nicht nur relevant, ob sie den Regler bedienen kann. Entscheidend ist, ob sie anschließend erklären kann, dass echte Distanzen im Sonnensystem im Verhältnis zu Planetengrößen extrem groß sind und deshalb in einer XR-Anwendung bewusst skaliert werden müssen. Ebenso wurde beim Größen-Minispiel nicht nur eine richtige Reihenfolge erwartet, sondern ein sichtbarer Größenvergleich erzeugt, der die abstrakte Information körperlich-räumlich unterstützt.

Für die Auswertung wurden Pre- und Post-Wissenswerte verglichen. Ergänzend wurden Likert-Skalen zu räumlichem XR-Mehrwert, Gamification/Motivation, wissenschaftlicher Glaubwürdigkeit sowie Usability/Tutorial ausgewertet. Die Beobachtungsdaten wurden zusätzlich qualitativ betrachtet, insbesondere mit Blick auf die Kategorien `fachlich nachvollziehbar`, `räumlich erfahrbar` und `spielerisch motivierend`. Aufgrund der kleinen Stichprobe und fehlenden Kontrollgruppe werden die Ergebnisse deskriptiv und vorsichtig interpretiert. Sie liefern Hinweise auf Wirkung und Gestaltungsqualität, ersetzen aber keine repräsentative Wirksamkeitsstudie.

## 2.2 Durchführung

Die Durchführung bestand aus Fragebogen, XR-Nutzung, Beobachtung und Nachbefragung. Während der App-Nutzung wurde dokumentiert, ob einzelne Aufgaben erfolgreich abgeschlossen wurden, welche Hilfestellung nötig war und ob Hinweise auf Unwohlsein oder Motion Sickness auftraten. Die Bedienbarkeit blieb dabei eine wichtige Kontrollperspektive. Wenn Teilnehmer:innen durch Controller-Probleme, unlesbare Panels oder instabile Platzierung abgelenkt werden, lassen sich Motivation und Lernwirkung nur eingeschränkt beurteilen. Deshalb wurden Hilfestellungen, Fehler, Rückwege, Orientierung im Raum und Auffälligkeiten bei den Interaktionen mitprotokolliert.

Für die konkrete Durchführung des User Testings wurde zusätzlich ein eigener Vorbereitungsmodus implementiert. Dieser Modus ist nicht als reguläre Lernfunktion für Endnutzer:innen gedacht, sondern als Werkzeug für die Testsituation. Die Testleitung kann ihn durch gleichzeitiges Drücken beider Controller-Thumbsticks aktivieren. Daraufhin wechselt die Anwendung in eine ruhige Passthrough-Ansicht: Die Testperson sieht ihre reale Umgebung durch die Brille, während App-Inhalte ausgeblendet, laufende Inhalte zurückgesetzt und Audio pausiert werden.

Der Zweck dieses Modus liegt vor allem in der Reduktion kognitiver Belastung. Direkt nach dem Aufsetzen einer XR-Brille müssen unerfahrene Personen häufig mehrere Dinge gleichzeitig verarbeiten: Sitz der Brille, Schärfe, räumliche Orientierung, Controllerposition, Sicherheit im realen Raum und erste digitale Inhalte. Wenn in diesem Moment bereits Menüs, Animationen, Planeten oder Erklärvideos sichtbar sind, kann die Aufmerksamkeit überlastet werden, bevor das eigentliche Testing überhaupt beginnt.

Der Passthrough-Vorbereitungsmodus trennt deshalb die technische und körperliche Eingewöhnung vom inhaltlichen Teststart. Zuerst kann die Testperson ihren realen Raum sehen, die Brille kann ruhig angepasst werden und die Testleitung kann bei Sitz, Orientierung und Komfort helfen. Erst danach startet die eigentliche App-Sequenz erneut über denselben Thumbstick-Shortcut mit Logo, Tutorial und anschließendem Hauptmenü.

Für die Evaluation ist diese Trennung methodisch wichtig. Wenn Startstress, Unsicherheit beim Aufsetzen oder Überforderung durch gleichzeitige Reize die ersten Minuten dominieren, könnten spätere Aussagen zur Usability, Motivation oder Lernwirkung verzerrt werden. Der Vorbereitungsmodus soll deshalb nicht nur Komfort erhöhen, sondern auch die Datenerhebung sauberer machen: Beobachtete Probleme während der Aufgaben sollen möglichst aus der App-Interaktion selbst entstehen und nicht aus vermeidbarer Unsicherheit beim initialen Headset-Setup.

In der Auswertung kann dieser Modus als praktische Maßnahme beschrieben werden, um die Testsituation zu standardisieren. Alle Teilnehmer:innen können zuerst in einer neutralen Passthrough-Situation ankommen, bevor die eigentliche Erfahrung startet. Gleichzeitig bleibt zu dokumentieren, ob trotz dieser Vorbereitung Hilfestellung nötig war, zum Beispiel bei Controllerhaltung, Lesbarkeit, räumlicher Orientierung oder körperlichem Komfort.

Die eigentliche App-Nutzung umfasste mehrere Aufgaben: Einstieg in den Lernmodus, Lesen von Planetendetails, Hinzufügen bzw. Erkunden weiterer Planeten, Platzierung des Sonnensystems, Nutzung der Skalierungs- und Orbit-Controls sowie die beiden Lernspiele zur Planetenreihenfolge und Planetengröße. Die Aufgaben konnten insgesamt von allen Teilnehmer:innen mindestens teilweise abgeschlossen werden. Gleichzeitig zeigten die Beobachtungen, dass die XR-Interaktion an mehreren Stellen noch erklärungsbedürftig war. Besonders häufig waren kleine verbale Hinweise beim Menüaufruf, beim erneuten Platzieren weiterer Planeten, beim Verständnis des `Compressed`-Toggles bzw. der Skalierungs-Controls und beim Erkennen der Platzierungsringe im Größen-Minispiel nötig.

Als Komfortbefund ist relevant, dass sechs von sieben Personen ohne Anzeichen von Unwohlsein beobachtet wurden. Eine Person wurde mit leichtem Unwohlsein codiert. Starke Motion-Sickness-Symptome oder ein Abbruch aufgrund körperlichen Unwohlseins traten nicht auf. Damit stützt die Durchführung grundsätzlich die technische Entscheidung, keine künstliche Fortbewegung und keine instabilen Szenenwechsel in den Hauptflow einzubauen.

## 2.3 Ergebnisse

### 2.3.1 Wissenszuwachs und fachliche Nachvollziehbarkeit

Die Wissensfragen zeigen eine deutliche Verbesserung zwischen Pre- und Post-Test. Der durchschnittliche Pre-Wissensscore lag bei 6 von 10 Punkten, der durchschnittliche Post-Wissensscore bei 9,14 von 10 Punkten. Die mittlere Verbesserung betrug damit +3,14 Punkte. Der Median stieg von 6 auf 9 Punkte. Alle sieben Teilnehmer:innen verbesserten ihren Wissensscore; die individuellen Verbesserungen lagen zwischen +1 und +9 Punkten.

| Kennzahl | Wert |
|---|---:|
| Teilnehmer:innen | 7 |
| Pre-Wissensscore Ø | 6 / 10 |
| Post-Wissensscore Ø | 9,14 / 10 |
| Durchschnittliche Verbesserung | +3,14 Punkte |
| Median Pre/Post | 6 / 9 |
| Individuelle Verbesserungen | +1 bis +9 Punkte |

Aufgrund der kleinen Stichprobe ist dieses Ergebnis nicht als allgemeiner Wirkungsnachweis zu verstehen. Dennoch ist auffällig, dass keine Person im Post-Test schlechter abschnitt und alle Personen mindestens eine Verbesserung zeigten. Als explorativer Hinweis kann zusätzlich ein einfacher Vorzeichen-Test herangezogen werden: Bei sieben Verbesserungen in sieben Fällen ergibt sich für die gerichtete Annahme einer Verbesserung ein exakter p-Wert von 0,0078. Dieser Wert ist aufgrund der Stichprobengröße und der fehlenden Kontrollgruppe vorsichtig zu interpretieren, unterstützt aber die Beobachtung, dass sich die Wissenswerte nach der Nutzung systematisch nach oben verschoben.

Inhaltlich betrafen die Verbesserungen besonders Grundlagenwissen zur Anzahl der Planeten, zur Reihenfolge, zu Größenverhältnissen und zur Schwierigkeit maßstabsgetreuer Sonnensystemdarstellungen. Gerade letzterer Punkt ist für die Arbeit zentral: Die Post-Antworten zeigen, dass die Teilnehmer:innen die Problematik großer Distanzen im Verhältnis zu Planetengrößen nach der Nutzung überwiegend korrekt einordnen konnten.

Auch die Selbsteinschätzungen verbesserten sich deutlich. Besonders stark stiegen die Einschätzungen zu Größenunterschieden, Distanzen und Planetenreihenfolge. Das Interesse am Thema war bereits vor der Nutzung hoch und veränderte sich deshalb nur gering.

| Item | Pre Ø | Post Ø | Delta |
|---|---:|---:|---:|
| Reihenfolge der Planeten benennen | 1,86 | 4,00 | +2,14 |
| Größenunterschiede vorstellen | 1,57 | 4,14 | +2,57 |
| Distanzen zwischen Sonne und Planeten vorstellen | 1,29 | 3,71 | +2,42 |
| Umlaufbahn verstehen | 2,29 | 4,00 | +1,71 |
| Interesse am Sonnensystem | 4,57 | 4,71 | +0,14 |

Die offenen Antworten bestätigen diese Richtung qualitativ. Mehrere Teilnehmer:innen nannten Größen, Distanzen, Reihenfolge und Umlaufbewegungen als zentrale Lerneffekte. Eine Person beschrieb, dass die Skalierung geholfen habe, sich vorzustellen, „how big the space and planets are“. Eine andere Antwort hob hervor, dass die Skalierungs-Controls die extrem großen Abstände sichtbar machten und zugleich eine kompakte Ansicht ermöglichten, in der alle Planeten erfassbar bleiben. Damit zeigt sich, dass die didaktische Skalierung nicht nur als technische Notwendigkeit, sondern als Lernwerkzeug wahrgenommen wurde.

### 2.3.2 Räumliche Erfahrbarkeit und XR-Mehrwert

Die Skala `Value of XR Visualization` erreichte einen Mittelwert von 4,2 bei einem Median von 4. Die räumliche Darstellung wurde also überwiegend positiv bewertet. In den offenen Antworten wurde besonders häufig die Gesamtansicht des Sonnensystems genannt. Teilnehmer:innen beschrieben, dass ihnen die räumliche Anordnung, die Möglichkeit des Vergleichs und das Ausprobieren der Controls beim Verständnis geholfen hätten. Eine Antwort nannte explizit „placing the whole solar system in the room and trying out the different controls“ als wichtigsten Verständnistreiber.

Die Beobachtungsdaten stützen diesen Befund teilweise. Das Sonnensystem konnte von allen sieben Teilnehmer:innen platziert werden. Vier Personen benötigten dabei keine Hilfe, drei Personen nur einen kleinen verbalen Hinweis. Die Aufgabe, die Skalierungs-Controls zu nutzen, wurde von sechs Personen vollständig und von einer Person teilweise abgeschlossen. Allerdings benötigten hier sechs Personen einen kleinen Hinweis. Das deutet darauf hin, dass der didaktische Wert der Controls hoch ist, ihre Auffindbarkeit bzw. Bedienaufforderung aber noch verbessert werden kann.

| Aufgabe | Yes | Partly | No | 0 Hilfe | 1 Hinweis | 2 Eingriff |
|---|---:|---:|---:|---:|---:|---:|
| Start learning mode | 5 | 2 | 0 | 2 | 4 | 1 |
| Read planet details | 6 | 1 | 0 | 5 | 2 | 0 |
| Add/explore more planets | 6 | 1 | 0 | 2 | 5 | 0 |
| Place solar system | 7 | 0 | 0 | 4 | 3 | 0 |
| Use scaling controls | 6 | 1 | 0 | 1 | 6 | 0 |
| Planet order minigame | 7 | 0 | 0 | 7 | 0 | 0 |
| Planet size minigame | 7 | 0 | 0 | 2 | 3 | 2 |

Besonders relevant ist, dass die Teilnehmer:innen die räumliche Darstellung nicht nur allgemein positiv bewerteten, sondern konkrete fachliche Aspekte damit verbanden: Größe, Abstand, Geschwindigkeit, Orbitform und Anordnung. Mehrere Personen erwähnten, dass die äußeren Planeten deutlich weiter von der Sonne entfernt seien als erwartet. Andere beschrieben den Vergleich zwischen kleinen inneren Planeten und großen Gasplaneten als einprägsam. Die XR-Darstellung unterstützte damit den Kernaspekt der Arbeit: astronomische Maßstabsverhältnisse wurden nicht nur gelesen, sondern räumlich erlebt und manipuliert.

### 2.3.3 Gamification und Motivation

Die Skala `Gamification and Motivation` erreichte einen Mittelwert von 4,31 und einen Median von 5. Die Lernspiele wurden damit insgesamt stark positiv bewertet. Besonders das Reihenfolge-Minispiel zeigte eine hohe Stabilität: Alle sieben Teilnehmer:innen schlossen es erfolgreich ab, und keine Person benötigte Hilfestellung. In den offenen Antworten wurde es mehrfach als motivierendste Aufgabe genannt, etwa als „Placing the Planets in order minigame“ oder als „tests in the end, especially the first one“.

Das Größen-Minispiel wurde ebenfalls von allen Teilnehmer:innen abgeschlossen, erforderte aber mehr Unterstützung. Zwei Personen benötigten einen klaren Eingriff, drei weitere einen kleinen Hinweis. Die Beobachtungen zeigen, dass nicht die fachliche Aufgabe selbst das Hauptproblem war, sondern die Sichtbarkeit und Bedienbarkeit der Platzierungsringe bzw. der kleinen Planetenobjekte. Damit ergibt sich eine wichtige Unterscheidung: Die Spielidee wurde verstanden und abgeschlossen, die Interaktionsgestaltung erzeugte aber mehr Reibung als beim Reihenfolge-Minispiel.

Die qualitativen Antworten legen nahe, dass die Lernspiele als motivierendes Element funktionieren, aber unterschiedlich stark. Mehrere Personen wünschten sich weitere Aufgaben oder Level. Eine Person mit geringerer Usability-/Tutorial-Bewertung beurteilte die Gamification-Skala zurückhaltender. Das deutet darauf hin, dass Motivation in diesem Prototyp eng an Bedienbarkeit gekoppelt ist: Wenn Greifen, Platzieren oder UI-Hinweise reibungslos funktionieren, werden die Lernspiele als aktivierend erlebt; wenn die Interaktion unklar ist, kann die spielerische Aufgabe schneller als technische Hürde wahrgenommen werden.

### 2.3.4 Wissenschaftliche Glaubwürdigkeit

Die Skala `Scientific Credibility and Trust` erreichte mit 4,4 den höchsten Mittelwert der Post-Test-Skalen. Die Teilnehmer:innen hatten überwiegend den Eindruck, dass die Anwendung fachlich glaubwürdig wirkt und dass die Planeten nicht beliebig, sondern datenbasiert dargestellt werden. Besonders relevant ist, dass mehrere Personen die Notwendigkeit von Skalierung oder Vereinfachung erkannten. Damit wurde ein zentrales Spannungsfeld der Arbeit sichtbar: Wissenschaftliche Korrektheit bedeutet in diesem XR-Kontext nicht, alle Größen und Distanzen im selben echten Maßstab zu zeigen, sondern transparent und didaktisch begründet mit Maßstabsmodellen zu arbeiten.

Gleichzeitig zeigten die Antworten, dass Quellen und weiterführende Informationen für eine finale Version sinnvoll wären. Einige Teilnehmer:innen wünschten sich Quellen oder zusätzliche Informationen in der Anwendung. Das passt zur wissenschaftlichen Zielsetzung der Masterarbeit: Wenn eine XR-Lernanwendung fachliche Korrektheit beansprucht, sollte sie nicht nur datenbasiert arbeiten, sondern diese Datenherkunft auch sichtbar oder nachvollziehbar machen.

### 2.3.5 Usability, Tutorial und technische Reibung

Die Skala `Usability & Tutorial` erreichte einen Mittelwert von 4,22 und einen Median von 4. Das Tutorial wurde überwiegend als hilfreich bewertet. Mehrere Personen gaben hohe Werte für das Verständnis der Grundsteuerung nach dem Tutorial. Gleichzeitig zeigen Beobachtungen und offene Antworten, dass das Tutorial nicht alle Bedienprobleme vollständig auffangen konnte.

Wiederkehrende Probleme betrafen:

- Menüaufruf und Rückkehr ins Hauptmenü.
- Platzieren eines zweiten Planeten bzw. Verständnis, dass weitere Planeten platziert werden können.
- Hand-Tracking, Pinch-Gesten und Greifmechaniken.
- Slider-Trefferflächen bzw. das genaue Anklicken der Controls.
- Sichtbarkeit der Platzierungsringe im Größen-Minispiel.
- Lesbarkeit kleiner Texte bzw. Labels auf kleinen Planeten.
- Drehen sehr großer Planeten.

Diese Probleme traten nicht so stark auf, dass Aufgaben scheiterten, sie führten aber wiederholt zu kleinen Hilfestellungen. Besonders deutlich ist dies bei `Use scaling controls`: Obwohl die Controls als lernförderlich wahrgenommen wurden, benötigten fast alle Teilnehmer:innen mindestens einen Hinweis, etwa zum `Compressed`-Toggle oder dazu, dass die Controls aktiv verwendet werden sollen. Ähnlich verhält es sich beim Größen-Minispiel: Die Aufgabe wurde abgeschlossen, aber die Sichtbarkeit der Platzierungsflächen und das Greifen kleiner Planeten führten zu Eingriffen.

Komfort und Motion Sickness waren dagegen kaum problematisch. Sechs Personen zeigten keine Anzeichen von Unwohlsein, eine Person leichtes Unwohlsein. In Kombination mit den positiven XR-Mehrwert-Werten spricht dies dafür, dass die grundlegende technische Strategie des Prototyps - stabile Darstellung, keine künstliche Fortbewegung und reduzierte Szenenwechsel - für den Testkontext tragfähig war.

## 2.4 Ableitungen

Aus den Ergebnissen lassen sich mehrere Ableitungen für die Forschungsfrage und die Weiterentwicklung des Prototyps formulieren.

Erstens stützen die Daten die Annahme, dass XR besonders für die Vermittlung von Maßstabsverhältnissen geeignet sein kann, wenn die Darstellung nicht nur betrachtet, sondern aktiv verändert wird. Die größten Zuwächse in der Selbsteinschätzung betrafen Größenunterschiede, Distanzen und Planetenreihenfolge. Auch die offenen Antworten verweisen wiederholt auf räumliche Anordnung, Vergleich mehrerer Planeten und Skalierungs-Controls als zentrale Verständnishilfen. Für die finale Argumentation bedeutet das: Der Mehrwert der Anwendung liegt nicht allein darin, Planeten dreidimensional zu zeigen, sondern darin, dass Nutzer:innen Maßstab, Kompression und relative Darstellung im Raum ausprobieren können.

Zweitens zeigen die Ergebnisse, dass Gamification im Prototyp grundsätzlich funktioniert, wenn die Interaktion klar genug ist. Das Reihenfolge-Minispiel war besonders erfolgreich: Es wurde von allen Personen ohne Hilfe abgeschlossen und mehrfach als motivierend genannt. Das Größen-Minispiel hatte ebenfalls fachliches Potenzial, zeigte aber stärkere Interaktionsprobleme. Daraus folgt, dass Lernspiele nicht nur inhaltlich sinnvoll konzipiert, sondern in XR besonders deutlich lesbar und greifbar sein müssen. Platzierungsflächen, Feedback und Objektgrößen sollten so gestaltet sein, dass die Spielregel nicht durch Bedienprobleme verdeckt wird.

Drittens bleibt Usability eine zentrale Voraussetzung für Lernwirkung. Die Tests zeigen, dass der Prototyp grundsätzlich bedienbar war, aber wiederholt kleine Hinweise brauchte. Besonders Menüführung, Rückwege, Slider-Trefferflächen, Platzierung weiterer Planeten und Hand-/Pinch-Interaktionen sollten vor einer finalen Version verbessert werden. Eine konsistente, sichtbare Menü-Schaltfläche zusätzlich zur Geste, größere oder tolerantere Slider-Interaktionsflächen, deutlichere Platzierungsringe und klarere Hinweise zum Platzieren mehrerer Planeten wären konkrete nächste Schritte.

Viertens sollte die wissenschaftliche Glaubwürdigkeit in einer finalen Version sichtbarer gestützt werden. Die Anwendung wurde zwar als datenbasiert und glaubwürdig wahrgenommen, mehrere Antworten deuten aber an, dass Quellen oder zusätzliche Informationen wünschenswert wären. Für eine Bildungsanwendung wäre daher sinnvoll, Planetendaten mit Quellenhinweisen, einem Info-/Enzyklopädie-Bereich oder optional einblendbaren Referenzen zu verbinden. Damit könnte die Anwendung deutlicher zeigen, welche Werte realen astronomischen Daten folgen und welche Darstellungen didaktisch skaliert oder vereinfacht sind.

Fünftens zeigt der User-Testing-Vorbereitungsmodus eine sinnvolle methodische Richtung, sollte aber künftig noch genauer dokumentiert werden. Der Modus reduziert potenziell kognitive Belastung beim Aufsetzen der Brille und trennt körperliche Eingewöhnung vom inhaltlichen Teststart. Für eine spätere Studie wäre es hilfreich, diese Phase explizit im Beobachtungsbogen zu erfassen: Wie viel Hilfe war beim Headset-Setup nötig? War die Person nach dem Passthrough-Setup orientiert? Musste die Testleitung nach dem Tutorial noch grundlegende Bedienung erklären? Dadurch ließe sich besser unterscheiden, ob Probleme aus der App, aus XR-Unerfahrenheit oder aus der initialen Gerätesituation entstehen.

Insgesamt beantworten die Ergebnisse die Forschungsfrage nicht abschließend, liefern aber deutliche Hinweise: Eine XR-Lernanwendung zum Sonnensystem kann astronomische Maßstabsverhältnisse fachlich nachvollziehbar und räumlich erfahrbar machen, wenn sie datenbasierte Inhalte, manipulierbare räumliche Modelle und klare spielerische Aufgaben kombiniert. Die zentrale Einschränkung liegt weniger in der inhaltlichen Idee als in der Präzision der Interaktionsgestaltung. Je weniger die Nutzer:innen über Controller, Menüs oder Trefferflächen nachdenken müssen, desto stärker kann sich die Aufmerksamkeit auf Größen, Distanzen, Orbits und planetare Unterschiede richten.

## 3 Fazit und Ausblick

## 3.1 Zusammenfassung der Erkenntnisse

Die vorliegende Masterarbeit hatte zum Ziel, zu untersuchen, wie sich eine immersive XR-Anwendung so gestalten lässt, dass sie wissenschaftliche Genauigkeit mit spielerischen und interaktiven Elementen verbindet und dadurch ein unterhaltsames sowie lernförderndes Erlebnis schafft. Im Mittelpunkt stand die prototypische Entwicklung von „Sonnensystem XR“, einer Anwendung für die Meta Quest 3, die das Sonnensystem räumlich erfahrbar macht und astronomische Inhalte durch Exploration, Interaktion und gamifizierte Aufgaben zugänglich macht. Die Arbeit verfolgte damit die Frage, wie eine XR-Lernanwendung gestaltet werden kann, die wissenschaftliche Genauigkeit mit spielerischer Interaktion verbindet, um Verständnis, Engagement und räumliches Lernen zu fördern.

Die theoretische Auseinandersetzung zeigte, dass XR im Bildungskontext besonders dann sinnvoll eingesetzt werden kann, wenn das Medium nicht als technischer Selbstzweck verstanden wird, sondern als Werkzeug zur aktiven Wissensvermittlung. Immersives Lernen entsteht nicht allein durch die räumliche Darstellung oder das Tragen eines Headsets, sondern durch das Zusammenspiel von Verständlichkeit, Orientierung, Präsenz, Interaktion und didaktisch begründeter Reduktion. Vor diesem Hintergrund wurde im praktischen Teil ein funktionsfähiger XR-Prototyp entwickelt, der mehrere Modi zur Erkundung des Sonnensystems kombiniert: die Betrachtung einzelner Planeten, die Platzierung und Steuerung eines Sonnensystemmodells, den Wechsel zwischen Passthrough- und VR-Ansicht sowie zwei Lernspiele zu Planetenreihenfolge und Größenverhältnissen.

Ein zentrales Ergebnis der Arbeit ist, dass wissenschaftliche Korrektheit und spielerische Vermittlung sich nicht ausschließen, sondern sich in einem gut strukturierten XR-Design gegenseitig unterstützen können. Der Prototyp arbeitet mit astronomischen Basisdaten für Planeten, Größen, Orbits und Beschreibungstexte, übersetzt diese Daten aber in didaktisch handhabbare Darstellungen. Gerade bei einem Sonnensystemmodell ist eine unveränderte 1:1-Abbildung von Planetengrößen und Distanzen für eine XR-Lernumgebung kaum sinnvoll, weil die enormen Größenunterschiede das Modell unbenutzbar machen würden. Die Arbeit zeigt daher, dass wissenschaftliche Genauigkeit in diesem Kontext nicht bedeutet, jede Relation vollständig maßstabsgetreu abzubilden, sondern reale Daten nachvollziehbar, transparent und lernförderlich zu vereinfachen.

Das User Testing stützt diese gestalterische Grundannahme. Die Evaluation wurde mit sieben Teilnehmer:innen durchgeführt und ist aufgrund der kleinen, nicht repräsentativen Stichprobe vorsichtig zu interpretieren. Dennoch zeigen die Ergebnisse deutliche Hinweise darauf, dass der Prototyp fachliches Verständnis unterstützen kann. Der durchschnittliche Wissensscore stieg von 6 von 10 Punkten im Pre-Test auf 9,14 von 10 Punkten im Post-Test; alle sieben Teilnehmer:innen verbesserten sich. Besonders die Themen Planetenreihenfolge, Größenverhältnisse, Distanzen und die Schwierigkeit maßstabsgetreuer Darstellungen wurden nach der Nutzung besser eingeordnet. Auch die Selbsteinschätzungen zu Größenunterschieden, Distanzen und Umlaufbahnen stiegen deutlich.

Neben dem Wissenszuwachs wurde auch der räumliche Mehrwert der Anwendung sichtbar. Die Teilnehmer:innen bewerteten die XR-Visualisierung überwiegend positiv und nannten vor allem das Platzieren des gesamten Sonnensystems, das Vergleichen der Planeten und das Ausprobieren der Skalierungs-Controls als hilfreiche Elemente. Damit bestätigt das Testing einen Kernpunkt der Arbeit: Der Mehrwert von XR liegt nicht nur darin, Planeten dreidimensional zu zeigen, sondern darin, dass Nutzer:innen Maßstab, Abstand, Kompression und Umlaufbewegung im Raum aktiv verändern und beobachten können. Das räumliche Modell wird dadurch nicht nur Anschauungsmaterial, sondern ein manipulierbares Lernwerkzeug.

Auch die gamifizierten Elemente erwiesen sich grundsätzlich als tragfähig. Besonders das Reihenfolge-Minispiel wurde von allen Teilnehmer:innen ohne Hilfestellung abgeschlossen und in den offenen Antworten mehrfach als motivierender Teil der Anwendung genannt. Das Größen-Minispiel wurde ebenfalls abgeschlossen, zeigte jedoch mehr Interaktionsprobleme, vor allem bei kleinen Planetenobjekten und der Sichtbarkeit der Platzierungsringe. Daraus ergibt sich eine wichtige Erkenntnis: Gamification funktioniert in XR nicht unabhängig von der Interaktionsqualität. Eine Lernspielidee kann fachlich sinnvoll und motivierend sein, wird aber nur dann ihr didaktisches Potenzial entfalten, wenn Greifen, Platzieren, Feedback und räumliche Lesbarkeit ausreichend klar gestaltet sind.

Der iterative Entwicklungsprozess ist ebenfalls als wesentliches Ergebnis zu verstehen. Die Arbeit zeigt, dass ein XR-Projekt dieser Art nicht linear entsteht, sondern durch fortlaufendes Testen, Reduzieren, Neuordnen und Stabilisieren. Architekturentscheidungen wie die Single-Scene-Struktur, die Nutzung des Meta XR SDK, die Orientierung an SDK-Samples, die vorbereiteten World-Space-UI-Prefabs und die Auslagerung planetenspezifischer Inhalte in `PlanetData` waren keine bloßen technischen Details. Sie waren Voraussetzungen dafür, dass der Prototyp als Lernanwendung überhaupt sinnvoll erlebbar wurde. Besonders in XR ist technische Stabilität damit nicht nur eine Qualitätsfrage, sondern eine didaktische Grundbedingung.

Als besonders zukunftsfähig erwies sich die datengetriebene Struktur des Projekts. Planetendaten, Detailtexte, visuelle Prefab-Referenzen und Spielregeln sind so angelegt, dass neue Inhalte nicht tief in der Programmlogik hardcodiert werden müssen. Dadurch kann der Prototyp vergleichsweise leicht erweitert werden: um weitere Himmelskörper wie Monde oder Pluto, um zusätzliche Informationsmodule, um weitere Lernspiele oder langfristig auch um größere astronomische Themenfelder wie Galaxien, Nebel, Exoplaneten oder schwarze Löcher. Für solche Erweiterungen müsste das Datenmodell je nach Objektklasse angepasst werden, der grundlegende Ansatz bleibt aber tragfähig: wissenschaftliche Inhalte werden als Daten gepflegt, während UI, Interaktion und Lernlogik möglichst generisch darauf reagieren.

## 3.2 Kritische Reflexion und Limitationen

Die kritische Reflexion des Projekts betrachtet vor allem die Stellen, an denen der realisierte Prototyp hinter dem konzeptionell möglichen Umfang zurückbleibt. Diese Grenzen sind für die Masterarbeit relevant, weil die finale Anwendung aus mehreren Reduktionen, Tool-Entscheidungen und technischen Abwägungen entstanden ist.

Ein früher Einschnitt war der Abbruch des ursprünglichen Unreal-Ansatzes. Zu Beginn wurde das Projekt in Unreal Engine angedacht bzw. begonnen, weil Unreal für visuelle Qualität, Echtzeit-Rendering und XR grundsätzlich sehr attraktiv wirkt. In der konkreten Umsetzung zeigte sich jedoch schnell, dass die geplante Anwendung mit vielen Zuständen, Datenstrukturen, UI-Flows, Interaktionen und Lernspielregeln in Blueprints zu unübersichtlich geworden wäre. Für ein Masterarbeitsprojekt mit begrenzter Zeit und ohne tiefere Unreal-Erfahrung wäre es riskant gewesen, zentrale Logik wie Planetenverwaltung, Menüführung, Platzierung, Gamification und späteres User Testing hauptsächlich visuell in Blueprint-Netzwerken aufzubauen. Der Wechsel zu Unity war deshalb eine pragmatische Entscheidung: C#-Skripte, ScriptableObjects, die Meta-XR-Beispiele und ein überschaubarerer Iterationsprozess passten besser zum benötigten Prototyping.

Eine zweite große Grenze lag in den Versuchen mit MRUK und Scene Understanding. Die Idee, reale Raumflächen semantisch zu erkennen und das Sonnensystem stabil im Raum zu platzieren, war für Mixed Reality sehr naheliegend. In der Praxis erwies sich der MRUK-Stack jedoch als schwerer integrierbar als erwartet. Unterschiedliche Meta-XR-SDK-Generationen, veraltete Komponenten wie `OVRSceneManager` und `OVRSceneAnchor`, geänderte APIs und die Verbindung aus Room-Setup, Anchors, Raycasts, Passthrough und eigener State Machine führten zu wiederholten Umbauten. Dadurch entstand eine wichtige Projekterkenntnis: Für die konkrete Anforderung, ein Objekt auf einer geeigneten Boden- oder Tischfläche zu platzieren, war semantisches Room Understanding zu schwergewichtig. Die spätere Reduktion auf die Depth API war funktional begrenzter, aber stabiler und besser mit dem Ziel eines testbaren Prototyps vereinbar.

Bestimmte Mixed-Reality-Ideen blieben dadurch außerhalb des realisierten Umfangs. Der Prototyp nutzt Passthrough und räumliche Platzierung, arbeitet aber ohne umfassende semantische Raumlogik, ohne gezielte Unterscheidung von Möbeln, Wänden oder Decken und ohne stärker raumadaptive Lernmomente. Die MR-Komponente erfüllt damit vor allem die Funktion einer stabilen räumlichen Einbettung. Eine deutlich stärker auf Raumverständnis ausgerichtete Anwendung hätte den Rahmen des Prototyps überschritten.

Auch das User Interface ist trotz funktionsfähigem Stand noch nicht abgeschlossen. Das User Testing zeigte, dass mehrere Bedienhandlungen zusätzliche Hinweise benötigten: Menüaufruf und Rückwege, das Platzieren weiterer Objekte, die Skalierungs-Controls, Slider-Trefferflächen, Hand-/Pinch-Interaktionen und die Sichtbarkeit der Platzierungsringe im Größen-Minispiel. Das gesammelte Feedback konnte im Rahmen der Arbeit noch nicht vollständig in eine weitere UI-Iteration übersetzt werden.

Das Tutorial ist ebenfalls nur ein erster Schritt. Aktuell basiert es vor allem auf kurzen Video-Loops und einfachen Step-Prefabs. Diese Lösung erleichtert das Onboarding, bleibt aber im Vergleich zu den Möglichkeiten einer räumlichen XR-Anwendung relativ passiv.

Der Umfang der Gamification bleibt begrenzt. Die beiden umgesetzten Lernspiele decken wichtige Grundlagen ab, vor allem Reihenfolge und Größenverhältnisse. Andere gamifizierte Elemente wie sichtbare Fortschrittsbalken, freischaltbare Badges, Medaillen oder ein stärker ausgearbeitetes Belohnungssystem werden in der Arbeit zwar konzeptionell aufgegriffen, sind im aktuellen Prototyp aber noch nicht als zusammenhängendes Motivationssystem umgesetzt. Gamification ist damit vor allem an einzelne Aufgaben und ihren Abschluss gekoppelt.

Auch inhaltlich bleibt der spielerische Umfang begrenzt. Weitere Aspekte wie Gravitation, Temperaturen, Atmosphären, Monde, Rotationsachsen, Jahreszeiten, Lichtlaufzeiten oder Missionswissen werden bisher nicht spielerisch erschlossen. Diese Eingrenzung hielt den Prototyp testbar und verhinderte technische Überladung, verkleinert aber den fachlichen Spielraum der Lernspiele.

Eine weitere konzeptionelle Limitation ist das aktuell noch fehlende Storytelling. Der Prototyp bietet Exploration, Information, Interaktion und Lernspiele, aber keine übergreifende narrative Rahmung. Dadurch fehlt eine erzählerische Klammer, die die einzelnen Modi stärker zu einer zusammenhängenden Experience verbinden könnte.

Hinzu kommt die notwendige Vereinfachung astronomischer Realität. Der Prototyp orientiert sich an realen Daten, kann das Sonnensystem aber nicht vollständig physikalisch exakt abbilden. Die Unterschiede zwischen Planetenradien, Distanzen, Umlaufzeiten und visueller Wahrnehmbarkeit sind so groß, dass eine vollständig maßstabsgetreue Darstellung in einer begehbaren XR-Umgebung kaum didaktisch sinnvoll wäre. Die Anwendung bewegt sich daher im Spannungsfeld zwischen wissenschaftlicher Genauigkeit und nutzerfreundlicher Visualisierung. Diese Spannung muss transparent werden: Nutzer:innen sollten nachvollziehen können, welche Werte realen Daten entsprechen und welche Parameter aus didaktischen Gründen skaliert oder komprimiert werden.

Auch die wissenschaftliche Glaubwürdigkeit ist noch ausbaufähig. Die Teilnehmer:innen bewerteten die Anwendung in dieser Hinsicht positiv, wünschten sich teilweise aber zusätzliche Quellen oder weiterführende Informationen. Der Prototyp arbeitet zwar datenbasiert, macht die Herkunft dieser Daten und den Unterschied zwischen astronomischem Fakt, didaktischem Modell und interaktiver Vereinfachung bisher nur begrenzt sichtbar.

Methodisch bleibt außerdem zu beachten, dass die Evaluation keine repräsentative Wirksamkeitsstudie ersetzt. Die Stichprobe umfasste sieben Personen und war als Gelegenheitsstichprobe angelegt. Sie erlaubt daher keine allgemeinen Aussagen über alle potenziellen Zielgruppen, Altersgruppen oder Bildungskontexte. Zudem fehlte eine Kontrollgruppe, etwa eine Vergleichsgruppe mit klassischem 2D-Lernmaterial. Der beobachtete Wissenszuwachs ist deshalb als explorativer Hinweis zu verstehen und nicht als endgültiger Nachweis einer überlegenen Lernwirkung von XR.

## 3.3 Potenziale für Weiterentwicklung und Forschung

Für die Weiterentwicklung der Anwendung ergeben sich mehrere Perspektiven. Zunächst sollten die im User Testing sichtbar gewordenen Usability-Probleme gezielt überarbeitet werden. Dazu gehören klarere Rückwege ins Hauptmenü, größere oder fehlertolerantere Slider, eindeutigere Beschriftungen der Skalierungs-Controls, sichtbarere Platzierungsringe, besser greifbare kleine Planetenobjekte und ein stärker geführtes Tutorial für zentrale XR-Handlungen. Besonders wichtig wäre, die Bedienung so weit zu vereinfachen, dass Nutzer:innen ihre Aufmerksamkeit auf Größen, Distanzen, Orbits und Lernaufgaben richten können.

Für die Mixed-Reality-Platzierung wäre eine erneute Beschäftigung mit MRUK bzw. semantischem Scene Understanding spannend. Eine zukünftige Version könnte die Skalierung und Positionierung der Planeten stärker an erkannte Oberflächen anpassen. Sitzt eine Person am Schreibtisch, könnte das System automatisch eine kompaktere, sauber lesbare Anordnung erzeugen; steht eine Person frei im Raum, könnte dieselbe Experience großzügiger skaliert werden. Dadurch hätte ein:e Spieler:in am Tisch eine ähnlich polierte Erfahrung wie jemand, der das Sonnensystem auf einer größeren freien Fläche platziert.

Das Tutorial könnte stärker als echte XR-Anleitung gestaltet werden. Eine ausgereiftere Lernanwendung könnte Interaktionen direkt im Raum vormachen, zum Beispiel durch 3D-Animationen, Ghost-Hands, animierte Controller-Hinweise, hervorgehobene Greifpunkte oder kurze spielinterne Übungsaufgaben. Dadurch würden Greifen, Platzieren, Menüaufruf und Slider-Bedienung als räumliche Handlung im Medium selbst eingeübt.

Auch das Motivationssystem könnte deutlich ausgebaut werden. Eine spätere Version könnte stärker mit langfristigem Fortschritt arbeiten: Nutzer:innen könnten Lernbereiche abschließen, Erfolge sammeln, Planetenkategorien freischalten oder über wiederholte Aufgaben ihren Kompetenzzuwachs sichtbar machen. Fortschrittsbalken, Badges und Medaillen würden an konkrete Lernhandlungen und wiedererkennbare Meilensteine gebunden.

Für eine finale Bildungsanwendung wäre außerdem sinnvoll, Datenquellen sichtbarer zu machen. Denkbar wären Quellenhinweise im Info-Panel, optionale Detailansichten oder ein kurzes Glossar zu Maßstab, Umlaufbahn, Durchmesser und Distanz. Dadurch könnte die Anwendung stärker zwischen astronomischem Fakt, didaktischem Modell und interaktiver Vereinfachung unterscheiden.

Ein weiterer Entwicklungsschritt wäre eine stärkere narrative Rahmung der gesamten Experience. Denkbar wäre zum Beispiel eine Forschungsreise durch das Sonnensystem, eine Ausbildung als Planetolog:in, eine Mission zur Rekonstruktion eines maßstabsgerechten Modells oder eine geführte Entdeckungstour, bei der neue Inhalte und Auszeichnungen narrativ begründet werden. Storytelling könnte damit Gamification und Wissensvermittlung verbinden: Fortschritt, Badges oder Medaillen wären Teil einer Lernreise statt abstrakte Belohnungen.

Methodisch wäre eine größere Evaluation mit Kontroll- oder Vergleichsgruppe ein nächster Schritt. Interessant wäre etwa ein Vergleich zwischen XR-Prototyp, klassischer 2D-Visualisierung und text-/bildbasiertem Lernmaterial. So ließe sich genauer prüfen, ob der beobachtete Wissenszuwachs spezifisch mit der räumlichen Interaktion zusammenhängt oder vor allem aus der wiederholten Beschäftigung mit den Inhalten entsteht. Zusätzlich wären verzögerte Post-Tests sinnvoll, um nicht nur kurzfristiges Erinnern, sondern nachhaltigeres Verständnis zu untersuchen.

Auf inhaltlicher Ebene bietet der Prototyp eine gute Grundlage für zusätzliche Lernmodule. Naheliegend wären weitere Himmelskörper wie Monde, Zwergplaneten oder Asteroiden sowie vertiefende Module zu Atmosphären, Gravitation, Temperaturen, Rotationsachsen, Licht und Schatten oder Raumfahrtmissionen. Auch komplexere astronomische Themen wie Exoplaneten, Galaxien, schwarze Löcher oder Sternentwicklung könnten langfristig ergänzt werden. Diese Themen würden jedoch nicht nur zusätzliche Assets benötigen, sondern teilweise neue Datenmodelle, neue Skalierungslogiken.

Besonders vielversprechend ist die Erweiterbarkeit des bestehenden Systems. Durch die Verwendung von `PlanetData`, generischen UI-Bindings, zentraler State Machine und modularen Lernspiel-Prefabs ist die Anwendung nicht auf eine feste Anzahl einzeln programmierter Inhalte beschränkt. Neue Planeteninformationen, zusätzliche Himmelskörper oder weitere Lernspiele können prinzipiell ergänzt werden, ohne die Grundarchitektur neu aufzubauen. Für die Masterarbeit ist das relevant, weil der Prototyp damit nicht nur als einmalige Demo funktioniert, sondern als Grundlage für ein erweiterbares XR-Lernsystem.

Außerdem wäre es spannend, unterschiedliche Vermittlungsformen innerhalb von XR zu vergleichen. Untersucht werden könnte, ob stärker diegetische Interfaces im Bildungskontext zu besserer Orientierung führen als klassische Panels, ob exploratives Lernen langfristig besser wirkt als geführte Aufgaben oder welche Form von Feedback in XR-Lernspielen am motivierendsten ist. Auch die Rolle von Gamification verdient weitere Untersuchung: Welche Mechaniken fördern Neugier und Wiederholung, ohne die wissenschaftliche Tiefe zu schwächen? Wie wirken Fortschrittsbalken, Badges, Medaillen oder freischaltbare Inhalte auf Motivation und Lernverhalten? Welche Rolle spielt Storytelling dabei, komplexe astronomische Zusammenhänge besser zu erinnern und einzuordnen?

## Offene Arbeitsnotizen

- TODO: Kapitelstruktur an die finale Gliederung der Word-Datei anpassen.
- TODO: Relevante Punkte aus `DOKU.md` und `ARCHIV.md` für den Prozessbericht übernehmen.
- TODO: Stellen markieren, an denen auf den bereits geschriebenen theoretischen Hintergrund verwiesen wird.
