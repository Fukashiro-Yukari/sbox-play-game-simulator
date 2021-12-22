using Sandbox;
using System;
using System.Linq;
using System.Collections.Generic;

public partial class PlayGames : Game
{
	public static float MaxPlayerStamina = 100f;

	public PlayGames()
	{
		if ( IsServer )
			_ = new PlayGamesHud();
	}

	public override void PostLevelLoaded()
	{
		base.PostLevelLoaded();

		ItemRespawn.Init();
	}

	public override void ClientJoined( Client cl )
	{
		base.ClientJoined( cl );

		var realPlayer = new RealPlayer( cl );
		var inGamePlayer = new InGamePlayer();

		realPlayer.PlayerTwe = inGamePlayer;
		inGamePlayer.PlayerTwe = realPlayer;

		realPlayer.Respawn();
		inGamePlayer.Respawn();

		cl.Pawn = realPlayer;
	}

	public override void ClientDisconnect( Client cl, NetworkDisconnectionReason reason )
	{
		if ( cl.Pawn.IsValid() )
		{
			if ( cl.Pawn is RealPlayer rply )
			{
				rply.PlayerTwe.Delete();
				rply.PlayerTwe = null;
			}
			else if ( cl.Pawn is InGamePlayer iply )
			{
				iply.PlayerTwe.Delete();
				iply.PlayerTwe = null;
			}
		}

		base.ClientDisconnect( cl, reason );
	}

	public override void MoveToSpawnpoint( Entity pawn )
	{
		if ( pawn is InGamePlayer iplayer )
		{
			var spawnpoint = All
									.OfType<InGamePlayerSpawnPoint>()
									.OrderBy( x => Guid.NewGuid() )
									.FirstOrDefault();

			if ( spawnpoint == null )
			{
				Log.Warning( $"Couldn't find spawnpoint for {iplayer}!" );
				return;
			}

			iplayer.Transform = spawnpoint.Transform;
		}
		else if ( pawn is RealPlayer rplayer )
		{
			var spawnpoint = All
									.OfType<RealPlayerSpawnPoint>()
									.OrderBy( x => Guid.NewGuid() )
									.FirstOrDefault();

			if ( spawnpoint == null )
			{
				Log.Warning( $"Couldn't find spawnpoint for {rplayer}!" );
				return;
			}

			rplayer.Transform = spawnpoint.Transform;
		}
		else if ( pawn is NPCPolice police )
		{
			var spawnpoint = All
									.OfType<PoliceSpawnPoint>()
									.OrderBy( x => Guid.NewGuid() )
									.FirstOrDefault();

			if ( spawnpoint == null )
			{
				Log.Warning( $"Couldn't find spawnpoint for {police}!" );
				return;
			}

			police.Transform = spawnpoint.Transform;
		}
	}

	public override void OnKilled( Entity pawn )
	{
		Host.AssertServer();

		var client = pawn.Client;

		if ( client == null )
		{
			if ( pawn is RealPlayer ply )
				client = ply.PlayerTwe.Client;
			else if ( pawn is InGamePlayer ply2 )
				client = ply2.PlayerTwe.Client;
		}

		if ( client != null )
		{
			OnKilled( client, pawn );
		}
		else
		{
			OnEntKilled( pawn );
		}
	}

	public override void OnKilled( Client client, Entity pawn )
	{
		Host.AssertServer();

		var isHeadShot = false;

		Log.Info( $"{client.Name} was killed" );

		if ( pawn is InGamePlayer ply )
			isHeadShot = ply.IsHeadShot;

		if ( pawn is InGamePlayer && pawn.LastAttacker is InGamePlayer attply && attply.PlayerTwe.IsValid() )
		{
			attply.PlayerTwe.GiveMoney( 35 );
		}

		if ( pawn is RealPlayer ply2 && pawn.LastAttacker is RealPlayer attply2 )
		{
			var addMoney = ply2.Money;

			ply2.LossMoney( addMoney );
			attply2.GiveMoney( addMoney );

			_ = new NPCPolice()
			{
				Target = attply2,
				Victim = ply2,
				Money = addMoney
			};
		}

		if ( pawn is InGamePlayer )
		{
			if ( pawn.LastAttacker != null )
			{
				var attackerClient = pawn.LastAttacker.Client;

				if ( attackerClient != null )
				{
					if ( pawn.LastAttackerWeapon != null )
						KillFeed.OnKilledMessage( attackerClient.PlayerId, attackerClient.Name, client.PlayerId, client.Name, pawn.LastAttackerWeapon.ClassInfo?.Name, isHeadShot );
					else
						KillFeed.OnKilledMessage( attackerClient.PlayerId, attackerClient.Name, client.PlayerId, client.Name, pawn.LastAttacker.ClassInfo?.Name, isHeadShot );
				}
				else
				{
					KillFeed.OnKilledMessage( pawn.LastAttacker.NetworkIdent, pawn.LastAttacker.ToString(), client.PlayerId, client.Name, "killed", isHeadShot );
				}
			}
			else
			{
				KillFeed.OnKilledMessage( 0, "", client.PlayerId, client.Name, "died", isHeadShot );
			}
		}
	}

	public void OnEntKilled( Entity ent )
	{
		Host.AssertServer();

		if ( ent.LastAttacker != null )
		{
			var attackerClient = ent.LastAttacker.Client;

			if ( attackerClient != null )
			{
				if ( ent.LastAttackerWeapon != null )
					KillFeed.OnKilledMessage( attackerClient.PlayerId, attackerClient.Name, ent.ClassInfo.Title, ent.LastAttackerWeapon?.ClassInfo?.Name );
				else
					KillFeed.OnKilledMessage( attackerClient.PlayerId, attackerClient.Name, ent.ClassInfo.Title, ent.LastAttacker.ClassInfo?.Name );
			}
			else
			{
				KillFeed.OnKilledMessage( ent.LastAttacker.NetworkIdent, ent.LastAttacker.ToString(), ent.ClassInfo.Title, "killed" );
			}
		}
		else
		{
			KillFeed.OnKilledMessage( 0, "", ent.ClassInfo.Title, "died" );
		}
	}
}
