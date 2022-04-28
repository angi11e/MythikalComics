using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.Athena
{
	public class ParthenosCardController : AspectBaseCardController
	{
		/*
		 * When this card enters play, destroy any other [u]aspect[/u] cards.
		 * 
		 * At the end of your turn, {Athena} regains 1 HP.
		 * 
		 * POWER
		 * Search your Deck or Trash for a [u]legend[/u] and put it into Play.
		 *  If you searched your Deck, shuffle your Deck.
		 */

		public ParthenosCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
		}

		public override void AddTriggers()
		{
			// At the end of your turn, {Athena} regains 1 HP.
			AddEndOfTurnTrigger(
				(TurnTaker tt) => tt == base.TurnTaker,
				(PhaseChangeAction p) => GameController.GainHP(
					base.CharacterCard,
					1,
					cardSource: GetCardSource()
				),
				TriggerType.GainHP
			);

			base.AddTriggers();
		}

		public override IEnumerator UsePower(int index = 0)
		{
			// Search your Deck or Trash for a [u]legend[/u] and put it into Play.
			// If you searched your Deck, shuffle your Deck.
			IEnumerator searchCR = SearchForCards(
				DecisionMaker,
				searchDeck: true,
				searchTrash: true,
				1,
				1,
				IsLegendCriteria(),
				putIntoPlay: true,
				putInHand: false,
				putOnDeck: false
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(searchCR);
			}
			else
			{
				GameController.ExhaustCoroutine(searchCR);
			}

			yield break;
		}
	}
}