﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Stareater.AppData.Expressions
{
	class ExponentSequence : IExpressionNode
	{
		IExpressionNode[] sequence;

		public ExponentSequence(IExpressionNode[] sequence)
		{
			this.sequence = sequence;
		}

		public IExpressionNode Simplified()
		{
			if (sequence.Length == 1)
				return sequence[0];
			
			if (isConstant)
				return new Constant(Evaluate(null));
			
			return this;
		}

		public bool isConstant
		{
			get
			{
				return sequence.All(element => element.isConstant);
			}
		}

		public double Evaluate(IDictionary<string, double> variables)
		{
			double result = sequence[sequence.Length - 1].Evaluate(variables);
			for (int i = sequence.Length - 2; i >= 0; i--)
				result = Math.Pow(sequence[i].Evaluate(variables), result);

			return result;
		}
		
		public IEnumerable<string> Variables 
		{ 
			get
			{
				foreach(var node in sequence)
					foreach(var variable in node.Variables)
						yield return variable;
			}
		}
	}
}
