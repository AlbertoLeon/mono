//
// ConditionParser.cs
//
// Author:
//   Marek Sieradzki (marek.sieradzki@gmail.com)
//   Jaroslaw Kowalski <jaak@jkowalski.net>
// 
// (C) 2006 Marek Sieradzki
// (C) 2004-2006 Jaroslaw Kowalski
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

#if NET_2_0

using System;
using System.Collections;
using System.Text;

namespace Microsoft.Build.BuildEngine {

	internal class ConditionParser {
	
		ConditionTokenizer tokenizer = new ConditionTokenizer ();
		
		private ConditionParser (string condition)
		{
			tokenizer.Tokenize (condition);
		}
		
		public static ConditionExpression ParseCondition (string condition)
		{
			ConditionParser parser = new ConditionParser (condition);
			ConditionExpression e = parser.ParseExpression ();
			
			if (!parser.tokenizer.IsEOF ())
				throw new ExpressionParseException (String.Format ("Unexpected token: {0}", parser.tokenizer.Token.Value));
			
			return e;
		}

		public static bool ParseAndEvaluate (string condition, Project context)
		{
			if (String.IsNullOrEmpty (condition))
				return true;

			ConditionExpression ce = ParseCondition (condition);

			if (!ce.CanEvaluateToBool (context))
				return false;

			return ce.BoolEvaluate (context);
		}
		
		private ConditionExpression ParseExpression ()
		{
			return ParseBooleanExpression ();
		}
		
		private ConditionExpression ParseBooleanExpression ()
		{
			return ParseBooleanAnd ();
		}
		
		private ConditionExpression ParseBooleanAnd ()
		{
			ConditionExpression e = ParseBooleanOr ();
			
			while (tokenizer.IsToken (TokenType.And)) {
				tokenizer.GetNextToken ();
				e = new ConditionAndExpression ((ConditionExpression) e, (ConditionExpression) ParseBooleanOr ());
			}
			
			return e;
		}
		
		private ConditionExpression ParseBooleanOr ()
		{
			ConditionExpression e = ParseRelationalExpression ();
			
			while (tokenizer.IsToken (TokenType.Or)) {
				tokenizer.GetNextToken ();
				e = new ConditionOrExpression ((ConditionExpression) e, (ConditionExpression) ParseRelationalExpression ());
			}
			
			return e;
		}
		
		private ConditionExpression ParseRelationalExpression ()
		{
			ConditionExpression e = ParseFactorExpression ();
			Token opToken;
			RelationOperator op;
			
			if (tokenizer.IsToken (TokenType.Less) ||
				tokenizer.IsToken (TokenType.Greater) ||
				tokenizer.IsToken (TokenType.Equal) ||
				tokenizer.IsToken (TokenType.NotEqual) ||
				tokenizer.IsToken (TokenType.LessOrEqual) ||
				tokenizer.IsToken (TokenType.GreaterOrEqual)) {
				
				opToken = tokenizer.Token;
				tokenizer.GetNextToken ();
								
				switch (opToken.Type) {
				case TokenType.Equal:
					op = RelationOperator.Equal;
					break;
				case TokenType.NotEqual:
					op = RelationOperator.NotEqual;
					break;
				case TokenType.Less:
					op = RelationOperator.Less;
					break;
				case TokenType.LessOrEqual:
					op = RelationOperator.LessOrEqual;
					break;
				case TokenType.Greater:
					op = RelationOperator.Greater;
					break;
				case TokenType.GreaterOrEqual:
					op = RelationOperator.GreaterOrEqual;
					break;
				default:
					throw new ExpressionParseException (String.Format ("Wrong relation operator {0}", opToken.Value));
				}
				
				e =  new ConditionRelationalExpression ((ConditionExpression) e, ParseFactorExpression (), op);
			}
			
			return e;
		}
		
		// FIXME: parse sub expression in parens, parse TokenType.Not, parse functions
		private ConditionExpression ParseFactorExpression ()
		{
			Token token = tokenizer.Token;
			tokenizer.GetNextToken ();
			
			if (token.Type != TokenType.String && token.Type != TokenType.Number)
				throw new ExpressionParseException (String.Format ("Unexpected token type {0}.", token.Type));
			
			return new ConditionFactorExpression (token);
		}
	}
}

#endif
