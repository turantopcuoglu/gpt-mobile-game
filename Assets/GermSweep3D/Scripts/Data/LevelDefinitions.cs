using System;
using System.Collections.Generic;
using UnityEngine;

namespace GermSweep3D
{
    public enum CollectibleType { Waste, Medical, Coin, SanitizerCrystal }
    public enum PatientType { Fever, Critical }
    public enum PatientState { Waiting, Treating, Healed, Failed }
    public enum GameState { Boot, Home, Gameplay, Pause, Win, Lose }
    public enum UpgradeType { VacuumPower, Capacity, MoveSpeed, SuctionRadius }

    [Serializable]
    public struct SpawnData
    {
        public string prefabKey;
        public Vector3 position;
        public int amountValue;
        public int requiredVacuumPower;
        public SpawnData(string prefabKey, Vector3 position, int amountValue, int requiredVacuumPower)
        {
            this.prefabKey = prefabKey; this.position = position; this.amountValue = amountValue; this.requiredVacuumPower = requiredVacuumPower;
        }
    }

    [Serializable]
    public struct PatientSpawnData
    {
        public Vector3 position;
        public PatientType patientType;
        public int requiredMedicalAmount;
        public float optionalCriticalTimer;
        public PatientSpawnData(Vector3 position, PatientType patientType, int requiredMedicalAmount, float optionalCriticalTimer = 0f)
        {
            this.position = position; this.patientType = patientType; this.requiredMedicalAmount = requiredMedicalAmount; this.optionalCriticalTimer = optionalCriticalTimer;
        }
    }

    [Serializable]
    public struct HazardSpawnData
    {
        public string hazardKey;
        public Vector3 position;
        public HazardSpawnData(string hazardKey, Vector3 position) { this.hazardKey = hazardKey; this.position = position; }
    }

    [Serializable]
    public struct BossConfig
    {
        public string bossKey;
        public int health;
        public int crystalsPerDamage;
        public float spawnInterval;
        public Vector3 position;
    }

    [Serializable]
    public sealed class LevelDefinition
    {
        public int levelIndex;
        public string displayName;
        public float timeLimit;
        public float requiredCleanPercent;
        public int requiredPatientsHealed;
        public Vector3 playerStartPosition;
        public readonly List<SpawnData> wasteSpawns = new List<SpawnData>();
        public readonly List<SpawnData> medicalSpawns = new List<SpawnData>();
        public readonly List<SpawnData> coinSpawns = new List<SpawnData>();
        public readonly List<PatientSpawnData> patientSpawns = new List<PatientSpawnData>();
        public readonly List<HazardSpawnData> hazardSpawns = new List<HazardSpawnData>();
        public bool hasBoss;
        public BossConfig bossConfig;
        public int baseCoinReward = 100;
        public int totalCleanUnits;
    }
}
