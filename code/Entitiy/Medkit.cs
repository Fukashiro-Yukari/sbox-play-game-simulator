using Sandbox;
using System;

[Library( "ent_medkit", Title = "Medkit" )]
[Hammer.EditorModel( "models/Medkit/medkit.vmdl", FixedBounds = true )]
public class Medkit : HealthBase
{
	public override int AddHealth => 100;
	public override string ModelPath => "models/Medkit/medkit.vmdl";
}
