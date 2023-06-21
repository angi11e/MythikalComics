using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.Supplicate
{
	public class GorillaSpiritCardController : YaojingBaseCardController
	{
		/*
		 * at the start of your turn,
		 * this card deals itself or {Supplicate} 2 irreducible psychic damage.
		 * 
		 * at the end of your turn,
		 * this card deals 1 target X melee damage,
		 * where X = the number of yaojing cards in your play area
		 * 
		 * whenever a yaojing target would be dealt damage,
		 * you may redirect it to this card.
		 */

		public GorillaSpiritCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
			SpecialStringMaker.ShowNumberOfCardsInPlay(new LinqCardCriteria((Card c) =>
				c.Location.HighestRecursiveLocation == HeroTurnTaker.PlayArea && IsOngoing(c), "ongoing"
			));
		}

		public override void AddTriggers()
		{
			base.AddTriggers();

			// at the end of your turn
			AddEndOfTurnTrigger(
				(TurnTaker tt) => tt == this.TurnTaker,
				EndOfTurnResponse,
				TriggerType.DealDamage
			);

			// whenever a yaojing target would be dealt damage
			// you may redirect it to this card.
			AddRedirectDamageTrigger(
				(DealDamageAction dd) => IsYaojing(dd.Target) && dd.Target != this.Card,
				() => this.Card,
				optional: true
			);
		}

		private IEnumerator EndOfTurnResponse(PhaseChangeAction pca)
		{
			// where X = the number of yaojing cards in your play area
			int damageNumeral = FindCardsWhere((Card c) =>
				c.IsInPlayAndHasGameText
				&& c.Location.HighestRecursiveLocation == HeroTurnTaker.PlayArea
				&& IsYaojing(c)
			).Count();

			// this card deals 1 target X melee damage
			IEnumerator damageCR = GameController.SelectTargetsAndDealDamage(
				DecisionMaker,
				new DamageSource(GameController, this.Card),
				damageNumeral,
				DamageType.Melee,
				1,
				false,
				1,
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

			yield break;
		}
	}
}