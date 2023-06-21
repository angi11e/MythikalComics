using System;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Angille.PecosBill
{
	public class PecosBillCharacterCardController : HeroCharacterCardController
	{
		public PecosBillCharacterCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController) {
			SpecialStringMaker.ShowListOfCardsInPlay(IsHyperboleCriteria());
		}

		public override IEnumerator UsePower(int index = 0)
		{
			int pecosTargetNumeral = GetPowerNumeral(0, 1);
			int pecosDamageNumeral = GetPowerNumeral(1, 1);
			int folkTargetNumeral = GetPowerNumeral(2, 1);
			int folkDamageNumeral = GetPowerNumeral(3, 1);

			// you may activate a [u]tall tale[/u] text.
			IEnumerator activateCR = GameController.SelectAndActivateAbility(
				DecisionMaker,
				"tall tale",
				optional: true,
				cardSource: GetCardSource()
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(activateCR);
			}
			else
			{
				GameController.ExhaustCoroutine(activateCR);
			}

			// {PecosBill} deals 1 target 1 projectile damage.
			IEnumerator pecosDamageCR = GameController.SelectTargetsAndDealDamage(
				DecisionMaker,
				new DamageSource(GameController, this.CharacterCard),
				pecosDamageNumeral,
				DamageType.Projectile,
				pecosTargetNumeral,
				false,
				pecosTargetNumeral,
				cardSource: GetCardSource()
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(pecosDamageCR);
			}
			else
			{
				GameController.ExhaustCoroutine(pecosDamageCR);
			}

			// a [u]folk[/u] target deals 1 target 1 melee damage.
			List<SelectCardDecision> storedResult = new List<SelectCardDecision>();
			IEnumerator pickTargetCR = GameController.SelectCardAndStoreResults(
				DecisionMaker,
				SelectionType.SelectTargetFriendly,
				new LinqCardCriteria(
					(Card c) => c.IsTarget && c.IsInPlayAndNotUnderCard && IsFolk(c),
					"folk",
					useCardsSuffix: false
				),
				storedResult,
				optional: false,
				cardSource: GetCardSource()
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(pickTargetCR);
			}
			else
			{
				GameController.ExhaustCoroutine(pickTargetCR);
			}

			SelectCardDecision selection = storedResult.FirstOrDefault();
			if (selection != null && selection.SelectedCard != null)
			{
				IEnumerator folkDamageCR = GameController.SelectTargetsAndDealDamage(
					DecisionMaker,
					new DamageSource(GameController, selection.SelectedCard),
					folkDamageNumeral,
					DamageType.Melee,
					folkTargetNumeral,
					false,
					folkTargetNumeral,
					cardSource: GetCardSource()
				);

				if (UseUnityCoroutines)
				{
					yield return GameController.StartCoroutine(folkDamageCR);
				}
				else
				{
					GameController.ExhaustCoroutine(folkDamageCR);
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
					// One hero may use a power.
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

				case 2:
					// Select a Hero.
					List<SelectTurnTakerDecision> storedResults = new List<SelectTurnTakerDecision>();
					IEnumerator selectCR = GameController.SelectHeroTurnTaker(
						HeroTurnTakerController,
						SelectionType.DealDamageAfterUsePower,
						false,
						false,
						storedResults,
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

					if (storedResults.Any(
						(SelectTurnTakerDecision d) => d.Completed
						&& d.SelectedTurnTaker != null
						&& IsHero(d.SelectedTurnTaker)
					) && IsHero(storedResults.FirstOrDefault().SelectedTurnTaker))
					{
						// The next time that Hero uses a Power...
						// ...that Hero also deals 1 Target 1 Melee Damage.
						HeroTurnTaker htt = storedResults.FirstOrDefault().SelectedTurnTaker.ToHero();
						Card damageSource = ((!htt.HasMultipleCharacterCards) ? htt.CharacterCard : null);

						DealDamageAfterUsePowerStatusEffect ddaupSE = new DealDamageAfterUsePowerStatusEffect(
							htt,
							damageSource,
							null,
							1,
							DamageType.Melee,
							1,
							isIrreducible: false
						);

						ddaupSE.TurnTakerCriteria.IsSpecificTurnTaker = htt;
						ddaupSE.NumberOfUses = 1;
						if (!htt.HasMultipleCharacterCards)
						{
							ddaupSE.CardDestroyedExpiryCriteria.Card = htt.CharacterCard;
						}

						if (UseUnityCoroutines)
						{
							yield return GameController.StartCoroutine(AddStatusEffect(ddaupSE));
						}
						else
						{
							GameController.ExhaustCoroutine(AddStatusEffect(ddaupSE));
						}
					}
					break;
			}
			yield break;
		}

		protected LinqCardCriteria IsFolkCriteria(Func<Card, bool> additionalCriteria = null)
		{
			var result = new LinqCardCriteria(c => IsFolk(c), "folk", true);
			if (additionalCriteria != null)
			{
				result = new LinqCardCriteria(result, additionalCriteria);
			}

			return result;
		}

		protected bool IsFolk(Card card, bool evenIfUnderCard = false, bool evenIfFaceDown = false)
		{
			return card != null && GameController.DoesCardContainKeyword(card, "folk", evenIfUnderCard, evenIfFaceDown);
		}

		protected LinqCardCriteria IsHyperboleCriteria(Func<Card, bool> additionalCriteria = null)
		{
			var result = new LinqCardCriteria(c => IsHyperbole(c), "hyperbole", true);
			if (additionalCriteria != null)
			{
				result = new LinqCardCriteria(result, additionalCriteria);
			}

			return result;
		}

		protected bool IsHyperbole(Card card, bool evenIfUnderCard = false, bool evenIfFaceDown = false)
		{
			return card != null && GameController.DoesCardContainKeyword(card, "hyperbole", evenIfUnderCard, evenIfFaceDown);
		}
	}
}