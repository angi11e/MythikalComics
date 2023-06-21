using System;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Angille.LaComodora
{
	public class MythikalLaComodoraCharacterCardController : HeroCharacterCardController
	{
		private const string RecoverableCards = "RecoverableCards";

		public MythikalLaComodoraCharacterCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
			SpecialStringMaker.ShowSpecialString(
				() => "Before the start of your next turn, you may return an equipment card in play to its player's hand."
					+ (GetCardPropertyJournalEntryInteger(RecoverableCards) > 1
					? $" (x{GetCardPropertyJournalEntryInteger(RecoverableCards)})" : ""),
				() => true
			).Condition = () => GetCardPropertyJournalEntryInteger(RecoverableCards) != null
				&& GetCardPropertyJournalEntryInteger(RecoverableCards) > 0;
		}

		public override void AddTriggers()
		{
			// Before the start of your next turn...
			AddPhaseChangeTrigger(
				(TurnTaker tt) => tt == this.TurnTaker,
				(Phase p) => true,
				(PhaseChangeAction pca) => pca.FromPhase.TurnTaker != this.TurnTaker
					&& pca.ToPhase.TurnTaker == this.TurnTaker,
				StartOfTurnResponse,
				new TriggerType[] { TriggerType.MoveCard },
				TriggerTiming.After
			);

			base.AddTriggers();
		}

		public override IEnumerator UsePower(int index = 0)
		{
			int oldProperty = GetCardPropertyJournalEntryInteger(RecoverableCards) ?? 0;
			SetCardProperty(RecoverableCards, ++oldProperty);

			IEnumerator messageCR = GameController.SendMessageAction(
				"Before the start of your next turn, you may return an equipment card in play to its player's hand.",
				Priority.High,
				GetCardSource()
			);
			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(messageCR);
			}
			else
			{
				GameController.ExhaustCoroutine(messageCR);
			}

			// Draw or play a card.
			IEnumerator drawPlayCR = DrawACardOrPlayACard(DecisionMaker, false);
			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(drawPlayCR);
			}
			else
			{
				GameController.ExhaustCoroutine(drawPlayCR);
			}

			yield break;
		}

		private IEnumerator StartOfTurnResponse(PhaseChangeAction pca)
		{
			int cardsToRecover = GetCardPropertyJournalEntryInteger(RecoverableCards) ?? 0;

			for (int i = 0; i < cardsToRecover; i++)
			{
				// ...you may return an equipment card in play to its player's hand.
				SelectCardDecision selectCard = new SelectCardDecision(
					GameController,
					DecisionMaker,
					SelectionType.MoveCardToHand,
					FindCardsWhere((Card c) => c.IsInPlayAndHasGameText && IsEquipment(c) && c.IsHero),
					true,
					cardSource: GetCardSource()
				);

				IEnumerator selectCR = GameController.SelectCardAndDoAction(
					selectCard,
					(SelectCardDecision scd) => GameController.MoveCard(
						DecisionMaker,
						scd.SelectedCard,
						scd.SelectedCard.Owner.ToHero().Hand,
						cardSource: GetCardSource()
					)
				);

				if (UseUnityCoroutines)
				{
					yield return GameController.StartCoroutine(selectCR);
				}
				else
				{
					GameController.ExhaustCoroutine(selectCR);
				}
			}

			SetCardProperty(RecoverableCards, 0);

			yield break;
		}

		public override IEnumerator UseIncapacitatedAbility(int index)
		{
			switch (index)
			{
				case 0:
					// One player may play a card.
					IEnumerator playCR = SelectHeroToPlayCard(DecisionMaker);

					if (UseUnityCoroutines)
					{
						yield return GameController.StartCoroutine(playCR);
					}
					else
					{
						GameController.ExhaustCoroutine(playCR);
					}
					break;
				case 1:
					// One player may draw a card.
					IEnumerator drawCardCR = GameController.SelectHeroToDrawCard(
						DecisionMaker,
						cardSource: GetCardSource()
					);

					if (UseUnityCoroutines)
					{
						yield return GameController.StartCoroutine(drawCardCR);
					}
					else
					{
						GameController.ExhaustCoroutine(drawCardCR);
					}
					break;
				case 2:
					//Move up to 3 non-character hero cards from play to their owner' hands.
					List<SelectCardsDecision> cardsDecision = new List<SelectCardsDecision>();
					IEnumerator selectCR = GameController.SelectCardsAndStoreResults(
						HeroTurnTakerController,
						SelectionType.ReturnToHand,
						(Card c) => c.IsHero && !c.IsCharacter && c.IsInPlayAndHasGameText,
						3,
						cardsDecision,
						false,
						0,
						cardSource: GetCardSource()
					);

					if (UseUnityCoroutines)
					{
						yield return GameController.StartCoroutine(selectCR);
					}
					else
					{
						GameController.ExhaustCoroutine(selectCR);
					}

					if (DidSelectCards(cardsDecision))
					{
						IEnumerable<Card> selectedCards = GetSelectedCards(cardsDecision);
						foreach (Card card in selectedCards)
						{
							IEnumerator moveCR = GameController.MoveCard(
								TurnTakerController,
								card,
								card.Owner.ToHero().Hand,
								cardSource: GetCardSource()
							);

							if (UseUnityCoroutines)
							{
								yield return GameController.StartCoroutine(moveCR);
							}
							else
							{
								GameController.ExhaustCoroutine(moveCR);
							}
						}
					}
					break;
			}
			yield break;
		}
	}
}