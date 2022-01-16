using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.Theurgy
{
	public class FocusingCrystalCardController : TheurgyBaseCardController
	{
		// When this card enters play, Theurgy regains 2 hp.
		// The first time a charm card is destroyed each turn, Theurgy regains 1 hp.
		// Power: Theurgy deals 1 target X energy damage,
		//  where X = the number of charm cards in play times 2.
		//  Destroy this card.

		public FocusingCrystalCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
			base.SpecialStringMaker.ShowNumberOfCardsInPlay(IsCharmCriteria());
		}

		public override IEnumerator Play()
		{
			// Theurgy regains 2 hp
			IEnumerator healTargetCR = base.GameController.GainHP(
				base.CharacterCard,
				2,
				cardSource: base.GetCardSource()
			);
			if (UseUnityCoroutines)
			{
				yield return base.GameController.StartCoroutine(healTargetCR);
			}
			else
			{
				base.GameController.ExhaustCoroutine(healTargetCR);
			}
			yield break;
		}

		public override void AddTriggers()
		{
			// first time a charm card is destroyed...
			AddTrigger(
				(DestroyCardAction d) => IsCharm(d.CardToDestroy.Card) &&
				!HasBeenSetToTrueThisTurn("FirstTimeCharmCardDestroyed") &&
				d.WasCardDestroyed,
				GainHPResponse,
				TriggerType.GainHP,
				TriggerTiming.After
			);

			AddAfterLeavesPlayAction(
				(GameAction ga) => ResetFlagAfterLeavesPlay("FirstTimeCharmCardDestroyed"),
				TriggerType.Hidden
			);

			base.AddTriggers();
		}

		private IEnumerator GainHPResponse(GameAction d)
		{
			SetCardPropertyToTrueIfRealAction("FirstTimeCharmCardDestroyed");
			// Theurgy regains 1 hp
			IEnumerator healTargetCR = base.GameController.GainHP(
				base.CharacterCard,
				1,
				cardSource: base.GetCardSource()
			);
			if (UseUnityCoroutines)
			{
				yield return base.GameController.StartCoroutine(healTargetCR);
			}
			else
			{
				base.GameController.ExhaustCoroutine(healTargetCR);
			}

			yield break;
		}
		public override IEnumerator UsePower(int index = 0)
		{
			int targetNumeral = GetPowerNumeral(0, 1);
			int damageMod = GetPowerNumeral(1, 2);

			// count the charm cards.
			int damageNumeral = FindCardsWhere((Card c) => c.IsInPlayAndHasGameText && IsCharm(c)).Count() * damageMod;

			// Theurgy deals 1 target X energy damage.
			IEnumerator damageCR = base.GameController.SelectTargetsAndDealDamage(
				DecisionMaker,
				new DamageSource(base.GameController, base.CharacterCard),
				damageNumeral,
				DamageType.Energy,
				targetNumeral,
				false,
				targetNumeral,
				cardSource: base.GetCardSource()
			);
			if (UseUnityCoroutines)
			{
				yield return base.GameController.StartCoroutine(damageCR);
			}
			else
			{
				base.GameController.ExhaustCoroutine(damageCR);
			}

			// Destroy this card
			IEnumerator destructionCR = GameController.DestroyCard(
				this.DecisionMaker,
				base.Card,
				cardSource: GetCardSource()
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(destructionCR);
			}
			else
			{
				GameController.ExhaustCoroutine(destructionCR);
			}

			yield break;
		}
	}
}