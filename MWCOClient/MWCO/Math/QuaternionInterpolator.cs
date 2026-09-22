using System;
using UnityEngine;

namespace MWCO.Math
{
	internal class QuaternionInterpolator
	{
		public Quaternion Current
		{
			get
			{
				return this.current;
			}
			set
			{
				this.current = value;
			}
		}

		public void SetTarget(Quaternion quat)
		{
			this.source = this.current;
			this.target = quat;
			this.Evaluate(0f);
		}

		public void Teleport(Quaternion quat)
		{
			this.target = quat;
			this.source = quat;
			this.current = quat;
		}

		public Quaternion Evaluate(float alpha)
		{
			this.current = Quaternion.Slerp(this.source, this.target, alpha);
			return this.current;
		}

		public QuaternionInterpolator()
		{
		}

		private Quaternion current;

		private Quaternion source;

		private Quaternion target;
	}
}
