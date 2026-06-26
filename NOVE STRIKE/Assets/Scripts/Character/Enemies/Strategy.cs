using UnityEngine;

public interface IMoveStrategy { void Move(Rigidbody rd, float speed, Vector3 direction); }

public interface IAttackStrategy { void ExecuteAttack(Transform firePoint); }