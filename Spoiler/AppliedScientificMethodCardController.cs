using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.Spoiler
{
	public class AppliedScientificMethodCardController : SpoilerEquipmentCardController
	{
		public AppliedScientificMethodCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
			SpecialStringMaker.ShowListOfCardsInPlay(
				new LinqCardCriteria((Card c) => IsOngoing(c) && c.Owner == this.TurnTaker, "ongoing")
			);
		}

		public override void AddTriggers()
		{
			// At the end of your turn, you may activate a [u]rewind[/i] text.
			AddEndOfTurnTrigger(
				(TurnTaker tt) => tt == this.TurnTaker,
				(PhaseChangeAction pca) => GameController.SelectAndActivateAbility(
					DecisionMaker,
					"rewind",
					optional: true,
					cardSource: GetCardSource()
				),
				TriggerType.ActivateTriggers
			);

			base.AddTriggers();
		}

		public override IEnumerator UsePower(int index = 0)
		{
			// You may activate a [u]rewind[/u] text.
			int preRewind = Journal.ActivateAbilityEntries().Where(
				(ActivateAbilityJournalEntry j) => j.AbilityKey == "rewind"
			).Count();
			IEnumerator activateCR = GameController.SelectAndActivateAbility(
				DecisionMaker,
				"rewind",
				optional: true,
				cardSource: GetCardSource()
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(activateCR);
			}
			else
			{
				GameController.ExhaustCoroutine(activateCR);
			}

			// If you do not...
			if (preRewind == Journal.ActivateAbilityEntries().Where(
				(ActivateAbilityJournalEntry j) => j.AbilityKey == "rewind"
			).Count())
			{
				// ...you may play a card.
				IEnumerator playCardCR = SelectAndPlayCardFromHand(this.HeroTurnTakerController);

				if (UseUnityCoroutines)
				{
					yield return GameController.StartCoroutine(playCardCR);
				}
				else
				{
					GameController.ExhaustCoroutine(playCardCR);
				}
			}

			yield break;
		}
	}
}