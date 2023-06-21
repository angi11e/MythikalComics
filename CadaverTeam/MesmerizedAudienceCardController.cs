using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Handelabra.Sentinels.Engine.Model;
using Handelabra.Sentinels.Engine.Controller;
using System.Collections;
using Handelabra;

namespace Angille.CadaverTeam
{
	public class MesmerizedAudienceCardController : CardController
	{
		private bool _dealPsychic = true;

		public MesmerizedAudienceCardController(Card card, TurnTakerController turnTakerController)
			: base(card, turnTakerController)
		{
		}

		public override void AddTriggers()
		{
			// Reduce damage dealt to targets in this play area by 2.
			AddReduceDamageTrigger(
				(Card c) => c.Location.OwnerTurnTaker == this.Card.Location.OwnerTurnTaker,
				2
			);

			// At the end of their turn...
			AddTrigger(
				(PhaseChangeAction pca) => pca.ToPhase.IsEnd && IsHero(pca.ToPhase.TurnTaker),
				SkipToDestroyResponse,
				TriggerType.DestroySelf,
				TriggerTiming.After
			);

			// If this card is destroyed any other way, each hero target deals itself 2 psychic damage.
			AddWhenDestroyedTrigger(
				(DestroyCardAction dca) => GameController.DealDamageToSelf(
					DecisionMaker,
					(Card c) => IsHero(c),
					2,
					DamageType.Psychic,
					cardSource: GetCardSource()
				),
				new TriggerType[] { TriggerType.DealDamage },
				(DestroyCardAction dca) => _dealPsychic
			);

			base.AddTriggers();
		}

		private IEnumerator SkipToDestroyResponse(PhaseChangeAction pca)
		{
			// ...if a player has skipped their play and power phases...
			TurnTaker tt = pca.ToPhase.TurnTaker;

			if ( Journal.PlayCardEntriesThisTurn().Any(
					(PlayCardJournalEntry p) => p.CardPlayed.Owner == tt
					&& p.TurnPhase.TurnTaker == tt
					&& p.TurnPhase.IsPlayCard
				) || Journal.UsePowerEntriesThisTurn().Any(
					(UsePowerJournalEntry p) => p.PowerUser == tt
					&& p.TurnPhase.TurnTaker == tt
					&& p.TurnPhase.IsUsePower
			))
			{
				yield break;
			}
			else
			{
				// ...they may discard a card to destroy this card.
				List<DiscardCardAction> storedResults = new List<DiscardCardAction>();
				IEnumerator discardCR = SelectAndDiscardCards(
					FindHeroTurnTakerController(tt.ToHero()),
					1,
					optional: true,
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

				if (storedResults.Count() > 0)
				{
					_dealPsychic = false;

					IEnumerator destroyCR = GameController.DestroyCard(
						DecisionMaker,
						this.Card,
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
				}
			}
		}

		public override IEnumerator DeterminePlayLocation(
			List<MoveCardDestination> storedResults,
			bool isPutIntoPlay,
			List<IDecision> decisionSources,
			Location overridePlayArea = null,
			LinqTurnTakerCriteria additionalTurnTakerCriteria = null
		)
		{
			// Play this card in the villain play area with the fewest copies of this card.
			List<TurnTaker> destinationVillain = new List<TurnTaker>();
			IEnumerator selectVillainCR = GameController.DetermineTurnTakersWithMostOrFewest(
				most: false,
				1,
				1,
				(TurnTaker tt) =>
					IsVillain(tt)
					&& !tt.IsIncapacitatedOrOutOfGame,
				(TurnTaker tt) => (from c in tt.GetPlayAreaCards()
					where c.IsAtLocationRecursive(tt.PlayArea) && c.Identifier == this.Card.Identifier
					select c).Count(),
				SelectionType.FewestCardsInPlayArea,
				destinationVillain,
				cardSource: GetCardSource()
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(selectVillainCR);
			}
			else
			{
				GameController.ExhaustCoroutine(selectVillainCR);
			}

			if (destinationVillain.FirstOrDefault() != null)
			{
				storedResults?.Add(new MoveCardDestination(destinationVillain.FirstOrDefault().PlayArea));
			}
			else
			{
				storedResults?.Add(new MoveCardDestination(this.TurnTaker.PlayArea));
			}

			yield break;
		}
	}
}
