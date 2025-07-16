# Projekt ShelfKeeper

## 📌 Zielbild der Anwendung

* Verwaltung einer **persönlichen Medienbibliothek** auf einem zentralen Server.
* Medien sind **physisch vorhanden (z. B. Bücher, DVDs, CDs, Schallplatten)**.
  Die App speichert nur deren Metadaten (Titel, Autor, Erscheinungsjahr, etc.), evtl. ein Cover.
* Zugriff über verschiedene Geräte (Handy, Tablet, Web-App, Desktop).
* Mehrere Nutzer können jeweils **komplett unabhängige Bibliotheken** haben.
* Optional langfristig Social Features wie Empfehlungen, Freundeslisten oder das Teilen einzelner Medienlisten.

---

## Use Cases

### 🚀 Unbedingt (Minimum Viable Product / MVP)

Damit du von Anfang an **geringste Hürden beim Hinzufügen** hast und eine geräteübergreifende Nutzung möglich ist.

#### 🔑 Nutzerverwaltung

* Konto erstellen (E-Mail, Passwort)
* Login & Logout
* Passwort-Reset („Passwort vergessen“)
* Passwort ändern
* Account löschen
* Administratorzugang, um Nutzer global zu verwalten

#### 📚 Medienverwaltung (Metadaten)

* Medienobjekt anlegen (manuell): Titel, Typ (Buch, CD etc.), Autor/Künstler, Jahr, Notizen
* Medienobjekt ansehen (Detailansicht)
* Medienobjekt bearbeiten & löschen
* Medien in einer Listenansicht sehen
* Suchfeld (z. B. nach Titel, Autor)
* Einfache Filter (z. B. nur Typ=Buch)

#### 📷 Barcode-Scan (schon MVP)

* Scan eines Barcodes (EAN/ISBN)
* App fragt externe Datenbank (z. B. Google Books API, Open Library, oder UPC-DB) ab und füllt Felder automatisch
* Nutzer kann Ergebnisse prüfen / bearbeiten

#### ☁️ Synchron & deviceübergreifend

* Server speichert alles, Clients holen Daten via API.
* Änderungen sofort synchron (z. B. via API mit HTTP Polling oder WebSockets / SSE).

#### 🔐 Sicherheit & Privacy

* Jeder Benutzer sieht **nur seine eigene Bibliothek**.
* Datenbank ist mandantenfähig (UserID → Medien).

---

### 🟡 Mittelfristig (Komfort-Features)

Was deine Anwendung deutlich besser und nutzerfreundlicher macht.

#### 🏷️ Erweiterte Medienorganisation

* Tags / eigene Kategorien
* Mehrere Coverbilder (z. B. Front, Spine, Back)
* Sortierung (z. B. nach Jahr, zuletzt hinzugefügt)

#### 📚 Stapel-Funktionen

* Mehrere Medien gleichzeitig bearbeiten (z. B. Tag hinzufügen)
* CSV / JSON-Import (z. B. von Excel-Listen oder anderen Bibliotheksapps)
* CSV-Export (zur Sicherung / offline Bearbeitung)

#### ✏️ Persönliche Notizen

* Eigene Kommentare pro Medium
* „Gesehen am“ / „Gelesen am“-Datum

#### 🔍 Verbesserte Suche

* Suche in Notizen, Genres, Erscheinungsjahr
* Vorschläge während der Eingabe

#### 🔒 Sicherheit

* Zwei-Faktor-Authentifizierung
* Geräteverwaltung (Sessions ausloggen)

#### 📊 Erste Statistiken

* „Wie viele Bücher habe ich?“
* „Wie viele davon Science Fiction?“

---

### 🟢 Langfristig (Social & Power-User-Features)

Hier hebst du die Anwendung auf ein neues Level.

#### 👥 Social Features

* Freunde hinzufügen
* Bibliotheken (oder Teile davon) teilen (nur Leserechte)
* Gemeinsame Listen (z. B. „Unsere Familien-DVDs“)
* Empfehlungen:

  * „Peter hat 5 neue Bücher hinzugefügt“
  * „Freunde empfehlen dir Science Fiction“

#### 💬 Kommunikation

* Likes & Kommentare auf geteilte Listen
* Nachrichten- oder Chat-Funktion (optional)

#### ⭐ Öffentliche Profile (optional)

* Nutzer kann Profil + ausgewählte Listen öffentlich sichtbar machen
* Andere können Profil besuchen, stöbern und abonnieren

#### 📈 Umfangreiche Statistiken

* „Meist hinzugefügte Autoren“
* Zeitachsen-Graphen: „Wann hast du was hinzugefügt?“

#### 📂 Erweiterte Integrationen & Automationen

* Webhooks / Zapier-Schnittstelle:
  „Wenn Buch hinzugefügt → trage in Google Sheet ein“
* API für Dritte (z. B. SmartHome, eigene Dashboards)

#### 🧠 Empfehlungen per Machine Learning

* „Aufgrund deiner Bibliothek empfehlen wir dir…“

### Monetarisierung via Abonnement

Die Anwendung soll monetarisiert werden durch ein Abomodell, das:

* gewisse Features nur in bezahlten Plänen freischaltet.
* in einem kostenlosen Plan eingeschränkt nutzbar ist (Freemium).
* Jede API-Route hat eine FeatureCheck-Middleware.
* Subscription-Änderungen triggern Background-Tasks: z. B. bei Downgrade auf Free → prüfen ob Medienlimit verletzt → User warnen.

#### Abo abschließen

* Nutzer wählt Plan (z. B. Plus), zahlt über Zahlungsanbieter (Stripe, PayPal etc.) und wird freigeschaltet.

#### Abo ansehen / verwalten

* Nutzer sieht aktuellen Plan, Ablaufdatum, Auto-Renew-Status.

#### Abo kündigen

* Nutzer beendet sein Abo, erhält ggf. bis Periodenende Zugriff.

#### Upgrade / Downgrade

* Nutzer wechselt Plan (z. B. Plus → Premium).

#### Zahlung fehlgeschlagen / Mahnungen

* System sperrt Features, erinnert Nutzer.

#### Feature-Gates prüfen

* Anwendung entscheidet pro Feature, ob erlaubt (z. B. Shared Lists nur Premium).

---

## Datenquellen für Medien-Metadaten

* Google Books API (ISBN) → Bücher
* Open Library
* UPCitemDB → DVDs, CDs

---

## 🚀 Erklärung der wichtigsten Entities

* Alle Daten sind **mandantenfähig**:
  Jede Tabelle, die vom User abhängt, hat `UserId` oder indirekt über `MediaId` die Verbindung.
* So sind alle Bibliotheken sauber getrennt.
* Verwendung von Standard-SQL um DB-agnostisch zu bleiben
* keine Nutzung von ON DELETE CASCADE
* Ergänzung aller Entites um die Metadaten `created_at`, `last_updated_at` und Pflege dieser Werte mittels Triggern
* Composite UNIQUE Constraints, damit z. B. ein Benutzer nicht zwei Tags oder Locations mit gleichem Namen hat
* Indizes auf alle relevanten UserId-Felder, um Abfragen wie „alle Medien eines Users“ schnell zu machen
* zusätzliche Erstellung praktischer Views, z. B.:
  * alle Medien inkl. Standort & Tags
  * alle SharedLists inkl. Likes & Kommentare
* Nutzung von Seed-Daten (mind. jeweils 10 Datensätze), damit du direkt Beispielnutzer, Medien, Tags etc. hast, um deine Anwendung & Abfragen zu testen.

### 👤 Users

* `UserId`: eindeutige ID
* `Email`, `PasswordHash`, `Name`, `CreatedAt`
* Nur dieser User sieht seine Medien.
* Hat ein Subscription (1:n, wobei meistens nur 1 aktiv), und eigene Locations, MediaItems, Tags.

### 💰 Subscriptions

* Verknüpft UserId, speichert aktuellen Plan, Start- & Endzeit, AutoRenew etc.
* Erlaubt dir Feature-Checks gegen Plan („nur Premium darf X ausführen“).

### 📚 MediaItems

* Gehören immer zu einem User (über `UserId`).
* Attribute:

  * `Title`, `Author/Artist`, `Type` (Buch, CD, DVD etc.), `Year`
  * `ISBN/UPC` (wichtig für Barcode-Scan & Lookup)
  * `Notes` (persönliche Notizen)
  * `Progress` (z. B. Seite 123, String)
  * `AddedAt`
  * `LocationId` (FK)
  * optional Borrower (wenn Social Features „verliehen an“ kommen)
  * Kann mehrere MediaImages haben.
  * `Author` sollte eine eigene einfache Entität (`Name`) sein, um Doppelungen oder ungleiche Schreibweisen zu vermeiden.

---

### 🏷️ MediaTags & MediaItemTags

* Tags sind global pro User, z. B. „SciFi“, „Lieblingsfilme“.
* Viele-zu-viele zwischen `MediaItems` und `MediaTags` via `MediaItemTags`.

---

### 🖼️ MediaImages

* Z. B. Front-, Spine-, Back-Cover.
* Könnte URL (bei S3 o.ä.) oder Binary Blob sein.
* Mehrere Bilder pro MediaItem möglich.

---

### Location (Standort)

* Jeder Benutzer hat seine eigenen Standorte („Wohnzimmer Regal 3“, „Kellerbox 2“).
* MediaItems zeigen darauf via LocationId.
* in welchem Regal / Zimmer / Lagerplatz sich das Medium befindet
* `Titel` (Pflicht)
* `Beschreibung` (optional)

---

### 👥 Friends

* Für Social Features (optional).
* Bidirektionale Freundschaft (`UserId`, `FriendId`).

---

### 📂 SharedLists

* User kann Listen teilen („Meine Lieblingsbücher“).
* Andere können sie anschauen, liken, kommentieren.
* Geplant für langfristig.

---

### ❤️ Likes & 💬 Comments

* `Likes` zeigt, wer was geliked hat.
* `Comments` erlaubt Kommentare zu SharedLists.

---

## Infrastruktur

* Nutzuung von Docker
* Erstellung einer vollständig einsatzbereite docker-compose.yml, die folgendes bereitstellt:
  * eine PostgreSQL-Instanz (Version 16) mit deinem Schema
  * plus Adminer (als DB-Admin-Oberfläche im Browser)
  * automatischer Import von Seed- und Schema-SQL, sodass beim Hochfahren des Containers alles bereitgestellt wird.

## Systemarchitektur

* ASP.NET Core Web API (klassisch, ohne Minimal API)
* Clean Architecture Pattern mit
  * Domain Layer (Geschäftslogik) → komplett Framework-unabhängig
  * Application Layer (Use Cases / Services) → orchestriert Prozesse, enthält Ports
  * Infrastructure Layer (Postgres, Stripe etc.) → Implementierung der Schnittstellen
  * Web API Layer (Controller, HTTP) → nur Kommunikation.
* Implementiere alle use cases
* Integration von Logging, Error Handling und Instrumentation und zwar sauber getrennt, sodass du das später jederzeit austauschen kannst (nutze Serilog und OpenTelemetry).
* Nutze eine Global Middleware zur Fehlerbehandlung (in Controllern keine try-catch mehr erforderlich) und Request Logging / Timing (Performance messen, Logging für Start/End jeder Anfrage)
* Bereitstellung von OpenTelemetry Export (Prometheus / Jaeger)
* 100% Docker-ready
* nutze das Rate Limiting Middleware-Framework von ASP.NET Core und zwar inkl.:
  * globale Policies (pro IP, Fixed Window → 10 Requests/30 Sek pro Client)
  * feiner anpassbar (pro Endpoint / pro Route / pro Benutzer später)
  * Logging, wenn Rate Limiting greift
  * Fehlerausgabe: 429 Too Many Requests
* Swagger UI
* Health Checks unter /health
* Prometheus Metriken unter /metrics
* API-Versioning über den URL-Pfad wie `/api/v1/...`
* verwende kein `var`

## Qualitätssicherung

* Unit Tests für die Domain- und Application-Layer.
* Integration Tests für den Infrastructure-Layer (z.B. Datenbankzugriffe).
* End-to-End (E2E) Tests für die API-Endpunkte.

## Sonstige Aufgaben

* ein einfaches Prometheus + Grafana Dashboard hinzufügen
* Beispiele für Alerts (429 Rate Limit Überschreitung) liefern
* Beispiel-Frontend (React/Vue) für die Use Cases des MVP hinzufügen
