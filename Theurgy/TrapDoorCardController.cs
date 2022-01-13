using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.Theurgy
{
	public class TrapDoorCardController : TheurgyBaseCardController
	{
		// Theurgy deals 1 target 3 melee damage.
		// That target deals up to X targets 2 projectile damage,
		//  where X = the number of charm cards in play plus 1.

		public TrapDoorCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
			base.SpecialStringMaker.ShowNumberOfCardsInPlay(IsCharmCriteria());
		}

		public override IEnumerator Play()
		{
			// deal 1 target 3 melee damage
			List<SelectCardDecision> storedResults = new List<SelectCardDecision>();
			IEnumerator firstDamageCR = GameController.SelectTargetsAndDealDamage(
				DecisionMaker,
				new DamageSource(base.GameController, base.CharacterCard),
				3,
				DamageType.Melee,
				1,
				false,
				1,
				storedResultsDecisions: storedResults,
				cardSource: GetCardSource()
			);

			if (base.UseUnityCoroutines)
			{
				yield return base.GameController.StartCoroutine(firstDamageCR);
			}
			else
			{
				base.GameController.ExhaustCoroutine(firstDamageCR);
			}

			// count the charm cards
			int numberOfTargets = FindCardsWhere((Card c) => c.IsInPlayAndHasGameText && IsCharm(c)).Count() + 1;

			// target deals up to that many targets 2 projectile damage
			List<Card> affectedCards = storedResults.Select((SelectCardDecision sc) => sc.SelectedCard).ToList();
			if (affectedCards != null && affectedCards.Count() > 0)
			{
				Card poorSchmuck = affectedCards.FirstOrDefault();
				if (poorSchmuck != null && poorSchmuck.IsInPlayAndHasGameText && numberOfTargets > 0)
				{
					IEnumerator splashDamageCR = GameController.SelectTargetsAndDealDamage(
						DecisionMaker,
						new DamageSource(base.GameController, poorSchmuck),
						2,
						DamageType.Projectile,
						numberOfTargets,
						optional: false,
						requiredTargets: 0,
						cardSource: GetCardSource()
					);

					if (base.UseUnityCoroutines)
					{
						yield return base.GameController.StartCoroutine(splashDamageCR);
					}
					else
					{
						base.GameController.ExhaustCoroutine(splashDamageCR);
					}
				}
			}

			yield break;
		}
	}
}