using Sandbox;
using System;

public class SnowMan : Prop
{
	public override void Spawn()
	{
		base.Spawn();

		SetModel( "models/christmas/snowman.vmdl" );
		SetupPhysicsFromModel( PhysicsMotionType.Dynamic, false );
		Scale = 3;
	}

	protected override void OnPhysicsCollision( CollisionEventData eventData )
	{
		base.OnPhysicsCollision( eventData );
		Delete();
	}
}
