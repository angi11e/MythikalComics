using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.NightMare
{
	public class CowabungaItIsCardController : NightMareBaseCardController
	{
		/*
		 * {NightMare} deals up to 3 targets 2 Melee damage each.
		 * 
		 * DISCARD
		 * Select a card from your trash and discard it.
		 */

		public CowabungaItIsCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
		}

		public override IEnumerator Play()
		{
			// {NightMare} deals up to 3 targets 2 Melee damage each.
			IEnumerator dealDamageCR = GameController.SelectTargetsAndDealDamage(
				DecisionMaker,
				new DamageSource(GameController, this.CharacterCard),
				2,
				DamageType.Melee,
				3,
				false,
				0,
				cardSource: GetCardSource()
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(dealDamageCR);
			}
			else
			{
				GameController.ExhaustCoroutine(dealDamageCR);
			}
			yield break;
		}

		protected override IEnumerator DiscardResponse(GameAction ga)
		{
			// Select a card from your trash and discard it.
			if (TurnTaker.Trash.IsEmpty)
			{
				IEnumerator promptCR = GameController.SendMessageAction(
					"No cards in the trash to discard",
					Priority.Medium,
					GetCardSource()
				);
				if (UseUnityCoroutines)
				{
					yield return GameController.StartCoroutine(promptCR);
				}
				else
				{
					GameController.ExhaustCoroutine(promptCR);
				}
			}
			else
			{
				List<MoveCardAction> storedResults = new List<MoveCardAction>();
				IEnumerator moveCardCR = GameController.SelectCardFromLocationAndMoveIt(
					DecisionMaker,
					TurnTaker.Trash,
					new LinqCardCriteria((Card c) => true),
					new MoveCardDestination[] { new MoveCardDestination(HeroTurnTaker.Hand) },
					optional: true,
					storedResultsMove: storedResults,
					cardSource: new CardSource(FindCardController(this.CharacterCard))
				);

				if (UseUnityCoroutines)
				{
					yield return GameController.StartCoroutine(moveCardCR);
				}
				else
				{
					GameController.ExhaustCoroutine(moveCardCR);
				}

				Card theCard = storedResults.FirstOrDefault().CardToMove;
				if (theCard != null && theCard.Location != TurnTaker.Trash)
				{
					IEnumerator discardCR = GameController.MoveCard(
						DecisionMaker,
						theCard,
						TurnTaker.Trash,
						isDiscard: true,
						cardSource: GetCardSource()
					);

					if (UseUnityCoroutines)
					{
						yield return GameController.StartCoroutine(discardCR);
					}
					else
					{
						GameController.ExhaustCoroutine(discardCR);
					}
				}
			}

			yield break;
		}
	}
}