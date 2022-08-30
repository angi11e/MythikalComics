using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.Speedrunner
{
	public class InputBufferCardController : SpeedrunnerBaseCardController
	{
		/*
		 * When this card enters play, draw 2 cards.
		 * 
		 * POWER
		 * Discard any number of cards.
		 * Deal 1 target X energy damage, where X = the number of cards discarded this way plus 1.
		 */

		public InputBufferCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
		}

		public override IEnumerator Play()
		{
			// When this card enters play, draw 2 cards.
			return DrawCards(DecisionMaker, 2);
		}

		public override IEnumerator UsePower(int index = 0)
		{
			int targetNumeral = GetPowerNumeral(0, 1);
			int damageNumeral = GetPowerNumeral(1, 1);
			// int extraNumeral = GetPowerNumeral(2, 1);

			// Discard any number of cards.
			List<DiscardCardAction> storedResults = new List<DiscardCardAction>();
			IEnumerator discardCR = SelectAndDiscardCards(
				DecisionMaker,
				null,
				optional: false,
				0,
				storedResults
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(discardCR);
			}
			else
			{
				GameController.ExhaustCoroutine(discardCR);
			}

			// int adjustmentNumeral = extraNumeral * storedResults.Count();

			// Deal 1 target X energy damage...
			IEnumerator dealDamageCR = GameController.SelectTargetsAndDealDamage(
				DecisionMaker,
				new DamageSource(GameController, this.CharacterCard),
				// ...where X = the number of cards discarded this way plus 1.
				damageNumeral + storedResults.Count(),
				DamageType.Energy,
				targetNumeral,
				false,
				targetNumeral,
				cardSource: GetCardSource()
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(dealDamageCR);
			}
			else
			{
				GameController.ExhaustCoroutine(dealDamageCR);
			}

			yield break;
		}
	}
}