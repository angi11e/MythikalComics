using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.Patina
{
	public class LabCoatCardController : PatinaBaseCardController
	{
		/*
		 * Whenever you play an equipment card from your hand, draw a card.
		 * 
		 * POWER
		 * You may move 1 equipment card from your trash into play.
		 * If no cards enter play this way, 1 hero other than {Patina} may draw 1 card now.
		 */

		public LabCoatCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
		}

		public override void AddTriggers()
		{
			// Whenever you play an equipment card from your hand, you may draw a card.
			AddTrigger(
				(CardEntersPlayAction p) =>
					!p.IsPutIntoPlay
					&& p.TurnTakerController == this.TurnTakerController
					&& p.Origin == this.HeroTurnTaker.Hand
					&& IsEquipment(p.CardEnteringPlay),
				DrawCardResponse,
				TriggerType.DrawCard,
				TriggerTiming.After
			);

			base.AddTriggers();
		}

		private IEnumerator DrawCardResponse(CardEntersPlayAction c)
		{
			string message = $"{this.Card.Title} allows {this.TurnTaker.Name} to draw a card.";
			IEnumerator messageCR = GameController.SendMessageAction(message, Priority.Medium, GetCardSource());
			IEnumerator drawCardCR = DrawCard(null, true);
			
			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(messageCR);
				yield return GameController.StartCoroutine(drawCardCR);
			}
			else
			{
				GameController.ExhaustCoroutine(messageCR);
				GameController.ExhaustCoroutine(drawCardCR);
			}
		}

		public override IEnumerator UsePower(int index = 0)
		{
			int salvageNumeral = GetPowerNumeral(0, 1);
			int heroNumeral = GetPowerNumeral(1, 1);
			int drawNumeral = GetPowerNumeral(2, 1);

			// You may move 1 equipment card from your trash into play.
			List<SelectCardDecision> storedResults = new List<SelectCardDecision>();
			IEnumerator moveCardCR = SearchForCards(
				DecisionMaker,
				false,
				true,
				salvageNumeral,
				salvageNumeral,
				new LinqCardCriteria((Card c) => IsEquipment(c)),
				true,
				false,
				false,
				true,
				storedResults
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(moveCardCR);
			}
			else
			{
				GameController.ExhaustCoroutine(moveCardCR);
			}

			// If no cards enter play this way...
			if (!storedResults.Where((SelectCardDecision scd) => scd.SelectedCard.IsInPlayAndHasGameText).Any())
			{
				// ...1 hero other than {Patina} may draw 1 card now.
				IEnumerator selectDrawerCR = GameController.SelectTurnTakersAndDoAction(
					DecisionMaker,
					new LinqTurnTakerCriteria(
						(TurnTaker tt) => IsHero(tt) && tt.ToHero().HasCardsInHand && tt != this.TurnTaker
					),
					SelectionType.DrawCard,
					(TurnTaker tt) => DrawCards(
						FindHeroTurnTakerController(tt.ToHero()),
						drawNumeral,
						true
					),
					heroNumeral,
					false,
					heroNumeral,
					cardSource: GetCardSource()
				);
				if (UseUnityCoroutines)
				{
					yield return GameController.StartCoroutine(selectDrawerCR);
				}
				else
				{
					GameController.ExhaustCoroutine(selectDrawerCR);
				}
			}

			yield break;
		}
	}
}