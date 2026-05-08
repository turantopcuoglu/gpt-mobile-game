using System.Collections;
using UnityEngine;

namespace GermSweep3D
{
    public sealed class CollectibleItem : MonoBehaviour
    {
        public string prefabKey;
        public CollectibleType type;
        public int amountValue = 1;
        public int requiredVacuumPower = 1;
        public bool IsBeingPulled { get; private set; }
        private LevelManager level;
        private Renderer cachedRenderer;
        private Vector3 originalScale;
        public void Initialize(LevelManager levelManager, string key, CollectibleType itemType, int amount, int requiredPower)
        { level = levelManager; prefabKey = key; type = itemType; amountValue = amount; requiredVacuumPower = requiredPower; IsBeingPulled = false; originalScale = transform.localScale; if (cachedRenderer == null) cachedRenderer = GetComponentInChildren<Renderer>(); gameObject.SetActive(true); }
        public bool CanCollect(PlayerRobotController player) { return player.EffectiveSuctionPower >= requiredVacuumPower && player.HasCapacityFor(type, amountValue); }
        public void ShowTooStrongFeedback() { StopAllCoroutines(); StartCoroutine(Flash(Color.magenta)); }
        public void BeginPull(PlayerRobotController player) { if (!IsBeingPulled) StartCoroutine(PullRoutine(player)); }
        private IEnumerator PullRoutine(PlayerRobotController player)
        {
            IsBeingPulled = true;
            while (player != null && Vector3.Distance(transform.position, player.transform.position) > .35f)
            { transform.position = Vector3.MoveTowards(transform.position, player.transform.position, 8f * Time.deltaTime); yield return null; }
            if (player != null) level.CollectItem(this, player); IsBeingPulled = false; gameObject.SetActive(false);
        }
        private IEnumerator Flash(Color color)
        { if (cachedRenderer == null) yield break; Color old = cachedRenderer.material.color; cachedRenderer.material.color = color; transform.localScale = originalScale * 1.18f; yield return new WaitForSeconds(.12f); cachedRenderer.material.color = old; transform.localScale = originalScale; }
    }

    public sealed class VacuumCollector : MonoBehaviour
    {
        public LayerMask collectibleMask = ~0;
        public float scanInterval = .12f;
        private readonly Collider[] hits = new Collider[48];
        private PlayerRobotController player;
        private float nextScan;
        public void Initialize(PlayerRobotController robot) { player = robot; }
        private void Update()
        {
            if (player == null || !player.InputEnabled || Time.time < nextScan) return;
            nextScan = Time.time + scanInterval;
            int count = Physics.OverlapSphereNonAlloc(transform.position, player.Stats.suctionRadius, hits, collectibleMask, QueryTriggerInteraction.Collide);
            for (int i = 0; i < count; i++)
            {
                CollectibleItem item = hits[i].GetComponentInParent<CollectibleItem>();
                if (item == null || item.IsBeingPulled || !item.gameObject.activeInHierarchy) continue;
                if (item.CanCollect(player)) item.BeginPull(player); else item.ShowTooStrongFeedback();
            }
        }
    }

    public sealed class BioWasteBin : MonoBehaviour
    {
        public float drainInterval = .25f;
        public int drainAmount = 1;
        private LevelManager level;
        private PlayerRobotController playerInside;
        private float nextDrain;
        public void Initialize(LevelManager manager) { level = manager; }
        private void OnTriggerEnter(Collider other) { playerInside = other.GetComponent<PlayerRobotController>(); }
        private void OnTriggerExit(Collider other) { if (other.GetComponent<PlayerRobotController>() == playerInside) playerInside = null; }
        private void Update()
        {
            if (playerInside == null || Time.time < nextDrain) return;
            nextDrain = Time.time + drainInterval;
            int drained = playerInside.DrainWaste(drainAmount); if (drained > 0) level.DeliverWaste(drained);
        }
    }

    public sealed class TreatmentStation : MonoBehaviour
    {
        public int AvailableMedicalSupply { get; private set; }
        public float drainInterval = .25f;
        private LevelManager level;
        private PlayerRobotController playerInside;
        private float nextDrain;
        public void Initialize(LevelManager manager) { level = manager; AvailableMedicalSupply = 0; }
        private void OnTriggerEnter(Collider other) { playerInside = other.GetComponent<PlayerRobotController>(); }
        private void OnTriggerExit(Collider other) { if (other.GetComponent<PlayerRobotController>() == playerInside) playerInside = null; }
        private void Update()
        {
            if (playerInside == null || Time.time < nextDrain) return;
            nextDrain = Time.time + drainInterval;
            int drained = playerInside.DrainMedical(1);
            if (drained > 0) { AvailableMedicalSupply += drained; level.TryHealPatients(this); }
        }
        public bool TryConsume(int amount) { if (AvailableMedicalSupply < amount) return false; AvailableMedicalSupply -= amount; return true; }
    }

    public sealed class Patient : MonoBehaviour
    {
        public PatientType patientType;
        public PatientState State { get; private set; }
        public int requiredMedicalAmount;
        public float optionalCriticalTimer;
        private LevelManager level;
        private Renderer cachedRenderer;
        public void Initialize(LevelManager manager, PatientSpawnData data)
        { level = manager; patientType = data.patientType; requiredMedicalAmount = data.requiredMedicalAmount; optionalCriticalTimer = data.optionalCriticalTimer; State = PatientState.Waiting; cachedRenderer = GetComponentInChildren<Renderer>(); SetColor(patientType == PatientType.Critical ? Color.red : Color.cyan); }
        private void Update()
        { if (State == PatientState.Waiting && optionalCriticalTimer > 0 && (optionalCriticalTimer -= Time.deltaTime) <= 0) { State = PatientState.Failed; SetColor(Color.black); level.FailLevel("Critical patient failed"); } }
        public bool TryHeal(TreatmentStation station)
        {
            if (State != PatientState.Waiting || !station.TryConsume(requiredMedicalAmount)) return false;
            State = PatientState.Healed; SetColor(Color.green); level.PatientHealed(this); return true;
        }
        private void SetColor(Color color) { if (cachedRenderer != null) cachedRenderer.material.color = color; }
    }

    public sealed class Stain : MonoBehaviour
    {
        public float cleanDuration = 2f;
        public int cleanValue = 1;
        private float progress;
        private LevelManager level;
        private Vector3 initialScale;
        public void Initialize(LevelManager manager, int value) { level = manager; cleanValue = value; progress = 0; initialScale = transform.localScale; }
        private void OnTriggerStay(Collider other)
        {
            if (other.GetComponent<PlayerRobotController>() == null) return;
            progress += Time.deltaTime;
            transform.localScale = Vector3.Lerp(initialScale, initialScale * .2f, progress / cleanDuration);
            if (progress >= cleanDuration) { level.DeliverWaste(cleanValue); gameObject.SetActive(false); }
        }
    }

    public sealed class HazardZone : MonoBehaviour
    {
        public string hazardKey;
        public float slowMultiplier = .55f;
        public float slowDuration = .25f;
        public float moveAmplitude = 0f;
        private Vector3 start;
        private void Awake() { start = transform.position; }
        private void Update() { if (moveAmplitude > 0) transform.position = start + Vector3.right * Mathf.Sin(Time.time) * moveAmplitude; }
        private void OnTriggerStay(Collider other) { PlayerRobotController player = other.GetComponent<PlayerRobotController>(); if (player != null) player.ApplySlow(slowMultiplier, slowDuration); }
    }

    public sealed class PowerUp : MonoBehaviour
    {
        public int powerBonus = 1;
        public float duration = 10f;
        private void OnTriggerEnter(Collider other) { PlayerRobotController player = other.GetComponent<PlayerRobotController>(); if (player != null) { player.ApplyPowerBoost(powerBonus, duration); gameObject.SetActive(false); } }
    }

    public sealed class GiantVirusBoss : MonoBehaviour
    {
        public int Health { get; private set; }
        private LevelManager level;
        private float spawnTimer;
        private float spawnInterval;
        public void Initialize(LevelManager manager, BossConfig config) { level = manager; Health = config.health; spawnInterval = config.spawnInterval; spawnTimer = spawnInterval; }
        public void Damage(int amount)
        {
            Health -= amount; transform.localScale = Vector3.one * (1.6f + Health * .15f);
            if (Health <= 0) { level.BossDefeated(); gameObject.SetActive(false); }
        }
        private void Update() { if (level == null || Health <= 0) return; spawnTimer -= Time.deltaTime; if (spawnTimer <= 0) { spawnTimer = spawnInterval; level.SpawnBossGerm(transform.position + Random.insideUnitSphere * 2f); } }
    }
}
