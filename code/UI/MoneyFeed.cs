using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System;

public partial class MoneyFeed : Panel
{
	public static MoneyFeed Current;

	public MoneyFeed()
	{
		Current = this;

		StyleSheet.Load( "/UI/MoneyFeed.scss" );
	}

	[ClientRpc]
	public static void OnGiveMoney( int money )
	{
		if ( money <= 0 ) return;

		Current?.AddMoneyFeed( money );
	}

	[ClientRpc]
	public static void OnLossMoney( int money )
	{
		if ( money <= 0 ) return;

		Current?.AddMoneyFeedLoss( money );
	}

	public virtual Panel AddMoneyFeed( int money )
	{
		var e = Current.AddChild<MoneyFeedEntry>();

		e.Text.Text = $"+ {money}";
		e.Text.Style.FontColor = Color.Green;
		e.Text.Style.Dirty();

		return e;
	}

	public virtual Panel AddMoneyFeedLoss( int money )
	{
		var e = Current.AddChild<MoneyFeedEntry>();

		e.Text.Text = $"- {money}";
		e.Text.Style.FontColor = Color.Red;
		e.Text.Style.Dirty();

		return e;
	}

	public override void Tick()
	{
		if ( Local.Pawn is RealPlayer )
		{
			Style.Bottom = 140;
			Style.Dirty();
		}
		else if ( Local.Pawn is InGamePlayer )
		{
			Style.Bottom = 90;
			Style.Dirty();
		}
	}
}
