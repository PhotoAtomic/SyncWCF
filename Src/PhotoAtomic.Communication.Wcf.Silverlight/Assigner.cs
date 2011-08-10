namespace PhotoAtomic.Communication.Wcf.Silverlight
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;

    /// <summary>
    /// Performs assignemnt back to the out and ref parameters
    /// </summary>
    public class Assigner<T> 
    {
        /// <summary>
        /// performs assigmnet on out and ref parameters
        /// </summary>
        /// <param name="exp">expression to evaluate</param>
        /// <param name="values">the array of values to assign back</param>
        public void Assign(Expression<Action<T>> exp, params object[] values)
        {
            PerformAssignation(exp, values);
        }

        /// <summary>
        /// performs assigmnet on out and ref parameters
        /// </summary>
        /// <param name="exp">expression to evaluate</param>
        /// <param name="values">the array of values to assign back</param>
        public void Assign<TResult>(Expression<Func<T, TResult>> exp, params object[] values)
        {
            PerformAssignation(exp, values);
        }

        /// <summary>
        /// performs assignation
        /// </summary>
        /// <param name="exp">expression to evaluate</param>
        /// <param name="values">values to return</param>
        private void PerformAssignation(Expression exp, object[] values)
        {
            var lambda = exp as LambdaExpression;
            var method = lambda.Body as MethodCallExpression;

            var assigmentToPerform =
                method.Method.GetParameters()
                .Zip(method.Arguments,
                    (p, a) =>
                        new
                        {
                            parameter = p,
                            argument = a,
                            isByRef = p.ParameterType.IsByRef
                        })
                .Where(x => x.isByRef == true);

            var statements = new List<Expression>();

            foreach (var assignment in assigmentToPerform)
            {
                Expression value;
                if (values == null)
                {
                    value = Expression.Default(assignment.argument.Type);
                }
                else
                {
                    if (assignment.parameter.Position < values.Length)
                    {
                        value =
                            Expression.Convert(
                                Expression.Constant(
                                    values[assignment.parameter.Position]),
                                assignment.argument.Type);
                    }
                    else
                    {
                        value = Expression.Default(assignment.argument.Type);
                    }
                }
                statements.Add(Expression.Assign(assignment.argument, value));
            }

            if (statements.Count != 0)
            {
                var assigner = Expression.Lambda(
                                    Expression.Block(
                                        statements));

                assigner.Compile().DynamicInvoke();
            }
        }
    }
}
