using System;

namespace MWCO.Network.Messages
{
	public enum MessageIds
	{
		Handshake,
		Heartbeat,
		HeartbeatResponse,
		Disconnect,
		PlayerSync,
		VehicleState,
		OpenDoors,
		FullWorldSync,
		AskForWorldState,
		VehicleEnter,
		VehicleLeave,
		PickupableSpawn,
		PickupableDestroy,
		PickupableActivate,
		PickupableSetPositionMessage,
		WorldPeriodicalUpdate,
		LightSwitch,
		WeatherSync,
		AnimSync,
		VehicleSwitch,
		ObjectSync,
		ObjectSyncResponse,
		EventHookSync,
		RequestObjectSync,
		HouseSwitch,
		FSMValueChange,
		Satsuma,
		Chat,
		Save,
		OrderList,
		Mod
	}
}
