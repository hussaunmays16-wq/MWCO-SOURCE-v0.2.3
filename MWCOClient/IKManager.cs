using System;
using MWCO.Utilities;
using UnityEngine;

public class IKManager : MonoBehaviour
{
	private void Awake()
	{
		this.upperArm_Length = 0f;
		this.forearm_Length = 0f;
		this.arm_Length = 0f;
		this.length_Offset = 0.05f;
	}

	private void LateUpdate()
	{
		try
		{
			if (this.target == null)
			{
				base.enabled = false;
			}
			else if (this.upperArm != null && this.forearm != null && this.hand != null && this.elbow != null && this.target != null)
			{
				this.upperArm.LookAt(this.target, this.elbow.position - this.upperArm.position);
				this.upperArm.Rotate(this.uppperArm_OffsetRotation);
				Vector3 vector = Vector3.Cross(this.elbow.position - this.upperArm.position, this.forearm.position - this.upperArm.position);
				this.upperArm_Length = Vector3.Distance(this.upperArm.position, this.forearm.position);
				this.forearm_Length = Vector3.Distance(this.forearm.position, this.hand.position) + this.length_Offset;
				this.arm_Length = this.upperArm_Length + this.forearm_Length;
				this.targetDistance = Vector3.Distance(this.upperArm.position, this.target.position);
				this.targetDistance = Mathf.Max(this.targetDistance, 0.001f);
				this.targetDistance = Mathf.Min(this.targetDistance, this.arm_Length - this.arm_Length * 0.001f);
				this.adyacent = Mathf.Clamp((this.upperArm_Length * this.upperArm_Length - this.forearm_Length * this.forearm_Length + this.targetDistance * this.targetDistance) / (2f * this.targetDistance), -this.upperArm_Length, this.upperArm_Length);
				this.angle = Mathf.Acos(this.adyacent / this.upperArm_Length) * 57.29578f;
				this.upperArm.RotateAround(this.upperArm.position, vector, -this.angle);
				this.forearm.LookAt(this.target, vector);
				this.forearm.Rotate(this.forearm_OffsetRotation);
				if (this.handMatchesTargetRotation)
				{
					this.hand.rotation = this.target.rotation;
					this.hand.Rotate(this.hand_OffsetRotation);
				}
			}
		}
		catch (Exception ex)
		{
			Logger.Debug(string.Format("FAILED IK IN LATEMANAGER: {0}", ex));
		}
	}

	public IKManager()
	{
	}

	public Transform upperArm;

	public Transform forearm;

	public Transform hand;

	public Transform elbow;

	public Transform target;

	public Vector3 uppperArm_OffsetRotation;

	public Vector3 forearm_OffsetRotation;

	public Vector3 hand_OffsetRotation;

	public bool handMatchesTargetRotation;

	public float angle;

	public float upperArm_Length;

	public float forearm_Length;

	public float arm_Length;

	public float length_Offset = 0.04f;

	public float targetDistance;

	public float adyacent;
}
