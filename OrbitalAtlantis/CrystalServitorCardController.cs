using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.OrbitalAtlantis
{
	public class CrystalServitorCardController : CardController
	{
		/*
		 * at the start and end of the villain turn,
		 * you may remove a token from a zone card's bias pool.
		 * if you do, 1 player may play a card.
		 * otherwise, play the top card of the villain deck.
		 */

		public CrystalServitorCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
		}

		public override void AddTriggers()
		{
			// at the start and end of the villain turn...
			AddStartOfTurnTrigger(
				(TurnTaker tt) => IsVillain(tt) && !tt.IsIncapacitatedOrOutOfGame,
				BiasResponse,
				TriggerType.ModifyTokens
			);
			AddEndOfTurnTrigger(
				(TurnTaker tt) => IsVillain(tt) && !tt.IsIncapacitatedOrOutOfGame,
				BiasResponse,
				TriggerType.ModifyTokens
			);

			base.AddTriggers();
		}

		private IEnumerator BiasResponse(PhaseChangeAction p)
		{
			// ...you may remove a token from a zone card's bias pool.
			List<RemoveTokensFromPoolAction> tokenResults = new List<RemoveTokensFromPoolAction>();
			List<SelectCardDecision> zoneResults = new List<SelectCardDecision>();

			IEnumerator selectCardCR = GameController.SelectCardAndStoreResults(
				DecisionMaker,
				SelectionType.RemoveTokens,
				new LinqCardCriteria(
					(Card c) => c.IsInPlayAndNotUnderCard
						&& c.DoKeywordsContain("zone")
						&& c.FindTokenPool("bias") != null
						&& c.FindTokenPool("bias").CurrentValue > 0,
					"zone cards with bias tokens"
				),
				zoneResults,
				true,
				cardSource: GetCardSource()
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(selectCardCR);
			}
			else
			{
				GameController.ExhaustCoroutine(selectCardCR);
			}

			if (DidSelectCard(zoneResults))
			{
				IEnumerator removeTokensCR = GameController.RemoveTokensFromPool(
					zoneResults.FirstOrDefault().SelectedCard.FindTokenPool("bias"),
					1,
					tokenResults,
					cardSource: GetCardSource()
				);

				if (UseUnityCoroutines)
				{
					yield return GameController.StartCoroutine(removeTokensCR);
				}
				else
				{
					GameController.ExhaustCoroutine(removeTokensCR);
				}
			}

			IEnumerator playCardCR = DoNothing();
			if (DidRemoveTokens(tokenResults))
			{
				// if you do, 1 player may play a card.
				playCardCR = SelectHeroToPlayCard(
					DecisionMaker,
					heroCriteria: new LinqTurnTakerCriteria(
						(TurnTaker tt) => IsHero(tt) && !tt.IsIncapacitated
					)
				);
			}
			else
			{
				// otherwise, play the top card of the villain deck.
				playCardCR = GameController.PlayTopCard(
					DecisionMaker,
					FindTurnTakerController(p.ToPhase.TurnTaker),
					cardSource: GetCardSource()
				);
			}

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(playCardCR);
			}
			else
			{
				GameController.ExhaustCoroutine(playCardCR);
			}

			yield break;
		}
	}
}