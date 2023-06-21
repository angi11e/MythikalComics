using System;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller.OmnitronX;

namespace Angille.OmnitronX
{
	public class MythikalOmnitronMCharacterCardController : HeroCharacterCardController
	{
		public MythikalOmnitronMCharacterCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
		}

		private List<Card> actedHeroes;

		public override IEnumerator UsePower(int index = 0)
		{
			int targetNumeral = GetPowerNumeral(0, 1);
			int damageNumeral = GetPowerNumeral(1, 2);

			// You may play a plating card.
			IEnumerator playCR = SelectAndPlayCardFromHand(
				DecisionMaker,
				false,
				cardCriteria: new LinqCardCriteria((Card c) => c.IsPlating, "plating")
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(playCR);
			}
			else
			{
				GameController.ExhaustCoroutine(playCR);
			}

			// ...of a type currently reduced by a plating card.
			List<DealDamageAction> damages = new List<DealDamageAction>();
			IEnumerable<Card> source = FindCardsWhere((Card c) => c.IsInPlayAndHasGameText && c.IsPlating);
			if (source.Count() > 0)
			{
				IEnumerable<DamageType> damageTypes = source.Select(
					(Card c) => (PlatingCardController)FindCardController(c)
				).SelectMany((PlatingCardController p) => p.DamageTypes).Distinct();

				List<SelectDamageTypeDecision> chosenType = new List<SelectDamageTypeDecision>();
				IEnumerator chooseTypeCR = GameController.SelectDamageType(
					DecisionMaker,
					chosenType,
					damageTypes,
					cardSource: GetCardSource()
				);

				if (UseUnityCoroutines)
				{
					yield return GameController.StartCoroutine(chooseTypeCR);
				}
				else
				{
					GameController.ExhaustCoroutine(chooseTypeCR);
				}

				DamageType? damageType = GetSelectedDamageType(chosenType);
				if (damageType != null)
				{
					// {OmnitronX} deals 1 target 2 damage...
					IEnumerator damageCR = GameController.SelectTargetsAndDealDamage(
						DecisionMaker,
						new DamageSource(GameController, this.CharacterCard),
						damageNumeral,
						damageType.Value,
						targetNumeral,
						false,
						targetNumeral,
						storedResultsDamage: damages,
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
				}
			}

			// If no damage is dealt this way...
			if (!DidDealDamage(damages))
			{
				// ...draw a card.
				if (UseUnityCoroutines)
				{
					yield return GameController.StartCoroutine(DrawCard());
				}
				else
				{
					GameController.ExhaustCoroutine(DrawCard());
				}
			}

			yield break;
		}

		public override IEnumerator UseIncapacitatedAbility(int index)
		{
			switch (index)
			{
				case 0:
					// One player may draw a card.
					IEnumerator drawCardCR = GameController.SelectHeroToDrawCard(
						DecisionMaker,
						cardSource: GetCardSource()
					);

					if (UseUnityCoroutines)
					{
						yield return GameController.StartCoroutine(drawCardCR);
					}
					else
					{
						GameController.ExhaustCoroutine(drawCardCR);
					}
					break;

				case 1:
					// Each player may discard a card.
					List<DiscardCardAction> discardedCards = new List<DiscardCardAction>();
					IEnumerator discardCR = GameController.EachPlayerDiscardsCards(
						0,
						1,
						discardedCards,
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

					// Any player that does...
					foreach (DiscardCardAction item in discardedCards)
					{
						if (item.WasCardDiscarded)
						{
							// ...may play an equipment card.
							IEnumerator playEquipCR = SelectAndPlayCardFromHand(
								FindHeroTurnTakerController(item.CardToDiscard.Owner.ToHero()),
								true,
								cardCriteria: new LinqCardCriteria((Card c) => IsEquipment(c))
							);
							if (UseUnityCoroutines)
							{
								yield return GameController.StartCoroutine(playEquipCR);
							}
							else
							{
								GameController.ExhaustCoroutine(playEquipCR);
							}
						}
					}
					break;

				case 2:
					// Each hero may deal themself 1 energy damage.
					this.actedHeroes = new List<Card>();
					IEnumerable<Function> functionsBasedOnCard(Card c) => new Function[]
					{
						new Function(
							FindCardController(c).DecisionMaker,
							"Deal self 1 energy damage to deal 1 target 1 fire damage.",
							SelectionType.DealDamageSelf,
							() => this.SelfAndOtherDamageResponse(c)
						)
					};
					IEnumerator selectHeroesCR = GameController.SelectCardsAndPerformFunction(
						DecisionMaker,
						new LinqCardCriteria(
							(Card c) => c.IsHeroCharacterCard
								&& c.IsInPlayAndHasGameText
								&& !c.IsIncapacitatedOrOutOfGame
								&& !this.actedHeroes.Contains(c),
							"active hero character cards",
							false
						),
						functionsBasedOnCard,
						true,
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
					break;
			}
			yield break;
		}

		private IEnumerator SelfAndOtherDamageResponse(Card card)
		{
			if (card != null)
			{
				// Any who take damage this way each...
				List<DealDamageAction> damageResults = new List<DealDamageAction>();
				IEnumerator selfDamageCR = DealDamage(
					card,
					card,
					1,
					DamageType.Energy,
					storedResults: damageResults,
					cardSource: GetCardSource()
				);

				if (UseUnityCoroutines)
				{
					yield return GameController.StartCoroutine(selfDamageCR);
				}
				else
				{
					GameController.ExhaustCoroutine(selfDamageCR);
				}

				if (DidDealDamage(damageResults, card, card))
				{
					// ...deal 1 target 1 fire damage.
					IEnumerator otherDamageCR = GameController.SelectTargetsAndDealDamage(
						FindHeroTurnTakerController(card.Owner.ToHero()),
						new DamageSource(GameController, card),
						1,
						DamageType.Fire,
						1,
						false,
						1,
						cardSource: GetCardSource()
					);

					if (UseUnityCoroutines)
					{
						yield return GameController.StartCoroutine(otherDamageCR);
					}
					else
					{
						GameController.ExhaustCoroutine(otherDamageCR);
					}
				}

				LogActedCard(card);
			}
			yield break;
		}

		private void LogActedCard(Card card)
		{
			if (card.SharedIdentifier != null)
			{
				IEnumerable<Card> collection = FindCardsWhere(
					(Card c) => c.SharedIdentifier != null
						&& c.SharedIdentifier == card.SharedIdentifier
						&& c != card
				);
				this.actedHeroes.AddRange(collection);
			}
		}
	}
}