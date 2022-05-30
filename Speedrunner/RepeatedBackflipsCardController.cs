using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.Speedrunner
{
	public class RepeatedBackflipsCardController : SpeedrunnerBaseCardController
	{
		/*
		 * Draw a card. Discard a card. Play a card.
		 * Draw a card. Discard a card. Play a card.
		 * One player other than {Speedrunner} may use a power now.
		 */

		public RepeatedBackflipsCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
		}

		public override IEnumerator Play()
		{
			// Draw a card.
			IEnumerator drawCR = DrawCard(HeroTurnTaker);
			IEnumerator drawCR2 = DrawCard(HeroTurnTaker);

			// Discard a card.
			IEnumerator discardCR = SelectAndDiscardCards(DecisionMaker, 1);
			IEnumerator discardCR2 = SelectAndDiscardCards(DecisionMaker, 1);

			// Play a card.
			IEnumerator playCR = GameController.SelectAndPlayCardFromHand(
				DecisionMaker,
				false,
				cardSource: GetCardSource()
			);
			IEnumerator playCR2 = GameController.SelectAndPlayCardFromHand(
				DecisionMaker,
				false,
				cardSource: GetCardSource()
			);

			// One player other than {Speedrunner} may use a power now.
			IEnumerator powerCR = GameController.SelectHeroToUsePower(
				DecisionMaker,
				additionalCriteria: new LinqTurnTakerCriteria((TurnTaker tt) => tt != this.TurnTaker),
				cardSource: GetCardSource()
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(drawCR);
				yield return GameController.StartCoroutine(discardCR);
				yield return GameController.StartCoroutine(playCR);
				yield return GameController.StartCoroutine(drawCR2);
				yield return GameController.StartCoroutine(discardCR2);
				yield return GameController.StartCoroutine(playCR2);
				yield return GameController.StartCoroutine(powerCR);
			}
			else
			{
				GameController.ExhaustCoroutine(drawCR);
				GameController.ExhaustCoroutine(discardCR);
				GameController.ExhaustCoroutine(playCR);
				GameController.ExhaustCoroutine(drawCR2);
				GameController.ExhaustCoroutine(discardCR2);
				GameController.ExhaustCoroutine(playCR2);
				GameController.ExhaustCoroutine(powerCR);
			}

			yield break;
		}
	}
}