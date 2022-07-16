using System;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Angille.Speedrunner
{
	public class GunClubSpeedrunnerCharacterCardController : HeroCharacterCardController
	{
		public GunClubSpeedrunnerCharacterCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
		}

		public override IEnumerator UsePower(int index = 0)
		{
			// Until the start of your next turn,
			// when {Speedrunner} would be dealt damage by a target,
			// ze first deals that target 1 projectile damage.

			yield break;
		}

		public IEnumerator AimbotResponse(DealDamageAction dd, TurnTaker hero, StatusEffect effect, int[] powerNumerals = null)
		{

			yield break;
		}


		public override IEnumerator UseIncapacitatedAbility(int index)
		{
			switch (index)
			{
				case 0:
					// One hero may use a power.
					break;
				case 1:
					// Until the start of your turn, increase all projectile damage by 2.
					break;
				case 2:
					// Put one card from the villain or environment trash into play.
					break;
			}
			yield break;
		}
	}
}