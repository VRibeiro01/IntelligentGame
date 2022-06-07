# JAZG

Ein cooles, simples Zombie-Spiel, das man nicht wirklich spielen kann, weil die Spieler intelligenter sind als du.

Das Ziel der Menschen ist es, möglichst viele Zombies zu töten und möglichst lange zu überleben.
Wenn ein Mensch gefressen wird, wird er selber zum Zombie.

Das Ziel der Zombies ist es, so viele Menschen wie möglich zu fressen, sonst sterben sie.
Wenn alle Zombies tot sind, fängt das neue Level an und es werden weitere Zombies ins Leben geweckt.


# Installation & Ausfürung

1. Requirements der Visualierung installieren (pip install -r requirements.txt)
2. C# Projekt in Rider öffnen
3. Nuget Packet von Mars installieren
4. Rider Projekt ausführen

Beim Ausführen des Projekts wird die Visualisierung automatisch gestartet. Wenn das nicht gewünscht ist, kann die Programmzeile in "Program.cs" auskommentiert werden. Die betroffene Zeile ist im Projekt gekennzeichnet.

# Eigene Logik implementieren

In dem Projekt gibt es eine Programmierschnittstelle für die eigene Implementierung der Menschenlogik.
Dafür können alle Hilfsmethoden der Klasse Player und der Klasse Human genutzt werden.

1. In die Klassendatei "CustomHuman" gehen
2. Klasse "PleaseRenameYourHuman" umbenennen
3. Tick-Methode mit eigener Logik füllen
4. Dein Agent im "Program.cs" zur Model Description unter dem entsprechenden TODO!!! hinzufügen
5. Dein Agent im "Fieldlayer" unter dem entsprechenden TODO in der initLayer-Methode
5. Dein Agent in der "config.json"-Datei gemäß den anderen Agenten eintragen

Deine Agenten werden in der Visualisierung anhand ihrer roten T-shirts erkennbar sein.

# Feld
Das Feld ist 100x100 groß. Das Feld ist von Wänden umgeben, sodass die Spieler nicht aus dem Feld hinauslaufen können.

# Spieler und Items

#### Zombies
Zombies sind die Gegner der Menschen. Sie verlieren Energie während der Ticks und sterben, wenn ihre Energie auf 0 ist.
Sie bewegen sich immer zu den Menschen hin, wenn welche in der Nähe sind. Wenn sie nah genug sind, fressen sie den nächsten Menschen. Damit wird ihren Energiewert erhöht.

#### Menschen
Das Ziel der Menschen ist, möglichst lange zu überleben und Zombies zu töten. Dafür dürfen sie nicht von Zombies gefressen werden.
Sie können weglaufen und können Waffen einsetzen. Um Waffen einzusetzen müssen diese erstmal gesammelt werden.
Die gesammelten Waffen werden in der Liste "weapons" festgehalten.

Es stehen folgende Waffen zur Verfügung:
##### Gun
Gun trifft einen Zombie mit 50% Wahrscheinlichkeit und entnimmt dem getroffenen Zombie 15 Energiepunkte.
Ein Gun kann beliebig von einem Menschen abgeschossen werden, aber es hat nur alle 8 Ticks einen Effekt.

##### M16
Eine M16 trifft einem Zombie mit 30% Wahrscheinlichkeit und entnimmt dem getroffenen Zombie 30 Energiepunkte.
Eine M16 kann beliebig von einem Menschen abgeschossen werden, aber es hat nur alle 5 Ticks einen Effekt.

# Hilfsmethoden aus Player und Human

#### RandomMove()

Der Agent bewegt sich um ein Feld in eine zufällige Richtung

#### GetDistanceFromPlayer(Player other), GetDistanceFromItem(Item item), GetDirectionFromPlayer(Player other), GetDirectionFromItem(Item item)

Liefert die Entfernung/Richtung zwischen dem aufrufenden Agenten und eines anderen Spielers(Menschen und Zombies sind Spieler),
Liefert die Entfernung/Richtung zwischen dem aufrufenden Agenten und eines Items(Wall, Food, Gun, M16 sind Items).


#### FindClosestWeapon()
Liefert eine Referenz auf die nächste Waffe, die sich in einem Umkreis von 20 Feldern befindet. Liefert null, wenn keine Waffe in der Nähe ist.

#### FindClosestZombie()
Liefert eine Referenz auf den nächsten Zombie, der sich in einem Umkreis von 20 Feldern befindet. Liefert null, wenn kein Zombie in der Nähe ist.

#### FindZombies()
Liefert eine Liste von Zombies, die sich in einem Umkreis von 20 Feldern befinden. Die Liste ist leer, wenn sich keine Zombies in der Umgebung befinden.

#### UseWeapon(Zombie zombie)

Wenn der Mensch eine M16 eingesammelt hat, wird diese Waffe einsetzt, ansonsten wird Gun eingesetzt.
Wenn der Mensch noch keine Waffen einsammelt hat, liefert die Methode false.

#### CollectItem(Item item)
Der Mensch bewegt sich in die Richtung von item (z.B. Gun). 
Sobald der Mensch mit dem Item "kollidiert", wird es eingesammelt.

#### RunFromZombies(Player closestZombie)
Der Mensch bewegt sich um zwei Felder weg von allen Zombies um sich herum (soweit möglich).


# Authors 

Jasper Wolny, Viviam Ribeiro, Ahmad Abbas, Nils Martens

Images from https://www.vecteezy.com
