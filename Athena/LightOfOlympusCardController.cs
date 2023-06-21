using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.Athena
{
	public class LightOfOlympusCardController : AthenaBaseCardController
	{
		/*
		 * Each hero target regains 1 HP.
		 * 
		 * If there is an [u]aspect[/u] card in play,
		 *  {Athena} deals each non-hero target 1 radiant damage.
		 */

		public LightOfOlympusCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
		}

		public override IEnumerator Play()
		{
			// Each hero target regains 1 HP.
			IEnumerator healCR = GameController.GainHP(
				DecisionMaker,
				(Card c) => IsHero(c),
				1,
				cardSource: GetCardSource()
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(healCR);
			}
			else
			{
				GameController.ExhaustCoroutine(healCR);
			}

			// If there is an [u]aspect[/u] card in play,
			if (ManifestInPlay)
			{
				// {Athena} deals each non-hero target 1 radiant damage.
				IEnumerator damageCR = DealDamage(
					this.CharacterCard,
					(Card c) => !IsHero(c),
					1,
					DamageType.Radiant
				);

				if (UseUnityCoroutines)
				{
					yield return GameController.StartCoroutine(damageCR);
				}
				else
				{
					GameController.ExhaustCoroutine(damageCR);
				}
			}

			yield break;
		}
	}
}