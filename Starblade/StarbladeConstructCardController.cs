using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.Starblade
{
	public abstract class StarbladeConstructCardController : CardController
	{
		protected StarbladeConstructCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
		}

		public override IEnumerator ActivateAbilityEx(CardDefinition.ActivatableAbilityDefinition definition)
		{
			if (definition.Name == "technique")
			{
				return ActivateTechnique();
			}

			return base.ActivateAbilityEx(definition);
		}

		public virtual IEnumerator ActivateTechnique()
		{
			yield return null;
		}
	}
}