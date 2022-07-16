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
	public class CadaverousHatCardController : CardController
	{
		public CadaverousHatCardController(Card card, TurnTakerController turnTakerController)
			: base(card, turnTakerController)
		{
		}

		public override void AddTriggers()
		{
			// Reduce damage dealt to haunt cards by 1.
			AddReduceDamageTrigger(
				(Card c) => c.DoKeywordsContain("haunt"),
				1
			);

			// Whenever a villain ongoing is destroyed...
			AddTrigger(
				(DestroyCardAction d) => d.CardToDestroy.Card.IsOngoing
				&& d.CardToDestroy.Card.IsVillain
				&& d.WasCardDestroyed,
				// ...discard the top card of each hero deck.
				(DestroyCardAction dca) => GameController.DiscardTopCardsOfDecks(
					null,
					(Location l) => l.OwnerTurnTaker.IsHero && !l.OwnerTurnTaker.IsIncapacitatedOrOutOfGame,
					1,
					responsibleTurnTaker: this.TurnTaker,
					cardSource: GetCardSource()
				),
				TriggerType.DiscardCard,
				TriggerTiming.After
			);

			// At the end of their turn, a player...
			AddTrigger(
				(PhaseChangeAction pca) => pca.ToPhase.IsEnd && pca.ToPhase.TurnTaker.IsHero,
				MutualDestructionResponse,
				TriggerType.DestroySelf,
				TriggerTiming.After
			);

			base.AddTriggers();
		}

		private IEnumerator MutualDestructionResponse(PhaseChangeAction pca)
		{
			// ...may destroy one of their equipment cards.
			TurnTaker tt = pca.ToPhase.TurnTaker;
			List<DestroyCardAction> storedResults = new List<DestroyCardAction>();
			IEnumerator destroyEquipCR = GameController.SelectAndDestroyCards(
				FindHeroTurnTakerController(tt.ToHero()),
				new LinqCardCriteria(
					(Card c) => c.Owner == tt && c.IsInPlayAndHasGameText && IsEquipment(c),
					"equipment"
				),
				1,
				optional: true,
				storedResultsAction: storedResults,
				cardSource: GetCardSource()
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(destroyEquipCR);
			}
			else
			{
				GameController.ExhaustCoroutine(destroyEquipCR);
			}

			// If they do so, destroy this card.
			if (DidDestroyCard(storedResults))
			{
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
}
