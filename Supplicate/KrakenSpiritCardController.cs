using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.Supplicate
{
	public class KrakenSpiritCardController : YaojingBaseCardController
	{
		/*
		 * at the start of your turn,
		 * this card deals itself or {Supplicate} 2 irreducible psychic damage.
		 * 
		 * at the end of your turn,
		 * this card deals each non-hero target 1 melee damage.
		 * then, this card may deal each target 1 cold damage.
		 */

		public KrakenSpiritCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
		}

		public override void AddTriggers()
		{
			base.AddTriggers();

			// at the end of your turn,
			AddEndOfTurnTrigger(
				(TurnTaker tt) => tt == this.TurnTaker,
				EndOfTurnResponse,
				TriggerType.DealDamage
			);
		}

		private IEnumerator EndOfTurnResponse(PhaseChangeAction pca)
		{
			// this card deals each non-hero target 1 melee damage.
			IEnumerator meleeDamageCR = DealDamage(
				this.Card,
				(Card  c) => !IsHeroTarget(c),
				1,
				DamageType.Melee
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(meleeDamageCR);
			}
			else
			{
				GameController.ExhaustCoroutine(meleeDamageCR);
			}

			var storedYesNo = new List<YesNoCardDecision> { };
			IEnumerator yesOrNoCR = GameController.MakeYesNoCardDecision(
				DecisionMaker,
				SelectionType.DealDamage,
				this.Card,
				storedResults: storedYesNo,
				cardSource: GetCardSource()
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(yesOrNoCR);
			}
			else
			{
				GameController.ExhaustCoroutine(yesOrNoCR);
			}

			if (DidPlayerAnswerYes(storedYesNo))
			{
				// then, this card may deal each target 1 cold damage.
				IEnumerator coldDamageCR = DealDamage(
					this.Card,
					(Card c) => c.IsTarget,
					1,
					DamageType.Cold
				);

				if (UseUnityCoroutines)
				{
					yield return GameController.StartCoroutine(coldDamageCR);
				}
				else
				{
					GameController.ExhaustCoroutine(coldDamageCR);
				}
			}

			yield break;
		}
	}
}