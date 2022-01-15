using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace IQueryableTask
{
    public class SqlVisitor : ExpressionVisitor
    {
        private StringBuilder query;
        private string wherePart = string.Empty;
        public string GetQuery(Expression expression)
        {
            query = new StringBuilder();
            Visit(expression);
            wherePart = query.ToString();
            string finalQuery = "SELECT * FROM person WHERE " + wherePart;
            return finalQuery;
        }
        private Expression StripQuotes(Expression expression)
        {
            while (expression.NodeType == ExpressionType.Quote)
            {
                expression = ((UnaryExpression)expression).Operand;
            }
            return expression;
        }

        protected override Expression VisitMethodCall(MethodCallExpression expression)
        {
            if (expression.Method.DeclaringType == typeof(Queryable) && expression.Method.Name == "Where")
            {
                Visit(expression.Arguments[0]);

                LambdaExpression lambda = (LambdaExpression)StripQuotes(expression.Arguments[1]);
                var body = lambda.Body;
                if (body.ToString().IndexOf("FullName", StringComparison.InvariantCultureIgnoreCase) >= 0 
                    || body.ToString().IndexOf("LastName", StringComparison.InvariantCultureIgnoreCase) >= 0) throw new NotSupportedException();
                Visit(body);
                return expression;
            }
            else if (expression.Method.Name == "Contains")
            {
                string nameProp = expression.Object.ToString().Split('.').Last();

                if (nameProp.Equals("FullName", StringComparison.InvariantCultureIgnoreCase) || nameProp.Equals("LastName", StringComparison.InvariantCultureIgnoreCase)) 
                    throw new NotSupportedException();
                query.Append(nameProp);
                query.Append(" like ");
                var innerLambda = Expression.Lambda<Func<object>>(Expression.Convert(StripQuotes(expression.Arguments[0]), typeof(object)));
                var argument = innerLambda.Compile().Invoke().ToString();
                query.Append($"'%{argument}%'");
                return expression;
            }

            throw new InvalidOperationException();
        }

        protected override Expression VisitUnary(UnaryExpression expression)
        {
            switch (expression.NodeType)
            {
                case ExpressionType.Not:
                    query.Append(" NOT ");
                    Visit(expression.Operand);
                    break;
                case ExpressionType.Convert:
                    Visit(expression.Operand);
                    break;
                default:
                    throw new NotSupportedException();
            }

            return expression;
        }

        protected override Expression VisitBinary(BinaryExpression expression)
        {
            Visit(expression.Left);
            switch (expression.NodeType)
            {
                case ExpressionType.And:
                case ExpressionType.AndAlso:
                    query.Append(" AND ");
                    break;

                case ExpressionType.Or:
                case ExpressionType.OrElse:
                    query.Append(" OR ");
                    break;

                case ExpressionType.Equal:
                    _ = IsNull(expression.Right) ? query.Append(" IS ") : query.Append(" = ");
                    break;

                case ExpressionType.NotEqual:
                    _ = IsNull(expression.Right) ? query.Append(" IS NOT ") : query.Append(" <> ");
                    break;

                case ExpressionType.LessThan:
                    query.Append(" < ");
                    break;
                case ExpressionType.GreaterThan:
                    query.Append(" > ");
                    break;
                default:
                    throw new NotSupportedException();

            }
            Visit(expression.Right);
            return expression;
        }

        protected override Expression VisitConstant(ConstantExpression expression)
        {
            IQueryable check = expression.Value as IQueryable;

            if (check == null && expression.Value == null)
            {
                query.Append("NULL");
            }
            else if (check == null)
            {
                switch (Type.GetTypeCode(expression.Value.GetType()))
                {
                    case TypeCode.Boolean:
                        query.Append(((bool)expression.Value) ? 1 : 0);
                        break;

                    case TypeCode.String:
                        query.Append($"'{expression.Value}'");
                        break;

                    case TypeCode.DateTime:
                        query.Append($"'{expression.Value}'");
                        break;

                    case TypeCode.Object:
                        throw new NotSupportedException();
                    default:
                        query.Append(expression.Value);
                        break;
                }
            }
            return expression;
        }

        protected override Expression VisitMember(MemberExpression expression)
        {
            if (expression.Expression != null && expression.Expression.NodeType == ExpressionType.Parameter)
            {
                query.Append(expression.Member.Name);
                return expression;
            }
            throw new NotSupportedException();
        }

        private bool IsNull(Expression expression)
        {
            return (expression.NodeType == ExpressionType.Constant && ((ConstantExpression)expression).Value == null);
        }
    }
}