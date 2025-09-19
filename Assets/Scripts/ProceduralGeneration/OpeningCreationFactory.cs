using UnityEngine;

public static class OpeningCreationFactory
{
    public static IOpeningCreationPlan CreateOpening(OpeningType type)
    {
        switch (type)
        {
            case OpeningType.Door: return new DoorOpeningStrategy();
            case OpeningType.Window: return new WindowOpeningStrategy();
            default:
                Debug.LogWarning($"No strategy found for {type}, defaulting to DoorStrategy");
                return new DoorOpeningStrategy();
        }
    }
}