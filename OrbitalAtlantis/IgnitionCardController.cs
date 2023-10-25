using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.OrbitalAtlantis
{
	public class IgnitionCardController : CardController
	{
		/*
		 * when this card enters play,
		 * play the top card of each deck in turn order, starting with the villain deck.
		 * 
		 * increase all HP recovery by 1.
		 * 
		 * this card is indestructible.
		 */

		public IgnitionCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
			AddThisCardControllerToList(CardControllerListType.MakesIndestructible);
		}

		public override bool AskIfCardIsIndestructible(Card card)
		{
			// this card is indestructible.
			return card == this.Card;
		}

		public override void AddTriggers()
		{
			// increase all HP recovery by 1.
			AddTrigger(
				(GainHPAction hp) => true,
				(GainHPAction hp) => GameController.IncreaseHPGain(
					hp,
					1,
					GetCardSource()
				),
				TriggerType.IncreaseHPGain,
				TriggerTiming.Before
			);

			base.AddTriggers();
		}

		public override IEnumerator Play()
		{
			// play the top card of each deck in turn order, starting with the villain deck.
			IEnumerator playAllCR = PlayTopCardOfEachDeckInTurnOrder(
				(TurnTakerController ttc) => true,
				(Location l) => true,
				this.TurnTaker
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(playAllCR);
			}
			else
			{
				GameController.ExhaustCoroutine(playAllCR);
			}

			yield break;
		}
	}
}