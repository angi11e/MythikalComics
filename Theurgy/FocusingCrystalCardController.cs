using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.Theurgy
{
	public class FocusingCrystalCardController : TheurgyBaseCardController
	{
		// When this card enters play, Theurgy regains 1 hp.
		// The first time a charm card is destroyed each turn, Theurgy regains 2 hp.
		// Power: Theurgy deals 1 target X energy damage,
		//  where X = the number of charm cards in play times 2.
		//  Destroy this card.

		public FocusingCrystalCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
			SpecialStringMaker.ShowNumberOfCardsInPlay(IsCharmCriteria());
		}

		public override IEnumerator Play()
		{
			// Theurgy regains 1 hp
			IEnumerator healTargetCR = GameController.GainHP(
				this.CharacterCard,
				1,
				cardSource: GetCardSource()
			);
			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(healTargetCR);
			}
			else
			{
				GameController.ExhaustCoroutine(healTargetCR);
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
			// Theurgy regains 2 hp
			IEnumerator healTargetCR = GameController.GainHP(
				this.CharacterCard,
				2,
				cardSource: GetCardSource()
			);
			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(healTargetCR);
			}
			else
			{
				GameController.ExhaustCoroutine(healTargetCR);
			}

			yield break;
		}

		public override IEnumerator UsePower(int index = 0)
		{
			int targetNumeral = GetPowerNumeral(0, 1);
			int damageMod = GetPowerNumeral(1, 2);

			// {Theurgy} deals 1 target X energy damage, where X = the number of charm cards in play times 2.
			IEnumerator damageCR = GameController.SelectTargetsAndDealDamage(
				DecisionMaker,
				new DamageSource(GameController, this.CharacterCard),
				CharmCardsInPlay * damageMod,
				DamageType.Energy,
				targetNumeral,
				false,
				targetNumeral,
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

			// Destroy this card
			IEnumerator destructionCR = GameController.DestroyCard(
				DecisionMaker,
				this.Card,
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