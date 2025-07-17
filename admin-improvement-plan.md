# Umsetzungsplan für Verbesserung 9.7: Detaillierte Admin-Funktionen

## Übersicht
Dieser Plan beschreibt die notwendigen Schritte zur Implementierung erweiterter Admin-Funktionen für das ShelfKeeper-System, insbesondere für Systemmetriken, -gesundheit und komplette Nutzerverwaltungsfunktionen.

## Ziele
1. Entwicklung eines Admin-Dashboards für Systemmetriken und -gesundheit
2. Vervollständigung der Admin-Funktionen für die Nutzerverwaltung
3. Implementierung der Passwortänderung durch Administratoren

## Meilensteine und Aufgaben

### Phase 1: Vorbereitung und Design (Geschätzte Zeit: 1 Woche)

#### 1.1 Anforderungsanalyse
- [ ] Detaillierte Anforderungen für Admin-Dashboard definieren
- [ ] Nutzerverwaltungs-Funktionen spezifizieren
- [ ] Berechtigungsmodell für Admin-Aktionen überprüfen und verfeinern

#### 1.2 Architektur und Design
- [ ] Dashboard-Layout und UI-Mockups erstellen
- [ ] API-Endpunkte für neue Admin-Funktionen definieren
- [ ] Datenzugriffsschicht für Systemmetriken planen

### Phase 2: Backend-Implementierung (Geschätzte Zeit: 2 Wochen)

#### 2.1 Admin-Controller erweitern
- [ ] Bestehenden AdminController um neue Endpunkte erweitern
- [ ] Methode zum Ändern von Benutzerpasswörtern hinzufügen:
```csharp
[HttpPut("users/{id}/password")]
[Authorize(Policy = "AdminOnly")]
public async Task<IActionResult> ChangeUserPassword(Guid id, [FromBody] AdminChangePasswordCommand command)
{
    if (id != command.UserId)
    {
        return BadRequest();
    }
    
    OperationResult result = await _adminUserService.ChangeUserPasswordAsAdminAsync(command, CancellationToken.None);
    
    if (result.IsFailure)
    {
        return BadRequest(result.Errors);
    }
    
    return NoContent();
}
```

#### 2.2 Service-Schicht erweitern
- [ ] IAdminUserService um Passwort-Änderungsmethode erweitern:
```csharp
public interface IAdminUserService
{
    // Bestehende Methoden...
    Task<OperationResult> ChangeUserPasswordAsAdminAsync(AdminChangePasswordCommand command, CancellationToken cancellationToken);
}
```
- [ ] Service-Implementierung:
```csharp
public async Task<OperationResult> ChangeUserPasswordAsAdminAsync(AdminChangePasswordCommand command, CancellationToken cancellationToken)
{
    User user = await _dbContext.Users.FindAsync(command.UserId);
    
    if (user == null)
    {
        return OperationResult.Failure("User not found", OperationErrorType.NotFoundError);
    }
    
    user.PasswordHash = _passwordHasher.HashPassword(command.NewPassword);
    await _dbContext.SaveChangesAsync(cancellationToken);
    
    return OperationResult.Success();
}
```

#### 2.3 System-Metriken sammeln
- [ ] System-Gesundheitsindikatoren identifizieren (CPU, Speicher, DB-Verbindungen)
- [ ] Health-Check-System erweitern mit detaillierten Metriken
- [ ] Speicherung von Metrik-Historien für Trend-Analysen

#### 2.4 API für System-Monitoring erweitern
- [ ] Endpunkte für Systemmetriken implementieren
- [ ] Endpunkt für aktive Benutzer-Sessions erstellen
- [ ] Ratenbegrenzungsdaten und -verletzungen abrufbar machen

### Phase 3: Frontend-Implementierung (Geschätzte Zeit: 2 Wochen)

#### 3.1 Admin-Dashboard
- [ ] Admin-Dashboard-Komponenten erstellen (Übersicht, Detailansichten)
- [ ] Grafische Darstellung von Systemmetriken mit Recharts oder D3.js
- [ ] Filter- und Suchfunktionen für Logs und Metriken

#### 3.2 Erweiterte Nutzerverwaltung
- [ ] UI für Passwortzurücksetzung durch Administratoren
- [ ] Batch-Aktionen für mehrere Nutzer (Status ändern, Tags hinzufügen)
- [ ] Detailansicht von Nutzeraktivitäten

### Phase 4: Tests und Dokumentation (Geschätzte Zeit: 1 Woche)

#### 4.1 Tests
- [ ] Unit-Tests für neue Admin-Service-Methoden
- [ ] Integrationstests für Admin-API-Endpunkte
- [ ] UI-Tests für Admin-Dashboard-Funktionalität

#### 4.2 Dokumentation
- [ ] API-Dokumentation aktualisieren
- [ ] Admin-Handbuch erstellen
- [ ] Sicherheitsrichtlinien für Admin-Funktionen dokumentieren

### Phase 5: Deployment und Validierung (Geschätzte Zeit: 3 Tage)

- [ ] Staging-Deployment und Validierung
- [ ] Produktions-Deployment
- [ ] Überwachung nach Deployment

## Risiken und Abhängigkeiten

### Risiken
1. Sicherheitsrisiken bei erweiterten Admin-Befugnissen
   - Maßnahme: Detaillierte Protokollierung aller Admin-Aktionen
   - Maßnahme: Granulare Berechtigungen statt pauschaler Admin-Zugriff

2. Performance-Probleme bei umfangreicher Metrik-Sammlung
   - Maßnahme: Sampling-Strategie für Metriken
   - Maßnahme: Separate Datenbank für Monitoring-Daten

### Abhängigkeiten
1. Vorhandene Authentifizierungs- und Autorisierungsinfrastruktur
2. Prometheus/Grafana-Integration für Metriken-Visualisierung
3. Rate-Limiting-Middleware für entsprechende Daten

## Ressourcen

### Personal
- 1 Backend-Entwickler (Vollzeit)
- 1 Frontend-Entwickler (Vollzeit)
- 1 QA-Engineer (Teilzeit)

### Geschätzter Gesamtaufwand
- Entwicklungszeit: 6 Wochen
- Personalaufwand: 11 Personenwochen

## Erfolgskriterien
1. Administratoren können Benutzerpasswörter sicher zurücksetzen
2. Admin-Dashboard zeigt Echtzeit-Systemmetriken und -gesundheit
3. Alle Admin-Aktionen werden protokolliert und sind nachvollziehbar
4. System unterstützt präventive Wartung durch frühzeitige Problemerkennung
