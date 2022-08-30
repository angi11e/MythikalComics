using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.Patina
{
	public class SurgeWaveCardController : PatinaBaseCardController
	{
		/*
		 * {Patina} deals each target X cold or melee damage,
		 * where X = the number of water cards in play plus 1.
		 * 
		 * Destroy an environment card.
		 */

		public SurgeWaveCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
			SpecialStringMaker.ShowNumberOfCardsInPlay(IsWaterCriteria((Card c) => !c.IsOneShot));
		}

		public override IEnumerator Play()
		{
			// ...where X = the number of water cards in play plus 1.
			int damageNumeral = FindCardsWhere(
				(Card c) => c.IsInPlayAndHasGameText && IsWater(c) && !c.IsOneShot
			).Count() + 1;

			// {Patina} deals each target X cold or melee damage...
			List<SelectDamageTypeDecision> chosenType = new List<SelectDamageTypeDecision>();
			IEnumerator chooseTypeCR = GameController.SelectDamageType(
				DecisionMaker,
				chosenType,
				new DamageType[] { DamageType.Cold, DamageType.Melee },
				cardSource: GetCardSource()
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(chooseTypeCR);
			}
			else
			{
				GameController.ExhaustCoroutine(chooseTypeCR);
			}

			DamageType? damageType = GetSelectedDamageType(chosenType);
			if (damageType != null)
			{
				IEnumerator damageCR = DealDamage(
					this.CharacterCard,
					(Card c) => c.IsTarget,
					damageNumeral,
					damageType.Value
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

			// Destroy an environment card.
			IEnumerator destroyCR = GameController.SelectAndDestroyCard(
				DecisionMaker,
				new LinqCardCriteria((Card c) => c.IsEnvironment, "environment"),
				false,
				cardSource: GetCardSource()
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(destroyCR);
			}
			else
			{
				GameController.ExhaustCoroutine(destroyCR);
			}

			yield break;
		}
	}
}