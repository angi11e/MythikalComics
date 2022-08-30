using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.PecosBill
{
	public class UnraveledShakeCardController : HyperboleBaseCardController
	{
		/*
		 */

		public UnraveledShakeCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
		}

		public override void AddTriggers()
		{
			base.AddTriggers();

		}

		public override IEnumerator Play()
		{

			yield break;
		}

		public override IEnumerator UsePower(int index = 0)
		{

			yield break;
		}

		public override IEnumerator ActivateAbilityEx(CardDefinition.ActivatableAbilityDefinition definition)
		{

			yield break;
		}
	}
}