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
		}

		public override void AddTriggers()
		{
			// At the end of the villain turn, this card deals the hero target with the highest HP {H - 1} melee damage.
			AddDealDamageAtEndOfTurnTrigger(
				TurnTaker,
				base.Card,
				(Card c) => c.IsHero,
				TargetType.HighestHP,
				base.H - 1,
				DamageType.Toxic
			);

			// Dog: This card is immune to melee damage.
			AddImmuneToDamageTrigger(
				(DealDamageAction dd) =>
					IsEnabled("dog")
					&& dd.Target == base.Card
					&& dd.DamageType == DamageType.Melee
			);

			// Mask: At the end of the villain turn, move {ImpCharacter} to the hero play area with the highest HP. She deals that hero 1 projectile damage.
			AddEndOfTurnTrigger(
				(TurnTaker tt) => tt == base.TurnTaker && IsEnabled("mask"),
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
				(Card c) => c.IsHeroCharacterCard,
				heroList,
				cardSource: GetCardSource()
			);

			if (base.UseUnityCoroutines)
			{
				yield return base.GameController.StartCoroutine(findHeroCR);
			}
			else
			{
				base.GameController.ExhaustCoroutine(findHeroCR);
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
					base.TurnTakerController,
					maybeImp,
					heroTarget.Owner.PlayArea,
					playCardIfMovingToPlayArea: false,
					cardSource: GetCardSource()
				);

				if (base.UseUnityCoroutines)
				{
					yield return base.GameController.StartCoroutine(moveImpCR);
				}
				else
				{
					base.GameController.ExhaustCoroutine(moveImpCR);
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

				if (base.UseUnityCoroutines)
				{
					yield return base.GameController.StartCoroutine(findVillainCR);
				}
				else
				{
					base.GameController.ExhaustCoroutine(findVillainCR);
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

			if (base.UseUnityCoroutines)
			{
				yield return base.GameController.StartCoroutine(dealDamageCR);
			}
			else
			{
				base.GameController.ExhaustCoroutine(dealDamageCR);
			}

			yield break;
		}

		public override IEnumerator Play()
		{
			yield break;
		}
	}
}
