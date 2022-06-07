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
4. Main.py der Visualisierung ausführen (in JAZG/Visualization, NICHT die offizielle)
5. Rider Projekt ausführen

# Eigene Logik implementieren

In dem Projekt gibt es eine Programmierschnittstelle für die eigene Implementierung der Menschenlogik.
Dafür können alle Hilfsmethoden der Klasse Player und der Klasse Human genutzt werden.

1. In die Klassendatei "CustomHuman" gehen
2. Klasse "PleaseRenameYourHuman" umbenennen
3. Tick-Methode mit eigener Logik füllen
4. Dein Agent im "Program.cs" zur Model Description unter dem entsprechenden TODO!!! hinzufügen
5. Dein Agent im "Fieldlayer" unter dem entsprechenden TODO in der initLayer-Methode
5. Dein Agent in der "config.json"-Datei gemäß den anderen Agenten eintragen

Deine Agenten werden in der Visualisierug anhand ihrer roten T-shirts erkennbar sein.


# Authors 

Jasper Wolny, Viviam Ribeiro, Ahmad Abbas, Nils Martens

Images from https://www.vecteezy.com
