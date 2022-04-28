using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.Athena
{
	public class HornOfAmaltheaCardController : AthenaBaseCardController
	{
		/*
		 * You may draw an additional card during your draw phase.
		 * 
		 * POWER
		 * Discard 3 cards. If you do so, draw 4 cards.
		 *  then, if no [u]aspect[/u] cards are in play, destroy this card.
		 */

		public HornOfAmaltheaCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
			GameController.AddCardControllerToList(CardControllerListType.IncreasePhaseActionCount, this);
		}

		public override IEnumerator Play()
		{
			IEnumerator drawCR = IncreasePhaseActionCountIfInPhase(
				(TurnTaker tt) => tt == base.TurnTaker,
				Phase.DrawCard,
				1
			);
			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(drawCR);
			}
			else
			{
				GameController.ExhaustCoroutine(drawCR);
			}
			yield break;
		}

		public override void AddTriggers()
		{
			// You may draw an additional card during your draw phase.
			AddAdditionalPhaseActionTrigger(
				(TurnTaker tt) => ShouldIncreasePhaseActionCount(tt),
				Phase.DrawCard,
				1
			);

			base.AddTriggers();
		}

		public override IEnumerator UsePower(int index = 0)
		{
			int discardNumeral = GetPowerNumeral(0, 3);
			int drawNumeral = GetPowerNumeral(1, 4);

			// Discard 3 cards. If you do so, draw 5 cards.
			List<DiscardCardAction> storedResults = new List<DiscardCardAction>();
			IEnumerator discardCR = SelectAndDiscardCards(
				HeroTurnTakerController,
				discardNumeral,
				optional: false,
				null,
				storedResults
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(discardCR);
			}
			else
			{
				GameController.ExhaustCoroutine(discardCR);
			}

			if (DidDiscardCards(storedResults, discardNumeral))
			{
				IEnumerator drawCR = DrawCards(
					DecisionMaker,
					drawNumeral
				);

				if (UseUnityCoroutines)
				{
					yield return GameController.StartCoroutine(drawCR);
				}
				else
				{
					GameController.ExhaustCoroutine(drawCR);
				}
			}
			else
			{
				IEnumerator messageCR = GameController.SendMessageAction(
					base.TurnTaker.Name + " did not discard enough cards to draw more.",
					Priority.High,
					GetCardSource(),
					null,
					showCardSource: true
				);
				
				if (UseUnityCoroutines)
				{
					yield return GameController.StartCoroutine(messageCR);
				}
				else
				{
					GameController.ExhaustCoroutine(messageCR);
				}
			}

			// then, if no [u]aspect[/u] cards are in play, destroy this card.
			if (!AspectInPlay)
			{
				IEnumerator destructionCR = GameController.DestroyCard(
					DecisionMaker,
					base.Card,
					cardSource: GetCardSource()
				);

				if (UseUnityCoroutines)
				{
					yield return GameController.StartCoroutine(destructionCR);
				}
				else
				{
					GameController.ExhaustCoroutine(destructionCR);
				}
			}

			yield break;
		}

		private bool ShouldIncreasePhaseActionCount(TurnTaker tt)
		{
			return tt == base.TurnTaker;
		}

		public override bool AskIfIncreasingCurrentPhaseActionCount()
		{
			if (GameController.ActiveTurnPhase.IsDrawCard)
			{
				return ShouldIncreasePhaseActionCount(GameController.ActiveTurnTaker);
			}
			return false;
		}
	}
}