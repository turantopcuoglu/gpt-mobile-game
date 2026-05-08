using System.Collections.Generic;
using UnityEngine;

namespace GermSweep3D
{
    public sealed class LevelDatabase
    {
        private readonly List<LevelDefinition> levels = new List<LevelDefinition>(5);
        public int Count { get { return levels.Count; } }
        public LevelDatabase() { BuildLevels(); }
        public LevelDefinition GetLevel(int index)
        {
            if (levels.Count == 0) BuildLevels();
            int wrapped = Mathf.Clamp(index, 1, levels.Count);
            return levels[wrapped - 1];
        }

        private void BuildLevels()
        {
            levels.Clear();
            LevelDefinition l1 = NewLevel(1, "First Cleanup", 90, 80, 1, new Vector3(0, 0, -6));
            AddGrid(l1.wasteSpawns, "GreenGerm", 12, -4, -3, 1, 1, 1);
            AddGrid(l1.medicalSpawns, "MedicalSupply", 8, 1, -3, 1, 1, 1);
            AddCoins(l1, 8); l1.patientSpawns.Add(new PatientSpawnData(new Vector3(0, 0, 5), PatientType.Fever, 5)); FinalizeLevel(l1); levels.Add(l1);

            LevelDefinition l2 = NewLevel(2, "Capacity Lesson", 100, 90, 2, new Vector3(0, 0, -7));
            AddGrid(l2.wasteSpawns, "GreenGerm", 20, -5, -4, 1, 1, 1); AddGrid(l2.wasteSpawns, "Stain", 5, -4, 1, 2, 1, 1);
            AddGrid(l2.medicalSpawns, "MedicalSupply", 14, 1, -4, 1, 1, 1); AddCoins(l2, 10);
            l2.patientSpawns.Add(new PatientSpawnData(new Vector3(-2, 0, 5), PatientType.Fever, 5)); l2.patientSpawns.Add(new PatientSpawnData(new Vector3(2, 0, 5), PatientType.Fever, 5)); FinalizeLevel(l2); levels.Add(l2);

            LevelDefinition l3 = NewLevel(3, "Red Germs", 110, 90, 2, new Vector3(0, 0, -7));
            AddGrid(l3.wasteSpawns, "GreenGerm", 16, -5, -4, 1, 1, 1); AddGrid(l3.wasteSpawns, "RedGerm", 6, 1, -2, 1, 1, 2);
            AddGrid(l3.medicalSpawns, "MedicalSupply", 16, -1, 1, 1, 1, 1); AddCoins(l3, 12);
            l3.patientSpawns.Add(new PatientSpawnData(new Vector3(-2, 0, 5), PatientType.Fever, 6)); l3.patientSpawns.Add(new PatientSpawnData(new Vector3(2, 0, 5), PatientType.Fever, 6));
            l3.hazardSpawns.Add(new HazardSpawnData("WetFloorHazard", new Vector3(-3, 0, 0))); l3.hazardSpawns.Add(new HazardSpawnData("SanitizerBoost", new Vector3(3, 0, -1))); FinalizeLevel(l3); levels.Add(l3);

            LevelDefinition l4 = NewLevel(4, "Ambulance Emergency", 120, 95, 2, new Vector3(0, 0, -7));
            AddGrid(l4.wasteSpawns, "GreenGerm", 20, -5, -4, 1, 1, 1); AddGrid(l4.wasteSpawns, "YellowGerm", 8, 1, -4, 1, 1, 1); AddGrid(l4.wasteSpawns, "RedGerm", 4, -1, 1, 1, 1, 2);
            AddGrid(l4.medicalSpawns, "MedicalSupply", 24, -5, 2, 1, 1, 1); AddCoins(l4, 14);
            l4.patientSpawns.Add(new PatientSpawnData(new Vector3(-2, 0, 5), PatientType.Critical, 10, 60)); l4.patientSpawns.Add(new PatientSpawnData(new Vector3(2, 0, 5), PatientType.Fever, 5));
            l4.hazardSpawns.Add(new HazardSpawnData("MovingGermCloud", new Vector3(0, 0, 0))); FinalizeLevel(l4); levels.Add(l4);

            LevelDefinition l5 = NewLevel(5, "Giant Virus Boss", 150, 100, 1, new Vector3(0, 0, -7));
            AddGrid(l5.medicalSpawns, "MedicalSupply", 30, -5, -4, 1, 1, 1); AddGrid(l5.wasteSpawns, "SanitizerCrystal", 15, -5, 0, 1, 1, 1); AddCoins(l5, 18);
            l5.patientSpawns.Add(new PatientSpawnData(new Vector3(0, 0, 5), PatientType.Critical, 12, 90));
            l5.hasBoss = true; l5.bossConfig = new BossConfig { bossKey = "GiantVirusBoss", health = 3, crystalsPerDamage = 5, spawnInterval = 10, position = new Vector3(0, 0, 2) }; FinalizeLevel(l5); levels.Add(l5);
        }

        private static LevelDefinition NewLevel(int index, string name, float time, float clean, int patients, Vector3 start)
        { return new LevelDefinition { levelIndex = index, displayName = name, timeLimit = time, requiredCleanPercent = clean, requiredPatientsHealed = patients, playerStartPosition = start, baseCoinReward = 75 + index * 25 }; }
        private static void AddGrid(List<SpawnData> list, string key, int count, float x0, float z0, float dx, float dz, int power)
        { for (int i = 0; i < count; i++) list.Add(new SpawnData(key, new Vector3(x0 + (i % 6) * dx, 0, z0 + (i / 6) * dz), 1, power)); }
        private static void AddCoins(LevelDefinition level, int count) { AddGrid(level.coinSpawns, "Coin", count, -4, 6, 1, .6f, 1); }
        private static void FinalizeLevel(LevelDefinition level)
        { level.totalCleanUnits = 0; for (int i = 0; i < level.wasteSpawns.Count; i++) if (level.wasteSpawns[i].prefabKey != "SanitizerCrystal") level.totalCleanUnits += Mathf.Max(1, level.wasteSpawns[i].amountValue); }
    }
}
