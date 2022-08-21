using System;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Angille.Patina
{
	public class VerdigrisCharacterCardController : HeroCharacterCardController
	{
		public VerdigrisCharacterCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
		}

		public override IEnumerator UsePower(int index = 0)
		{
			int meleeNumeral = GetPowerNumeral(0, 2);
			int coldNumeral = GetPowerNumeral(1, 2);
			int counterNumeral = GetPowerNumeral(4, 3);

			// [i]Verdigris[i] deals a target 2 melee damage and 2 cold damage,
			List<SelectTargetDecision> selectedTarget = new List<SelectTargetDecision>();
			IEnumerable<Card> choices = FindCardsWhere(
				new LinqCardCriteria((Card c) => c.IsTarget && c.IsInPlayAndHasGameText)
			);
			IEnumerator selectTargetCR = GameController.SelectTargetAndStoreResults(
				this.HeroTurnTakerController,
				choices,
				selectedTarget,
				selectionType: SelectionType.SelectTarget,
				cardSource: GetCardSource()
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(selectTargetCR);
			}
			else
			{
				GameController.ExhaustCoroutine(selectTargetCR);
			}

			if (selectedTarget != null && selectedTarget.Any())
			{
				SelectTargetDecision selectedTargetDecision = selectedTarget.FirstOrDefault();
				if (selectedTargetDecision != null && selectedTargetDecision.SelectedCard != null)
				{
					Card theCard = selectedTargetDecision.SelectedCard;

					// ...in either order.
					List<SelectDamageTypeDecision> chosenType = new List<SelectDamageTypeDecision>();
					IEnumerator chooseDamageCR = GameController.SelectDamageType(
						DecisionMaker,
						chosenType,
						new DamageType[] { DamageType.Melee, DamageType.Cold },
						cardSource: GetCardSource()
					);

					if (UseUnityCoroutines)
					{
						yield return GameController.StartCoroutine(chooseDamageCR);
					}
					else
					{
						GameController.ExhaustCoroutine(chooseDamageCR);
					}

					DamageType damageType = chosenType.First(
						(SelectDamageTypeDecision d) => d.Completed
					).SelectedDamageType ?? DamageType.Melee;

					IEnumerator dealMeleeCR = DealDamage(
						this.CharacterCard,
						(Card c) => c == theCard,
						meleeNumeral,
						DamageType.Melee
					);
					IEnumerator dealColdCR = DealDamage(
						this.CharacterCard,
						(Card c) => c == theCard,
						coldNumeral,
						DamageType.Cold
					);

					if (UseUnityCoroutines)
					{
						yield return GameController.StartCoroutine((damageType == DamageType.Melee) ? dealMeleeCR : dealColdCR);
						yield return GameController.StartCoroutine((damageType == DamageType.Melee) ? dealColdCR : dealMeleeCR);
					}
					else
					{
						GameController.ExhaustCoroutine((damageType == DamageType.Melee) ? dealMeleeCR : dealColdCR);
						GameController.ExhaustCoroutine((damageType == DamageType.Melee) ? dealColdCR : dealMeleeCR);
					}

					if (theCard.IsInPlayAndHasGameText && theCard.IsTarget && !this.CharacterCard.IsIncapacitatedOrOutOfGame)
					{
						// That target deals [i]Verdigris[/i] 3 melee damage.
						IEnumerator counterDamageCR = DealDamage(
							theCard,
							this.CharacterCard,
							counterNumeral,
							DamageType.Melee,
							cardSource: GetCardSource()
						);

						if (UseUnityCoroutines)
						{
							yield return GameController.StartCoroutine(counterDamageCR);
						}
						else
						{
							GameController.ExhaustCoroutine(counterDamageCR);
						}
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
					// One player may play a card.
					IEnumerator playCardCR = SelectHeroToPlayCard(
						this.HeroTurnTakerController,
						heroCriteria: new LinqTurnTakerCriteria(
							(TurnTaker tt) => tt.IsHero && !tt.IsIncapacitated
						)
					);

					if (UseUnityCoroutines)
					{
						yield return GameController.StartCoroutine(playCardCR);
					}
					else
					{
						GameController.ExhaustCoroutine(playCardCR);
					}
					break;
				case 1:
					// One hero target deals 1 target 1 melee damage. A different hero target deals 1 target 1 cold damage.
					List<SelectCardDecision> firstAttacker = new List<SelectCardDecision>();
					IEnumerator selectMeleeCR = GameController.SelectCardAndStoreResults(
						DecisionMaker,
						SelectionType.CardToDealDamage,
						new LinqCardCriteria(
							(Card c) => c.IsHero && c.IsTarget && c.IsInPlayAndHasGameText,
							"hero target",
							false
						),
						firstAttacker,
						false,
						cardSource: GetCardSource()
					);

					if (UseUnityCoroutines)
					{
						yield return GameController.StartCoroutine(selectMeleeCR);
					}
					else
					{
						GameController.ExhaustCoroutine(selectMeleeCR);
					}

					Card firstAttackerCard = GetSelectedCard(firstAttacker);
					if (firstAttackerCard != null)
					{
						IEnumerator meleeDamageCR = GameController.SelectTargetsAndDealDamage(
							DecisionMaker,
							new DamageSource(GameController, firstAttackerCard),
							1,
							DamageType.Melee,
							1,
							false,
							1,
							cardSource: GetCardSource()
						);

						if (UseUnityCoroutines)
						{
							yield return GameController.StartCoroutine(meleeDamageCR);
						}
						else
						{
							GameController.ExhaustCoroutine(meleeDamageCR);
						}

						List<SelectCardDecision> secondAttacker = new List<SelectCardDecision>();
						IEnumerator selectColdCR = GameController.SelectCardAndStoreResults(
							DecisionMaker,
							SelectionType.CardToDealDamage,
							new LinqCardCriteria(
								(Card c) => c.IsHero && c.IsTarget && c.IsInPlayAndHasGameText && c != firstAttackerCard,
								"hero target",
								false
							),
							secondAttacker,
							false,
							cardSource: GetCardSource()
						);

						if (UseUnityCoroutines)
						{
							yield return GameController.StartCoroutine(selectColdCR);
						}
						else
						{
							GameController.ExhaustCoroutine(selectColdCR);
						}

						Card secondAttackerCard = GetSelectedCard(secondAttacker);
						if (secondAttackerCard != null)
						{
							IEnumerator coldDamageCR = GameController.SelectTargetsAndDealDamage(
								DecisionMaker,
								new DamageSource(GameController, secondAttackerCard),
								1,
								DamageType.Cold,
								1,
								false,
								1,
								cardSource: GetCardSource()
							);

							if (UseUnityCoroutines)
							{
								yield return GameController.StartCoroutine(coldDamageCR);
							}
							else
							{
								GameController.ExhaustCoroutine(coldDamageCR);
							}
						}
					}

					break;
				case 2:
					// Each hero character may deal themselves 3 cold damage to use a power now.
					IEnumerator damagePowerCR = GameController.DealDamageToSelf(
						DecisionMaker,
						(Card c) => c.IsHeroCharacterCard,
						3,
						DamageType.Cold,
						addStatusEffect: UsePowerResponse,
						requiredDecisions: 0,
						cardSource: GetCardSource()
					);

					if (UseUnityCoroutines)
					{
						yield return GameController.StartCoroutine(damagePowerCR);
					}
					else
					{
						GameController.ExhaustCoroutine(damagePowerCR);
					}
					break;
			}

			yield break;
		}

		private IEnumerator UsePowerResponse(DealDamageAction dda)
		{
			if (
				dda.DidDealDamage
				&& !dda.DidDestroyTarget
				&& (!dda.Target.WillBeDestroyed || GameController.IsCardIndestructible(dda.Target))
				&& !dda.Target.IsIncapacitatedOrOutOfGame
			)
			{
				IEnumerator powerCR = SelectAndUsePower(FindCardController(dda.Target));
				if (UseUnityCoroutines)
				{
					yield return GameController.StartCoroutine(powerCR);
				}
				else
				{
					GameController.ExhaustCoroutine(powerCR);
				}
			}

			yield break;
		}

		protected LinqCardCriteria IsWaterCriteria(Func<Card, bool> additionalCriteria = null)
		{
			var result = new LinqCardCriteria(c => IsWater(c), "water", true);
			if (additionalCriteria != null)
			{
				result = new LinqCardCriteria(result, additionalCriteria);
			}

			return result;
		}

		protected bool IsWater(Card card, bool evenIfUnderCard = false, bool evenIfFaceDown = false)
		{
			return card != null && base.GameController.DoesCardContainKeyword(card, "water", evenIfUnderCard, evenIfFaceDown);
		}
	}
}