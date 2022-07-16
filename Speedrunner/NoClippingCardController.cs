using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.Speedrunner
{
	public class NoClippingCardController : SpeedrunnerBaseCardController
	{
		/*
		 * You may destroy 1 environment card.
		 * If you do so, {Speedrunner} regains 2 HP.
		 * If not, play the top card of the environment deck, then you may use a power.
		 */

		public NoClippingCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
		}

		public override IEnumerator Play()
		{
			// You may destroy 1 environment card.
			List<DestroyCardAction> storedResults = new List<DestroyCardAction>();
			IEnumerator destroyCR = GameController.SelectAndDestroyCard(
				DecisionMaker,
				new LinqCardCriteria((Card c) => c.IsEnvironment, "environment"),
				true,
				storedResults,
				cardSource: GetCardSource()
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(destroyCR);
			}
			else
			{
				GameController.ExhaustCoroutine(destroyCR);
			}

			// If you do so, {Speedrunner} regains 2 HP.
			if (DidDestroyCard(storedResults))
			{
				IEnumerator healCR = GameController.GainHP(
					this.CharacterCard,
					2,
					cardSource: GetCardSource()
				);

				if (UseUnityCoroutines)
				{
					yield return GameController.StartCoroutine(healCR);
				}
				else
				{
					GameController.ExhaustCoroutine(healCR);
				}
			}
			else
			{
				// If not, play the top card of the environment deck...
				IEnumerator enviroCR = GameController.PlayTopCard(
					DecisionMaker,
					FindEnvironment(),
					cardSource: GetCardSource()
				);

				// ...then you may use a power.
				IEnumerator usePowerCR = GameController.SelectAndUsePower(
					DecisionMaker,
					cardSource: GetCardSource()
				);

				if (UseUnityCoroutines)
				{
					yield return GameController.StartCoroutine(enviroCR);
					yield return GameController.StartCoroutine(usePowerCR);
				}
				else
				{
					GameController.ExhaustCoroutine(enviroCR);
					GameController.ExhaustCoroutine(usePowerCR);
				}
			}

			yield break;
		}
	}
}