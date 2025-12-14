# ProgettoDiGruppo_GPOI

Applicazione WPF (C# 7.3, .NET Framework 4.7.2) per risolvere problemi di trasporto con i metodi `Nord-Ovest` e `Minimi Costi`.

## Requisiti
- Windows
- Visual Studio 2022
- .NET Framework 4.7.2

## Avvio rapido
1. Clona il repository.
2. Apri la soluzione in Visual Studio.
3. Esegui Build.
4. Avvia con Start Debugging o Start Without Debugging.

## Utilizzo
- Inserisci numero di `righe` (UP) e `colonne` (D) nelle textbox.
- Clicca `Crea Tabella` per generare la griglia.
- Compila i `costi` nelle celle, `Offerta` per ciascuna riga e `Domanda` nell’ultima riga, oppure usa `Autofill`.
- Verifica che Σ Offerte = Σ Domande (problema bilanciato).
- Avvia `Metodo Nord-Ovest` o `Metodo Minimi Costi`.
- Leggi il log dettagliato nel pannello di destra e il costo totale (mostrato nell’ultima riga, colonna `Offerta`).

## Funzionalità
- Generazione dinamica di colonne D1..Dn e riga `Domanda` con `ExpandoObject`.
- `Autofill` con dati bilanciati e range adattivo per le offerte.
- Log dei passi di calcolo per entrambi gli algoritmi.
- Aggiornamento risultati nella `DataGrid`.

## Struttura UI
- `TopGrid`: comandi.
- `leftGrid`: input righe/colonne.
- `mainGrid`: tabella costi, offerte, domande.
- `rightPanel` e `txtCalcoli`: log calcoli.

## Note di sviluppo
- Validazioni input con `Regex` (solo numeri).
- Gli algoritmi richiedono dati numerici validi e problema bilanciato.
- Target progetto: WPF su .NET Framework 4.7.2, C# 7.3.

## Compilazione da terminale
- Apri "Developer Command Prompt for VS".
- Esegui:
  - `msbuild ProgettoDiGruppo_GPOI\ProgettoDiGruppo_GPOI.csproj`

## Contributi
- Apri una issue o una pull request.
- Mantieni lo stile del codice e aggiungi test dove opportuno.
