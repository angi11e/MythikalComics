﻿using System;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Angille.Knyfe
{
	public class MythikalKnyfeCharacterCardController : HeroCharacterCardController
	{
		public MythikalKnyfeCharacterCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
		}

		public override IEnumerator UsePower(int index = 0)
		{

			yield break;
		}

		public override IEnumerator UseIncapacitatedAbility(int index)
		{
			switch (index)
			{
				case 0:
					break;

				case 1:
					break;

				case 2:
					break;
			}
			yield break;
		}
	}
}