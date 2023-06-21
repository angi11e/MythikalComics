using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.WhatsHerFace
{
	public class WatchYourStepCardController : WhatsHerFaceBaseCardController
	{
		/*
		 * One target deals itself 2 Melee damage, plus 2 per [u]recall[/u] card next to it.
		 */

		public WatchYourStepCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
		}

		public override IEnumerator Play()
		{
			// One target deals itself 2 Melee damage, plus 2 per [u]recall[/u] card next to it.
			List<SelectCardDecision> storedResult = new List<SelectCardDecision>();
			IEnumerator pickTargetCR = GameController.SelectCardAndStoreResults(
				DecisionMaker,
				SelectionType.DealDamageSelf,
				new LinqCardCriteria((Card c) => c.IsTarget && c.IsInPlay, "target", useCardsSuffix: false),
				storedResult,
				optional: false,
				cardSource: GetCardSource()
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(pickTargetCR);
			}
			else
			{
				GameController.ExhaustCoroutine(pickTargetCR);
			}

			SelectCardDecision selection = storedResult.FirstOrDefault();
			if (selection != null && selection.SelectedCard != null)
			{
				Card theTarget = selection.SelectedCard;
				int recallCount = theTarget.NextToLocation.Cards.Where(c => IsRecall(c)).Count();

				IEnumerator selfDamageCR = DealDamage(
					theTarget,
					theTarget,
					2 * (recallCount + 1),
					DamageType.Melee
				);
				if (UseUnityCoroutines)
				{
					yield return GameController.StartCoroutine(selfDamageCR);
				}
				else
				{
					GameController.ExhaustCoroutine(selfDamageCR);
				}
			}

			yield break;
		}
	}
}