using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.Nexus
{
	public class SandblastCardController : NexusOneShotCardController
	{
		/*
		 * {Nexus} deals 1 target 2 projectile damage and 1 different target 2 melee damage, in either order.
		 * 
		 * {Nexus} may deal herself 2 irreducible psychic damage.
		 * if she takes damage this way, another player may use a power.
		 */

		public SandblastCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController, DamageType.Projectile, DamageType.Melee)
		{
		}

		public override IEnumerator Play()
		{
			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(base.Play());
			}
			else
			{
				GameController.ExhaustCoroutine(base.Play());
			}

			// {Nexus} may deal herself 2 irreducible psychic damage.
			List<DealDamageAction> storedDamage = new List<DealDamageAction>();
			IEnumerator selfDamageCR = DealDamage(
				this.CharacterCard,
				this.CharacterCard,
				2,
				DamageType.Psychic,
				isIrreducible: true,
				optional: true,
				storedResults: storedDamage
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(selfDamageCR);
			}
			else
			{
				GameController.ExhaustCoroutine(selfDamageCR);
			}

			// if she takes damage this way...
			if (DidDealDamage(storedDamage, this.CharacterCard, this.CharacterCard))
			{
				// ...another player may use a power.
				IEnumerator powerCR = GameController.SelectHeroToUsePower(
					DecisionMaker,
					additionalCriteria: new LinqTurnTakerCriteria((TurnTaker tt) => tt != this.TurnTaker),
					cardSource: GetCardSource()
				);

				if (UseUnityCoroutines)
				{
					yield return GameController.StartCoroutine(powerCR);
				}
				else
				{
					GameController.ExhaustCoroutine(powerCR);
				}
			}

			yield break;
		}
	}
}