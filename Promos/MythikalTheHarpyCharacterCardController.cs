using System;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Angille.TheHarpy
{
	public class MythikalTheHarpyCharacterCardController : HeroCharacterCardController
	{
		public MythikalTheHarpyCharacterCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
			TokenPool arcanaPool = this.Card.FindTokenPool(TokenPool.ArcanaControlPool);
			TokenPool avianPool = this.Card.FindTokenPool(TokenPool.AvianControlPool);

			SpecialStringMaker.ShowSpecialString(() => String.Format(
				"The Harpy has {0} {1} and {2} {3} control tokens.",
				arcanaPool.CurrentValue,
				"{arcana}",
				avianPool.CurrentValue,
				"{avian}"
			));
		}

		private List<Card> actedHeroes;

		public override IEnumerator UsePower(int index = 0)
		{
			int tokensNumeral = GetPowerNumeral(0, 1);
			int drawNumeral = GetPowerNumeral(1, 1);
			int targetNumeral = GetPowerNumeral(2, 1);
			int damageNumeral = GetPowerNumeral(3, 1);
			TokenPool arcanaPool = this.Card.FindTokenPool(TokenPool.ArcanaControlPool);

			for (int i = 0; i < tokensNumeral; i++)
			{
				int startingArcana = arcanaPool.CurrentValue;

				// Flip 1 control token.
				IEnumerator flipCR = FlipControlToken();
				if (UseUnityCoroutines)
				{
					yield return GameController.StartCoroutine(flipCR);
				}
				else
				{
					GameController.ExhaustCoroutine(flipCR);
				}

				if (arcanaPool.CurrentValue > startingArcana)
				{
					// When a {avian} token is flipped this way, draw 1 card.
					IEnumerator drawCardCR = DrawCards(this.HeroTurnTakerController, drawNumeral);
					if (UseUnityCoroutines)
					{
						yield return GameController.StartCoroutine(drawCardCR);
					}
					else
					{
						GameController.ExhaustCoroutine(drawCardCR);
					}
				}
				else if (arcanaPool.CurrentValue < startingArcana)
				{
					// When a {arcana} token is flipped this way, [i]Pinion[/i] deals 1 target 1 infernal damage.
					IEnumerator dealDamageCR = GameController.SelectTargetsAndDealDamage(
						DecisionMaker,
						new DamageSource(GameController, this.CharacterCard),
						damageNumeral,
						DamageType.Infernal,
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

			yield break;
		}

		public IEnumerator FlipControlToken()
		{
			TokenPool avianPool = this.Card.FindTokenPool(TokenPool.AvianControlPool);
			TokenPool arcanaPool = this.Card.FindTokenPool(TokenPool.ArcanaControlPool);
			if (avianPool == null || !TurnTaker.IsHero)
			{
				TurnTaker turnTaker = FindTurnTakersWhere((TurnTaker tt) => tt.Identifier == "TheHarpy").FirstOrDefault();
				if (turnTaker != null)
				{
					avianPool = turnTaker.CharacterCard.FindTokenPool(TokenPool.AvianControlPool);
					arcanaPool = turnTaker.CharacterCard.FindTokenPool(TokenPool.ArcanaControlPool);
				}
			}

			if (avianPool != null && arcanaPool != null)
			{
				string text;
				if (arcanaPool.CurrentValue == arcanaPool.MaximumValue)
				{
					text = "{arcana}";
				}
				else if (avianPool.CurrentValue == avianPool.MaximumValue)
				{
					text = "{avian}";
				}
				else
				{
					List<SelectWordDecision> storedResults = new List<SelectWordDecision>();
					IEnumerator selectCR = GameController.SelectWord(
						DecisionMaker,
						new string[2] { "{arcana}", "{avian}" },
						SelectionType.HarpyTokenType,
						storedResults,
						optional: false,
						cardSource: GetCardSource()
					);

					if (UseUnityCoroutines)
					{
						yield return GameController.StartCoroutine(selectCR);
					}
					else
					{
						GameController.ExhaustCoroutine(selectCR);
					}
					text = GetSelectedWord(storedResults);
				}
				if (text != null)
				{
					TokenPool originPool;
					TokenPool destinationPool;
					if (text == "{avian}")
					{
						originPool = avianPool;
						destinationPool = arcanaPool;
					}
					else
					{
						originPool = arcanaPool;
						destinationPool = avianPool;
					}

					IEnumerator flipCR = FlipTokens(originPool, destinationPool);
					if (UseUnityCoroutines)
					{
						yield return GameController.StartCoroutine(flipCR);
					}
					else
					{
						GameController.ExhaustCoroutine(flipCR);
					}
				}
			}
			else
			{
				IEnumerator emptyCR = GameController.SendMessageAction(
					"There are no control tokens in play.", 
					Priority.Medium,
					GetCardSource(),
					showCardSource: true
				);

				if (UseUnityCoroutines)
				{
					yield return GameController.StartCoroutine(emptyCR);
				}
				else
				{
					GameController.ExhaustCoroutine(emptyCR);
				}
			}
		}

		public override IEnumerator UseIncapacitatedAbility(int index)
		{
			switch (index)
			{
				case 0:
					// One Player may play a card.
					IEnumerator playCR = SelectHeroToPlayCard(
						this.HeroTurnTakerController,
						heroCriteria: new LinqTurnTakerCriteria(
							(TurnTaker tt) => IsHero(tt) && !tt.IsIncapacitated
						)
					);

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
					// Destroy an Ongoing card.
					IEnumerator destroyOngoingCR = GameController.SelectAndDestroyCard(
						DecisionMaker,
						new LinqCardCriteria((Card c) => IsOngoing(c) && c.IsInPlay, "ongoing"),
						false,
						cardSource: GetCardSource()
					);

					if (UseUnityCoroutines)
					{
						yield return GameController.StartCoroutine(destroyOngoingCR);
					}
					else
					{
						GameController.ExhaustCoroutine(destroyOngoingCR);
					}
					break;
				case 2:
					// Each hero may deal themself 1 psychic damage.
					// Any who take damage this way each deal 1 target 1 infernal damage.
					this.actedHeroes = new List<Card>();
					IEnumerable<Function> functionsBasedOnCard(Card c) => new Function[]
					{
						new Function(
							FindCardController(c).DecisionMaker,
							"Deal self 1 psychic damage to deal 1 target 1 infernal damage.",
							SelectionType.DealDamageSelf,
							() => this.SelfAndOtherDamageResponse(c)
						)
					};
					IEnumerator selectHeroesCR = GameController.SelectCardsAndPerformFunction(
						DecisionMaker,
						new LinqCardCriteria(
							(Card c) => IsHeroCharacterCard(c)
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
				List<DealDamageAction> damageResults = new List<DealDamageAction>();
				IEnumerator selfDamageCR = DealDamage(
					card,
					card,
					1,
					DamageType.Psychic,
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
					IEnumerator otherDamageCR = GameController.SelectTargetsAndDealDamage(
						FindHeroTurnTakerController(card.Owner.ToHero()),
						new DamageSource(GameController, card),
						1,
						DamageType.Infernal,
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