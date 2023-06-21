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
	public class CardChicaneryCardController : CardController
	{
		public CardChicaneryCardController(Card card, TurnTakerController turnTakerController)
			: base(card, turnTakerController)
		{
		}

		public override IEnumerator Play()
		{
			// Reveal the top card of each villain deck
			IEnumerator revealAndDoStuffCR = GameController.SelectTurnTakersAndDoAction(
				DecisionMaker,
				new LinqTurnTakerCriteria(
					tt => GameController.IsTurnTakerVisibleToCardSource(tt, GetCardSource())
					&& (tt.IsVillainTeam || IsVillain(tt))
				),
				SelectionType.RevealTopCardOfDeck,
				RevealAndDoStuffResponse,
				allowAutoDecide: true,
				cardSource: GetCardSource()
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(revealAndDoStuffCR);
			}
			else
			{
				GameController.ExhaustCoroutine(revealAndDoStuffCR);
			}

			// The player with the most cards in their trash...
			List<TurnTaker> heroList = new List<TurnTaker>();
			IEnumerator trashHeroCR = FindHeroWithMostCardsInTrash(heroList);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(trashHeroCR);
			}
			else
			{
				GameController.ExhaustCoroutine(trashHeroCR);
			}

			TurnTaker trashHero = heroList.FirstOrDefault();
			if (trashHero.CharacterCard.IsTarget)
			{
				// ...shuffles their trash into their deck...
				IEnumerator shuffleCR = GameController.ShuffleTrashIntoDeck(
					FindTurnTakerController(trashHero)
				);

				// ...then discards 2 cards.
				IEnumerator discardCR = SelectAndDiscardCards(
					FindHeroTurnTakerController(trashHero.ToHero()),
					2
				);

				if (UseUnityCoroutines)
				{
					yield return GameController.StartCoroutine(shuffleCR);
					yield return GameController.StartCoroutine(discardCR);
				}
				else
				{
					GameController.ExhaustCoroutine(shuffleCR);
					GameController.ExhaustCoroutine(discardCR);
				}
			}

			yield break;
		}

		private IEnumerator RevealAndDoStuffResponse(TurnTaker tt)
		{

			// Put any targets into play and discard the rest.
			List<MoveCardAction> storedResults = new List<MoveCardAction>();
			IEnumerator revealPlayDiscardCR = RevealCard_PlayItOrDiscardIt(
				FindTurnTakerController(tt),
				tt.Deck,
				true,
				false,
				new LinqCardCriteria((Card c) => c.IsTarget, "target"),
				storedResults,
				true,
				this.TurnTaker
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(revealPlayDiscardCR);
			}
			else
			{
				GameController.ExhaustCoroutine(revealPlayDiscardCR);
			}

			Log.Debug("cards revealed: " + storedResults.Count);
			// Targets that enter play this way deal themselves 1 irreducible psychic damage.
			if (storedResults.FirstOrDefault() != null && storedResults.FirstOrDefault().Destination.IsInPlay)
			{
				IEnumerator selfDamageCR = GameController.DealDamageToSelf(
					DecisionMaker,
					(Card c) => c == storedResults.FirstOrDefault().CardToMove,
					1,
					DamageType.Psychic,
					true,
					cardSource: GetCardSource()
				);

				if (UseUnityCoroutines)
				{
					yield return GameController.StartCoroutine(selfDamageCR);
				}
				else
				{
					GameController.ExhaustCoroutine(selfDamageCR);
				}
			}

			yield break;
		}
	}
}
