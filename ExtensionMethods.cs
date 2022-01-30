using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Handelabra;

namespace Angille
{
	public static class ExtensionMethods
	{
		public static void ReorderTokenPool(this TokenPool[] tokenPools, string poolThatShouldBeFirst)
		{
			var temp = new List<TokenPool>(tokenPools);
			int targetIndex = temp.FindIndex(tp => string.Equals(
				tp.Identifier,
				poolThatShouldBeFirst,
				StringComparison.Ordinal
			));
			//if targetIndex == -1, no matching pool found, make no change.
			//if targetIndex == 0, matching pool already first, make no change.
			if (targetIndex > 0)
			{
				var newFirst = tokenPools[targetIndex];

				//shuffle all other indexes forward without changing the relative order
				int index = targetIndex;
				while (index > 0)
				{
					tokenPools[index] = tokenPools[--index];
				}
				tokenPools[0] = newFirst;
			}
		}

		/* ended up putting this in a BaseCardController instead
		public static IEnumerator DiscardResponse(this CardController card)
		{
			yield break;
		}
		*/
	}
}
