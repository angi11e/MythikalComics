using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.Athena
{
	public class ErichthoniusCardController : AthenaBaseCardController
	{
		/*
		 * This card is immune to toxic damage.
		 * 
		 * The first time each turn {Athena} deals damage to a villain target,
		 *  this card deals 1 target 1 melee damage.
		 * 
		 * At the end of your turn, if there is an [u]aspect[/u] card in play,
		 *  this card deals 1 target 1 toxic damage.
		 */

		private const string FirstDamageByAthena = "FirstDamageByAthena";

		public ErichthoniusCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
			base.SpecialStringMaker.ShowHasBeenUsedThisTurn(
				FirstDamageByAthena,
				"{0} has already lashed out this turn.",
				"{0} has not yet lashed out this turn."
			);
		}

		public override void AddTriggers()
		{
			// This card is immune to toxic damage.
			AddImmuneToDamageTrigger(
				(DealDamageAction dd) => dd.Target == base.Card && dd.DamageType == DamageType.Toxic
			);

			// The first time each turn {Athena} deals damage to a villain target,
			// this card deals 1 target 1 melee damage.
			AddTrigger(
				(DealDamageAction dd) =>
					dd.Target.IsVillain
					&& dd.DamageSource.Card == base.CharacterCard
					&& dd.DidDealDamage
					&& !HasBeenSetToTrueThisTurn(FirstDamageByAthena),
				(DealDamageAction dd) => HelpStrikeResponse(dd),
				TriggerType.DealDamage,
				TriggerTiming.After
			);
			AddAfterLeavesPlayAction(
				(GameAction ga) => ResetFlagAfterLeavesPlay(FirstDamageByAthena),
				TriggerType.Hidden
			);

			// At the end of your turn, if there is an [u]aspect[/u] card in play,
			// this card deals 1 target 1 toxic damage.
			AddEndOfTurnTrigger(
				(TurnTaker tt) => tt == base.TurnTaker && AspectInPlay,
				(PhaseChangeAction p) => GameController.SelectTargetsAndDealDamage(
					DecisionMaker,
					new DamageSource(GameController, base.Card),
					1,
					DamageType.Toxic,
					1,
					false,
					1,
					cardSource: GetCardSource()
				),
				TriggerType.DealDamage
			);

			base.AddTriggers();
		}

		private IEnumerator HelpStrikeResponse(DealDamageAction dd)
		{
			SetCardPropertyToTrueIfRealAction(FirstDamageByAthena);
			IEnumerator strikeCR = GameController.SelectTargetsAndDealDamage(
				DecisionMaker,
				new DamageSource(GameController, base.Card),
				1,
				DamageType.Melee,
				1,
				false,
				1,
				cardSource: GetCardSource()
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(strikeCR);
			}
			else
			{
				GameController.ExhaustCoroutine(strikeCR);
			}

			yield break;
		}
	}
}