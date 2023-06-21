using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.Spoiler
{
	public class IllDoThatYesterdayCardController : SpoilerOngoingCardController
	{
		public IllDoThatYesterdayCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
		}

		public override IEnumerator Play()
		{
			// when this card enters play each player may...
			IEnumerator selectPlayersCR = GameController.SelectTurnTakersAndDoAction(
				DecisionMaker,
				new LinqTurnTakerCriteria((TurnTaker tt) => IsHero(tt) && !tt.ToHero().IsIncapacitatedOrOutOfGame),
				SelectionType.MoveCardToHandFromTrash,
				MoveCardToHandResponse,
				allowAutoDecide: true,
				cardSource: GetCardSource()
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(selectPlayersCR);
			}
			else
			{
				GameController.ExhaustCoroutine(selectPlayersCR);
			}

			yield break;
		}

		private IEnumerator MoveCardToHandResponse(TurnTaker tt)
		{
			// ...move a one-shot card from their trash to their hand.
			IEnumerator getOneshotCR = GameController.SelectCardsFromLocationAndMoveThem(
				FindHeroTurnTakerController(tt.ToHero()),
				tt.Trash,
				0,
				1,
				new LinqCardCriteria((Card c) => c.Location == tt.Trash && c.IsOneShot),
				new MoveCardDestination[] { new MoveCardDestination(tt.ToHero().Hand) },
				cardSource: GetCardSource()
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(getOneshotCR);
			}
			else
			{
				GameController.ExhaustCoroutine(getOneshotCR);
			}

			yield break;
		}

		public override IEnumerator ActivateRewind()
		{
			// The environment deals 1 target 2 melee damage.
			DamageSource envSource = new DamageSource(GameController, FindEnvironment().TurnTaker);
			IEnumerator envDamageCR = GameController.SelectTargetsAndDealDamage(
				DecisionMaker,
				envSource,
				2,
				DamageType.Melee,
				1,
				false,
				1,
				cardSource: GetCardSource()
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(envDamageCR);
			}
			else
			{
				GameController.ExhaustCoroutine(envDamageCR);
			}

			yield break;
		}
	}
}