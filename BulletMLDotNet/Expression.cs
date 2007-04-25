using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;

namespace BulletML
{
    internal enum Operation { Add, Sub, Div, Mod, Mul }

    internal enum ValueType { Immediate, Rank, Rand, Param, Operation }

    internal class Value
    {
        private ValueType _type;
        private float _immediateValue;
        private int _paramNumber;
        private Operation _op;
        private Value _left, _right;

        private Value()
        {
        }

        public Value(float immediateValue)
        {
            _type = ValueType.Immediate;
            _immediateValue = immediateValue;
        }

        public Value(Operation op, Value left, Value right)
        {
            _type = ValueType.Operation;
            _op = op;
            _left = left;
            _right = right;
        }

        public static Value Param(int paramNumber)
        {
            Value v = new Value();
            v._type = ValueType.Param;
            v._paramNumber = paramNumber;
            return v;
        }

        public static Value Rank
        {
            get
            {
                Value v = new Value();
                v._type = ValueType.Rank;
                return v;
            }
        }

        public static Value Rand
        {
            get
            {
                Value v = new Value();
                v._type = ValueType.Rand;
                return v;
            }
        }

        public override string ToString()
        {
            switch (_type)
            {
                case ValueType.Immediate:
                    return string.Format(new CultureInfo("en-US"), "{0}", _immediateValue);
                case ValueType.Rank:
                    return "$rank";
                case ValueType.Rand:
                    return "$rand";
                case ValueType.Param:
                    return "$" + _paramNumber;
                case ValueType.Operation:
                    string op;
                    switch (_op)
                    {
                        case Operation.Add:
                            op = "+";
                            break;
                        case Operation.Sub:
                            op = "-";
                            break;
                        case Operation.Div:
                            op = "/";
                            break;
                        case Operation.Mod:
                            op = "%";
                            break;
                        case Operation.Mul:
                            op = "*";
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                    return "(" + _left + " " + op + " " + _right + ")";
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public ValueType Type
        {
            get { return _type; }
        }

        public int ParamNumber
        {
            get { return _paramNumber; }
        }

        public float Evaluate(ParameterBind bind)
        {
            switch (_type)
            {
                case ValueType.Immediate:
                    return _immediateValue;
                case ValueType.Operation:
                    switch (_op)
                    {
                        case Operation.Add:
                            return _left.Evaluate(bind) + _right.Evaluate(bind);
                        case Operation.Sub:
                            return _left.Evaluate(bind) - _right.Evaluate(bind);
                        case Operation.Mul:
                            return _left.Evaluate(bind) * _right.Evaluate(bind);
                        case Operation.Div:
                            return _left.Evaluate(bind) / _right.Evaluate(bind);
                        case Operation.Mod:
                            return _left.Evaluate(bind) % _right.Evaluate(bind);
                    }
                    return 0;
                case ValueType.Rand:
                    Random r = new Random();
                    return Convert.ToSingle(r.NextDouble());
                case ValueType.Rank:
                    return bind.Rank;
                case ValueType.Param:
                    return bind[_paramNumber];
            }
            return 0;
        }
    }

    public class Expression
    {
        private Value _val;

        private Expression(Value val)
        {
            _val = val;
        }

        public float Evaluate(ParameterBind bind)
        {
            return _val.Evaluate(bind);
        }

        public static Expression Parse(string exp)
        {
            string exp2 = exp;
            return new Expression(ParseSubExpression(ref exp2));
        }

        public override string ToString()
        {
            return _val.ToString();
        }

        private static Value ParseSubExpression(ref string exp)
        {
            bool openPar, closePar;
            Operation op;
            Operation currentTermOp = Operation.Add;
            Operation currentFactorOp = Operation.Mul;
            Value val;
            Value currentTerm = new Value(0f);
            Value currentFactor = null;
            while (ParseToken(ref exp, out openPar, out closePar, out val, out op))
            {
                if (openPar || val != null)
                {
                    if (openPar)
                    {
                        val = ParseSubExpression(ref exp);
                    }
                    if (currentFactor == null)
                    {
                        currentFactor = val;
                    }
                    else
                    {
                        currentFactor = new Value(currentFactorOp, currentFactor, val);
                    }
                }
                else if (closePar)
                {
                    if (currentFactor != null)
                    {
                        currentTerm = new Value(currentTermOp, currentTerm, currentFactor);
                    }
                    return currentTerm;
                }
                else // operation token
                {
                    if (op == Operation.Add || op == Operation.Sub)
                    {
                        if (currentFactor != null)
                        {
                            currentTerm = new Value(currentTermOp, currentTerm, currentFactor);
                        }
                        currentTermOp = op;
                        currentFactor = null;
                    }
                    else
                    {
                        currentFactorOp = op;
                    }
                }
            }
            if (currentFactor != null)
            {
                currentTerm = new Value(currentTermOp, currentTerm, currentFactor);
            }
            return currentTerm;
        }

        private static bool ParseToken(ref string exp, out bool openParenth, out bool closeParenth, out Value val, out Operation op)
        {
            op = Operation.Add;
            val = null;
            openParenth = false;
            closeParenth = false;
            Match match = Regex.Match(exp, @"^\s*(?<token>\(|\)|\+|-|\*|/|%|\$[0-9]+|\$rank|\$rand|[0-9]*\.[0-9]+|[0-9]+)");
            if (match.Success)
            {
                string token = match.Groups["token"].Value;
                exp = exp.Substring(token.Length);
                switch (token)
                {
                    case "+":
                        op = Operation.Add;
                        return true;
                    case "-":
                        op = Operation.Sub;
                        return true;
                    case "*":
                        op = Operation.Mul;
                        return true;
                    case "/":
                        op = Operation.Div;
                        return true;
                    case "%":
                        op = Operation.Mod;
                        return true;
                    case "(":
                        openParenth = true;
                        return true;
                    case ")":
                        closeParenth = true;
                        return true;
                    case "$rank":
                        val = Value.Rank;
                        return true;
                    case "$rand":
                        val = Value.Rand;
                        return true;
                    default:
                        if (token.StartsWith("$"))
                        {
                            val = Value.Param(int.Parse(token.Substring(1)));
                        }
                        else
                        {
                            val = new Value(float.Parse(token, new CultureInfo("en-US")));
                        }
                        return true;
                }
            }
            return false;
        }
    }
}
