using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Handelabra.Sentinels.Engine.Model;
using Handelabra.Sentinels.Engine.Controller;
using System.Collections;
using Handelabra;

namespace Angille.TheUndersiders
{
	public class BentleyCardController : TheUndersidersBaseCardController
	{
		public BentleyCardController(Card card, TurnTakerController turnTakerController)
			: base(card, turnTakerController)
		{
			SpecialStringMaker.ShowHeroTargetWithHighestHP();
			SpecialStringMaker.ShowHeroCharacterCardWithHighestHP().Condition = () => IsEnabled("mask");
			SpecialStringMaker.ShowLocationOfCards(
				new LinqCardCriteria((Card c) => c.Identifier == "ImpCharacter")
			).Condition = () => ImpCharacter.IsInPlayAndNotUnderCard && !ImpCharacter.IsFlipped;

			SpecialStringMaker.ShowSpecialString(() => GetSpecialStringIcons("dog", "mask"));
		}

		public override void AddTriggers()
		{
			// At the end of the villain turn, this card deals the hero target with the highest HP {H - 1} melee damage.
			AddDealDamageAtEndOfTurnTrigger(
				TurnTaker,
				this.Card,
				(Card c) => IsHeroTarget(c),
				TargetType.HighestHP,
				H - 1,
				DamageType.Toxic
			);

			// Dog: This card is immune to melee damage.
			AddImmuneToDamageTrigger(
				(DealDamageAction dd) =>
					IsEnabled("dog")
					&& dd.Target == this.Card
					&& dd.DamageType == DamageType.Melee
			);

			// Mask: At the end of the villain turn, move {ImpCharacter} to the hero play area with the highest HP. She deals that hero 1 projectile damage.
			AddEndOfTurnTrigger(
				(TurnTaker tt) => tt == this.TurnTaker && IsEnabled("mask"),
				ShuttleImpResponse,
				new TriggerType[] {
					TriggerType.MoveCard,
					TriggerType.DealDamage
				}
			);

			base.AddTriggers();
		}

		private IEnumerator ShuttleImpResponse(PhaseChangeAction p)
		{
			List<Card> heroList = new List<Card>();
			IEnumerator findHeroCR = GameController.FindTargetWithHighestHitPoints(
				1,
				(Card c) => IsHeroCharacterCard(c),
				heroList,
				cardSource: GetCardSource()
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(findHeroCR);
			}
			else
			{
				GameController.ExhaustCoroutine(findHeroCR);
			}

			Card heroTarget = heroList.FirstOrDefault();
			if (heroTarget == null)
			{
				yield break;
			}

			Card maybeImp = ImpCharacter;
			if (!maybeImp.IsFlipped)
			{
				IEnumerator moveImpCR = GameController.MoveCard(
					this.TurnTakerController,
					maybeImp,
					heroTarget.Owner.PlayArea,
					playCardIfMovingToPlayArea: false,
					showMessage: true,
					cardSource: GetCardSource()
				);

				if (UseUnityCoroutines)
				{
					yield return GameController.StartCoroutine(moveImpCR);
				}
				else
				{
					GameController.ExhaustCoroutine(moveImpCR);
				}
			}
			else
			{
				List<Card> villainList = new List<Card>();
				IEnumerator findVillainCR = GameController.FindTargetWithHighestHitPoints(
					1,
					(Card c) => c.IsVillainCharacterCard,
					villainList,
					cardSource: GetCardSource()
				);

				if (UseUnityCoroutines)
				{
					yield return GameController.StartCoroutine(findVillainCR);
				}
				else
				{
					GameController.ExhaustCoroutine(findVillainCR);
				}

				maybeImp = villainList.FirstOrDefault();
				if (maybeImp == null)
				{
					yield break;
				}
			}

			IEnumerator dealDamageCR = GameController.DealDamageToTarget(
				new DamageSource(GameController, maybeImp),
				heroTarget,
				1,
				DamageType.Projectile,
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

			yield break;
		}
	}
}
