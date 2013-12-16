﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Stareater.Utils.NumberFormatters
{
	public class ThousandsFormatter
	{
		private static string[] MagnitudePrefixes = new string[] { "", "k", "M", "G", "T", "P", "E", "Z", "Y" };

		private KeyValuePair<int, double>? magnitudeInfo = null;

		public ThousandsFormatter()
		{ }

		public ThousandsFormatter(string magnitudePrefix)
		{
			int index = Array.FindIndex(MagnitudePrefixes, x => { return x == magnitudePrefix; });
			if (index < 0)
				throw new ArgumentException("Invalid magnitude prefix", "magnitudePrefix");

			double factor = 1;
			for (int i = 0; i < index; i++)
				factor *= 1000;

			this.magnitudeInfo = new KeyValuePair<int, double>(index, factor);
		}

		public ThousandsFormatter(params double[] numbersInContext)
		{
			this.magnitudeInfo = new KeyValuePair<int, double>(int.MaxValue, 0);
			
			foreach (double number in numbersInContext) {
				var candidateInfo = greatestLowerPrefix(number);
				if (candidateInfo.Key < magnitudeInfo.Value.Key)
					this.magnitudeInfo = candidateInfo;
			}
		}

		public string Format(double number)
		{
			var prefixInfo = this.magnitudeInfo ?? greatestLowerPrefix(number);

			return ((number / prefixInfo.Value).ToString("0.##") + " " + MagnitudePrefixes[prefixInfo.Key]).TrimEnd();
		}

		private static KeyValuePair<int, double> greatestLowerPrefix(double number)
		{
			int prefixIndex = 0;
			double weight = 1;
			for (; prefixIndex < MagnitudePrefixes.Length && number >= weight * 1000; prefixIndex++)
				weight *= 1000;

			return new KeyValuePair<int, double>(prefixIndex, weight);
		}

		public static double? TryParse(string numberText)
		{
			if (string.IsNullOrEmpty(numberText))
				return null;

			char inputPrefix = numberText[numberText.Length - 1];
			int index = Array.FindIndex(MagnitudePrefixes, x => { return x.Length > 0 && x[0] == inputPrefix; });
			if (index < 0)
				return null;

			double result;
			if (!double.TryParse(numberText.Substring(0, numberText.Length - 1).TrimEnd(), out result))
				return null;

			for (int i = 0; i < index; i++)
				result *= 1000;

			return result;
		}
	}
}
