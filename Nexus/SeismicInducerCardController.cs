using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.Nexus
{
	public class SeismicInducerCardController : NexusEquipmentCardController
	{
		/*
		 * the first time each turn {Nexus} deals melee damage to a target,
		 * she also deals that target 1 infernal damage.
		 * 
		 * POWER
		 * Discard the top card of each deck.
		 * if [i]Aerokinesis[/i] is in play, put the top card of 1 trash into play.
		 */

		public SeismicInducerCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController, DamageType.Melee, DamageType.Infernal)
		{
		}

		public override IEnumerator UsePower(int index = 0)
		{
			int playNumeral = GetPowerNumeral(0, 1);

			// Discard the top card of each deck.
			IEnumerator discardTopsCR = GameController.DiscardTopCardsOfDecks(
				DecisionMaker,
				(Location l) => !l.OwnerTurnTaker.IsIncapacitatedOrOutOfGame,
				1,
				responsibleTurnTaker: this.TurnTaker,
				cardSource: GetCardSource()
			);
			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(discardTopsCR);
			}
			else
			{
				GameController.ExhaustCoroutine(discardTopsCR);
			}

			// if [i]Aerokinesis[/i] is in play
			if (FindCardsWhere(c => c.Identifier == "Aerokinesis" && c.IsInPlayAndHasGameText).Any())
			{
				// put the top card of 1 trash into play.
				SelectCardsDecision theCard = new SelectCardsDecision(
					GameController,
					HeroTurnTakerController,
					(Card c) => c == c.Owner.Trash.TopCard,
					SelectionType.PutIntoPlay,
					playNumeral,
					cardSource: GetCardSource()
				);
				IEnumerator playTopCR = GameController.SelectCardsAndDoAction(
					theCard,
					(SelectCardDecision scd) => GameController.MoveCard(
						TurnTakerController,
						scd.SelectedCard,
						scd.SelectedCard.Owner.PlayArea,
						isPutIntoPlay: true,
						cardSource: GetCardSource()
					)
				);

				if (UseUnityCoroutines)
				{
					yield return GameController.StartCoroutine(playTopCR);
				}
				else
				{
					GameController.ExhaustCoroutine(playTopCR);
				}
			}

			yield break;
		}
	}
}