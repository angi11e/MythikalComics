using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Handelabra.Sentinels.Engine.Model;
using Handelabra.Sentinels.Engine.Controller;
using System.Collections;
using Handelabra;

namespace Angille.CadaverTeam
{
	public class DitchCardController : CardController
	{
		public DitchCardController(Card card, TurnTakerController turnTakerController)
			: base(card, turnTakerController)
		{
			SpecialStringMaker.ShowLowestHP(1, () => 2, new LinqCardCriteria(
				(Card c) => IsHero(c)
			));
		}

		public override void AddTriggers()
		{
			// This card is immune to cold damage.
			AddImmuneToDamageTrigger(
				(DealDamageAction dda) => dda.Target == this.Card && dda.DamageType == DamageType.Cold
			);

			// At the end of {CadaverTeam}'s turn...
			AddEndOfTurnTrigger(
				(TurnTaker tt) => tt == this.TurnTaker,
				EndOfTurnResponse,
				new TriggerType[] { TriggerType.DealDamage, TriggerType.DestroyCard }
			);

			base.AddTriggers();
		}

		private IEnumerator EndOfTurnResponse(PhaseChangeAction pca)
		{
			// ...this card deals the 2 hero targets with the lowest HP {H - 2} melee damage.
			IEnumerator dealDamageCR = DealDamageToLowestHP(
				this.Card,
				1,
				(Card c) => IsHero(c),
				(Card c) => Game.H - 2,
				DamageType.Melee,
				numberOfTargets: 2
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(dealDamageCR);
			}
			else
			{
				GameController.ExhaustCoroutine(dealDamageCR);
			}

			// Then, if {Angille.Patina} is active in this game...
			if (IsHeroActiveInThisGame("PatinaCharacter"))
			{
				// ...destroy 1 hero ongoing card and 1 hero equipment card.
				IEnumerator destroyOngoingCR = GameController.SelectAndDestroyCard(
					DecisionMaker,
					new LinqCardCriteria((Card c) => IsOngoing(c) && IsHero(c)),
					false,
					cardSource: GetCardSource()
				);
				IEnumerator destroyEquipCR = GameController.SelectAndDestroyCard(
					DecisionMaker,
					new LinqCardCriteria((Card c) => IsEquipment(c) && IsHero(c)),
					false,
					cardSource: GetCardSource()
				);

				if (UseUnityCoroutines)
				{
					yield return GameController.StartCoroutine(destroyOngoingCR);
					yield return GameController.StartCoroutine(destroyEquipCR);
				}
				else
				{
					GameController.ExhaustCoroutine(destroyOngoingCR);
					GameController.ExhaustCoroutine(destroyEquipCR);
				}
			}

			yield break;
		}
	}
}
