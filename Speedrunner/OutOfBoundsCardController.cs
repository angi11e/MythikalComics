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
		 * Move an environment card in play under this card.
		 *  If you do so, you may play a different card from under this card,
		 *  then {Speedrunner} deals 1 target 2 psychic damage.
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
				showMessage: true,
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

			if (AllCards.Where((Card c) => c.IsInPlayAndNotUnderCard && c.IsEnvironment).Any())
			{
				// Move an environment card in play under this card.
				List<SelectCardDecision> selectedEnviro = new List<SelectCardDecision>();
				IEnumerator selectEnviroCR = GameController.SelectCardAndStoreResults(
					DecisionMaker,
					SelectionType.MoveCardToUnderCard,
					new LinqCardCriteria((Card c) => c.IsInPlayAndNotUnderCard && c.IsEnvironment),
					selectedEnviro,
					false,
					cardSource: GetCardSource()
				);

				if (UseUnityCoroutines)
				{
					yield return GameController.StartCoroutine(selectEnviroCR);
				}
				else
				{
					GameController.ExhaustCoroutine(selectEnviroCR);
				}

				if (selectedEnviro != null && selectedEnviro.Any())
				{
					Card pulledCard = selectedEnviro.FirstOrDefault().SelectedCard;
					IEnumerator retractCR = GameController.MoveCard(
						DecisionMaker,
						pulledCard,
						this.Card.UnderLocation,
						cardSource: GetCardSource()
					);

					if (UseUnityCoroutines)
					{
						yield return GameController.StartCoroutine(retractCR);
					}
					else
					{
						GameController.ExhaustCoroutine(retractCR);
					}

					// If you do...
					if (pulledCard.Location == this.Card.UnderLocation)
					{
						// ...put a different card from under this card into play.
						List<SelectCardDecision> selectedUnderCard = new List<SelectCardDecision>();
						IEnumerator playUnderCR = GameController.SelectCardFromLocationAndMoveIt(
							DecisionMaker,
							this.Card.UnderLocation,
							new LinqCardCriteria((Card c) => c != pulledCard),
							new List<MoveCardDestination> { new MoveCardDestination(FindEnvironment().TurnTaker.PlayArea) },
							isPutIntoPlay: true,
							optional: false,
							storedResults: selectedUnderCard,
							cardSource: GetCardSource()
						);

						if (UseUnityCoroutines)
						{
							yield return GameController.StartCoroutine(playUnderCR);
						}
						else
						{
							GameController.ExhaustCoroutine(playUnderCR);
						}

						// If you do, {Speedrunner} deals 1 target 2 psychic damage.
						if (selectedUnderCard != null && selectedUnderCard.Any())
						{
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
						}
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