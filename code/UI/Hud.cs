using Sandbox;
using Sandbox.UI;

public partial class PlayGamesHud : HudEntity<RootPanel>
{
	public PlayGamesHud()
	{
		if ( !IsClient )
			return;

		RootPanel.StyleSheet.Load( "/ui/hud.scss" );

		RootPanel.AddChild<NameTags>();
		RootPanel.AddChild<CrosshairCanvas>();
		RootPanel.AddChild<RealState>();
		RootPanel.AddChild<InGameState>();
		RootPanel.AddChild<Money>();
		RootPanel.AddChild<Ammo>();
		RootPanel.AddChild<Hint>();
		RootPanel.AddChild<PickupFeed>();
		RootPanel.AddChild<MoneyFeed>();
		RootPanel.AddChild<KillFeed>();
		RootPanel.AddChild<InventoryBar>();
		RootPanel.AddChild<DamageIndicator>();
		RootPanel.AddChild<HitIndicator>();
		RootPanel.AddChild<ChatBox>();
		RootPanel.AddChild<VoiceList>();
		RootPanel.AddChild<Scoreboard<ScoreboardEntry>>();
	}
}
