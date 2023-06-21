using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.Spoiler
{
	public class NowMakeItHappenCardController : SpoilerOneshotCardController
	{
		public NowMakeItHappenCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
		}

		public override IEnumerator Play()
		{
			// Each player may discard a card.
			List<DiscardCardAction> discardedCards = new List<DiscardCardAction>();
			IEnumerator discardCR = GameController.EachPlayerDiscardsCards(
				0,
				1,
				discardedCards,
				cardSource: GetCardSource()
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(discardCR);
			}
			else
			{
				GameController.ExhaustCoroutine(discardCR);
			}

			// Any players who do so draw 2 cards.
			foreach (DiscardCardAction item in discardedCards)
			{
				if (item.WasCardDiscarded)
				{
					IEnumerator drawCR = DrawCards(item.HeroTurnTakerController, 2);
					if (UseUnityCoroutines)
					{
						yield return GameController.StartCoroutine(drawCR);
					}
					else
					{
						GameController.ExhaustCoroutine(drawCR);
					}
				}
			}

			// One player other than {Spoiler} may play a card.
			IEnumerator playCR = GameController.SelectHeroToPlayCard(
				DecisionMaker,
				additionalCriteria: new LinqTurnTakerCriteria((TurnTaker tt) => tt != this.TurnTaker),
				cardSource: GetCardSource()
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(playCR);
			}
			else
			{
				GameController.ExhaustCoroutine(playCR);
			}

			// You may discard a card. If you do, activate a [u]rewind[/u] text.
			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(DiscardToRewind());
			}
			else
			{
				GameController.ExhaustCoroutine(DiscardToRewind());
			}

			yield break;
		}
	}
}