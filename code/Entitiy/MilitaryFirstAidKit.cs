using Sandbox;
using System;

[Library( "ent_militaryfirstaidKit", Title = "Military First Aid Kit" )]
[Hammer.EditorModel( "models/MilitaryFirstAidKit/militaryfirstaidkit.vmdl", FixedBounds = true )]
public class MilitaryFirstAidKit : HealthBase
{
	public override int AddHealth => 100;
	public override string ModelPath => "models/MilitaryFirstAidKit/militaryfirstaidkit.vmdl";
}
