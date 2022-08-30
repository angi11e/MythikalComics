using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.NightMare
{
	public class HoofStompCardController : NightMareBaseCardController
	{
		/*
		 * {NightMare} deals each non-Hero Target 1 Sonic Damage.
		 * Targets dealt damage this way deal themselves 1 Melee Damage.
		 * 
		 * DISCARD
		 * Until the start of your next turn, increase the damage {NightMare} deals to targets at full HP by 2.
		 */

		public HoofStompCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
		}

		public override IEnumerator Play()
		{
			// {NightMare} deals each non-Hero Target 1 Sonic Damage.
			List<DealDamageAction> storedResults = new List<DealDamageAction>();
			IEnumerator damageCR = DealDamage(
				base.CharacterCard,
				(Card c) => !c.IsHero,
				1,
				DamageType.Sonic,
				storedResults: storedResults
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(damageCR);
			}
			else
			{
				GameController.ExhaustCoroutine(damageCR);
			}

			// Targets dealt damage this way deal themselves 1 Melee Damage.
			List<Card> retargets = (from dd in storedResults where dd.DidDealDamage select dd.Target).Distinct().ToList();
			IEnumerator selfDamageCR = GameController.DealDamageToSelf(
				DecisionMaker,
				(Card c) => retargets.Contains(c),
				1,
				DamageType.Melee,
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

			yield break;
		}

		protected override IEnumerator DiscardResponse(GameAction ga)
		{
			// Until the start of your next turn, increase the damage {NightMare} deals to targets at full HP by 2.
			IncreaseDamageStatusEffect increaseDamageSE = new IncreaseDamageStatusEffect(2);
			increaseDamageSE.TargetCriteria.HasMaxHitPoints = true;
			increaseDamageSE.SourceCriteria.IsSpecificCard = base.CharacterCard;
			increaseDamageSE.UntilStartOfNextTurn(TurnTaker);

			IEnumerator increaseDamageCR = AddStatusEffect(increaseDamageSE);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(increaseDamageCR);
			}
			else
			{
				GameController.ExhaustCoroutine(increaseDamageCR);
			}

			yield break;
		}
	}
}