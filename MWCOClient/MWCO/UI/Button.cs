using System;
using System.Runtime.CompilerServices;
using System.Threading;
using MWCO.Utilities;
using UnityEngine;

namespace MWCO.UI
{
	public class Button : MonoBehaviour
	{
		public event Button.ClickAction OnButtonClick
		{
			[CompilerGenerated]
			add
			{
				Button.ClickAction clickAction = this.OnButtonClick;
				Button.ClickAction clickAction2;
				do
				{
					clickAction2 = clickAction;
					Button.ClickAction clickAction3 = (Button.ClickAction)Delegate.Combine(clickAction2, value);
					clickAction = Interlocked.CompareExchange<Button.ClickAction>(ref this.OnButtonClick, clickAction3, clickAction2);
				}
				while (clickAction != clickAction2);
			}
			[CompilerGenerated]
			remove
			{
				Button.ClickAction clickAction = this.OnButtonClick;
				Button.ClickAction clickAction2;
				do
				{
					clickAction2 = clickAction;
					Button.ClickAction clickAction3 = (Button.ClickAction)Delegate.Remove(clickAction2, value);
					clickAction = Interlocked.CompareExchange<Button.ClickAction>(ref this.OnButtonClick, clickAction3, clickAction2);
				}
				while (clickAction != clickAction2);
			}
		}

		private void Start()
		{
			this.button = ((base.transform.childCount > 0) ? base.transform.GetChild(0).gameObject : base.gameObject);
			this.originalScale = this.button.transform.localScale;
			this.hoverScale = new Vector3(this.originalScale.x - this.originalScale.x * 0.1f, this.originalScale.y - this.originalScale.y * 0.1f, this.originalScale.z - this.originalScale.z * 0.1f);
		}

		public Button SetText(string newText)
		{
			TextMesh[] componentsInChildren = base.gameObject.GetComponentsInChildren<TextMesh>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].text = newText;
			}
			return this;
		}

		public Button SetColliderBounds(Vector3 center, Vector3 size)
		{
			BoxCollider boxCollider = base.transform.gameObject.EnsureComponentExists<BoxCollider>();
			boxCollider.center = center;
			boxCollider.size = size;
			return this;
		}

		public Button OverrideScaling(Vector3 hover)
		{
			this.hoverScale = hover;
			return this;
		}

		private void OnMouseEnter()
		{
			this.OnPointerEnter();
		}

		private void OnMouseOver()
		{
			this.OnPointerHover();
		}

		private void OnMouseExit()
		{
			this.OnPointerExit();
		}

		private void OnMouseDown()
		{
			this.OnPointerClick();
		}

		protected virtual void OnPointerEnter()
		{
			this.button.transform.localScale = this.hoverScale;
		}

		protected virtual void OnPointerExit()
		{
			this.button.transform.localScale = this.originalScale;
		}

		protected virtual void OnPointerHover()
		{
		}

		protected virtual void OnPointerClick()
		{
			Button.ClickAction onButtonClick = this.OnButtonClick;
			if (onButtonClick == null)
			{
				return;
			}
			onButtonClick();
		}

		public Button()
		{
		}

		[CompilerGenerated]
		private Button.ClickAction OnButtonClick;

		private GameObject button;

		private Vector3 originalScale;

		private Vector3 hoverScale;

		public delegate void ClickAction();
	}
}
