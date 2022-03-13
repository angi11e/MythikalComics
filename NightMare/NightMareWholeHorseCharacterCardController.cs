using System;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Angille.NightMare
{
	public class NightMareWholeHorseCharacterCardController : HeroCharacterCardController
	{
		public NightMareWholeHorseCharacterCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
		}

		public override IEnumerator UsePower(int index = 0)
		{
			// Discard 1 card...
			int discardNumeral = GetPowerNumeral(0, 1);
			List<DiscardCardAction> storedResults = new List<DiscardCardAction>();
			IEnumerator discardCR = SelectAndDiscardCards(
				DecisionMaker,
				discardNumeral,
				storedResults: storedResults
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(discardCR);
			}
			else
			{
				GameController.ExhaustCoroutine(discardCR);
			}

			// ...play any cards discarded this way from your trash.
			for (int i = storedResults.Count() - 1; i >= 0; i--)
			{
				Card theCard = storedResults.ElementAt(i).CardToDiscard;
				if (DidDiscardCards(storedResults) && theCard != null)
				{
					IEnumerator playCardCR = GameController.MoveCard(
						DecisionMaker,
						theCard,
						TurnTaker.PlayArea,
						cardSource: GetCardSource()
					);

					if (UseUnityCoroutines)
					{
						yield return GameController.StartCoroutine(playCardCR);
					}
					else
					{
						GameController.ExhaustCoroutine(playCardCR);
					}
				}
			}

			yield break;
		}

		public override IEnumerator UseIncapacitatedAbility(int index)
		{
			switch (index)
			{
				case 0:
					// One hero may use a power.
					IEnumerator usePowerCR = GameController.SelectHeroToUsePower(
						DecisionMaker,
						cardSource: GetCardSource()
					);
					if (UseUnityCoroutines)
					{
						yield return GameController.StartCoroutine(usePowerCR);
					}
					else
					{
						GameController.ExhaustCoroutine(usePowerCR);
					}
					break;

				case 1:
					// Each player may discard 2 cards. Any player that does may play a card.
					IEnumerator discardAndPlayCR = GameController.SelectTurnTakersAndDoAction(
						DecisionMaker,
						new LinqTurnTakerCriteria((TurnTaker tt) => tt.IsHero && !tt.IsIncapacitatedOrOutOfGame && tt.ToHero().Hand.NumberOfCards >= 2),
						SelectionType.DiscardCard,
						DiscardAndPlayResponse,
						requiredDecisions: 0,
						allowAutoDecide: true,
						cardSource: GetCardSource()
					);
					if (UseUnityCoroutines)
					{
						yield return GameController.StartCoroutine(discardAndPlayCR);
					}
					else
					{
						GameController.ExhaustCoroutine(discardAndPlayCR);
					}
					break;

				case 2:
					// Reveal the top card of a deck. Put it into play or discard it.
					List<SelectLocationDecision> storedResults = new List<SelectLocationDecision>();
					IEnumerator selectDeckCR = GameController.SelectADeck(
						DecisionMaker,
						SelectionType.RevealTopCardOfDeck,
						(Location l) => l.IsDeck && !l.OwnerTurnTaker.IsIncapacitatedOrOutOfGame,
						storedResults,
						cardSource: GetCardSource()
					);
					if (UseUnityCoroutines)
					{
						yield return GameController.StartCoroutine(selectDeckCR);
					}
					else
					{
						GameController.ExhaustCoroutine(selectDeckCR);
					}

					Location selectedLocation = GetSelectedLocation(storedResults);
					if (selectedLocation != null)
					{
						IEnumerator revealAndStuffCR = RevealCard_PlayItOrDiscardIt(
							TurnTakerController,
							selectedLocation,
							isPutIntoPlay: true,
							responsibleTurnTaker: base.TurnTaker
						);
						if (UseUnityCoroutines)
						{
							yield return GameController.StartCoroutine(revealAndStuffCR);
						}
						else
						{
							GameController.ExhaustCoroutine(revealAndStuffCR);
						}
					}
					break;
			}
			yield break;
		}

		private IEnumerator DiscardAndPlayResponse(TurnTaker tt)
		{
			List<DiscardCardAction> storedResults = new List<DiscardCardAction>();
			IEnumerator discardCR = SelectAndDiscardCards(
				FindHeroTurnTakerController(tt.ToHero()),
				2,
				true,
				storedResults: storedResults
			);
			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(discardCR);
			}
			else
			{
				GameController.ExhaustCoroutine(discardCR);
			}

			if (DidDiscardCards(storedResults, 2))
			{
				IEnumerator playCardCR = SelectAndPlayCardFromHand(FindHeroTurnTakerController(tt.ToHero()));
				if (UseUnityCoroutines)
				{
					yield return GameController.StartCoroutine(playCardCR);
				}
				else
				{
					GameController.ExhaustCoroutine(playCardCR);
				}
			}
			yield break;
		}
	}
}