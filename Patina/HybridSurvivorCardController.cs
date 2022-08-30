using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.Patina
{
	public class HybridSurvivorCardController : PatinaBaseCardController
	{
		/*
		 * {Patina} deals 1 target 1 melee damage, then deals 1 target 1 cold damage.
		 * {Patina} regains 1 HP.
		 * draw a card or play a card.
		 */

		public HybridSurvivorCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
		}

		public override IEnumerator Play()
		{
			// {Patina} deals 1 target 1 melee damage...
			IEnumerator dealMeleeCR = GameController.SelectTargetsAndDealDamage(
				DecisionMaker,
				new DamageSource(GameController, this.CharacterCard),
				1,
				DamageType.Melee,
				1,
				false,
				1,
				cardSource: GetCardSource()
			);

			// ...then deals 1 target 1 cold damage.
			IEnumerator dealColdCR = GameController.SelectTargetsAndDealDamage(
				DecisionMaker,
				new DamageSource(GameController, this.CharacterCard),
				1,
				DamageType.Cold,
				1,
				false,
				1,
				cardSource: GetCardSource()
			);

			// {Patina} regains 1 HP.
			IEnumerator healingCR = GameController.GainHP(
				this.CharacterCard,
				1,
				cardSource: GetCardSource()
			);

			// draw a card or play a card.
			IEnumerator drawPlayCR = DrawACardOrPlayACard(DecisionMaker, optional: false);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(dealMeleeCR);
				yield return GameController.StartCoroutine(dealColdCR);
				yield return GameController.StartCoroutine(healingCR);
				yield return GameController.StartCoroutine(drawPlayCR);
			}
			else
			{
				GameController.ExhaustCoroutine(dealMeleeCR);
				GameController.ExhaustCoroutine(dealColdCR);
				GameController.ExhaustCoroutine(healingCR);
				GameController.ExhaustCoroutine(drawPlayCR);
			}

			yield break;
		}
	}
}