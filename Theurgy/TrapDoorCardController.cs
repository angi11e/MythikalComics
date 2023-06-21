using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.Theurgy
{
	public class TrapDoorCardController : TheurgyBaseCardController
	{
		// Theurgy deals 1 target 3 projectile damage.
		// That target deals up to X targets 2 melee damage,
		//  where X = the number of charm cards in play plus 1.
		// You may destroy a [u]charm[/u] card.

		public TrapDoorCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
			SpecialStringMaker.ShowNumberOfCardsInPlay(IsCharmCriteria());
		}

		public override IEnumerator Play()
		{
			// deal 1 target 3 projectile damage
			List<SelectCardDecision> storedResults = new List<SelectCardDecision>();
			IEnumerator firstDamageCR = GameController.SelectTargetsAndDealDamage(
				DecisionMaker,
				new DamageSource(GameController, this.CharacterCard),
				3,
				DamageType.Projectile,
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

			// ...where X = the number of charm cards in play plus 1.
			int numberOfTargets = CharmCardsInPlay + 1;

			// That target deals up to X other targets 2 melee damage...
			List<Card> affectedCards = storedResults.Select((SelectCardDecision sc) => sc.SelectedCard).ToList();
			if (affectedCards != null && affectedCards.Count() > 0)
			{
				Card poorSchmuck = affectedCards.FirstOrDefault();
				if (poorSchmuck != null && poorSchmuck.IsInPlayAndHasGameText && numberOfTargets > 0)
				{
					IEnumerator splashDamageCR = GameController.SelectTargetsAndDealDamage(
						DecisionMaker,
						new DamageSource(GameController, poorSchmuck),
						2,
						DamageType.Melee,
						numberOfTargets,
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

			// You may destroy a [u]charm[/u] card.
			IEnumerator destroyCR = GameController.SelectAndDestroyCard(
				DecisionMaker,
				IsCharmCriteria(),
				true,
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