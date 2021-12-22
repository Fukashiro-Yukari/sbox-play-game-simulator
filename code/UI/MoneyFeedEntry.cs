using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

public class MoneyFeedEntry : Panel
{
	public Label Icon { get; internal set; }
	public Label Text { get; internal set; }

	public RealTimeSince TimeSinceBorn = 0;

	public MoneyFeedEntry()
	{
		Icon = Add.Label( "attach_money", "Icon" );
		Text = Add.Label( "0", "Text" );
	}

	public override void Tick()
	{
		base.Tick();

		if ( TimeSinceBorn > 6 )
		{
			Delete();
		}
	}
}
