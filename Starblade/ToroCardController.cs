using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.Starblade
{
	public class ToroCardController : CardController
	{
		/*
		 * {Starblade} deals 1 target 2 melee damage.
		 * that target deals another target 2 projectile damage.
		 * 
		 * activate a [u]technique[/u] text.
		 */

		public ToroCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
		}

		public override IEnumerator Play()
		{
			// {Starblade} deals 1 target 2 melee damage.
			List<SelectCardDecision> storedResults = new List<SelectCardDecision>();
			IEnumerator firstDamageCR = GameController.SelectTargetsAndDealDamage(
				DecisionMaker,
				new DamageSource(GameController, this.CharacterCard),
				2,
				DamageType.Melee,
				1,
				false,
				1,
				storedResultsDecisions: storedResults,
				cardSource: GetCardSource()
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(firstDamageCR);
			}
			else
			{
				GameController.ExhaustCoroutine(firstDamageCR);
			}

			// that target deals another target 2 projectile damage.
			List<Card> affectedCards = storedResults.Select((SelectCardDecision sc) => sc.SelectedCard).ToList();
			if (affectedCards != null && affectedCards.Count() > 0)
			{
				Card poorSchmuck = affectedCards.FirstOrDefault();
				if (poorSchmuck != null && poorSchmuck.IsInPlayAndHasGameText)
				{
					IEnumerator splashDamageCR = GameController.SelectTargetsAndDealDamage(
						DecisionMaker,
						new DamageSource(GameController, poorSchmuck),
						2,
						DamageType.Projectile,
						1,
						optional: false,
						requiredTargets: 0,
						additionalCriteria: (Card c) => c != poorSchmuck,
						cardSource: GetCardSource()
					);

					if (UseUnityCoroutines)
					{
						yield return GameController.StartCoroutine(splashDamageCR);
					}
					else
					{
						GameController.ExhaustCoroutine(splashDamageCR);
					}
				}
			}

			// activate a [u]technique[/u] text.
			IEnumerator activateCR = GameController.SelectAndActivateAbility(
				DecisionMaker,
				"technique",
				optional: false,
				cardSource: GetCardSource()
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(activateCR);
			}
			else
			{
				GameController.ExhaustCoroutine(activateCR);
			}

			yield break;
		}
	}
}