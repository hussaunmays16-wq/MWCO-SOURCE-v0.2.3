using System;

namespace MWCO.Game
{
	internal interface IObjectSubtype
	{
		float[] ReturnSyncedVariables();

		void HandleSyncedVariables(float[] variables);

		bool CanSync();
	}
}
