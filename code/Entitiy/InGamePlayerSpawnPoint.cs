namespace Sandbox
{
	/// <summary>
	/// This entity defines the spawn point of the player in first person shooter gamemodes.
	/// </summary>
	[Library( "info_player_ingame" )]
	[Hammer.EditorModel( "models/editor/playerstart.vmdl", FixedBounds = true )]
	[Hammer.EntityTool( "In Game Player Spawnpoint", "Player", "Defines a point where the player can (re)spawn" )]
	public class InGamePlayerSpawnPoint : Entity
	{

	}
}
