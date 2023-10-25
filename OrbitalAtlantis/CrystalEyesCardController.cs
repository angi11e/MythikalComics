using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.OrbitalAtlantis
{
	public class CrystalEyesCardController : CardController
	{
		/*
		 * at the start of the environment turn,
		 * each active player may remove a token from a zone card's bias pool.
		 * each player who does may discard the top card of the villain deck.
		 * each player who does not discards a card.
		 */

		public CrystalEyesCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
		}

		public override void AddTriggers()
		{
			// at the start of the environment turn each active player may...
			AddStartOfTurnTrigger(
				(TurnTaker tt) => tt == this.TurnTaker,
				(PhaseChangeAction p) => GameController.SelectTurnTakersAndDoAction(
					null,
					new LinqTurnTakerCriteria(
						(TurnTaker tt) => IsHero(tt) && !tt.IsIncapacitatedOrOutOfGame,
						"active heroes"
					),
					SelectionType.RemoveTokens,
					BiasResponse,
					cardSource: GetCardSource()
				),
				new TriggerType[] { TriggerType.ModifyTokens, TriggerType.DiscardCard }
			);

			base.AddTriggers();
		}

		private IEnumerator BiasResponse(TurnTaker tt)
		{
			// ...each active player may remove a token from a zone card's bias pool.
			List<RemoveTokensFromPoolAction> tokenResults = new List<RemoveTokensFromPoolAction>();
			List<SelectCardDecision> zoneResults = new List<SelectCardDecision>();
			HeroTurnTakerController httc = FindHeroTurnTakerController(tt.ToHero());

			IEnumerator selectCardCR = GameController.SelectCardAndStoreResults(
				httc,
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

			if (DidRemoveTokens(tokenResults))
			{
				// each player who does may discard the top card of the villain deck.
				List<YesNoCardDecision> yesOrNo = new List<YesNoCardDecision>();
				IEnumerator yesNoDiscardCR = GameController.MakeYesNoCardDecision(
					httc,
					SelectionType.DiscardFromDeck,
					this.Card,
					storedResults: yesOrNo,
					cardSource: GetCardSource()
				);

				if (UseUnityCoroutines)
				{
					yield return GameController.StartCoroutine(yesNoDiscardCR);
				}
				else
				{
					GameController.ExhaustCoroutine(yesNoDiscardCR);
				}

				if (yesOrNo.Count > 0 && yesOrNo.FirstOrDefault().Answer == true)
				{
					List<SelectLocationDecision> villainResult = new List<SelectLocationDecision>();
					IEnumerator getDeckCR = FindVillainDeck(
						httc,
						SelectionType.DiscardFromDeck,
						villainResult,
						(Location l) => true
					);

					if (UseUnityCoroutines)
					{
						yield return GameController.StartCoroutine(getDeckCR);
					}
					else
					{
						GameController.ExhaustCoroutine(getDeckCR);
					}
					Location deckToDiscard = GetSelectedLocation(villainResult);

					if (deckToDiscard != null)
					{
						List<MoveCardAction> notNullIGuess = new List<MoveCardAction>();
						IEnumerator discardCR = GameController.DiscardTopCard(
							deckToDiscard,
							notNullIGuess,
							responsibleTurnTaker: tt,
							cardSource: GetCardSource()
						);

						if (UseUnityCoroutines)
						{
							yield return GameController.StartCoroutine(discardCR);
						}
						else
						{
							GameController.ExhaustCoroutine(discardCR);
						}
					}
				}
			}
			else
			{
				// each player who does not discards a card.
				IEnumerator heroDiscardCR = GameController.SelectAndDiscardCard(
					httc,
					false,
					cardSource: GetCardSource()
				);

				if (UseUnityCoroutines)
				{
					yield return GameController.StartCoroutine(heroDiscardCR);
				}
				else
				{
					GameController.ExhaustCoroutine(heroDiscardCR);
				}
			}

			yield break;
		}
	}
}