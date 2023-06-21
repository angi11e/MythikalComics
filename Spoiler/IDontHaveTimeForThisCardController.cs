using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.Spoiler
{
	public class IDontHaveTimeForThisCardController : SpoilerOngoingCardController
	{
		public IDontHaveTimeForThisCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
		}

		public override IEnumerator Play()
		{
			// when this card enters play, you may destroy an ongoing or environment card.
			IEnumerator destroyCR = GameController.SelectAndDestroyCard(
				DecisionMaker,
				new LinqCardCriteria((Card c) => c.IsEnvironment || IsOngoing(c), "ongoing or environment"),
				true,
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

			yield break;
		}

		public override IEnumerator ActivateRewind()
		{
			// Reduce the next damage dealt to a hero target by 2.
			ReduceDamageStatusEffect reduceDamageStatusEffect = new ReduceDamageStatusEffect(2);
			reduceDamageStatusEffect.TargetCriteria.IsHero = true;
			reduceDamageStatusEffect.TargetCriteria.IsTarget = true;
			reduceDamageStatusEffect.NumberOfUses = 1;

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(AddStatusEffect(reduceDamageStatusEffect));
			}
			else
			{
				GameController.ExhaustCoroutine(AddStatusEffect(reduceDamageStatusEffect));
			}

			yield break;
		}
	}
}