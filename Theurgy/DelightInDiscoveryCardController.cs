using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.Theurgy
{
	public class DelightInDiscoveryCardController : TheurgyBaseCardController
	{
		// Draw X cards, where X = the number of [u]charm[/u] cards in play plus 1.
		// if {Theurgy} has no [u]charm[/u] cards in her play area, you may play a card.
		// You may destroy a [u]charm[/u] card.

		public DelightInDiscoveryCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
			SpecialStringMaker.ShowNumberOfCardsInPlay(IsCharmCriteria());
		}

		public override IEnumerator Play()
		{
			// Draw X cards, where X = the number of [u]charm[/u] cards in play plus 1.
			IEnumerator drawCardsCR = DrawCards(DecisionMaker, CharmCardsInPlay + 1);
			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(drawCardsCR);
			}
			else
			{
				GameController.ExhaustCoroutine(drawCardsCR);
			}

			// no charm cards? play a card.
			if (!TurnTaker.GetPlayAreaCards().Any((Card c) => IsCharm(c)))
			{
				IEnumerator playCardCR = SelectAndPlayCardFromHand(HeroTurnTakerController);
				if (UseUnityCoroutines)
				{
					yield return GameController.StartCoroutine(playCardCR);
				}
				else
				{
					GameController.ExhaustCoroutine(playCardCR);
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