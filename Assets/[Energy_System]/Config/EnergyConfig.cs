using UnityEngine;

[CreateAssetMenu(fileName = "EnergyConfig", menuName = "Configs/EnergyConfig")]
public class EnergyConfig : ScriptableObject
{
    public int MaxEnergy = 100;
    public int InitialEnergy = 100;
    public int RegenAmountPerTick = 1;
    public float RegenIntervalSeconds = 60f;

    public string EnergySaveKey = "PlayerEnergy";
    public string TimeStampSaveKey = "EnergyTimestamp";
}
