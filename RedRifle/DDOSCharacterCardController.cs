using System;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Angille.RedRifle
{
	public class DDOSCharacterCardController : HeroCharacterCardController
	{
		public DDOSCharacterCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
			// no clue why the below is in the file? I guess I'll find out
			CardWithoutReplacements.TokenPools.ReorderTokenPool("RedRifleTrueshotPool");
			base.SpecialStringMaker.ShowTokenPool(base.Card.FindTokenPool("RedRifleTrueshotPool"));
		}

		public override void AddStartOfGameTriggers()
		{
			// base.AddStartOfGameTriggers();
		}

		public override IEnumerator UsePower(int index = 0)
		{
			TokenPool trueshotPool = RedRifleTrueshotPoolUtility.GetTrueshotPool(this);
			// [i]D.D.O.S.[/i] deals up to 3 targets 1 projectile damage each.
			// For each target destroyed this way,
			// add 2 tokens to your trueshot pool.

			yield break;
		}

		public override IEnumerator UseIncapacitatedAbility(int index)
		{
			switch (index)
			{
				case 0:
					// One player may play a card.
					break;

				case 1:
					// One hero destroys one of their Equipment cards.
					// If they do, they deal 1 target 4 energy damage.
					break;

				case 2:
					// Until the start of your turn, increase all projectile damage by 2.
					break;
			}
			yield break;
		}
	}
}