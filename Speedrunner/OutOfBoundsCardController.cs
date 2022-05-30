using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.Speedrunner
{
	public class OutOfBoundsCardController : SpeedrunnerBaseCardController
	{
		/*
		 * When this card enters play, move the top card of the environment deck under it.
		 * Then {Speedrunner} deals 1 target 2 psychic damage.
		 * 
		 * POWER
		 * Swap the locations of an environment card in play with a card under this card.
		 * Then {Speedrunner} deals 1 target 2 psychic damage.
		 */

		public OutOfBoundsCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
		}

		public override void AddTriggers()
		{
			// When this card is destroyed...
			AddBeforeLeavesPlayActions(ReturnCardsToOwnersTrashResponse);

			base.AddTriggers();
		}

		public override IEnumerator Play()
		{
			// When this card enters play, move the top card of the environment deck under it.
			IEnumerator grabEnviroCR = GameController.MoveCard(
				DecisionMaker,
				FindEnvironment().TurnTaker.Deck.TopCard,
				this.Card.UnderLocation,
				cardSource: GetCardSource()
			);

			// Then {Speedrunner} deals 1 target 2 psychic damage.
			IEnumerator dealDamageCR = GameController.SelectTargetsAndDealDamage(
				DecisionMaker,
				new DamageSource(GameController, this.CharacterCard),
				2,
				DamageType.Psychic,
				1,
				false,
				1,
				cardSource: GetCardSource()
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(grabEnviroCR);
				yield return GameController.StartCoroutine(dealDamageCR);
			}
			else
			{
				GameController.ExhaustCoroutine(grabEnviroCR);
				GameController.ExhaustCoroutine(dealDamageCR);
			}

			yield break;
		}

		public override IEnumerator UsePower(int index = 0)
		{
			int targetNumeral = GetPowerNumeral(0, 1);
			int damageNumeral = GetPowerNumeral(1, 2);

			// Swap the locations of an environment card in play with a card under this card.
			if (
				AllCards.Where((Card c) => c.IsInPlayAndNotUnderCard && c.IsEnvironment).Any()
				&& this.Card.UnderLocation.HasCards
			)
			{
				List<SelectCardDecision> selectedEnviro = new List<SelectCardDecision>();
				IEnumerator selectEnviroCR = GameController.SelectCardAndStoreResults(
					DecisionMaker,
					SelectionType.MoveCardToUnderCard,
					new LinqCardCriteria((Card c) => c.IsInPlayAndNotUnderCard && c.IsEnvironment),
					selectedEnviro,
					false,
					cardSource: GetCardSource()
				);

				List<SelectCardDecision> selectedUnder = new List<SelectCardDecision>();
				IEnumerator selectUnderCR = GameController.SelectCardAndStoreResults(
					DecisionMaker,
					SelectionType.MoveCardFromUnderCard,
					new LinqCardCriteria((Card c) => c.Location == this.Card.UnderLocation),
					selectedUnder,
					false,
					cardSource: GetCardSource()
				);

				if (UseUnityCoroutines)
				{
					yield return GameController.StartCoroutine(selectEnviroCR);
					yield return GameController.StartCoroutine(selectUnderCR);
				}
				else
				{
					GameController.ExhaustCoroutine(selectEnviroCR);
					GameController.ExhaustCoroutine(selectUnderCR);
				}

				if (selectedEnviro != null && selectedUnder != null & selectedEnviro.Any() && selectedUnder.Any())
				{
					IEnumerator deployCR = GameController.MoveCard(
						DecisionMaker,
						selectedUnder.FirstOrDefault().SelectedCard,
						selectedEnviro.FirstOrDefault().SelectedCard.Location,
						isPutIntoPlay: true,
						playCardIfMovingToPlayArea: false,
						cardSource: GetCardSource()
					);
					IEnumerator retractCR = GameController.MoveCard(
						DecisionMaker,
						selectedEnviro.FirstOrDefault().SelectedCard,
						this.Card.UnderLocation,
						cardSource: GetCardSource()
					);

					if (UseUnityCoroutines)
					{
						yield return GameController.StartCoroutine(deployCR);
						yield return GameController.StartCoroutine(retractCR);
					}
					else
					{
						GameController.ExhaustCoroutine(deployCR);
						GameController.ExhaustCoroutine(retractCR);
					}
				}
			}
			else
			{
				IEnumerator missingMessageCR = GameController.SendMessageAction(
					"cannot swap a card when there's nothing to swap with",
					Priority.Medium,
					GetCardSource(),
					showCardSource: true
				);

				if (UseUnityCoroutines)
				{
					yield return GameController.StartCoroutine(missingMessageCR);
				}
				else
				{
					GameController.ExhaustCoroutine(missingMessageCR);
				}
			}

			// Then {Speedrunner} deals 1 target 2 psychic damage.
			IEnumerator dealDamageCR = GameController.SelectTargetsAndDealDamage(
				DecisionMaker,
				new DamageSource(GameController, this.CharacterCard),
				damageNumeral,
				DamageType.Psychic,
				targetNumeral,
				false,
				targetNumeral,
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
	}
}