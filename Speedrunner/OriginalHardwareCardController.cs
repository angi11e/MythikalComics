using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.Speedrunner
{
	/*
	 * Reduce damage dealt to {Speedrunner} by 1.
	 * 
	 * POWER
	 * Discard a glitch or strat card.
	 * Search your deck for a card with the chosen keyword and 
	 *  either put it in your hand or into play, then shuffle your deck.
	 */

	public class OriginalHardwareCardController : SpeedrunnerBaseCardController
	{
		public OriginalHardwareCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
		}

		public override void AddTriggers()
		{
			// Reduce damage dealt to {Speedrunner} by 1.
			AddReduceDamageTrigger((Card c) => c == this.CharacterCard, 1);

			base.AddTriggers();
		}

		public override IEnumerator UsePower(int index = 0)
		{
			// Discard a glitch or strat card.
			List<DiscardCardAction> storedResults = new List<DiscardCardAction>();
			IEnumerator discardCR = SelectAndDiscardCards(
				DecisionMaker,
				1,
				optional: false,
				1,
				storedResults,
				cardCriteria: new LinqCardCriteria((Card c) => IsGlitch(c) || IsStrat(c))
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(discardCR);
			}
			else
			{
				GameController.ExhaustCoroutine(discardCR);
			}

			if (DidDiscardCards(storedResults))
			{
				bool isGlitch = IsGlitch(storedResults.FirstOrDefault().CardToDiscard);

				// Search your deck for a card with the chosen keyword and
				// either put it in your hand or into play, then shuffle your deck.
				IEnumerator searchCR = SearchForCards(
					DecisionMaker,
					true,
					false,
					1,
					1,
					isGlitch ? IsGlitchCriteria() : IsStratCriteria(),
					true,
					true,
					false,
					shuffleAfterwards: true
				);

				if (UseUnityCoroutines)
				{
					yield return GameController.StartCoroutine(searchCR);
				}
				else
				{
					GameController.ExhaustCoroutine(searchCR);
				}
			}

			yield break;
		}
	}
}