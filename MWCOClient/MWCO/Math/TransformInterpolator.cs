using System;
using UnityEngine;

namespace MWCO.Math
{
	public class TransformInterpolator
	{
		public Vector3 CurrentPosition
		{
			get
			{
				return this.position.Current;
			}
			set
			{
				this.position.Current = value;
			}
		}

		public Quaternion CurrentRotation
		{
			get
			{
				return this.rotation.Current;
			}
			set
			{
				this.rotation.Current = value;
			}
		}

		public void Teleport(Vector3 pos, Quaternion rot)
		{
			this.position.Teleport(pos);
			this.rotation.Teleport(rot);
		}

		public void SetTarget(Vector3 pos, Quaternion rot)
		{
			this.position.SetTarget(pos);
			this.rotation.SetTarget(rot);
		}

		public void Evaluate(ref Vector3 pos, ref Quaternion rot, float alpha)
		{
			pos = this.position.Evaluate(alpha);
			rot = this.rotation.Evaluate(alpha);
		}

		public void Evaluate(float alpha)
		{
			this.position.Evaluate(alpha);
			this.rotation.Evaluate(alpha);
		}

		public TransformInterpolator()
		{
		}

		private QuaternionInterpolator rotation = new QuaternionInterpolator();

		private Vector3Interpolator position = new Vector3Interpolator();
	}
}
