using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RangeGravity : MonoBehaviour
{
	[SerializeField]
	private float m_gravityForce;

	[SerializeField]
	private AnimationCurve m_forceCurve;
	
	[SerializeField]
	private ContactFilter2D m_triggerFilter;

	[SerializeField]
	private Vector2 m_centerOffset;

	[SerializeField]
	private float m_radius;

	private List<Collider2D> m_triggerBuffer = new();

	public Vector2 GravityCenter
	{
		get => this.transform.TransformPoint(m_centerOffset);
	}

	public float GravityForce { get => m_gravityForce; set => m_gravityForce = value; }

	public bool ApplyGravity(Rigidbody2D rb)
	{
		if (rb.bodyType != RigidbodyType2D.Dynamic) return false;

		var deltaPos = this.GravityCenter - rb.position;

		float dist = deltaPos.magnitude;
		var dir = deltaPos.normalized;

		float rate = 1 - Mathf.Clamp01(dist / m_radius);

		var force = m_gravityForce * m_forceCurve.Evaluate(rate);
		rb.AddForce(force * dir, ForceMode2D.Force);

#if UNITY_EDITOR
		DebugUtility.DrawMark(rb.position, 0.1f, Color.green);
		DebugUtility.DrawArrow(rb.position, force * dir, 0.1f, color: Color.grey);
		DebugUtility.DrawArrow(rb.position, rb.velocity, 0.1f, color: Color.white);
#endif

		return true;
	}

	private void Update()
	{
		int count = Physics2D.OverlapCircle(this.GravityCenter, m_radius, m_triggerFilter, m_triggerBuffer);
		if (count > 0)
		{
			foreach (var c in m_triggerBuffer)
			{
				var rb = c.attachedRigidbody;

				this.ApplyGravity(rb);
			}
			m_triggerBuffer.Clear();
		}
	}

#if UNITY_EDITOR
	private void Reset()
	{
		m_forceCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
	}
	private void OnDrawGizmosSelected()
	{
		DebugUtility.DrawCircle(this.GravityCenter, m_radius, 64, color: Color.green);
	}
#endif
}
