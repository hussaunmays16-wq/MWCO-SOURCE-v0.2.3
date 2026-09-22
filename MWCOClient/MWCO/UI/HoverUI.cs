using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace MWCO.UI
{
	public class HoverUI : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler
	{
		private void OnEnable()
		{
			base.transform.GetChild(0).gameObject.SetActive(false);
		}

		private void OnDisable()
		{
			base.transform.GetChild(0).gameObject.SetActive(false);
		}

		public void OnPointerEnter(PointerEventData eventData)
		{
			base.transform.GetChild(0).gameObject.SetActive(true);
		}

		public void OnPointerExit(PointerEventData eventData)
		{
			base.transform.GetChild(0).gameObject.SetActive(false);
		}

		public HoverUI()
		{
		}

		public bool Underline = true;
	}
}
