using Sandbox;
using System;
using System.Linq;

public class NPCPolice : NPC
{
	public override float Speed => 315;
	public override bool HaveDress => false;

	public RealPlayer Target { get; set; }
	public RealPlayer Victim { get; set; }
	public int Money { get; set; }

	private TimeSince timeSinceDelete = 0;

	bool dontMove;

	public override void Spawn()
	{
		base.Spawn();

		SetMaterialGroup( 4 );

		AddClothing( "models/citizen_clothes/hat/hat_leathercap.vmdl" );
		AddClothing( "models/citizen_clothes/shirt/shirt_longsleeve.police.vmdl" );
		AddClothing( "models/citizen_clothes/trousers/trousers.police.vmdl" );
		AddClothing( "models/citizen_clothes/shoes/shoes.police.vmdl" );

		Game.Current?.MoveToSpawnpoint( this );
		timeSinceDelete = 0;
	}

	public override void OnTick()
	{
		if ( timeSinceDelete > 15 )
		{
			DeleteAsync( 5f );
			dontMove = true;
		}

		if ( Target.IsValid() && Target.LifeState == LifeState.Alive && !dontMove )
		{
			Steer = new NavSteer();
			Steer.Target = Target.Position;
			Steer.DontAvoidance = e => true;
		}
	}

	public override void DoMeleeStrike()
	{
		if ( Target == null ) return;
		if ( Target.LifeState != LifeState.Alive )
		{
			if ( Target.LastAttacker == this && !dontMove )
			{
				Target.Money -= Money;
				Victim.Money += Money;

				var money = Math.Min( Target.Money, 200 );

				Target.Money -= money;

				MoneyFeed.OnGiveMoney( To.Single( Victim ), Money );
				MoneyFeed.OnLossMoney( To.Single( Target ), Money + money );
			}

			DeleteAsync( 5f );
			dontMove = true;
		}

		if ( Target.Position.Distance( Position ) < 100 && !dontMove )
		{
			MeleeStrike( 100, 1.5f );
		}
	}
}
