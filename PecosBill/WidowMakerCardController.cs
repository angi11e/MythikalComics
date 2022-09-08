using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.PecosBill
{
	public class WidowMakerCardController : HyperboleBaseCardController
	{
		/*
		 * Play this card next to [i]Loyal Lightning[/i].
		 * If [i]Loyal Lightning[/i] is ever not in play, destroy this card.
		 * 
		 * When damage would be dealt by {PecosBill} but is prevented or reduced so {PecosBill} deals no damage,
		 * [i]Loyal Lightning[/i] deals that target 1 irreducible lightning damage.
		 * 
		 * TALL TALE
		 * [i]Loyal Lightning[/i] deals 1 target 1 irreducible lightning damage.
		 */

		public WidowMakerCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController, "LoyalLightning")
		{
		}

		public override void AddTriggers()
		{
			base.AddTriggers();

			// When damage would be dealt by {PecosBill} but is prevented or reduced so {PecosBill} deals no damage...
			// ...[i]Loyal Lightning[/i] deals that target 1 irreducible lightning damage.
			AddTrigger(
				(DealDamageAction dd) => CheckDamageCriteria(dd),
				(DealDamageAction dd) => DealDamage(
					GetCardThisCardIsNextTo(),
					dd.Target,
					1,
					DamageType.Lightning,
					true,
					cardSource: GetCardSource()
				),
				TriggerType.DealDamage,
				TriggerTiming.After
			);
			AddTrigger(
				(CancelAction c) => c.ActionToCancel is DealDamageAction
					&& c.IsPreventEffect
					&& CheckDamageCriteria(c.ActionToCancel as DealDamageAction),
				(CancelAction c) => DealDamage(
					GetCardThisCardIsNextTo(),
					(c.ActionToCancel as DealDamageAction).Target,
					1,
					DamageType.Lightning,
					true,
					cardSource: GetCardSource()
				),
				TriggerType.DealDamage,
				TriggerTiming.After
			);
		}

		private bool CheckDamageCriteria(DealDamageAction dd)
		{
			if (!dd.IsPretend && dd.DamageSource.IsSameCard(this.CharacterCard) && !dd.DidDealDamage)
			{
				if (dd.OriginalAmount <= 0)
				{
					return dd.DamageModifiers.Where((ModifyDealDamageAction ga) => ga is IncreaseDamageAction).Any();
				}
				return true;
			}
			return false;
		}

		public override IEnumerator ActivateTallTale()
		{
			// [i]Loyal Lightning[/i] deals 1 target 1 irreducible lightning damage.
			IEnumerator damageCR = GameController.SelectTargetsAndDealDamage(
				DecisionMaker,
				new DamageSource(GameController, GetCardThisCardIsNextTo()),
				1,
				DamageType.Lightning,
				1,
				false,
				1,
				true,
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