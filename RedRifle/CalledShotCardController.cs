using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.RedRifle
{
	public class CalledShotCardController : RedRifleBaseCardController
	{
		/*
		 * {RedRifle} deals 1 target 3 projectile damage.
		 * Add 1 or remove 3 tokens from your trueshot pool.
		 * If you removed 3 tokens this way, destroy 1 Ongoing or Environment card.
		 */

		public CalledShotCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
			base.SpecialStringMaker.ShowTokenPool(base.TrueshotPool);
		}

		public override IEnumerator Play()
		{
			// {RedRifle} deals 1 target 3 projectile damage.
			IEnumerator dealDamageCR = GameController.SelectTargetsAndDealDamage(
				DecisionMaker,
				new DamageSource(GameController, base.CharacterCard),
				3,
				DamageType.Projectile,
				1,
				false,
				1,
				cardSource: GetCardSource()
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(dealDamageCR);
			}
			else
			{
				GameController.ExhaustCoroutine(dealDamageCR);
			}

			// Add 1 or remove 3 tokens from your trueshot pool.
			IEnumerator addOrRemoveCR = AddOrRemoveTrueshotTokens<GameAction, GameAction>(
				1,
				3,
				removeTokenResponse: RemoveTokensFromPoolResponse,
				insufficientTokenMessage: "nothing happens."
			);

			yield break;
		}

		private IEnumerator RemoveTokensFromPoolResponse(
			GameAction ga,
			List<RemoveTokensFromPoolAction> storedResults
		)
		{
			// If you removed 3 tokens this way, destroy 1 Ongoing or Environment card.
			IEnumerator destroyCR = GameController.SelectAndDestroyCard(
				DecisionMaker,
				new LinqCardCriteria((Card c) => c.IsEnvironment || c.IsOngoing, "ongoing or environment"),
				false,
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
	}
}