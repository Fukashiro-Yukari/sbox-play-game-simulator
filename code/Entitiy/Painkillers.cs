using Sandbox;
using System;

[Library( "ent_painkillers", Title = "Painkillers" )]
[Hammer.EditorModel( "models/Painkillers/painkillers.vmdl", FixedBounds = true )]
public class Painkillers : HealthBase
{
	public override int AddHealth => 50;
	public override string ModelPath => "models/Painkillers/painkillers.vmdl";
}
