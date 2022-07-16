using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.Speedrunner
{
	public class RNGManipulationCardController : SpeedrunnerBaseCardController
	{
		/*
		 * When a villain card would be played, you may play a card from under this card instead.
		 * At the start of your turn, if there are no cards under this card,
		 *  one villain target deals themself 2 melee damage, then destroy this card.
		 *  
		 * POWER
		 * Move 1 card from the villain trash or the top card of the villain deck under this card.
		 */

		public RNGManipulationCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
		}

		public override void AddTriggers()
		{
			// When a villain card would be played, you may play a card from under this card instead.
			AddTrigger(
				(PlayCardAction pc) =>
					IsVillain(pc.CardToPlay)
					&& !pc.IsPutIntoPlay
					&& this.Card.UnderLocation.HasCards
					&& pc.CardToPlay.Location != this.Card.UnderLocation,
				VillainPlayResponse,
				new TriggerType[2]
				{
					TriggerType.CancelAction,
					TriggerType.PlayCard
				},
				TriggerTiming.Before,
				orderMatters: true
			);

			// At the start of your turn, if there are no cards under this card...
			AddStartOfTurnTrigger(
				(TurnTaker tt) => tt == TurnTaker,
				NoCardsResponse,
				new TriggerType[2] { TriggerType.DealDamage, TriggerType.DestroySelf },
				(PhaseChangeAction pca) => this.Card.UnderLocation.IsEmpty
			);

			AddBeforeLeavesPlayActions(ReturnCardsToOwnersTrashResponse);
			base.AddTriggers();
		}

		private IEnumerator ReturnCardsToOwnersTrashResponse(GameAction ga)
		{
			while (this.Card.UnderLocation.Cards.Count() > 0)
			{
				Card topCard = this.Card.UnderLocation.TopCard;
				MoveCardDestination trashDestination = FindCardController(topCard).GetTrashDestination();
				IEnumerator returnCR = GameController.MoveCard(
					TurnTakerController,
					topCard,
					trashDestination.Location,
					trashDestination.ToBottom,
					cardSource: GetCardSource()
				);

				if (UseUnityCoroutines)
				{
					yield return GameController.StartCoroutine(returnCR);
				}
				else
				{
					GameController.ExhaustCoroutine(returnCR);
				}
			}
		}

		private IEnumerator VillainPlayResponse(PlayCardAction pca)
		{
			List<YesNoCardDecision> storedResults = new List<YesNoCardDecision>();
			IEnumerator yesNoCR = GameController.MakeYesNoCardDecision(
				DecisionMaker,
				SelectionType.PlayCard,
				this.Card,
				storedResults: storedResults,
				cardSource: GetCardSource()
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(yesNoCR);
			}
			else
			{
				GameController.ExhaustCoroutine(yesNoCR);
			}

			// If you do so...
			if (DidPlayerAnswerYes(storedResults))
			{
				// you may play a card from under this card instead.
				IEnumerator cancelCR = CancelAction(pca);
				IEnumerator playCR = GameController.SelectCardFromLocationAndMoveIt(
					DecisionMaker,
					this.Card.UnderLocation,
					new LinqCardCriteria((Card c) => true),
					new MoveCardDestination[] { new MoveCardDestination(this.TurnTaker.PlayArea) },
					cardSource: GetCardSource()
				);

				if (UseUnityCoroutines)
				{
					yield return GameController.StartCoroutine(cancelCR);
					yield return GameController.StartCoroutine(playCR);
				}
				else
				{
					GameController.ExhaustCoroutine(cancelCR);
					GameController.ExhaustCoroutine(playCR);
				}
			}

			yield break;
		}

		private IEnumerator NoCardsResponse(PhaseChangeAction pca)
		{
			// one villain target deals themself 2 melee damage...
			IEnumerator selfDamageCR = GameController.SelectTargetsToDealDamageToSelf(
				DecisionMaker,
				2,
				DamageType.Melee,
				1,
				false,
				1,
				additionalCriteria: (Card c) => c.IsVillain,
				cardSource: GetCardSource()
			);

			// ...then destroy this card.
			IEnumerator destroyCR = GameController.DestroyCard(
				DecisionMaker,
				this.Card,
				optional: false,
				cardSource: GetCardSource()
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(selfDamageCR);
				yield return GameController.StartCoroutine(destroyCR);
			}
			else
			{
				GameController.ExhaustCoroutine(selfDamageCR);
				GameController.ExhaustCoroutine(destroyCR);
			}

			yield break;
		}

		public override IEnumerator UsePower(int index = 0)
		{
			int trashNumeral = GetPowerNumeral(0, 1);

			// gotta select a villain because eugh vengeance mode
			List<SelectLocationDecision> villainDecks = new List<SelectLocationDecision>();
			IEnumerator findVillainCR = GameController.SelectADeck(
				DecisionMaker,
				SelectionType.MoveCardToUnderCard,
				(Location l) => l.IsVillain,
				villainDecks,
				cardSource: GetCardSource()
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(findVillainCR);
			}
			else
			{
				GameController.ExhaustCoroutine(findVillainCR);
			}

			SelectLocationDecision villainDeck = villainDecks.FirstOrDefault();

			if (villainDeck != null && villainDeck.SelectedLocation.Location != null)
			{
				List<Function> functionList = new List<Function>();

				// ...from the villain trash or...
				functionList.Add(
					new Function(
						DecisionMaker,
						"move from villain trash",
						SelectionType.MoveCardToUnderCard,
						() => GameController.SelectCardsFromLocationAndMoveThem(
							DecisionMaker,
							villainDeck.SelectedLocation.Location.OwnerTurnTaker.Trash,
							trashNumeral,
							trashNumeral,
							new LinqCardCriteria((Card c) => true),
							new MoveCardDestination[] { new MoveCardDestination(this.Card.UnderLocation) },
							cardSource: GetCardSource()
						),
						onlyDisplayIfTrue: villainDeck.SelectedLocation.Location.OwnerTurnTaker.Trash.HasCards
					)
				);

				// ...the top card of the villain deck
				functionList.Add(
					new Function(
						DecisionMaker,
						"move from top of villain deck",
						SelectionType.MoveCardToUnderCard,
						() => GameController.MoveCard(
							DecisionMaker,
							villainDeck.SelectedLocation.Location.TopCard,
							this.Card.UnderLocation,
							showMessage: true,
							cardSource: GetCardSource()
						),
						onlyDisplayIfTrue: villainDeck.SelectedLocation.Location.HasCards
					)
				);

				// Move 1 card... ...under this card.
				SelectFunctionDecision selectFunction = new SelectFunctionDecision(
					GameController,
					DecisionMaker,
					functionList,
					false,
					cardSource: GetCardSource()
				);

				IEnumerator selectFunctionCR = GameController.SelectAndPerformFunction(selectFunction);
				if (UseUnityCoroutines)
				{
					yield return GameController.StartCoroutine(selectFunctionCR);
				}
				else
				{
					GameController.ExhaustCoroutine(selectFunctionCR);
				}
			}

			yield break;
		}
	}
}