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
		// if {Theurgy} has no [u]charm[/u] cards in her play area, play a card.
		// You may destroy a [u]charm[/u] card.

		public DelightInDiscoveryCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
			base.SpecialStringMaker.ShowNumberOfCardsInPlay(IsCharmCriteria());
		}

		public override IEnumerator Play()
		{
			// count the charm cards
			int drawNumeral = FindCardsWhere((Card c) => c.IsInPlayAndHasGameText && IsCharm(c)).Count() + 1;

			// draw that many
			IEnumerator drawCardsCR = DrawCards(DecisionMaker, drawNumeral);
			if (base.UseUnityCoroutines)
			{
				yield return base.GameController.StartCoroutine(drawCardsCR);
			}
			else
			{
				base.GameController.ExhaustCoroutine(drawCardsCR);
			}

			// no charm cards? play a card.
			if (!base.TurnTaker.GetPlayAreaCards().Any((Card c) => IsCharm(c)))
			{
				IEnumerator playCardCR = SelectAndPlayCardFromHand(base.HeroTurnTakerController);
				if (base.UseUnityCoroutines)
				{
					yield return base.GameController.StartCoroutine(playCardCR);
				}
				else
				{
					base.GameController.ExhaustCoroutine(playCardCR);
				}
			}

			// You may destroy a [u]charm[/u] card.
			IEnumerator destroyCR = base.GameController.SelectAndDestroyCard(
				DecisionMaker,
				IsCharmCriteria(),
				true,
				cardSource: GetCardSource()
			);

			if (base.UseUnityCoroutines)
			{
				yield return base.GameController.StartCoroutine(destroyCR);
			}
			else
			{
				base.GameController.ExhaustCoroutine(destroyCR);
			}

			yield break;
		}
	}
}