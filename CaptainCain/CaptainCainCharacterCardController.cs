using System;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Angille.CaptainCain
{
	public class CaptainCainCharacterCardController : HeroCharacterCardController
	{
		public CaptainCainCharacterCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
		}

		private List<Card> actedHeroes;

		public override IEnumerator UsePower(int index = 0)
		{
			int targetNumeral = GetPowerNumeral(0, 1);
			int infernalNumeral = GetPowerNumeral(1, 1);
			int meleeNumeral = GetPowerNumeral(2, 1);
			int healingNumeral = GetPowerNumeral(3, 1);

			// {CaptainCainCharacter} deals 1 target 1 infernal damage.
			List<DealDamageAction> storedDamage = new List<DealDamageAction>();
			IEnumerator firstDamageCR = GameController.SelectTargetsAndDealDamage(
				DecisionMaker,
				new DamageSource(GameController, this.CharacterCard),
				infernalNumeral,
				DamageType.Infernal,
				targetNumeral,
				false,
				targetNumeral,
				storedResultsDamage: storedDamage,
				cardSource: GetCardSource()
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(firstDamageCR);
			}
			else
			{
				GameController.ExhaustCoroutine(firstDamageCR);
			}

			// If {Fist} is active, he deals that target 1 melee damage.
			if (HeroTurnTaker.GetCardsWhere(
				(Card c) => c.IsInPlayAndNotUnderCard
				&& GameController.DoesCardContainKeyword(c, "fist")
				&& c.Owner == this.Card.Owner
			).Count() > 0)
			{
				foreach (DealDamageAction item in storedDamage)
				{
					if (!item.DidDestroyTarget) {
						IEnumerator secondDamageCR = DealDamage(
							this.CharacterCard,
							item.Target,
							meleeNumeral,
							DamageType.Melee,
							cardSource: GetCardSource()
						);

						if (UseUnityCoroutines)
						{
							yield return GameController.StartCoroutine(secondDamageCR);
						}
						else
						{
							GameController.ExhaustCoroutine(secondDamageCR);
						}
					}
				}
			}

			// If {Blood} is active, he regains 1 HP.
			if (HeroTurnTaker.GetCardsWhere(
				(Card c) => c.IsInPlayAndNotUnderCard
				&& GameController.DoesCardContainKeyword(c, "blood")
				&& c.Owner == this.Card.Owner
			).Count() > 0)
			{
				IEnumerator healingCR = GameController.GainHP(
					this.Card,
					healingNumeral,
					cardSource: GetCardSource()
				);

				if (UseUnityCoroutines)
				{
					yield return GameController.StartCoroutine(healingCR);
				}
				else
				{
					GameController.ExhaustCoroutine(healingCR);
				}
			}

			yield break;
		}

		public override IEnumerator UseIncapacitatedAbility(int index)
		{
			switch (index)
			{
				case 0:
					// One hero may use a power now.
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
					// One player may play a card.
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

				case 2:
					// Each hero may deal themself 1 toxic damage.
					// Any who take damage this way each deal 1 target 1 infernal damage.
					this.actedHeroes = new List<Card>();
					IEnumerable<Function> functionsBasedOnCard(Card c) => new Function[]
					{
						new Function(
							FindCardController(c).DecisionMaker,
							"Deal self 1 psychic damage to draw a card now.",
							SelectionType.DrawCard,
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
				List<DealDamageAction> damageResults = new List<DealDamageAction>();
				IEnumerator selfDamageCR = DealDamage(
					card,
					card,
					1,
					DamageType.Toxic,
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