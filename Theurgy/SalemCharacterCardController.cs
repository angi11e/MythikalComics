using System;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra;

namespace Angille.Theurgy
{
	public class SalemCharacterCardController : TheurgyBaseCharacterCardController
	{
		public SalemCharacterCardController(Card card, TurnTakerController turnTakerController)
			: base(card, turnTakerController)
		{
		}

		public override IEnumerator UsePower(int index = 0)
		{
			// {Theurgy} deals 1 target X sonic damage,
			//  where X = the number of [u]charm[/u] cards in play plus 1.
			//
			// If that damage destroys a target, you may move a [u]charm[/u] card from your trash into play.

			int targetNumeral = GetPowerNumeral(0, 1);
			int damageMod = GetPowerNumeral(1, 1);

			List<DealDamageAction> storedResults = new List<DealDamageAction>();
			IEnumerator damageCR = GameController.SelectTargetsAndDealDamage(
				DecisionMaker,
				new DamageSource(GameController, this.CharacterCard),
				CharmCardsInPlay + damageMod,
				DamageType.Sonic,
				targetNumeral,
				false,
				targetNumeral,
				storedResultsDamage: storedResults,
				cardSource: GetCardSource()
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(damageCR);
			}
			else
			{
				GameController.ExhaustCoroutine(damageCR);
			}

			if (storedResults.Any((DealDamageAction dd) => dd.DidDestroyTarget))
			{
				IEnumerator moveCardCR = SearchForCards(
					DecisionMaker,
					false,
					true,
					0,
					1,
					IsCharmCriteria(),
					true,
					false,
					false,
					true
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

			yield break;
		}

		public override IEnumerator UseIncapacitatedAbility(int index)
		{
			switch (index)
			{
				case 0:
					// One Hero may use a power now.
					IEnumerator usePowerCR = GameController.SelectHeroToUsePower(DecisionMaker);
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
					// Up to 2 hero targets deal 1 target 1 sonic damage each.
					List<SelectCardsDecision> selectedCards = new List<SelectCardsDecision>();
					IEnumerator selectHeroesCR = GameController.SelectCardsAndStoreResults(
						DecisionMaker,
						SelectionType.CardToDealDamage,
						(Card c) => c.IsInPlayAndHasGameText && IsHeroTarget(c),
						2,
						selectedCards,
						optional: false,
						0,
						eliminateOptions: true,
						allowAutoDecide: false,
						allAtOnce: false,
						null,
						GetCardSource()
					);

					if (UseUnityCoroutines)
					{
						yield return GameController.StartCoroutine(selectHeroesCR);
					}
					else
					{
						GameController.ExhaustCoroutine(selectHeroesCR);
					}

					SelectCardsDecision selectCardsDecision = selectedCards.FirstOrDefault();
					if (selectCardsDecision == null || selectCardsDecision.SelectCardDecisions == null)
					{
						break;
					}
					foreach (SelectCardDecision selectCardDecision in selectCardsDecision.SelectCardDecisions)
					{
						if (selectCardDecision != null && selectCardDecision.SelectedCard != null)
						{
							IEnumerator dealDamageCR = GameController.SelectTargetsAndDealDamage(
								DecisionMaker,
								new DamageSource(
									GameController,
									selectCardDecision.SelectedCard
								),
								1,
								DamageType.Sonic,
								1,
								optional: false,
								1,
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
					break;
				
				case 2:
					// Each player may discard 2 cards. Any player that does may play a card.
					IEnumerator discardPlayCR = GameController.SelectTurnTakersAndDoAction(
						DecisionMaker,
						new LinqTurnTakerCriteria((TurnTaker tt) =>
							IsHero(tt)
							&& !tt.IsIncapacitatedOrOutOfGame
							&& tt.ToHero().Hand.NumberOfCards >= 2
						),
						SelectionType.DiscardCard,
						DiscardAndPlayResponse,
						requiredDecisions: 0,
						allowAutoDecide: true,
						ignoreBattleZone: false,
						cardSource: GetCardSource()
					);

					if (UseUnityCoroutines)
					{
						yield return GameController.StartCoroutine(discardPlayCR);
					}
					else
					{
						GameController.ExhaustCoroutine(discardPlayCR);
					}
					break;
			}
			yield break;
		}

		private IEnumerator DiscardAndPlayResponse(TurnTaker tt)
		{
			HeroTurnTakerController httc = FindHeroTurnTakerController(tt.ToHero());
			List<DiscardCardAction> storedResults = new List<DiscardCardAction>();

			IEnumerator discardCR = SelectAndDiscardCards(
				httc,
				2,
				optional: true,
				null,
				storedResults
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
				IEnumerator playCR = SelectAndPlayCardFromHand(httc);
				if (UseUnityCoroutines)
				{
					yield return GameController.StartCoroutine(playCR);
				}
				else
				{
					GameController.ExhaustCoroutine(playCR);
				}
			}
		}
	}
}