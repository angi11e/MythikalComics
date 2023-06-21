using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.CaptainCain
{
	public class VampirePrinceCardController : CaptainCainSetupCardController
	{
		/*
		 * When this card enters play, destroy any of your [u]fist[/u] cards in play.
		 * When this card is destroyed, reveal the top card of each Hero deck and either discard it or replace it.
		 * 
		 * Treat {Blood} effects as active.
		 * 
		 * You may draw an extra card during your draw phase.
		 */

		public VampirePrinceCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
			GameController.AddCardControllerToList(CardControllerListType.IncreasePhaseActionCount, this);
		}

		public override IEnumerator Play()
		{
			IEnumerator drawCR = IncreasePhaseActionCountIfInPhase(
				(TurnTaker tt) => tt == this.TurnTaker,
				Phase.DrawCard,
				1
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(base.Play());
				yield return GameController.StartCoroutine(drawCR);
			}
			else
			{
				GameController.ExhaustCoroutine(base.Play());
				GameController.ExhaustCoroutine(drawCR);
			}

			yield break;
		}

		public override void AddTriggers()
		{
			base.AddTriggers();

			// You may draw an extra card during your draw phase.
			AddAdditionalPhaseActionTrigger(
				(TurnTaker tt) => ShouldIncreasePhaseActionCount(tt),
				Phase.DrawCard,
				1
			);
		}

		private bool ShouldIncreasePhaseActionCount(TurnTaker tt)
		{
			return tt == this.TurnTaker;
		}

		public override bool AskIfIncreasingCurrentPhaseActionCount()
		{
			if (GameController.ActiveTurnPhase.IsDrawCard)
			{
				return ShouldIncreasePhaseActionCount(GameController.ActiveTurnTaker);
			}
			return false;
		}

		protected override TriggerType[] DestructionTriggers => new TriggerType[2] {
			TriggerType.RevealCard,
			TriggerType.DiscardCard
		};

		protected override IEnumerator SetupDestroyResponse(GameAction ga)
		{
			// When this card is destroyed, reveal the top card of each Hero deck and either discard it or replace it.
			IEnumerator eachDeckCR = DoActionToEachTurnTakerInTurnOrder(
				tt => !tt.IsIncapacitatedOrOutOfGame && tt.IsHero,
				RevealDeckResponse,
				this.TurnTaker
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(eachDeckCR);
			}
			else
			{
				GameController.ExhaustCoroutine(eachDeckCR);
			}

			yield break;
		}

		private IEnumerator RevealDeckResponse(TurnTakerController ttc)
		{
			List<Card> revealedCards = new List<Card>();
			TurnTaker turnTaker = ttc.TurnTaker;

			foreach (Location deck in turnTaker.Decks)
			{
				Location trash = FindTrashFromDeck(deck);

				IEnumerator revealCR = GameController.RevealCards(
					ttc,
					deck,
					1,
					revealedCards,
					false,
					cardSource: GetCardSource()
				);

				if (UseUnityCoroutines)
				{
					yield return GameController.StartCoroutine(revealCR);
				}
				else
				{
					GameController.ExhaustCoroutine(revealCR);
				}

				Card revealedCard = revealedCards.FirstOrDefault();
				if (revealedCard != null)
				{
					var destinations = new[]
					{
						new MoveCardDestination(deck),
						new MoveCardDestination(trash)
					};

					IEnumerator moveCardCR = GameController.SelectLocationAndMoveCard(
						DecisionMaker,
						revealedCard,
						destinations,
						cardSource: GetCardSource()
					);

					if (UseUnityCoroutines)
					{
						yield return GameController.StartCoroutine(moveCardCR);
					}
					else
					{
						GameController.ExhaustCoroutine(moveCardCR);
					}
				}
			}

			yield break;
		}
	}
}