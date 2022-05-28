using System;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Angille.Speedrunner
{
	public class SpeedrunnerCharacterCardController : HeroCharacterCardController
	{
		public SpeedrunnerCharacterCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
		}

		public override IEnumerator UsePower(int index = 0)
		{
			int reduceNumeral = GetPowerNumeral(0, 1);
			int drawNumeral = GetPowerNumeral(1, 1);

			// The next time {Speedrunner} would be dealt damage, reduce it by 1 and draw 1 card.
			OnDealDamageStatusEffect damageEffect = new OnDealDamageStatusEffect(
				CardWithoutReplacements,
				"BoostResponse",
				$"The next time that {this.CharacterCard.Title} is dealt damage, it's reduced by {reduceNumeral} and ze draws {drawNumeral} card{(drawNumeral == 1 ? "" : "s")}.",
				new TriggerType[] { TriggerType.ReduceDamageOneUse, TriggerType.DrawCard },
				DecisionMaker.TurnTaker,
				this.Card
			);
			damageEffect.TargetCriteria.IsSpecificCard = this.CharacterCard;
			damageEffect.TargetLeavesPlayExpiryCriteria.Card = this.CharacterCard;
			damageEffect.NumberOfUses = 1;
			// damageEffect.DoesDealDamage = true;
			damageEffect.BeforeOrAfter = BeforeOrAfter.Before;

			IEnumerator addStatusCR = AddStatusEffect(damageEffect);
			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(addStatusCR);
			}
			else
			{
				GameController.ExhaustCoroutine(addStatusCR);
			}

			yield break;
		}

		public IEnumerator BoostResponse(DealDamageAction dd, TurnTaker hero, StatusEffect effect, int[] powerNumerals = null)
		{
			if (powerNumerals == null)
			{
				powerNumerals = new int[2] {1,1};
			}

			// ...reduce it by 1...
			IEnumerator reduceCR = GameController.ReduceDamage(
				dd,
				powerNumerals[0],
				null,
				GetCardSource()
			);

			// ...and draw 1 card.
			IEnumerator drawCR = DrawCards(this.HeroTurnTakerController, powerNumerals[1]);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(reduceCR);
				yield return GameController.StartCoroutine(drawCR);
			}
			else
			{
				GameController.ExhaustCoroutine(reduceCR);
				GameController.ExhaustCoroutine(drawCR);
			}
			yield break;
		}


		public override IEnumerator UseIncapacitatedAbility(int index)
		{
			switch (index)
			{
				case 0:
					// One Player may Play a card now.
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
					// One Player may Discard 2 cards. If they do, they may Draw 2 cards.
					List<SelectTurnTakerDecision> storedTurnTaker = new List<SelectTurnTakerDecision>();
					List<DiscardCardAction> storedDiscards = new List<DiscardCardAction>();
					IEnumerator discardCR = GameController.SelectHeroToDiscardCards(
						DecisionMaker,
						0,
						2,
						optionalDiscardCard: true,
						storedResultsTurnTaker: storedTurnTaker,
						storedResultsDiscard: storedDiscards,
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

					if (!DidDiscardCards(storedDiscards, 2))
					{
						break;
					}

					SelectTurnTakerDecision selectTurnTakerDecision = storedTurnTaker.FirstOrDefault();
					if (selectTurnTakerDecision != null && selectTurnTakerDecision.SelectedTurnTaker.IsHero)
					{
						IEnumerator drawCR = DrawCards(
							FindHeroTurnTakerController(selectTurnTakerDecision.SelectedTurnTaker.ToHero()),
							2,
							optional: true
						);

						if (UseUnityCoroutines)
						{
							yield return GameController.StartCoroutine(drawCR);
						}
						else
						{
							GameController.ExhaustCoroutine(drawCR);
						}
					}
					break;
				case 2:
					// Reveal the Top card of the Villain Deck, then replace it.
					List<SelectLocationDecision> storedResults = new List<SelectLocationDecision>();
					IEnumerator findDeckCR = FindVillainDeck(
						DecisionMaker,
						SelectionType.RevealTopCardOfDeck,
						storedResults,
						(Location l) => true
					);

					if (UseUnityCoroutines)
					{
						yield return GameController.StartCoroutine(findDeckCR);
					}
					else
					{
						GameController.ExhaustCoroutine(findDeckCR);
					}

					Location deck = GetSelectedLocation(storedResults);
					if (deck != null)
					{
						List<Card> revealedCards = new List<Card>();
						IEnumerator revealCR = GameController.RevealCards(
							TurnTakerController,
							deck,
							1,
							revealedCards,
							revealedCardDisplay: RevealedCardDisplay.Message,
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

						List<Location> list = new List<Location>();
						list.Add(deck.OwnerTurnTaker.Revealed);
						IEnumerator cleanupCR = CleanupCardsAtLocations(
							list,
							deck,
							cardsInList: revealedCards
						);

						if (UseUnityCoroutines)
						{
							yield return GameController.StartCoroutine(cleanupCR);
						}
						else
						{
							GameController.ExhaustCoroutine(cleanupCR);
						}
					}
					break;
			}
			yield break;
		}
	}
}