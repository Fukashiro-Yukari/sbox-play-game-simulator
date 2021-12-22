using Sandbox;
using System;

[Library( "ent_change_player_computer", Title = "Change Player Computer" )]
[Hammer.EditorModel( "models/Monitor/monitor.vmdl", FixedBounds = true )]
public class ChangePlayerComputer : Prop, IUse
{
	public override void Spawn()
	{
		base.Spawn();

		SetModel( "models/Monitor/monitor.vmdl" );
		SetupPhysicsFromModel( PhysicsMotionType.Dynamic, false );
	}

	public bool IsUsable( Entity user )
	{
		return user is RealPlayer ply && ply.Velocity.Length < 50;
	}

	public bool OnUse( Entity user )
	{
		if ( user is RealPlayer ply )
		{
			ply.ChangePlayer();
		}

		return false;
	}
}
