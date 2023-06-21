using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.Supplicate
{
	public class RealityWeaveCardController : SupplicateBaseCardController
	{
		/*
		 * if a hero character would be dealt 3 or more damage from a single source,
		 * you may reduce that damage to 0. if you do, destroy this card.
		 * 
		 * POWER:
		 * move 1 hero ongoing or equipment card from a trash to its owner's hand.
		 * 1 player may play a card.
		 */

		private ITrigger _reduceDamageTrigger;
		private bool _reduction;

		public RealityWeaveCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
			_reduction = false;
			_reduceDamageTrigger = null;
			AllowFastCoroutinesDuringPretend = false;
		}

		public override void AddTriggers()
		{
			base.AddTriggers();

			// if a hero character would be dealt 3 or more damage from a single source,
			_reduceDamageTrigger = AddTrigger(
				(DealDamageAction dda) => dda.Amount >= 3 && IsHeroCharacterCard(dda.Target),
				DestroyAndReduceResponse,
				new TriggerType[] { TriggerType.DestroySelf, TriggerType.ReduceDamage },
				TriggerTiming.Before,
				orderMatters: true
			);
		}

		private IEnumerator DestroyAndReduceResponse(DealDamageAction dda)
		{
			// you may reduce that damage to 0.
			if (GameController.PretendMode || !_reduction)
			{
				YesNoDecision decision = new YesNoDecision(
					GameController,
					DecisionMaker,
					SelectionType.ReduceDamageTaken,
					gameAction: dda,
					associatedCards: new Card[1] { dda.Target },
					cardSource: GetCardSource()
				);
				IEnumerator yesNoCR = GameController.MakeDecisionAction(decision);

				if (UseUnityCoroutines)
				{
					yield return GameController.StartCoroutine(yesNoCR);
				}
				else
				{
					GameController.ExhaustCoroutine(yesNoCR);
				}

				if (DidPlayerAnswerYes(decision))
				{
					_reduction = true;
				}
			}

			if (_reduction)
			{
				// If you do...
				ReduceDamageAction reduceDamage = new ReduceDamageAction(
					GetCardSource(),
					dda,
					dda.Amount,
					_reduceDamageTrigger
				);
				IEnumerator reduceCR = DoAction(reduceDamage);

				// ...destroy this card.
				IEnumerator destroyCR = GameController.DestroyCard(
					DecisionMaker,
					this.Card,
					cardSource: GetCardSource()
				);

				if (UseUnityCoroutines)
				{
					yield return GameController.StartCoroutine(reduceCR);
					yield return GameController.StartCoroutine(destroyCR);
				}
				else
				{
					GameController.ExhaustCoroutine(reduceCR);
					GameController.ExhaustCoroutine(destroyCR);
				}
			}

			if (!GameController.PretendMode)
			{
				_reduction = false;
			}

			yield break;
		}

		public override IEnumerator UsePower(int index = 0)
		{
			int moveNumeral = GetPowerNumeral(0, 1);
			int playNumeral = GetPowerNumeral(1, 1);

			// move 1 hero ongoing or equipment card from a trash to its owner's hand.
			List<SelectCardsDecision> selectedCards = new List<SelectCardsDecision>();

			// select the card
			IEnumerator selectCardCR = GameController.SelectCardsAndStoreResults(
				DecisionMaker,
				SelectionType.MoveCardToHandFromTrash,
				(Card c) => c.IsInTrash
					&& GameController.IsLocationVisibleToSource(c.Location, GetCardSource(null))
					&& (IsOngoing(c) || IsEquipment(c))
					&& IsHero(c),
				moveNumeral,
				selectedCards,
				false,
				cardSource: GetCardSource()
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(selectCardCR);
			}
			else
			{
				GameController.ExhaustCoroutine(selectCardCR);
			}

			SelectCardsDecision selectCardsDecision = selectedCards.FirstOrDefault();
			if (selectCardsDecision != null && selectCardsDecision.SelectCardDecisions != null)
			{
				// move each selected card
				foreach (SelectCardDecision cardSelection in selectCardsDecision.SelectCardDecisions)
				{
					IEnumerator moveCardCR = GameController.MoveCard(
						DecisionMaker,
						cardSelection.SelectedCard,
						cardSelection.SelectedCard.Owner.ToHero().Hand,
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

			// 1 player may play a card.
			IEnumerator playCR = SelectHeroToPlayCard(DecisionMaker);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(playCR);
			}
			else
			{
				GameController.ExhaustCoroutine(playCR);
			}

			yield break;
		}
	}
}