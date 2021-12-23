
using Sandbox;
using Sandbox.Hooks;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System;
using System.Collections.Generic;

namespace Sandbox.UI
{
	public partial class NewScoreboardEntry : Panel
	{
		public Client Client;

		public Label PlayerName;
		public Label Money;
		public Label Kills;
		public Label Deaths;
		public Label Ping;

		public NewScoreboardEntry()
		{
			AddClass( "entry" );

			PlayerName = Add.Label( "PlayerName", "name" );
			Money = Add.Label( "", "money" );
			Kills = Add.Label( "", "kills" );
			Deaths = Add.Label( "", "deaths" );
			Ping = Add.Label( "", "ping" );
		}

		RealTimeSince TimeSinceUpdate = 0;

		public override void Tick()
		{
			base.Tick();

			if ( !IsVisible )
				return;

			if ( !Client.IsValid() )
				return;

			if ( TimeSinceUpdate < 0.1f )
				return;

			TimeSinceUpdate = 0;
			UpdateData();
		}

		public virtual void UpdateData()
		{
			PlayerName.Text = Client.Name;

			var money = 0;

			if ( Client.Pawn is RealPlayer ply )
				money = ply.Money;
			else if ( Client.Pawn is InGamePlayer ply2 )
				money = ply2.PlayerTwe.Money;

			Money.Text = money.ToString();
			Kills.Text = Client.GetInt( "kills" ).ToString();
			Deaths.Text = Client.GetInt( "deaths" ).ToString();
			Ping.Text = Client.Ping.ToString();
			SetClass( "me", Client == Local.Client );
		}

		public virtual void UpdateFrom( Client client )
		{
			Client = client;
			UpdateData();
		}
	}
}
