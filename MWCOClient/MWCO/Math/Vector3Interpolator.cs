using System;
using UnityEngine;

namespace MWCO.Math
{
	internal class Vector3Interpolator
	{
		public Vector3 Current
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

		public void SetTarget(Vector3 vec)
		{
			this.source = this.current;
			this.target = vec;
			this.Evaluate(0f);
		}

		public void Teleport(Vector3 vec)
		{
			this.target = vec;
			this.source = vec;
			this.current = vec;
		}

		public Vector3 Evaluate(float alpha)
		{
			this.current = Vector3.Lerp(this.source, this.target, alpha);
			return this.current;
		}

		public Vector3Interpolator()
		{
		}

		private Vector3 current;

		private Vector3 source;

		private Vector3 target;
	}
}
