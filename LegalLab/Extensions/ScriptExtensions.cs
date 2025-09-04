using System.Reflection;
using Waher.Script;
using Waher.Script.Content;
using Waher.Script.Content.Functions.Encoding;
using Waher.Script.Data.Functions;
using Waher.Script.Fractals;
using Waher.Script.Functions.Runtime;
using Waher.Script.Model;
using Waher.Script.Networking.Functions;
using Waher.Script.Operators;
using Waher.Script.Operators.Assignments;
using Waher.Script.Operators.Membership;
using Waher.Script.Persistence.SQL;

namespace LegalLab.Extensions
{
	/// <summary>
	/// Static class with script extensions.
	/// </summary>
	public static class ScriptExtensions
	{
		private readonly static Assembly scriptContent = typeof(GraphEncoder).Assembly;
		private readonly static Assembly[] prohibitedAssemblies =
		[
			scriptContent,						// Waher.Script.Content
			typeof(FractalGraph).Assembly,		// Waher.Script.Fractals
			typeof(WhoIs).Assembly,				// Waher.Script.Networking
			typeof(Select).Assembly,			// Waher.Script.Persistence
			typeof(ConnectMsSql).Assembly		// Waher.Script.Data
		];

		/// <summary>
		/// Checks if an expression is safe to execute (if it comes from an external source).
		/// </summary>
		/// <param name="Expression">Parsed expression.</param>
		/// <param name="Prohibited">Element that is prohibited.</param>
		/// <returns>If the expression is safe to execute.</returns>
		public static bool CheckExpressionSafe(this Expression Expression, out ScriptNode Prohibited)
		{
			return CheckExpressionSafe(Expression, false, false, false, out Prohibited);
		}

		/// <summary>
		/// Checks if an expression is safe to execute (if it comes from an external source).
		/// </summary>
		/// <param name="Expression">Parsed expression.</param>
		/// <param name="AllowNamedMembers">If named members are allowed.</param>
		/// <param name="AllowError">If error funcion is allowed.</param>
		/// <param name="AllowCustomFunctions">If custom functions are allowed.</param>
		/// <param name="Prohibited">Element that is prohibited.</param>
		/// <returns>If the expression is safe to execute.</returns>
		public static bool CheckExpressionSafe(this Expression Expression, bool AllowNamedMembers, bool AllowError,
			bool AllowCustomFunctions, out ScriptNode Prohibited)
		{
			ScriptNode Prohibited2 = null;
			bool Safe = Expression.ForAll((ScriptNode Node, out ScriptNode NewNode, object State) =>
			{
				NewNode = null;

				Assembly Assembly = Node.GetType().Assembly;

				foreach (Assembly A in prohibitedAssemblies)
				{
					if (A.FullName == Assembly.FullName)
					{
						if (A == scriptContent &&
							(Node is Waher.Script.Content.Functions.Duration ||
							Node.GetType().Namespace == typeof(Utf8Encode).Namespace))
						{
							return true;
						}

						Prohibited2 = Node;
						return false;
					}
				}

				if ((Node is NamedMember && !AllowNamedMembers) ||
					(Node is NamedMemberAssignment && !AllowNamedMembers) ||
					(Node is LambdaDefinition && !AllowCustomFunctions) ||
					Node is NamedMethodCall ||
					Node is DynamicFunctionCall ||
					Node is DynamicMember ||
					Node is Create ||
					Node is Destroy ||
					(Node is Error && !AllowError))
				{
					Prohibited2 = Node;
					return false;
				}

				return true;

			}, null, SearchMethod.TreeOrder);

			Prohibited = Prohibited2;
			return Safe;
		}
	}
}
