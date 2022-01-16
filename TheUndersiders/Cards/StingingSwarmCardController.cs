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
	public class StingingSwarmCardController : TheUndersidersBaseCardController
	{
		public StingingSwarmCardController(Card card, TurnTakerController turnTakerController)
			: base(card, turnTakerController)
		{
		}

		public override void AddTriggers()
		{
			// At the end of the villain turn, this card deals the hero target with the highest number of cards in their hand 1 toxic damage.
			AddEndOfTurnTrigger(
				(TurnTaker tt) => tt == base.TurnTaker,
				StingingResponse,
				TriggerType.DealDamage
			);

			// Spider: Whenever this card deals damage to a target, {SkitterCharacter} deals that target 1 psychic damage.
			AddTrigger(
				(DealDamageAction dd) =>
					dd.DamageSource.IsSameCard(base.Card)
					&& dd.DidDealDamage
					&& IsEnabled("spider"),
				(DealDamageAction dd) => SkitterStrikeResponse(dd),
				TriggerType.DealDamage,
				TriggerTiming.After
			);

			// Skull: Whenever this card deals damage to a target, {GrueCharacter} deals that target 1 infernal damage.
			AddTrigger(
				(DealDamageAction dd) =>
					dd.DamageSource.IsSameCard(base.Card)
					&& dd.DidDealDamage
					&& IsEnabled("skull"),
				(DealDamageAction dd) => GrueStrikeResponse(dd),
				TriggerType.DealDamage,
				TriggerTiming.After
			);

			base.AddTriggers();
		}

		private IEnumerator StingingResponse(PhaseChangeAction p)
		{
			List<TurnTaker> storedResults = new List<TurnTaker>();
			IEnumerator findHeroCR = FindHeroWithMostCardsInHand(storedResults);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(findHeroCR);
			}
			else
			{
				GameController.ExhaustCoroutine(findHeroCR);
			}

			if (storedResults.Count > 0)
			{
				IEnumerator stingingCR = DealDamage(
					base.Card,
					storedResults.FirstOrDefault().CharacterCard,
					1,
					DamageType.Toxic,
					cardSource: GetCardSource()
				);

				if (base.UseUnityCoroutines)
				{
					yield return base.GameController.StartCoroutine(stingingCR);
				}
				else
				{
					base.GameController.ExhaustCoroutine(stingingCR);
				}
			}
			yield break;
		}

		private IEnumerator SkitterStrikeResponse(DealDamageAction dd)
		{
			Card maybeSkitter = SkitterCharacter;
			Card heroTarget = dd.Target;
			if (maybeSkitter.IsFlipped)
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

				maybeSkitter = villainList.FirstOrDefault();
			}

			if (maybeSkitter.IsTarget && heroTarget.IsTarget)
			{
				IEnumerator skitterStrikeCR = DealDamage(
					maybeSkitter,
					heroTarget,
					1,
					DamageType.Psychic,
					cardSource: GetCardSource()
				);

				if (base.UseUnityCoroutines)
				{
					yield return base.GameController.StartCoroutine(skitterStrikeCR);
				}
				else
				{
					base.GameController.ExhaustCoroutine(skitterStrikeCR);
				}
			}

			yield break;
		}

		private IEnumerator GrueStrikeResponse(DealDamageAction dd)
		{
			Card maybeGrue = GrueCharacter;
			Card heroTarget = dd.Target;
			if (maybeGrue.IsFlipped)
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

				maybeGrue = villainList.FirstOrDefault();
			}

			if (maybeGrue.IsTarget && heroTarget.IsTarget)
			{
				IEnumerator grueStrikeCR = DealDamage(
					maybeGrue,
					heroTarget,
					1,
					DamageType.Infernal,
					cardSource: GetCardSource()
				);

				if (base.UseUnityCoroutines)
				{
					yield return base.GameController.StartCoroutine(grueStrikeCR);
				}
				else
				{
					base.GameController.ExhaustCoroutine(grueStrikeCR);
				}
			}

			yield break;
		}

		public override IEnumerator Play()
		{
			yield break;
		}
	}
}
