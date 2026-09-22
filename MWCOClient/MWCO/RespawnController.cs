using System;
using MWCO.Game.Objects;
using MWCO.Utilities;
using UnityEngine;

namespace MWCO
{
	public class RespawnController : MonoBehaviour
	{
		private void OnEnable()
		{
			if (this.crouchFsm == null)
			{
				foreach (PlayMakerFSM playMakerFSM in MPController.Instance.boat.GetComponentsInChildren<PlayMakerFSM>(true))
				{
					if (playMakerFSM.gameObject.name == "PLAYER" && playMakerFSM.FsmName == "Crouch")
					{
						this.crouchFsm = playMakerFSM;
						break;
					}
				}
			}
			if (GamePlayer.Instance.Object.transform.parent != null)
			{
				PlayerVehicle.MPCarController component = GamePlayer.Instance.Object.GetTopmostParent().gameObject.GetComponent<PlayerVehicle.MPCarController>();
				if (component != null)
				{
					try
					{
						((PlayerVehicle)component.osc.syncedObject).ForceLeaveCar();
					}
					catch (Exception ex)
					{
						string text = "Error retrieving pv ";
						Exception ex2 = ex;
						Logger.Debug(text + ((ex2 != null) ? ex2.ToString() : null));
					}
				}
			}
			GamePlayer.Instance.characterMotor.canControl = false;
		}

		private void Update()
		{
			GamePlayer.Instance.mouseLook.enabled = false;
			if (Input.GetKeyDown(13))
			{
				if (GamePlayer.Instance.Object.transform.parent != null)
				{
					PlayerVehicle.MPCarController component = GamePlayer.Instance.Object.GetTopmostParent().gameObject.GetComponent<PlayerVehicle.MPCarController>();
					if (component != null)
					{
						try
						{
							((PlayerVehicle)component.osc.syncedObject).ForceLeaveCar();
						}
						catch (Exception ex)
						{
							string text = "Error retrieving pv ";
							Exception ex2 = ex;
							Logger.Debug(text + ((ex2 != null) ? ex2.ToString() : null));
						}
					}
				}
				GamePlayer.Instance.characterMotor.canControl = true;
				GamePlayer.Instance.characterMotor.enabled = true;
				GamePlayer.Instance.Object.GetComponent<CharacterController>().enabled = true;
				GamePlayer.Instance.Object.GetComponent<CharacterMotor>().enabled = true;
				GamePlayer.Instance.Object.GetComponent<FPSInputController>().enabled = true;
				GamePlayer.Instance.mouseLook.enabled = true;
				GamePlayer.Instance.Object.transform.position = new Vector3(-1430.44f, 5.7f, 1152f);
				GamePlayer.Instance.ResetAttributes();
				this.crouchFsm.Fsm.GetFsmBool("PlayerInCar").Value = false;
				base.gameObject.SetActive(false);
			}
		}

		private void OnGUI()
		{
			GUI.color = new Color(1f, 0f, 0f, 0.5f);
			GUI.DrawTexture(new Rect(0f, 0f, (float)Screen.width, (float)Screen.height), Texture2D.whiteTexture);
			GUI.color = Color.white;
			GUIStyle guistyle = new GUIStyle(GUI.skin.label);
			guistyle.alignment = 4;
			guistyle.fontSize = 40;
			guistyle.normal.textColor = Color.white;
			GUI.Label(new Rect(0f, (float)(Screen.height / 2) - 50f, (float)Screen.width, 100f), "YOU DIED! PRESS [ENTER] TO RESPAWN", guistyle);
			guistyle.fontSize = 24;
			guistyle.normal.textColor = Color.yellow;
			GUI.Label(new Rect(0f, (float)(Screen.height / 2) - 12f, (float)Screen.width, 100f), "You will be charged between 5-20mk", guistyle);
		}

		public RespawnController()
		{
		}

		private PlayMakerFSM crouchFsm;
	}
}
