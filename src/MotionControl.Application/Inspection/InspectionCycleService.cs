using MotionControl.Domain;

namespace MotionControl.Application;

internal sealed class InspectionCycleService
{
    private readonly MachineControlContext _context;
    private readonly RecipeValidator _recipeValidator;

    public InspectionCycleService(
        MachineControlContext context,
        RecipeValidator recipeValidator)
    {
        _context = context;
        _recipeValidator = recipeValidator;
    }

    public async Task<CycleReport> RunAsync(
        InspectionRecipe recipe,
        CancellationToken cancellationToken)
    {
        _context.EnsureState(MachineState.Ready);

        var errors = _recipeValidator.Validate(recipe);
        if (errors.Count > 0)
        {
            throw new MotionControlException(
                FaultCode.InvalidRecipe,
                string.Join(" ", errors));
        }

        var startedAt = _context.Clock.UtcNow;
        var cycleId = Guid.NewGuid();
        var measurements = new List<MeasurementResult>();

        try
        {
            await _context.ValidatePermissivesAsync(
                requirePart: true,
                cancellationToken);

            await _context.TransitionAsync(
                MachineState.Automatic,
                $"Inspection cycle {cycleId} started.",
                cancellationToken);

            await _context.UpdatePlcOutputsAsync(cancellationToken);

            foreach (var point in recipe.InspectionPoints)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var abovePoint = new AxisVector(
                    point.XMillimeters,
                    point.YMillimeters,
                    recipe.SafeZMillimeters);

                await _context.MotionController.MoveAbsoluteAsync(
                    abovePoint,
                    recipe.TravelVelocityMillimetersPerSecond,
                    cancellationToken);

                var measuredZ =
                    await _context.MotionController.ProbeZAsync(
                        targetZMillimeters: 0,
                        recipe.ProbeVelocityMillimetersPerSecond,
                        cancellationToken);

                var measurement = new MeasurementResult(
                    point.Name,
                    point.NominalZMillimeters,
                    measuredZ,
                    point.LowerLimitMillimeters,
                    point.UpperLimitMillimeters);

                measurements.Add(measurement);

                await _context.MotionController.MoveAbsoluteAsync(
                    abovePoint,
                    recipe.TravelVelocityMillimetersPerSecond,
                    cancellationToken);

                await _context.TryWriteEventAsync(
                    "Information",
                    "InspectionPointMeasured",
                    new
                    {
                        cycleId,
                        point = point.Name,
                        measuredZ,
                        measurement.Passed,
                    },
                    cancellationToken);
            }

            var report = new CycleReport(
                cycleId,
                recipe.RecipeId,
                recipe.Version,
                recipe.PartNumber,
                startedAt,
                _context.Clock.UtcNow,
                measurements);

            await _context.OperationsStore.SaveCycleAsync(
                report,
                cancellationToken);

            await _context.TransitionAsync(
                MachineState.Ready,
                $"Inspection cycle {cycleId} completed.",
                cancellationToken);

            await _context.UpdatePlcOutputsAsync(cancellationToken);
            return report;
        }
        catch (OperationCanceledException)
        {
            await _context.TryStopMotionAsync(CancellationToken.None);
            await _context.EnterFaultAsync(
                new MotionControlException(
                    FaultCode.OperationCancelled,
                    "The active inspection cycle was cancelled."),
                CancellationToken.None);
            throw;
        }
        catch (Exception exception)
        {
            await _context.TryStopMotionAsync(CancellationToken.None);
            await _context.EnterFaultAsync(
                MachineControlContext.ConvertException(exception),
                CancellationToken.None);
            throw;
        }
    }
}
