using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.PecosBill
{
	public class BigManOnTheSceneCardController : PecosBillBaseCardController
	{
		/*
		 * When this card enters play, each hero target regains 1 hp.
		 * 
		 * POWER
		 * {PecosBill} deals 1 target 2 melee damage.
		 * If a target is destroyed this way,
		 * {PecosBill} deals 1 target 2 irreducible psychic damage,
		 * then if no damage is dealt this way, play a card.
		 */

		public BigManOnTheSceneCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
		}

		public override IEnumerator Play()
		{
			// When this card enters play, each hero target regains 1 hp.
			IEnumerator healCR = GameController.GainHP(
				DecisionMaker,
				(Card c) => c.IsHero,
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

			yield break;
		}

		public override IEnumerator UsePower(int index = 0)
		{
			int target1Numeral = GetPowerNumeral(0, 1);
			int damage1Numeral = GetPowerNumeral(1, 2);
			int target2Numeral = GetPowerNumeral(2, 1);
			int damage2Numeral = GetPowerNumeral(3, 2);

			// {PecosBill} deals 1 target 2 melee damage.
			List<DealDamageAction> meleeResults = new List<DealDamageAction>();
			IEnumerator damageCR = GameController.SelectTargetsAndDealDamage(
				DecisionMaker,
				new DamageSource(GameController, this.CharacterCard),
				damage1Numeral,
				DamageType.Melee,
				target1Numeral,
				false,
				target1Numeral,
				storedResultsDamage: meleeResults,
				cardSource: GetCardSource()
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(damageCR);
			}
			else
			{
				GameController.ExhaustCoroutine(damageCR);
			}

			// If a target is destroyed this way...
			if (meleeResults.Any((DealDamageAction dd) => dd.DidDestroyTarget))
			{
				// ...{PecosBill} deals 1 target 2 irreducible psychic damage...
				List<DealDamageAction> scareResults = new List<DealDamageAction>();
				IEnumerator scareCR = GameController.SelectTargetsAndDealDamage(
					DecisionMaker,
					new DamageSource(GameController, this.CharacterCard),
					damage2Numeral,
					DamageType.Psychic,
					target2Numeral,
					false,
					target2Numeral,
					true,
					storedResultsDamage: scareResults,
					cardSource: GetCardSource()
				);

				if (UseUnityCoroutines)
				{
					yield return GameController.StartCoroutine(scareCR);
				}
				else
				{
					GameController.ExhaustCoroutine(scareCR);
				}

				// ...then if no damage is dealt this way...
				if (!scareResults.Any((DealDamageAction dd) => dd.DidDealDamage))
				{
					// ...play a card.
					IEnumerator playCardCR = SelectAndPlayCardsFromHand(this.HeroTurnTakerController, 1);
					if (UseUnityCoroutines)
					{
						yield return GameController.StartCoroutine(playCardCR);
					}
					else
					{
						GameController.ExhaustCoroutine(playCardCR);
					}
				}

			}

			yield break;
		}
	}
}