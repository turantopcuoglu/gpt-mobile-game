using UnityEngine;

namespace GermSweep3D
{
    public struct PlayerStats { public float moveSpeed; public float suctionRadius; public int suctionPower; public int capacity; }

    [RequireComponent(typeof(CharacterController))]
    public sealed class PlayerRobotController : MonoBehaviour
    {
        public event System.Action InventoryChanged;
        public PlayerStats Stats { get; private set; }
        public int CurrentWaste { get; private set; }
        public int CurrentMedical { get; private set; }
        public int CarriedAmount { get { return CurrentWaste + CurrentMedical; } }
        public bool InputEnabled { get; private set; }
        public float SpeedMultiplier { get; private set; } = 1f;
        public int TemporaryPowerBonus { get; private set; }
        public int EffectiveSuctionPower { get { return Stats.suctionPower + TemporaryPowerBonus; } }

        private CharacterController character;
        private PlayerInputController input;
        private float boostTimer;
        private float slowTimer;
        private Vector2 bounds = new Vector2(7.5f, 8.5f);

        public void Initialize(PlayerStats stats, Vector3 startPosition)
        {
            Stats = stats; CurrentWaste = 0; CurrentMedical = 0; TemporaryPowerBonus = 0; boostTimer = 0; slowTimer = 0; SpeedMultiplier = 1f;
            character = GetComponent<CharacterController>(); input = GetComponent<PlayerInputController>(); if (input == null) input = gameObject.AddComponent<PlayerInputController>();
            character.enabled = false; transform.position = startPosition + Vector3.up * .55f; character.enabled = true; SetInputEnabled(true); NotifyInventory();
        }
        public void SetInputEnabled(bool enabled) { InputEnabled = enabled; if (input != null) input.enabled = enabled; }
        public bool HasCapacityFor(CollectibleType type, int amount) { return type == CollectibleType.Coin || type == CollectibleType.SanitizerCrystal || CarriedAmount + amount <= Stats.capacity; }
        public void AddWaste(int amount) { CurrentWaste += amount; NotifyInventory(); }
        public void AddMedical(int amount) { CurrentMedical += amount; NotifyInventory(); }
        public int DrainWaste(int maxAmount) { int drained = Mathf.Min(CurrentWaste, maxAmount); CurrentWaste -= drained; if (drained > 0) NotifyInventory(); return drained; }
        public int DrainMedical(int maxAmount) { int drained = Mathf.Min(CurrentMedical, maxAmount); CurrentMedical -= drained; if (drained > 0) NotifyInventory(); return drained; }
        public void ApplySlow(float multiplier, float duration) { SpeedMultiplier = Mathf.Min(SpeedMultiplier, multiplier); slowTimer = Mathf.Max(slowTimer, duration); }
        public void ApplyPowerBoost(int bonus, float duration) { TemporaryPowerBonus = Mathf.Max(TemporaryPowerBonus, bonus); boostTimer = Mathf.Max(boostTimer, duration); }
        private void Update()
        {
            if (!InputEnabled || input == null || character == null) return;
            Vector2 drag = input.MoveVector;
            Vector3 move = new Vector3(drag.x, 0, drag.y);
            if (move.sqrMagnitude > 1f) move.Normalize();
            character.Move(move * (Stats.moveSpeed * SpeedMultiplier * Time.deltaTime));
            Vector3 pos = transform.position; pos.x = Mathf.Clamp(pos.x, -bounds.x, bounds.x); pos.z = Mathf.Clamp(pos.z, -bounds.y, bounds.y); transform.position = pos;
            if (move.sqrMagnitude > .001f) transform.forward = Vector3.Lerp(transform.forward, move.normalized, Time.deltaTime * 12f);
            if (boostTimer > 0 && (boostTimer -= Time.deltaTime) <= 0) TemporaryPowerBonus = 0;
            if (slowTimer > 0 && (slowTimer -= Time.deltaTime) <= 0) SpeedMultiplier = 1f;
        }
        private void NotifyInventory() { if (InventoryChanged != null) InventoryChanged(); }
    }

    public sealed class PlayerInputController : MonoBehaviour
    {
        public Vector2 MoveVector { get; private set; }
        private Vector2 dragStart;
        private bool dragging;
        private const float DragPixelsForFullSpeed = 120f;
        private void Update()
        {
            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);
                if (touch.phase == TouchPhase.Began) { dragging = true; dragStart = touch.position; }
                if (touch.phase == TouchPhase.Canceled || touch.phase == TouchPhase.Ended) { dragging = false; MoveVector = Vector2.zero; return; }
                if (dragging) MoveVector = Vector2.ClampMagnitude((touch.position - dragStart) / DragPixelsForFullSpeed, 1f);
                return;
            }
#if UNITY_EDITOR || UNITY_STANDALONE
            if (Input.GetMouseButtonDown(0)) { dragging = true; dragStart = Input.mousePosition; }
            if (Input.GetMouseButtonUp(0)) { dragging = false; MoveVector = Vector2.zero; }
            if (dragging) MoveVector = Vector2.ClampMagnitude(((Vector2)Input.mousePosition - dragStart) / DragPixelsForFullSpeed, 1f);
#endif
        }
    }

    public sealed class CameraFollow : MonoBehaviour
    {
        public Transform target;
        public Vector3 offset = new Vector3(0, 9, -8);
        public float followSpeed = 8f;
        private void LateUpdate()
        {
            if (target == null) return;
            transform.position = Vector3.Lerp(transform.position, target.position + offset, Time.deltaTime * followSpeed);
            transform.rotation = Quaternion.Euler(55, 0, 0);
        }
    }
}
