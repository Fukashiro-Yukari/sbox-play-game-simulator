using Sandbox;
using System;

public class SnowBall : Prop
{
	public override void Spawn()
	{
		base.Spawn();

		SetModel( "models/christmas/snowball.vmdl" );
		SetupPhysicsFromModel( PhysicsMotionType.Dynamic, false );
		Scale = Rand.Float( 1, 10 );
	}

	protected override void OnPhysicsCollision( CollisionEventData eventData )
	{
		base.OnPhysicsCollision( eventData );
		Delete();
	}
}
