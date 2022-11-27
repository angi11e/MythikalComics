using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.Spoiler
{
	public abstract class SpoilerOngoingCardController : CardController
	{
		protected SpoilerOngoingCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
		}

		public override IEnumerator ActivateAbilityEx(CardDefinition.ActivatableAbilityDefinition definition)
		{
			if (definition.Name == "rewind")
			{
				return ActivateRewind();
			}

			return base.ActivateAbilityEx(definition);
		}

		public virtual IEnumerator ActivateRewind()
		{
			yield return null;
		}
	}
}