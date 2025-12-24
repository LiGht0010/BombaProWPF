# 📋 BL to Invoice (Facturation) - Complete Architecture Plan

## 🎯 Overview

This document outlines the complete structure for the **Bon de Livraison (BL) to Facture** invoicing workflow across the API and MAUI applications.

---

## 📁 API Project Structure (`BombaProMaxApi`)

### 1. Models (`/Models`)

| File | Purpose | Key Properties |
|------|---------|----------------|
| `BonLivraison.cs` | ✅ Delivery note header | ID, NumeroBL, DateBL, ClientID, MontantTotal, EstFacture, Notes, audit fields |
| `BonLivraisonDetails.cs` | ✅ Delivery note line items | ID, BonLivraisonID, ProduitID, ServiceID, Quantite, PrixUnitaire, MontantLigne |
| `FactureBonLivraison.cs` | ✅ Junction table (Facture ↔ BL) | ID, FactureID, BonLivraisonID, DateAssociation |
| `Facture.cs` | ✅ Invoice header | ID, NumeroFacture, DateFacture, ClientID, MontantTotal, Statut, + nav to FactureBonLivraisons |
| `ElementsFacture.cs` | ✅ Invoice line items | ID, FactureID, ProduitID, ServiceID, Quantite, PrixUnitaire |

### 2. DTOs (`/DTOs`)

#### BonLivraison DTOs
| File | Classes | Purpose |
|------|---------|---------|
| `BonLivraisonDto.cs` | `BonLivraisonDto` | Full BL with display fields (ClientNom, ClientNumero) + Details list |
| | `CreateBonLivraisonDto` | Create new BL with nested details |
| | `UpdateBonLivraisonDto` | Update existing BL with nested details |
| `BonLivraisonDetailsDto.cs` | `BonLivraisonDetailsDto` | Detail line with display fields (ProduitNom, ServiceNom) |
| | `CreateBonLivraisonDetailsDto` | Create detail line (no ID, no BonLivraisonID) |

#### Facture DTOs
| File | Classes | Purpose |
|------|---------|---------|
| `FactureDto.cs` | `FactureDto` | Full facture with display fields (ClientNom, MoyenPaiementNom) |
| `ElementsFactureDto.cs` | `ElementsFactureDto` | Element line with display fields (ProduitNom, ServiceNom) |

#### Facturation DTOs
| File | Classes | Purpose |
|------|---------|---------|
| `FactureBonLivraisonDto.cs` | `FactureBonLivraisonDto` | Junction record with display fields |
| | `CreateFactureFromBLsDto` | **Request**: List of BL IDs to invoice |
| | `FacturationResultDto` | **Response**: Success, FactureID, NumeroFacture, MontantTotal, Errors |

### 3. AutoMapper Profiles (`/Mappig`)

| File | Mappings |
|------|----------|
| `BonLivraisonProfile.cs` | BonLivraison ↔ BonLivraisonDto, CreateBonLivraisonDto → BonLivraison |
| `BonLivraisonDetailsProfile.cs` | BonLivraisonDetails ↔ BonLivraisonDetailsDto, auto-calc MontantLigne |
| `FactureBonLivraisonProfile.cs` | FactureBonLivraison ↔ FactureBonLivraisonDto with nested display fields |
| `FactureProfile.cs` | ✅ Already exists - Facture ↔ FactureDto |
| `ElementsFactureProfile.cs` | ✅ Already exists - ElementsFacture ↔ ElementsFactureDto |

### 4. Controllers (`/Controllers`)

#### BonLivraisonsController
```
GET    /api/BonLivraisons                      → List<BonLivraisonDto>
GET    /api/BonLivraisons/{id}                 → BonLivraisonDto
GET    /api/BonLivraisons/numero/{numeroBL}    → BonLivraisonDto
GET    /api/BonLivraisons/client/{clientId}    → List<BonLivraisonDto>
GET    /api/BonLivraisons/non-factures         → List<BonLivraisonDto>  ⭐ Key for facturation
GET    /api/BonLivraisons/non-factures/client/{clientId} → List<BonLivraisonDto>
GET    /api/BonLivraisons/date-range?start=&end= → List<BonLivraisonDto>
GET    /api/BonLivraisons/next-numero          → string
POST   /api/BonLivraisons                      → BonLivraisonDto (CreateBonLivraisonDto)
PUT    /api/BonLivraisons/{id}                 → NoContent (UpdateBonLivraisonDto)
DELETE /api/BonLivraisons/{id}                 → NoContent (blocks if EstFacture=true)
```

#### BonLivraisonDetailsController
```
GET    /api/BonLivraisonDetails                → List<BonLivraisonDetailsDto>
GET    /api/BonLivraisonDetails/{id}           → BonLivraisonDetailsDto
GET    /api/BonLivraisonDetails/bonlivraison/{blId} → List<BonLivraisonDetailsDto>
GET    /api/BonLivraisonDetails/produit/{produitId} → List<BonLivraisonDetailsDto>
GET    /api/BonLivraisonDetails/service/{serviceId} → List<BonLivraisonDetailsDto>
POST   /api/BonLivraisonDetails                → BonLivraisonDetailsDto
POST   /api/BonLivraisonDetails/batch          → List<BonLivraisonDetailsDto>
PUT    /api/BonLivraisonDetails/{id}           → NoContent
DELETE /api/BonLivraisonDetails/{id}           → NoContent (blocks if parent EstFacture=true)
DELETE /api/BonLivraisonDetails/bonlivraison/{blId} → NoContent
```

#### FactureBonLivraisonController (⭐ Core Invoicing)
```
GET    /api/FactureBonLivraison                → List<FactureBonLivraisonDto>
GET    /api/FactureBonLivraison/{id}           → FactureBonLivraisonDto
GET    /api/FactureBonLivraison/facture/{factureId} → List<FactureBonLivraisonDto>
GET    /api/FactureBonLivraison/facture/{factureId}/bls → List<BonLivraisonDto>
GET    /api/FactureBonLivraison/bonlivraison/{blId} → FactureBonLivraisonDto
GET    /api/FactureBonLivraison/next-numero-facture → string
POST   /api/FactureBonLivraison/from-bls       → FacturationResultDto ⭐⭐⭐ CORE ENDPOINT
```

#### FacturesController
```
GET    /api/Factures                           → List<FactureDto>
GET    /api/Factures/{id}                      → FactureDto
GET    /api/Factures/{id}/details              → FactureDto (with elements + BLs)
GET    /api/Factures/numero/{numero}           → FactureDto
GET    /api/Factures/client/{clientId}         → List<FactureDto>
GET    /api/Factures/statut/{statut}           → List<FactureDto>
GET    /api/Factures/date-range?start=&end=    → List<FactureDto>
GET    /api/Factures/next-numero               → string
POST   /api/Factures                           → FactureDto
PUT    /api/Factures/{id}                      → NoContent
PUT    /api/Factures/{id}/statut               → NoContent
DELETE /api/Factures/{id}                      → NoContent (unlocks BLs: EstFacture=false)
```

#### ElementsFactureController
```
GET    /api/ElementsFacture                    → List<ElementsFactureDto>
GET    /api/ElementsFacture/{id}               → ElementsFactureDto
GET    /api/ElementsFacture/facture/{factureId} → List<ElementsFactureDto>
GET    /api/ElementsFacture/produit/{produitId} → List<ElementsFactureDto>
GET    /api/ElementsFacture/service/{serviceId} → List<ElementsFactureDto>
POST   /api/ElementsFacture                    → ElementsFactureDto
POST   /api/ElementsFacture/batch              → List<ElementsFactureDto>
PUT    /api/ElementsFacture/{id}               → NoContent
DELETE /api/ElementsFacture/{id}               → NoContent
DELETE /api/ElementsFacture/facture/{factureId} → NoContent
```

---

## 📱 MAUI Project Structure (`BombaProMax`)

### 1. DTOs/Models (`/Models`)

| File | Classes | Notes |
|------|---------|-------|
| `BonLivraisonDto.cs` | `BonLivraisonDto` | Mirror API + `IsSelected` for UI multi-select |
| | `CreateBonLivraisonDto` | Mirror API |
| | `UpdateBonLivraisonDto` | Mirror API |
| `BonLivraisonDetailsDto.cs` | `BonLivraisonDetailsDto` | Mirror API + `DisplayName` computed property |
| | `CreateBonLivraisonDetailsDto` | Mirror API |
| `FactureBonLivraisonDto.cs` | `FactureBonLivraisonDto` | Mirror API |
| | `CreateFactureFromBLsDto` | Mirror API |
| | `FacturationResultDto` | Mirror API |
| `FactureDto.cs` | `FactureDto` | ✅ Already exists |
| `ElementsFactureDto.cs` | `ElementsFactureDto` | ✅ Already exists |

### 2. Services (`/Services`)

#### BonLivraisonService.cs

```csharp
public class BonLivraisonService
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;

    // Constructor with HttpClient injection

    // ═══════════════════════════════════════════════════════════════
    // BON LIVRAISON CRUD
    // ═══════════════════════════════════════════════════════════════
    
    Task<List<BonLivraisonDto>> GetAllAsync()
    Task<BonLivraisonDto?> GetByIdAsync(int id)
    Task<BonLivraisonDto?> GetByNumeroAsync(string numeroBL)
    Task<List<BonLivraisonDto>> GetByClientAsync(int clientId)
    Task<List<BonLivraisonDto>> GetNonFacturesAsync()                    // ⭐ For facturation UI
    Task<List<BonLivraisonDto>> GetNonFacturesByClientAsync(int clientId) // ⭐ For facturation UI
    Task<List<BonLivraisonDto>> GetByDateRangeAsync(DateOnly start, DateOnly end)
    Task<string> GetNextNumeroAsync()
    Task<BonLivraisonDto?> CreateAsync(CreateBonLivraisonDto dto)
    Task<bool> UpdateAsync(UpdateBonLivraisonDto dto)
    Task<bool> DeleteAsync(int id)

    // ═══════════════════════════════════════════════════════════════
    // BON LIVRAISON DETAILS (nested operations)
    // ═══════════════════════════════════════════════════════════════
    
    Task<List<BonLivraisonDetailsDto>> GetDetailsByBLAsync(int bonLivraisonId)
    Task<BonLivraisonDetailsDto?> AddDetailAsync(BonLivraisonDetailsDto dto)
    Task<List<BonLivraisonDetailsDto>> AddDetailsBatchAsync(List<BonLivraisonDetailsDto> dtos)
    Task<bool> UpdateDetailAsync(BonLivraisonDetailsDto dto)
    Task<bool> DeleteDetailAsync(int detailId)
    Task<bool> DeleteAllDetailsAsync(int bonLivraisonId)

    // ═══════════════════════════════════════════════════════════════
    // FACTURATION (invoicing from BLs)
    // ═══════════════════════════════════════════════════════════════
    
    Task<FacturationResultDto> CreateFactureFromBLsAsync(CreateFactureFromBLsDto request) // ⭐⭐⭐
    Task<string> GetNextNumeroFactureAsync()
    Task<List<BonLivraisonDto>> GetBLsByFactureAsync(int factureId)
}
```

#### FactureService.cs

```csharp
public class FactureService
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;

    // Constructor with HttpClient injection

    // ═══════════════════════════════════════════════════════════════
    // FACTURE CRUD
    // ═══════════════════════════════════════════════════════════════
    
    Task<List<FactureDto>> GetAllAsync()
    Task<FactureDto?> GetByIdAsync(int id)
    Task<FactureDto?> GetByIdWithDetailsAsync(int id)  // Includes elements + linked BLs
    Task<FactureDto?> GetByNumeroAsync(string numeroFacture)
    Task<List<FactureDto>> GetByClientAsync(int clientId)
    Task<List<FactureDto>> GetByStatutAsync(string statut)
    Task<List<FactureDto>> GetByDateRangeAsync(DateOnly start, DateOnly end)
    Task<string> GetNextNumeroAsync()
    Task<FactureDto?> CreateAsync(FactureDto dto)
    Task<bool> UpdateAsync(FactureDto dto)
    Task<bool> UpdateStatutAsync(int id, string statut)
    Task<bool> DeleteAsync(int id)  // Also unlocks linked BLs

    // ═══════════════════════════════════════════════════════════════
    // ELEMENTS FACTURE (nested operations)
    // ═══════════════════════════════════════════════════════════════
    
    Task<List<ElementsFactureDto>> GetElementsByFactureAsync(int factureId)
    Task<ElementsFactureDto?> AddElementAsync(ElementsFactureDto dto)
    Task<List<ElementsFactureDto>> AddElementsBatchAsync(List<ElementsFactureDto> dtos)
    Task<bool> UpdateElementAsync(ElementsFactureDto dto)
    Task<bool> DeleteElementAsync(int elementId)
    Task<bool> DeleteAllElementsAsync(int factureId)

    // ═══════════════════════════════════════════════════════════════
    // LINKED BLS (via FactureBonLivraison)
    // ═══════════════════════════════════════════════════════════════
    
    Task<List<FactureBonLivraisonDto>> GetLinkedBLsAsync(int factureId)
    Task<List<BonLivraisonDto>> GetFullLinkedBLsAsync(int factureId)
}
```

---

## 🔄 Core Invoicing Flow

### Request: `CreateFactureFromBLsDto`
```json
{
  "BonLivraisonIds": [1, 2, 3],
  "DateFacture": "2025-01-15",
  "CreatedByUserId": 5
}
```

### Process (in `FactureBonLivraisonController.CreateFactureFromBLs`):
```
1. BEGIN TRANSACTION
2. Load BLs where ID in list AND EstFacture = false
3. VALIDATE: All BLs found? Same ClientID?
4. Generate NumeroFacture (FAC-2025-00001)
5. Create Facture record
6. FOR EACH BL:
   - Create FactureBonLivraison junction record
   - Copy BonLivraisonDetails → ElementsFacture
   - Set BL.EstFacture = true
7. Calculate Facture.MontantTotal = SUM(BL.MontantTotal)
8. COMMIT TRANSACTION
9. Return FacturationResultDto
```

### Response: `FacturationResultDto`
```json
{
  "Success": true,
  "Message": "Facture FAC-2025-00001 créée avec succès.",
  "FactureID": 42,
  "NumeroFacture": "FAC-2025-00001",
  "MontantTotal": 15000.00,
  "BLsFactures": 3,
  "Errors": null
}
```

---

## 🖥️ MAUI UI Flow (Future Implementation)

### Page: `FacturationPage.xaml`

```
┌─────────────────────────────────────────────────────────────────┐
│  Facturation des Bons de Livraison                              │
├─────────────────────────────────────────────────────────────────┤
│  Client: [Picker: Select Client ▼]                              │
├─────────────────────────────────────────────────────────────────┤
│  BLs Non Facturés:                                              │
│  ┌─────────────────────────────────────────────────────────────┐│
│  │ ☑ BL-2025-00001  │ 15/01/2025 │ 5,000.00 DH                ││
│  │ ☑ BL-2025-00002  │ 16/01/2025 │ 3,500.00 DH                ││
│  │ ☐ BL-2025-00003  │ 17/01/2025 │ 2,000.00 DH                ││
│  └─────────────────────────────────────────────────────────────┘│
├─────────────────────────────────────────────────────────────────┤
│  Sélectionnés: 2 BLs    │    Total: 8,500.00 DH                 │
├─────────────────────────────────────────────────────────────────┤
│  [    Facturer les BL sélectionnés    ]                         │
└─────────────────────────────────────────────────────────────────┘
```

### ViewModel: `FacturationViewModel.cs`

```csharp
public partial class FacturationViewModel : ObservableObject
{
    // Services
    private readonly BonLivraisonService _blService;
    private readonly ClientService _clientService;

    // Observable Properties
    [ObservableProperty] ObservableCollection<ClientDto> clients;
    [ObservableProperty] ClientDto? selectedClient;
    [ObservableProperty] ObservableCollection<BonLivraisonDto> availableBLs;
    [ObservableProperty] decimal totalSelected;
    [ObservableProperty] int countSelected;
    [ObservableProperty] bool isLoading;
    [ObservableProperty] bool canFacturer;

    // Commands
    [RelayCommand] async Task LoadClientsAsync()
    [RelayCommand] async Task LoadBLsForClientAsync()
    [RelayCommand] async Task FacturerSelectedBLsAsync()
    [RelayCommand] void ToggleBLSelection(BonLivraisonDto bl)
    [RelayCommand] void SelectAllBLs()
    [RelayCommand] void DeselectAllBLs()

    // Computed
    void RecalculateSelection()
    {
        var selected = AvailableBLs.Where(bl => bl.IsSelected).ToList();
        CountSelected = selected.Count;
        TotalSelected = selected.Sum(bl => bl.MontantTotal);
        CanFacturer = CountSelected > 0;
    }
}
```

---

## 📊 Database Tables (PostgreSQL)

```sql
-- New Tables
CREATE TABLE "BonsLivraison" (
    "ID" SERIAL PRIMARY KEY,
    "NumeroBL" VARCHAR(20) NOT NULL UNIQUE,
    "DateBL" DATE NOT NULL,
    "ClientID" INT NOT NULL REFERENCES "Clients"("ID"),
    "MontantTotal" DECIMAL(10,2) NOT NULL,
    "EstFacture" BOOLEAN DEFAULT FALSE,
    "Notes" VARCHAR(255),
    "AjoutePar" INT,
    "DateCreation" TIMESTAMP,
    "ModifiePar" INT,
    "DateModification" TIMESTAMP
);

CREATE TABLE "BonLivraisonDetails" (
    "ID" SERIAL PRIMARY KEY,
    "BonLivraisonID" INT NOT NULL REFERENCES "BonsLivraison"("ID") ON DELETE CASCADE,
    "ProduitID" INT REFERENCES "Produits"("ID"),
    "ServiceID" INT REFERENCES "Services"("ID"),
    "Quantite" INT NOT NULL,
    "PrixUnitaire" DECIMAL(10,2) NOT NULL,
    "MontantLigne" DECIMAL(10,2) NOT NULL,
    "Description" VARCHAR(255)
);

CREATE TABLE "FactureBonLivraisons" (
    "ID" SERIAL PRIMARY KEY,
    "FactureID" INT NOT NULL REFERENCES "Factures"("ID") ON DELETE CASCADE,
    "BonLivraisonID" INT NOT NULL UNIQUE REFERENCES "BonsLivraison"("ID"),
    "DateAssociation" TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Existing table modifications
ALTER TABLE "Factures" ADD COLUMN IF NOT EXISTS navigation to FactureBonLivraisons;
```

---

## ✅ Implementation Checklist

### API Side
- [x] Models: BonLivraison, BonLivraisonDetails, FactureBonLivraison
- [x] Navigation properties in Facture, Client, Produit, Service
- [x] DTOs: All BL and Facturation DTOs
- [x] AutoMapper Profiles
- [x] DbContext: DbSets + Entity configurations
- [x] BonLivraisonsController
- [x] BonLivraisonDetailsController
- [x] FactureBonLivraisonController (with from-bls endpoint)
- [x] FacturesController (updated)
- [x] ElementsFactureController (updated)
- [ ] EF Core Migration

### MAUI Side
- [x] DTOs mirroring API
- [ ] BonLivraisonService implementation
- [ ] FactureService implementation
- [ ] Register services in MauiProgram.cs
- [ ] FacturationViewModel
- [ ] FacturationPage.xaml
- [ ] Navigation integration

---

## 🔑 Key Business Rules Enforced

| Rule | Where Enforced |
|------|----------------|
| BL can only be invoiced once | `FactureBonLivraison.BonLivraisonID` UNIQUE constraint |
| All BLs must belong to same client | `CreateFactureFromBLs` validation |
| Invoiced BLs cannot be modified | `BonLivraisonsController.Put/Delete` checks `EstFacture` |
| Invoiced BLs cannot be deleted | `BonLivraisonsController.Delete` checks `EstFacture` |
| Deleting Facture unlocks BLs | `FacturesController.Delete` sets `EstFacture = false` |
| Invoice elements auto-copied from BL details | `CreateFactureFromBLs` copies details |
| Totals auto-calculated | Controllers recalculate on add/update/delete |

---

## 📝 Notes

- All API endpoints use DTOs (not raw entities) for security
- AutoMapper handles entity ↔ DTO conversions
- Transactions ensure data consistency for invoicing
- `AsNoTracking()` used for read-only queries (performance)
- Navigation properties loaded with `Include`/`ThenInclude`
- Audit fields (AjoutePar, DateCreation, ModifiePar, DateModification) tracked

---

## 🚀 Next Steps

1. Create EF Core migration for new tables
2. Implement MAUI services (BonLivraisonService, FactureService)
3. Register services in MauiProgram.cs
4. Create FacturationViewModel with MVVM Toolkit
5. Build FacturationPage.xaml UI
6. Integrate navigation and test end-to-end

---

*Document created: January 2025*
*Project: BombaProMax ERP System*
