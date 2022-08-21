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
	public class RegentCharacterCardController : TheUndersidersVillainCardController
	{
		private const string HasBeenDealtDamage = "HasBeenDealtDamage";

		public RegentCharacterCardController(Card card, TurnTakerController turnTakerController)
			: base(card, turnTakerController)
		{
		}

		public override IEnumerator Play()
		{
			yield break;
		}

		public override void AddSideTriggers()
		{
			if (!base.Card.IsFlipped)
			{
				// The first time {RegentCharacter} is dealt damage by a target each turn, he deals that target 2 lightning damage.
				AddSideTrigger(AddCounterDamageTrigger(
					(DealDamageAction dda) => dda.Target == this.Card && dda.DidDealDamage,
					() => this.Card,
					() => this.Card,
					oncePerTargetPerTurn: true,
					2,
					DamageType.Lightning
				));
				/* old version
				AddSideTrigger(AddTrigger(
					(DealDamageAction dd) =>
						dd.Target == base.Card
						&& dd.DidDealDamage
						&& dd.DamageSource.IsTarget
						&& !HasBeenSetToTrueThisTurn(HasBeenDealtDamage),
					TaserSceptreResponse,
					TriggerType.DealDamage,
					TriggerTiming.After,
					ActionDescription.DamageTaken
				));
				*/

				// At the end of the villain turn, the hero with the least cards in their play area deals themself 2 melee damage.
				AddSideTrigger(AddEndOfTurnTrigger(
					(TurnTaker tt) => tt == base.TurnTaker,
					StopHittingYourselfResponse,
					TriggerType.DealDamage
				));

				// Treat {Crown} effects as active. (taken care of by the cards)
			}
			else
			{
				// At the end of each hero's turn, they may discard a card. If that hero does not do so they deal themself 1 melee damage.
				AddSideTrigger(AddEndOfTurnTrigger(
					(TurnTaker tt) => tt.IsHero && !tt.IsIncapacitatedOrOutOfGame,
					DiscardOrPainResponse,
					new TriggerType[] {
						TriggerType.DiscardCard,
						TriggerType.DealDamage
					}
				));

			}
			base.AddSideTriggers();
		}

		private IEnumerator TaserSceptreResponse(DealDamageAction dd)
		{
			SetCardPropertyToTrueIfRealAction(HasBeenDealtDamage);
			IEnumerator retaliationCR = DealDamage(
				base.Card,
				(Card c) => c.IsTarget && c == dd.DamageSource.Card,
				2,
				DamageType.Lightning
			);

			if (base.UseUnityCoroutines)
			{
				yield return base.GameController.StartCoroutine(retaliationCR);
			}
			else
			{
				base.GameController.ExhaustCoroutine(retaliationCR);
			}

			yield break;
		}

		private IEnumerator StopHittingYourselfResponse(PhaseChangeAction p)
		{
			List<TurnTaker> storedResults = new List<TurnTaker>();
			IEnumerator findHeroCR = FindHeroWithFewestCardsInPlayArea(storedResults);

			if (base.UseUnityCoroutines)
			{
				yield return base.GameController.StartCoroutine(findHeroCR);
			}
			else
			{
				base.GameController.ExhaustCoroutine(findHeroCR);
			}

			TurnTaker poorSap = storedResults.FirstOrDefault();
			if (poorSap != null)
			{
				IEnumerator stopHittingCR = DealDamage(
					poorSap.CharacterCard,
					poorSap.CharacterCard,
					2,
					DamageType.Melee
				);

				if (base.UseUnityCoroutines)
				{
					yield return base.GameController.StartCoroutine(stopHittingCR);
				}
				else
				{
					base.GameController.ExhaustCoroutine(stopHittingCR);
				}
			}

			yield break;
		}

		private IEnumerator DiscardOrPainResponse(PhaseChangeAction p)
		{
			List<DiscardCardAction> storedResults = new List<DiscardCardAction>();
			IEnumerator discardCR = SelectAndDiscardCards(
				GameController.FindHeroTurnTakerController(p.ToPhase.TurnTaker.ToHero()),
				1,
				optional: true,
				storedResults: storedResults
			);

			if (base.UseUnityCoroutines)
			{
				yield return base.GameController.StartCoroutine(discardCR);
			}
			else
			{
				base.GameController.ExhaustCoroutine(discardCR);
			}

			if (!DidDiscardCards(storedResults))
			{
				IEnumerator stopHittingCR = DealDamage(
					p.ToPhase.TurnTaker.CharacterCard,
					p.ToPhase.TurnTaker.CharacterCard,
					1,
					DamageType.Melee
				);

				if (base.UseUnityCoroutines)
				{
					yield return base.GameController.StartCoroutine(stopHittingCR);
				}
				else
				{
					base.GameController.ExhaustCoroutine(stopHittingCR);
				}
			}

			yield break;
		}
	}
}
