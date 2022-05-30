using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.Speedrunner
{
	public class ResetWarpCardController : SpeedrunnerBaseCardController
	{
		/*
		 * Discard your hand. Draw 5 cards. Play 1 card. {Speedrunner} regains 2 HP.
		 */

		public ResetWarpCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
		}

		public override IEnumerator Play()
		{
			// Discard your hand.
			List<DiscardCardAction> storedResults = new List<DiscardCardAction>();
			IEnumerator discardCR = GameController.DiscardHand(
				DecisionMaker,
				false,
				storedResults,
				TurnTaker,
				GetCardSource()
			);

			// Draw 5 cards.
			IEnumerator drawCR = DrawCards(DecisionMaker, 5);

			// Play 1 card.
			IEnumerator playCR = GameController.SelectAndPlayCardFromHand(
				DecisionMaker,
				false,
				cardSource: GetCardSource()
			);

			// {Speedrunner} regains 2 HP.
			IEnumerator healCR = GameController.GainHP(
				this.CharacterCard,
				2,
				cardSource: GetCardSource()
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(discardCR);
				yield return GameController.StartCoroutine(drawCR);
				yield return GameController.StartCoroutine(playCR);
				yield return GameController.StartCoroutine(healCR);
			}
			else
			{
				GameController.ExhaustCoroutine(discardCR);
				GameController.ExhaustCoroutine(drawCR);
				GameController.ExhaustCoroutine(playCR);
				GameController.ExhaustCoroutine(healCR);
			}

			yield break;
		}
	}
}