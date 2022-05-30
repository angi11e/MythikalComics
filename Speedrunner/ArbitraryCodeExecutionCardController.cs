using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.Speedrunner
{
	public class ArbitraryCodeExecutionCardController : SpeedrunnerBaseCardController
	{
		/*
		 * When this card enters play, put one one-shot card from each trash under it.
		 * When this card is destroyed, first play each card from under it, in any order.
		 * 
		 * POWER
		 * Put 1 card from your hand under this card. Destroy this card.
		 */

		public ArbitraryCodeExecutionCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
		}

		public override IEnumerator Play()
		{
			// When this card enters play, put one one-shot card from each trash under it.
			IEnumerator grabCardCR = GameController.SelectTurnTakersAndDoAction(
				DecisionMaker,
				new LinqTurnTakerCriteria(
					(TurnTaker tt) => tt.Trash.Cards.Where((Card c) => c.IsOneShot).Any()
				),
				SelectionType.MoveCardToUnderCard,
				(TurnTaker tt) => GameController.SelectCardsFromLocationAndMoveThem(
					DecisionMaker,
					tt.Trash,
					1,
					1,
					new LinqCardCriteria((Card c) => c.IsOneShot),
					new MoveCardDestination[] { new MoveCardDestination(this.Card.UnderLocation) },
					cardSource: GetCardSource()
				),
				cardSource: GetCardSource()
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(grabCardCR);
			}
			else
			{
				GameController.ExhaustCoroutine(grabCardCR);
			}

			yield break;
		}

		public override void AddTriggers()
		{
			// When this card is destroyed...
			AddWhenDestroyedTrigger(DestructionResponse, new TriggerType[1] { TriggerType.PlayCard });

			base.AddTriggers();
		}

		private IEnumerator DestructionResponse(DestroyCardAction dca)
		{
			// ...first play each card from under it, in any order.
			while (this.Card.UnderLocation.Cards.Count() > 0)
			{
				IEnumerator playCR = GameController.SelectCardFromLocationAndMoveIt(
					DecisionMaker,
					this.Card.UnderLocation,
					new LinqCardCriteria((Card c) => true),
					new MoveCardDestination[] { new MoveCardDestination(this.TurnTaker.PlayArea) },
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
			}

			yield break;
		}

		public override IEnumerator UsePower(int index = 0)
		{
			int extraNumeral = GetPowerNumeral(0, 1);

			// Put 1 card from your hand under this card.
			IEnumerator moveCardCR = GameController.SelectCardsFromLocationAndMoveThem(
				DecisionMaker,
				this.HeroTurnTaker.Hand,
				extraNumeral,
				extraNumeral,
				new LinqCardCriteria((Card c) => true),
				new MoveCardDestination[] { new MoveCardDestination(this.Card.UnderLocation) },
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

			// Destroy this card.
			IEnumerator destroyCR = GameController.DestroyCard(
				DecisionMaker,
				this.Card,
				optional: false,
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