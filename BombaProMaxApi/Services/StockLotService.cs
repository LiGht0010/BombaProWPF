using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using BombaProMaxApi.Data;
using BombaProMaxApi.DTOs;
using BombaProMaxApi.Models;

namespace BombaProMaxApi.Services;

/// <summary>
/// FIFO stock lot consumption service.
/// Handles stock deduction during Periode creation and stock creation from purchases.
/// </summary>
public class StockLotService : IStockLotService
{
    private readonly AppDbContext _context;
    private readonly ILogger<StockLotService> _logger;

    public StockLotService(AppDbContext context, ILogger<StockLotService> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<bool> ConsumeAsync(int produitId, int reservoirId, decimal quantite, int periodeDetailId)
    {
        if (quantite <= 0)
        {
            _logger.LogWarning("ConsumeAsync called with zero or negative quantity: {Quantite}", quantite);
            return true; // Nothing to consume
        }

        _logger.LogInformation(
            "Consuming {Quantite}L from Reservoir {ReservoirId} for PeriodeDetail {PeriodeDetailId}",
            quantite, reservoirId, periodeDetailId);

        // ?????????????????????????????????????????????????????????????????
        // FIFO CONSUMPTION: Order by DateEntree, then by ID for deterministic tiebreaker
        // This ensures consistent ordering even when multiple lots have the same date.
        // ?????????????????????????????????????????????????????????????????
        var availableLots = await _context.StockLots
            .Where(s => s.ReservoirID == reservoirId 
                     && s.ProduitID == produitId
                     && s.Statut == "Disponible"
                     && s.QuantiteDisponible > 0)
            .OrderBy(s => s.DateEntree)
            .ThenBy(s => s.ID)  // Deterministic tiebreaker for same-date lots
            .ToListAsync();

        if (!availableLots.Any())
        {
            _logger.LogError(
                "No available stock lots for Reservoir {ReservoirId}, Produit {ProduitId}",
                reservoirId, produitId);
            throw new InvalidOperationException(
                $"Stock insuffisant: Aucun lot disponible pour le réservoir {reservoirId}");
        }

        var totalAvailable = availableLots.Sum(l => l.QuantiteDisponible);
        if (totalAvailable < quantite)
        {
            _logger.LogError(
                "Insufficient stock: Required {Required}L, Available {Available}L for Reservoir {ReservoirId}",
                quantite, totalAvailable, reservoirId);
            throw new InvalidOperationException(
                $"Stock insuffisant: Requis {quantite:F3}L, Disponible {totalAvailable:F3}L");
        }

        var remainingToConsume = quantite;
        var consumptionDate = DateTime.UtcNow;

        // ?????????????????????????????????????????????????????????????????
        // ATOMIC CONSUMPTION: All consumptions and lot updates are tracked together.
        // SaveChanges is called by the caller to ensure atomic transaction.
        // If any consumption fails mid-loop, the entire batch should be rolled back.
        // ?????????????????????????????????????????????????????????????????
        foreach (var lot in availableLots)
        {
            if (remainingToConsume <= 0)
                break;

            var toConsumeFromLot = Math.Min(lot.QuantiteDisponible, remainingToConsume);

            // Skip if nothing meaningful to consume (prevents constraint violation)
            if (toConsumeFromLot <= 0)
                continue;

            // Create consumption record (audit trail)
            var consumption = new StockLotConsumption
            {
                StockLotID = lot.ID,
                PeriodeDetailID = periodeDetailId,
                QuantiteConsommee = toConsumeFromLot,
                PrixUnitaire = lot.PrixAchat,
                DateConsommation = consumptionDate
            };
            _context.StockLotConsumptions.Add(consumption);

            // Update lot
            lot.QuantiteDisponible -= toConsumeFromLot;
            if (lot.QuantiteDisponible <= 0)
            {
                lot.Statut = "Épuisé";
                _logger.LogInformation("StockLot {LotId} exhausted", lot.ID);
            }

            remainingToConsume -= toConsumeFromLot;

            _logger.LogDebug(
                "Consumed {Consumed}L from StockLot {LotId}, Remaining in lot: {Remaining}L",
                toConsumeFromLot, lot.ID, lot.QuantiteDisponible);
        }

        // Update reservoir level
        var reservoir = await _context.Reservoirs.FindAsync(reservoirId);
        if (reservoir != null)
        {
            reservoir.NiveauDeCarburant -= quantite;
            if (reservoir.NiveauDeCarburant < 0)
            {
                _logger.LogWarning(
                    "Reservoir {ReservoirId} level went negative: {Level}L",
                    reservoirId, reservoir.NiveauDeCarburant);
            }
        }

        _logger.LogInformation(
            "Successfully consumed {Quantite}L from Reservoir {ReservoirId}",
            quantite, reservoirId);

        return true;
    }

    /// <inheritdoc />
    /// <remarks>
    /// TRANSACTION SCOPE: This method modifies StockLots and optionally Reservoir.NiveauDeCarburant.
    /// The caller is responsible for calling SaveChangesAsync() to commit all changes atomically.
    /// If the caller's transaction fails after this method, all changes will be rolled back.
    /// </remarks>
    public async Task<bool> ReverseConsumptionAsync(int periodeDetailId)
    {
        _logger.LogInformation("Reversing consumption for PeriodeDetail {PeriodeDetailId}", periodeDetailId);

        // ?????????????????????????????????????????????????????????????????
        // ATOMIC REVERSAL: All consumption records for this PeriodeDetail 
        // are restored together. SaveChanges is called by the caller.
        // ?????????????????????????????????????????????????????????????????
        var consumptions = await _context.StockLotConsumptions
            .Include(c => c.StockLot)
            .Where(c => c.PeriodeDetailID == periodeDetailId)
            .ToListAsync();

        if (!consumptions.Any())
        {
            _logger.LogWarning("No consumptions found for PeriodeDetail {PeriodeDetailId}", periodeDetailId);
            return true; // Nothing to reverse
        }

        decimal totalRestored = 0;
        int? reservoirId = null;

        foreach (var consumption in consumptions)
        {
            var stockLot = consumption.StockLot;
            if (stockLot == null)
            {
                _logger.LogWarning("StockLot not found for consumption {ConsumptionId}", consumption.ID);
                continue;
            }

            // Restore quantity to the stock lot
            stockLot.QuantiteDisponible += consumption.QuantiteConsommee;
            
            // If lot was exhausted, make it available again
            if (stockLot.Statut == "Épuisé" && stockLot.QuantiteDisponible > 0)
            {
                stockLot.Statut = "Disponible";
            }

            totalRestored += consumption.QuantiteConsommee;
            reservoirId = stockLot.ReservoirID;

            _logger.LogDebug(
                "Restored {Quantite}L to StockLot {LotId}, New available: {Available}L",
                consumption.QuantiteConsommee, stockLot.ID, stockLot.QuantiteDisponible);
        }

        // Update reservoir level
        if (reservoirId.HasValue && totalRestored > 0)
        {
            var reservoir = await _context.Reservoirs.FindAsync(reservoirId.Value);
            if (reservoir != null)
            {
                reservoir.NiveauDeCarburant += totalRestored;
                _logger.LogInformation(
                    "Restored {Quantite}L to Reservoir {ReservoirId}, New level: {Level}L",
                    totalRestored, reservoirId.Value, reservoir.NiveauDeCarburant);
            }
        }

        // Delete the consumption records
        _context.StockLotConsumptions.RemoveRange(consumptions);

        _logger.LogInformation(
            "Successfully reversed {Quantite}L for PeriodeDetail {PeriodeDetailId}",
            totalRestored, periodeDetailId);

        return true;
    }

    /// <inheritdoc />
    /// <remarks>
    /// TRANSACTION SCOPE: This method creates a StockLot and updates Reservoir.NiveauDeCarburant.
    /// The caller is responsible for calling SaveChangesAsync() to commit all changes atomically.
    /// Typically called from AchatAllocation creation within the same transaction.
    /// </remarks>
    public async Task CreateStockLotAsync(
        int achatId, 
        int reservoirId, 
        int produitId, 
        decimal quantite, 
        decimal prixAchat)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(quantite, nameof(quantite));

        var stockLot = new StockLot
        {
            Type = StockLotType.Purchase,
            AchatID = achatId,
            ReservoirID = reservoirId,
            ProduitID = produitId,
            QuantiteInitiale = quantite,
            QuantiteDisponible = quantite,
            PrixAchat = prixAchat,
            DateEntree = DateTime.UtcNow,
            Statut = "Disponible"
        };

        // Validate domain rules
        var errors = stockLot.Validate().ToList();
        if (errors.Count > 0)
        {
            throw new InvalidOperationException($"Invalid StockLot: {string.Join(", ", errors)}");
        }

        _context.StockLots.Add(stockLot);

        // Also update reservoir level
        var reservoir = await _context.Reservoirs.FindAsync(reservoirId);
        if (reservoir != null)
        {
            reservoir.NiveauDeCarburant += quantite;
            _logger.LogInformation(
                "Updated Reservoir {ReservoirId} level to {Level}L after adding StockLot",
                reservoirId, reservoir.NiveauDeCarburant);
        }

        _logger.LogInformation(
            "Created StockLot (Purchase): Achat {AchatId}, Reservoir {ReservoirId}, Quantite {Quantite}L, Prix {Prix}",
            achatId, reservoirId, quantite, prixAchat);
    }

    // ?????????????????????????????????????????????????????????????????
    // OPENING BALANCE OPERATIONS
    // ?????????????????????????????????????????????????????????????????

    /// <inheritdoc />
    /// <remarks>
    /// TRANSACTION SCOPE: This method creates a StockLot and updates Reservoir.
    /// Unlike CreateStockLotAsync, this method calls SaveChangesAsync() internally
    /// because it's typically called from API endpoints as a standalone operation.
    /// The partial unique index IX_StockLots_ReservoirID_OpeningBalance_Unique
    /// enforces that only one OpeningBalance can exist per reservoir at the DB level.
    /// </remarks>
    public async Task<int> CreateOpeningBalanceAsync(
        int reservoirId,
        int produitId,
        decimal quantite,
        decimal prixAchat,
        DateTime? dateEntree = null,
        string? notes = null)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(quantite, nameof(quantite));
        ArgumentOutOfRangeException.ThrowIfNegative(prixAchat, nameof(prixAchat));

        // Validate reservoir exists
        var reservoir = await _context.Reservoirs.FindAsync(reservoirId);
        if (reservoir == null)
        {
            throw new InvalidOperationException($"Reservoir {reservoirId} not found");
        }

        // ?????????????????????????????????????????????????????????????????
        // RACE CONDITION DEFENSE: Check for existing stock lots before creating.
        // The partial unique index provides DB-level protection, but this 
        // application-level check provides better error messages.
        // In case of race condition, the DB constraint will reject the insert.
        // ?????????????????????????????????????????????????????????????????
        if (await HasAnyStockLotsAsync(reservoirId))
        {
            throw new InvalidOperationException(
                $"Le réservoir {reservoir.Numero} possède déjà des lots de stock. " +
                "Le stock initial ne peut être créé que pour un réservoir vide.");
        }

        // Validate capacity
        if (quantite > reservoir.Capacite)
        {
            throw new InvalidOperationException(
                $"La quantité ({quantite:N2}L) dépasse la capacité du réservoir ({reservoir.Capacite:N2}L)");
        }

        // Validate product matches reservoir (if reservoir has assigned product)
        if (reservoir.ProduitID.HasValue && reservoir.ProduitID.Value != produitId)
        {
            throw new InvalidOperationException(
                $"Le produit ne correspond pas au type de carburant assigné au réservoir");
        }

        var stockLot = new StockLot
        {
            Type = StockLotType.OpeningBalance,
            AchatID = null, // No purchase for opening balance
            ReservoirID = reservoirId,
            ProduitID = produitId,
            QuantiteInitiale = quantite,
            QuantiteDisponible = quantite,
            PrixAchat = prixAchat,
            DateEntree = dateEntree ?? DateTime.UtcNow,
            Statut = "Disponible",
            Notes = notes ?? "Stock initial lors de l'installation du système"
        };

        // Validate domain rules
        var errors = stockLot.Validate().ToList();
        if (errors.Count > 0)
        {
            throw new InvalidOperationException($"Invalid StockLot: {string.Join(", ", errors)}");
        }

        _context.StockLots.Add(stockLot);

        // Update reservoir level (this is the ONLY place NiveauDeCarburant is set for opening balance)
        reservoir.NiveauDeCarburant = quantite;

        // If reservoir doesn't have a product assigned, assign it now
        if (!reservoir.ProduitID.HasValue)
        {
            reservoir.ProduitID = produitId;
            _logger.LogInformation(
                "Assigned product {ProduitId} to Reservoir {ReservoirId}",
                produitId, reservoirId);
        }

        // ?????????????????????????????????????????????????????????????????
        // ATOMIC COMMIT: StockLot creation + Reservoir update in single transaction.
        // If this fails due to unique constraint violation, no changes are persisted.
        // ?????????????????????????????????????????????????????????????????
        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "Created Opening Balance StockLot: Reservoir {ReservoirId} ({Numero}), " +
            "Quantite {Quantite}L, Prix {Prix}, ID {StockLotId}",
            reservoirId, reservoir.Numero, quantite, prixAchat, stockLot.ID);

        return stockLot.ID;
    }

    // ?????????????????????????????????????????????????????????????????
    // ADJUSTMENT OPERATIONS (Stock Calibration)
    // ????????????????????????????????????????????????????????????????

    /// <inheritdoc />
    public async Task<int> CreateAdjustmentAsync(
        int reservoirId,
        int produitId,
        decimal quantite,
        decimal prixAchat,
        int? jaugeageId = null,
        string? notes = null)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(quantite, nameof(quantite));
        ArgumentOutOfRangeException.ThrowIfNegative(prixAchat, nameof(prixAchat));

        // Validate reservoir exists
        var reservoir = await _context.Reservoirs.FindAsync(reservoirId);
        if (reservoir == null)
        {
            throw new InvalidOperationException($"Reservoir {reservoirId} not found");
        }

        // Validate capacity
        var currentStock = await GetAvailableStockAsync(reservoirId);
        if (currentStock + quantite > reservoir.Capacite)
        {
            throw new InvalidOperationException(
                $"L'ajustement ({quantite:N2}L) dépasserait la capacité du réservoir. " +
                $"Stock actuel: {currentStock:N2}L, Capacité: {reservoir.Capacite:N2}L");
        }

        // Build notes with jaugeage reference
        var adjustmentNotes = notes ?? "Ajustement de stock suite à jaugeage";
        if (jaugeageId.HasValue)
        {
            adjustmentNotes = $"Calibration Jaugeage #{jaugeageId}. {adjustmentNotes}";
        }

        var stockLot = new StockLot
        {
            Type = StockLotType.Adjustment,
            AchatID = null, // No purchase for adjustment
            ReservoirID = reservoirId,
            ProduitID = produitId,
            QuantiteInitiale = quantite,
            QuantiteDisponible = quantite,
            PrixAchat = prixAchat,
            DateEntree = DateTime.UtcNow,
            Statut = "Disponible",
            Notes = adjustmentNotes
        };

        // Validate domain rules
        var errors = stockLot.Validate().ToList();
        if (errors.Count > 0)
        {
            throw new InvalidOperationException($"Invalid StockLot: {string.Join(", ", errors)}");
        }

        _context.StockLots.Add(stockLot);

        // Update reservoir level
        reservoir.NiveauDeCarburant += quantite;
        reservoir.DateModification = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "Created Adjustment StockLot: Reservoir {ReservoirId} ({Numero}), " +
            "Quantite +{Quantite}L, JaugeageRef={JaugeageId}, ID {StockLotId}",
            reservoirId, reservoir.Numero, quantite, jaugeageId, stockLot.ID);

        return stockLot.ID;
    }

    // ?????????????????????????????????????????????????????????????????
    // CALIBRATION OPERATIONS (Jaugeage Reconciliation)
    // ?????????????????????????????????????????????????????????????????

    /// <inheritdoc />
    public async Task<StockCalibrationPreviewDto> GetCalibrationPreviewAsync(int jaugeageId)
    {
        _logger.LogInformation("Getting calibration preview for Jaugeage {JaugeageId}", jaugeageId);

        var jaugeage = await _context.Jaugeages
            .Include(j => j.Temoin)
            .Include(j => j.JaugeageDetails)
                .ThenInclude(d => d.Reservoir)
                    .ThenInclude(r => r.Produit)
            .AsNoTracking()
            .FirstOrDefaultAsync(j => j.ID == jaugeageId);

        if (jaugeage == null)
        {
            throw new InvalidOperationException($"Jaugeage {jaugeageId} not found");
        }

        var preview = new StockCalibrationPreviewDto
        {
            JaugeageId = jaugeage.ID,
            JaugeageNumero = jaugeage.NumeroJaugeage,
            DateJaugeage = jaugeage.DateJaugeage,
            TemoinNom = jaugeage.Temoin?.Nom,
            Reservoirs = []
        };

        foreach (var detail in jaugeage.JaugeageDetails)
        {
            var reservoir = detail.Reservoir;
            if (reservoir == null) continue;

            // Get current system stock for this reservoir
            var systemStock = await GetAvailableStockAsync(detail.ReservoirID);

            // Jaugeage volume is the measured reality
            var jaugeageVolume = detail.VolumeCalcule;

            // Calculate difference
            var difference = jaugeageVolume - systemStock;

            var reservoirPreview = new ReservoirCalibrationPreviewDto
            {
                ReservoirId = detail.ReservoirID,
                ReservoirNumero = reservoir.Numero,
                ProduitId = reservoir.ProduitID ?? 0,
                ProduitNom = reservoir.Produit?.Description,
                VolumeJaugeage = jaugeageVolume,
                HauteurMesuree = detail.HauteurMesuree,
                StockSysteme = systemStock,
                Difference = difference,
                CanReduce = true
            };

            // If reduction needed, check if we can reduce
            if (difference < -0.01m)
            {
                var reductionNeeded = Math.Abs(difference);
                if (reductionNeeded > systemStock)
                {
                    reservoirPreview.CanReduce = false;
                    reservoirPreview.WarningMessage = 
                        $"Stock insuffisant pour réduire de {reductionNeeded:N2}L. " +
                        $"Disponible: {systemStock:N2}L";
                }
            }

            preview.Reservoirs.Add(reservoirPreview);

            _logger.LogDebug(
                "Calibration preview for Reservoir {ReservoirId}: " +
                "Jaugeage={JaugeageVol}L, System={SystemStock}L, Diff={Diff}L ({Action})",
                detail.ReservoirID, jaugeageVolume, systemStock, difference, reservoirPreview.Action);
        }

        return preview;
    }

    /// <inheritdoc />
    public async Task<StockCalibrationResultDto> CalibrateToJaugeageAsync(StockCalibrationRequestDto request)
    {
        _logger.LogInformation(
            "Calibrating stock to Jaugeage {JaugeageId}, User: {User}",
            request.JaugeageId, request.UtilisateurCalibration);

        // Get preview first to know what needs to be done
        var preview = await GetCalibrationPreviewAsync(request.JaugeageId);

        var result = new StockCalibrationResultDto
        {
            JaugeageId = request.JaugeageId,
            DateCalibration = DateTime.UtcNow,
            Reservoirs = []
        };

        // Filter reservoirs if specific ones requested
        var reservoirsToProcess = preview.Reservoirs;
        if (request.ReservoirIds?.Any() == true)
        {
            reservoirsToProcess = preview.Reservoirs
                .Where(r => request.ReservoirIds.Contains(r.ReservoirId))
                .ToList();
        }

        // Process each reservoir
        foreach (var reservoirPreview in reservoirsToProcess)
        {
            var reservoirResult = new ReservoirCalibrationResultDto
            {
                ReservoirId = reservoirPreview.ReservoirId,
                ReservoirNumero = reservoirPreview.ReservoirNumero,
                ProduitId = reservoirPreview.ProduitId,
                ProduitNom = reservoirPreview.ProduitNom,
                StockBefore = reservoirPreview.StockSysteme
            };

            try
            {
                if (Math.Abs(reservoirPreview.Difference) < 0.01m)
                {
                    // No adjustment needed
                    reservoirResult.StockAfter = reservoirPreview.StockSysteme;
                    reservoirResult.ActionPerformed = "Aucun";
                    reservoirResult.Message = "Stock déjà calibré";
                }
                else if (reservoirPreview.Difference > 0)
                {
                    // POSITIVE: Need to ADD stock - create Adjustment lot
                    // CreateAdjustmentAsync calls SaveChangesAsync internally, so sync works correctly after
                    await ProcessPositiveAdjustmentAsync(
                        reservoirPreview, request, reservoirResult);
                    // Sync reservoir level after the adjustment was saved
                    reservoirResult.StockAfter = await SyncReservoirLevelAsync(reservoirPreview.ReservoirId);
                }
                else
                {
                    // NEGATIVE: Need to REDUCE stock - FIFO consume
                    // CascadeReduceForCalibrationAsync updates StockLots AND Reservoir.NiveauDeCarburant
                    // but doesn't call SaveChangesAsync - changes are tracked but not persisted yet
                    await ProcessNegativeAdjustmentAsync(
                        reservoirPreview, request, reservoirResult);
                    // For negative adjustments, the expected StockAfter is the jaugeage volume
                    // Don't call SyncReservoirLevelAsync here as changes aren't saved yet!
                    reservoirResult.StockAfter = reservoirPreview.VolumeJaugeage;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error calibrating Reservoir {ReservoirId}: {Message}",
                    reservoirPreview.ReservoirId, ex.Message);

                reservoirResult.ActionPerformed = "Erreur";
                reservoirResult.Message = ex.Message;
                reservoirResult.StockAfter = reservoirPreview.StockSysteme;
            }

            result.Reservoirs.Add(reservoirResult);
        }

        // Save all pending changes (negative adjustments that haven't been saved yet)
        await _context.SaveChangesAsync();

        // After save, sync and verify reservoir levels for negative adjustments
        foreach (var reservoirResult in result.Reservoirs.Where(r => r.ActionPerformed == "Réduction"))
        {
            reservoirResult.StockAfter = await SyncReservoirLevelAsync(reservoirResult.ReservoirId);
        }
        await _context.SaveChangesAsync();

        result.Success = result.Reservoirs.All(r => r.ActionPerformed != "Erreur");
        result.Message = result.Success
            ? $"Calibration réussie: {result.AdjustmentLotsCreated} lot(s) d'ajustement créé(s), " +
              $"+{result.TotalStockAdded:N2}L ajouté, -{result.TotalStockReduced:N2}L réduit"
            : "Calibration partielle avec erreurs";

        _logger.LogInformation(
            "Calibration completed for Jaugeage {JaugeageId}: {Message}",
            request.JaugeageId, result.Message);

        return result;
    }

    /// <summary>
    /// Processes a positive adjustment (jaugeage > system) by creating an Adjustment StockLot.
    /// </summary>
    private async Task ProcessPositiveAdjustmentAsync(
        ReservoirCalibrationPreviewDto preview,
        StockCalibrationRequestDto request,
        ReservoirCalibrationResultDto result)
    {
        var quantiteToAdd = preview.Difference;

        // Determine price: use provided estimate, or average from existing lots
        var prixAchat = request.PrixAchatEstime ?? await GetAveragePrixAchatAsync(preview.ReservoirId);

        var notes = string.IsNullOrWhiteSpace(request.Notes)
            ? $"Ajustement positif suite à calibration jaugeage. Écart: +{quantiteToAdd:N2}L"
            : request.Notes;

        // Create adjustment lot (this calls SaveChangesAsync internally)
        var lotId = await CreateAdjustmentAsync(
            preview.ReservoirId,
            preview.ProduitId,
            quantiteToAdd,
            prixAchat,
            request.JaugeageId,
            notes);

        result.AdjustmentLotId = lotId;
        result.StockAdded = quantiteToAdd;
        result.ActionPerformed = "Ajout";
        result.Message = $"Lot d'ajustement créé: +{quantiteToAdd:N2}L";

        _logger.LogInformation(
            "Created positive adjustment for Reservoir {ReservoirId}: +{Quantite}L, LotId={LotId}",
            preview.ReservoirId, quantiteToAdd, lotId);
    }

    /// <summary>
    /// Processes a negative adjustment (jaugeage < system) by FIFO consuming existing lots.
    /// </summary>
    private async Task ProcessNegativeAdjustmentAsync(
        ReservoirCalibrationPreviewDto preview,
        StockCalibrationRequestDto request,
        ReservoirCalibrationResultDto result)
    {
        var quantiteToReduce = Math.Abs(preview.Difference);

        if (!preview.CanReduce)
        {
            throw new InvalidOperationException(preview.WarningMessage ?? "Cannot reduce stock");
        }

        // Use cascade reduce (FIFO order)
        var lotsAffected = await CascadeReduceForCalibrationAsync(
            preview.ReservoirId, 
            quantiteToReduce,
            request.JaugeageId,
            request.Notes);

        result.StockReduced = quantiteToReduce;
        result.LotsConsumed = lotsAffected;
        result.ActionPerformed = "Réduction";
        result.Message = $"Stock réduit de {quantiteToReduce:N2}L via {lotsAffected} lot(s)";

        _logger.LogInformation(
            "Reduced stock for Reservoir {ReservoirId}: -{Quantite}L, {LotsAffected} lots affected",
            preview.ReservoirId, quantiteToReduce, lotsAffected);
    }

    /// <summary>
    /// Gets the average PrixAchat from available lots in a reservoir.
    /// Used when no price is specified for adjustment lots.
    /// </summary>
    private async Task<decimal> GetAveragePrixAchatAsync(int reservoirId)
    {
        var avgPrice = await _context.StockLots
            .Where(s => s.ReservoirID == reservoirId 
                     && s.Statut == "Disponible" 
                     && s.PrixAchat > 0)
            .Select(s => (decimal?)s.PrixAchat)
            .AverageAsync();

        return avgPrice ?? 0;
    }

    /// <summary>
    /// FIFO reduces stock for calibration purposes (without creating consumption records).
    /// This is different from ConsumeAsync which creates PeriodeDetail-linked consumptions.
    /// Returns the number of lots affected.
    /// </summary>
    private async Task<int> CascadeReduceForCalibrationAsync(
        int reservoirId, 
        decimal amountToReduce,
        int? jaugeageId,
        string? notes)
    {
        if (amountToReduce <= 0)
            return 0;

        _logger.LogInformation(
            "Calibration reduction of {Amount}L for Reservoir {ReservoirId}",
            amountToReduce, reservoirId);

        // Get all lots for this reservoir, FIFO order (oldest first)
        var lots = await _context.StockLots
            .Where(s => s.ReservoirID == reservoirId 
                     && s.Statut == "Disponible" 
                     && s.QuantiteDisponible > 0)
            .OrderBy(s => s.DateEntree)
            .ThenBy(s => s.ID)
            .ToListAsync();

        var remainingToReduce = amountToReduce;
        var lotsAffected = 0;
        var calibrationNote = $"Réduction calibration Jaugeage #{jaugeageId}. {notes ?? ""}".Trim();

        foreach (var lot in lots)
        {
            if (remainingToReduce <= 0)
                break;

            lotsAffected++;

            if (lot.QuantiteDisponible >= remainingToReduce)
            {
                // This lot can absorb the entire remaining reduction
                lot.QuantiteDisponible -= remainingToReduce;
                lot.Notes = string.IsNullOrWhiteSpace(lot.Notes)
                    ? calibrationNote
                    : $"{lot.Notes} | {calibrationNote}";

                _logger.LogDebug(
                    "Reduced StockLot {LotId} by {Amount}L, new QteDisponible: {NewQte}L",
                    lot.ID, remainingToReduce, lot.QuantiteDisponible);

                if (lot.QuantiteDisponible <= 0)
                {
                    lot.Statut = "Épuisé";
                }
                remainingToReduce = 0;
            }
            else
            {
                // This lot is exhausted, continue to next
                remainingToReduce -= lot.QuantiteDisponible;
                
                _logger.LogDebug(
                    "Exhausted StockLot {LotId}, reduced by {Amount}L, remaining to reduce: {Remaining}L",
                    lot.ID, lot.QuantiteDisponible, remainingToReduce);

                lot.Notes = string.IsNullOrWhiteSpace(lot.Notes)
                    ? calibrationNote
                    : $"{lot.Notes} | {calibrationNote}";
                lot.QuantiteDisponible = 0;
                lot.Statut = "Épuisé";
            }
        }

        if (remainingToReduce > 0.01m)
        {
            _logger.LogError(
                "Could not fully reduce stock for Reservoir {ReservoirId}. Remaining: {Remaining}L",
                reservoirId, remainingToReduce);
            throw new InvalidOperationException(
                $"Stock insuffisant pour la calibration. Restant à réduire: {remainingToReduce:N2}L");
        }

        // Update reservoir level
        var reservoir = await _context.Reservoirs.FindAsync(reservoirId);
        if (reservoir != null)
        {
            reservoir.NiveauDeCarburant -= amountToReduce;
            reservoir.DateModification = DateTime.UtcNow;
        }

        return lotsAffected;
    }

    /// <inheritdoc />
    public async Task<bool> HasOpeningBalanceAsync(int reservoirId)
    {
        return await _context.StockLots
            .AnyAsync(s => s.ReservoirID == reservoirId && s.Type == StockLotType.OpeningBalance);
    }

    /// <inheritdoc />
    public async Task<bool> HasAnyStockLotsAsync(int reservoirId)
    {
        return await _context.StockLots
            .AnyAsync(s => s.ReservoirID == reservoirId && s.Statut != "Annulé");
    }

    /// <inheritdoc />
    public async Task<bool> ValidateCapacityAsync(int reservoirId, decimal quantiteToAdd)
    {
        var reservoir = await _context.Reservoirs.FindAsync(reservoirId);
        if (reservoir == null)
            return false;

        var currentLevel = await GetAvailableStockAsync(reservoirId);
        return (currentLevel + quantiteToAdd) <= reservoir.Capacite;
    }

    /// <inheritdoc />
    public async Task<bool> ValidateStockAvailabilityAsync(int reservoirId, int produitId, decimal quantiteRequired)
    {
        if (quantiteRequired <= 0)
            return true;

        var available = await _context.StockLots
            .Where(s => s.ReservoirID == reservoirId 
                     && s.ProduitID == produitId
                     && s.Statut == "Disponible"
                     && s.QuantiteDisponible > 0)
            .SumAsync(s => s.QuantiteDisponible);

        return available >= quantiteRequired;
    }

    /// <inheritdoc />
    public async Task<decimal> GetAvailableStockAsync(int reservoirId)
    {
        return await _context.StockLots
            .Where(s => s.ReservoirID == reservoirId && s.Statut == "Disponible")
            .SumAsync(s => s.QuantiteDisponible);
    }

    /// <inheritdoc />
    public async Task<bool> ReverseStockLotAsync(int achatId, int reservoirId, decimal quantite)
    {
        _logger.LogInformation(
            "Attempting to reverse StockLot: Achat {AchatId}, Reservoir {ReservoirId}, Qty {Quantite}L",
            achatId, reservoirId, quantite);

        // Find the StockLot created from this allocation
        var stockLot = await _context.StockLots
            .Include(s => s.Consumptions)
            .Where(s => s.AchatID == achatId && s.ReservoirID == reservoirId)
            .OrderByDescending(s => s.DateEntree) // Get the most recent one
            .FirstOrDefaultAsync();

        if (stockLot == null)
        {
            _logger.LogWarning(
                "No StockLot found to reverse for Achat {AchatId}, Reservoir {ReservoirId}",
                achatId, reservoirId);
            return false;
        }

        // Check if any consumption has occurred
        if (stockLot.Consumptions.Any())
        {
            _logger.LogWarning(
                "Cannot reverse StockLot {StockLotId} - it has {Count} consumption records",
                stockLot.ID, stockLot.Consumptions.Count);
            return false;
        }

        // Check if the quantity matches (or at least available)
        if (stockLot.QuantiteDisponible < quantite)
        {
            _logger.LogWarning(
                "Cannot reverse StockLot {StockLotId} - available {Available}L < requested {Requested}L",
                stockLot.ID, stockLot.QuantiteDisponible, quantite);
            return false;
        }

        // Mark as cancelled/remove
        stockLot.QuantiteDisponible = 0;
        stockLot.Statut = "Annulé";

        // Update reservoir level
        var reservoir = await _context.Reservoirs.FindAsync(reservoirId);
        if (reservoir != null)
        {
            reservoir.NiveauDeCarburant = Math.Max(0, reservoir.NiveauDeCarburant - quantite);
            _logger.LogInformation(
                "Reversed Reservoir {ReservoirId} level to {Level}L",
                reservoirId, reservoir.NiveauDeCarburant);
        }

        _logger.LogInformation("Successfully reversed StockLot {StockLotId}", stockLot.ID);
        return true;
    }

    // ???????????????????????????????????????????????????????????????????
    // ALLOCATION ADJUSTMENT OPERATIONS
    // ???????????????????????????????????????????????????????????????????

    /// <inheritdoc />
    public async Task<AdjustmentPreviewDto?> GetAdjustmentPreviewAsync(int achatId)
    {
        _logger.LogInformation("Getting adjustment preview for Achat {AchatId}", achatId);

        var achat = await _context.Achats
            .Include(a => a.Produit)
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.ID == achatId);

        if (achat == null)
        {
            _logger.LogWarning("Achat {AchatId} not found for adjustment preview", achatId);
            return null;
        }

        // Get existing allocations for this Achat
        var allocations = await _context.AchatAllocations
            .Include(a => a.Reservoir)
            .Where(a => a.AchatID == achatId && a.Statut != "Annulée")
            .AsNoTracking()
            .ToListAsync();

        _logger.LogInformation("Found {Count} allocations for Achat {AchatId}", allocations.Count, achatId);

        var previewItems = new List<AllocationPreviewItemDto>();

        foreach (var allocation in allocations)
        {
            // Find the StockLot for this allocation
            var stockLot = await _context.StockLots
                .Where(s => s.AchatID == achatId && s.ReservoirID == allocation.ReservoirID && s.Statut != "Annulé")
                .FirstOrDefaultAsync();

            // Get total QteRestante in this reservoir (all lots)
            var reservoirQteRestante = await _context.StockLots
                .Where(s => s.ReservoirID == allocation.ReservoirID && s.Statut == "Disponible")
                .SumAsync(s => s.QuantiteDisponible);

            var consumedQte = stockLot != null 
                ? stockLot.QuantiteInitiale - stockLot.QuantiteDisponible 
                : 0;

            // MaxReducible is the minimum of:
            // 1. What hasn't been consumed from this specific allocation
            // 2. What's available in the reservoir to "give back"
            var maxReducibleFromAllocation = allocation.QuantiteAllouee - consumedQte;
            var maxReducible = Math.Min(maxReducibleFromAllocation, reservoirQteRestante);

            _logger.LogInformation(
                "Allocation preview for Reservoir {ReservoirId}: " +
                "AllocQte={AllocQte}, ConsumedQte={ConsumedQte}, MaxReducibleFromAlloc={MaxReducibleFromAlloc}, " +
                "ReservoirQteRestante={ReservoirQteRestante}, FinalMaxReducible={MaxReducible}",
                allocation.ReservoirID, allocation.QuantiteAllouee, consumedQte, 
                maxReducibleFromAllocation, reservoirQteRestante, maxReducible);

            previewItems.Add(new AllocationPreviewItemDto
            {
                AllocationId = allocation.ID,
                ReservoirId = allocation.ReservoirID,
                ReservoirNumero = allocation.Reservoir?.Numero,
                CurrentQuantite = allocation.QuantiteAllouee,
                ConsumedQuantite = consumedQte,
                MaxReducible = maxReducible,
                ReservoirQteRestante = reservoirQteRestante,
                ReservoirCapacite = allocation.Reservoir?.Capacite ?? 0,
                ReservoirNiveauActuel = allocation.Reservoir?.NiveauDeCarburant ?? 0
            });
        }

        var result = new AdjustmentPreviewDto
        {
            AchatId = achatId,
            AchatNumero = achat.Numero,
            CurrentAchatQuantite = achat.Quantite ?? 0,
            ProduitId = achat.ProduitID ?? 0,
            ProduitNom = achat.Produit?.Description,
            PrixAchatUnitaire = achat.PrixAchatUnitaire ?? 0,
            Allocations = previewItems
        };

        _logger.LogInformation(
            "Adjustment preview for Achat {AchatId}: CurrentQte={CurrentQte}, {AllocationCount} allocations",
            achatId, result.CurrentAchatQuantite, previewItems.Count);

        return result;
    }

    /// <inheritdoc />
    public async Task<AllocationAdjustmentValidationResult> ValidateAllocationAdjustmentAsync(AdjustAllocationsRequestDto request)
    {
        var result = new AllocationAdjustmentValidationResult
        {
            IsValid = true,
            Details = []
        };

        // Get current allocations
        var currentAllocations = await _context.AchatAllocations
            .Include(a => a.Reservoir)
            .Where(a => a.AchatID == request.AchatId && a.Statut != "Annulée")
            .ToListAsync();

        var currentAllocationsDict = currentAllocations.ToDictionary(a => a.ReservoirID);

        // Validate total matches
        var totalNewQuantite = request.Allocations.Sum(a => a.NewQuantite);
        if (Math.Abs(totalNewQuantite - request.NewAchatQuantite) > 0.01m)
        {
            result.IsValid = false;
            result.ErrorMessage = $"La somme des allocations ({totalNewQuantite:N2}L) ne correspond pas à la quantité de l'achat ({request.NewAchatQuantite:N2}L)";
            return result;
        }

        foreach (var newAlloc in request.Allocations)
        {
            var detail = new AllocationValidationDetailDto
            {
                AllocationId = newAlloc.AllocationId,
                ReservoirId = newAlloc.ReservoirId,
                NewQuantite = newAlloc.NewQuantite,
                IsValid = true
            };

            // Get reservoir info
            var reservoir = await _context.Reservoirs.FindAsync(newAlloc.ReservoirId);
            if (reservoir == null)
            {
                detail.IsValid = false;
                detail.ErrorMessage = "Réservoir non trouvé";
                result.Details.Add(detail);
                result.IsValid = false;
                continue;
            }
            detail.ReservoirNumero = reservoir.Numero;

            // Check if this is an existing allocation or new
            if (currentAllocationsDict.TryGetValue(newAlloc.ReservoirId, out var existingAlloc))
            {
                detail.OldQuantite = existingAlloc.QuantiteAllouee;
                detail.Difference = newAlloc.NewQuantite - existingAlloc.QuantiteAllouee;

                if (detail.Difference < 0)
                {
                    // DECREASE: Check if we can reduce
                    var reduction = Math.Abs(detail.Difference);

                    // Get total QteRestante in this reservoir
                    var reservoirQteRestante = await _context.StockLots
                        .Where(s => s.ReservoirID == newAlloc.ReservoirId && s.Statut == "Disponible")
                        .SumAsync(s => s.QuantiteDisponible);

                    detail.MaxReducible = reservoirQteRestante;

                    if (reduction > reservoirQteRestante)
                    {
                        detail.IsValid = false;
                        detail.ErrorMessage = $"Réduction impossible: {reduction:N2}L demandé, seulement {reservoirQteRestante:N2}L disponible dans le réservoir";
                        result.IsValid = false;
                    }
                }
                else if (detail.Difference > 0)
                {
                    // INCREASE: Check reservoir capacity
                    var increase = detail.Difference;
                    var espaceDisponible = reservoir.Capacite - reservoir.NiveauDeCarburant;

                    if (increase > espaceDisponible)
                    {
                        detail.IsValid = false;
                        detail.ErrorMessage = $"Augmentation impossible: {increase:N2}L demandé, seulement {espaceDisponible:N2}L d'espace disponible";
                        result.IsValid = false;
                    }
                }
            }
            else
            {
                // NEW allocation - check capacity
                detail.OldQuantite = 0;
                detail.Difference = newAlloc.NewQuantite;

                var espaceDisponible = reservoir.Capacite - reservoir.NiveauDeCarburant;
                if (newAlloc.NewQuantite > espaceDisponible)
                {
                    detail.IsValid = false;
                    detail.ErrorMessage = $"Capacité insuffisante: {newAlloc.NewQuantite:N2}L demandé, seulement {espaceDisponible:N2}L d'espace disponible";
                    result.IsValid = false;
                }
            }

            result.Details.Add(detail);
        }

        return result;
    }

    /// <inheritdoc />
    public async Task<AllocationAdjustmentResultDto> AdjustAllocationsAsync(AdjustAllocationsRequestDto request)
    {
        _logger.LogInformation(
            "Adjusting allocations for Achat {AchatId}, NewQuantite: {NewQte}L",
            request.AchatId, request.NewAchatQuantite);

        // First validate
        var validation = await ValidateAllocationAdjustmentAsync(request);
        if (!validation.IsValid)
        {
            return new AllocationAdjustmentResultDto
            {
                Success = false,
                Message = validation.ErrorMessage ?? "Validation échouée",
                AchatId = request.AchatId
            };
        }

        // Get current allocations
        var currentAllocations = await _context.AchatAllocations
            .Include(a => a.Reservoir)
            .Where(a => a.AchatID == request.AchatId && a.Statut != "Annulée")
            .ToListAsync();

        var currentAllocationsDict = currentAllocations.ToDictionary(a => a.ReservoirID);

        // Get the Achat for price info
        var achat = await _context.Achats.FindAsync(request.AchatId);
        if (achat == null)
        {
            return new AllocationAdjustmentResultDto
            {
                Success = false,
                Message = "Achat non trouvé",
                AchatId = request.AchatId
            };
        }

        var oldAchatQuantite = achat.Quantite ?? 0;
        var prixAchat = achat.PrixAchatUnitaire ?? 0;

        foreach (var newAlloc in request.Allocations)
        {
            if (currentAllocationsDict.TryGetValue(newAlloc.ReservoirId, out var existingAlloc))
            {
                // EXISTING ALLOCATION - Adjust
                var difference = newAlloc.NewQuantite - existingAlloc.QuantiteAllouee;

                if (Math.Abs(difference) < 0.001m)
                    continue; // No change

                // Find the StockLot for this allocation
                var stockLot = await _context.StockLots
                    .Where(s => s.AchatID == request.AchatId && s.ReservoirID == newAlloc.ReservoirId && s.Statut != "Annulé")
                    .FirstOrDefaultAsync();

                if (stockLot == null)
                {
                    _logger.LogWarning(
                        "StockLot not found for Achat {AchatId}, Reservoir {ReservoirId}",
                        request.AchatId, newAlloc.ReservoirId);
                    continue;
                }

                var reservoir = existingAlloc.Reservoir;

                if (difference > 0)
                {
                    // INCREASE
                    _logger.LogInformation(
                        "Increasing allocation for Reservoir {ReservoirId} by {Diff}L",
                        newAlloc.ReservoirId, difference);

                    existingAlloc.QuantiteAllouee = newAlloc.NewQuantite;
                    stockLot.QuantiteInitiale += difference;
                    stockLot.QuantiteDisponible += difference;
                    reservoir.NiveauDeCarburant += difference;
                }
                else
                {
                    // DECREASE
                    var reduction = Math.Abs(difference);
                    _logger.LogInformation(
                        "Decreasing allocation for Reservoir {ReservoirId} by {Reduction}L",
                        newAlloc.ReservoirId, reduction);

                    existingAlloc.QuantiteAllouee = newAlloc.NewQuantite;
                    stockLot.QuantiteInitiale -= reduction;

                    // Cascade reduce QteRestante through reservoir's lots
                    await CascadeReduceQteRestanteAsync(newAlloc.ReservoirId, reduction);

                    reservoir.NiveauDeCarburant -= reduction;
                }

                reservoir.DateModification = DateTime.UtcNow;
            }
            else if (newAlloc.NewQuantite > 0)
            {
                // NEW ALLOCATION
                _logger.LogInformation(
                    "Creating new allocation for Reservoir {ReservoirId}: {Qte}L",
                    newAlloc.ReservoirId, newAlloc.NewQuantite);

                var reservoir = await _context.Reservoirs.FindAsync(newAlloc.ReservoirId);
                if (reservoir == null)
                    continue;

                // Create allocation
                var newAllocation = new AchatAllocation
                {
                    AchatID = request.AchatId,
                    ReservoirID = newAlloc.ReservoirId,
                    QuantiteAllouee = newAlloc.NewQuantite,
                    DateAllocation = DateTime.UtcNow,
                    Statut = "Confirmée",
                    UtilisateurAllocation = request.UtilisateurAdjustment,
                    Notes = request.Notes
                };
                _context.AchatAllocations.Add(newAllocation);

                // Create StockLot
                await CreateStockLotAsync(
                    request.AchatId,
                    newAlloc.ReservoirId,
                    achat.ProduitID ?? reservoir.ProduitID ?? 0,
                    newAlloc.NewQuantite,
                    prixAchat);
            }
        }

        // Handle allocations that were removed (set to 0 or not in new list)
        var newReservoirIds = request.Allocations.Where(a => a.NewQuantite > 0).Select(a => a.ReservoirId).ToHashSet();
        foreach (var existingAlloc in currentAllocations)
        {
            if (!newReservoirIds.Contains(existingAlloc.ReservoirID))
            {
                // This allocation was removed - reduce to 0
                var reduction = existingAlloc.QuantiteAllouee;

                _logger.LogInformation(
                    "Removing allocation for Reservoir {ReservoirId}: reducing {Reduction}L",
                    existingAlloc.ReservoirID, reduction);

                var stockLot = await _context.StockLots
                    .Where(s => s.AchatID == request.AchatId && s.ReservoirID == existingAlloc.ReservoirID && s.Statut != "Annulé")
                    .FirstOrDefaultAsync();

                if (stockLot != null)
                {
                    stockLot.QuantiteInitiale -= reduction;
                    if (stockLot.QuantiteInitiale <= 0)
                    {
                        stockLot.Statut = "Annulé";
                    }

                    await CascadeReduceQteRestanteAsync(existingAlloc.ReservoirID, reduction);
                }

                existingAlloc.QuantiteAllouee = 0;
                existingAlloc.Statut = "Annulée";

                var reservoir = existingAlloc.Reservoir;
                reservoir.NiveauDeCarburant -= reduction;
                reservoir.DateModification = DateTime.UtcNow;
            }
        }

        await _context.SaveChangesAsync();

        // Get updated allocations for response
        var updatedAllocations = await _context.AchatAllocations
            .Include(a => a.Reservoir)
                .ThenInclude(r => r.Produit)
            .Where(a => a.AchatID == request.AchatId && a.Statut != "Annulée")
            .ToListAsync();

        var mapper = new AutoMapper.Mapper(new AutoMapper.MapperConfiguration(cfg => { }));
        // Manual mapping since we may not have automapper configured here
        var allocDtos = updatedAllocations.Select(a => new AchatAllocationDto
        {
            ID = a.ID,
            AchatID = a.AchatID,
            ReservoirID = a.ReservoirID,
            QuantiteAllouee = a.QuantiteAllouee,
            DateAllocation = a.DateAllocation,
            Notes = a.Notes,
            Statut = a.Statut,
            ReservoirNumero = a.Reservoir?.Numero,
            ReservoirCapacite = a.Reservoir?.Capacite,
            ReservoirNiveauActuel = a.Reservoir?.NiveauDeCarburant,
            ProduitID = a.Reservoir?.ProduitID,
            ProduitNom = a.Reservoir?.Produit?.Description
        }).ToList();

        _logger.LogInformation(
            "Successfully adjusted allocations for Achat {AchatId}: {OldQte}L ? {NewQte}L",
            request.AchatId, oldAchatQuantite, request.NewAchatQuantite);

        return new AllocationAdjustmentResultDto
        {
            Success = true,
            Message = $"Allocations ajustées avec succès: {oldAchatQuantite:N2}L ? {request.NewAchatQuantite:N2}L",
            AchatId = request.AchatId,
            OldAchatQuantite = oldAchatQuantite,
            NewAchatQuantite = request.NewAchatQuantite,
            UpdatedAllocations = allocDtos
        };
    }

    /// <summary>
    /// Cascades QteRestante reduction through a reservoir's StockLots following FIFO order.
    /// Starts from the lot being modified and continues to newer lots until the reduction is absorbed.
    /// </summary>
    private async Task CascadeReduceQteRestanteAsync(int reservoirId, decimal amountToReduce)
    {
        if (amountToReduce <= 0)
            return;

        _logger.LogInformation(
            "Cascading QteRestante reduction of {Amount}L for Reservoir {ReservoirId}",
            amountToReduce, reservoirId);

        // Get all lots for this reservoir, ordered by ID (oldest first)
        var lots = await _context.StockLots
            .Where(s => s.ReservoirID == reservoirId && s.Statut == "Disponible" && s.QuantiteDisponible > 0)
            .OrderBy(s => s.ID)
            .ToListAsync();

        var remainingToReduce = amountToReduce;

        foreach (var lot in lots)
        {
            if (remainingToReduce <= 0)
                break;

            if (lot.QuantiteDisponible >= remainingToReduce)
            {
                // This lot can absorb the entire remaining reduction
                lot.QuantiteDisponible -= remainingToReduce;
                _logger.LogDebug(
                    "Reduced StockLot {LotId} by {Amount}L, new QteDisponible: {NewQte}L",
                    lot.ID, remainingToReduce, lot.QuantiteDisponible);

                if (lot.QuantiteDisponible <= 0)
                {
                    lot.Statut = "Épuisé";
                }
                remainingToReduce = 0;
            }
            else
            {
                // This lot is exhausted, continue to next
                remainingToReduce -= lot.QuantiteDisponible;
                _logger.LogDebug(
                    "Exhausted StockLot {LotId}, reduced by {Amount}L, remaining to reduce: {Remaining}L",
                    lot.ID, lot.QuantiteDisponible, remainingToReduce);

                lot.QuantiteDisponible = 0;
                lot.Statut = "Épuisé";
            }
        }

        if (remainingToReduce > 0)
        {
            _logger.LogError(
                "Could not fully reduce QteRestante for Reservoir {ReservoirId}. Remaining: {Remaining}L",
                reservoirId, remainingToReduce);
            throw new InvalidOperationException(
                $"Stock insuffisant pour la réduction. Restant à réduire: {remainingToReduce:N2}L");
        }
    }

    // ???????????????????????????????????????????????????????????????????????
    // SYNC OPERATIONS (Option C - Always Recalculate from Source of Truth)
    // ???????????????????????????????????????????????????????????????????????

    /// <inheritdoc />
    public async Task<decimal> SyncReservoirLevelAsync(int reservoirId)
    {
        var reservoir = await _context.Reservoirs.FindAsync(reservoirId);
        if (reservoir == null)
        {
            _logger.LogWarning("Reservoir {ReservoirId} not found for sync", reservoirId);
            return 0;
        }

        // Calculate level from StockLots (source of truth)
        var calculatedLevel = await _context.StockLots
            .Where(s => s.ReservoirID == reservoirId && s.Statut == "Disponible")
            .SumAsync(s => s.QuantiteDisponible);

        var oldLevel = reservoir.NiveauDeCarburant;
        reservoir.NiveauDeCarburant = calculatedLevel;

        if (Math.Abs(oldLevel - calculatedLevel) > 0.001m)
        {
            _logger.LogInformation(
                "Synced Reservoir {ReservoirId} level: {OldLevel}L ? {NewLevel}L (diff: {Diff}L)",
                reservoirId, oldLevel, calculatedLevel, calculatedLevel - oldLevel);
        }

        return calculatedLevel;
    }

    /// <inheritdoc />
    public async Task<bool> SyncPompeCountersAsync(int pompeId)
    {
        var pompe = await _context.Pompes.FindAsync(pompeId);
        if (pompe == null)
        {
            _logger.LogWarning("Pompe {PompeId} not found for sync", pompeId);
            return false;
        }

        // Find the most recent PeriodeDetail for this pump (by period end date)
        var latestDetail = await _context.PeriodeDetails
            .Include(d => d.Periode)
            .Where(d => d.PompeID == pompeId && d.Periode != null)
            .OrderByDescending(d => d.Periode!.DateFin)
            .FirstOrDefaultAsync();

        if (latestDetail == null)
        {
            _logger.LogDebug("No period details found for Pompe {PompeId}, counters unchanged", pompeId);
            return false;
        }

        var oldElec = pompe.CompteurElectroniqueActuel;
        var oldMeca = pompe.CompteurMecaniqueActuel;

        pompe.CompteurElectroniqueActuel = latestDetail.CompteurElectroniqueFinal;
        pompe.CompteurMecaniqueActuel = latestDetail.CompteurMecaniqueFinal;

        if (oldElec != latestDetail.CompteurElectroniqueFinal || 
            oldMeca != latestDetail.CompteurMecaniqueFinal)
        {
            _logger.LogInformation(
                "Synced Pompe {PompeId} counters from Periode {PeriodeId} ({DateFin:d}): " +
                "Elec {OldElec} ? {NewElec}, Meca {OldMeca} ? {NewMeca}",
                pompeId, latestDetail.PeriodeID, latestDetail.Periode?.DateFin,
                oldElec, pompe.CompteurElectroniqueActuel,
                oldMeca, pompe.CompteurMecaniqueActuel);
        }

        return true;
    }

    /// <inheritdoc />
    public async Task SyncMultiplePompeCountersAsync(IEnumerable<int> pompeIds)
    {
        foreach (var pompeId in pompeIds.Distinct())
        {
            await SyncPompeCountersAsync(pompeId);
        }
        await _context.SaveChangesAsync();
    }

    /// <inheritdoc />
    public async Task SyncMultipleReservoirLevelsAsync(IEnumerable<int> reservoirIds)
    {
        foreach (var reservoirId in reservoirIds.Distinct())
        {
            await SyncReservoirLevelAsync(reservoirId);
        }
        await _context.SaveChangesAsync();
    }

    // ???????????????????????????????????????????????????????????????????????
    // ANALYSIS OPERATIONS (Margin/Profit Reporting)
    // ???????????????????????????????????????????????????????????????????????

    /// <inheritdoc />
    public async Task<StockLotAnalysisDto?> GetStockLotAnalysisAsync(int stockLotId)
    {
        var stockLot = await _context.StockLots
            .Include(s => s.Reservoir)
            .Include(s => s.Produit)
            .Include(s => s.Consumptions)
                .ThenInclude(c => c.PeriodeDetail)
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.ID == stockLotId);

        if (stockLot == null)
            return null;

        return BuildStockLotAnalysis(stockLot);
    }

    /// <inheritdoc />
    public async Task<ReservoirAnalysisDto?> GetReservoirAnalysisAsync(int reservoirId)
    {
        var reservoir = await _context.Reservoirs
            .Include(r => r.Produit)
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.ID == reservoirId);

        if (reservoir == null)
            return null;

        var stockLots = await _context.StockLots
            .Include(s => s.Produit)
            .Include(s => s.Consumptions)
                .ThenInclude(c => c.PeriodeDetail)
            .Where(s => s.ReservoirID == reservoirId)
            .AsNoTracking()
            .ToListAsync();

        var lotAnalyses = stockLots.Select(BuildStockLotAnalysis).ToList();
        
        // Count lots with unknown cost (PrixAchat = 0)
        var unknownCostCount = lotAnalyses.Count(l => !l.HasKnownCost && l.QuantiteVendue > 0);

        return new ReservoirAnalysisDto
        {
            ReservoirID = reservoir.ID,
            ReservoirNumero = reservoir.Numero,
            ProduitID = reservoir.ProduitID,
            ProduitNom = reservoir.Produit?.Description,
            CapaciteTotale = reservoir.Capacite,
            NiveauActuel = reservoir.NiveauDeCarburant,
            TotalStockLots = stockLots.Count,
            StockLotsDisponibles = stockLots.Count(s => s.Statut == "Disponible"),
            StockLotsEpuises = stockLots.Count(s => s.Statut == "Épuisé"),
            TotalQuantiteAchetee = lotAnalyses.Sum(l => l.QuantiteInitiale),
            TotalQuantiteVendue = lotAnalyses.Sum(l => l.QuantiteVendue),
            TotalQuantiteDisponible = lotAnalyses.Sum(l => l.QuantiteDisponible),
            TotalChiffreAffaires = lotAnalyses.Sum(l => l.ChiffreAffaires),
            TotalCoutRevient = lotAnalyses.Sum(l => l.CoutRevient),
            PrixAchatMoyen = lotAnalyses.Any() 
                ? Math.Round(lotAnalyses.Average(l => l.PrixAchat), 2) 
                : 0,
            PrixVenteMoyen = lotAnalyses.Where(l => l.QuantiteVendue > 0).Any()
                ? Math.Round(lotAnalyses.Where(l => l.QuantiteVendue > 0).Average(l => l.PrixVenteMoyen), 2)
                : 0,
            // Unknown cost tracking for margin accuracy warnings
            HasUnknownCostStock = unknownCostCount > 0,
            UnknownCostStockLotCount = unknownCostCount,
            StockLots = lotAnalyses
        };
    }

    /// <inheritdoc />
    public async Task<GlobalAnalysisSummaryDto> GetGlobalAnalysisAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        // Build consumption query with optional date filter
        var consumptionQuery = _context.StockLotConsumptions
            .Include(c => c.StockLot)
                .ThenInclude(s => s.Produit)
            .Include(c => c.PeriodeDetail)
            .AsNoTracking();

        if (startDate.HasValue)
            consumptionQuery = consumptionQuery.Where(c => c.DateConsommation >= startDate.Value);
        if (endDate.HasValue)
            consumptionQuery = consumptionQuery.Where(c => c.DateConsommation <= endDate.Value);

        var consumptions = await consumptionQuery.ToListAsync();

        // Get all reservoirs with their stock lots
        var reservoirs = await _context.Reservoirs
            .Include(r => r.Produit)
            .Include(r => r.StockLots)
            .AsNoTracking()
            .ToListAsync();

        // Calculate per-product metrics
        var productGroups = consumptions
            .GroupBy(c => new { c.StockLot.ProduitID, c.StockLot.Produit?.Description })
            .Select(g => new ProductAnalysisSummaryDto
            {
                ProduitID = g.Key.ProduitID,
                ProduitNom = g.Key.Description,
                TotalQuantiteVendue = g.Sum(c => c.QuantiteConsommee),
                TotalCoutRevient = g.Sum(c => c.QuantiteConsommee * c.PrixUnitaire),
                TotalChiffreAffaires = g.Sum(c => c.QuantiteConsommee * c.PeriodeDetail.PrixCarburant)
            })
            .ToList();

        // Get reservoir analyses
        var reservoirAnalyses = new List<ReservoirAnalysisDto>();
        foreach (var reservoir in reservoirs.Where(r => r.StockLots.Any()))
        {
            var analysis = await GetReservoirAnalysisAsync(reservoir.ID);
            if (analysis != null)
            {
                // Clear nested lots for summary to reduce payload
                analysis.StockLots = [];
                reservoirAnalyses.Add(analysis);
            }
        }

        // Count distinct periodes
        var periodeIds = consumptions
            .Select(c => c.PeriodeDetail.PeriodeID)
            .Distinct()
            .Count();

        // Check if any consumption came from a stock lot with unknown cost (PrixAchat = 0)
        // This indicates margin calculations may be inflated
        var hasUnknownCost = consumptions.Any(c => c.PrixUnitaire == 0);

        return new GlobalAnalysisSummaryDto
        {
            DateDebut = startDate,
            DateFin = endDate,
            TotalReservoirs = reservoirs.Count(r => r.StockLots.Any()),
            TotalStockLots = reservoirs.Sum(r => r.StockLots.Count),
            TotalPeriodesAnalysees = periodeIds,
            TotalQuantiteAchetee = reservoirs.Sum(r => r.StockLots.Sum(s => s.QuantiteInitiale)),
            TotalQuantiteVendue = consumptions.Sum(c => c.QuantiteConsommee),
            TotalQuantiteEnStock = reservoirs.Sum(r => r.StockLots.Where(s => s.Statut == "Disponible").Sum(s => s.QuantiteDisponible)),
            TotalChiffreAffaires = consumptions.Sum(c => c.QuantiteConsommee * c.PeriodeDetail.PrixCarburant),
            TotalCoutRevient = consumptions.Sum(c => c.QuantiteConsommee * c.PrixUnitaire),
            // Flag for margin accuracy warning
            HasUnknownCostStock = hasUnknownCost,
            ParProduit = productGroups,
            ParReservoir = reservoirAnalyses
        };
    }

    /// <inheritdoc />
    public async Task<PeriodeMargeAnalysisDto?> GetPeriodeMargeAnalysisAsync(int periodeId)
    {
        _logger.LogInformation("Getting marge analysis for Periode {PeriodeId}", periodeId);

        // Get the periode
        var periode = await _context.Periodes
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.PeriodeID == periodeId);

        if (periode == null)
        {
            _logger.LogWarning("Periode {PeriodeId} not found", periodeId);
            return null;
        }

        // Get all consumptions for this periode's details
        var consumptions = await _context.StockLotConsumptions
            .Include(c => c.StockLot)
                .ThenInclude(s => s.Produit)
            .Include(c => c.StockLot)
                .ThenInclude(s => s.Reservoir)
            .Include(c => c.PeriodeDetail)
                .ThenInclude(d => d.Pompe)
            .Where(c => c.PeriodeDetail.PeriodeID == periodeId)
            .AsNoTracking()
            .ToListAsync();

        // Build consumption details
        var consommationDetails = consumptions.Select(c => new ConsommationDetailDto
        {
            ConsumptionID = c.ID,
            StockLotID = c.StockLotID,
            PeriodeDetailID = c.PeriodeDetailID,
            ProduitID = c.StockLot.ProduitID,
            ProduitNom = c.StockLot.Produit?.Description,
            ReservoirID = c.StockLot.ReservoirID,
            ReservoirNumero = c.StockLot.Reservoir?.Numero,
            PompeID = c.PeriodeDetail.PompeID,
            PompeNumero = c.PeriodeDetail.Pompe?.Numero,
            QuantiteConsommee = c.QuantiteConsommee,
            PrixAchat = c.PrixUnitaire,
            PrixVente = c.PeriodeDetail.PrixCarburant,
            DateConsommation = c.DateConsommation
        }).ToList();

        // Aggregate by product
        var parProduit = consommationDetails
            .GroupBy(c => new { c.ProduitID, c.ProduitNom })
            .Select(g => new ProduitMargeDto
            {
                ProduitID = g.Key.ProduitID,
                ProduitNom = g.Key.ProduitNom,
                TotalQuantite = g.Sum(c => c.QuantiteConsommee),
                TotalCoutAchat = g.Sum(c => c.QuantiteConsommee * c.PrixAchat),
                TotalVente = g.Sum(c => c.QuantiteConsommee * c.PrixVente),
                PrixAchatMoyen = g.Sum(c => c.QuantiteConsommee) > 0
                    ? Math.Round(g.Sum(c => c.CoutAchat) / g.Sum(c => c.QuantiteConsommee), 2)
                    : 0,
                PrixVenteMoyen = g.Sum(c => c.QuantiteConsommee) > 0
                    ? Math.Round(g.Sum(c => c.Vente) / g.Sum(c => c.QuantiteConsommee), 2)
                    : 0
            })
            .ToList();

        var result = new PeriodeMargeAnalysisDto
        {
            PeriodeID = periodeId,
            DateDebut = periode.DateDebut,
            DateFin = periode.DateFin,
            Consommations = consommationDetails,
            ParProduit = parProduit,
            TotalQuantiteVendue = consommationDetails.Sum(c => c.QuantiteConsommee),
            TotalCoutAchat = consommationDetails.Sum(c => c.CoutAchat),
            TotalVente = consommationDetails.Sum(c => c.Vente)
        };

        _logger.LogInformation(
            "Periode {PeriodeId} marge analysis: {Count} consumptions, Marge={Marge:N2} ({Percent}%)",
            periodeId, consommationDetails.Count, result.TotalMarge, result.MargePercent);

        return result;
    }

    // ?????????????????????????????????????????????????????????????????
    // PRIVATE HELPERS
    // ?????????????????????????????????????????????????????????????????

    private static StockLotAnalysisDto BuildStockLotAnalysis(StockLot stockLot)
    {
        var quantiteVendue = stockLot.Consumptions.Sum(c => c.QuantiteConsommee);
        var chiffreAffaires = stockLot.Consumptions.Sum(c => c.QuantiteConsommee * c.PeriodeDetail.PrixCarburant);
        var coutRevient = stockLot.Consumptions.Sum(c => c.QuantiteConsommee * c.PrixUnitaire);

        var prixVenteMoyen = quantiteVendue > 0
            ? chiffreAffaires / quantiteVendue
            : 0;

        return new StockLotAnalysisDto
        {
            StockLotID = stockLot.ID,
            AchatID = stockLot.AchatID,
            ReservoirID = stockLot.ReservoirID,
            ProduitID = stockLot.ProduitID,
            Type = (int)stockLot.Type,
            ReservoirNumero = stockLot.Reservoir?.Numero,
            ProduitNom = stockLot.Produit?.Description,
            Statut = stockLot.Statut,
            DateEntree = stockLot.DateEntree,
            QuantiteInitiale = stockLot.QuantiteInitiale,
            QuantiteVendue = quantiteVendue,
            QuantiteDisponible = stockLot.QuantiteDisponible,
            PrixAchat = stockLot.PrixAchat,
            PrixVenteMoyen = Math.Round(prixVenteMoyen, 2),
            ChiffreAffaires = Math.Round(chiffreAffaires, 2),
            CoutRevient = Math.Round(coutRevient, 2),
            Notes = stockLot.Notes
        };
    }
}
