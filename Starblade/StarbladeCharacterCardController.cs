using System;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Angille.Starblade
{
	public class StarbladeCharacterCardController : HeroCharacterCardController
	{
		public StarbladeCharacterCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController) {
		}

		public override IEnumerator UsePower(int index = 0)
		{
			int targetOneNumeral = GetPowerNumeral(0, 1);
			int damageOneNumeral = GetPowerNumeral(1, 2);
			int targetTwoNumeral = GetPowerNumeral(2, 1);
			int damageTwoNumeral = GetPowerNumeral(3, 1);

			// {Starblade} deals 1 target 2 melee damage.
			List<DealDamageAction> theTarget = new List<DealDamageAction>();
			IEnumerator damageCR = GameController.SelectTargetsAndDealDamage(
				DecisionMaker,
				new DamageSource(GameController, this.CharacterCard),
				damageOneNumeral,
				DamageType.Melee,
				targetOneNumeral,
				false,
				targetOneNumeral,
				storedResultsDamage: theTarget,
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

			Card notTheTarget = null;
			if (theTarget.Any())
			{
				notTheTarget = theTarget.FirstOrDefault().Target;
				if (!notTheTarget.IsInPlayAndHasGameText || notTheTarget.IsIncapacitatedOrOutOfGame)
				{
					notTheTarget = null;
				}
			}

			// {Starblade} deals 1 different target 1 energy damage.
			IEnumerator energyDamageCR = GameController.SelectTargetsAndDealDamage(
				DecisionMaker,
				new DamageSource(GameController, this.CharacterCard),
				damageTwoNumeral,
				DamageType.Melee,
				targetTwoNumeral,
				false,
				targetTwoNumeral,
				additionalCriteria: (Card c) => notTheTarget == null || c != notTheTarget,
				cardSource: GetCardSource()
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(energyDamageCR);
			}
			else
			{
				GameController.ExhaustCoroutine(energyDamageCR);
			}

			yield break;
		}

		public override IEnumerator UseIncapacitatedAbility(int index)
		{
			switch (index)
			{
				case 0:
					// One Player may Play a card now.
					IEnumerator playCardCR = SelectHeroToPlayCard(
						this.HeroTurnTakerController,
						heroCriteria: new LinqTurnTakerCriteria(
							(TurnTaker tt) => IsHero(tt) && !tt.IsIncapacitated
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
					// One hero may deal 1 target 1 energy damage.
					IEnumerator dealDamageCR = GameController.SelectHeroToSelectTargetAndDealDamage(
						DecisionMaker,
						1,
						DamageType.Energy,
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
					break;

				case 2:
					// Each player may...
					IEnumerator discardPlayCR = GameController.SelectTurnTakersAndDoAction(
						DecisionMaker,
						new LinqTurnTakerCriteria((TurnTaker tt) =>
							IsHero(tt)
							&& !tt.IsIncapacitatedOrOutOfGame
							&& tt.ToHero().Hand.NumberOfCards >= 2
						),
						SelectionType.DiscardCard,
						ReturnAndPlayResponse,
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

		private IEnumerator ReturnAndPlayResponse(TurnTaker tt)
		{
			HeroTurnTakerController httc = FindHeroTurnTakerController(tt.ToHero());
			List<SelectCardDecision> selected = new List<SelectCardDecision>();

			// ...move one of their non-character cards in play to their hand.
			IEnumerator moveCR = GameController.SelectAndMoveCard(
				httc,
				(Card c) => c.Owner == httc.HeroTurnTaker && c.IsInPlayAndNotUnderCard && !c.IsCharacter,
				httc.HeroTurnTaker.Hand,
				optional: true,
				storedResults: selected,
				cardSource: GetCardSource()
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(moveCR);
			}
			else
			{
				GameController.ExhaustCoroutine(moveCR);
			}

			// Any who do...
			if (selected.Any() && GetSelectedCard(selected).Location == httc.HeroTurnTaker.Hand)
			{
				// ...may play a different card.
				IEnumerator playCR = SelectAndPlayCardFromHand(
					httc,
					cardCriteria: new LinqCardCriteria((Card c) => c != GetSelectedCard(selected))
				);

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